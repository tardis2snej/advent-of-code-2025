using System.Numerics;

namespace aoc_2025;

public class Day9
{
	private class CoordinatePair((Vector2, Vector2) pair)
	{
		public readonly (Vector2, Vector2) Points = pair;
		public readonly long Area = (long)(Math.Abs(pair.Item2.X - pair.Item1.X) + 1) * (long)(Math.Abs(pair.Item2.Y - pair.Item1.Y) + 1);
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		Vector2[] coordinates = ParseInput(lines);
		CoordinatePair[] pairs = CreatePairs(coordinates);
		
		Console.WriteLine($"{pairs[0].Area}");
	}

	private Vector2[] ParseInput(string[] lines)
	{
		Vector2[] points = new Vector2[lines.Length];
		for (int i = 0; i < lines.Length; i++)
		{
			string[] numbers = lines[i].Split(',');
			points[i] = new Vector2(int.Parse(numbers[0]), int.Parse(numbers[1]));
		}

		return points;
	}

	private CoordinatePair[] CreatePairs(Vector2[] coordinates)
	{
		List<CoordinatePair> pairs = new();
		
		for (int i = 0; i < coordinates.Length; i++)
		{
			for (int j = i + 1; j < coordinates.Length; j++)
			{
				CoordinatePair pair = new((coordinates[i], coordinates[j]));
				pairs.Add(pair);
			}
		}

		return pairs.OrderBy(pair => pair.Area).Reverse().ToArray();
	}
}