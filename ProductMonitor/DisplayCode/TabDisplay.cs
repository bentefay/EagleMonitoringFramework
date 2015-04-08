using System.Collections.Generic;
using System.Linq;
using ProductMonitor.Framework;
using ProductMonitor.Framework.Generic;

namespace ProductMonitor.DisplayCode
{
    class TabDisplay
    {
        private readonly List<string> _locations;
        private readonly List<string> _types;
        private readonly DataMatrix<ICheckDisplay> _checks;
        private readonly string _name;

        public TabDisplay(string name)
        {
            _locations = new List<string>();
            _types = new List<string>();
            _name = name;
            _checks = new DataMatrix<ICheckDisplay>();
        }

        /// <summary>
        /// Adds a check to the tab and returns true if the check causes a restructure
        /// </summary>
        /// <param name="check">The check display to add to the tab</param>
        /// <returns>True if check is new, false if replacing an existing check or filling an existing space</returns>
        public bool AddCheck(ICheckDisplay check)
        {
            bool returnValue = false;

            var indexInTypesArray = _types.IndexOf(check.GetCheckType());
            if (indexInTypesArray == -1)
            {
                indexInTypesArray = _types.Count;
                _types.Add(check.GetCheckType());
                _checks.AddRow();
                returnValue = true;
            }

            var indexInLocsArray = _locations.IndexOf(check.GetLocation());
            if (indexInLocsArray == -1)
            {
                indexInLocsArray = _locations.Count;
                _locations.Add(check.GetLocation());
                _checks.AddColumn();
                returnValue = true;              
            }
            
            _checks.InsertItem(check, indexInTypesArray, indexInLocsArray);

            LastColumn = indexInLocsArray;
            LastRow = indexInTypesArray;

            return returnValue;
        }

        public int LastRow { get; private set; }

        public int LastColumn { get; private set; }

        public IReadOnlyList<string> GetLocations()
        {
            return _locations.ToList();
        }

        public IReadOnlyList<string> GetTypes()
        {
            return _types.ToList();
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
