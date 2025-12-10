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
		
		private readonly uint _targetPattern;
		private readonly uint[] _targetJoltages;

		private uint[] _buttons;
		
		public Machine(string info)
		{
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

		private bool CheckBootSequence(List<uint> sequence)
		{
			uint indicator = 0;
			
			foreach (uint btn in sequence)
			{
				indicator ^= btn;
			}
			
			return indicator == _targetPattern;
		}

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
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		Machine[] machines = new Machine[lines.Length];

		int sum = 0;
		for (int i = 0; i < lines.Length; i++)
		{
			machines[i] = new Machine(lines[i]);
			int ans = machines[i].Hack(true);
			Console.WriteLine($"\n{i + 1} : {ans}");
			sum += ans;
		}

		Console.WriteLine($"Result: {sum}");
	}
}