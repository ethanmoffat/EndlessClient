using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public interface IReadOnlyMatrix<T> : IEnumerable<IList<T>>
    {
        int Rows { get; }

        int Cols { get; }

        T[] GetRow(int rowIndex);

        T this[int row, int col] { get; }
    }
}