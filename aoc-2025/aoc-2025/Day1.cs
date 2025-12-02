namespace aoc_2025;

public class Day1
{
	public void Run(string input)
	{
		Console.WriteLine("Day 1");
		string text = File.ReadAllText(input);
		Calculate(text.Split('\n'));
	}

	private void Calculate(string[] lines)
	{
		int counterExtended = 0;
		int currentNum = 50;
		
		for (int i = 0; i < lines.Length; i++)
		{
			int dir = lines[i][0] == 'L' ? -1 : 1;
			int num = int.Parse(lines[i].Substring(1));
			
			Console.WriteLine($"{currentNum} + {num * dir} = {currentNum + num * dir}");

			int oldNum = currentNum;
			
			int circles = Math.Abs(num / 100);
			counterExtended += circles;
			num %= 100; // cut off circles
			
			currentNum += num * dir;
			
			if (currentNum == 0)
			{
				counterExtended++;
			}

			if (currentNum > 99)
			{
				counterExtended++;
				currentNum -= 100;
			}

			if (currentNum < 0)
			{
				if (oldNum != 0)
				{
					counterExtended++;
				}
				currentNum += 100;
			}
			
			Console.WriteLine($"normalised: {currentNum}, counter {counterExtended}");
		}

		Console.WriteLine(counterExtended);
	}
}