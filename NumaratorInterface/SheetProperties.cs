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
    // PURPOSE     : this class is used in sheetSetting. it holds the properties of Sheettype like row, coll number etc.
    // ===============================   
    public class SheetProperties
    {
        public delegate void floatdelegate(float value);
        public delegate void intdelegate(int value);
        public event floatdelegate banknoteheightchanged;
        public event floatdelegate banknotewidthchanged;
        public event floatdelegate sheetheightchanged;
        public event floatdelegate sheetwidthchanged;
        public event intdelegate rownumberchanged;
        public event intdelegate collnumberchanged;
        public SheetProperties()
        {
        }
        public SerailNumberStyle serialnumberstyle;
        public int rownumber
        {
            get { return _rownumber; }
            set {
                if (value != _rownumber)
                {
                    _rownumber = value;
                    if (rownumberchanged != null)
                        rownumberchanged(value);
                }
            }
        }
        public int collnumber
        {
            get { return _collnumber; }
            set {
                if (value != _collnumber)
                {
                    _collnumber = value;
                    if (collnumberchanged != null)
                        collnumberchanged(value);
                }
            }
        }
        public bool horizantal
        {
            get { return _horizantal; }
            set { _horizantal = value; }
        }
        public bool updown
        {
            get { return _updown; }
            set { _updown = value; }
        }
        public float sheetheight
        {
            get { return _sheetheight; }
            set {
                if (value != _sheetheight)
                {
                    _sheetheight = value;
                    if (sheetheightchanged != null)
                        sheetheightchanged(value);
                }
            }
        }
        public float sheetwidth
        {
            get { return _sheetwidth; }
            set {
                if (_sheetwidth != value)
                {
                    _sheetwidth = value;
                    if (sheetwidthchanged != null)
                        sheetwidthchanged(value);
                }
            }
        }
        public float banknoteheight
        {
            get { return _banknoteheight; }
            set {
                if (value != _banknoteheight)
                {
                    _banknoteheight = value;
                    if (banknoteheightchanged != null)
                        banknoteheightchanged(value);
                }
            }
        }
        public float banknotewidth
        {
            get { return _banknotewidth; }
            set {
                if (value != _banknotewidth)
                {
                    _banknotewidth = value;
                    if (banknotewidthchanged != null)
                        banknotewidthchanged(value);
                }
            }
        }

        private int _rownumber;
        private int _collnumber;
        private float _sheetheight;
        private float _sheetwidth;
        private bool _horizantal;
        private bool _updown;
        private float _banknoteheight;
        private float _banknotewidth;
    }
}
