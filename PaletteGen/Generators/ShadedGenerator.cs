using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PaletteGen.Generators
{
	internal class ShadedGenerator : IPaletteGenerator
	{
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
			float threshold = 5;

			desiredCount = (desiredCount - 4) / SHADES;

			var colorList = colors.ToList().OrderByDescending(kv => kv.Value).Select(kv => kv.Key);

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
				for(int i = 1; i <= SHADES; i++)
				{
					colors.Add(ShadeColor(color, i / SHADES));
				}
			}

			return colors;
		}

		private void AddColorToLookup(Color color, Dictionary<ColorRatio, List<Color>> colorLookup, float threshold)
		{
			var trialRatio = new ColorRatio(color);

			foreach(var keyValue in colorLookup)
			{
				var existingRatio = keyValue.Key;

				if (trialRatio.IsSimilarTo(existingRatio, threshold))
				{
					colorLookup[existingRatio].Add(color);
					return;
				}
			}

			colorLookup[trialRatio] = new List<Color>() { color };
		}

		private const int THRESHOLD_INTERVAL = 20;
		private IEnumerable<Color> GetCandidates(IEnumerable<Color> colorList, float threshold, int desiredCount)
		{
			Dictionary<ColorRatio, List<Color>> colorLookup = new Dictionary<ColorRatio, List<Color>>();

			foreach (var color in colorList)
			{
				AddColorToLookup(color, colorLookup, threshold);
			}

			Console.WriteLine($"Distinct candidates: {colorLookup.Count}");

			var selectedColors =
				colorLookup.Keys.Select(ratio => ratio.ToColor());

			if (colorLookup.Count <= desiredCount)
			{

				return selectedColors;
			}

			return GetCandidates(selectedColors, threshold + THRESHOLD_INTERVAL, desiredCount);
		}
	}
}
