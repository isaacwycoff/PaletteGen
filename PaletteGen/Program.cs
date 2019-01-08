using System;

namespace PaletteGen
{
	static class Program
	{
		static void Main(string[] args)
		{
			var config = new Config();

			var processor = new Processor(config.OutputPath);

			processor.MakePalettesForFolder(config.SourceDir, config.DesiredColorCount);

			Console.WriteLine("Press the any key");
			Console.ReadLine();
		}
	}
}
