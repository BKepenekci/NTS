using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DALSA.SaperaLT.SapClassBasic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Shapes;
using NationalInstruments.DAQmx;

namespace NumaratorInterface.Controls.OperatorController
{
    // ===============================
    //  AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATxE     : 20.08.2016
    // PURPOSE     : Heart of the Operator Window. 
    // it holds the inital parameters (Başlangıç Seri No, Sıra No, Tabaka Ayarı, etc)
    // Connects to cam starts the inspection, Sends the collected image if GoodSheet Signal is true.
    // gets the results updates the GUI
    // ===============================
    public partial class OperatorSettingsControl : UserControl
    {
        public bool isLoaded = false;

        //
        int FalseRejectLimit = 5;
      //  int FalseCounter = 0;
        int interval;
        //
        public string ParameterFileName=string.Empty;
        public System.Threading.Mutex isFrameOkMut = new Mutex();
        //For NI 
        //digitalWriteTask is used for changing the output like sending warning or stop alarm
        private NationalInstruments.DAQmx.Task digitalWriteTask; 
        private DigitalSingleChannelWriter writer;
        bool[] dataArrayPort0 = new bool[8]; //initial values are zero
        bool[] dataArrayPort1 = new bool[8];
        bool[] dataArrayPort2 = new bool[8];
        //
        List<FrameResult> FalseList = new List<FrameResult>();
        List<FrameResult> WarningList = new List<FrameResult>();
        

      


        //
        private NationalInstruments.DAQmx.Task myTask;
        private DigitalSingleChannelReader myDigitalReader;

        //For ImageViewer
        public PVImageBox ImgBox = null;

        //Mutex for result list
        Mutex ResultList = new Mutex();

        //Mutex for Database Threads
        Mutex Num = new Mutex();
        public int NumCount = 0; 
        //

        //Mutex for ImageSaveThreads
        Mutex Img = new Mutex();
        public int ImgCount = 0;
        //
        public Mutex IOMut; //Mutex for Signal Write
        double pixellpermm = 7.4;

        

        //Number of pressed Sheets and FalsePressed Sheets
        int PressedNum = 0;
        int FalsePressedNum = 0;
        //

        //input parameters
        int sheetcount;
        int serialcharnumber;
        int sequencecharnumber;
        public bool isrunning = false;
        bool istopleft;
        int SheetToBePressedNum;
        

        //Object instances for camera use
        SapAcquisition Acq = null;
        public SapBuffer Buffers = null;
        public SapTransfer Xfer = null;
        public SapView View = null;

        //Referances of other controls //just referance not created in this class // these referances are assigned in ControlOperator
        public ProcessControl m_Pro;
        public FalseSheetController FSController;
        public SheetSettings sheetsetting;
        public ImageViewerControl ViewControl;
        public ReactionController reactionController;

        //For Blinking the green light timers are used//
        System.Timers.Timer timerWarningKorna = new System.Timers.Timer();
        System.Timers.Timer timer2 = new System.Timers.Timer();
        System.Timers.Timer timer3 = new System.Timers.Timer();//end of operation signal
        //Flag for generating signalwave

        //
        public Mutex ListFalseMut = new Mutex();//Mutex For temp FalseList
        public Mutex ListWarningMut = new Mutex();//Mutex For temp WarningList


        //For FalseReject Counter timer//
        System.Timers.Timer FalseRejectTimer = new System.Timers.Timer();
        System.Timers.Timer WarningTimer = new System.Timers.Timer();


        //SNAP TIMER
        //System.Timers.Timer SnapTimer = new System.Timers.Timer();
        //bool indicator = false;

        
        public void WriteIO(bool data, int portNo, int lineNo)
        {
            if (IOMut != null)
            {
                IOMut.WaitOne();
                using (digitalWriteTask = new NationalInstruments.DAQmx.Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("Dev2/Port" + portNo.ToString() + "/line" + lineNo.ToString(), "",
                           ChannelLineGrouping.OneChannelForAllLines);
                    //dataArray[LineNo] = data;               
                    writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleSingleLine(true, data);
                    digitalWriteTask.Dispose();
                }
                IOMut.ReleaseMutex();
            }
        }
        //public void GenerateWave(int LineNo,int portNo,int samples=3000)
        //{
        //    //if (OnAlarm) return;
        //    //OnAlarm = true;
        //    using (digitalWriteTask = new NationalInstruments.DAQmx.Task())
        //    {
        //        // Create the digital output channel
        //        string s = "Dev1/Port"+portNo.ToString()+"/line" + LineNo.ToString();
        //        digitalWriteTask.DOChannels.CreateChannel(s, "",
        //            ChannelLineGrouping.OneChannelForAllLines);

        //        // Verify the task so we can query the channel's properties
        //        digitalWriteTask.Control(TaskAction.Verify);

        //        // Create the data to write
        //      //  int samples = (int)3000;
        //        int signals = (int)digitalWriteTask.DOChannels[0].NumberOfLines;

        //        // Set up the timing
        //        digitalWriteTask.Timing.ConfigureSampleClock
        //        (null,
        //            Convert.ToDouble(1000),
        //            SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples,
        //            Convert.ToInt32(samples));

        //        // Write the data
        //        DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);

        //        // Loop through every sample
        //        NationalInstruments.DigitalWaveform waveform = new NationalInstruments.DigitalWaveform(Convert.ToInt32(samples),
        //                                                        Convert.ToInt32(digitalWriteTask.DOChannels[0].NumberOfLines));
        //        Random r = new Random();

            
        //        for (int i = 0; i < samples -3; i++)
        //        {
        //            waveform.Samples[i].States[0] = NationalInstruments.DigitalState.ForceUp;

        //        }
        //        for (int i = samples / 2; i < samples; i++)
        //        {
        //            waveform.Samples[i].States[0] = NationalInstruments.DigitalState.ForceDown;
        //        }

        //        // Write those values
        //        writer.WriteWaveform(true, waveform);
        //        digitalWriteTask.WaitUntilDone();
        //        digitalWriteTask.Dispose();
             
        //    }
        //    //OnAlarm = false;
        //}

        //Signal Read
        //private NationalInstruments.DAQmx.Task FrameOkTask;
        //private NationalInstruments.DAQmx.DigitalSingleChannelReader DigitalReaderFrameOk;
        //private bool IsFrameOk = false;
        //private void StartToDEtecFrameOk()
        //{
        //    try
        //    {
        //        // Create the task.
        //        FrameOkTask = new NationalInstruments.DAQmx.Task();

        //        // Create channel
        //        FrameOkTask.DIChannels.CreateChannel("Dev1/port0/line1",
        //            "",
        //            NationalInstruments.DAQmx.ChannelLineGrouping.OneChannelForAllLines);

        //        // Configure digital change detection timing
        //        FrameOkTask.Timing.ConfigureChangeDetection(
        //            "Dev1/port0/line1",
        //            "",
        //            NationalInstruments.DAQmx.SampleQuantityMode.ContinuousSamples, 200);

        //        // Add the digital change detection event handler
        //        // Use SynchronizeCallbacks to specify that the object 
        //        // marshals callbacks across threads appropriately.
        //        FrameOkTask.SynchronizeCallbacks = true;

        //        FrameOkTask.DigitalChangeDetection += new NationalInstruments.DAQmx.DigitalChangeDetectionEventHandler(ShutDownEventHandler);

        //        // Create the reader
        //        DigitalReaderFrameOk = new NationalInstruments.DAQmx.DigitalSingleChannelReader(FrameOkTask.Stream);

        //        // Start the task
        //        FrameOkTask.Start();
        //    }
        //    catch (NationalInstruments.DAQmx.DaqException exception)
        //    {
        //        FrameOkTask.Dispose();

        //        MessageBox.Show(exception.Message);
        //    }
        //}
        //private void ShutDownEventHandler(object sender, NationalInstruments.DAQmx.DigitalChangeDetectionEventArgs e)
        //{
        //    IsFrameOk = !IsFrameOk;
        //}
        public bool StartReadingIO()
        {
            try
            {
                //Create a task such that it will be disposed after
                //we are done using it.
                myTask = new NationalInstruments.DAQmx.Task();

                //Create channel
                myTask.DIChannels.CreateChannel(
                    "Dev2/port0/line1",
                    "myChannel",
                    ChannelLineGrouping.OneChannelForAllLines);

                myDigitalReader = new DigitalSingleChannelReader(myTask.Stream);
         
            }
            catch (DaqException exception)
            {
                
                MessageBox.Show(exception.Message);

                //dispose task
                myTask.Dispose();
                return false;
            }
            return true;
        }

        //reads the "GoodSheet" Signal
        Mutex ReadTask = new Mutex();
        public bool isFrameOk()
        {
            try
            {
                bool[] readData;
                readData = myDigitalReader.ReadSingleSampleMultiLine();
                return readData[0];

            }
            catch //(DaqException exception)
            {
                //myTask.Dispose();
                //MessageBox.Show("hata");
                // MessageBox.Show(exception.Message);
                return false;
            }
          
        }

        //Constructor for UserControl. 
        //Note: Referance of Other "UserControl" (FalseSheetController, ReactionConttoller, ImageViewer) 's is not created, 
        public OperatorSettingsControl()
        {
            InitializeComponent();
           StartReadingIO();
             isFrameOk();
           // StartToDEtecFrameOk();


            for (int i = 0; i < 8; i++)
            {
                if (i != 6)
                {
                    dataArrayPort0[i] = false;
                    WriteIO(false, 0, i);
                }
                dataArrayPort1[i] = false;
                dataArrayPort2[i] = false;
                WriteIO(false, 1, i);
                WriteIO(false, 2, i);
            }
            
            dataArrayPort0[2] = true;
          //  dataArrayPort2[2] = true;

            WriteIO(dataArrayPort0[2], 0,2);
            WriteIO(dataArrayPort2[2], 2,2);
            

            //timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerTick);
            //timer.Enabled = true;
            //timer.Interval = 500;
            //SnapTimer.Elapsed+=SnapTimer_Elapsed;
            //SnapTimer.Enabled = true;
         
            FalseRejectTimer.Interval = interval * 1000;
            FalseRejectTimer.Elapsed += new System.Timers.ElapsedEventHandler(FalseRejectTimerHandler);
            WarningTimer.Interval = interval * 1000;
            WarningTimer.Elapsed += new System.Timers.ElapsedEventHandler(WarningTimerHandler);
            timer2.Elapsed += new System.Timers.ElapsedEventHandler(TimerTick2);
            timer2.Enabled = false;
            timer2.Interval = 1000;
            timerWarningKorna.Elapsed += new System.Timers.ElapsedEventHandler(TimerTickWarningKorna);
            timerWarningKorna.Enabled = false;
            timerWarningKorna.Interval = 1000;
            timer3.Elapsed += new System.Timers.ElapsedEventHandler(TimerTick3);
            timer3.Enabled = false;
            timer3.Interval = 200;


            FalseRejectLimit = Convert.ToInt32(FalseRejeckSlider.Value);
            FillComboBox();
            this.isrunning = false;
            this.istopleft = true;            
            ThreadPool.SetMaxThreads(5,5);

          

                            
                      

        }

       /* private void SnapTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                
                Xfer.Snap(1);
                indicator = true;
                SnapTimer.Enabled = false;
            }
            catch {
                indicator = false;
            }
        }*/

        //implementation of FalseRejectTimer
        void FalseRejectTimerHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
        //    if (FalseCounter > 0)
        //    {
        //        dataArrayPort0[5] = false;
        //        dataArrayPort1[3] = false;
        //        dataArrayPort1[2] = false;
        //        dataArrayPort2[2] = true;
        //        WriteIO(dataArrayPort0[5], 0, 5);
        //        WriteIO(dataArrayPort1[3], 1, 3);
        //        WriteIO(dataArrayPort1[2], 1, 2);
        //        WriteIO(dataArrayPort2[2], 2, 2);

        //    }
            //FalseRejectTimer.Enabled = false;
                ListFalseMut.WaitOne();
                int count = FalseList.Count;
                for (int i = 0; i < count; i++)
                {
                    Marshal.FreeHGlobal(FalseList[i].imageBuff);
                }
                FalseList.Clear();
                ListFalseMut.ReleaseMutex();
           // FalseCounter = 0;

        }
        //implementation of FalseRejectTimer
        void WarningTimerHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            //if (FalseCounter > 0)
            //{
            //    dataArrayPort0[5] = false;
            //    dataArrayPort1[3] = false;
            //    dataArrayPort1[2] = false;
            //    dataArrayPort2[2] = true;
            //    WriteIO(dataArrayPort0[5], 0, 5);
            //    WriteIO(dataArrayPort1[3], 1, 3);
            //    WriteIO(dataArrayPort1[2], 1, 2);
            //    WriteIO(dataArrayPort2[2], 2, 2);

            //}
            //FalseRejectTimer.Enabled = false;
                ListWarningMut.WaitOne();
                int count = WarningList.Count;
                for (int i = 0; i < count; i++)
                {
                    Marshal.FreeHGlobal(WarningList[i].imageBuff);
                }
                WarningList.Clear();
                ListWarningMut.ReleaseMutex();
        }
       
        //void TimerTick(object sender, System.Timers.ElapsedEventArgs args)
        //{
        //    dataArrayPort2[2] = !dataArrayPort2[2];
        //    WriteIO(dataArrayPort2[2], 2,2);
            
        //}


        void TimerTick2(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (dataArrayPort2[3] == true) timer2.Enabled = false;
            dataArrayPort2[3] = !dataArrayPort2[3];
            WriteIO(dataArrayPort2[3], 2, 3);

        }
        void TimerTickWarningKorna(object sender, System.Timers.ElapsedEventArgs args)
        {
            //if (dataArrayPort2[3] == true) timer2.Enabled = false;
        //    dataArrayPort2[3] = !dataArrayPort2[3];
            WriteIO(dataArrayPort2[3], 2, 3);
        }

        void TimerTick3(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (dataArrayPort1[4] == true) timer3.Enabled = false;
            dataArrayPort1[4] = !dataArrayPort1[4];
            WriteIO(dataArrayPort1[4], 1,4);

        }
       
        //Fills the ComboBox (which holds the names of SheetSetting)
        //so it connects the database get the names of SheetSetting names and fill combobox
        private void FillComboBox()
        {
            List<SheetSettings> LSS = new List<SheetSettings>(); 
            NumaratorDataBase D = new NumaratorDataBase();
            D.GetSheetSettings(LSS); //Fills the list with the SheetSettings
            foreach (SheetSettings s in LSS)
            {
                SheetSettingComboBox.Items.Add(s.settingname); //Add Settings' name to combobox
            }
        }

        //if any TextBox is required to enter only int number use this previewIntInput
        private void TextBoxPreviewIntInput(object sender, TextCompositionEventArgs e)
        {
            TextBox T = sender as TextBox;
            if (T.Text.Contains(" "))
                T.Text = T.Text.Replace(" ", "");
            if (T.Text.Length > 4)
                e.Handled = true;
            foreach (char c in e.Text)
            {
                if (!(e.Text[0] >= '0' && e.Text[0] <= '9'))
                {
                    e.Handled = true;
                }
            }
        }

        //Event For TopLeft BottomRight CheckBoxes
        private void TopLeftChecked(object sender, RoutedEventArgs e)
        {
            this.istopleft = true;
        }

        private void BottomRightChecked(object sender, RoutedEventArgs e)
        {
            this.istopleft = false;
        }

        //starts camera to grap images
        private void StartControl()
        {
            if (Xfer != null)
                Xfer.Grab();
        }

        //Event For "Başla" Button Calls "StartControl" Function, Gives signals, Changes the state of Start/Stop Button
        private void StartControl(object sender, RoutedEventArgs e)
        {
            if (this.SheetToBePressedNum - this.PressedNum <= 0)
                MessageBox.Show("Kalan Tabaka Sayısı 0. Eğer Devam Etmek İstiyorsanız Basılacak Tabaka Sayısını Değiştirin ve Enter'a Basın!");
            else
            {
                if (isrunning == false)
                {
                    //  timer.Enabled = false;
                    StartControl();
                    isrunning = true;
                    dataArrayPort0[3] = true;//mod
                    dataArrayPort0[2] = true;

                    WriteIO(dataArrayPort0[3], 0, 3);
                    WriteIO(dataArrayPort0[2], 0, 2);

                    dataArrayPort2[2] = true;
                    WriteIO(dataArrayPort2[2], 2, 2);

                    StopCont.Content = "Durdur";
                    StopCont.Foreground = new SolidColorBrush(Colors.Red);
                    (sender as Button).IsEnabled = false;
                    StopCont.IsEnabled = true;
                }
            }
        }

        //it is triggered when the cam takes an image
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
             
             OperatorSettingsControl context = args.Context as OperatorSettingsControl;
             context.View.ShowNext();
             context.isFrameOkMut.WaitOne();
             context.m_Pro.isFrameOkList.Add(context.isFrameOk());
             context.isFrameOkMut.ReleaseMutex();
             context.m_Pro.ExecuteNext();
             //ThreadPool.QueueUserWorkItem(o => context.AddTrueFalse());
            //update the live view

            // bool isOK = context.isFrameOk();
             //bool isOK = true;
             //context.isFrameOkMut.WaitOne();
             //context.m_Pro.isFrameOkList.Add(isOK);
             //context.isFrameOkMut.ReleaseMutex();
             //context.m_Pro.ExecuteNext();
        }

        
        //
        // This function is called each time a buffer (a sheet or taken image) has been processed by the processing object (m_Pro)
        //
        static void ProCallback(object sender, SapProcessingDoneEventArgs pInfo)
        {
            
                OperatorSettingsControl context = pInfo.Context as OperatorSettingsControl;
                // Refresh view
                App.Current.Dispatcher.Invoke(new Action(context.ShowResult));
            
        }

        //Loads the last entered parameters.
        public void LoadParameters(string fname)
        {
             if (!System.IO.File.Exists(fname))
             {
                 return;
             }
             using (System.IO.StreamReader file = new System.IO.StreamReader(fname, false))
             {
                 string s=file.ReadLine();
                 string[] parameters=s.Split(';');
                 //MessageBox.Show(parameters.Count().ToString());
                 if (parameters.Count() == 15)
                 {
                    if(SheetSettingComboBox.Items.Count>Convert.ToInt32(parameters[0]))
                       SheetSettingComboBox.SelectedIndex = Convert.ToInt32(parameters[0]);
                    StartSerialNumber.Text = parameters[1];
                    StartSequenceNumber.Text = parameters[2];
                    topleftradiobutton.IsChecked = Convert.ToBoolean(parameters[3]);
                    PressCount.Text = parameters[4];
                    CapakSensivity.Value = Convert.ToInt32(parameters[5]);
                    WeakSensivity.Value = Convert.ToInt32(parameters[6]);
                    PositionSensivity.Value = Convert.ToInt32(parameters[7]);
                    Pressed.Text = parameters[8];
                    FalsePressed.Text = parameters[9];
                    PressedNum = Convert.ToInt32(parameters[8]);
                    FalsePressedNum = Convert.ToInt32(parameters[9]);
                    SheetToBePressed.Text = Convert.ToString(parameters[14]);
                    LeftSheetTextBlock.Text = Convert.ToString(Convert.ToInt32(SheetToBePressed.Text) - PressedNum);
                 }
             }
         }
         
         //Saves the entered parameters
         public void SaveParameters(string fname) 
         {
             if (!System.IO.File.Exists(fname))
             {
                 if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fname)))
                     System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fname));
                 System.IO.FileStream fs= System.IO.File.Create(fname);
                 fs.Close();
             }
             using (System.IO.StreamWriter file = new System.IO.StreamWriter(fname, false))
             {
                 int Comboboxindex=SheetSettingComboBox.SelectedIndex;
                 string Serialnumber=StartSerialNumber.Text;
                 string sequencenumber=NextSerialNumber.Text;
                 bool IsTopleft=(bool)topleftradiobutton.IsChecked;
                 string count=PressCount.Text;
                 int CapakSens=(int)CapakSensivity.Value;
                 int weakSens=(int)WeakSensivity.Value;
                 int PositionSens=(int)PositionSensivity.Value;
                 string PressedSheets=Pressed.Text;
                 string FalsePressedSheet=FalsePressed.Text;
                 int FS; //falseSheet
                 int WS; //weak/Kelek Sheet
                 int Capak;
                 int position;
                 FS = (int)this.reactionController.FalseNumber;
                 WS = (int)this.reactionController.WeakPrint;
                 Capak = (int)this.reactionController.ExcessivePrint;
                 position = (int)this.reactionController.Position;
                 string last = Comboboxindex.ToString() + ";" + Serialnumber + ";" + sequencenumber + ";" + IsTopleft.ToString() + ";" + count + ";"
                     + CapakSens.ToString() + ";" + weakSens.ToString() + ";" + PositionSens.ToString() + ";" + PressedSheets + ";" + FalsePressedSheet + ";" +
                     FS.ToString() + ";" + WS.ToString() + ";" + Capak.ToString() + ";" + position.ToString()+";" + SheetToBePressedNum.ToString();
                 file.WriteLine(last);
                 file.Close();
             }
         }

        //for database operation. each controlled Sheet (it is false or true) is sended for database operations 
        public void NumaratorDatabase(FrameResult Result, bool fail)
        {
            Num.WaitOne();
                ++NumCount;
            Num.ReleaseMutex();
            NumaratorDataBase D = new NumaratorDataBase();
            if (Result.result.Count > 0)
            {
                if (D.IsSheetExist(Result.result[0].SheetSerial))
                    D.DeleteSheet(Result.result[0].SheetSerial);
                D.InsertSheetV2(Result.result[0].SheetSerial, sheetsetting.settingname, sheetsetting.settingname, DateTime.Now, fail, Result.xofset, Result.yofset);
            }
            if (fail)
            {
                foreach (CheckResultPair CRP in Result.result)
                {
                    if (CRP.data != 1)
                    {
                        int r = CRP.indexX;
                        int c = CRP.indexY;
                        D.InsertFailure(Result.result[0].SheetSerial, r, c, CRP.index, CRP.SheetSerial, DateTime.Now, CRP.data);
                    }
                }
            }
            Num.WaitOne();
                --NumCount;
            Num.ReleaseMutex();
        }

        //saves images
        public void ImageSave(FrameResult Result, bool fail)
        {
            Img.WaitOne();
                ++ImgCount;
            Img.ReleaseMutex();
            if (fail && Result.result.Count > 0)
            {
                //BitmapSource bs = BitmapSource.Create(Buffers.Width, Buffers.Height, 36, 36, System.Windows.Media.PixelFormats.Gray8, BitmapPalettes.Gray256, Result.imageBuff, Result.buffersize, Buffers.Width);
                //PngBitmapEncoder pngBME = new PngBitmapEncoder();
                //pngBME.Frames.Add(BitmapFrame.Create(bs));
                if (!System.IO.Directory.Exists("FalseSheets"))
                    System.IO.Directory.CreateDirectory("FalseSheets");
                Buffers.Save("FalseSheets\\" + Result.result[0].SheetSerial + ".jpeg", "-format jpeg -quality 25", Result.FrameNo, 1);
                //using (System.IO.FileStream fs = new System.IO.FileStream("FalseSheets\\" + Result.result[0].SheetSerial + ".bmp", System.IO.FileMode.Create))
                //    pngBME.Save(fs);
            }
            else
            {
                //if (counter == 0)
                //{
                    //BitmapSource bs = BitmapSource.Create(Buffers.Width, Buffers.Height, 36, 36, System.Windows.Media.PixelFormats.Gray8, BitmapPalettes.Gray256, Result.imageBuff, Result.buffersize, Buffers.Width);
                    //PngBitmapEncoder pngBME = new PngBitmapEncoder();
                    //pngBME.Frames.Add(BitmapFrame.Create(bs));

                    //if (!System.IO.Directory.Exists("CorrectSheets"))
                    //    System.IO.Directory.CreateDirectory("CorrectSheets");
                    //using (System.IO.FileStream fs = new System.IO.FileStream("CorrectSheets\\" + Result.result[0].SheetSerial + ".bmp", System.IO.FileMode.Create))
                    //    pngBME.Save(fs);
                //}
                //if (!System.IO.Directory.Exists("CorrectSheets"))
                //    System.IO.Directory.CreateDirectory("CorrectSheets");
                //Buffers.Save("CorrectSheets\\" + Result.result[0].SheetSerial + ".jpeg", "-format jpeg -quality 100", Result.FrameNo, 1);
                Marshal.FreeHGlobal(Result.imageBuff); //free the memory
                /*DateTime now = DateTime.Now;
                string[] CSPaths = System.IO.Directory.GetFiles(@"C:\Users\User\Documents\Visual Studio 2013\Projects\CombinedNumarator\NumaratorInterface\NumaratorInterface\NumaratorInterface\CorrectSheets\");
                if (CSPaths.Count() > 100)
                {
                    foreach (string CSPath in CSPaths)
                    {
                        NumaratorDataBase D = new NumaratorDataBase();
                        string SheetNumber = System.IO.Path.GetFileName(CSPath).Replace(".bmp", "");
                        if (D.IsSheetExist(SheetNumber))
                        {
                            DateTime DT = D.GetSheetTime(SheetNumber);
                            if (DT == DateTime.MinValue)
                                System.IO.File.Delete(CSPath);
                            else if (now - DT > System.TimeSpan.FromDays(1))
                                System.IO.File.Delete(CSPath);
                        }
                        else
                        {
                            System.IO.File.Delete(CSPath);
                        }
                    }
                }*/
            }
            Img.WaitOne();
                --ImgCount;
            Img.ReleaseMutex();
        }
        //
        //ProCallback function invokes this function. this function takes one result from the resultlist
        //Updates GUI and directs results to Database and imageSave function
        //


        public void ShowResult()
        {
           
            ResultList.WaitOne();
            FrameResult Result = m_Pro.Results[0]; // take result
            m_Pro.Results.RemoveAt(0);
            ResultList.ReleaseMutex();
            if (!Result.isOK)
                return;
                //look if it is false or not
                bool fail = false;
                Result.result.ForEach(p =>
                {
                    if (p.data == -4)
                        p.data = 1;
                });
                if (Result.result.Exists(p => p.data != 1))
                {
                    fail = true;
                    
                    //FalseRejectTimer.Start();
                    /*if (Result.result.Exists(p => p.data == -1) || Result.result.Exists(p => p.data == -2))
                        GenerateWave(5);
                    else 
                        GenerateWave(0);*/
                }
                ++PressedNum; //icrease pressedNum which holds the number of pressed Sheet
                ThreadPool.QueueUserWorkItem(o => ImageSave(Result, fail)); //put image save on a thread queue
                //if Sheet is false 
                if (fail)
                {

                    //++FalsePressedNum; //increase FalsePressedNum
                    //FalsePressed.Text = Convert.ToString(FalsePressedNum); //Change GUI
                    //this.FSController.AddSheet(Result, this.sheetsetting, Buffers.Width, Buffers.Height,Result.result[0].SheetSerial); //Add result to FalseSheetController
                    //send result to reaction controller and if it is 0 (means stop) send signals if it is 1 (means warn) send warn signal
                    int r=this.reactionController.ReactToSheet(Result);
                    if (FalseList.Count == 0)
                        FalseRejectTimer.Enabled = false;
                    if (WarningList.Count == 0)
                        WarningTimer.Enabled = false;         
                    if (r == 0)
                    {
                        if (FalseRejectTimer.Enabled != true)
                        {
                            FalseRejectTimer.Enabled = true;
                        }
                        ListFalseMut.WaitOne();
                        FalseList.Add(Result);
                        ListFalseMut.ReleaseMutex();
                        //FalseCounter++;
                       // FalseCounter = FalseList.Count;
                        if (FalseList.Count >= FalseRejectLimit)
                        {
                            FalsePressedNum += FalseList.Count; //increase FalsePressedNum
                            FalsePressed.Text = Convert.ToString(FalsePressedNum); //Change GUI
                            foreach (FrameResult fr in FalseList)
                            {
                                //***////daha önce ekli mi diye bak!!!!!
                                this.FSController.AddSheet(fr, this.sheetsetting, Buffers.Width, Buffers.Height, fr.result[0].SheetSerial); //Add result to FalseSheetController                               
                            }
                            ListFalseMut.WaitOne();
                            FalseList.Clear();
                            ListFalseMut.ReleaseMutex();
                           // FalseCounter = 0;
                            dataArrayPort0[5] = true;
                            timer2.Enabled = true;
                            FalseRejectTimer.Enabled = false;
                            WriteIO(dataArrayPort0[5], 0, 5);

                            dataArrayPort0[0] = true; //uyarı  
                            dataArrayPort1[3] = true;//kırmızı
                            dataArrayPort2[2] = false;//yeşil ışık                     

                            WriteIO(dataArrayPort2[2], 2, 2);
                            WriteIO(dataArrayPort1[3], 1, 3);
                            WriteIO(dataArrayPort0[0], 0, 0);
                            DeleteError.IsEnabled = true;
                            
                        }
                      //  else dataArrayPort0[5] = false;

                
                        //if (!OnAlarm)
                        //    ThreadPool.QueueUserWorkItem(o => GenerateWave(3,2));
                    }
                    else if (r == 1)
                    {
                        if (WarningTimer.Enabled != true)
                        {
                            WarningTimer.Enabled = true;
                        }
  

                        ListWarningMut.WaitOne();
                        WarningList.Add(Result);
                        ListWarningMut.ReleaseMutex();
                        if (WarningList.Count >= FalseRejectLimit)
                        {
                            FalsePressedNum += WarningList.Count; //increase FalsePressedNum
                            FalsePressed.Text = Convert.ToString(FalsePressedNum); //Change GUI
                            foreach (FrameResult fr in WarningList)
                            {
                                //***////daha önce ekli mi diye bak!!!!!
                                this.FSController.AddSheet(fr, this.sheetsetting, Buffers.Width, Buffers.Height, fr.result[0].SheetSerial); //Add result to FalseSheetController
                            }
                            ListWarningMut.WaitOne();
                            WarningList.Clear();
                            ListWarningMut.ReleaseMutex();
                            WarningTimer.Enabled = false;
                            dataArrayPort0[0] = true; //uyarı           
                            dataArrayPort1[2] = true;//sarı ışık
                            dataArrayPort2[2] = false;//yeşil ışık

                            dataArrayPort2[3] = true;//korna
                            timerWarningKorna.Enabled = true;                        

                            //ThreadPool.QueueUserWorkItem(o => WriteIO(dataArrayPort0, 0));
                            //ThreadPool.QueueUserWorkItem(o => WriteIO(dataArrayPort2, 2));
                            //ThreadPool.QueueUserWorkItem(o => WriteIO(dataArrayPort1, 1));
                          //  dataArrayPort0[3] = true;//mod                            

                          //  WriteIO(dataArrayPort0[3], 0, 3);
                            WriteIO(dataArrayPort0[0], 0, 0);
                            WriteIO(dataArrayPort2[2], 2, 2);
                            WriteIO(dataArrayPort1[2], 1, 2);
                            DeleteError.IsEnabled = true;
                            
                        }
                    } //ThreadPool.QueueUserWorkItem(o => GenerateWave(0));
                }
                LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum-PressedNum);
                Pressed.Text = Convert.ToString(PressedNum);
                SheetToBePressed.Text = SheetToBePressedNum.ToString();
                NextSerialNumber.Text = Convert.ToString(Convert.ToSingle((Result.result[0].SheetSerial.Substring(this.sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber)))-1).PadLeft(this.sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber,'0');
                SaveParameters(ParameterFileName);

                if (SheetToBePressedNum - PressedNum == 14 || SheetToBePressedNum - PressedNum <= 0)
                {
                    SendEndOfOperation();
                }
                if (SheetToBePressedNum - PressedNum == 0 || SheetToBePressedNum - PressedNum <= 0)
                {
                    if (StopCont.Content as string == "Durdur") //Sistem durduktan sonra devam ettiğinde "Devam Et" "DurDur" arasında gidip gelmemesi için!
                    {
                        StopCont_Click(null, null);
                        if (System.IO.File.Exists(ParameterFileName))
                            System.IO.File.Delete(ParameterFileName);
                    }
                }
                    //these actions will be on queue when thread is available they will be executed.
                ThreadPool.QueueUserWorkItem(o => NumaratorDatabase(Result, fail)); //put database action on a thread queue
        }
        //End of Operation Signal (is called When The PressedNum and SheetToBePressed are equal or difference is 14)
        public void SendEndOfOperation()
        {
            timer3.Enabled = true;
        }
        //Creates objects related to cam and processControl Object (SapView , Buffer, Xfer, m_Pro) 
        public bool CreateNewObjects()
        {
            string ServerName = "Xtium-CL_PX4_1";
            //string GrabberConfigFileName = @"C:\Program Files\Teledyne DALSA\Sapera\CamFiles\User\T_P4_CM_08K070_00_R_8_Default_EncoderTrigger.ccf";
            string GrabberConfigFileName = "Cam\\grabbercamera.ccf";
            //string GrabberConfigFileName = "C:\\Users\\User\\Documents\\CaptureParam\\GrabberConfig.ccf";
            SapLocation loc = new SapLocation(ServerName, 0);
            isLoaded = true;
            if (SapManager.GetResourceCount(ServerName, SapManager.ResourceType.Acq) > 0)
            {
                Acq = new SapAcquisition(loc, GrabberConfigFileName);
                // event for signal status
                


                Buffers = new SapBuffer(10, Acq, SapBuffer.MemoryType.ScatterGather);
                Xfer = new SapAcqToBuf(Acq, Buffers);
                View = new SapView(Buffers);
                
                this.ImgBox.View = View;
                System.Drawing.Size ViewSize = new System.Drawing.Size(1800, 1000);
               
                View.SetScalingMode(0.145f, 0.1f);

                // Create acquisition object
                if (!Acq.Create())
                {
                    Console.WriteLine("Error during SapAcquisition creation!\n");
                    DestroyObjects();//

                    return false;
                }
                Acq.Flip = SapAcquisition.FlipMode.None;
                Acq.EnableEvent(SapAcquisition.AcqEventType.StartOfFrame);
                Acq.SetParameter(SapAcquisition.Prm.EXT_TRIGGER_ENABLE,SapAcquisition.Val.EXT_TRIGGER_ON, true);
                Acq.SignalNotify += new SapSignalNotifyHandler(GetSignalStatus);

                
                Acq.SignalNotifyContext = this;

                bool m_IsSignalDetected = (Acq.SignalStatus != SapAcquisition.AcqSignalStatus.None);
                if (m_IsSignalDetected == false)
                    isLoaded = false;
                Acq.SignalNotifyEnable = true;  
            }


            if (Buffers != null)
            {
               
                // End of frame event
                Xfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
                Xfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
                Xfer.XferNotifyContext = this;

                // Create buffer object
                if (!Buffers.Create())
                {
                    Console.WriteLine("Error during SapBuffer creation!\n");
                    DestroyObjects();//Acq, AcqDevice, Buffers, Xfer, View);
                    return false;
                }

                // Create buffer object
                if (!Xfer.Create())
                {
                    Console.WriteLine("Error during SapTransfer creation!\n");
                    DestroyObjects();//
                    return false;
                }
                //Create View
                if (!View.Create())
                {
                    Console.WriteLine("Error during SapView creation!\n");
                    DestroyObjects();//
                    return false;
                }
               
            }
            else
            {
                //define off-line object
                Buffers = new SapBuffer();
                if (!Buffers.Create())
                {
                    Console.WriteLine("Error during SapBuffer creation!\n");
                    DestroyObjects();//Acq, AcqDevice, Buffers, Xfer, View);
                    return false;
                }
              
            }
            
            //inputs of process controll
          
            this.serialcharnumber = sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber;
            this.sequencecharnumber = sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber;
            string serialP1=StartSerialNumber.Text;
            string serialP2=StartSequenceNumber.Text;
            long serialP2long=Convert.ToInt64(serialP2);
            string path=this.sheetsetting.sheetproperties.serialnumberstyle.CharsFolderPath;
            List<Rect> Rects = new List<Rect>();
            bool topleft = istopleft;
            int charcount = serialcharnumber+sequencecharnumber;
            int row = this.sheetsetting.sheetproperties.rownumber;
            int coll = this.sheetsetting.sheetproperties.collnumber;
            this.sheetcount = Convert.ToInt32(PressCount.Text);
            this.SheetToBePressedNum = Convert.ToInt32(SheetToBePressed.Text);
         
            this.NextSerialNumber.Text = StartSequenceNumber.Text;
            foreach(IntPoint x in this.sheetsetting.serialnumberpositions.positions)
            {
                Rect rect=new Rect(x.X,x.Y,this.sheetsetting.serialnumberpositions.boxwidth,this.sheetsetting.serialnumberpositions.boxheight);
                Rects.Add(rect);
            }

            // Rects[0] = new Rect(Rects[0].X + 200, Rects[0].Y + 200, Rects[0].Width, Rects[0].Height);

            int[] Sp=new int[serialcharnumber+sequencecharnumber];
            int[] Sz=new int[serialcharnumber+sequencecharnumber];
            int i=0;
            foreach(Box B in this.sheetsetting.sheetproperties.serialnumberstyle.BoxList)
            { 
                Sp[i]= Convert.ToInt32(B.Ofset/10*pixellpermm);
                Sz[i]= Convert.ToInt32(B.Width/10*pixellpermm);
                ++i;
            }
            Sz[0] = 25;
            Sz[1] = 17;
            Sz[2] = 17;
            Sz[3] = 17;
            Sz[4] = 17;
            Sz[5] = 17;
            Sz[6] = 17;
            Sz[7] = 17;
            Sz[8] = 17;
            Sz[9] = 17;

            Sp[0] = 10;
            Sp[1] = 9;
            Sp[2] = 9;
            Sp[3] = 19;
            Sp[4] = 9;
            Sp[5] = 9;
            Sp[6] = 9;
            Sp[7] = 9;
            Sp[8] = 9;


            List<IntPoint> LTP = this.sheetsetting.templatePoint;
            int width = this.sheetsetting.templateWidth;
            int height = this.sheetsetting.templateHeight;
            List<string> templateImagePaths=new List<string>();
            for (int j = 0; j < this.sheetsetting.templatePoint.Count; ++j)
            {
                templateImagePaths.Add ( this.sheetsetting.settingname + j.ToString() + ".png");
            }

           // string templatepath = @"C:\Users\User\Documents\Visual Studio 2013\Projects\CombinedNumarator\NumaratorInterface\NumaratorInterface\NumaratorInterface\templateSubImages\" + this.sheetsetting.settingname + ".png";

            m_Pro = new ProcessControl(Buffers, new SapProcessingDoneHandler(ProCallback), this, path, Rects, Buffers.Width, Buffers.Height, serialP1, serialP2long, sequencecharnumber, topleft, sheetcount, Sp, Sz, charcount, row, coll, width, height, LTP, templateImagePaths, Convert.ToInt32(CapakSensivity.Value), Convert.ToInt32(WeakSensivity.Value), Convert.ToInt32(PositionSensivity.Value), ResultList, isFrameOkMut);
            /*m_Pro = new ProcessControl(Buffers, new SapProcessingDoneHandler(ProCallback), this,
                "", null, 10, 20,"", 1000,0,true,30000, null, null, 8,5,8);*/
            // Create processing object
            if (m_Pro != null && !m_Pro.Initialized)
            {
                if (!m_Pro.Create())
                {
                    DestroyObjects();
                    return false;
                }
                m_Pro.AutoEmpty = true;
            }
            
            SapManager.Error+=SapManager_Error;
            SapManager.ServerNotify+=SapManager_ServerNotify;
            return true;
        }

        private void SapManager_Error(object sender, SapErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SapManager_ServerNotify(object sender, SapServerNotifyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetSignalStatus(object sender, SapSignalNotifyEventArgs e)
        {
            SapAcquisition.AcqSignalStatus signalStatus = e.SignalStatus;

         
        }
        public void FinalizeSignals()
        {
            //timer.Enabled = false;
            dataArrayPort0[5] = false;
            dataArrayPort0[0] = false;
            dataArrayPort0[3] = false;
            dataArrayPort0[2] = true;

            dataArrayPort2[2] = false;
            dataArrayPort1[2] = false;
            dataArrayPort1[3] = false;
            WriteIO(dataArrayPort0[5], 0, 5);
            WriteIO(dataArrayPort0[0], 0, 0);
            WriteIO(dataArrayPort0[3], 0, 3);
            WriteIO(dataArrayPort0[2], 0, 2);

            WriteIO(dataArrayPort2[2], 2,2);
            WriteIO(dataArrayPort1[2], 1, 2);
            WriteIO(dataArrayPort1[3], 1, 3);

            dataArrayPort2[3] = false;
            WriteIO(dataArrayPort2[3], 2, 3);
            timerWarningKorna.Enabled = false;
        }
        //SapAcquisition acq, SapAcqDevice camera, SapBuffer buf, SapTransfer xfer, SapView view objects will be destroyed
        // simple explanation: disconnect camera, destroy m_Pro
        public void DestroyObjects()
        {
            myTask.Dispose();
            if (m_Pro != null && m_Pro.Initialized)
            {
                int cnt = 0;
                while (m_Pro.running)
                {
                    cnt++;
                    if (cnt > 100000000) break;
                }
                m_Pro.Destroy();
            }

                if (Xfer != null)
                {
                    Xfer.Destroy();
                    Xfer.Dispose();
                }


                if (Acq != null)
                {
                    Acq.Destroy();
                    Acq.Dispose();
                }

                if (Buffers != null)
                {
                    Buffers.Destroy();
                    Buffers.Dispose();
                }

                if (View != null)
                {
                    View.Destroy();
                    View.Dispose();
                }  
        }

        //Events for Stop/Stop Button
        public void StopCont_Click(object sender, RoutedEventArgs e)
        {
            if (isrunning && StopCont.Content as string == "Durdur") //Stop Part
            {
                //Xfer.Freeze();
                //Xfer.Wait(1000);
             


                dataArrayPort0[3] = false;
                dataArrayPort0[2] = true;
                WriteIO(dataArrayPort0[3], 0, 3);
                WriteIO(dataArrayPort0[2], 0, 2);

                dataArrayPort2[3] = false;
                WriteIO(dataArrayPort2[3], 2, 3);
            
                timerWarningKorna.Enabled = false;      
                //timer.Enabled = true;

                StopCont.Content = "Devam Et";
                StopCont.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (isrunning && StopCont.Content as string == "Devam Et") //Continue part
            {
                int s = Convert.ToInt32(SheetToBePressed.Text);
                if (s != SheetToBePressedNum)
                {
                    ApproveWindow W = new ApproveWindow("Basılacak Tabaka Sayısını Değiştirmek İstediğinize Emin misiniz?");
                    W.ShowDialog();
                    if ((bool)W.DialogResult)
                    {
                        // int oldvalue=this.SheetToBePressedNum;
                        if (sheetcount < s)
                        {
                            MessageBox.Show("Basılacak Tabaka Sayısı Kurulum Sayısından Büyük Olamaz!");
                            SheetToBePressed.Text = SheetToBePressedNum.ToString();                          
                        }
                        else
                            this.SheetToBePressedNum = s;
                        // int diff=oldvalue-s;
                        //PressedNum += diff;
                        //Pressed.Text = Convert.ToString(PressedNum);                    
                    }
                    else
                    {
                        SheetToBePressed.Text = SheetToBePressedNum.ToString();
                    }
                }
                //timer.Enabled = false;
                dataArrayPort0[3] = true;
                dataArrayPort0[2] = true;
                dataArrayPort2[2] = true;
                WriteIO(dataArrayPort0[3], 0, 3);
                WriteIO(dataArrayPort0[2], 0, 2);
                WriteIO(dataArrayPort2[2], 2, 2);
                
                //Xfer.Grab();
                StopCont.Content = "Durdur";
                StopCont.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private bool IsParametersOk()
        {
             if (SheetSettingComboBox.SelectedItem != null)
             {
                        NumaratorDataBase D = new NumaratorDataBase();
                        string sheetsettingname = SheetSettingComboBox.SelectedItem as string;

                        if (D.IsSheetSettingExist(sheetsettingname))
                        {
                            this.sheetsetting = D.GetSheetSetting(sheetsettingname);
                            List<Box> BL = this.sheetsetting.sheetproperties.serialnumberstyle.BoxList;
                            StartSerialNumber.Text = StartSerialNumber.Text.Replace(" ", "");
                            StartSequenceNumber.Text = StartSequenceNumber.Text.Replace(" ", "");
                            if (StartSerialNumber.Text.Length == sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber && StartSequenceNumber.Text.Length == sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber && sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber + sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber == BL.Count)
                            {
                                int i = 0;
                                string s = StartSerialNumber.Text + StartSequenceNumber.Text;
                                foreach (Box B in BL)
                                {
                                    if (B.IsChar)
                                    {
                                        if (!(s[i] >= 'A' && s[i] <= 'Z' && s[i] != 'X' && s[i] != 'Q' && s[i] != 'W'))
                                        {
                                            if (i < this.sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber)
                                                MessageBox.Show("Başlangıç Seri Numarası Uygun Değil!" + " " + "Seri Numarası " + Convert.ToString(i + 1) + ". Karakteri Harf Olmalıdır!");
                                            else
                                                MessageBox.Show("Başlangıç Sıra Numarası Uygun Değil!" + " " + "Sıra Numarası " + Convert.ToString(i - this.sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber + 1) + ". Karakteri Harf Olmalıdır!");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (!(s[i] >= '0' && s[i] <= '9'))
                                        {
                                            if (i < this.sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber)
                                                MessageBox.Show("Başlangıç Seri Numarası Uygun Değil!" + " " + "Seri Numarası " + Convert.ToString(i + 1) + ". Karakteri Rakam Olmalıdır!");
                                            else
                                                MessageBox.Show("Başlangıç Sıra Numarası Uygun Değil!" + " " + "Sıra Numarası " + Convert.ToString(i - this.sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber + 1) + ". Karakteri Rakam Olmalıdır!");
                                            return false;
                                        }
                                    }
                                    ++i;
                                }
                                PressCount.Text = PressCount.Text.Replace(" ", "");
                                if (PressCount.Text == "")
                                {
                                    MessageBox.Show("Basım Sayısını Girin!");
                                    return false;
                                }
                                int a = Convert.ToInt32(PressCount.Text);
                                int b = Convert.ToInt32(SheetToBePressed.Text);
                                if (a < b)
                                {
                                    MessageBox.Show("Basılacak Tabaka Sayısı Kurulum Sayısından Büyük Olamaz!");
                                    return false;
                                }
            
                                //all inputs are appropriate//
                                return true;
                            }
                            else
                            {
                                if (StartSerialNumber.Text.Length != sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber)
                                    MessageBox.Show("Başlangıç Seri Numarası Uygun Değil!" + " " + Convert.ToString(sheetsetting.sheetproperties.serialnumberstyle.SerialCharNumber) + " Karakter Girilmelidir!");
                                else if (StartSequenceNumber.Text.Length != sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber)
                                    MessageBox.Show("Başlangıç Sıra Numarası Uygun Değil!" + " " + Convert.ToString(sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber) + " Rakam Girilmelidir!");
                                return false;
                            }                           
                        }
                        else
                        {
                            MessageBox.Show("Seçili Tabaka Ayarı Yüklenemedi!, Veritabanını Kontrol Edin!");
                            return false;
                        }               
             }
             else
             {
                 MessageBox.Show("Tabaka Ayarı Seçili Değil!");
                 return false;
             } 
        }

        //
        // Event for "yükle" button
        // Checks all the inputs, and if all of them is correct or appropriate,
        // Calls CreateNewObjects(), SaveParameters(), enable Start Button and disable itself
        //
        private void LoadParameters(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            if (StartSequenceNumber.Text == string.Empty && StartSerialNumber.Text == string.Empty)
            {
                System.Windows.Forms.OpenFileDialog ofdlg = new System.Windows.Forms.OpenFileDialog();
                ofdlg.Multiselect = false;
                ofdlg.Filter = "Text Files |*.txt";
                System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo("parameters");
                ofdlg.InitialDirectory = DI.FullName;// "parameters\\";
                if(ofdlg.ShowDialog()==System.Windows.Forms.DialogResult.OK )
                {
                    ParameterFileName = ofdlg.FileName;
                    LoadParameters(ParameterFileName);

                    if (IsParametersOk())
                    {
                        flag = true;
                    }
                }
                else
                {
                    MessageBox.Show("Tabaka Ayarı Seçili Değil!");
                }
            }
            else
            {
                ParameterFileName = "parameters\\"+StartSequenceNumber.Text + StartSerialNumber.Text + ".txt";
                if (IsParametersOk())
                {
                    flag = true;
                }
            }
            if (flag == true) //if all inputs are appropriate
            {
                NumaratorDataBase D = new NumaratorDataBase();
                if (D.IsSheetExist((StartSerialNumber.Text + StartSequenceNumber.Text).Replace(" ", "")))
                {
                    ApproveWindow W = new ApproveWindow("Başlangıç Tabaka Numarası Daha Önce Basılmıştır!\n Devam Etmek İstediğinize Emin misiniz? ");
                    W.Height = W.Height + 30;
                    W.ShowDialog();
                    if((bool)W.DialogResult)
                    {
                        
                        CreateNewObjects();
                        LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum - PressedNum);
                        LoadButton.IsEnabled = false;
                        Start.IsEnabled = true;
                        this.SaveParameters(ParameterFileName);
                    }
                }
                else
                {
                    
                    CreateNewObjects();
                    LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum - PressedNum);
                    LoadButton.IsEnabled = false;
                    Start.IsEnabled = true;
                    this.SaveParameters(ParameterFileName);
                }
            }
          
        }

        //Events for sensivitysliders
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s=sender as Slider;
            
            SensivityText.Text = Convert.ToString(s.Value);
            if (this.m_Pro != null)
                this.m_Pro.CapakSensivity = (int)s.Value;
        }
        private void WeakSensivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = sender as Slider;

            WeakSensivityText.Text = Convert.ToString(s.Value);
            if (this.m_Pro != null)
                this.m_Pro.Sensivity = (int)s.Value;
        }
        private void PositionSensivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = sender as Slider;
            PositionSensivityText.Text = Convert.ToString(s.Value);
            if (this.m_Pro != null)
                this.m_Pro.PositionSensivity = (int)s.Value;
        }

        //Event For "Hata/Uyarı Sil" Button
        private void DeleteError_Click(object sender, RoutedEventArgs e)
        {
            dataArrayPort0[5] = false;
            dataArrayPort1[3] = false;
            dataArrayPort1[2] = false;
            dataArrayPort2[2] = true;
          
            WriteIO(dataArrayPort0[5], 0, 5);
            WriteIO(dataArrayPort1[3], 1, 3);
            WriteIO(dataArrayPort1[2], 1, 2);
            WriteIO(dataArrayPort2[2], 2, 2);
            dataArrayPort2[3] = false;
            WriteIO(dataArrayPort2[3], 2, 3);
            DeleteError.IsEnabled = false;
            //FalseCounter = 0;
            FalseList.Clear();
            WarningList.Clear();
            FalseRejectTimer.Enabled = false;
            WarningTimer.Enabled = false;
            timerWarningKorna.Enabled = false;      
        }

        private void FalseRejeckSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FalseRejectLimit = Convert.ToInt32((sender as Slider).Value);
            FalseRejectText.Text = Convert.ToString(FalseRejectLimit);
        }

        private void FalseRejectTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            interval = Convert.ToInt32((sender as Slider).Value);
            FalseRejectTimeText.Text = Convert.ToString(interval) + "sn";
            FalseRejectTimer.Interval = interval * 1000;
            WarningTimer.Interval = interval * 1000;
        }

        private void MakeZero_Click(object sender, RoutedEventArgs e)
        {
            ApproveWindow W = new ApproveWindow("Sıfırlamak İstediğinize Emin misiniz?");
            W.ShowDialog();
            if((bool)W.DialogResult)
            {
                this.PressedNum = 0;
                this.FalsePressedNum = 0;
                this.Pressed.Text = PressedNum.ToString();
                this.FalsePressed.Text = FalsePressedNum.ToString();
            }
        }

        private void IncreaseWaitedNum(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
            {
                ApproveWindow W = new ApproveWindow("Seri Sıra Numarasını Değiştirmek İstediğinize Emin misiniz?");
                W.ShowDialog();
                if ((bool)W.DialogResult)
                {

                    long s = Convert.ToInt64(NextSerialNumber.Text) + 1;
                    this.m_Pro.CurrentSerialStart = s;
                    NextSerialNumber.Text = Convert.ToString(s).PadLeft(this.sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber, '0');
                    --PressedNum;
                    Pressed.Text = Convert.ToString(PressedNum);
                    LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum- PressedNum);
                    //SheetToBePressedNum++;
                    //SheetToBePressed.Text = SheetToBePressedNum.ToString();
                }
            }
        }

        private void DecreaseWaitedNum(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
            {
                ApproveWindow W = new ApproveWindow("Seri Sıra Numarasını Değiştirmek İstediğinize Emin misiniz?");
                W.ShowDialog();
                if ((bool)W.DialogResult)
                {
                    long s = Convert.ToInt64(NextSerialNumber.Text) - 1;
                    this.m_Pro.CurrentSerialStart = s;
                    NextSerialNumber.Text = Convert.ToString(s).PadLeft(this.sheetsetting.sheetproperties.serialnumberstyle.SequenceCharNumber, '0');
                    ++PressedNum;
                    Pressed.Text = Convert.ToString(PressedNum);
                    LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum - PressedNum);
                    //SheetToBePressedNum--;
                    //SheetToBePressed.Text = SheetToBePressedNum.ToString();
                }
            }
        }

        private void SheetToBePressed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter )
            {
                if (StopCont.Content as string == "Devam Et")
                {
                    ApproveWindow W = new ApproveWindow("Basılacak Tabaka Sayısını Değiştirmek İstediğinize Emin misiniz?");
                    W.ShowDialog();
                    if ((bool)W.DialogResult)
                    {
                        // int oldvalue=this.SheetToBePressedNum;
                        int s = Convert.ToInt32(SheetToBePressed.Text);


                        if (sheetcount < s)
                        {
                            MessageBox.Show("Basılacak Tabaka Sayısı Kurulum Sayısından Büyük Olamaz!");
                            SheetToBePressed.Text = SheetToBePressedNum.ToString();
                            e.Handled = true;
                        }
                        else
                        {
                            this.SheetToBePressedNum = s;
                            LeftSheetTextBlock.Text = Convert.ToString(SheetToBePressedNum - PressedNum);
                        }
                        // int diff=oldvalue-s;
                        //PressedNum += diff;
                        //Pressed.Text = Convert.ToString(PressedNum);                    
                    }
                    else
                    {
                        SheetToBePressed.Text = SheetToBePressedNum.ToString();
                    }
                }
                else SheetToBePressed.Text = SheetToBePressedNum.ToString();
            }
            if (StopCont.Content as string == "Durdur")
            {
                SheetToBePressed.Text = SheetToBePressedNum.ToString();
                e.Handled = true;
            }
        }    

      
    }
}
