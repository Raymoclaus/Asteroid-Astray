using UnityEngine;

[System.Serializable]
public struct BooleanMatrix
{
	public bool[][] matrix;

	public int Width { get { return matrix?.Length ?? 0; } }
	public int Height { get { return matrix == null ? 0 : (matrix?.Length > 0 ? matrix[0].Length : 0); } }

	public BooleanMatrix(int width, int height, bool defaultValue = false)
	{
		matrix = new bool[width][];
		for (int i = 0; i < matrix.Length; i++)
		{
			matrix[i] = new bool[height];
			for (int j = 0; j < matrix[i].Length; j++)
			{
				matrix[i][j] = defaultValue;
			}
		}
	}

	public static BooleanMatrix And(params BooleanMatrix[] list)
	{
		if (list.Length == 0) return new BooleanMatrix(0, 0);
		if (list.Length == 1) return list[0];
		int width = GetSmallestWidth(list);
		int height = GetSmallestHeight(list);
		BooleanMatrix newMatrix = new BooleanMatrix(width, height, true);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < list.Length; k++)
				{
					newMatrix.matrix[i][j] &= list[k].matrix[i][j];
					if (!newMatrix.matrix[i][j]) break;
				}
			}
		}

		return newMatrix;
	}

	public static BooleanMatrix Or(params BooleanMatrix[] list)
	{
		if (list.Length == 0) return new BooleanMatrix(0, 0);
		if (list.Length == 1) return list[0];
		int width = GetSmallestWidth(list);
		int height = GetSmallestHeight(list);
		BooleanMatrix newMatrix = new BooleanMatrix(width, height);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < list.Length; k++)
				{
					newMatrix.matrix[i][j] |= list[k].matrix[i][j];
					if (newMatrix.matrix[i][j]) break;
				}
			}
		}

		return newMatrix;
	}

	private static int GetSmallestWidth(params BooleanMatrix[] list)
	{
		if (list.Length == 0) return 0;
		if (list.Length == 1) return list[0].Width;

		int smallestWidth = int.MaxValue;
		for (int i = 0; i < list.Length; i++)
		{
			smallestWidth = Mathf.Min(smallestWidth, list[i].Width);
		}
		return smallestWidth;
	}

	private static int GetSmallestHeight(params BooleanMatrix[] list)
	{
		if (list.Length == 0) return 0;
		if (list.Length == 1) return list[0].Height;

		int smallestHeight = int.MaxValue;
		for (int i = 0; i < list.Length; i++)
		{
			smallestHeight = Mathf.Min(smallestHeight, list[i].Height);
		}
		return smallestHeight;
	}

	private bool[] GetOneDimensionalArray()
	{
		bool[] arr = new bool[Width * Height];

		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				int index = i * Height + j;

				arr[index] = matrix[i][j];
			}
		}

		return arr;
	}

	public override string ToString()
	{
		string s = string.Empty;
		string separator = ", ";
		bool[] arr = GetOneDimensionalArray();

		for (int i = 0; i < arr.Length; i++)
		{
			s += arr[i];
			if (i != arr.Length - 1)
			{
				s += separator;
			}
		}

		return s;
	}
}
