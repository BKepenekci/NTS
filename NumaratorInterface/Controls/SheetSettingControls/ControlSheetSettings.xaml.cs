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
using tessnet2;
using System.Runtime.InteropServices;
using NationalInstruments.DAQmx;

namespace NumaratorInterface.Controls.SheetSettingControls
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Combine UserControls (SerialNumberPositionsControl,SheetPropertiesControl , DataBaseSheetSettingContoller)
    // in one UserControl
    // ===============================
    public partial class ControlSheetSettings : UserControl
    {
        SerialNumberPositionsControl PositionControl = new SerialNumberPositionsControl(); //Create SerialNumberPositionsControl instance

    
        public ControlSheetSettings()
        {
            InitializeComponent();
            PositionControl.Name = "PositionControl";
            SerialNbrView.Children.Add(PositionControl);// add SerialNumberPositionsControl to Grid

            //assign PositionControl's sheetProperties to PropertyControl's Sheetproperties (for constructing the positions, sheetproperties like coll and row number is needed)
            this.PositionControl.sheetproperties = this.PropertyControl.sheetproperties; 

            this.DataBaseControl.LoadEvent += DataBaseControl_LoadEvent; //Construct an event for Database load Event
        }

        //Update GUI according to new SheetSettings (Database Controller sends new SheetSetting)
        private void DataBaseControl_LoadEvent(SheetSettings s)
        {
            //Change the SheetProperties
            this.PropertyControl.SheetWidth.Text = Convert.ToString(s.sheetproperties.sheetwidth);
            this.PropertyControl.SheetHeight.Text = Convert.ToString(s.sheetproperties.sheetheight);
            this.PropertyControl.RowNumber.Text = Convert.ToString(s.sheetproperties.rownumber);
            this.PropertyControl.CollNumber.Text = Convert.ToString(s.sheetproperties.collnumber);
            this.PropertyControl.BanknoteHeight.Text = Convert.ToString(s.sheetproperties.banknoteheight);
            this.PropertyControl.BanknoteWidth.Text = Convert.ToString(s.sheetproperties.banknotewidth);

            //Select from Combobox where appropriate name
            foreach (object o in this.PropertyControl.ComboBox.Items)
            {
                if (o as string == s.sheetproperties.serialnumberstyle.SerialStyleName)
                {
                    this.PropertyControl.ComboBox.SelectedItem = o;
                    break;
                }
            }

            //Create Rectangles of new SheetSettings
            this.PositionControl.CreatePivotBoxes(); 
            this.PositionControl.AddBoxes(null,null);
            this.PositionControl.BringImage();
            List<Path> PL=this.PositionControl.TemplateCanvas.Children.OfType<Path>().ToList<Path>();
            int i = 0;
            foreach (Path p in PL)
            {
                ((RectangleGeometry)p.Data).Rect=new Rect(0,0,s.serialnumberpositions.boxwidth,s.serialnumberpositions.boxheight);
                Canvas.SetLeft(p, s.serialnumberpositions.positions[i].X);
                Canvas.SetTop(p, s.serialnumberpositions.positions[i].Y);
                ++i;
            }
            //

            //Change the position of Template Path according to new SheetSetting
            List<Path> subPaths=this.PositionControl.subImageCanvas.Children.OfType<Path>().ToList<Path>();
            int y = 0;
            foreach (Path subPath in subPaths)
            {
                ((RectangleGeometry)subPath.Data).Rect = new Rect(0, 0, s.templateWidth, s.templateHeight);
                Canvas.SetLeft(subPath, s.templatePoint[y].X);
                Canvas.SetTop(subPath, s.templatePoint[y].Y);
                ++y;
            }

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
        public static List<System.Drawing.Rectangle> CheckRectangles(List<System.Drawing.Rectangle> rects, IntPtr dptr, int columns, int rows)
        {
            int s = (int)Math.Ceiling((double)columns / 4) * 4;

            var stride = columns;
            byte[] ar = new byte[columns * rows];
            Marshal.Copy(dptr, ar, 0, rows * columns);
            var newbytes = PadLines(ar, rows, columns, s);
            IntPtr dptr2 = Marshal.AllocHGlobal(rows * s);
            Marshal.Copy(newbytes, 0, dptr2, s * rows);
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(columns, rows, s, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, dptr2);

            System.Drawing.Imaging.ColorPalette pal = bm.Palette;

            for (int j = 0; j < 256; j++)
            {
                pal.Entries[j] = System.Drawing.Color.FromArgb(255, j, j, j);
            }

            bm.Palette = pal;
            

            List<System.Drawing.Rectangle> result = new List<System.Drawing.Rectangle>();// rects.Count);
            var ocr = new Tesseract();
            ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPRSTUVYZ1234567890"); // If digit only
            ocr.Init(@"C:\tessdata", "eng", false);
            foreach (System.Drawing.Rectangle r in rects)
            {
                var res = ocr.DoOCR(bm, r);
                int maxh = 0;
                int minh = int.MaxValue;
                int maxx = 0;
                int minx = int.MaxValue;
                res.ForEach(n =>
                {
                    if (n.Top < minh) minh = n.Top;
                    if (n.Bottom > maxh) maxh = n.Bottom;
                    if (n.Left < minx) minx = n.Left;
                    if (n.Right > maxx) maxx = n.Right;
                });
                result.Add(new System.Drawing.Rectangle(minx - 10+r.X, minh - 10+r.Y, (maxx - minx) + 20, (maxh - minh) + 20));
            }
            return result;
        }
        
        //Event of "Kaydet" Button. opens a dialog for saving or replacing
        private void SaveSheetSettings(object sender, RoutedEventArgs e)
        {
            if (this.PositionControl.intp != IntPtr.Zero)
            {
                if (this.PropertyControl.ComboBox.SelectedValue as string != null)
                {
                    NumaratorDataBase D = new NumaratorDataBase();
                    this.PropertyControl.sheetproperties.serialnumberstyle = D.GetSerialNumberStyle(this.PropertyControl.ComboBox.SelectedValue as string);
                    List<Path> LP = this.PositionControl.TemplateCanvas.Children.OfType<Path>().ToList<Path>();
                    List<Path> TP = this.PositionControl.subImageCanvas.Children.OfType<Path>().ToList<Path>();
                    if (LP.Count == this.PropertyControl.sheetproperties.rownumber * this.PropertyControl.sheetproperties.collnumber * 2)
                    {
                        //Register Rectangles
                        List<System.Drawing.Rectangle> RectList = new List<System.Drawing.Rectangle>();
                        foreach (Path p in LP)
                        {
                            RectList.Add(new System.Drawing.Rectangle(Convert.ToInt32(Canvas.GetLeft(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.X), Convert.ToInt32(Canvas.GetTop(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.Y), (int)((RectangleGeometry)p.Data).Rect.Width, (int)((RectangleGeometry)p.Data).Rect.Height));
                        }
                        int i = 0;
                        foreach (Path p in LP) //change Paths
                        {
                            ((RectangleGeometry)(p.Data)).Rect = new Rect(0, 0, RectList[i].Width, RectList[i].Height);
                            Canvas.SetLeft(p, RectList[i].X);
                            Canvas.SetTop(p, RectList[i].Y);
                            ++i;
                        }
                        //
                        List<IntPoint> TPL = new List<IntPoint>();
                        foreach (Path p in TP)
                        {
                            int x = Convert.ToInt32(Canvas.GetLeft(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.X);
                            int y = Convert.ToInt32(Canvas.GetTop(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.Y);
                            IntPoint IP = new IntPoint(x, y);
                            TPL.Add(IP);
                        }
                        List<IntPoint> IPL = new List<IntPoint>();
                        foreach (Path p in LP)
                        {
                            int x = Convert.ToInt32(Canvas.GetLeft(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.X);
                            int y = Convert.ToInt32(Canvas.GetTop(p) + ((RectangleGeometry)p.Data).Rect.TopLeft.Y);
                            IntPoint IP = new IntPoint(x, y);
                            IPL.Add(IP);
                        }

                        int WidthMax = (int)LP.Max<Path>(p => ((RectangleGeometry)p.Data).Rect.Width);
                        int HeightMax = (int)LP.Max<Path>(p => ((RectangleGeometry)p.Data).Rect.Height);
                        this.PositionControl.serialnumberpositions.positions = IPL;
                        
                        if (IPL.Count > 0)
                        {
                            this.PositionControl.serialnumberpositions.boxwidth = Convert.ToInt32(((RectangleGeometry)LP[0].Data).Rect.Width);
                            this.PositionControl.serialnumberpositions.boxheight = Convert.ToInt32(((RectangleGeometry)LP[0].Data).Rect.Height);
                        }
                        else
                        {
                            MessageBox.Show("Seri Numarası Pozisyonları Girilmemiştir!");
                            return;
                        }
                        Path subImagePath = this.PositionControl.subImageCanvas.Children.OfType<Path>().First<Path>();
                        Rect r = new Rect(Canvas.GetLeft(subImagePath) + ((RectangleGeometry)subImagePath.Data).Rect.TopLeft.X, Canvas.GetTop(subImagePath) + ((RectangleGeometry)subImagePath.Data).Rect.TopLeft.Y, ((RectangleGeometry)subImagePath.Data).Rect.Width, ((RectangleGeometry)subImagePath.Data).Rect.Height);
                        SheetSettingsSaveWindow W = new SheetSettingsSaveWindow(this.PropertyControl.sheetproperties, this.PositionControl.serialnumberpositions, r, this.PositionControl.Buffers, TPL);
                        W.ShowDialog();

                        //after Save Method Refresh DatabaseController
                        this.DataBaseControl.FillList();
                        this.DataBaseControl.FillTreeView();
                        //
                    }
                    else
                    {
                        MessageBox.Show("Seri Numarası Yerlerini Belirten Kutu Sayısı Satır ve Sütun Sayısıyla Uyumlu Değil!");
                    }
                }
                else
                {
                    MessageBox.Show("Seri Numarası Stilini Seçin");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Resim Alınmamıştır!");
            }
        }
    }
}
