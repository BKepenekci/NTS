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
using NumaratorInterface.Controls.SerialNumberControls;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Combine UserControls (BoxListController, CharFolderController, DataBaseSerialContoller, SerialNumberGenerator)
    // in one UserControl
    // ===============================
    public partial class ControlSerialNumberStyle : UserControl
    {
        public ControlSerialNumberStyle()
        {
            InitializeComponent();
            this.Generator.serialnumberstyle = new SerailNumberStyle(boxlcontroller.BoxList, FolderController.FolderLocation.Text,boxlcontroller.charnumberserial,boxlcontroller.charnumbersequence);
            //Events of Each UserControl instance in this UserControl 
            this.FolderController.folderchanged += FolderController_folderchanged;
            this.Generator.serialnumberstyle.folderchanged += Serialnumberstyle_folderchanged;
            this.boxlcontroller.boxlchanged += Boxlcontroller_boxlchanged;
            this.DatabaseController.LoadEvent += DatabaseController_LoadEvent;
        }

        //Event For DataBase Load Event (Loads SerialNumberStyle, Update GUI and refresh the Generator Controller's SerailNumberStyle property )
        void DatabaseController_LoadEvent(SerailNumberStyle s)
        {
            s = new SerailNumberStyle(s.BoxList,s.CharsFolderPath,s.SerialCharNumber,s.SequenceCharNumber);
            this.boxlcontroller.BoxList = s.BoxList;
            this.FolderController.FolderLocation.Text = s.CharsFolderPath;
            this.boxlcontroller.serialcharnumber.Text = Convert.ToString(s.SerialCharNumber);
            this.boxlcontroller.sequencecharnumber.Text = Convert.ToString(s.SequenceCharNumber);
            this.Generator.serialnumberstyle = s;
            this.Generator.SerialNumber.Text = "";
            this.boxlcontroller.FillBox();
            this.DatabaseController.RefreshClick(null, null);
        }


        private void Boxlcontroller_boxlchanged()
        {
            this.Generator.MakeImage();
        }

        private void Serialnumberstyle_folderchanged()
        {
            this.Generator.FolderControl();
        }

        private void FolderController_folderchanged()
        {
            this.Generator.serialnumberstyle.CharsFolderPath = this.FolderController.FolderLocation.Text;
        }

        //Events for SaveClick, Open's a dialog for Saving or replacing the SerialNumberStyle
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (this.FolderController.load) //if folder is correct (folder is exist and all required png files are exist)
            {
                this.Generator.serialnumberstyle.SequenceCharNumber = this.boxlcontroller.charnumbersequence;
                this.Generator.serialnumberstyle.SerialCharNumber = this.boxlcontroller.charnumberserial;
                for (int i = this.boxlcontroller.charnumberserial; i < this.boxlcontroller.BoxList.Count; ++i)
                    if (this.boxlcontroller.BoxList[i].IsChar)
                    {
                        MessageBox.Show("Sıra No Kısmında Harf Olan Bir Kutu Tanımlanmaz!, Sıra No kısmında Yer Alan Kutuların Özelliklerini Rakam Olarak Değiştirin!");
                        return;
                    }
                SerialNumberSaveWindow W = new SerialNumberSaveWindow(this.Generator.serialnumberstyle);
                W.ShowDialog(); //Open a dialog or replacing the SerialNumberStyle
            }
            else
            {
                MessageBox.Show("Uygun Seri Numarası Karakterleri Klasörü Seçili Değil");
            }
        }
    }
}
