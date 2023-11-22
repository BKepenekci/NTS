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
    // UPDATE DATxE     : 20.08.2016
    // PURPOSE     : Dialog Window that changes the User Type. User is Required to enter username and Password.
    // ===============================
    public partial class ChangeUserWindow : Window
    {
        private AnaSayfa window;
        public ChangeUserWindow(AnaSayfa window)
        {
            InitializeComponent();
            this.window = window;
        }

        private void ChangeUser(object sender, RoutedEventArgs e)
        {
            NumaratorDataBase D = new NumaratorDataBase();
            User user = D.GetUser(userbox.Text, paswordbox.Password);
            if (user == null)
            {
                MessageBox.Show("Şifre ya da Kullanıcı Adı Yanlış!");
                return;
            }
            else
            {
                this.window.ChangeUser(user);
                this.Close();
            }
        }
        
    }
}
