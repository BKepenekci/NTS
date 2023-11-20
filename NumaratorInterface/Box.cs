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
    // PURPOSE     : Holds the data (Width, Height, Ofset, IsChar) for serial Number's each char
    //               Height: Height of a char
    //               Width: Widht of a char
    //               Ofset: Distance from the next char
    //               IsChar: true if char is letter false if it is number
    // ===============================
    public class Box
    {
        public float Width;
        public float Height;
        public float Ofset;
        public bool IsChar;
        public Box()
        {
            this.Width = 17;
            this.Height = 28;
            this.Ofset = 9;
            this.IsChar = false;
        }
        public Box(float width, float height, float ofset, bool IsChar)
        {
            this.Width = width;
            this.Height = height;
            this.Ofset = ofset;
            this.IsChar = IsChar;
        }
    }
}
