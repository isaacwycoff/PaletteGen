using DaggerLevelEditor;

namespace PaletteGen
{
	public class Config : AutoConfig
	{
		public string OutputPath { get; set; }
		public string SourceDir { get; set; }
		public int DesiredColorCount { get; set; }
	}
}
