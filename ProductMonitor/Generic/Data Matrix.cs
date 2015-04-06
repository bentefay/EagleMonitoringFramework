/* Generic Class designed to hold data in a tabular form like an array
 * The working parts of this have been developed specifically for the Product Monitor but 
 * it could be expanded to be fully re-usable (with the inclusion of accesses [0,0] etc)
 * By Adam Myers
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Product_Monitor.Generic
{
    /// <summary>
    /// Object matrix worker class similar to an array.
    /// </summary>
    /// <typeparam name="type">The type of elements in the matrix</typeparam>
    class Data_Matrix<type>
    {

        // multi dimensional items array
        private type[,] items;
        private int numRows;
        private int numColumns;

        /// <summary>
        /// Creates a new Data Matrix with no columns or rows
        /// </summary>
        public Data_Matrix()
        {
            items = new type[0, 0];
            numColumns = 0;
            numRows = 0;
        }

        /// <summary>
        /// Creates a new Data Matrix of the specified size filled with null elements
        /// </summary>
        /// <param name="rows">The number of Rows in the matrix</param>
        /// <param name="columns">The number of Columns in the matrix</param>
        public Data_Matrix(int rows, int columns)
        {
            items = new type[rows, columns];
            numColumns = columns;
            numRows = rows;
        }

        /// <summary>
        /// Creates a new Data Matrix from the elements of a Two-Dimensional array
        /// </summary>
        /// <param name="array">A Multi-Dimensional array of elements to fill the matrix with</param>
        public Data_Matrix(type[,] array)
        {
            if (array.Rank != 2)
            {
                throw new InvalidOperationException("Incorrect number of dimensions in array");
            }
            numRows = array.GetLength(0);
            numColumns = array.GetLength(1);
            items = array;
        }

        /// <summary>
        /// Adds a new empty row to the end of the Matrix
        /// </summary>
        public void AddRow()
        {
            //create the new items array
            numRows++;
            type[,] newItems = new type[numRows, numColumns];

            //add the current items to the array
            for (int i = 0; i < numRows - 1; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    newItems[i, j] = items[i, j];
                }
            }

            //replace the array
            items = newItems;
        }

        /// <summary>
        /// Adds a new column to the end of the Matrix
        /// </summary>
        public void AddColumn()
        {
            //create the new items array
            numColumns++;
            type[,] newItems = new type[numRows, numColumns];

            //add the current items to the array
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns - 1; j++)
                {
                    newItems[i, j] = items[i, j];
                }
            }

            //replace the array
            items = newItems;
        }

        /// <summary>
        /// Gets the item at the specified position
        /// </summary>
        /// <param name="row">The row that contains the item</param>
        /// <param name="column">The column that contains the item</param>
        /// <returns>The item at the specified postion in the Matrix</returns>
        public type GetItem(int row, int column)
        {
            return items[row, column];
        }

        /// <summary>
        /// Inserts an item into a specified position in the Matrix. Will over-write any existing item;
        /// </summary>
        /// <param name="item">The item to insert into the Matrix</param>
        /// <param name="row">The row where the item shall be inserted</param>
        /// <param name="column">The column where the item shall be inserted</param>
        public void InsertItem(type item, int row, int column){
            items[row, column] = item;
        }

        /// <summary>
        /// Gets a two-dimensional array of the Matrix' values
        /// </summary>
        /// <returns>A two-dimensional array of the Matrix' values</returns>
        public type[,] GetValues()
        {
            return items;
        }

        /// <summary>
        /// The number of rows in the Matrix
        /// </summary>
        public int NumRows
        {
            get
            {
                return numRows;
            }
        }
        
        /// <summary>
        /// The number of Columns in the Matrix
        /// </summary>
        public int NumColumns
        {
            get
            {
                return numColumns;
            }
        }

    }
}
