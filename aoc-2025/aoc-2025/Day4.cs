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

	private int GetAvailableRolls(bool[][] rolls)
	{
		int count = 0;
		int lines =  rolls.Length;

		for (int i = 0; i < lines; i++)
		{
			int columns = rolls[i].Length;
			for (int j = 0; j < columns; j++)
			{
				if (!rolls[i][j])
					continue;
				
				int rollsAround = 0;

				// upper row
				if (i > 0) 
				{
					rollsAround += rolls[i - 1][j] ? 1 : 0;

					if (j > 0)
					{
						rollsAround += rolls[i - 1][j - 1] ? 1 : 0;
					}

					if (j < columns - 1)
					{
						rollsAround += rolls[i - 1][j + 1] ? 1 : 0;
					}
				}

				// current row
				if (j > 0) 
				{
					rollsAround += rolls[i][j - 1] ? 1 : 0;
				}
				if (j < columns - 1)
				{
					rollsAround += rolls[i][j + 1] ? 1 : 0;
				}
				
				// lower row
				if (i < lines - 1)
				{
					rollsAround += rolls[i + 1][j] ? 1 : 0;
					
					if (j > 0)
					{
						rollsAround += rolls[i + 1][j - 1] ? 1 : 0;
					}

					if (j < columns - 1)
					{
						rollsAround += rolls[i + 1][j + 1] ? 1 : 0;
					}
				}

				if (rollsAround < 4)
				{
					count++;
				}
			}
		}

		return count;
	}
}