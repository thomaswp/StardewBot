// This module provides info about a tile in a location.
// Gathering that is a real PITA that has to draw from lots of different sources,
// so it gets its own file.
//
// It also contains some related methods to get info about objects and items,
// which you may well find on a tile.

using System.Collections.Generic;

namespace Farmtronics
{
    public class ValString
    {
        public readonly string value;
        public ValString(string value)
        {
            this.value = value;
        }
    }

    public class ValNumber
    {
        public readonly float value;
        public ValNumber(float value)
        {
            this.value = value;
        }
    }

    public class ValMap
    {
        public readonly Dictionary<object, object> map = new Dictionary<object, object>();
        public ValMap()
        {
        }
    }
}