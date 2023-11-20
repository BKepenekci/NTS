using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : this class is used in sheetSetting. it holds the positions of serial numbers, witdh and height.
    // ===============================       
    public class SerialNumberPositions
    {
        public SerialNumberPositions()
        {
            positions = new List<IntPoint>();
        }
        public int boxwidth
        {
            get { return _boxwidth; }
            set { _boxwidth = value; }
        }
        public int boxheight
        {
            get { return _boxheight; }
            set { _boxheight = value; }
        }
        public List<IntPoint> positions; 
        private int _boxwidth;
        private int _boxheight;
    }
}
