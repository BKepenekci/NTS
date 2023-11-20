using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading;
using System.Runtime.InteropServices;
using NationalInstruments.DAQmx;
using System.IO;

namespace NumaratorInterface.Controls.OperatorController
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE : 20.08.2016
    // PURPOSE     : View the list of last false Sheets
    // ===============================
    public partial class FalseSheetController : UserControl
    {
        public List<FalseSheet> FSL;
        private NationalInstruments.DAQmx.Task myTask;
        private DigitalSingleChannelReader myDigitalReader;


        public FalseSheetController()
        {
            InitializeComponent();
            //initiliazation of FalseSheetList
            FSL = new  List<FalseSheet>();
          
        }
      
        //If "Son Yüz" Button is intented to hide, call this function
        public void MakeButtonInvisible()
        {
            ContainerBorder.Margin = new Thickness(30,35,30,10);
            LoadLastSheets.Visibility = Visibility.Collapsed;
        }

        //
        public void ClearMemory()
        {
            foreach (FalseSheet FS in FSL)
            {
                Marshal.FreeHGlobal(FS.Result.imageBuff);
            }
        }
        //

        //To add another FalseSheet to controller
        //result: FalseSheet data<->instance of FrameResult which comes from control alg.
        //SS: SheetSetting of Sheet
        //width: image width
        //height:image height
        public void AddSheet(FrameResult result, SheetSettings SS, int width, int height,string pilotNumber)
        {
            //Create new FalseSheet to add
            FalseSheet FS=new FalseSheet();
            DateTime now=DateTime.Now;
            FS.PilotNumber = pilotNumber;
            FS.Result = result;
            FS.Settings = SS;
            FS.imageWidth = width;
            FS.imageHeight = height; 
            FSL.Insert(0,FS);
            FillTreeView(FS);
        }

        public void FillTreeView(FalseSheet FS)
        {
            //if there is already 100 items in controller remove the last one free the memory
            if (FalseSheetTree.Items.Count > 99)
            {
                Marshal.FreeHGlobal(((FalseSheet)(((TreeViewItem)FalseSheetTree.Items.GetItemAt(99)).Tag)).Result.imageBuff);
                FalseSheetTree.Items.RemoveAt(99);
            }

            TreeViewItem TVI = new TreeViewItem();
            TVI.Margin = new Thickness(5, 5, 5, 0);
            //when double click an item it shows sheet for inspection, for this purpose add an event// 
            TVI.MouseDoubleClick+=TVI_MouseDoubleClick;
            TVI.Tag = FS;

            //Create Stack Panel//
            StackPanel Sp = new StackPanel();
            Sp.Height = 30;
            Sp.HorizontalAlignment = HorizontalAlignment.Stretch;
            Sp.Orientation = Orientation.Horizontal;

            //CreateIndikatorPath// Rectangle Indicator which is showing the error type//
            System.Windows.Shapes.Path p = CreateRectanglePath();
            if (FS.Result.result.Count > 0 && FS.Result.result.Exists(o => o.data == -1))
                p.Fill = p.Fill = new SolidColorBrush(Colors.Red);
            else if (FS.Result.result.Count > 0 && FS.Result.result.Exists(o => o.data == -2))
                p.Fill = p.Fill = new SolidColorBrush(Colors.Yellow);
            else if (FS.Result.result.Count > 0 && FS.Result.result.Exists(o => o.data == -3))
                p.Fill = p.Fill = new SolidColorBrush(Colors.Violet);
            else if (FS.Result.result.Count > 0 && FS.Result.result.Exists(o => o.data == -4))
                p.Fill = p.Fill = new SolidColorBrush(Colors.Blue);
            else if (FS.Result.result.Count > 0 && FS.Result.result.Exists(o => o.data == -5))
                p.Fill = p.Fill = new SolidColorBrush(Colors.DarkGray);
            else if (FS.Result.result.Count > 0 && FS.Result.result[0].data == 1)
                p.Fill = p.Fill = new SolidColorBrush(Colors.Green);
            DockPanel DP = new DockPanel();
            TextBlock T = new TextBlock();
            T.Foreground = new SolidColorBrush(Colors.White);

            T.Text = FS.PilotNumber.Substring(0, FS.Settings.sheetproperties.serialnumberstyle.SerialCharNumber) + " " + FS.PilotNumber.Substring(FS.Settings.sheetproperties.serialnumberstyle.SerialCharNumber, FS.Settings.sheetproperties.serialnumberstyle.SequenceCharNumber);
            if (FS.Result.result.Exists(o => o.data != 1 && o.index % 2 == 0))
                T.Text = T.Text + " ^";
            if (FS.Result.result.Exists(o => o.data != 1 && o.index % 2 == 1))
                T.Text = T.Text + " v";
            T.FontSize = 19;
            DP.Children.Add(p);
            DP.Children.Add(T);          
           // Sp.Children.Add(DP);
            TVI.Header = DP;
         
            FalseSheetTree.Items.Insert(0,TVI);
        }

      


        //opens a new window for inspection of FalseSheet 
        private void OpenWindow(FalseSheet FS)
        {
            FalseSheetViewerWindow window = new FalseSheetViewerWindow();
            window.Viewer.SheetNo.Text = FS.PilotNumber;
            if (FS.Result.imageBuff == IntPtr.Zero)
            {
                Stream jpegStrem = new FileStream("FalseSheets\\" + FS.PilotNumber+ ".jpeg", FileMode.Open, FileAccess.Read, FileShare.Read);
                JpegBitmapDecoder dec=new JpegBitmapDecoder(jpegStrem,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.Default);
                BitmapSource bs = dec.Frames[0];
                Image img = new Image();
                img.BeginInit();
                img.Source = bs;
                img.EndInit();
                window.Viewer.TemplateCanvas.Children.Add(img);
                img.Width = FS.imageWidth;
                img.Height = FS.imageHeight;
            }
            else
            {
                BitmapSource bs = BitmapSource.Create(FS.imageWidth, FS.imageHeight, 36, 36, System.Windows.Media.PixelFormats.Gray8, BitmapPalettes.Gray256, FS.Result.imageBuff, FS.Result.buffersize, FS.imageWidth);
                Image img = new Image();
                img.BeginInit();
                img.Source = bs;
                img.EndInit();
                window.Viewer.TemplateCanvas.Children.Add(img);
                img.Width = FS.imageWidth;
                img.Height = FS.imageHeight;
            }
            
            //foreach (CheckResultPair CRP in FS.Result.result)
            //{
            //    System.Windows.Shapes.Path p = this.CreateRectanglePath();
            //    p.Data = new RectangleGeometry(new Rect(0, 0, FS.Settings.serialnumberpositions.boxwidth, FS.Settings.serialnumberpositions.boxheight));
            //    p.StrokeThickness = 1;
            //    p.Fill = new SolidColorBrush(Colors.HotPink);
            //    p.Opacity = 0.5;
            //    Canvas.SetLeft(p, FS.Settings.serialnumberpositions.positions[CRP.index].X );
            //    Canvas.SetTop(p, FS.Settings.serialnumberpositions.positions[CRP.index].Y );
            //    window.Viewer.TemplateCanvas.Children.Add(p);
            //}
            
            //draw rectangle for each serial number with correct color according to error. 
            foreach (CheckResultPair CRP in FS.Result.result)
            {
                
                System.Windows.Shapes.Path p = this.CreateRectanglePath();
               p.Data = new RectangleGeometry(new Rect(0, 0, FS.Settings.serialnumberpositions.boxwidth, FS.Settings.serialnumberpositions.boxheight));
               if (CRP.data == -1)
                    p.Fill = new SolidColorBrush(Colors.Red);//red for false number
               else if (CRP.data == 1)
                    p.Fill = new SolidColorBrush(Colors.Green);//green for correct serial number
               else if (CRP.data == -2)
                    p.Fill = new SolidColorBrush(Colors.Yellow);//yellow for weakprint
               else if (CRP.data == -3)
                   p.Fill = new SolidColorBrush(Colors.Violet);//Violet for excessiveprint
               else if (CRP.data == -4)
                   p.Fill = new SolidColorBrush(Colors.Blue);//Blue for position error
               else if (CRP.data == -5)
                   p.Fill = new SolidColorBrush(Colors.DarkGray);
               p.Opacity = 0.5;
               Canvas.SetLeft(p, FS.Settings.serialnumberpositions.positions[CRP.index].X - CRP.offsetX);
               Canvas.SetTop(p, FS.Settings.serialnumberpositions.positions[CRP.index].Y - CRP.offsetY);
               window.Viewer.TemplateCanvas.Children.Add(p);
               
            }
            int number = 1;
            for (int c = FS.Settings.sheetproperties.collnumber - 1; c > -1; --c)
            {
                for (int r = FS.Settings.sheetproperties.rownumber - 1; r > -1; --r)
                {
                    string text = number.ToString() + ". Magazin";
                    int x = FS.Settings.sheetproperties.collnumber * r*2 + c*2;
                    int px=(FS.Settings.serialnumberpositions.positions[x].X + FS.Settings.serialnumberpositions.positions[x + 1].X)/2;
                    int py = (FS.Settings.serialnumberpositions.positions[x].Y + FS.Settings.serialnumberpositions.positions[x + 1].Y) / 2;
                    TextBlock T = new TextBlock();
                    T.Text = text;
                    T.FontSize = 100;
                    Canvas.SetLeft(T, px);
                    Canvas.SetTop(T, py);
                    window.Viewer.TemplateCanvas.Children.Add(T);
                    ++number;
                }
            
            }


                window.ShowDialog();
            window.Dispatcher.InvokeShutdown(); 
        }
        private void TVI_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //open window in new GUI Thread
            FalseSheet FS = (FalseSheet)((sender as TreeViewItem).Tag);
            string s=((TextBlock)((DockPanel)((sender as TreeViewItem).Header)).Children[1]).Text;
            Thread OpenWindowThread = new Thread(() => OpenWindow(FS));
            OpenWindowThread.SetApartmentState(ApartmentState.STA);
            OpenWindowThread.Start();
        }

        private System.Windows.Shapes.Path CreateRectanglePath()
        {
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
            p.Data = new RectangleGeometry(new Rect(0, 0, 20, 20));
            p.StrokeThickness = 0.1;
            p.Stroke = new SolidColorBrush(Colors.Black);
            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.A = 120;
            c.B = 200;
            c.G = 0;
            c.R = 0;
            p.Fill = new SolidColorBrush(c);
            return p;
        }

        //event for "son yüz" button, loads last 100 FalseSheet from the database
        private void LoadLasSheets(object sender, RoutedEventArgs e)
        {
             this.Cursor = Cursors.Wait;
             NumaratorDataBase D = new NumaratorDataBase();
             List<FalseSheet> LFS = D.GetLastFalseSheets();
             foreach (FalseSheet FS in LFS)
             {
                  this.AddSheet(FS.Result, FS.Settings, FS.imageWidth, FS.imageHeight,FS.PilotNumber);
             }
             this.Cursor = Cursors.Arrow;
        }
    }
}
