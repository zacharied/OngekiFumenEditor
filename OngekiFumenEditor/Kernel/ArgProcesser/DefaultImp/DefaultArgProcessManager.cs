using Caliburn.Micro;
using NWaves.Audio.Interfaces;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Kernel.ArgProcesser.Attributes;
using OngekiFumenEditor.Kernel.Audio;
using OngekiFumenEditor.Modules.AudioAdjustWindow;
using OngekiFumenEditor.Modules.FumenVisualEditor;
using OngekiFumenEditor.Modules.PreviewSvgGenerator;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Properties;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.Logs.DefaultImpls;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using OngekiFumenEditor.Modules.FumenConverter;
using OngekiFumenEditor.Modules.OptionGeneratorTools.Base;
using OngekiFumenEditor.Modules.OptionGeneratorTools.Kernel;
using OngekiFumenEditor.Modules.OptionGeneratorTools.Models;
using Expression = System.Linq.Expressions.Expression;

namespace OngekiFumenEditor.Kernel.ArgProcesser.DefaultImp
{
    [Export(typeof(IProgramArgProcessManager))]
    internal class DefaultArgProcessManager : IProgramArgProcessManager
    {
        void Exit(int code = 0) => ErrorExit(string.Empty, true, code);

        void ErrorExit(string message, bool noDialog, int code = 0)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (noDialog)
                    Log.LogError(message);
                else
                    MessageBox.Show(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            Application.Current.Shutdown(code);
        }

        public async Task ProcessArgs(string[] args)
        {
            if (args.Length == 0)
                return;

            if (args.Length == 1)
            {
                var filePath = args[0];

                if (File.Exists(filePath) && File.GetAttributes(filePath).HasFlag(FileAttributes.Normal)) 
                {
                    Log.LogInfo($"arg.filePath: {filePath}");

                    _ = Application.Current.Dispatcher.Invoke(async () =>
                    {
                        if (await DocumentOpenHelper.TryOpenAsDocument(filePath))
                            Application.Current?.MainWindow?.Focus();
                    });

                    return;
                }
            }
            
            Log.Instance.AddOutputIfNotExist<ConsoleLogOutput>();

            var rootCommand = new RootCommand("CommandLine for OngekiFumenEditor");
            rootCommand.AddCommand(GenerateVerbCommands<GenerateOption>("svg", Resources.ProgramCommandDescriptionSvg, ProcessSvgCommand));
            rootCommand.AddCommand(GenerateVerbCommands<FumenConvertOption>("convert", Resources.ProgramCommandDescriptionConvert, ProcessConvertCommand));
            rootCommand.AddCommand(GenerateVerbCommands<JacketGenerateOption>("jacket", Resources.ProgramCommandDescriptionConvert, ProcessJacketCommand));
            rootCommand.AddCommand(GenerateVerbCommands<AcbGenerateOption>("acb", Resources.ProgramCommandDescriptionConvert, ProcessAcbCommand));
            await rootCommand.InvokeAsync(args);
            
            Exit();
        }

        IEnumerable<Option> GenerateOptionsByAttributes<T>()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<OptionBindingAttrbuteBase>() is OptionBindingAttrbuteBase attrbuteBase)
                {
                    var funcType = typeof(Func<>).MakeGenericType(attrbuteBase.Type);
                    var valParam = Expression.Constant(attrbuteBase.DefaultValue, attrbuteBase.Type);
                    var lambda = Expression.Lambda(funcType, valParam);
                    var func = lambda.Compile();

                    var optionType = typeof(Option<>).MakeGenericType(attrbuteBase.Type);
                    var optName = $"--{attrbuteBase.Name}";

                    var option = (Option)LambdaActivator.CreateInstance(optionType, optName, func, attrbuteBase.Description);
                    option.IsRequired = attrbuteBase.Require;

                    yield return option;
                }
            }
        }

        T Generate<T>(Command command, ParseResult result) where T : new()
        {
            var obj = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<OptionBindingAttrbuteBase>() is OptionBindingAttrbuteBase attrbuteBase)
                {
                    var name = $"{attrbuteBase.Name}";
                    if (command.Options.FirstOrDefault(x => x.Name == name) is Option opt)
                    {
                        var val = result.GetValueForOption(opt);
                        prop.SetValue(obj, val);
                    }
                }
            }

            return obj;
        }

        private Command GenerateVerbCommands<T>(string verb, string description, Func<T, Task> callbackFunc) where T : new()
        {
            var command = new Command(verb, description);
            foreach (var option in GenerateOptionsByAttributes<T>())
                command.AddOption(option);

            command.SetHandler(async ctx =>
            {
                var opt = Generate<T>(command, ctx.ParseResult);
                await callbackFunc(opt);
            });
            return command;
        }

        private async Task ProcessSvgCommand(GenerateOption opt)
        {
            try
            {
                using var fumenFileStream = File.OpenRead(opt.InputFumenFilePath);
                var fumenDeserializer = IoC.Get<IFumenParserManager>().GetDeserializer(opt.InputFumenFilePath);
                if (fumenDeserializer is null)
                    throw new NotSupportedException($"{Resources.DeserializeFumenFileFail}{opt.InputFumenFilePath}");
                var fumen = await fumenDeserializer.DeserializeAsync(fumenFileStream);

                //calculate duration
                if (File.Exists(opt.AudioFilePath))
                {
                    var audioPlayer = await IoC.Get<IAudioManager>().LoadAudioAsync(opt.AudioFilePath);
                    opt.Duration = audioPlayer.Duration;
                }
                else
                {
                    //只能通过谱面来计算
                    var maxTGrid = fumen.GetAllDisplayableObjects().OfType<ITimelineObject>().Max(x => x.TGrid);
                    maxTGrid += new GridOffset(5, 0);
                    var duration = TGridCalculator.ConvertTGridToAudioTime(maxTGrid, fumen.BpmList);
                    opt.Duration = duration;
                }

                _ = await IoC.Get<IPreviewSvgGenerator>().GenerateSvgAsync(fumen, opt);
                Log.LogInfo(Resources.GenerateSvgSuccess);
            }
            catch (Exception e)
            {
                Log.LogError(Resources.CallGenerateSvgAsyncFail, e);
                Exit(1);
            }

            Exit();
        }
        
        private async Task ProcessConvertCommand(FumenConvertOption opt)
        {
            try {
                var converter = IoC.Get<IFumenConverter>();
                var parserManager = IoC.Get<IFumenParserManager>();

                if (opt.InputFumenFilePath == default) {
                    throw new FileNotFoundException("Missing input file");
                }
                    
                if (parserManager.GetDeserializer(opt.InputFumenFilePath) is not { } deserializable) {
                    throw new FileFormatException("Invalid input file format");
                }

                await using var inputFileStream = File.OpenRead(opt.InputFumenFilePath);
                var input = await deserializable.DeserializeAsync(inputFileStream);

                await converter.ConvertFumenAsync(input, opt);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"{Resources.ConvertFail}: {e}");
                Exit(1);
            }

            Exit();
        }

        private async Task ProcessJacketCommand(JacketGenerateOption arg)
        {
            if (!Path.IsPathRooted(arg.InputImageFilePath)) {
                arg.InputImageFilePath = Path.GetFullPath(arg.InputImageFilePath);
            }
            
            if (!Path.IsPathRooted(arg.OutputAssetbundleFolderPath)) {
                arg.OutputAssetbundleFolderPath = Path.GetFullPath(arg.OutputAssetbundleFolderPath);
            }
            
            GenerateResult result;
            try {
                result = await JacketGenerateWrapper.Generate(arg);
            }
            catch (Exception e) {
                result = new GenerateResult(false, e.Message);
            }
            
            if (!result.IsSuccess) {
                await Console.Error.WriteLineAsync($"Failed to generate jacket: {result.Message}");
                Exit(1);
            }
            
            Exit();
        }

        private async Task ProcessAcbCommand(AcbGenerateOption arg)
        {
            var result = await AcbGeneratorFuckWrapper.Generate(arg);
            if (!result.IsSuccess) {
                await Console.Error.WriteLineAsync($"Failed to generate acb: {result.Message}");
                Exit(1);
            }
            Exit();
        }
    }
}