namespace aoc_2025;

public class Day5
{
	class Range
	{
		public long Start;
		public long End;
	}
	
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] rangesAndIds = text.Split([Environment.NewLine + Environment.NewLine],
		                             StringSplitOptions.RemoveEmptyEntries);
		List<Range> ranges = GetRanges(rangesAndIds[0].Split(Environment.NewLine));

		foreach (Range range in ranges)
		{
			Console.WriteLine($"{range.Start} - {range.End}");
		}
		
		int freshProductsCounter = 0;
		if (rangesAndIds.Length > 1)
		{
			long[] ids = GetIDs(rangesAndIds[1].Split(Environment.NewLine));
			
			foreach (long id in ids)
			{
				if (ranges.Find(range => IsWithin(id, range)) != null)
				{
					freshProductsCounter++;
				}
			}
		}

		long freshIdsCounter = 0;
		foreach (Range range in ranges)
		{
			freshIdsCounter += range.End - range.Start + 1;
		}
		
		Console.WriteLine($"Fresh products: {freshProductsCounter}");
		Console.WriteLine($"Fresh ids: {freshIdsCounter}");
	}

	private List<Range> GetRanges(string[] rangesText)
	{
		List<Range> ranges = new();
		foreach (string text in rangesText)
		{
			string[] endsText = text.Split("-");
			Range newRange = new Range { Start = long.Parse(endsText[0]), End = long.Parse(endsText[1]) };
			
			TryInsertRange(ranges, newRange);
		}

		return ranges;
	}

	private void TryInsertRange(List<Range> ranges, Range newRange)
	{
		if (ranges.Contains(newRange))
		{
			return; // duplicate
		}
		
		if (ranges.FindIndex(range => IsWithin(newRange.Start, range) && IsWithin(newRange.End, range)) != -1)
		{
			return; // range fully within bigger range
		}

		int includesRange = ranges.FindIndex(range => IsWithin(range.Start, newRange) && IsWithin(range.End, newRange));

		if (includesRange != -1)
		{
			ranges.RemoveAt(includesRange);
		}
		
		int startWithinExiting = ranges.FindIndex(range => IsWithin(newRange.Start, range));
		int endWithinExiting = ranges.FindIndex(range => IsWithin(newRange.End, range));

		// merge two other ranges together
		if (startWithinExiting != -1 && endWithinExiting != -1)
		{
			Range mergedRange = new Range { Start = ranges[startWithinExiting].Start, End = ranges[endWithinExiting].End };
			ranges[startWithinExiting] = mergedRange;
			ranges.RemoveAt(endWithinExiting);
			return;
		}
		
		// extends existing from End
		if (startWithinExiting != -1)
		{
			if (ranges[startWithinExiting].End < newRange.End)
			{
				ranges[startWithinExiting].End = newRange.End;
			}
			return;
		}
		
		// extends existing from Start
		if (endWithinExiting != -1)
		{
			if (ranges[endWithinExiting].Start > newRange.Start)
			{
				ranges[endWithinExiting].Start = newRange.Start;
			}
			return;
		}
			
		// keep it ordered
		int index = ranges.FindIndex(range => newRange.Start < range.Start);

		if (index != -1)
		{
			ranges.Insert(index, newRange);
			return;
		}
		
		ranges.Add(newRange);
	}

	private long[] GetIDs(string[] idsText)
	{
		long[]  ids = new long[idsText.Length];
		for (int i = 0; i < idsText.Length; i++)
		{
			ids[i] = long.Parse(idsText[i]);
		}

		return ids;
	}

	private bool IsWithin(long number, Range range) => number >= range.Start && number <= range.End;
}