// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public interface IReadOnlyMatrix<T>
    {
        int Rows { get; }

        int Cols { get; }

        T this[int row, int col] { get; }
    }
}