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
using System.Windows.Shapes;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Dialog Window that saves or Replace the SerialNumberStyle.
    // ===============================
    public partial class SerialNumberSaveWindow : Window
    {
        SerailNumberStyle S;
        NumaratorDataBase D = new NumaratorDataBase();
        public SerialNumberSaveWindow(SerailNumberStyle s)
        {
            InitializeComponent();
            this.S = s;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (this.SerialName.Text == "")
            {
                MessageBox.Show("Seri Numarası Stilinin İsmini Girin!");
                return;
            }
            else if (!this.D.IsSerialNumberExist(this.SerialName.Text))
            {
                if (D.SaveSerialNumberStyle(S, this.SerialName.Text))
                {
                    MessageBox.Show("Seri Numarası Stili Kaydedildi");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Kayıt İşlemi Başarısız!");
                    this.Close();
                }
            }
            else
            {
                ApproveWindow W = new ApproveWindow("Gerilen İsimde Bir Seri Numarası Stili Mevcut!\nSeri Numarası Stili Mevcut Olan Üzerine Kaydedilsin mi?");
                W.Title = "Seri Numarası Stili Kaydetmeyi Onayla!";
                W.Height = W.Height + 10;
                W.ShowDialog();
                if ((bool)W.DialogResult)
                {
                    D.ReplaceSerialNumberStyle(S, this.SerialName.Text);
                    this.Close();
                }
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
