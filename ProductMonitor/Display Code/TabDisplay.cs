using Product_Monitor.Generic;

namespace ProductMonitor.Display_Code
{
    class TabDisplay
    {
        private string[] _locations;
        private string[] _types;
        private readonly Data_Matrix<ICheckDisplay> _checks;
        private readonly string _name;

        private int _lastRow;
        private int _lastColumn;

        public TabDisplay(string name)
        {
            _locations = new string[0];
            _types = new string[0];
            _name = name;
            _checks = new Data_Matrix<ICheckDisplay>();
        }

        /// <summary>
        /// Adds a check to the tab and returns true if the check causes a restructure
        /// </summary>
        /// <param name="check">The check display to add to the tab</param>
        /// <returns>True if check is new, false if replacing an existing check or filling an existing space</returns>
        public bool AddCheck(ICheckDisplay check)
        {
            bool returnValue = false;

            // add the type to the array

            // Finds the index where the type is found in the array (-1 for not found)
            int indexInTypesArray = -1;
            for (int i = 0; i < _types.Length; i++)
            {
                if (_types[i].Equals(check.GetCheckType()))
                {
                    indexInTypesArray = i;
                    break;
                }
            } 
            if (indexInTypesArray == -1)
            {
                //the type hasn't happened yet, add it
                var newTypes = new string[Utilities<string>.RemoveNulls(_types).Length + 1];
                Utilities<string>.RemoveNulls(_types).CopyTo(newTypes, 0);
                newTypes[newTypes.Length - 1] = check.GetCheckType();
                _types = newTypes;
                _checks.AddRow();
                returnValue = true; //a new cell will be added
                indexInTypesArray = newTypes.Length - 1;
            }

            // add the location to the array

            // Finds the index where the location is found in the array (-1 for not found)
            int indexInLocsArray = -1;
            for (int i = 0; i < _locations.Length; i++)
            {
                if (_locations[i].Equals(check.GetLocation()))
                {
                    indexInLocsArray = i;
                    break;
                }
            }
            if (indexInLocsArray == -1)
            {
                //the location is new, add it
                string[] newLocs = new string[Utilities<string>.RemoveNulls(_locations).Length + 1];
                Utilities<string>.RemoveNulls(_locations).CopyTo(newLocs, 0);
                newLocs[newLocs.Length - 1] = check.GetLocation();
                _locations = newLocs;
                _checks.AddColumn();
                returnValue = true; //a new cell will be added
                indexInLocsArray = newLocs.Length - 1;
            }
            
            _checks.InsertItem(check, indexInTypesArray, indexInLocsArray);

            _lastColumn = indexInLocsArray;
            _lastRow = indexInTypesArray;

            return returnValue;
        }

        public int LastRow
        {
            get { return _lastRow; }
        }

        public int LastColumn
        {
            get { return _lastColumn; }
        }

        public string[] GetLocations()
        {
            return _locations;
        }

        public string[] GetTypes()
        {
            return _types;
        }

        public string GetName()
        {
            return _name;
        }

        public ICheckDisplay[,] GetTable()
        {
            return _checks.GetValues();
        }
    }
}
