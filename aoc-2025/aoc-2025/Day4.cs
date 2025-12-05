using System.Text.RegularExpressions;

namespace aoc_2025;

public class Day4
{
	public void Run(string input)
	{
		string text = File.ReadAllText(input);

		int[][] map = ParseText(text);

		int removedCount = 0;
		List<(int, int)> rollsToRemove = GetAvailableRolls(map);
			
		while (rollsToRemove.Count > 0)
		{
			removedCount += rollsToRemove.Count;
			RemoveAvailableRolls(ref map, rollsToRemove);
			rollsToRemove = GetAvailableRolls(map);
		}
		
		Console.WriteLine($"Count: {removedCount}");
	}

	private int[][] ParseText(string text)
	{
		string[] lines = Regex.Split(text, "\r\n|\r|\n");
		int[][] rolls =  new int[lines.Length][];
		
		for (int i = 0; i < lines.Length; i++)
		{
			rolls[i] = new int[lines[i].Length];
			for (int j = 0; j < lines[i].Length; j++)
			{
				rolls[i][j] = lines[i][j] == '@' ? 1 : 0;
			}
		}

		return rolls;
	}

	private List<(int,int)> GetAvailableRolls(int[][] map)
	{
		List<(int i, int j)> indexes = new();
		int lines =  map.Length;

		for (int i = 0; i < lines; i++)
		{
			for (int j = 0; j < map[i].Length; j++)
			{
				if (map[i][j] == 0)
					continue;

				int rollsAround = CountRollsInArea(ref map, i, j);

				if (rollsAround < 5)
				{
					indexes.Add((i,j));
				}
			}
		}

		return indexes;
	}

	private int CountRollsInArea(ref int[][] rolls, int i, int j)
	{
		int rollsAround = 0; 
		
		if (i > 0)
		{
			rollsAround += CountRollsOnLine(rolls, i - 1, j);
		}

		rollsAround += CountRollsOnLine(rolls, i, j);
				
		if (i < rolls.Length - 1)
		{
			rollsAround += CountRollsOnLine(rolls, i + 1, j);
		}
		
		rolls[i][j] = rollsAround;

		return rollsAround;
	}

	private int CountRollsOnLine(int[][] rolls, int i, int j)
	{
		int counter = 0;
		
		counter += rolls[i][j] != 0 ? 1 : 0;
		
		if (j > 0) 
		{
			counter += rolls[i][j - 1] != 0 ? 1 : 0;
		}
		if (j < rolls[i].Length - 1)
		{
			counter += rolls[i][j + 1] != 0 ? 1 : 0;
		}

		return counter;
	}

	private void RemoveAvailableRolls(ref int[][] map, List<(int,int)> indexes)
	{
		foreach ((int, int) index in indexes)
		{
			map[index.Item1][index.Item2] = 0;
		}
	}
}