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
    // PURPOSE     : Creates a sample view of SerialNumberStyle, also holds the SerialNumberStyle object that is to be saved
    // ===============================
    public partial class SerialNumberGenerator : UserControl
    {
        bool folder = false;
        bool serial = false;
        private string letters = "ABCDEFGHIJKLMNOPRSTUVYZ";
        public SerailNumberStyle serialnumberstyle;
        public SerialNumberGenerator()
        {
            InitializeComponent();
        }

        //Event for "Seri Numarası Oluştur" button
        private void GenerateNumber(object sender, RoutedEventArgs e)
        {
            SerialNumber.Text = ""; //clear the TextBox' Text
            foreach (Box B in this.serialnumberstyle.BoxList)
            {
                if (B.IsChar) //if it is letter put 'A'
                {
                    SerialNumber.Text += "A";
                }
                else //otherwise put '0'
                {
                    SerialNumber.Text += "0";
                }
            }
        }

        //Control Alg. of whether the input number is correct 
        public void NumberControl()
        {
            if (this.SerialNumber.Text == "")
            {
                serial = false;
                SerialWarning.Text = "Seri Numarası Oluşturun!";
                SerialWarning.Foreground = new SolidColorBrush(Colors.Yellow);
                ClearImage();
                return;
            }
            else
            {
                string value = SerialNumber.Text.Replace(" ", "");
                if (value.Length == this.serialnumberstyle.BoxList.Count)
                {
                    int i = 0;
                    foreach (Box B in this.serialnumberstyle.BoxList)
                    {
                        if (B.IsChar)
                        {
                            if (value[i] >= 'A' && value[i] <= 'Z' && value[i] != 'X' && value[i] != 'W' && value[i] != 'Q')
                                ++i;
                            else
                            {
                                SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                                SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                                ClearImage();
                                serial = false;
                                return;
                            }
                        }
                        else
                        {
                            if (value[i] >= '0' && value[i] <= '9')
                                ++i;
                            else
                            {
                                SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                                serial = false;
                                SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                                ClearImage();
                                return;
                            }
                        }
                    }
                    SerialWarning.Text = "Seri Numarası Uygun";
                    serial = true;
                    SerialWarning.Foreground = new SolidColorBrush(Colors.Green);
                    return;
                }
                else
                {
                    serial = false;
                    SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                    SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                    ClearImage();
                    return;
                }
            }
        }

        public void FolderControl()
        {
            if (this.serialnumberstyle.CharsFolderPath == "")
            {
                GeneratedImageWarning.Foreground = new SolidColorBrush(Colors.Red);
                GeneratedImageWarning.Text = "Karakter Klasörü Seçili Değil";
                ClearImage();
                folder = false;
                return;
            }
            else if (!System.IO.Directory.Exists(this.serialnumberstyle.CharsFolderPath))
            {
                GeneratedImageWarning.Foreground = new SolidColorBrush(Colors.Red);
                GeneratedImageWarning.Text = "Seçili Klasör Uygun Değil";
                ClearImage();
                folder = false;
                return;
            }
            else
            {
                string[] s = System.IO.Directory.GetFiles(this.serialnumberstyle.CharsFolderPath);
                //check and get number chars//
                for (int i = 0; i < 10; ++i)
                {
                    string value = this.serialnumberstyle.CharsFolderPath + "\\" + Convert.ToString(i) + ".png";
                    if (!(s.Contains<string>(value)))
                    {
                        GeneratedImageWarning.Foreground = new SolidColorBrush(Colors.Red);
                        GeneratedImageWarning.Text = "Seçili Klasör Uygun Değil";
                        ClearImage();
                        folder = false;
                        return;
                    }
                }
                //check and get letterchars//
                foreach (char c in letters)
                {
                    string value = this.serialnumberstyle.CharsFolderPath + "\\" + c + ".png";
                    if (!(s.Contains<string>(value)))
                    {
                        GeneratedImageWarning.Foreground = new SolidColorBrush(Colors.Red);
                        GeneratedImageWarning.Text = "Seçili Klasör Uygun Değil";
                        ClearImage();
                        folder = false;
                        return;
                    }
                }
                GeneratedImageWarning.Text = "Seçili Klasör Uygun";
                folder = true;
                GeneratedImageWarning.Foreground = new SolidColorBrush(Colors.Green);
                MakeImage();
            }
        }

        //Clears the sample image
        private void ClearImage()
        {
            GeneratedSerialNumberGrid.Children.Clear();
        }

        //makes Image according to input
        public void MakeImage()
        {
            NumberControl();
            GeneratedSerialNumberGrid.Children.Clear();
            if (serial && folder)
            {
                float height = 0;
                float width = 0;
                foreach (Box B in this.serialnumberstyle.BoxList)
                {
                    if (B.Height > height)
                        height = B.Height;
                    width = width + B.Width + B.Ofset;
                }
                width = width - this.serialnumberstyle.BoxList[this.serialnumberstyle.BoxList.Count - 1].Ofset;
                DockPanel D = new DockPanel();
                D.VerticalAlignment = VerticalAlignment.Top;
                D.HorizontalAlignment = HorizontalAlignment.Left;
                D.Width = width;
                D.Height = height;
                D.Background = new SolidColorBrush(Colors.White);
                D.Margin = new Thickness { Top = 15, Left = 10 };
                int i = 0;
                foreach (Box B in this.serialnumberstyle.BoxList)
                {
                    string value=this.serialnumberstyle.CharsFolderPath +"\\" + this.SerialNumber.Text[i] + ".png";
                    Image Img = new Image();
                    Img.Stretch = System.Windows.Media.Stretch.Fill;
                    if(B!= this.serialnumberstyle.BoxList[this.serialnumberstyle.BoxList.Count-1])
                        Img.Margin = new System.Windows.Thickness { Right=B.Ofset};
                    Img.HorizontalAlignment = HorizontalAlignment.Left;
                    Img.VerticalAlignment = VerticalAlignment.Bottom;
                    DockPanel.SetDock(Img, Dock.Left);
                    BitmapImage BImg = new BitmapImage();
                    BImg.BeginInit();
                    BImg.UriSource = new Uri(value);
                    BImg.EndInit();
                    Img.Source = BImg;
                    Img.Height = B.Height;
                    Img.Width = B.Width;
                    D.Children.Add(Img);
                    ++i;
                }
                GeneratedSerialNumberGrid.Children.Add(D);
            }

        }

        //Event For TextBox Change
        private void SerialNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.SerialNumber.Text == "")
            {
                serial = false;
                SerialWarning.Text = "Seri Numarası Oluşturun!";
                SerialWarning.Foreground= new SolidColorBrush(Colors.Yellow);
                ClearImage();
                return;
            }
            else
            {
                SerialNumber.Text = SerialNumber.Text.Replace(" ", "");
                if (SerialNumber.Text.Length == this.serialnumberstyle.BoxList.Count)
                {
                    int i = 0;
                    foreach (Box B in this.serialnumberstyle.BoxList)
                    {
                        if (B.IsChar)
                        {
                            if (SerialNumber.Text[i] >= 'A' && SerialNumber.Text[i] <= 'Z' && SerialNumber.Text[i] != 'X' && SerialNumber.Text[i] != 'W' && SerialNumber.Text[i] != 'Q')
                                ++i;
                            else
                            {
                                SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                                SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                                ClearImage();
                                serial = false;
                                return;
                            }
                        }
                        else
                        {
                            if (SerialNumber.Text[i] >= '0' && SerialNumber.Text[i] <= '9')
                                ++i;
                            else
                            {
                                SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                                serial = false;
                                SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                                ClearImage();
                                return;
                            }
                        }
                    }
                    SerialWarning.Text = "Seri Numarası Uygun";
                    serial = true;
                    SerialWarning.Foreground = new SolidColorBrush(Colors.Green);
                    MakeImage();
                    return;
                }
                else
                {
                    serial = false;
                    SerialWarning.Text = "Gerilen Seri Numarası Seri Numarası Stili İle Uyumsuz!";
                    SerialWarning.Foreground = new SolidColorBrush(Colors.Red);
                    ClearImage();
                    return;
                }
            }

        }
    }
}
