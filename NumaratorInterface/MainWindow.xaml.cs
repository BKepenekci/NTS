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
using DALSA.SaperaLT.SapClassBasic;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Initial Window of the Program. User has to enter username and password to login.
    // ===============================
    public partial class MainWindow : Window
    {
        private NationalInstruments.DAQmx.Task digitalWriteTask;
        private NationalInstruments.DAQmx.DigitalSingleChannelWriter writer;
        public System.Threading.Mutex IOMut; //Mutex for Signal Write
        //System.Timers.Timer deleteTimer = new System.Timers.Timer(60000);
        //System.Diagnostics.Process process1 = new System.Diagnostics.Process();
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
   
         public MainWindow()
        {
            InitializeComponent();
            CheckUsers();
            
           // StartToDEtectShoudown();
            WriteIO(false, 2, 5);
            WriteIO(true, 0, 2);
            //deleteTimer.Elapsed += new System.Timers.ElapsedEventHandler(DeleteFiles);
                
            //System.Diagnostics.ProcessStartInfo sinfo1 = new System.Diagnostics.ProcessStartInfo();
            //sinfo1.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //sinfo1.FileName = "deleteFilesCorrectSheet.bat";
            //process1.StartInfo = sinfo1;

            System.Diagnostics.ProcessStartInfo sinfo2 = new System.Diagnostics.ProcessStartInfo();
            sinfo2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            sinfo2.FileName = "deleteFilesFalseSheet.bat";
            process2.StartInfo = sinfo2;

            //deleteTimer.Start();
        }

      
         //private void DeleteFiles(object sender, System.Timers.ElapsedEventArgs e)
         //{
           
         //    process1.Start();            
         //    process2.Start();            
         //}
    
        public void CheckUsers()
        {
            NumaratorDataBase D = new NumaratorDataBase();
            if (!D.IsDBHasUser())
            {
                LoginBorder.Visibility = Visibility.Collapsed;
                newUSer.Visibility = Visibility.Visible;
            }
            else
            {
                LoginBorder.Visibility = Visibility.Visible;
                newUSer.Visibility = Visibility.Collapsed;
            }
        }
        private void Login(object sender, RoutedEventArgs e)
        {
            NumaratorDataBase D = new NumaratorDataBase();
            User user=D.GetUser(userbox.Text, paswordbox.Password);
            if (user == null)
            {
                MessageBox.Show("Şifre ya da Kullanıcı Adı Yanlış!");
                return;
            }
            else
            {
                AnaSayfa main = new AnaSayfa(user);
                paswordbox.Password = "";             
                main.ShowDialog();
             
               // this.Hide();
            }
        }
        private void CreateUser(object sender, RoutedEventArgs e)
        {
            if (UserName.Text.Length < 5)
            {
                MessageBox.Show("Kullanıcı Adı 5 Haneliden Küçük Olamaz!");
                return;
            }
            else if (pw1.Password.Length < 5)
            {
                MessageBox.Show("Şifre 5 Haneliden Küçük Olamaz!");
                return;
            }
            else if (!pw1.Password.Equals(pw2.Password))
            {
                MessageBox.Show("Girilen Şifreler Birbirinden Farklı!");
                return;
            }
            NumaratorDataBase D = new NumaratorDataBase();
            if (D.IsUserExist(UserName.Text))
            {
                MessageBox.Show("Bu İsimde Bir Kullanıcı Mevcut!");
                return;
            }
            User user=new User();
            user.UserName=UserName.Text;
            user.setUserType(User.Users.Service);
            D.InsertUser(user,pw1.Password);
            CheckUsers();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.UpdateLayout();
            //System.IO.Ports.SerialPort sp = new System.IO.Ports.SerialPort("COM4");

            //try
            //{
            //    this.Cursor = Cursors.Wait;
            //    sp.Open();
            //    byte[] d = { 27 };
            //    sp.Write(d, 0, 1);
            //    byte[] data = { 114, 99, 13 };
            //    byte[] dataenter = { 13 };
            //    sp.Write(data, 0, 3);
            //    sp.Close();
            //    System.Threading.Thread.Sleep(5000);
            //    this.Cursor = Cursors.Arrow;                
            //}
            //catch (Exception ec)
            //{
            //    System.Windows.MessageBox.Show(ec.Message);
            //}
            //finally
            //{
            //    // Acq.Destroy();
            //}            
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            //ShutDownSignalDetectionTask.Stop();
            //ShutDownSignalDetectionTask.Dispose();
            this.Close();
        }
        //ForNI
        //Shutdown signal event

        //private NationalInstruments.DAQmx.Task ShutDownSignalDetectionTask;
        //private NationalInstruments.DAQmx.DigitalSingleChannelReader DigitalReader2;
        //private void StartToDEtectShoudown()
        //{
        //    try
        //    {
        //        // Create the task.
        //        ShutDownSignalDetectionTask = new NationalInstruments.DAQmx.Task();

        //        // Create channel
        //        ShutDownSignalDetectionTask.DIChannels.CreateChannel("Dev2/port0/line6",
        //            "",
        //            NationalInstruments.DAQmx.ChannelLineGrouping.OneChannelForAllLines);

        //        // Configure digital change detection timing
        //        ShutDownSignalDetectionTask.Timing.ConfigureChangeDetection(
        //            "Dev2/port0/line6",
        //            "",
        //            NationalInstruments.DAQmx.SampleQuantityMode.ContinuousSamples, 200);

        //        // Add the digital change detection event handler
        //        // Use SynchronizeCallbacks to specify that the object 
        //        // marshals callbacks across threads appropriately.
        //        ShutDownSignalDetectionTask.SynchronizeCallbacks = true;

        //        ShutDownSignalDetectionTask.DigitalChangeDetection += new NationalInstruments.DAQmx.DigitalChangeDetectionEventHandler(ShutDownEventHandler);

        //        // Create the reader
        //        DigitalReader2 = new NationalInstruments.DAQmx.DigitalSingleChannelReader(ShutDownSignalDetectionTask.Stream);

        //        // Start the task
        //        ShutDownSignalDetectionTask.Start();
        //    }
        //    catch (NationalInstruments.DAQmx.DaqException exception)
        //    {
        //        ShutDownSignalDetectionTask.Dispose();

        //        MessageBox.Show(exception.Message);
        //    }
        //}

        public void WriteIO(bool data, int portNo, int lineNo)
        {
                using (digitalWriteTask = new NationalInstruments.DAQmx.Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("Dev2/Port" + portNo.ToString() + "/line" + lineNo.ToString(), "",
                           NationalInstruments.DAQmx.ChannelLineGrouping.OneChannelForAllLines);
                    //dataArray[LineNo] = data;               
                    writer = new NationalInstruments.DAQmx.DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleSingleLine(true, data);
                    digitalWriteTask.Dispose();
                }
            
        }
        private void ShutDownEventHandler(object sender, NationalInstruments.DAQmx.DigitalChangeDetectionEventArgs e)
        {           
            App.Current.Shutdown();
            WriteIO(true, 2, 5);
            WriteIO(false, 0, 2);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            //string ServerName = "Xtium-CL_PX4_1";
            //string GrabberConfigFileName = "Cam\\grabbercamera.ccf";
            //SapLocation loc = new SapLocation(ServerName, 0);

            //SapAcquisition Acq = new SapAcquisition(loc, GrabberConfigFileName);
            //Acq.Create();
            //string s = Acq.SerialPortName;
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            process2.Start();            
            process2.WaitForExit();
        }
    }
}
