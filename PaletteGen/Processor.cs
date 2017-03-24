using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace PaletteGen
{
	class Processor
	{
		//private Bitmap m_Bitmap;

		private string m_OutputPath;

		private string[] m_ImageExtensions = new string[]
		{
			".jpg",
			".jpeg"
		};

		public Processor(string outputPath)
		{
			m_OutputPath = outputPath;
		}

		public void MakePalettesForFolder(string folder, byte desiredCount)
		{
			var files = Directory.GetFiles(folder).Where(path => FileHasImageExt(path));

			foreach(var file in files)
			{
				Console.WriteLine(file);

				MakePalette(file, desiredCount);
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

					if (i > 100)
					{
						Console.Write(".");
						i = 0;
					}
				}
			}

			Console.Write("\n");

			return colors;
		}

		private int ColorDistance(Color color1, Color color2)
		{
			return 
			Math.Abs(color1.R - color2.R) +
			Math.Abs(color1.G - color2.G) +
			Math.Abs(color1.B - color2.B);
		}

		private const byte MAX_COLORS = 0xff;
		
		public void MakePalette(string path, byte desiredCount)
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

			var minDistance = 20;

			var colorList = colors.ToList().OrderByDescending(kv => kv.Value).Select(kv => kv.Key);

			var candidates = GetCandidates(colorList, minDistance, desiredCount);

			Bitmap bmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed);

			var palette = bmp.Palette;

			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

			byte[] bytes = new byte[data.Height * data.Stride];

			for(byte i = 0; i < MAX_COLORS; ++i)
			{
				bytes[i] = i;

				if (i < candidates.Count())
					palette.Entries[i] = candidates[i];
				else
					palette.Entries[i] = Color.FromArgb(0, 0, 0);
			}

			Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
			bmp.UnlockBits(data);

			bmp.Palette = palette;

			bmp.Save($"{m_OutputPath}/{Path.GetFileNameWithoutExtension(path)} - {desiredCount} colors.gif", ImageFormat.Gif);
		}

		//private string GetFilenameWithoutExt(string path)
		//{
		//	var filename = Path.GetFileName(path);
		//
		//	return filename.Split('.')[0];
		//}

		private const int DISTANCE_INTERVAL = 20;

		public List<Color> GetCandidates(IEnumerable<Color> colorList, int minDistance, int desiredCount)
		{
			var candidates = new List<Color>();

			foreach (var color in colorList)
			{
				if (IsDistinctish(color, candidates, minDistance))
				{
					candidates.Add(color);
				}
			}

			Console.WriteLine($"Distinct candidates: {candidates.Count}");


			if (candidates.Count() < desiredCount)
			{
				var deficit = desiredCount - candidates.Count();

				var excluded = colorList.Where(color => !candidates.Contains(color));

				candidates.AddRange(colorList.Take(deficit));

				return candidates;
			}

			return GetCandidates(candidates, minDistance + DISTANCE_INTERVAL, desiredCount);
		}

		private bool IsDistinctish(Color color, List<Color> others, int minDistance)
		{
			foreach (var other in others)
			{
				var distance = ColorDistance(color, other);
				if (distance < minDistance) return false;
			}
			return true;
		}
	}
}
