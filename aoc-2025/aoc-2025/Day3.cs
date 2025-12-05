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
			sum += FindBiggestCombo(numbers, 12);
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

	private long FindBiggestCombo(int[] numbers, int numLen = 2)
	{
		int[] pointers = new int[numLen];
		long number = 0;
		for (int i = 0; i < numLen; i++)
		{
			int remainingNumLen = numLen - i - 1;
			int start = i == 0 ? 0 : pointers[i - 1] + 1;
			int end = numbers.Length - remainingNumLen;
			pointers[i] = FindBiggestPtr(numbers, start, end);
			number += numbers[pointers[i]] * (long)Math.Pow(10, remainingNumLen);
		}
		
		Console.WriteLine($"{number}");
		return number;
	}

	private int FindBiggestPtr(int[] numbers, int start, int end)
	{
		int biggestPtr = start;
		for (int i = biggestPtr; i < end; i++)
		{
			if (numbers[i] > numbers[biggestPtr])
			{
				biggestPtr = i;
			}
		}

		return biggestPtr;
	}
}