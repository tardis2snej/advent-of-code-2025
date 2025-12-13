#define DEBUG_ENABLED

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Z3;

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
		private readonly uint[] _targetJoltagesSorted;
		private readonly uint _maxJoltageValue;
		private readonly int _joltageCount;
		private readonly uint[] _currentJoltageCache;

		private readonly uint[] _buttons;
		private Matrix<double> _buttonsMatrix;
		
		public Machine(string info, int id)
		{
			Id = id;
			string[] parameters = info.Split(' ');
			_targetPattern = ParseIndicatorPattern(parameters[0]);
			
			_targetJoltages = ParseJoltages(parameters[^1]);
			_joltageCount = _targetJoltages.Length;
			_currentJoltageCache = new uint[_joltageCount];
			
#if DEBUG_ENABLED
			_targetPatternDebugStr = Convert.ToString(_targetPattern, 2);
#endif
			List<uint> buttons = new List<uint>();
			List<double[]> matrix = new();

			int iterator = 1;
			while (iterator < parameters.Length && parameters[iterator][0] == '(')
			{
				uint button = ParseButton(parameters[iterator]);
				buttons.Add(button);
				matrix.Add(BinaryNumToArray(button, _joltageCount));
				
#if DEBUG_ENABLED
				_buttonsDebugStr.Add(Convert.ToString(button, 2));
#endif
				
				iterator++;
			}
			_buttons = buttons.ToArray();
			_buttonsMatrix = DenseMatrix.OfColumnArrays(matrix.ToArray());
			_targetJoltagesSorted = [.._targetJoltages];
			Sort(ref _buttonsMatrix, ref _targetJoltagesSorted);

			_maxJoltageValue = _targetJoltages.Max();
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

		private double[] BinaryNumToArray(uint btn, int len)
		{
			string binaryText = Convert.ToString(btn, 2);
			char[] array = binaryText.ToCharArray();
			Array.Reverse(array);
			binaryText = new string(array);
			
			double[] arr = new double[len];
			for (int i = binaryText.Length - 1; i >= 0; i--)
			{
				arr[i] = uint.Parse(binaryText[i].ToString());
			}

			return arr;
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

		public uint Hack_LowMemory_BruteForce()
		{
			bool hasAnswer = false;
			int buttonsCount = _buttons.Length;
			int[] indexesSeq = new int[_maxJoltageValue];
			indexesSeq[^1] = -1;
			uint sequenceCount = _maxJoltageValue;
			uint counter = _maxJoltageValue;
			
			while (true)
			{
				indexesSeq[^1]++;
				counter++;
				
				if (indexesSeq[^1] == buttonsCount)
				{
					for (uint i = sequenceCount - 1; i > 0; i--)
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

				sequenceCount = (uint)indexesSeq.Length;

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
		
		public long Hack_PickCombination()
		{
			Global.ToggleWarningMessages(true);
			uint totalCount = 0;

			using (Context ctx = new Context(new Dictionary<string, string>() { { "model", "true" } }))
			{
				int coefficientsCount = _buttonsMatrix.ColumnCount;
				
				IntExpr[] coefficients = new IntExpr[coefficientsCount];
				BoolExpr[] coefficientsConstraints_Bounds = new BoolExpr[coefficientsCount];
				ArithExpr coefficientSums = null;

				for (int i = 0; i < coefficientsCount; i++)
				{
					coefficients[i] = (IntExpr)ctx.MkConst(ctx.MkSymbol("x_" + (i + 1)), ctx.IntSort);
					
					coefficientsConstraints_Bounds[i] = ctx.MkAnd(ctx.MkLe(ctx.MkInt(0), coefficients[i]));

					if (coefficientSums == null)
					{
						coefficientSums = coefficients[i];
					}
					else
					{
						coefficientSums += coefficients[i]; 
					}
				}
				
				double[][] buttonsModel = _buttonsMatrix.ToRowArrays();
				BoolExpr[] sumConstraints = new BoolExpr[_joltageCount];
				for (int i = 0; i < _joltageCount; i++)
				{
					ArithExpr[] operations = new ArithExpr[_joltageCount];
					for (int j = 0; j < coefficientsCount; j++)
					{
						if (operations[i] == null)
						{
							operations[i] = ctx.MkMul(ctx.MkInt((int)buttonsModel[i][j]), coefficients[j]);
						}
						else
						{
							operations[i] += ctx.MkMul(ctx.MkInt((int)buttonsModel[i][j]), coefficients[j]);
						}
					}

					sumConstraints[i] = ctx.MkEq(operations[i], ctx.MkInt(_targetJoltagesSorted[i]));
				}
				

				Optimize optimization = ctx.MkOptimize();
				optimization.Assert(coefficientsConstraints_Bounds);
				optimization.Assert(sumConstraints);
				optimization.MkMinimize(coefficientSums);

				var result = optimization.Check();
				Console.WriteLine(result);

				Model model = optimization.Model;
				
				uint[] solution = new uint[coefficientsCount];
				for (int i = 0; i < coefficientsCount; i++)
				{
					Expr evaluated = model.Eval(coefficients[i]);
					solution[i] = uint.Parse(evaluated.ToString());
					totalCount += solution[i];
					Console.WriteLine(solution[i]);
				}

				Console.WriteLine($"Solution fits: {CheckCombination(solution)})");
				
				ctx.Dispose();
			}

			return totalCount;
		}
		
		private void Sort(ref Matrix<double> matrix, ref uint[] sortValues)
		{
			double[][] matrixArr = matrix.ToRowArrays();
			
			for (int i = 0; i < sortValues.Length - 1; i++)
			{
				int minIndex = i;
				for (int j = i + 1; j < sortValues.Length; j++)
				{
					if (sortValues[j] < sortValues[minIndex])
					{
						minIndex = j;
					}
				}
				uint tempSort = sortValues[i];
				double[] tempRow = matrixArr[i];
				sortValues[i] = sortValues[minIndex];
				matrixArr[i] = matrixArr[minIndex];
				sortValues[minIndex] = tempSort;
				matrixArr[minIndex] = tempRow;
			}
			
			matrix = DenseMatrix.OfRowArrays(matrixArr);
		}

		private int[] GetUpperBounds(Matrix<double> matrix, uint[] outputs)
		{
			int[] bounds = new int[matrix.ColumnCount];

			for (int i = 0; i < bounds.Length; i++)
			{
				bounds[i] = (int)_maxJoltageValue;
				for (int j = 0; j < outputs.Length; j++)
				{
					if (matrix[j,i] == 1 && bounds[i] > outputs[j])
					{
						bounds[i] = (int)outputs[j];
					}
				}
			}

			return bounds;
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

		private bool CheckCombination(uint[] coefficients)
		{
			for (int i = 0; i < _buttonsMatrix.RowCount; i++)
			{
				double rowSum = 0;
				for (int j = 0; j < _buttonsMatrix.ColumnCount; j++)
				{
					rowSum += _buttonsMatrix[i, j] * coefficients[j];
				}

				if (rowSum != _targetJoltagesSorted[i])
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

		for (int i = 0; i < machinesCount; i++)
		{
			Console.WriteLine($"Starting {machines[i].Id} (started:{startedCount}/finished:{finishedCount}/total:{machinesCount})");
							
			long ans = machines[i].Hack_PickCombination();
							
			Console.WriteLine($"\n{i + 1} : {ans}");
			sum += ans;
		}
		
		Console.WriteLine($"Result: {sum}");
	}
}