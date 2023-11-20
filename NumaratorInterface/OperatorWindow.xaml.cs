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
using System.Threading;
using DALSA.SaperaLT.SapClassBasic;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Contains the "ControlOperator" UserControl. When it is closed Direct Window is showed.
    // ===============================
    public partial class OperatorWindow : Window
    {
        public OperatorWindow(System.Threading.Mutex IOMut)
        {
            InitializeComponent();
            this.ControlOperator.SettingsControl.IOMut = IOMut;
        }

     
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
     
                if (this.ControlOperator.SettingsControl.Xfer != null)
                {
                    if (this.ControlOperator.SettingsControl.isrunning && this.ControlOperator.SettingsControl.StopCont.Content as string == "Durdur")
                    {
                        this.ControlOperator.SettingsControl.Xfer.Freeze();
                        this.ControlOperator.SettingsControl.Xfer.Wait(1000);

                        this.ControlOperator.SettingsControl.StopCont.Content = "Devam Et";
                        this.ControlOperator.SettingsControl.StopCont.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    while (this.ControlOperator.SettingsControl.NumCount != 0 || this.ControlOperator.SettingsControl.ImgCount != 0)
                        ;
                    TimerWindow TW = new TimerWindow();
                    TW.ShowDialog();
                    //this.ControlOperator.SettingsControl.StopCont.Content = "Devam Et";
                    //this.ControlOperator.SettingsControl.StopCont.Foreground = new SolidColorBrush(Colors.Green);

                    //this.ControlOperator.SettingsControl.Xfer.Freeze();
                    //this.ControlOperator.SettingsControl.Xfer.Wait(1000);
                    this.ControlOperator.SettingsControl.DestroyObjects();
                 
                   
                    this.ControlOperator.FalseSheetControl.ClearMemory();
                    this.ControlOperator.SettingsControl.FinalizeSignals();
                    if (this.ControlOperator.SettingsControl.isLoaded)
                        this.ControlOperator.SettingsControl.SaveParameters(this.ControlOperator.SettingsControl.ParameterFileName);
                }
            }
            catch { }
          
            //System.Windows.Application.Current.Windows.OfType<AnaSayfa>().ToList<AnaSayfa>().First<AnaSayfa>().Show();
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
