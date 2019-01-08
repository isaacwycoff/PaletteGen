using CmdArgsParser;
using System;

namespace PaletteGen
{
	static class Program
	{
		static void Main(string[] args)
		{
			var config = new Config();

			var parser = new Parser();
			parser.AddRule<string>("Output Path", s => config.OutputPath = s, "-o", "-output");
			parser.AddRule<string>("Source Dir", s => config.SourceDir = s, "-s", "-source");
			parser.AddRule<int>("Desired Color Count", i => config.DesiredColorCount = i, "-c", "-colors");

			parser.Parse(args);

			Console.ReadLine();

			var processor = new Processor(config.OutputPath);

			processor.MakePalettesForFolder(config.SourceDir, config.DesiredColorCount);

			Console.WriteLine("Press the any key");
			Console.ReadLine();
		}
	}
}
