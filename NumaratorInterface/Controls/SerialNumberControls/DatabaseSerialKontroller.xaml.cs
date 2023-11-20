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
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Database Controller of SerialNumberStyle, (Delete and load events)
    // ===============================
    public partial class DatabaseSerialKontroller : UserControl
    {
        public delegate void LoadDelegate(SerailNumberStyle s);
        public event LoadDelegate LoadEvent;
        List<SerailNumberStyle> SNSL;
        public DatabaseSerialKontroller()
        {
            InitializeComponent();
            this.SNSL = new List<SerailNumberStyle>();
            FillList();
            FillTreeView();
        }

        //SNSL property is filled by reading from the database
        private void FillList()
        {
            NumaratorDataBase D = new NumaratorDataBase();
            D.GetSerialNumberStyles(this.SNSL);
        }

        //using SNSL property update the GUI (add names, buttons (delete and load buttons) and rectangles for preview)
        private void FillTreeView()
        {
            databasetreeview.Items.Clear();
            foreach(SerailNumberStyle s in this.SNSL)
            {
                TreeViewItem TVI=new TreeViewItem();
                TVI.Margin = new Thickness(5, 5, 5, 0);
                //Create Stack Panel//
                StackPanel Sp = new StackPanel();
                Sp.Height = 25;
                Sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                Sp.Orientation = Orientation.Horizontal;

                //Create TextBlock// (name of Style)
                TextBlock TB = new TextBlock();
                TB.Text = s.SerialStyleName+"  : ";
                TB.VerticalAlignment = VerticalAlignment.Bottom;
                TB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                TB.VerticalAlignment = VerticalAlignment.Center;
                TB.Margin = new Thickness(3, 3, 3, 3);

                //Create Dockpanel for Small Style Represantation//
                DockPanel D = new DockPanel();
                D.Height = 25;
                D.Width = 200;
                foreach (Box B in s.BoxList)
                {
                    Rectangle R = new Rectangle();
                    R.Width = B.Width/2;
                    R.Height = B.Height/2;
                    R.Stroke = new SolidColorBrush(Colors.Black);
                    if (B.IsChar)
                        R.Fill = new SolidColorBrush(Colors.Green);
                    else
                        R.Fill = new SolidColorBrush(Colors.Orange);
                    R.Margin = new Thickness { Right = B.Ofset/2 };
                    R.VerticalAlignment = VerticalAlignment.Bottom;
                    R.HorizontalAlignment = HorizontalAlignment.Left;
                    D.Children.Add(R);
                }
                //Buttons//
                Button Load = new Button();
                Load.Content = "Yükle";
                Load.Height = 20;
                Load.Width = 50;
                Load.Margin = new Thickness(5, 0, 5, 0);
                Load.Tag = s;
                Load.Click += Load_Click;
                Button Erase = new Button();
                Erase.Tag = s;
                Erase.Content = "Sil";
                Erase.Height = 20;
                Erase.Width = 50;
                Erase.Click += Erase_Click;

                //Add items to Stackpanel
                Sp.Children.Add(TB);
                Sp.Children.Add(D);
                Sp.Children.Add(Load);
                Sp.Children.Add(Erase);

                TVI.Header = Sp;
                databasetreeview.Items.Add(TVI);
            }

        }

        //Delete Event
        void Erase_Click(object sender, RoutedEventArgs e)
        {
            ApproveWindow W = new ApproveWindow(((SerailNumberStyle)((sender as Button).Tag)).SerialStyleName+ " Adlı Seri Numarası Stilini Silmek İstediğinize Emin misiniz?");
            if (W.ShowDialog()==true)
            {
                NumaratorDataBase D = new NumaratorDataBase();
                try
                {
                    D.DeleteSerialNumberStyle(((SerailNumberStyle)((sender as Button).Tag)).SerialStyleName);
                    FillList();
                    FillTreeView();
                }
                catch
                {
                    MessageBox.Show("Silinmek istenen Seri Numarası Başka Bir Tabaka Ayrında Kullanılmaktadır. Öncelikli Olarak Tabaka Ayrının Silinmesi Gereklidir.");
                }
                
            }
        }

        //LoadEvent
        void Load_Click(object sender, RoutedEventArgs e)
        {
            if (LoadEvent != null)
            {
                LoadEvent((SerailNumberStyle)((sender as Button).Tag));
            }
        }

        //Refresh the view ( rereads the database and refill the list )
        public void RefreshClick(object sender, RoutedEventArgs e)
        {
            FillList();
            FillTreeView();
        }
    }
}
