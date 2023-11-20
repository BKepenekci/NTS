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
    // PURPOSE     : Control of Folder which holds the *.png files of Numbers and Letters
    // ===============================

    public partial class CharFolderController : UserControl
    {
        public delegate void delegatefolderpathchanged();
        public event delegatefolderpathchanged folderchanged;
        public bool load = false;
        private string letters = "ABCDEFGHIJKLMNOPRSTUVYZ";
        public CharFolderController()
        {
            InitializeComponent();
        }

        //Folder SelectionEvent
        private void FolderSelect(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            FolderLocation.Text = dialog.SelectedPath;
        }

        //Event for FolderLocationChanged
        private void FolderLocationChanged(object sender, TextChangedEventArgs e)
        {
            if (folderchanged != null)
            {
                folderchanged();
            }
            LetterDockPanel.Children.Clear(); //Clear the LetterDockPanel
            NumberDockPanel.Children.Clear(); //Clear the NumberDockPanel
            //if there is no such directory
            if (!System.IO.Directory.Exists(FolderLocation.Text))
            {
                FolderWarning.Foreground = new SolidColorBrush(Colors.Red);
                FolderWarning.Text = "Belirtilen Klasör Mevcut Değil!";
                load = false;
                return;
            }
            else
            {
                string[] s=System.IO.Directory.GetFiles(FolderLocation.Text);
                //check and get number chars//
                for(int i=0;i<10;++i) //for each number add the png file to Dockpanel
                {
                    string value = FolderLocation.Text + "\\" + Convert.ToString(i) + ".png";
                    if ((s.Contains<string>(value)))
                    {
                        Image Img = new Image();
                        if (i == 0)
                            Img.Margin = new System.Windows.Thickness { Left = 30, Bottom = 10 };
                        else
                            Img.Margin = new System.Windows.Thickness { Left=5,Bottom=10};
                        Img.HorizontalAlignment = HorizontalAlignment.Left;
                        DockPanel.SetDock(Img, Dock.Left);
                        BitmapImage BImg = new BitmapImage();
                        BImg.BeginInit();
                        BImg.UriSource = new Uri(value);
                        BImg.EndInit();
                        Img.Source = BImg;
                        NumberDockPanel.Children.Add(Img);
                    }
                    else //else part is stand for if any png file of any number is missing 
                    {
                        FolderWarning.Text = Convert.ToString(i) + ".png Dosyası Mevcut Değil, Klasörü Kontrol Edin!";
                        FolderWarning.Foreground = new SolidColorBrush(Colors.Red);
                        LetterDockPanel.Children.Clear();//Clear the LetterDockPanel
                        NumberDockPanel.Children.Clear();//Clear the NumberDockPanel
                        load = false;
                        return;
                    }
                }
                //check and get letterchars//
                foreach (char c in letters)
                {
                    string value = FolderLocation.Text + "\\" + c + ".png";
                    if ((s.Contains<string>(value)))
                    {
                        Image Img = new Image();
                        if (c == 'A')
                            Img.Margin = new System.Windows.Thickness { Left = 30, Bottom = 10 };
                        else
                            Img.Margin = new System.Windows.Thickness { Left = 5, Bottom = 10 };
                        Img.HorizontalAlignment = HorizontalAlignment.Left;
                        DockPanel.SetDock(Img, Dock.Left);
                        BitmapImage BImg = new BitmapImage();
                        BImg.BeginInit();
                        BImg.UriSource = new Uri(value);
                        BImg.EndInit();
                        Img.Source = BImg;
                        LetterDockPanel.Children.Add(Img);
                    }
                    else //else part is stand for if any png file of any letter is missing 
                    {
                        FolderWarning.Text = c + ".png Dosyası Mevcut Değil, Klasörü Kontrol Edin!";
                        FolderWarning.Foreground = new SolidColorBrush(Colors.Red);
                        LetterDockPanel.Children.Clear();
                        NumberDockPanel.Children.Clear();
                        load = false;
                        return;
                    }
                }
                FolderWarning.Text = "Karakter Dosyaları Başarıyla Yüklendi!";
                FolderWarning.Foreground = new SolidColorBrush(Colors.Green);
                load = true;
            }
        }
    }
}
