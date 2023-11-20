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


using System.Windows.Forms;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using DALSA.SaperaLT.SapClassBasic;
using System.Runtime.InteropServices;

namespace NumaratorInterface.Controls.SheetSettingControls
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Determines the positions of SerialNumbers, SerialNumber's width, height, Template SubImage, 
    // ===============================
    public partial class SerialNumberPositionsControl : System.Windows.Controls.UserControl
    {
       
        AdornerLayer ADL;
        public string aim = "";
        public System.Windows.Point? MidPointPosition = null;
        public SheetProperties sheetproperties; //this is only referance. Assign this referance to SheetPropertiesControl's sheetproperties object in ControlSheetSettings
        public SerialNumberPositions serialnumberpositions;
        int ImageHeight = 3725;
        int ImageWidth = 4325;
        const float PixelPermm = 7.9f;
        const float PixelPermmX = 7.4f;
        public IntPtr intp;
        public bool isSnaping = false;
        DALSA.SaperaLT.SapClassBasic.SapAcquisition Acq = null;
        public System.Threading.Mutex IOMut = new System.Threading.Mutex();
        public SapBuffer Buffers = null;
        public SapTransfer Xfer = null;
        public Path SelectedPath = null;
        public SerialNumberPositionsControl() 
        {

            CreateNewObjects();
            InitializeComponent();
            
            this.serialnumberpositions = new SerialNumberPositions();

            //Template Image Rectangle
            //Path p=CreateRectanglePath();
            //System.Windows.Media.Color c = new System.Windows.Media.Color();
            //c.A = 80;
            //c.B = 250;
            //c.G = 125;
            //c.R = 125;
            //p.Fill = new SolidColorBrush(c);
            //System.Windows.Media.Color strokeColor = new System.Windows.Media.Color();
            //strokeColor.A = 255;
            //strokeColor.B = 0;
            //strokeColor.G = 0;
            //strokeColor.R = 0;
            //p.Stroke = new SolidColorBrush(strokeColor);
            //p.StrokeThickness = 1;
            //subImageCanvas.Children.Add(p);
            //Canvas.SetLeft(p,1200);
            //Canvas.SetTop(p, 500);
            //p.Data=new RectangleGeometry(new Rect(0, 0, 150, 150));
            //p.MouseLeftButtonDown += Path_MouseLeftButtonDown;
            //
        }
        public static BitmapImage GetBitmapImage(byte[] imageBuffer)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new System.IO.MemoryStream(imageBuffer);
            bi.EndInit();
            return bi;
        }

        //fires when camera takes an image
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
           SerialNumberPositionsControl context = args.Context as SerialNumberPositionsControl;
           App.Current.Dispatcher.Invoke(new Action(context.ShowImage));
        }
        
        //Create Camera's object
        public bool CreateNewObjects()
        {
            string ServerName = "Xtium-CL_PX4_1";
            string GrabberConfigFileName = "Cam\\grabbercamera.ccf";

            SapLocation loc = new SapLocation(ServerName, 0);

            if (SapManager.GetResourceCount(ServerName, SapManager.ResourceType.Acq) > 0)
            {
                Acq = new SapAcquisition(loc, GrabberConfigFileName);

                Buffers = new SapBuffer(5, Acq, SapBuffer.MemoryType.ScatterGather);
                Xfer = new SapAcqToBuf(Acq, Buffers);


                // Create acquisition object
                if (!Acq.Create())
                {
                    Console.WriteLine("Error during SapAcquisition creation!\n");
                    DestroyObjects();//
                    return false;
                }
                Acq.Flip = SapAcquisition.FlipMode.None;
                Acq.EnableEvent(SapAcquisition.AcqEventType.StartOfFrame);

            }


            if (Buffers != null)
            {
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
            return true;
        }

        // destroys SapAcquisition acq, SapAcqDevice camera, SapBuffer buf, SapTransfer xfer
        public void DestroyObjects()
        {
            Xfer.Freeze();
            Xfer.Wait(1000);
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
        }
        
        //Create first two rectangle for positions of first banknot's serialnumber's positions (one is Red other one is Black)
        public void CreatePivotBoxes()
        {

            List<Path> LP = TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            foreach (Path p in LP)
            {
                TemplateCanvas.Children.Remove(p);
            }

            int startX=Math.Max(0,Convert.ToInt32((ImageWidth-(this.sheetproperties.banknotewidth * this.sheetproperties.collnumber)*PixelPermmX)/2));
            int startY= Math.Max(0,Convert.ToInt32((ImageHeight - (this.sheetproperties.banknoteheight * this.sheetproperties.rownumber) * PixelPermm) / 2));
            Path p1 = CreateRectanglePath();
            Path p2 = CreateRectanglePath();

            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.A = 200;
            c.B = 50;
            c.G = 50;
            c.R = 50;
            p1.Fill = new SolidColorBrush(c);

            System.Windows.Media.Color c2 = new System.Windows.Media.Color();
            c2.A = 120;
            c2.B = 0;
            c2.G = 0;
            c2.R = 200;
            p2.Fill = new SolidColorBrush(c2);

            Canvas.SetLeft(p1, Convert.ToInt32(startX+this.sheetproperties.banknotewidth*PixelPermmX/2));
            Canvas.SetTop(p1, Convert.ToInt32(startY+ PixelPermm * 3));
            Canvas.SetLeft(p2, Convert.ToInt32(startX+PixelPermmX*10));
            Canvas.SetTop(p2, Convert.ToInt32(startY+this.sheetproperties.banknoteheight*PixelPermm*3/4));
            TemplateCanvas.Children.Add(p1);
            TemplateCanvas.Children.Add(p2);
            p1.MouseLeftButtonDown += Path_MouseLeftButtonDown;
            p2.MouseLeftButtonDown += Path_MouseLeftButtonDown;
        }

        //Event of Black Box's Height or witdh change (first Rectangle) //Changes all of the other rectangles heights
        private void A_heightwidthchanged()
        {
            List<Path> LP=TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            foreach (Path p in LP)
            {
                if (p != LP[0])
                {
                    ((RectangleGeometry)(p.Data)).Rect= new Rect(0,0,((RectangleGeometry)((LP[0]).Data)).Rect.Width, ((RectangleGeometry)((LP[0]).Data)).Rect.Height);
                }

            }
        }

        //MouseClick event on Paths
        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<Path> LP = TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            List<Path> TemplatePaths = subImageCanvas.Children.OfType<Path>().ToList<Path>();
            for(int i=0;i<TemplatePaths.Count;++i)
            {
                LP.Add(TemplatePaths[i]);
            }
            if (LP.Count >= 2)
            {
                Path p = sender as Path;
                RemoveAdorner();
                if (TemplatePaths.Count != 0)
                {
                    if (LP[0] == p || LP[LP.Count - TemplatePaths.Count] == p)
                    {
                        XamlEditor.ResizingAdorner A = new XamlEditor.ResizingAdorner(p, true); //send true for first rectangle (true creates two red dots for changing the dimensions)
                        if (LP[0] == p)
                        {
                            A.heightwidthchanged += A_heightwidthchanged;
                        }
                        else
                        {
                            A.heightwidthchanged += Template_heightwidthchanged;
                        }
                        ADL = AdornerLayer.GetAdornerLayer(p);
                        ADL.Add(A);
                    }
                    else
                    {
                        XamlEditor.ResizingAdorner A = new XamlEditor.ResizingAdorner(p, false); //send false for others
                        ADL = AdornerLayer.GetAdornerLayer(p);
                        ADL.Add(A);
                    }
                }
                else
                {
                    if (LP[0] == p)
                    {
                        XamlEditor.ResizingAdorner A = new XamlEditor.ResizingAdorner(p, true); //send true for first rectangle (true creates two red dots for changing the dimensions)
                        if (LP[0] == p)
                        {
                            A.heightwidthchanged += A_heightwidthchanged;
                        }
                        else
                        {
                            A.heightwidthchanged += Template_heightwidthchanged;
                        }
                        ADL = AdornerLayer.GetAdornerLayer(p);
                        ADL.Add(A);
                    }
                    else
                    {
                        XamlEditor.ResizingAdorner A = new XamlEditor.ResizingAdorner(p, false); //send false for others
                        ADL = AdornerLayer.GetAdornerLayer(p);
                        ADL.Add(A);
                    }
                }
            }
        }

        private void Template_heightwidthchanged()
        {
            List<Path> LP = subImageCanvas.Children.OfType<Path>().ToList<Path>();
            foreach (Path p in LP)
            {
                if (p != LP[0])
                {
                    ((RectangleGeometry)(p.Data)).Rect = new Rect(0, 0, ((RectangleGeometry)((LP[0]).Data)).Rect.Width, ((RectangleGeometry)((LP[0]).Data)).Rect.Height);
                }
            }
        }

        //creates arbitrary rectangle path (only helper method, call whenever you need a rectangle)
        private Path CreateRectanglePath()
        {
            Path p = new Path();
            p.Data = new RectangleGeometry(new Rect(0, 0, 156, 36));
            p.StrokeThickness = 0.1;
            p.Stroke = new SolidColorBrush(Colors.Black);
            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.A=120;
            c.B = 200;
            c.G = 0;
            c.R = 0;
            p.Fill = new SolidColorBrush(c);
            return p;
        }


        private void sheetproperties_sheetwidthchanged(float value)
        {
            List<Path> LP=TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            foreach (Path p in LP)
            {
                TemplateCanvas.Children.Remove(p);
            }
        }

        //Event for Pan ability
        private void TemplateGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                if (MidPointPosition != null)
                {
                    System.Windows.Point p = e.GetPosition(TemplateGrid);
                    XO1.TransformationOperations.MoveUIElement(TemplateStackPanel, p.X - MidPointPosition.Value.X, p.Y - MidPointPosition.Value.Y);
                    MidPointPosition = p;
                }
                else
                {
                    MidPointPosition = e.GetPosition(TemplateGrid);
                }
            }
            else
            {
                MidPointPosition = e.GetPosition(TemplateGrid);
            }
            if (ADL != null)
                ADL.Update();
            if (aim == "")
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            else
                this.Cursor = System.Windows.Input.Cursors.Pen;
        }

        //Event For Zoom Ability
        private void TemplateGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            XO1.TransformationOperations.ScaleUIElement(TemplateStackPanel, e.Delta > 0, e.GetPosition(TemplateStackPanel));
            if (ADL != null)
                ADL.Update();
        }

        //if mouse leave the area, MidPointProperty should be null for pan ability to work correctly
        private void TemplateGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MidPointPosition = null;
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        //defines the openning transform of the image
        public void BringImage()
        {
            TransformGroup TG = new TransformGroup();
            ScaleTransform ST = new ScaleTransform();
            ST.ScaleX = 0.16;
            ST.ScaleY = 0.16;
            TG.Children.Insert(0, ST);
            TemplateStackPanel.RenderTransform = TG;
        }

        //Event For "Resim Al" Button
        public void TakeImage(object sender, RoutedEventArgs e)
        {
            if (Xfer == null) return;
            if (!isSnaping) //if it is allready taking an image do nothing
            {
                Xfer.Snap(1);
                isSnaping = true;
            }
        }

        //Update GUI and apply alg. that decides whether the image has sheet or not and take another image if there is none
        public void ShowImage()
        {
            isSnaping = false;
            IntPtr bufData = Marshal.AllocHGlobal(Buffers.Width * Buffers.Height * Buffers.BytesPerPixel);
            Buffers.Read(0, Buffers.Width * Buffers.Height, bufData);
            intp = bufData;
            BitmapSource bs = BitmapSource.Create(Buffers.Width, Buffers.Height, 36, 36, System.Windows.Media.PixelFormats.Indexed8, BitmapPalettes.Gray256, bufData, Buffers.get_SpaceUsed(Buffers.Index), Buffers.Width);
            BanknoteImg.BeginInit();
            BanknoteImg.Source = bs;
            BanknoteImg.EndInit();

            BanknoteImg.Height = Buffers.Height;// context.Buffers.Height;
            BanknoteImg.Width = Buffers.Width;// context.Buffers.Width;
            ImageWidth = Buffers.Width;
            ImageHeight = Buffers.Height;

            //
            byte[] bytes = new byte[Buffers.Height * Buffers.Width];
            bs.CopyPixels(bytes, Buffers.Width, 0);
            int index = Buffers.Height / 2 * Buffers.Width;
            int count=0;
            for (int i = 0; i < Buffers.Width; ++i)
            {
                if (bytes[index + i] < 50)
                    ++count;
            }
            //

            BringImage();
            if (count < Buffers.Width * 0.4) //there is sheet Call CreatePivotBoxes
                CreatePivotBoxes();
            else //blank image, take another one
                TakeImage(null, null); 
            
        }


        //Clear all rectangles except the first two (Event for "Konum Kutalarını Sil" Button)
        private void ClearBoxes(object sender, RoutedEventArgs e)
        {
            List<Path> LP=TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            if (LP.Count > 2)
                for (int i = 2; i < LP.Count; ++i)
                    TemplateCanvas.Children.Remove(LP[i]);
            subImageCanvas.Children.Clear();

        }

        
        private void RemoveAdorner()
        {
            List<Path> LP = TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            if(subImageCanvas.Children.Count>0)
            LP.Add(subImageCanvas.Children.OfType<Path>().ToList<Path>().First<Path>());
            foreach (Path p in LP)
            {
                try
                {
                    Adorner[] toRemoveArray = ADL.GetAdorners(p);
                    if (toRemoveArray != null)
                    {
                        for (int x = 0; x < toRemoveArray.Length; x++)
                        {
                                ADL.Remove(toRemoveArray[x]);
                        }
                    }
                }
                catch { }
            }
        
        }

        //Event For "Konum Kutularını Yerleştir" Button
        public void AddBoxes(object sender, RoutedEventArgs e)
        {
            List<Path> LP = TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            if (LP.Count >= 2)
            {
                ClearBoxes(sender, e);
                for (int i = 0; i < this.sheetproperties.rownumber; ++i)
                    for (int j = 0; j < this.sheetproperties.collnumber; ++j)
                    {
                        if (!(j == 0 && i == 0))
                        {
                            Path p1 = CreateRectanglePath();
                            Path p2 = CreateRectanglePath();
                            System.Windows.Media.Color c = new System.Windows.Media.Color();
                            c.A = 120;
                            c.B = 200;
                            c.G = 200;
                            c.R = 0;
                            p1.Fill = new SolidColorBrush(c);
                            p2.Fill = new SolidColorBrush(c);
                            Canvas.SetLeft(p1, j * this.sheetproperties.banknotewidth * PixelPermmX + Canvas.GetLeft(LP[0])+((RectangleGeometry)LP[0].Data).Rect.TopLeft.X);
                            Canvas.SetTop(p1, i * this.sheetproperties.banknoteheight * PixelPermm + Canvas.GetTop(LP[0])+((RectangleGeometry)LP[0].Data).Rect.TopLeft.Y);
                            Canvas.SetLeft(p2, j * this.sheetproperties.banknotewidth * PixelPermmX + Canvas.GetLeft(LP[1]) + ((RectangleGeometry)LP[1].Data).Rect.TopLeft.X);
                            Canvas.SetTop(p2, i * this.sheetproperties.banknoteheight * PixelPermm + Canvas.GetTop(LP[1]) + ((RectangleGeometry)LP[1].Data).Rect.TopLeft.Y);
                            ((RectangleGeometry)(p1.Data)).Rect = new Rect(0, 0, ((RectangleGeometry)(LP[0].Data)).Rect.Width, ((RectangleGeometry)(LP[0].Data)).Rect.Height);
                            ((RectangleGeometry)(p2.Data)).Rect = new Rect(0, 0, ((RectangleGeometry)(LP[0].Data)).Rect.Width, ((RectangleGeometry)(LP[0].Data)).Rect.Height);
                            TemplateCanvas.Children.Add(p1);
                            TemplateCanvas.Children.Add(p2);
                            p1.MouseLeftButtonDown += Path_MouseLeftButtonDown;
                            p2.MouseLeftButtonDown += Path_MouseLeftButtonDown;
                        }
                    }
            }
            subImageCanvas.Children.Clear();
            for (int i = 0; i < this.sheetproperties.rownumber; ++i)
            {


                Path templatepath= CreateRectanglePath();
                System.Windows.Media.Color c = new System.Windows.Media.Color();
                c.A = 80;
                c.B = 250;
                c.G = 125;
                c.R = 125;
                templatepath.Fill = new SolidColorBrush(c);
                System.Windows.Media.Color strokeColor = new System.Windows.Media.Color();
                strokeColor.A = 255;
                strokeColor.B = 0;
                strokeColor.G = 0;
                strokeColor.R = 0;
                templatepath.Stroke = new SolidColorBrush(strokeColor);
                templatepath.StrokeThickness = 1;
                subImageCanvas.Children.Add(templatepath);
                Canvas.SetLeft(templatepath, 1200);
                Canvas.SetTop(templatepath, 500*(i+2));
                templatepath.Data = new RectangleGeometry(new Rect(0, 0, 150, 150));
                templatepath.MouseLeftButtonDown += Path_MouseLeftButtonDown;
            }

        }
        public void AutoFindPositions(object sender, RoutedEventArgs e)
        {
            if (this.intp != IntPtr.Zero)
            {
                List<Path> LP = this.TemplateCanvas.Children.OfType<Path>().ToList<Path>();

                //Register Rectangles
                List<System.Drawing.Rectangle> RectList = new List<System.Drawing.Rectangle>();
                foreach (Path p in LP)
                {
                    RectList.Add(new System.Drawing.Rectangle(Convert.ToInt32(Canvas.GetLeft(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.X), Convert.ToInt32(Canvas.GetTop(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.Y), (int)((RectangleGeometry)p.Data).Rect.Width, (int)((RectangleGeometry)p.Data).Rect.Height));
                }
                RectList = ControlSheetSettings.CheckRectangles(RectList, this.intp, this.Buffers.Width, this.Buffers.Height);//get new Rectangles
                int i = 0;
                foreach (Path p in LP) //change Paths
                {
                    ((RectangleGeometry)(p.Data)).Rect = new Rect(0, 0, RectList[i].Width, RectList[i].Height);
                    Canvas.SetLeft(p, RectList[i].X);
                    Canvas.SetTop(p, RectList[i].Y);
                    ++i;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Resim Alınmamıştır!");
            }
        }

        private void Vertical_Click(object sender, RoutedEventArgs e)
        {
            aim = "Vertical";
        }

        private void Horizontal_Click(object sender, RoutedEventArgs e)
        {
            aim = "Horizontal";
        }

        private void TemplateGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (aim != "")
                this.Cursor = System.Windows.Input.Cursors.Pen;
            else
                this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                EscapeKeyDown();
            }
        }
        public void EscapeKeyDown()
        {
            this.aim = "";
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            if (SelectedPath != null)
            {
                SelectedPath.Stroke = new SolidColorBrush(Colors.SkyBlue);
                try
                {
                    Adorner[] toRemoveArray = ADL.GetAdorners(SelectedPath);
                    if (toRemoveArray != null)
                    {
                        for (int x = 0; x < toRemoveArray.Length; x++)
                        {
                            ADL.Remove(toRemoveArray[x]);
                        }
                    }
                }
                catch { }
            }
        }

        private void TemplateGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (aim == "Vertical")
            {
                System.Windows.Point p=e.GetPosition(TemplateStackPanel);
                Path line = new Path();
                line.Data = new LineGeometry(new System.Windows.Point(p.X, 0), new System.Windows.Point(p.X, 10500));
                line.StrokeThickness = 3;
                line.Stroke = new SolidColorBrush(Colors.SkyBlue);
                line.MouseMove += line_MouseMoveVertical;
                line.MouseLeave += line_MouseLeaveVertical;
                line.MouseLeftButtonDown += line_MouseLeftButtonDown;
                GuideLines.Children.Add(line);
            }
            else if (aim == "Horizontal")
            {
                System.Windows.Point p = e.GetPosition(TemplateStackPanel);
                Path line = new Path();
                line.Data = new LineGeometry(new System.Windows.Point(0,p.Y), new System.Windows.Point(8192,p.Y));
                line.StrokeThickness = 3;
                line.Stroke = new SolidColorBrush(Colors.SkyBlue);
                line.MouseMove += line_MouseMoveHorizontal;
                line.MouseLeave += line_MouseLeaveHorizontal;
                line.MouseLeftButtonDown+=line_MouseLeftButtonDown;
                GuideLines.Children.Add(line);
            }
        }

        private void line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedPath != null)
            {
                SelectedPath.Stroke = new SolidColorBrush(Colors.SkyBlue);
                try
                {
                    Adorner[] toRemoveArray = ADL.GetAdorners(SelectedPath);
                    if (toRemoveArray != null)
                    {
                        for (int x = 0; x < toRemoveArray.Length; x++)
                        {
                            ADL.Remove(toRemoveArray[x]);
                        }
                    }
                }
                catch { }
            }
            Path p = sender as Path;
            SelectedPath = p;
            p.Stroke = new SolidColorBrush(Colors.Red);
            XamlEditor.ResizingAdorner A = new XamlEditor.ResizingAdorner(p, true);
            ADL = AdornerLayer.GetAdornerLayer(p);
            ADL.Add(A);

        }

        private void line_MouseLeaveHorizontal(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*for (int i = 0; i < 5; ++i)
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point p = e.GetPosition(TemplateStackPanel);
                    (sender as Path).Data = new LineGeometry(new System.Windows.Point(0, p.Y), new System.Windows.Point(8192, p.Y));
                }*/
        }

        private void line_MouseMoveHorizontal(object sender, System.Windows.Input.MouseEventArgs e)
        {
           /* if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(TemplateStackPanel);
                (sender as Path).Data = new LineGeometry(new System.Windows.Point(0, p.Y), new System.Windows.Point(8192, p.Y));
            }*/
        }

        void line_MouseLeaveVertical(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*for (int i = 0; i < 5;++i )
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point p = e.GetPosition(TemplateStackPanel);
                    (sender as Path).Data = new LineGeometry(new System.Windows.Point(p.X, 0), new System.Windows.Point(p.X, 10500));
                }*/
        }

        void line_MouseMoveVertical(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(TemplateStackPanel);
                (sender as Path).Data = new LineGeometry(new System.Windows.Point(p.X, 0), new System.Windows.Point(p.X, 10500));
            }*/
        }

        private void detele_Click(object sender, RoutedEventArgs e)
        {
            GuideLines.Children.Clear();
        }
    }
}

