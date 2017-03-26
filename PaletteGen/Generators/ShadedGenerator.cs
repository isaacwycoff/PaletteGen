using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PaletteGen.Generators
{
	internal class ShadedGenerator : IPaletteGenerator
	{
		private class NormalizedColor
		{

		}

		private Color NormalizeColor(Color color)
		{
			byte red = 0x0;
			byte green = 0x0;
			byte blue = 0x0;

			if(color.R == 0x0 && color.G == 0x0 && color.B == 0)
			{

			}
			else if(color.R >= color.G && color.R >= color.B)
			{
				red = 0xff;
				green = (byte)((255.0 * color.G) / color.R);
				blue = (byte)((255.0 * color.B) / color.R);
			}
			else if(color.G >= color.R && color.G >= color.B)
			{
				red = (byte)((255.0 * color.R) / color.G);
				green = 0xff;
				blue = (byte)((255.0 * color.B) / color.G);
			}
			else
			{
				red = (byte)((255.0 * color.R) / color.B);
				green = (byte)((255.0 * color.G) / color.B);
				blue = 0xff;
			}

			return Color.FromArgb(0xff, red, green, blue);
		}

		private class ColorRatio
		{
			public readonly float RedGreen;
			public readonly float RedBlue;

			public ColorRatio(Color color)
			{
				byte green = color.G > 0 ? color.G : (byte)1;
				byte blue = color.B > 0 ? color.B : (byte)1;

				RedGreen = color.R / green;
				RedBlue = color.G / blue;
			}

			public bool IsSimilarTo(ColorRatio other, float threshold)
			{
				return Math.Abs(other.RedGreen - RedGreen) + Math.Abs(other.RedBlue - RedBlue) < threshold;
			}

			public Color ToColor()
			{

				float red = MAX_COLOR;
				float green = red / RedGreen;
				float blue = red / RedBlue;

				if(green > MAX_COLOR)
				{
					var divisor = green / MAX_COLOR;

					red /= divisor;
					green /= divisor;
					blue /= divisor;
				}

				if(blue > MAX_COLOR)
				{
					var divisor = blue / MAX_COLOR;

					red /= divisor;
					green /= divisor;
					blue /= divisor;
				}

				//red = red > 255 ? 255 : red;
				//green = green > 255 ? 255 : green;
				//blue = blue > 255 ? blue : blue;

				var color = Color.FromArgb(255,
					LimitColor(red), LimitColor(green), LimitColor(blue));

				return color;
			}
		}

		const int MAX_COLOR = 255;

		private static int LimitColor(float color)
		{
			if ((int)color > MAX_COLOR) return MAX_COLOR;
			if ((int)color < 0) return 0;
			return (int)color;
		}

		private const int SHADES = 4;

		public List<Color> GeneratePalette(Dictionary<Color, int> colors, int desiredCount)
		{
			int threshold = 20;

			desiredCount = (desiredCount - 4) / SHADES;

			var colorList = colors.ToList().OrderByDescending(kv => kv.Value).Select(kv => kv.Key);

			colorList = colorList.Select(color => NormalizeColor(color));

			var archetypes = GetCandidates(colorList, threshold, desiredCount);

			return ExtrapolatePalette(archetypes);
		}

		private List<Color> RequiredColors()
		{
			return new List<Color>()
			{
				Color.FromArgb(0, 0, 0, 0),
				Color.FromArgb(255, 0, 0, 0),
				Color.FromArgb(255, 255, 255, 255),
				Color.FromArgb(255, 127, 127, 127)
			};
		}

		private Color ShadeColor(Color color, float ratio)
		{
			var red = LimitColor(color.R * ratio);
			var green = LimitColor(color.G * ratio);
			var blue = LimitColor(color.B * ratio);

			return Color.FromArgb(255, red, green, blue);
			
		}

		private List<Color> ExtrapolatePalette(IEnumerable<Color> archetypes)
		{
			var colors = RequiredColors();

			foreach(var color in archetypes)
			{
				for(float i = 1; i <= SHADES; i++)
				{
					var shade = ShadeColor(color, i / SHADES);
					Console.WriteLine($"Shade {i}/{SHADES} = {shade.R} {shade.G} {shade.B}");
					colors.Add(shade);
				}
			}

			return colors;
		}

		//private void AddColorToLookup(Color color, List<Color> candidates, float threshold)
		//{
		//	var trialRatio = new ColorRatio(color);
		//
		//	//var normalized = 
		//
		//	foreach(var candidate in candidates)
		//	{
		//		if (trialRatio.IsSimilarTo(existingRatio, threshold))
		//		{
		//			candidates[existingRatio].Add(color);
		//			return;
		//		}
		//	}
		//
		//	candidates[trialRatio] = new List<Color>() { color };
		//}

		private const int THRESHOLD_INTERVAL = 20;
		private IEnumerable<Color> GetCandidates(IEnumerable<Color> colorList, int threshold, int desiredCount)
		{
			var candidates = new List<Color>();

			//Dictionary<ColorRatio, List<Color>> colorLookup = new Dictionary<ColorRatio, List<Color>>();

			foreach (var color in colorList)
			{
				if (IsDistinctish(color, candidates, threshold))
				{
					candidates.Add(color);
				}
			}

			Console.WriteLine($"Distinct candidates: {candidates.Count}");

			//var selectedColors =
			//	colorLookup.Keys.Select(ratio => ratio.ToColor());

			if (candidates.Count() <= desiredCount)
			{
				var deficit = desiredCount - candidates.Count();

				var excluded = colorList.Where(color => !candidates.Contains(color));

				candidates.AddRange(colorList.Take(deficit));

				return candidates;
			}

			//if (colorLookup.Count <= desiredCount)
			//{
			//
			//	return selectedColors;
			//}

			return GetCandidates(candidates, threshold + THRESHOLD_INTERVAL, desiredCount);
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

		private int ColorDistance(Color color1, Color color2)
		{
			return
			Math.Abs(color1.R - color2.R) +
			Math.Abs(color1.G - color2.G) +
			Math.Abs(color1.B - color2.B);
		}
	}
}
