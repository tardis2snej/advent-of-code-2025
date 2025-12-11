#define DEBUG_ENABLED

namespace aoc_2025;

public class Day10
{
	private class Machine
	{
#if DEBUG_ENABLED
		private readonly string _targetPatternDebugStr;
		private List<string> _buttonsDebugStr = new();
#endif
		public readonly int Id;
		private readonly uint _targetPattern;
		private readonly uint[] _targetJoltages;
		private readonly int _lowerBound;
		private readonly int _joltageCount;
		private readonly uint[] _currentJoltageCache;

		private readonly uint[] _buttons;
		
		public Machine(string info, int id)
		{
			Id = id;
			string[] parameters = info.Split(' ');
			_targetPattern = ParseIndicatorPattern(parameters[0]);
			
#if DEBUG_ENABLED
			_targetPatternDebugStr = Convert.ToString(_targetPattern, 2);
#endif
			
			List<uint> buttons = new List<uint>();

			int iterator = 1;
			while (iterator < parameters.Length && parameters[iterator][0] == '(')
			{
				uint button = ParseButton(parameters[iterator]);
				buttons.Add(button);
				
#if DEBUG_ENABLED
				_buttonsDebugStr.Add(Convert.ToString(button, 2));
#endif
				
				iterator++;
			}
			_buttons = buttons.ToArray();

			_targetJoltages = ParseJoltages(parameters[^1]);
			_joltageCount = _targetJoltages.Length;
			_currentJoltageCache = new uint[_joltageCount];

			_lowerBound = (int)_targetJoltages.Max();
		}

		private uint ParseIndicatorPattern(string text)
		{
			text = text.Trim('[').Trim(']');
			string binary = text.Replace('.', '0').Replace('#', '1');
			char[] array = binary.ToCharArray();
			Array.Reverse(array);
			string reversedStr = new string(array);

			return Convert.ToUInt16(reversedStr, 2);
		}

		private uint ParseButton(string text)
		{
			text = text.Trim('(').Trim(')');
			string[] lightIndex = text.Split(',');

			uint button = 0;

			foreach (string light in lightIndex)
			{
				uint toggle = 1;
				int shift = int.Parse(light);
				toggle <<= shift;
				button |= toggle;
			}
			return button;
		}

		private uint[] ParseJoltages(string text)
		{
			text = text.Trim('{').Trim('}');
			string[] joltagesStr = text.Split(',');

			uint[] joltages = new uint[joltagesStr.Length];
			for (int i = 0; i < joltagesStr.Length; i++)
			{
				joltages[i] = uint.Parse(joltagesStr[i]);
			}

			return joltages;
		}

		public int Hack(bool isJoltage = false)
		{
			List<List<uint>> initialSequences = new();
			
			for (int i = 0; i < _buttons.Length; i++)
			{
				List<uint> seq = new();
				seq.Add(_buttons[i]);
				if (CheckSequence(seq, isJoltage))
				{
					return 1;
				}
				initialSequences.Add(seq);
			}

			int depth = 1;
			bool hasAnswer = false;
			List<List<uint>> sequencesForPrevDepth = initialSequences;

			while (!hasAnswer)
			{
				depth++;
				Console.Write($"D = {depth}... ");
				List<List<uint>> seqForNewDepth = new();

				for (int i = 0; i < sequencesForPrevDepth.Count; i++)
				{
					hasAnswer = CheckSequences(sequencesForPrevDepth[i], out var list, isJoltage);

					if (hasAnswer)
					{
						return depth;
					}
					seqForNewDepth.AddRange(list);
				}

				sequencesForPrevDepth = seqForNewDepth;
			}
			
			return depth;
		}

		public int Hack_LowMemory()
		{
			bool hasAnswer = false;
			int buttonsCount = _buttons.Length;
			int[] indexesSeq = new int[_lowerBound];
			indexesSeq[^1] = -1;
			int sequenceCount = _lowerBound;
			int counter = _lowerBound;
			
			while (true)
			{
				indexesSeq[^1]++;
				counter++;
				
				if (indexesSeq[^1] == buttonsCount)
				{
					for (int i = sequenceCount - 1; i > 0; i--)
					{
						if (indexesSeq[i] >= buttonsCount)
						{
							indexesSeq[i] = 0;
							indexesSeq[i - 1]++;
						}
					}

					if (indexesSeq[0] == buttonsCount)
					{
						indexesSeq[0] = 0;
						indexesSeq = [1, ..indexesSeq];
					}
				}

				sequenceCount = indexesSeq.Length;

				if (counter % 1000000 == 0)
				{
					Console.WriteLine($"#{Id}. D={sequenceCount}; {counter/1000000}M");
				}
					
				if (CheckJoltageSequenceByIndexes(ref indexesSeq))
				{
					break;
				}
			}

			return sequenceCount;
		}

		private bool CheckSequences(List<uint> parent, out List<List<uint>> sequences, bool isJoltage = false)
		{
			List<List<uint>> options = new();
			
			uint lastElement = parent[^1];

			for (int i = 0; i < _buttons.Length; i++)
			{
				if (!isJoltage && _buttons[i] == lastElement)
					continue;

				List<uint> newSeq = new List<uint>(parent);
				newSeq.Add(_buttons[i]);

				if (CheckSequence(newSeq, isJoltage))
				{
					sequences = null;
					return true;
				}
				
				options.Add(newSeq);
			}

			sequences = options;
			return false;
		}

		private bool CheckSequence(List<uint> sequence, bool isJoltage) =>
			isJoltage ? CheckJoltageSequence(sequence) : CheckBootSequence(sequence);

		private bool CheckJoltageSequence(List<uint> sequence)
		{
			uint[] currentJoltage = new uint[_targetJoltages.Length];
			
			foreach (uint btn in sequence)
			{
				for (int i = 0; i < currentJoltage.Length; i++)
				{
					uint indexMask = (uint)1 << i;
					if ((indexMask & btn) == indexMask)
					{
						currentJoltage[i]++;
					}
				}
			}
			
			return currentJoltage.SequenceEqual(_targetJoltages);
		}
		
		private bool CheckBootSequence(List<uint> sequence)
		{
			uint indicator = 0;
			
			foreach (uint btn in sequence)
			{
				indicator ^= btn;
			}
			
			return indicator == _targetPattern;
		}
		
		private bool CheckJoltageSequenceByIndexes(ref int[] indexes)
		{
			Array.Clear(_currentJoltageCache);
			int indexesLength = indexes.Length;
			
			for(int i = 0; i < indexesLength; i++)
			{
				for (int j = 0; j < _joltageCount; j++)
				{
					uint indexMask = (uint)1 << j;
					if ((indexMask & _buttons[indexes[i]]) == indexMask)
					{
						_currentJoltageCache[j]++;

						if (_currentJoltageCache[j] > _targetJoltages[j])
						{
							return false;
						}
					}
				}
			}

			for (int i = 0; i < _joltageCount; i++)
			{
				if (_currentJoltageCache[i] != _targetJoltages[i])
				{
					return false;
				}
			}
			
			return true;
		}
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		Machine[] machines = new Machine[lines.Length];

		long sum = 0;
		int machinesCount = machines.Length;
		for (int i = 0; i < lines.Length; i++)
		{
			machines[i] = new Machine(lines[i], i);
		}

		int startedCount = 0;
		int finishedCount = 0;

		Parallel.For(0, machinesCount, i =>
			             {
							Interlocked.Add(ref startedCount, 1);
							Console.WriteLine($"Starting {machines[i].Id} (started:{startedCount}/finished:{finishedCount}/total:{machinesCount})");
							
							int ans = machines[i].Hack_LowMemory();
							
							Console.WriteLine($"\n{i + 1} : {ans}");
							Interlocked.Add(ref sum, ans);
							Interlocked.Add(ref finishedCount, 1);
			             });

		Console.WriteLine($"Result: {sum}");
	}
}