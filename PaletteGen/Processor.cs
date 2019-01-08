using PaletteGen.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PaletteGen
{
	class Processor
	{
		private readonly string m_OutputPath;
		private readonly IPaletteGenerator m_Generator;

		private readonly string[] m_ImageExtensions = new string[]
		{
			//".png",
			".jpg",
			".jpeg"
		};

		public Processor(string outputPath)
		{
			m_OutputPath = outputPath;

			m_Generator = new PaletteGenerator();
		}

		public void MakePalettesForFolder(string folder, int desiredCount)
		{
			var files = Directory.GetFiles(folder).Where(path => FileHasImageExt(path));

			foreach(var file in files)
			{
				Console.WriteLine(file);

				GenerateAndSavePalette(file, desiredCount);
			}
		}

		private bool FileHasImageExt(string path)
		{
			return m_ImageExtensions.Contains(Path.GetExtension(path).ToLower());
		}

		private Bitmap LoadBitmap(string filename)
		{
			using (var original = Image.FromFile(filename))
			{
				Console.WriteLine($"Loaded image {filename}");

				return new Bitmap(original, new Size(original.Width, original.Height));
			}
		}

		private Dictionary<Color, int> GetColors(Bitmap bitmap)
		{
			var colors = new Dictionary<Color, int>();

			int i = 0;

			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					++i;

					var pixel = bitmap.GetPixel(x, y);

					if (colors.ContainsKey(pixel)) colors[pixel] += 1;
					else colors[pixel] = 1;

					if (i > 1000)
					{
						Console.Write(".");
						i = 0;
					}
				}
			}

			Console.Write("\n");

			return colors;
		}

		private const byte MAX_COLORS = 0xff;
		
		public void GenerateAndSavePalette(string path, int desiredCount)
		{
			Dictionary<Color, int> colors;

			try
			{
				using (var bitmap = LoadBitmap(path))
				{
					colors = GetColors(bitmap);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Couldn't load bitmap {path}: {ex.Message}");
				return;
			}

			Console.WriteLine($"Distinct colors: {colors.Count}");

			if (desiredCount > MAX_COLORS) desiredCount = MAX_COLORS;

			var candidates = m_Generator.GeneratePalette(colors, desiredCount);

			using (Bitmap bmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
			{
				var palette = bmp.Palette;

				var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

				byte[] bytes = new byte[data.Height * data.Stride];

				for(byte i = 0; i < MAX_COLORS; ++i)
				{
					bytes[i] = i;

					if (i < candidates.Count)
						palette.Entries[i] = candidates[i];
					else
						palette.Entries[i] = Color.FromArgb(0, 0, 0);
				}

				Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
				bmp.UnlockBits(data);

				bmp.Palette = palette;

				var filename = Path.GetFileNameWithoutExtension(path);

				bmp.Save($"{m_OutputPath}/{filename} - {desiredCount} colors.gif", ImageFormat.Gif);
				SaveAsepritePalette($"{m_OutputPath}/{filename}.gpl", candidates, filename);
			}
		}

		private void SaveAsepritePalette(string path, IEnumerable<Color> colors, string name)
		{
			var builder = new StringBuilder();

			builder.Append($"GIMP Palette\n");
			builder.Append($"# {name} \n");
			builder.Append($"# by PaletteGen\n");
			builder.Append($"#\n");
			builder.Append($"#\n");

			foreach(var color in colors)
			{
				var red = color.R.ToString().PadLeft(3);
				var green = color.G.ToString().PadLeft(3);
				var blue = color.B.ToString().PadLeft(3);

				builder.Append($"{red} {green} {blue} Untitled\n");
			}

			File.WriteAllText(path, builder.ToString());
		}
	}
}
