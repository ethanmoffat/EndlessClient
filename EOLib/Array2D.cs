// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib
{
	public class Array2D<T> : IReadOnly2DArray<T>
	{
		private readonly T[,] _arr;

		public int Rows { get; private set; }
		public int Cols { get; private set; }

		public Array2D(int rows, int cols)
		{
			Rows = rows;
			Cols = cols;
			_arr = new T[rows, cols];
		}

		public Array2D(int rows, int cols, T defaultValue)
			: this(rows, cols)
		{
			Fill(defaultValue);
		} 

		public void Fill(T value)
		{
			for (int row = 0; row < Rows; ++row)
			{
				for (int col = 0; col < Cols; ++col)
				{
					_arr[row, col] = value;
				}
			}
		}

		public T this[int row, int col]
		{
			get { return _arr[row, col]; }
			set { _arr[row, col] = value; }
		}

		public static implicit operator T[,](Array2D<T> array)
		{
			var ret = new T[array.Rows, array.Cols];
			Array.Copy(array._arr, ret, ret.Length);
			return ret;
		}

		public static implicit operator Array2D<T>(T[,] array)
		{
			var ret = new Array2D<T>(array.GetLength(0), array.GetLength(1));
			Array.Copy(array, ret._arr, array.Length);
			return ret;
		}
	}
}
