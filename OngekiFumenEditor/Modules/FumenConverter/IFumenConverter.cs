using System.Threading.Tasks;
using Gemini.Framework;
using OngekiFumenEditor.Base;

namespace OngekiFumenEditor.Modules.FumenConverter
{
	public interface IFumenConverter
	{
		Task<byte[]> ConvertToOgkrAsync(OngekiFumen fumen, FumenConvertOption option);
	}
}
