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
        public int X;
        public int Y;
        public bool isHorizontal;

        public Ship(int length, int x, int y, bool isHorizontal)
        {
            this.length = length;
            this.X = x;
            this.Y = y;
            this.isHorizontal = isHorizontal;
        }
    }
}
