namespace aoc_2025;

public class Day6
{
	private abstract class Problem
	{
		public long EndProduct { get; protected set; }
		
		protected abstract long PerformOperation(long value);

		public long AddValue(long value)
		{
			EndProduct = PerformOperation(value);

			return EndProduct;
		}
	}

	private class MulProblem : Problem
	{
		public MulProblem()
		{
			EndProduct = 1;
		}
		
		protected override long PerformOperation(long value) => EndProduct * value;
	}
	
	private class AddProblem : Problem
	{
		protected override long PerformOperation(long value) => EndProduct + value;
	}
	
	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] problemsTexts = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		
		Problem[] problems = ParseProblems(problemsTexts[^1]);

		for (int i = 0; i < problemsTexts.Length - 1; i++)
		{
			long[] numbers = ParseLine(problemsTexts[i]);

			if (numbers.Length != problems.Length)
			{
				Console.WriteLine("ERROR: input misallignment");
				continue;
			}

			for (int j = 0; j < problems.Length; j++)
			{
				problems[j].AddValue(numbers[j]);
			}
		}

		long sum = 0;
		foreach (var problem in problems)
		{
			sum += problem.EndProduct;
		}
		
		Console.WriteLine($"Sum is: {sum}");
	}

	private Problem[] ParseProblems(string operators)
	{
		string[] allOperators = operators.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		Problem[] problems = new Problem[allOperators.Length];

		for(int i = 0; i < allOperators.Length; i++)
		{
			if (allOperators[i] == "+")
			{
				problems[i] = new AddProblem();
			}
			else if (allOperators[i] == "*")
			{
				problems[i] = new MulProblem();
			}
		}

		return problems;
	}

	private long[] ParseLine(string line)
	{
		string[] allNumbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		long[] numbers = new long[allNumbers.Length];

		for(int i = 0; i < allNumbers.Length; i++)
		{
			numbers[i] = long.Parse(allNumbers[i]);
		}

		return numbers;
	}
}