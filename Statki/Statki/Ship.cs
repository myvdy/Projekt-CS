using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki
{
    internal class Ship
    {
        public int length;
        public int row;
        public int column;
        public int startingRow;
        public int startingColumn;
        public bool isHorizontal;

        public Ship(int length = 1, int row = 50, int column = 0, int startingRow = 0, int startingColumn = 0, bool isHorizontal = true)
        {
            this.length = length;
            this.column = column;
            this.row = row;
            this.startingRow = startingRow;
            this.startingColumn = startingColumn;
            this.isHorizontal = isHorizontal;
        }
    }
}
