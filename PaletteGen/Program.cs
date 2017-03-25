using System;

namespace PaletteGen
{
	class Program
	{
		static void Main(string[] args)
		{
			var folder = "G:/projects/PaletteSources";

			var processor = new Processor("g:/projects/PaletteSources/Palettes");

			processor.MakePalettesForFolder(folder, 64);

			Console.WriteLine("Press the any key");
			Console.ReadLine();
		}
	}
}
