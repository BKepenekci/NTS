using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DALSA.SaperaLT.SapClassBasic;

using System.Drawing;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR      : Burcu KEPENEKÇİ, 
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : ProcessControl class is derived from SapProcessing. it is used for control the process of taken image from camera 
    // ===============================
    public class ProcessControl : SapProcessing 
    {
        public System.Threading.Mutex isFrameOkMut;
        public List<bool> isFrameOkList = new List<bool>();

        System.Threading.Mutex time = new System.Threading.Mutex();
        public Mutex ResultList;
        NumaratorDllWrapper NumDll = null; //NumDll will be used in Run Method in this class
        public List<FrameResult> Results; //list of Results ()
        public long CurrentSerialStart;
        public bool running = false;
        
        public int Sensivity; //weaksensivity olarak kullanılıyor//
        public int PositionSensivity;
        public int CapakSensivity;

        //subImage için gerekli parametreler//
        int templateheight;
        int templatewidth;
        List<IntPoint> templatePoints;
        List<string> templatepaths;
        //

        //CharReadPath karakter görüntülerinin okunacağı path
        // Rects seri numaralarının bulunduğu regionlar
        //SerialPart1 Seri numarası text olarak
        //SerialPart2 Sıra numarası
        //Part2DgtCnt Sıra numarasındaki digit sayısı
        //Topleft numalar sol üstten mi başlıyor?
        //SpData Boşluk büyüklükleri
        //SzData karakter büyüklükleri
        //CharCnt Seri ve Sıra numarası toplam kaç karakter
        //rowCnt bir tabakada bulunan row sayısı
        //colCnt bir tabakada bulunan column sayısı
        public ProcessControl(SapBuffer pBuffers, SapProcessingDoneHandler pCallback, Object pContext,
            string CharReadPath,  List<Rect> Rects, int ImgW, int ImgH, string SerialPart1, long SerialPart2,int Part2DgtCnt, bool TopLeft, int TabakaCount, int[] SpData, int[] SzData, int CharCnt,int  rowCnt,int  ColCnt,int templatewidth, int templateheight, List<IntPoint> templatePoints, List<string> pathstoImages, int capaksensivity, int weakSensivity, int positionSensivity, Mutex ResultList, Mutex isFrameOkMut)
        : base(pBuffers)
     {
         this.isFrameOkMut = isFrameOkMut;
        this.ResultList = ResultList;
        this.Sensivity = weakSensivity;
        this.PositionSensivity = positionSensivity;
        this.CapakSensivity = capaksensivity;
        this.templateheight = templateheight;
        this.templatewidth = templatewidth;
        this.templatePoints = templatePoints;
        this.templatepaths = pathstoImages;
        base.ProcessingDoneEnable = true;
        base.ProcessingDone += pCallback;
        base.ProcessingDoneContext = pContext;
        NumDll=new NumaratorDllWrapper(CharReadPath,Rects, ImgW, ImgH, SerialPart1, SerialPart2,Part2DgtCnt, TopLeft, TabakaCount,SpData,  SzData,  CharCnt,  rowCnt,  ColCnt);
            
        base.Buffer = pBuffers;
        Results = new List<FrameResult>();
        CurrentSerialStart = NumDll._serialNum;
        string tmp = NumDll.GetSerialString(NumDll._serialStr, NumDll._serialNum, 0, 0, NumDll._numDgtCnt, NumDll._tabakaCnt, NumDll._rowCnt, NumDll._colCnt);
        NumDll.CheckSerialNumber(0,0,IntPtr.Zero, 0, 0, 0,Sensivity,PositionSensivity,0,0,false);
        NumDll.LoadTemplate(templatepaths);    
     }
       
        static byte[] PadLines(byte[] bytes, int rows, int columns, int s)
        {
            var currentStride = columns; // 3
            var newStride = s;  // 4
            var newBytes = new byte[newStride * rows];
            for (var i = 0; i < rows; i++)
                System.Buffer.BlockCopy(bytes, currentStride * i, newBytes, newStride * i, currentStride);
            return newBytes;
        }

        //overriden method//
        //it is called when ExecuteNext() Method is called. (look at xfer_XferNotify() method in OperatorSettingsControl.xaml.cs)

        public override bool Run()
        {
            isFrameOkMut.WaitOne();
            bool isOK = isFrameOkList[0]; // take result
            isFrameOkList.RemoveAt(0);//
            isFrameOkMut.ReleaseMutex();
            if (!isOK)
            {
                FrameResult Result = new FrameResult(false); //last 0, 0 maybe changed but not realy important
                ResultList.WaitOne();
                Results.Add(Result);
                ResultList.ReleaseMutex();
                return false;
            }


            if (NumDll == null) return false;
            running = true;
            NumDll._serialNum=CurrentSerialStart;
            int proIndex = base.Index;//base.Buffer.Index;
         
            //bool goodContent = (Buffer.get_State(proIndex) == SapBuffer.DataState.Full);
                        
            int inSize = Buffer.get_SpaceUsed(proIndex);
          
            SapFormat sapFormat = Buffer.Format;
            var watch = Stopwatch.StartNew();
         
            int h = base.Buffer.Height;
            int w = base.Buffer.Width;
            List<CheckResultPair> result = new List<CheckResultPair>();

          
            IntPtr iptr = Marshal.AllocHGlobal(base.Buffer.Width * base.Buffer.Height * base.Buffer.BytesPerPixel);
            int buffersize = base.Buffer.get_SpaceUsed(proIndex);
            base.Buffer.Read(proIndex, 0, base.Buffer.Width * base.Buffer.Height, iptr);
            //int isOk=NumDll.IsFrameValid(iptr, base.Buffer.Width, base.Buffer.Height);
            try
            {
                if (sapFormat == SapFormat.Mono8)
                {
                    int row = 0;
                    int isRed = 0;
                    int index = 0;
                    int rowtemp = 0;
                    System.Windows.Point offsetp = NumDll.FindImageOffset(iptr, this.NumDll.tmplp[0], base.Buffer.Width, base.Buffer.Height, templatePoints[0].X, templatePoints[0].Y);

                    int falseCount = 0;
                    foreach (LocSerialPair p in NumDll.RectLocations)
                    {

                        if (index >= 2 * NumDll._colCnt)
                        {
                            index = 0;
                            rowtemp++;
                        }
                        if (index == 0)
                        {
                            offsetp = NumDll.FindImageOffset(iptr, this.NumDll.tmplp[p.row], base.Buffer.Width, base.Buffer.Height, templatePoints[rowtemp].X, templatePoints[rowtemp].Y);
                        }
                        index++;
                        IntPtr dptr = Marshal.AllocHGlobal(p.width * p.height * base.Buffer.BytesPerPixel);
                        base.Buffer.ReadRect(proIndex, (int)(p.x - offsetp.X), (int)(p.y - offsetp.Y), p.width, p.height, dptr);


                        string tmp = NumDll.GetSerialString(NumDll._serialStr, NumDll._serialNum, p.row, p.col, NumDll._numDgtCnt, NumDll._tabakaCnt, NumDll._rowCnt, NumDll._colCnt);
                       //if(p.row==4 && p.col==2 && isRed==0)
                       //    NumDll.SaveInput(dptr, p.width, p.height, "C:\\Users\\User\\Documents\\samples\\" + tmp + p.index.ToString() + ".png");
                        result.Add(NumDll.CheckSerialNumber(p.row, p.col, dptr, p.width, p.height, p.index, Sensivity, PositionSensivity, isRed, CapakSensivity, falseCount < 8));
                        //if(p.row==0 && p.col==0 && result.Last().data==-2)
                        //    result.Last().data=1;
                        if (result.Last().data != 1)
                            falseCount++;
                        result[result.Count - 1].offsetX = Convert.ToInt32(offsetp.X);
                        result[result.Count - 1].offsetY = Convert.ToInt32(offsetp.Y);
                        if (isRed == 0) isRed = 1;
                        else isRed = 0;
                        Marshal.FreeHGlobal(dptr);

                    }
                }
           
            CurrentSerialStart = NumDll.UpdateSerialStart();



            FrameResult FResult = new FrameResult(result, proIndex, iptr, buffersize, 0, 0); //last 0, 0 maybe changed but not realy important
            ResultList.WaitOne();
            Results.Add(FResult);
            ResultList.ReleaseMutex();
         
            running = false;
            watch.Stop();
            var elapsedtime = watch.ElapsedMilliseconds;
         

            time.WaitOne();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\User\Documents\Visual Studio 2013\Projects\CombinedNumarator\process.txt", true))
            {
                file.WriteLine("Process Control:" + Convert.ToString(elapsedtime), true);
                file.Close();
            }

            time.ReleaseMutex();
            }
            catch
            {
                MessageBox.Show("212");
            }
            return true;
        }
    }
}
