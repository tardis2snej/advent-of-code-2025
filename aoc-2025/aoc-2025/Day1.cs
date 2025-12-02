namespace aoc_2025;

public class Day1
{
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		int result = Calculate(text.Split('\n'));
		Console.WriteLine($"Password: {result}");
	}

	private int Calculate(string[] lines)
	{
		int counterExtended = 0;
		int currentNum = 50;
		
		for (int i = 0; i < lines.Length; i++)
		{
			int oldNum = currentNum;
			
			int rotationAmount = ParseLine(lines[i]);
			rotationAmount = NormaliseRotationAmount(rotationAmount, out int circles);
			counterExtended += circles;
			
			currentNum += rotationAmount;
			
			if (currentNum == 0)
			{
				counterExtended++;
			}

			if (currentNum > 99)
			{
				counterExtended++;
				currentNum -= 100;
			}
			else if (currentNum < 0)
			{
				if (oldNum != 0)
				{
					counterExtended++;
				}
				currentNum += 100;
			}
		}

		return counterExtended;
	}

	private int ParseLine(string line)
	{
		int dir = line[0] == 'L' ? -1 : 1;

		if (int.TryParse(line.Substring(1), out int num))
		{
			return num * dir;
		}
		else
		{
			Console.WriteLine("ERROR: failed to parse input");
			return 0;
		}
	}

	private int NormaliseRotationAmount(int initialRotation, out int circles)
	{
		circles = Math.Abs(initialRotation / 100);
		initialRotation %= 100;

		return initialRotation;
	}
}