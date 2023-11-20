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
using System.Timers;

namespace NumaratorInterface
{
    /// <summary>
    ///  Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        public TimerWindow()
        {
            InitializeComponent();
            this.Dispatcher.Invoke((Action)(() => { wait(); }));
        }
        public void wait()
        {
            for (int i = 0; i < 999999999; ++i)
                ;
            button.Visibility = Visibility.Visible;
            WarningText.Text = "İşlemler Durduruldu, Devam Etmek için Kapat Tuşuna Basınız!";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
