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
    // PURPOSE     : in This project used as dialog window. Adds User to Database
    // ===============================
    public partial class AddUserWindow : Window
    {
        private User user;
        public AddUserWindow(User user)
        {
            InitializeComponent();
            this.user = user;
        }

        //Add User Alg.
        private void AddUser(object sender, RoutedEventArgs e)
        {
            if(usertypes.SelectedItem==null)
            {
                MessageBox.Show("Kullanıcı Tipini Seçin");
                return;
            }
            else if (this.user.getUserType() == (int)User.Users.Operator)
            {
                MessageBox.Show("Kullanıcı Eklemek İçin Yetkiniz Bulunmamakta!");
                return;
            }
            else if (UserName.Text.Length < 5)
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
            else if ((this.user.getUserType() == (int)User.Users.Admin))
            {
                if(((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Servis"))
                {
                    MessageBox.Show("Admin Kullanıcı Servis Kullanıcısı Oluşturamaz!");
                    return;
                }
                NumaratorDataBase D = new NumaratorDataBase();
                if (D.IsUserExist(UserName.Text))
                {
                    MessageBox.Show("Bu İsimde Bir Kullanıcı Mevcut!");
                    return;
                }
                User user = new User();
                user.UserName = UserName.Text;
                if (((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Admin"))
                    user.setUserType(User.Users.Admin);
                else if (((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Operatör"))
                    user.setUserType(User.Users.Operator);
                D.InsertUser(user, pw1.Password);
                MessageBox.Show("Kullanıcı Oluşturuldu");
            }
            else if ((this.user.getUserType() == (int)User.Users.Service))
            {
                NumaratorDataBase D = new NumaratorDataBase();
                if (D.IsUserExist(UserName.Text))
                {
                    MessageBox.Show("Bu İsimde Bir Kullanıcı Mevcut!");
                    return;
                }
                User user = new User();
                user.UserName = UserName.Text;
                if (((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Admin"))
                    user.setUserType(User.Users.Admin);
                else if (((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Operatör"))
                    user.setUserType(User.Users.Operator);
                else if (((ComboBoxItem)(usertypes.SelectedItem)).Content.Equals("Servis"))
                    user.setUserType(User.Users.Service);
                D.InsertUser(user, pw1.Password);
                MessageBox.Show("Kullanıcı Oluşturuldu");
            }
        }
        
        //closes dialog
        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
