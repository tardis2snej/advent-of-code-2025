using System.Numerics;

namespace aoc_2025;

public class Day8
{
	private class Circuit
	{
		public static List<Circuit> AllCircuits = new();
		
		private List<Box> boxes = new();
		
		public int Count => boxes.Count;

		public Circuit()
		{
			AllCircuits.Add(this);
		}
		
		public void AddBoxIfUnique(Box box)
		{
			if (boxes.Contains(box))
			{
				return;
			}
			
			box.Circuit = this;
			boxes.Add(box);
		}

		public static void Merge(params Circuit[] circuits)
		{
			Circuit newCircuit = new();

			foreach (Circuit circuit in circuits)
			{
				foreach (Box box in circuit.boxes)
				{
					newCircuit.AddBoxIfUnique(box);
				}
				circuit.Destroy();
			}
		}

		public void Merge(Circuit addCircuit)
		{
			foreach (Box box in addCircuit.boxes)
			{
				AddBoxIfUnique(box);
			}
			
			addCircuit.Destroy();
		}

		private void Destroy()
		{
			AllCircuits.Remove(this);
		}
	}

	private class Box
	{
		public readonly Vector3 coordinates;
		public Circuit Circuit;

		public Box(Vector3 coordinates)
		{
			this.coordinates = coordinates;
			Circuit = new Circuit();
			Circuit.AddBoxIfUnique(this);
		}
	}

	private class BoxPair((Box, Box) pair)
	{
		public readonly (Box, Box) Boxes = pair;
		public readonly float DistanceSqr = Vector3.DistanceSquared(pair.Item1.coordinates, pair.Item2.coordinates);
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		Vector3[] coordinatesList = ParseCoordinates(lines);
		Box[] boxes = CreateBoxes(coordinatesList);

		BoxPair[] allPairs = CreatePairs(boxes);
		
		Circuit[] circuits = CreateCircuits(allPairs, 1000);

		int mul = 1;
		for (int i = 0; i < 3 && i < circuits.Length; i++)
		{
			mul *= circuits[i].Count;
		}
		
		Console.WriteLine(mul);
	}

	private Vector3[] ParseCoordinates(string[] input)
	{
		Vector3[] coordinates = new Vector3[input.Length];

		for (int i = 0; i < input.Length; i++)
		{
			string[] values = input[i].Split(',', StringSplitOptions.RemoveEmptyEntries);

			if (values.Length != 3)
			{
				Console.WriteLine("Input parse error");
			}

			coordinates[i].X = int.Parse(values[0]);
			coordinates[i].Y = int.Parse(values[1]);
			coordinates[i].Z = int.Parse(values[2]);
		}

		return coordinates;
	}

	private Box[] CreateBoxes(Vector3[] coordinates)
	{
		Box[] boxes = new Box[coordinates.Length];
		for (int i = 0; i < coordinates.Length; i++)
		{
			boxes[i] = new Box(coordinates[i]);
		}

		return boxes;
	}

	private BoxPair[] CreatePairs(Box[] boxes)
	{
		List<BoxPair> pairs = new();
		
		for (int i = 0; i < boxes.Length; i++)
		{
			for (int j = i + 1; j < boxes.Length; j++)
			{
				BoxPair pair = new((boxes[i], boxes[j]));
				pairs.Add(pair);
			}
		}

		return pairs.OrderBy(pair => pair.DistanceSqr).ToArray();
	}
	
	private Circuit[] CreateCircuits(BoxPair[] pairs, int pairsLimit = 10)
	{
		for (int i = 0; i < pairsLimit && i < pairs.Length; i++)
		{
			Circuit.Merge(pairs[i].Boxes.Item1.Circuit, pairs[i].Boxes.Item2.Circuit);
		}
		
		return Circuit.AllCircuits.OrderBy(item => item.Count).Reverse().ToArray();
	}
}