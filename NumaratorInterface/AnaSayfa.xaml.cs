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
using NationalInstruments.DAQmx;


namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Direct Window has four link for Operator, Report, SerialNumberStyle and SheetSetting Window
    // ===============================
    public partial class AnaSayfa : Window
    {
        private User user;
        private NationalInstruments.DAQmx.Task digitalWriteTask ;
        private DigitalSingleChannelWriter writer;
        public System.Threading.Mutex IOMut = new System.Threading.Mutex();
        bool[] dataArrayPort0 = new bool[8];
        bool[] dataArrayPort1 = new bool[8];
        bool[] dataArrayPort2 = new bool[8];
        
        //int interval = 2000;

      
    
        System.Timers.Timer timer = new System.Timers.Timer();
        
        public AnaSayfa(User user)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i != 6)
                {
                    dataArrayPort0[i] = false;
                    WriteIO(false, 0, i);
                }
                dataArrayPort1[i] = false;
                dataArrayPort2[i] = false;
                WriteIO(false, 1, i);
                WriteIO(false, 2, i);
            }
            InitializeComponent();
            dataArrayPort0[2] = true;

            //WriteIO(dataArrayPort0, 0);
            //WriteIO(dataArrayPort1, 1);
            //WriteIO(dataArrayPort2, 2);
            this.ChangeUser(user);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerTick);
            //timer.Enabled = true;
            timer.Interval = 500;
        }

        //public void WriteIO(bool[] dataArray, int portNo)
        //{
        //    IOMut.WaitOne();
        //    using (digitalWriteTask = new NationalInstruments.DAQmx.Task())
        //    {
        //        digitalWriteTask.DOChannels.CreateChannel("Dev1/Port" + portNo.ToString() + "/line0:7", "",
        //               ChannelLineGrouping.OneChannelForAllLines);
        //        //dataArray[LineNo] = data;               
        //        writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
        //        writer.WriteSingleSampleMultiLine(true, dataArray);
        //        digitalWriteTask.Dispose();
        //    }
        //    IOMut.ReleaseMutex();
        //}

        public void WriteIO(bool data, int portNo,int lineNo)
        {
            IOMut.WaitOne();
            using (digitalWriteTask = new NationalInstruments.DAQmx.Task())
            {
                digitalWriteTask.DOChannels.CreateChannel("Dev2/Port" + portNo.ToString() + "/line"+lineNo.ToString(), "",
                       ChannelLineGrouping.OneChannelForAllLines);
                //dataArray[LineNo] = data;               
                writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                writer.WriteSingleSampleSingleLine(true, data);
                digitalWriteTask.Dispose();
            }
            IOMut.ReleaseMutex();
        }
        void TimerTick(object sender, System.Timers.ElapsedEventArgs args)
        {
            dataArrayPort2[2] = !dataArrayPort2[2];
            WriteIO(dataArrayPort2[2],2, 2);
        }
       
        public void ChangeUser(User user)
        {
            this.user = user;
            if (user.getUserType() == (int)User.Users.Admin)
            {
                UserName.Text = user.UserName;
                UserAuthority.Text = "Admin";
                Newuser.IsEnabled = true;
                DeleteUser.IsEnabled = true;
                UserSettings.IsEnabled = true;
                SerailNumberDefine.Foreground = new SolidColorBrush(Colors.Orange);
                BanknoteSettings.Foreground = new SolidColorBrush(Colors.Orange);
                OperatorWindow.Foreground = new SolidColorBrush(Colors.Orange);
                FaultReport.Foreground = new SolidColorBrush(Colors.Orange);

                SerailNumberDefine.IsEnabled = true;
                BanknoteSettings.IsEnabled = true;
                OperatorWindow.IsEnabled = true;
                FaultReport.IsEnabled = true;
            }
            else if (user.getUserType() == (int)User.Users.Operator)
            {
                UserName.Text = user.UserName;
                UserAuthority.Text = "Operatör";
                Newuser.IsEnabled = false;
                DeleteUser.IsEnabled = false;
                UserSettings.IsEnabled = false;
                SerailNumberDefine.Foreground = new SolidColorBrush(Colors.Gray);
                BanknoteSettings.Foreground = new SolidColorBrush(Colors.Orange);
                OperatorWindow.Foreground = new SolidColorBrush(Colors.Orange);
                FaultReport.Foreground = new SolidColorBrush(Colors.Orange);

                SerailNumberDefine.IsEnabled = false;
                BanknoteSettings.IsEnabled = true;
                OperatorWindow.IsEnabled = true;
                FaultReport.IsEnabled = true;
            }
            else if (user.getUserType() == (int)User.Users.Service)
            {
                UserName.Text = user.UserName;
                UserAuthority.Text = "Servis";
                Newuser.IsEnabled = true;
                DeleteUser.IsEnabled = true;
                UserSettings.IsEnabled = true;
                SerailNumberDefine.Foreground = new SolidColorBrush(Colors.Orange);
                BanknoteSettings.Foreground = new SolidColorBrush(Colors.Orange);
                OperatorWindow.Foreground = new SolidColorBrush(Colors.Orange);
                FaultReport.Foreground = new SolidColorBrush(Colors.Orange);

                SerailNumberDefine.IsEnabled = true;
                BanknoteSettings.IsEnabled = true;
                OperatorWindow.IsEnabled = true;
                FaultReport.IsEnabled = true;
            }
        
        }
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i != 6)
                {
                    dataArrayPort0[i] = false;
                    WriteIO(false, 0, i);
                }
                dataArrayPort1[i] = false;
                dataArrayPort2[i] = false;
                WriteIO(false, 1, i);
                WriteIO(false, 2, i);
            }
            timer.Enabled = false;
            //WriteIO(dataArrayPort0, 0);
            //WriteIO(dataArrayPort1, 1);
            //WriteIO(dataArrayPort2, 2);
         
           // Application.Current.Windows.OfType<MainWindow>().ToList<MainWindow>().First<MainWindow>().Show();
        }

        private void SerailNumberDefineClick(object sender, RoutedEventArgs e)
        {
            if (user.getUserType() == (int)User.Users.Admin)
            {
                SerialNumberStyle window = new SerialNumberStyle();
             
                window.ShowDialog();
            }
            else if (user.getUserType() == (int)User.Users.Service)
            {
                SerialNumberStyle window = new SerialNumberStyle();
                window.ShowDialog();    
            }
            else
            {
                MessageBox.Show("Seri Numarası Stili Belirleme Yetkiniz Bulunmamaktadır!");
            }
        }
        private void BanknoteSettingsClick(object sender, RoutedEventArgs e)
        {
          //  this.Hide();
            timer.Enabled = true;
            BanknoteSettings window = new BanknoteSettings();
            dataArrayPort0[7] = true;
         
            WriteIO(dataArrayPort0[7], 0,7);
          
            window.ShowDialog();
         
            dataArrayPort0[7] = false;
            WriteIO(dataArrayPort0[7],0,7);
            timer.Enabled = false;
        }
        private void ReportWindowClick(object sender, RoutedEventArgs e)
        {
           // this.Hide();
            ReportWindow window = new ReportWindow();           
            window.ShowDialog();
        }

        private void OperatorWindowClick(object sender, RoutedEventArgs e)
        {
           // this.Hide();
            OperatorWindow window = new OperatorWindow(IOMut);
            using (digitalWriteTask )
            {
                timer.Enabled = false;
               
                    window.ShowDialog();
                    dataArrayPort0[3] = false;
                    WriteIO(dataArrayPort0[3], 0, 3);
               
            }
           // timer.Enabled = true;      
        }

        private void ChangeUser(object sender, MouseButtonEventArgs e)
        {
            ChangeUserWindow w = new ChangeUserWindow(this);
            w.ShowDialog();
        }

        private void LogOut(object sender, MouseButtonEventArgs e)
        {
          
            this.Close();
        }
        private void AddUser(object sender, MouseButtonEventArgs e)
        {
            if (this.user.getUserType() == (int)User.Users.Operator)
            {
                MessageBox.Show("Admin Ya da Servis Yetkisi Gerekli!");
                return;
            }
            else
            {
                AddUserWindow w = new AddUserWindow(this.user);
                w.ShowDialog();
                return;
            }
        }
        private void RemoveUser(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("RemoveUser");
        }

        private void ChangeUserAttributes(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("ChangeAttributes");
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = new SolidColorBrush(Colors.SaddleBrown);
            this.Cursor = Cursors.Hand;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = bggrid.Background;
            this.Cursor = Cursors.Arrow;
        }

        private void FaultReportWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReportWindowClick(null, null);
        }

        private void OperatorWindowBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OperatorWindowClick(null, null);
        }

        private void BanknoteSettingBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BanknoteSettingsClick(null, null);
        }

        private void SerialNumberBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SerailNumberDefineClick(null, null);
        }

    }
}
