namespace aoc_2025;

public class Day7
{
	private const char ENTRY = 'S';
	private const char SPLITTER = '^';
	private const char BEAM = '|';
	private const char SPACE = '.';
	
	private const int ENTRY_INT = 1;
	private const int SPLITTER_INT = -1;
	private const int SPACE_INT = 0;
	
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		long[][] map = ParseInputAsNum(lines);

		CalculateTimelines(ref map);
		
		int lastLine = map.Length - 1;
		long count = 0;

		for (int i = 0; i < map[lastLine].Length; i++)
		{
			count += map[lastLine][i];
		}
		
		PrintMap(map);
		
		Console.WriteLine($"Total timelines: {count}");
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
	
	private long[][] ParseInputAsNum(string[] lines)
	{
		long[][] map = new long[lines.Length][];
		
		for (int i = 0; i < lines.Length; i++)
		{
			map[i] = new long[lines[i].Length];

			for (int j = 0; j < lines[i].Length; j++)
			{
				if (lines[i][j] == ENTRY)
				{
					map[i][j] = ENTRY_INT;
				}
				else if (lines[i][j] == SPLITTER)
				{
					map[i][j] = SPLITTER_INT;
				}
				else if (lines[i][j] == SPACE)
				{
					map[i][j] = SPACE_INT;
				}
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

	private void CalculateTimelines(ref long[][] map)
	{
		for (int i = 1; i < map.Length; i++)
		{
			for (int j = 0; j < map[i].Length; j++)
			{
				if (map[i][j] > SPLITTER_INT && map[i - 1][j] != SPLITTER_INT) // direct beam
				{
					map[i][j] += map[i - 1][j];
				}
				else if (map[i][j] == SPLITTER_INT)
				{
					if (j > 0) // upd left side
					{
						map[i][j - 1] += map[i - 1][j];
					}
					
					if (j < map[i].Length - 1) // upd right side
					{
						map[i][j + 1] += map[i - 1][j];
					}
				}
			}
		}
	}
	
	private void PrintMap<T>(T[][] map)
	{
		foreach (var i in map)
		{
			foreach (var j in i)
			{
				Console.Write(j + "\t");
			}

			Console.Write("\n");
		}
	}
}