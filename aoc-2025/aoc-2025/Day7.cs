namespace aoc_2025;

public class Day7
{
	private const char ENTRY = 'S';
	private const char SPLITTER = '^';
	private const char BEAM = '|';
	private const char SPACE = '.';
	
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		char[][] map = ParseInput(lines);

		int startLoc = Array.IndexOf(map[0], ENTRY);
		map[0][startLoc] = BEAM;
		int count = TraceBeam(ref map, 0, startLoc);
		
		PrintMap(map);
		
		Console.WriteLine($"Total splits: {count}");
	}

	private char[][] ParseInput(string[] lines)
	{
		char[][] map = new char[lines.Length][];
		
		for (int i = 0; i < lines.Length; i++)
		{
			map[i] = new char[lines[i].Length];

			for (int j = 0; j < lines[i].Length; j++)
			{
				map[i][j] = lines[i][j];
			}
		}

		return map;
	}

	private int TraceBeam(ref char[][] map, int startLine, int startLoc)
	{
		int splitCount = 0;

		for (int i = startLine + 1; i < map.Length; i++)
		{
			if (map[i][startLoc] == SPACE)
			{
				map[i][startLoc] = BEAM;
			}
			else if (map[i][startLoc] == SPLITTER)
			{
				splitCount++;

				if (startLoc > 0)
				{
					map[i][startLoc - 1] = BEAM;
					splitCount += TraceBeam(ref map,i, startLoc - 1);
				}

				if (startLoc < map.Length - 1)
				{
					map[i][startLoc + 1] = BEAM;
					splitCount += TraceBeam(ref map,i, startLoc + 1);
				}

				break;
			}
			else if (map[i][startLoc] == BEAM)
			{
				break;
			}
		}

		return splitCount;
	}
	
	private void PrintMap(char[][] map)
	{
		for (int i = 0; i < map.Length; i++)
		{
			for (int j = 0; j < map[i].Length; j++)
			{
				Console.Write(map[i][j]);
			}
			Console.Write("\n");
		}
	}
}