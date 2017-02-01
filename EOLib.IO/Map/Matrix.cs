// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections;
using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public class Matrix<T> : IReadOnlyMatrix<T>
    {
        private static readonly Matrix<T> _empty = new Matrix<T>(0, 0);
        public static Matrix<T> Empty => _empty;

        private readonly T[,] _arr;

        public int Rows { get; private set; }
        public int Cols { get; private set; }

        private Matrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _arr = new T[rows, cols];
        }

        public Matrix(int rows, int cols, T defaultValue)
            : this(rows, cols)
        {
            Fill(defaultValue);
        } 

        public Matrix(Matrix<T> other)
            : this(other.Rows, other.Cols)
        {
            for (int row = 0; row < other.Rows; ++row)
                for (int col = 0; col < other.Cols; ++col)
                    _arr[row, col] = other[row, col];
        }

        private void Fill(T value)
        {
            for (int row = 0; row < Rows; ++row)
                for (int col = 0; col < Cols; ++col)
                    _arr[row, col] = value;
        }

        public T[] GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows)
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index is out of bounds of the number of matrix rows");

            var retArray = new T[Cols];
            for (int i = 0; i < Cols; ++i)
                retArray[i] = _arr[rowIndex, i];

            return retArray;
        }

        public T this[int row, int col]
        {
            get { return _arr[row, col]; }
            set { _arr[row, col] = value; }
        }

        public static implicit operator T[,](Matrix<T> array)
        {
            var ret = new T[array.Rows, array.Cols];
            Array.Copy(array._arr, ret, ret.Length);
            return ret;
        }

        public static implicit operator Matrix<T>(T[,] array)
        {
            var ret = new Matrix<T>(array.GetLength(0), array.GetLength(1));
            Array.Copy(array, ret._arr, array.Length);
            return ret;
        }

        public IEnumerator<IList<T>> GetEnumerator()
        {
            return new MatrixRowEnumerator(_arr);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _arr.GetEnumerator();
        }

        private class MatrixRowEnumerator : IEnumerator<IList<T>>
        {
            private Matrix<T> _matrix;
            private int _row = -1;

            public MatrixRowEnumerator(Matrix<T> matrix)
            {
                _matrix = matrix;
            }

            public void Dispose()
            {
                _row = -1;
                _matrix = null;
            }

            public bool MoveNext()
            {
                if (_row + 1 >= _matrix.Rows)
                    return false;

                _row++;

                return true;
            }

            public void Reset()
            {
                _row = 0;
            }

            public IList<T> Current => _matrix.GetRow(_row);

            object IEnumerator.Current => Current;
        }
    }
}
