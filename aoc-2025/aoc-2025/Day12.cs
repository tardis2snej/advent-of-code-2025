using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace aoc_2025;

public class Day12
{
	private class Shape
	{
		public readonly int Id;
		public readonly double TotalSize;
		public Matrix<double> Matrix { get; private set; }

		public static bool TryParse(string input, out Shape shape)
		{
			shape = null;
			
			string[] inputs = input.Split(Environment.NewLine);
			string idStr = inputs[0].Trim(':');

			if (!int.TryParse(idStr, out int id))
			{
				return false;
			}
			
			double[][] matrix = new double[inputs.Length - 1][];

			for (int i = 0; i < inputs.Length - 1; i++)
			{
				string line = inputs[i + 1].Replace('#', '1').Replace('.', '0');
				matrix[i] = new double[line.Length];

				for (int j = 0; j < matrix[i].Length; j++)
				{
					if (!double.TryParse(line[j].ToString(), out var num))
					{
						return false;
					}

					matrix[i][j] = num;
				}
			}

			shape = new Shape(id, DenseMatrix.OfRowArrays(matrix));

			return true;
		}

		private Shape(int id, Matrix<double> matrix)
		{
			Id = id;
			Matrix = matrix;
			TotalSize = matrix.RowSums().Sum();
		}
	}

	private class Space
	{
		private readonly Vector2 _dimensions;
		private readonly double _totalSize;
		private readonly Dictionary<int, int> _boxesToFit;
		private Matrix<double> _field;
		private Dictionary<int, Shape> _shapes;
		private readonly Matrix<double> _emptyFieldTemplate;

		public Space(string info)
		{
			string[] parameters = info.Split(' ');
			string[] dimensionsStr = parameters[0].Split('x');
			_dimensions = new Vector2(int.Parse(dimensionsStr[0]), int.Parse(dimensionsStr[1].TrimEnd(':')));
			_boxesToFit = new();

			for (int i = 0; i < parameters.Length - 1; i++)
			{
				_boxesToFit.Add(i, int.Parse(parameters[i + 1]));
			}
			
			_totalSize = _dimensions.X * _dimensions.Y;
			_emptyFieldTemplate = DenseMatrix.Create((int)_dimensions.Y, (int)_dimensions.X, (_,_) => 0);
			_field = DenseMatrix.OfMatrix(_emptyFieldTemplate);
		}

		public bool TryFitAll(Dictionary<int, Shape> shapes)
		{
			_shapes = shapes;
			double totalSizeToFit = 0;
			double totalBoxesToFit = 0;
			foreach (var box in _boxesToFit)
			{
				totalSizeToFit += _shapes[box.Key].TotalSize * box.Value;
				totalBoxesToFit += box.Value;
			}

			if (totalSizeToFit >= _totalSize)
			{
				return false;
			}

			// sort by shape size
			// _boxesToFit.Sort((item1, item2) =>
			// 	                 shapes[item1.id].TotalSize.CompareTo(shapes[item2.id].TotalSize));
			// _boxesToFit.Reverse();

			bool isLastBoxFits = true;
			
			Vector2[] potentialPoints = new Vector2[1];
			potentialPoints[0] = Vector2.Zero;

			while (totalBoxesToFit > 0 && isLastBoxFits)
			{
				isLastBoxFits = TryPickAndPlaceNextBox(ref potentialPoints, out var id);
				Console.WriteLine($"{_dimensions.X}x{_dimensions.Y} : boxes left {totalBoxesToFit}");
				Console.WriteLine(_field.ToString());
				Console.WriteLine();

				if (isLastBoxFits)
				{
					totalBoxesToFit--;
					_boxesToFit[id]--;
				}
			}
			
			return totalSizeToFit == 0;
		}

		public bool TryPickAndPlaceNextBox(ref Vector2[] potentialPoints, out int placedShapeId)
		{
			List<(Matrix<double> newField, Vector2[] spaceAround, int shapeId)> boxesThatFit = new();
			
			for (int i = 0; i < potentialPoints.Length; i++)
			{
				for (int j = 0; j < _boxesToFit.Count; j++)
				{
					if (_boxesToFit[j] == 0)
					{
						continue;
					}
					
					for (int rotations = 0; rotations < 4; rotations++)
					{
						Matrix<double> cache = DenseMatrix.OfMatrix(_field);

						if (TryPlaceShape(ref cache, potentialPoints[i], _shapes[j], rotations, out var spaceAround))
						{
							boxesThatFit.Add((cache, spaceAround, j));
						}
						
						// mirror as well?
					}
				}
			}

			if (boxesThatFit.Count == 0)
			{
				placedShapeId = -1;
				return false;
			}
			
			int minSpace = boxesThatFit.Min(x => x.spaceAround.Length);
			int cachedMin = boxesThatFit.FindIndex(item => item.spaceAround.Length == minSpace);
			potentialPoints = boxesThatFit[cachedMin].spaceAround;
			_field = boxesThatFit[cachedMin].newField;
			placedShapeId = boxesThatFit[cachedMin].shapeId;
			return true;
		}

		public bool TryPlaceShape(ref Matrix<double> field, Vector2 point, Shape shape, int rotations, out Vector2[] spaceAround)
		{
			// perform transformations
			Matrix<double> shapeMatrix = DenseMatrix.OfMatrix(shape.Matrix);
			for (int i = 1; i < rotations; i++)
			{
				Console.WriteLine($"Rotating for {90 * i}");
				Console.WriteLine(shapeMatrix.ToString());
				shapeMatrix = shapeMatrix.Transpose();
				//Matrix<double> diagonalMatrix = DenseMatrix.Build.DenseDiagonal(shapeMatrix.RowCount, shapeMatrix.ColumnCount, 1).Transpose();
				//shapeMatrix *= diagonalMatrix;
				Console.WriteLine();
				Console.WriteLine(shapeMatrix.ToString());

			}
			// Console.WriteLine($"Trying to place #{shape.Id}: ");
			// Console.WriteLine(shapeMatrix.ToString());
			
			Matrix<double> shapeMatrixField = DenseMatrix.OfMatrix(_emptyFieldTemplate);

			try
			{
				shapeMatrixField.SetSubMatrix((int)point.Y, (int)point.X, shapeMatrix);
			}
			catch (ArgumentOutOfRangeException exception)
			{
				spaceAround = null;
				return false;
			}
			
			field = field.Add(shapeMatrixField);

			List<Vector2> newSpaceAround = new();
			for (int i = 0; i < field.RowCount; i++)
			{
				for (int j = 0; j < field.ColumnCount; j++)
				{
					if (field.At(i, j) > 1) // overlap
					{
						spaceAround = null;
						return false;
					}

					if (field.At(i, j) == 1)
					{
						Vector2[] surroundingPoints = GetSurroundingPoints(field, i, j);

						for (int p = 0; p < surroundingPoints.Length; p++)
						{
							if (field.At((int)surroundingPoints[p].Y, (int)surroundingPoints[p].X) == 0
							    && !newSpaceAround.Contains(surroundingPoints[p]))
							{
								newSpaceAround.Add(surroundingPoints[p]);
							}
						}
					}
				}
			}

			spaceAround = newSpaceAround.ToArray();
			return true;
		}
		
		
		private Vector2[] GetSurroundingPoints(Matrix<double> matrix, int row, int column)
		{
			List<Vector2> points = new();
		
			if (column > 0)
			{
				points.Add(new Vector2(column - 1, row)); // point on the left
			
				if (row > 0)
				{
					points.Add(new Vector2(column - 1, row - 1)); // upper left corner
				}

				if (row < matrix.RowCount - 1) // lower left corner
				{
					points.Add(new Vector2(column - 1, row + 1));
				}
			}

			if (column < matrix.ColumnCount - 1)
			{
				points.Add(new Vector2(column + 1, row)); // point on the right
			
				if (row > 0)
				{
					points.Add(new Vector2(column + 1, row - 1)); // upper right corner
				}

				if (row < matrix.RowCount - 1) // lower right corner
				{
					points.Add(new Vector2(column + 1, row + 1));
				}
			}

			if (row > 0)
			{
				points.Add(new Vector2(column, row - 1)); // up
			}

			if (row < matrix.RowCount - 1)
			{
				points.Add(new Vector2(column, row + 1));
			}

			return points.ToArray();
		}
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] inputs = text.Split([Environment.NewLine + Environment.NewLine],
		                                   StringSplitOptions.RemoveEmptyEntries);
		int iterator = 0;

		Dictionary<int, Shape> shapes = new();

		while (iterator < inputs.Length && Shape.TryParse(inputs[iterator], out var shape))
		{
			shapes.Add(shape.Id, shape);
			iterator++;
		}
		
		List<Space> spaces = new();
		
		string[] spacesStr = inputs[^1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		
		for (int i = 0; i < spacesStr.Length; i++)
		{
			spaces.Add(new Space(spacesStr[i]));
		}

		int counter = 0;
		for(int i = 0; i < spaces.Count; i++)
		{
			counter += spaces[i].TryFitAll(shapes) ? 1 : 0;
		}
		
		Console.WriteLine($"Spaces that fit all boxes: {counter}");
	}
}