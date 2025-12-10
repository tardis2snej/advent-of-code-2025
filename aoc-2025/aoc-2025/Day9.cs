using System.Drawing;
using System.Numerics;

namespace aoc_2025;

public class Day9
{
	private class Rectangle((Vector2, Vector2) pair)
	{
		public readonly (Vector2, Vector2) Points = pair;
		public readonly long Area = (long)(Math.Abs(pair.Item2.X - pair.Item1.X) + 1) * (long)(Math.Abs(pair.Item2.Y - pair.Item1.Y) + 1);

		public Vector2[] GetCornerPoints() =>
		[
			Points.Item1, 
			new (Points.Item2.X, Points.Item1.Y), 
			Points.Item2,
			new (Points.Item1.X, Points.Item2.Y)
		];
	}
	
	private class Line
	{
		public readonly (Vector2, Vector2) Points;
		public bool IsVertical;
		
		public Line((Vector2, Vector2) coordinates)
		{
			if (coordinates.Item1.X == coordinates.Item2.X) // vertical line
			{
				IsVertical = true;
				if (coordinates.Item1.Y > coordinates.Item2.Y)
				{
					coordinates = (coordinates.Item2, coordinates.Item1);
				}
			}
			else if (coordinates.Item1.Y == coordinates.Item2.Y) // horizontal line
			{
				IsVertical = false;
				if (coordinates.Item1.X > coordinates.Item2.X)
				{
					coordinates = (coordinates.Item2, coordinates.Item1);
				}
			}

			Points = coordinates;
		}

		public Vector2[] GetPointsArray()
		{
			int count = (IsVertical ? (int) (Points.Item2.Y - Points.Item1.Y) : (int) (Points.Item2.X - Points.Item1.X)) + 1;
			Vector2[] points = new Vector2[count];

			for (int i = 0; i < count; i++)
			{
				if (IsVertical)
				{
					points[i] = Points.Item1 with { Y = Points.Item1.Y + i };
				}
				else
				{
					points[i] = Points.Item1 with { X = Points.Item1.X + i };
				}
			}

			return points;
		}

		public bool HasPoint(Vector2 point)
		{
			if (IsVertical)
			{
				return point.X == Points.Item1.X && point.Y >= Points.Item1.Y && point.Y <= Points.Item2.Y;
			}
			else
			{
				return point.Y == Points.Item1.Y && point.X >= Points.Item1.X && point.X <= Points.Item2.X;
			}
		}
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		Vector2[] coordinates = ParseInput(lines);
		Rectangle[] pairs = CreatePairs(coordinates);
		
		Console.WriteLine($"The biggest rectangle: {pairs[0].Area}");
		
		(Line[] vertical, Line[] horizontal) polygon = ParseLines(coordinates);
		Line[] allLines = polygon.vertical.Concat(polygon.horizontal).ToArray();
		
		Rectangle rectangle = pairs[0];
		
		for (int i = 0; i < pairs.Length; i++)
		{
			Console.WriteLine($"{i}/{pairs.Length}");
			if (IsRectangleInArea(pairs[i], allLines))
			{
				rectangle = pairs[i];
				break;
			}	
		}
		Console.WriteLine($"The biggest rectangle within the area: {rectangle.Area}");
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

	private Rectangle[] CreatePairs(Vector2[] coordinates)
	{
		List<Rectangle> pairs = new();
		
		for (int i = 0; i < coordinates.Length; i++)
		{
			for (int j = i + 1; j < coordinates.Length; j++)
			{
				Rectangle pair = new((coordinates[i], coordinates[j]));
				pairs.Add(pair);
			}
		}

		return pairs.OrderBy(pair => pair.Area).Reverse().ToArray();
	}
	
	private (Line[] vertical, Line[] horizontal) ParseLines(Vector2[] points)
	{
		List<Line> verticalLines = new List<Line>();
		List<Line> horizontalLines = new List<Line>();

		for (int i = 1; i <= points.Length; i++)
		{
			Line line;
			
			if (i == points.Length)
			{
				line =  new ((points[i - 1], points[0]));
			}
			else
			{
				line = new ((points[i - 1], points[i]));
			}

			if (line.IsVertical)
			{
				verticalLines.Add(line);
			}
			else
			{
				horizontalLines.Add(line);
			}
		}
		
		verticalLines.Sort((first, second) =>
		{
			int horComp = first.Points.Item1.X.CompareTo(second.Points.Item1.X);

			return horComp == 0 ? first.Points.Item1.Y.CompareTo(second.Points.Item1.Y) : horComp;
		});
		
		horizontalLines.Sort((first, second) =>
		{
			int vertComp = first.Points.Item1.Y.CompareTo(second.Points.Item1.Y);

			return vertComp == 0 ? first.Points.Item1.X.CompareTo(second.Points.Item1.X) : vertComp;
		});

		return (verticalLines.ToArray(), horizontalLines.ToArray());
	}

	private bool IsRectangleInArea(Rectangle rectangle, Line[] allLines)
	{
		Vector2[] rectanglePoints = rectangle.GetCornerPoints();

		foreach (Vector2 point in rectanglePoints)
		{
			if (!IsPointInArea(point, allLines))
			{
				return false;
			}
		}

		Line[] lines =
		[
			new ((rectanglePoints[0], rectanglePoints[1])),
			new ((rectanglePoints[1], rectanglePoints[2])),
			new ((rectanglePoints[2], rectanglePoints[3])),
			new ((rectanglePoints[3], rectanglePoints[0]))
		];

		foreach (Line line in lines)
		{
			Vector2[] linePoints = line.GetPointsArray();
			
			foreach (Vector2 point in linePoints)
			{
				if (!IsPointInArea(point, allLines))
				{
					return false;
				}
			}
		}

		return true;
	}

	private bool IsPointInArea(Vector2 point, Line[] allLines)
	{
		long rayLeft = 0, rayRight = 0, rayUp = 0, rayDown = 0;

		foreach (Line line in allLines)
		{
			if (line.HasPoint(point))
			{
				return true;
			}
		}

		foreach (Line line in allLines)
		{
			if (line.IsVertical)
			{
				if (IsWithinRange(point.Y, line.Points.Item1.Y, line.Points.Item2.Y))
				{
					if (point.Y == line.Points.Item1.Y || point.Y == line.Points.Item2.Y) // hits the corner BUT doesn't lie on it (different X)
					{
						return IsPointInArea(new(point.X, point.Y - 1), allLines);
					}
					
					float horLoc = line.Points.Item1.X;
					if (point.X > horLoc)
					{
						rayLeft++;
					}

					if (point.X < horLoc)
					{
						rayRight++;
					}
					
				}
			}
			else
			{
				if (IsWithinRange(point.X, line.Points.Item1.X, line.Points.Item2.X))
				{
					if (point.X == line.Points.Item1.X || point.X == line.Points.Item2.X) // hits the corner BUT doesn't lie on it (different Y)
					{
						return IsPointInArea(new(point.X - 1, point.Y), allLines);
					}
					
					float verticalLoc = line.Points.Item1.Y;

					if (point.Y > verticalLoc)
					{
						rayUp++;
					}

					if (point.Y < verticalLoc)
					{
						rayDown++;
					}
				}
			}
		}

		return rayDown % 2 != 0 && rayUp % 2 != 0 && rayLeft % 2 != 0 && rayUp % 2 != 0;
	}

	private bool IsWithinRange(float number, float min, float max) => number >= min && number <= max;
}