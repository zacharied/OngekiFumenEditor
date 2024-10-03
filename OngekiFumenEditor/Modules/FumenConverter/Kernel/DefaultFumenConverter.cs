using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Properties;

namespace OngekiFumenEditor.Modules.FumenConverter.Kernel
{
    [Export(typeof(IFumenConverter))]
    public class DefaultFumenConverter : IFumenConverter
    {
        public async Task<byte[]> ConvertToOgkrAsync(OngekiFumen fumen, FumenConvertOption option)
        {
            var parserManager = IoC.Get<IFumenParserManager>();
            
            if (fumen is null) {
                throw new FumenConvertException(Resources.NoFumenInput);
            }

            if (parserManager.GetDeserializer(option.InputFumenFilePath) is not IFumenDeserializable deserializable) {
                throw new FumenConvertException(Resources.FumenFileDeserializeNotSupport);
            }

            if (string.IsNullOrWhiteSpace(option.OutputFumenFilePath)) {
                throw new FumenConvertException(Resources.OutputFumenFileNotSelect);
            }

            if (parserManager.GetSerializer(option.OutputFumenFilePath) is not IFumenSerializable serializable) {
                throw new FumenConvertException(Resources.OutputFumenNotSupport);
            }

            try {
                return await serializable.SerializeAsync(fumen);
            }
            catch (Exception e) {
                throw new FumenConvertException($"{Resources.ConvertFail}{e.Message}");
            }
        }
    }

    public class FumenConvertException : Exception
    {
        public FumenConvertException() { }
        public FumenConvertException(string message) : base(message) { }
        public FumenConvertException(string message, Exception inner) : base(message, inner) { }
    }
}