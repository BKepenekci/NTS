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
using NumaratorInterface.Controls.SheetSettingControls;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Holds The ControlSheetSettings UserControl. When it is closed it shows the "AnaSayfa" (DirectWindow), Also Destroys the 
    // Cam Related Objects
    // ===============================
    public partial class BanknoteSettings : Window
    {
        public BanknoteSettings()
        {
            InitializeComponent();
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((SerialNumberPositionsControl)(this.Controls.SerialNbrView.Children[0])).Xfer.Freeze();
            ((SerialNumberPositionsControl)(this.Controls.SerialNbrView.Children[0])).Xfer.Wait(1000);
            ((SerialNumberPositionsControl)(this.Controls.SerialNbrView.Children[0])).DestroyObjects();
           // System.Windows.Application.Current.Windows.OfType<AnaSayfa>().ToList<AnaSayfa>().First<AnaSayfa>().Show();
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
         
            this.Close();
        }
      
     
    }
}
