using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR      : Burcu KEPENEKÇİ
    // UPDATE D ATE     : 20.08.2016
    // PURPOSE     : Wrapper Class for the codes written in C++ 
    // ===============================
    public class LocSerialPair
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public int index;
        public int row;
        public int col;
    
        public LocSerialPair(int X, int Y,int Width,int Height, int ind,int Row,int Col)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
            index = ind;
            row = Row;
            col = Col;
        }
    };

    public class CheckResultPair
    {
        public int data;
        public int indexX;
        public int indexY;
        public int index;
        public string SheetSerial;
        public int offsetX;
        public int offsetY; 
        public CheckResultPair(int result, int indX,int indY,int ind,string serial)
        {
            data = result;
            indexX = indX;
            indexY = indY;
            index=ind;
            SheetSerial = serial;
        }
    };
    public class FrameResult
    {
        public int FrameNo;
        public bool isOK;
        public List<CheckResultPair> result;
        public IntPtr imageBuff;
        public int buffersize;
        public int xofset;
        public int yofset;
        public FrameResult(List<CheckResultPair> res, int ind,IntPtr buff, int buffersize,int x,int y)
        {
            result = res;          
            FrameNo = ind;
            imageBuff=buff;
            this.buffersize = buffersize;
            xofset = x;
            yofset = y;
            isOK = true;
        }
        public FrameResult(bool isOK)
        {
            this.isOK = isOK;
        }
    }

    public class NumaratorDllWrapper
    {
        string dirName = "";

        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Debug\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void FindOffset(IntPtr pData, int width, int height, IntPtr pTemplData, int twidth, int theight, int templpX, int templpY, IntPtr xoffset, IntPtr yoffset);
        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Debug\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int  CheckNumber(string CharPath, string Serial, long StartNbr, int TabakaCount, int RowIndex, int ColIndex, int SerialNumDgtCnt, IntPtr pData, int width, int height, int[] SpaceData, int[] SizeData, int CharCnt, int rowCnt, int ColCnt, int Sensivity, int KonumSensivity, int isRed, int CapakSensivity);
        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Debug\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern long NextSerialNumStart(long Start, int TabakaCnt, bool desc);

        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Debug\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int SaveImage(IntPtr pData, int width, int height, string fname);
        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Debug\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern string GetSerial(string SerialStr, long SerialNum, int rowIndex, int colIndex, int SerialNumDgtCnt, int tabakaCnt, int rowCnt, int colCnt);


        [DllImport("NumDLL\\NumaratorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FindOffset(IntPtr pData, int width, int height, IntPtr pTemplData, int twidth, int theight, int templpX, int templpY, IntPtr xoffset, IntPtr yoffset);
        //[DllImport("C:\\Users\\User\\Documents\\Visual Studio 2013\\Projects\\CombinedNumarator\\NumaratorDll\\x64\\Release\\NumaratorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int isFrameValid(IntPtr pData, int width, int height);
        [DllImport("NumDLL\\NumaratorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CheckNumber(string CharPath, string Serial, long StartNbr, int TabakaCount, int RowIndex, int ColIndex, int SerialNumDgtCnt, IntPtr pData, int width, int height, int[] SpaceData, int[] SizeData, int CharCnt, int rowCnt, int ColCnt, int Sensivity, int KonumSensivity, int isRed, int CapakSensivity,bool DoRepeat);
        [DllImport("NumDLL\\NumaratorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long NextSerialNumStart(long Start, int TabakaCnt, bool desc);

        [DllImport("NumDLL\\NumaratorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SaveImage(IntPtr pData, int width, int height, string fname);
        [DllImport("NumDLL\\NumaratorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetSerial(string SerialStr, long SerialNum, int rowIndex, int colIndex, int SerialNumDgtCnt, int tabakaCnt, int rowCnt, int colCnt);

              
        int _rectHeight;
        int _rectWidth;
        List<LocSerialPair> _rectLocations;
        int _imgHeight;
        int _imgWidth;

        public long _serialNum;
        long _serialStart;
        public string _serialStr;
        public int _numDgtCnt;
        //public int _rowsTabaka;
        //public int _colsTabaka;
        public int _tabakaCnt;
        public int _rowCnt;
        public int _colCnt;

        int [] _spData;
        int [] _szData;
        int _charCnt;
        string _charReadPath;

        public List<IntPtr> tmplp;
        int tw, th;
        
        //public int ImgHeight { get; set; }
        //public int ImgWidth { get; set; }
        //public int RectWidth { get; set; }
        //public int RectHeight { get; set; }
        public List<LocSerialPair> RectLocations { get {return _rectLocations;} }
        /// <summary>
        /// about serial compute
        /// </summary>        
        //public long SerialNum { get; set; }
        //public string SerialStr { get; set; }
        //public int TabakaToPrint { get; set; }
        //public int RowsTabaka { get; set; }
        //public int ColsTabaka { get; set; }
        ///// <summary>
        ///// about serial template
        ///// </summary>
        //public int[] SpaceData { get; set; }
        //public int[] SizeData { get; set; }
        //public string CharReadPath { get; set; }
       
        public string GetSerialString(long serial)
        {
            string result = _serialStr + serial.ToString();
          //  char[] result= GetSerial(_serialStr, serial, 0, 0, _numDgtCnt, _tabakaCnt, _rowCnt, _colCnt);;
            return result;// GetSerial(_serialStr, serial, 0, 0, _numDgtCnt, _tabakaCnt, _rowCnt, _colCnt);
        }

        public NumaratorDllWrapper()
        {
        }
        public NumaratorDllWrapper(string CharReadPath, List<Rect> Rects, int ImgW, int ImgH, string SerialPart1, long SerialPart2, int Part2DgtCnt, bool TopLeft, int TabakaCount, int[] SpData, int[] SzData, int CharCnt, int rowCnt, int ColCnt)
        {
            _charReadPath = CharReadPath;
          //  _rectHeight = RectHeight;
          //  _rectWidth = RectWidth;
            _rowCnt = rowCnt;
            _colCnt = ColCnt;
            _serialStart = SerialPart2;

            _rectLocations = new List<LocSerialPair>();
            int row = 0; int col = 0;
            if (Rects != null)
            {
                //Rects = Rects.OrderBy(x => x.X).ThenBy(x => x.Y).ToList();
                for (int i = 0; i < Rects.Count-1; i+=2)
                {
                    _rectLocations.Add(new LocSerialPair((int)Rects[i].X, (int)Rects[i].Y, (int)Rects[i].Width, (int)Rects[i].Height, i, row, col));
                    _rectLocations.Add(new LocSerialPair((int)Rects[i+1].X, (int)Rects[i+1].Y, (int)Rects[i+1].Width, (int)Rects[i+1].Height, i+1, row, col));
                    col++;
                    if(col==ColCnt)
                    { col = 0; row++; }
                }
            }
            _imgHeight = ImgH;
            _imgWidth = ImgW;
            _numDgtCnt = Part2DgtCnt;

            _spData = SpData;
            _szData = SzData;
            _charCnt = CharCnt;
            _serialStr = SerialPart1;
            _tabakaCnt = TabakaCount;
            if (!TopLeft)
            {
                _serialStart = ComputeTopLeft(SerialPart2, _tabakaCnt, _rowCnt, _colCnt, _numDgtCnt);
            }
            _serialNum = _serialStart;
            //else _serialNum = SerialPart2;

           /* if (_charReadPath != string.Empty)
                GetCharData(_charReadPath);*/
        }
        public void SaveInput(IntPtr pData, int w, int h, string fname)
        {
            // string fname="C:\\Users\\User\\Documents\\sample\\Frame_"+index.ToString()+".png";
            SaveImage(pData, w, h, fname);
        }
        public void LoadTemplate(List<string> paths)
        {
            tmplp = new List<IntPtr>();
            // string fname="C:\\Users\\User\\Documents\\sample\\Frame_"+index.ToString()+".png";
            int i = 0;
            foreach (string path in paths)
            {
                System.IO.DirectoryInfo Dinfo = new System.IO.DirectoryInfo("subTemplateDirectory");

                System.Drawing.Bitmap BM = new System.Drawing.Bitmap(Dinfo.FullName + "\\" + path);
                PngBitmapDecoder dec = new PngBitmapDecoder(new Uri(Dinfo.FullName+"\\"+ path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource bs = dec.Frames[0];

                int lenght = BM.Width * BM.Height;
                tmplp.Add(Marshal.AllocHGlobal(lenght));
                bs.CopyPixels(new Int32Rect(0, 0, BM.Width, BM.Height), tmplp[i], lenght, BM.Width);
                tw = BM.Width;
                th = BM.Height;
                ++i;
            }
        }
        public Point FindImageOffset(IntPtr pData, IntPtr templateImage, int w, int h, int templpX, int templpY)
        {
            //IntPtr x=new IntPtr(0),y=new IntPtr(0);
            IntPtr x = Marshal.AllocHGlobal(1);
            IntPtr y = Marshal.AllocHGlobal(1);
            FindOffset(pData, w, h, templateImage, tw, th, templpX, templpY, x, y);
            int xi = Marshal.ReadInt32(x);
            int yi = Marshal.ReadInt32(y);
            return new Point((double)xi, (double)yi);
        }
        //public int IsFrameValid(IntPtr pData, int w, int h)
        //{
        //    int result = isFrameValid(pData, w, h);
        //    return result;
        //}
        //public bool InitializeParameters(string CharReadPath, List<Rect> Rects, int ImgW, int ImgH, string SerialPart1, long SerialPart2,int Part2DgtCnt, bool TopLeft, int TabakaCount, int[] SpData, int[] SzData, int CharCnt,int RowCnt,int ColCnt)
        //{
        //    _charReadPath = CharReadPath;
        //    _serialStart = SerialPart2;
        //    _rectLocations = new List<LocSerialPair>();
        //    int row = 0; int col = 0;
        //    if (Rects != null)
        //    {
        //        Rects = Rects.OrderBy(x => x.X).ThenBy(x => x.Y).ToList();
        //        for (int i = 0; i < Rects.Count; i++)
        //        {
        //            _rectLocations.Add(new LocSerialPair((int)Rects[i].X, (int)Rects[i].Y, (int)Rects[i].Width, (int)Rects[i].Height, i, row, col));
        //            col++;
        //            if (col == ColCnt)
        //            { col = 0; row++; }
        //        }
        //    }
        //    _imgHeight = ImgH;
        //    _imgWidth = ImgW;
        //    _numDgtCnt = Part2DgtCnt;
        //    _spData = SpData;
        //    _szData = SzData;
        //    _charCnt = CharCnt;
        //    _serialStr = SerialPart1;
        //    _tabakaCnt = TabakaCount;
        //    _rowCnt = RowCnt;
        //    _colCnt = ColCnt;

        // /*   if (!TopLeft)
        //    {
        //        _serialNum = ComputeTopLeft(SerialPart2, _tabakaCnt);
        //    }
        //    else _serialNum = SerialPart2;

        //    if (_charReadPath != string.Empty)
        //        GetCharData(_charReadPath);*/
        //    return true;
        //}
        public CheckResultPair CheckSerialNumber(int RowIndex, int ColIndex, IntPtr pData, int width, int height, int index, int Sensivity, int KonumSensivity,int isRed,int CapakSensivity, bool DoRepeat)
        {

             string serial="" ;//= new string(str);
             int res = CheckNumber(_charReadPath, _serialStr, _serialNum, _tabakaCnt, RowIndex, ColIndex, _numDgtCnt, pData, width, height, _spData, _szData, _charCnt, _rowCnt, _colCnt, Sensivity, KonumSensivity, isRed, CapakSensivity,DoRepeat);
            serial = GetSerialString(_serialStr, _serialNum, RowIndex, ColIndex, _numDgtCnt, _tabakaCnt, _rowCnt, _colCnt);
            CheckResultPair sonuc = new CheckResultPair(res, RowIndex, ColIndex, index,serial);
            return sonuc;
            //return 1;// CheckNumberWS(_serialStart, index, pData, _rectWidth, _rectHeight, _spData, _szData, _charCnt);
        }     
        public long UpdateSerialStart()
        {
            //_serialNum = NextSerialStart(_serialStart, _rectLocations.Count, _tabakaCnt);
            return NextSerialNumStart(_serialNum, _numDgtCnt, true);
        }

        //Gets the serial number of banknot given the row, coll and setup number
        public string GetSerialString(string SerialStr, long SerialNum, int rowIndex, int colIndex, int SerialNumDgtCnt, int tabakaCnt, int rowCnt, int colCnt)
        {

            //char [] outStr = new char[SerialStr.Length + SerialNumDgtCnt];
            //char [] fmt = new char[100];
            double StartNum = SerialNum;
            StartNum += Math.Pow(10, SerialNumDgtCnt);

            double serialnum = StartNum - (rowCnt * tabakaCnt) * colIndex - (rowIndex * tabakaCnt);
            if (serialnum >= Math.Pow(10, SerialNumDgtCnt))
                serialnum -= Math.Pow(10, SerialNumDgtCnt);
            string s1 = serialnum.ToString();
            s1 = s1.PadLeft(SerialNumDgtCnt, '0');
            return SerialStr + s1;
            //return GetSerial(SerialStr, SerialNum, rowIndex, colIndex, SerialNumDgtCnt, tabakaCnt, rowCnt, colCnt);
        }
        public long ComputeTopLeft(long SerialNum, int tabakaCnt, int rowCnt, int colCnt, int SerialNumDgtCnt)
        {
            //char [] outStr = new char[SerialStr.Length + SerialNumDgtCnt];
            //char [] fmt = new char[100];
            long StartNum = SerialNum;
            StartNum += (long)Math.Pow(10, SerialNumDgtCnt);

            long serialnum = StartNum + (rowCnt * tabakaCnt) * (colCnt - 1) + ((rowCnt - 1) * tabakaCnt);
            if (serialnum >=(long) Math.Pow(10, SerialNumDgtCnt))
                serialnum -=(long) Math.Pow(10, SerialNumDgtCnt);

            return serialnum;
            //return GetSerial(SerialStr, SerialNum, rowIndex, colIndex, SerialNumDgtCnt, tabakaCnt, rowCnt, colCnt);
        }
    }
}
