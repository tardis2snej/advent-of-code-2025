using System.Text.RegularExpressions;

namespace aoc_2025;

public class Day3
{
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] batteries = Regex.Split(text, "\r\n|\r|\n");
		
		long sum = 0;

		for (long i = 0; i < batteries.Length; i++)
		{
			int[] numbers = ParseLine(batteries[i]);
			sum += FindBiggestCombo(numbers);
		}
		
		Console.WriteLine($"Sum is {sum}");
	}

	private int[] ParseLine(string line)
	{
		int[] numbers = new int[line.Length];

		for (int i = 0; i < line.Length; i++)
		{
			numbers[i] = int.Parse(line[i].ToString());	
		}

		return numbers;
	}

	private int FindBiggestCombo(int[] numbers)
	{
		int biggestFirstPtr = 0;

		for (int i = 0; i < numbers.Length - 1; i++)
		{
			if (numbers[i] > numbers[biggestFirstPtr])
			{
				biggestFirstPtr = i;
			}
		}

		int biggestSecondPtr = biggestFirstPtr + 1;

		for (int i = biggestSecondPtr; i < numbers.Length; i++)
		{
			if (numbers[i] > numbers[biggestSecondPtr])
			{
				biggestSecondPtr = i;
			}
		}
		
		Console.WriteLine($"{numbers[biggestFirstPtr] * 10 + numbers[biggestSecondPtr]}");
		return numbers[biggestFirstPtr] * 10 + numbers[biggestSecondPtr];
	}
}