using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Represents a point that only takes int values
    // ===============================
    public class IntPoint
    {
        public int X;
        public int Y;
        public IntPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
