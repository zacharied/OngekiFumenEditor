using OngekiFumenEditor.Kernel.ArgProcesser.Attributes;

namespace OngekiFumenEditor.Modules.FumenConverter;

public class FumenConvertOption
{
    [OptionBindingAttrbute<string>("inputFile", "", default, Require = true)]
    public string InputFumenFilePath { get; init; }
    
    [OptionBindingAttrbute<string>("outputFile", "", default, Require = true)]
    public string OutputFumenFilePath { get; init; }
}