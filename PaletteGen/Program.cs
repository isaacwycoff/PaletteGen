using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PaletteGen
{

	class Program
	{
		static void Main(string[] args)
		{
			var folder = "G:/Dropbox/Programming/SylvanSneakerAssets/Sylvan-Sneaker-Assets/Reference";

			var processor = new Processor("g:/projects/images");

			processor.MakePalette("g:/projects/images/test1.jpg", 16);

			processor.MakePalettesForFolder(folder, 64);

			Console.ReadLine();
		}
	}
}
