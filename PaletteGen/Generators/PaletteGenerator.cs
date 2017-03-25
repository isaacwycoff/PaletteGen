using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PaletteGen.Generators
{
	public interface IPaletteGenerator
	{
		List<Color> GeneratePalette(Dictionary<Color, int> colors, int desiredCount);
	}

	internal class PaletteGenerator : IPaletteGenerator
	{
		public List<Color> GeneratePalette(Dictionary<Color, int> colors, int desiredCount)
		{
			var minDistance = 20;
			
			var colorList = colors.ToList().OrderByDescending(kv => kv.Value).Select(kv => kv.Key);
			
			var candidates = GetCandidates(colorList, minDistance, desiredCount);

			return candidates;
		}

		private const int DISTANCE_INTERVAL = 20;

		private List<Color> GetCandidates(IEnumerable<Color> colorList, int minDistance, int desiredCount)
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


			if (candidates.Count() <= desiredCount)
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

		private int ColorDistance(Color color1, Color color2)
		{
			return
			Math.Abs(color1.R - color2.R) +
			Math.Abs(color1.G - color2.G) +
			Math.Abs(color1.B - color2.B);
		}
	}
}
