using System.Text.RegularExpressions;

namespace aoc_2025;

public class Day4
{
	public void Run(string input)
	{
		string text = File.ReadAllText(input);

		bool[][] rolls = ParseText(text);
		
		int count = GetAvailableRolls(rolls);

		
		Console.WriteLine($"Count: {count}");
	}

	private bool[][] ParseText(string text)
	{
		string[] lines = Regex.Split(text, "\r\n|\r|\n");
		bool[][] rolls =  new bool[lines.Length][];
		
		for (int i = 0; i < lines.Length; i++)
		{
			rolls[i] = new bool[lines[i].Length];
			for (int j = 0; j < lines[i].Length; j++)
			{
				rolls[i][j] = lines[i][j] == '@';
			}
		}

		return rolls;
	}

	private int GetAvailableRolls(bool[][] map)
	{
		int count = 0;
		int lines =  map.Length;

		for (int i = 0; i < lines; i++)
		{
			for (int j = 0; j < map[i].Length; j++)
			{
				if (!map[i][j])
					continue;

				int rollsAround = CountRollsInArea(map, i, j);

				if (rollsAround < 5)
				{
					count++;
				}
			}
		}

		return count;
	}

	private int CountRollsInArea(bool[][] rolls, int i, int j)
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

		return rollsAround;
	}

	private int CountRollsOnLine(bool[][] rolls, int i, int j)
	{
		int counter = 0;
		
		counter += rolls[i][j] ? 1 : 0;
		
		if (j > 0) 
		{
			counter += rolls[i][j - 1] ? 1 : 0;
		}
		if (j < rolls[i].Length - 1)
		{
			counter += rolls[i][j + 1] ? 1 : 0;
		}

		return counter;
	}
}