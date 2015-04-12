using System;

namespace Eagle.Server.Framework.Generic
{
    /// <summary>
    /// Object matrix worker class similar to an array.
    /// </summary>
    /// <typeparam name="TType">The type of elements in the matrix</typeparam>
    public class DataMatrix<TType>
    {

        // multi dimensional items array
        private TType[,] _items;

        /// <summary>
        /// Creates a new Data Matrix with no columns or rows
        /// </summary>
        public DataMatrix()
        {
            _items = new TType[0, 0];
            NumColumns = 0;
            NumRows = 0;
        }

        /// <summary>
        /// Creates a new Data Matrix of the specified size filled with null elements
        /// </summary>
        /// <param name="rows">The number of Rows in the matrix</param>
        /// <param name="columns">The number of Columns in the matrix</param>
        public DataMatrix(int rows, int columns)
        {
            _items = new TType[rows, columns];
            NumColumns = columns;
            NumRows = rows;
        }

        /// <summary>
        /// Creates a new Data Matrix from the elements of a Two-Dimensional array
        /// </summary>
        /// <param name="array">A Multi-Dimensional array of elements to fill the matrix with</param>
        public DataMatrix(TType[,] array)
        {
            if (array.Rank != 2)
            {
                throw new InvalidOperationException("Incorrect number of dimensions in array");
            }
            NumRows = array.GetLength(0);
            NumColumns = array.GetLength(1);
            _items = array;
        }

        /// <summary>
        /// Adds a new empty row to the end of the Matrix
        /// </summary>
        public void AddRow()
        {
            //create the new items array
            NumRows++;
            var newItems = new TType[NumRows, NumColumns];

            //add the current items to the array
            for (int i = 0; i < NumRows - 1; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    newItems[i, j] = _items[i, j];
                }
            }

            //replace the array
            _items = newItems;
        }

        /// <summary>
        /// Adds a new column to the end of the Matrix
        /// </summary>
        public void AddColumn()
        {
            //create the new items array
            NumColumns++;
            var newItems = new TType[NumRows, NumColumns];

            //add the current items to the array
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns - 1; j++)
                {
                    newItems[i, j] = _items[i, j];
                }
            }

            //replace the array
            _items = newItems;
        }

        /// <summary>
        /// Gets the item at the specified position
        /// </summary>
        /// <param name="row">The row that contains the item</param>
        /// <param name="column">The column that contains the item</param>
        /// <returns>The item at the specified postion in the Matrix</returns>
        public TType GetItem(int row, int column)
        {
            return _items[row, column];
        }

        /// <summary>
        /// Inserts an item into a specified position in the Matrix. Will over-write any existing item;
        /// </summary>
        /// <param name="item">The item to insert into the Matrix</param>
        /// <param name="row">The row where the item shall be inserted</param>
        /// <param name="column">The column where the item shall be inserted</param>
        public void InsertItem(TType item, int row, int column){
            _items[row, column] = item;
        }

        /// <summary>
        /// Gets a two-dimensional array of the Matrix' values
        /// </summary>
        /// <returns>A two-dimensional array of the Matrix' values</returns>
        public TType[,] GetValues()
        {
            return _items;
        }

        /// <summary>
        /// The number of rows in the Matrix
        /// </summary>
        public int NumRows { get; private set; }

        /// <summary>
        /// The number of Columns in the Matrix
        /// </summary>
        public int NumColumns { get; private set; }
    }
}
