using Caliburn.Micro;
using OngekiFumenEditor.Kernel.ArgProcesser.Attributes;

namespace OngekiFumenEditor.Modules.OptionGeneratorTools.Models
{
	public class AcbGenerateOption : PropertyChangedBase
	{
		private int musicId = -1;
		[OptionBindingAttrbute<int>("musicId", "", -1, Require = true)]
		public int MusicId
		{
			get => musicId;
			set => Set(ref musicId, value);
		}

		private string inputAudioFilePath;
		[OptionBindingAttrbute<string>("inputFile", "", "", Require = true)]
		public string InputAudioFilePath
		{
			get => inputAudioFilePath;
			set => Set(ref inputAudioFilePath, value);
		}

		private string outputFolderPath;
		[OptionBindingAttrbute<string>("outputFolder", "", "", Require = true)]
		public string OutputFolderPath
		{
			get => outputFolderPath;
			set => Set(ref outputFolderPath, value);
		}

		private int previewBeginTime = 60000;
		[OptionBindingAttrbute<int>("previewBegin", "", 60000)]
		public int PreviewBeginTime
		{
			get => previewBeginTime;
			set
			{
				Set(ref previewBeginTime, value);
			}
		}

		private int previewEndTime = 80000;
		[OptionBindingAttrbute<int>("previewEnd", "", 80000)]
		public int PreviewEndTime
		{
			get => previewEndTime;
			set
			{
				Set(ref previewEndTime, value);
			}
		}
	}
}
