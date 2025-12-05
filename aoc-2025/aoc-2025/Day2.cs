namespace aoc_2025;

public class Day2
{
	private struct Range
	{
		public long Start;
		public long End;
	}
	
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] rangesTexts = text.Split(",");
		
		long sum = 0;

		for (long i = 0; i < rangesTexts.Length; i++)
		{
			Range range = ParseRange(rangesTexts[i]);
			List<long> sequences = FindSequences_Part2(range);

			foreach (long sequence in sequences)
			{
				sum += sequence;
			}
		}
		
		Console.WriteLine($"Sum is {sum}");
	}

	private Range ParseRange(string input)
	{
		string[] rangeEnds = input.Split("-");
		Range range = new Range();
		try
		{
			range.Start = long.Parse(rangeEnds[0]);
			range.End = long.Parse(rangeEnds[1]);
		}
		catch
		{
			Console.WriteLine("Error - failed to parse input");
		}
		return range;
	}

	private List<long> FindSequences_Part1(Range range)
	{
		Console.WriteLine($"Checking {range.Start}-{range.End}");
		List<long> sequences = new List<long>();
		
		long pointer = range.Start;

		while (pointer <= range.End)
		{
			int digits = GetDigitCount(pointer);
			if (digits % 2 != 0)
			{
				digits++;
				pointer = GetSmallest(digits);

				if (pointer > range.End)
				{
					break;
				}
			}
			
			string half = pointer.ToString().Substring(0, digits / 2);
			long possibleSeq = long.Parse(half + half);

			if (possibleSeq >= range.Start && possibleSeq <= range.End)
			{
				sequences.Add(possibleSeq);
			}

			string halfPtr = (long.Parse(half) + 1).ToString();
			pointer = long.Parse(halfPtr + halfPtr);
		}

		foreach (var sequence in sequences)
		{
			Console.WriteLine(sequence);
		}
		return sequences;
	}

	private List<long> FindSequences_Part2(Range range)
	{
		Console.WriteLine($"Checking {range.Start}-{range.End}");
		List<long> sequences = new List<long>();
		
		int digitCount = GetDigitCount(range.Start);

		if (digitCount == 1)
		{
			digitCount++;
		}
		
		int endDigitCount = GetDigitCount(range.End);

		while (digitCount <= endDigitCount)
		{
			List<int> denominators = GetDenominators(digitCount);

			foreach (int denominator in denominators)
			{
				long pointer = Math.Max(range.Start, GetSmallest(digitCount));
				
				while (pointer <= range.End)
				{
					string seq = pointer.ToString().Substring(0, denominator);
					long possibleNum =  long.Parse(string.Concat(Enumerable.Repeat(seq, digitCount / denominator)));

					if (possibleNum >= range.Start && possibleNum <= range.End && !sequences.Contains(possibleNum))
					{
						sequences.Add(possibleNum);
					}
					
					string nextSeq = (long.Parse(seq) + 1).ToString();

					if (nextSeq.Length == seq.Length)
					{
						pointer = long.Parse(nextSeq + new string('0', digitCount - nextSeq.Length));
					}
					else
					{
						break;
					}
				}
			}

			digitCount++;
		}

		foreach (var sequence in sequences)
		{
			Console.WriteLine(sequence);
		}
		
		return sequences;
	}

	private long GetSmallest(int digitCount) => (long) Math.Pow(10, digitCount - 1);

	private int GetDigitCount(long value) => value.ToString().Length;

	private List<int> GetDenominators(int value)
	{
		List<int> denominators = [1];

		for (int i = 2; i < value; i++)
		{
			if (value % i == 0)
			{
				denominators.Add(i);
			}
		}

		return denominators;
	}
}