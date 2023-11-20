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

namespace NumaratorInterface.Controls.SheetSettingControls
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Database Controller of SheetSetting, (Delete and load events)
    // ===============================
    public partial class DataBaseSheetSettingController : UserControl
    {
        public delegate void LoadDelegate(SheetSettings s);
        public event LoadDelegate LoadEvent;
        List<SheetSettings> SSL; //list of SheetSetting in Database
        public DataBaseSheetSettingController()
        {
            InitializeComponent();
            this.SSL = new List<SheetSettings>();
            FillList(); //Get List of SheetSetting from the Database
            FillTreeView();
        }

        //Gets the List of SheetSetting from the Database
        public void FillList()
        {
            NumaratorDataBase D = new NumaratorDataBase();
            D.GetSheetSettings(this.SSL);
        }

        //Updates GUI (add names buttons and buttons' events (delete & load))
        public void FillTreeView()
        {
            databasetreeview.Items.Clear();
            foreach (SheetSettings s in this.SSL)
            {
                TreeViewItem TVI = new TreeViewItem();
                TVI.Margin = new Thickness(5, 5, 5, 0);
                //Create Stack Panel//
                StackPanel Sp = new StackPanel();
                Sp.Height = 25;
                Sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                Sp.Orientation = Orientation.Horizontal;

                //Create TextBlock//
                TextBlock TB = new TextBlock();
                TB.Text = s.settingname + "   ";
                TB.VerticalAlignment = VerticalAlignment.Bottom;
                TB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                TB.VerticalAlignment = VerticalAlignment.Center;
                TB.Margin = new Thickness(3, 3, 3, 3);

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
                Sp.Children.Add(Load);
                Sp.Children.Add(Erase);

                TVI.Header = Sp;
                databasetreeview.Items.Add(TVI);
            }

        }

        //deletes the SheetSetting
        void Erase_Click(object sender, RoutedEventArgs e)
        {
            ApproveWindow W = new ApproveWindow(((SheetSettings)((sender as Button).Tag)).settingname + " Adlı Tabaka Ayarını Silmek İstediğinize Emin misiniz?");
            if (W.ShowDialog() == true)
            {
                NumaratorDataBase D = new NumaratorDataBase();
                D.DeleteSheetSetting(((SheetSettings)((sender as Button).Tag)).settingname);
                FillList();
                FillTreeView();
            }
        }

        //Load event
        void Load_Click(object sender, RoutedEventArgs e)
        {
            if (LoadEvent != null)
            {
                LoadEvent((SheetSettings)((sender as Button).Tag));
            }
        }

        //Refreshes the DatabaseController (refill the list and update GUI)
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            FillList();
            FillTreeView();
        }
    }
}

