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

namespace NumaratorInterface.Controls.SerialNumberControls
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU 
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Controls the properties of BoxList SerialNumberStyle
    // ===============================
    public partial class BoxListController : UserControl
    {
        public List<Box> BoxList;
        public Rectangle selectedRec;
        public delegate void delegatelistchanged();
        public event delegatelistchanged boxlchanged;
        public int charnumberserial;
        public int charnumbersequence;
        public BoxListController()
        {
            InitializeComponent();
            this.BoxList = new List<Box>(10);
            for (int i = 0; i < 10; ++i)
                this.BoxList.Add(new Box());
            this.BoxList[0].IsChar = true;
            this.BoxList[0].Ofset = 10;
            this.BoxList[0].Width = 25;
            this.BoxList[3].Ofset = 19;
            serialcharnumber.Text = "4";
            charnumberserial = 4;
            sequencecharnumber.Text = "6";
            charnumbersequence = 6;
            HeightTB.Tag = selectedRec;
            WidthTB.Tag = selectedRec;
            OfsetTB.Tag = selectedRec;
            FillBox();
        }

        //Updates the View of SerialNumberStyle (Draws Rectangles according to inputs)
        public void FillBox()
        {
            selectedRec = null;
            SerialNumberDockPanel.Children.Clear();
            int a = BoxList.Count;
            for (int i = 0; i < a; ++i)
            {
                    Rectangle x = new Rectangle();
                    x.Tag = BoxList[i];
                    x.MouseLeftButtonDown += BoxMouseLeftButtonDown;
                    x.Width = Math.Max(5,BoxList[i].Width);
                    x.Height =Math.Max(5, BoxList[i].Height);
                    x.Margin = new System.Windows.Thickness { Right = BoxList[i].Ofset, Bottom=10 };
                    if (BoxList[i].IsChar)
                        x.Fill=new SolidColorBrush(Colors.Lime);
                    else
                        x.Fill = new SolidColorBrush(Colors.Orange);
                    x.Stroke = new SolidColorBrush(Colors.Black);
                    DockPanel.SetDock(x, Dock.Left);
                    x.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    x.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    if (i == 0)
                    {
                        x.Margin = new System.Windows.Thickness { Left= 30, Right=BoxList[i].Ofset,Bottom=10 };
                    }
                    SerialNumberDockPanel.Children.Add(x);
            }
        }

        //event for selecting a Box
        void BoxMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedRec != null)
            {
                if (((Box)(selectedRec.Tag)).IsChar)
                    selectedRec.Fill = new SolidColorBrush(Colors.Lime);
                else
                    selectedRec.Fill = new SolidColorBrush(Colors.Orange);
            }
            selectedRec = (Rectangle)sender;
            selectedRec.Fill = new SolidColorBrush(Colors.Red);
            WidthTB.Text = Convert.ToString(((Box)(selectedRec.Tag)).Width/10);
            HeightTB.Text = Convert.ToString(((Box)(selectedRec.Tag)).Height/10);
            OfsetTB.Text = Convert.ToString(((Box)(selectedRec.Tag)).Ofset/10);
            IsChar.IsChecked = ((Box)(selectedRec.Tag)).IsChar;
            IsNumber.IsChecked = !(((Box)(selectedRec.Tag)).IsChar);
        }

        //Events for Box and BoxList Property Change
        private void charnumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox charnumber = sender as TextBox;
            if (charnumber.Text.Contains(' '))
            {
                try
                {
                    charnumber.Text = Convert.ToString(Convert.ToInt32(charnumber.Text));
                }
                catch
                {
                    charnumber.Text = "";
                }
            }

            int j;
            int k;
            if (serialcharnumber.Text == "")
                j = 0;
            else
                j = Convert.ToInt32(serialcharnumber.Text);

            if (sequencecharnumber.Text == "")
                k = 0;
            else
                k = Convert.ToInt32(sequencecharnumber.Text);
            int a = j + k;
            if (this.BoxList.Count < a)
            {
                for (int i = this.BoxList.Count; i < a; ++i)
                    this.BoxList.Add(new Box());
            }
            else
            {
                for (int i = a; this.BoxList.Count > a; --i)
                    this.BoxList.RemoveAt(this.BoxList.Count - 1);
            }
            charnumberserial = j;
            charnumbersequence = k;
            if (boxlchanged != null)
                boxlchanged();
            FillBox();
        }

        private void charnumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox charnumber = sender as TextBox;
            if (boxlchanged != null)
                boxlchanged();
            try
            {
                Convert.ToInt32(e.Text);
                if (charnumber.Text.Length > 1 && charnumber.SelectedText=="")
                {
                    e.Handled = true;
                }
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void HeightTextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox T=sender as TextBox;
            if (T.Text.Contains(' '))
            {
                T.Text = T.Text.Replace(" ", "");
            }
            float Height;
            if (HeightTB.Text != "")
            {
                Height = Convert.ToSingle(HeightTB.Text);
            }
            else
            {
                Height= 0;
            }

            if (selectedRec != null)
            {
                ((Box)(selectedRec.Tag)).Height = Height*10;
                selectedRec.Height = Math.Max(Height * 10,5);
            }
            if (boxlchanged != null)
                boxlchanged();
        }

        private void WidthTextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox T = sender as TextBox;
            if (T.Text.Contains(' '))
            {
                T.Text = T.Text.Replace(" ", "");
            }
            float Width;
            if (WidthTB.Text != "")
            {
                Width = Convert.ToSingle(WidthTB.Text);
            }
            else
            {
                Width = 0;
            }
            if (selectedRec != null)
            {
                ((Box)(selectedRec.Tag)).Width = Width * 10;
                selectedRec.Width = Math.Max(Width * 10, 5);
            }
            if (boxlchanged != null)
                boxlchanged();
        }

        private void OfsetTextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox T = sender as TextBox;
            if (T.Text.Contains(' '))
            {
                T.Text = T.Text.Replace(" ", "");
            }
            float Ofset;
            if (OfsetTB.Text != "")
            {
                Ofset = Convert.ToSingle(OfsetTB.Text);
            }
            else
            {
                Ofset = 0;
            }

            if (selectedRec != null)
            {
                if (((Box)(selectedRec.Tag)) != BoxList[0])
                    selectedRec.Margin = new System.Windows.Thickness { Right = Ofset*10, Bottom = 10 };
                else
                    selectedRec.Margin = new System.Windows.Thickness { Left = 30, Right = Ofset*10, Bottom = 10 };
                ((Box)(selectedRec.Tag)).Ofset = Ofset * 10;
            }
            if (boxlchanged != null)
                boxlchanged();
        }

        private void PropertyPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox T = sender as TextBox;
            if (e.Text == ".")
                e.Handled = true;
            try
            {
                string s = T.Text + e.Text;
                Convert.ToSingle(s);
                if (T.Text.Length > 4 && T.SelectedText == "")
                {
                    e.Handled = true;
                }
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void NumberChecked(object sender, RoutedEventArgs e)
        {
            if (selectedRec!=null)
                ((Box)(selectedRec.Tag)).IsChar = false;
            if (boxlchanged != null)
                boxlchanged();
        }

        private void LetterChecked(object sender, RoutedEventArgs e)
        {
            if (selectedRec != null)
                ((Box)(selectedRec.Tag)).IsChar = true;
            if (boxlchanged != null)
                boxlchanged();
        }
    }
}
