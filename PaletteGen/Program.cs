using System;

namespace PaletteGen
{
	class Program
	{
		static void Main(string[] args)
		{
			var folder = "G:/Dropbox/Programming/SylvanSneakerAssets/Sylvan-Sneaker-Assets/Reference";

			var processor = new Processor("g:/projects/images");

			processor.MakePalettesForFolder(folder, 64);

			Console.WriteLine("Press the any key");
			Console.ReadLine();
		}
	}
}
