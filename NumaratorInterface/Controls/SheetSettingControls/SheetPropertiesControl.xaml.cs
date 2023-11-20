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
    // AUTHOR     :  Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Controller for SheetProperties Class, 
    // ===============================
    public partial class SheetPropertiesControl : UserControl
    {
        //Events of each property change in SheetProperties
        public delegate void floatdelegate(float value);
        public delegate void intdelegate(int value);
        public event floatdelegate banknoteheightchanged;
        public event floatdelegate banknotewidthchanged;
        public event floatdelegate sheetheightchanged;
        public event floatdelegate sheetwidthchanged;
        public event intdelegate rownumberchanged;
        public event intdelegate collnumberchanged;
        //
        public SheetProperties sheetproperties; //an instance of SheetProperties
        public SheetPropertiesControl()
        {
            InitializeComponent();
            sheetproperties = new SheetProperties();
            RowNumber.Text = "8";
            CollNumber.Text = "5";
            SheetHeight.Text = "700";
            SheetWidth.Text = "820";
            BanknoteHeight.Text = "64";
            BanknoteWidth.Text = "136";
            sheetproperties.updown = true;
            FillComboBox(); 
        }

        //Sets the SerialNumberStyle of sheetproperties according to selected name in combobox
        public bool SetSerialStyle()
        { 
            NumaratorDataBase D=new NumaratorDataBase();
            SerailNumberStyle S=D.GetSerialNumberStyle(ComboBox.SelectedItem as string);
            if (S == null)
            {
                FillComboBox();
                return false;
            }
            else
            {
                sheetproperties.serialnumberstyle = S;
                return true;
            }
        }

        //Fills the Comboxbox of SerialNumberStyle
        private void FillComboBox()
        {
            ComboBox.Items.Clear();
            NumaratorDataBase D = new NumaratorDataBase();
            List<SerailNumberStyle> SNSL = new List<SerailNumberStyle>();
            D.GetSerialNumberStyles(SNSL);
            foreach (SerailNumberStyle S in SNSL)
            {
                ComboBox.Items.Add(S.SerialStyleName);
            }
        }

        //use when an input requires only int number
        private void TextBoxPreviewIntInput(object sender, TextCompositionEventArgs e)
        {
            TextBox T = sender as TextBox;
            if (T.Text.Contains(" "))
                T.Text = T.Text.Replace(" ", "");
            if (T.Text.Length > 2)
                e.Handled = true;
            foreach (char c in e.Text)
            {
                if (!(e.Text[0] >= '0' && e.Text[0] <= '9'))
                {
                    e.Handled = true;
                }
            }
        }

        //use when input requires only float number
        private void TextBoxPreviewFloatInput(object sender, TextCompositionEventArgs e)
        {
            bool iscontaincoma = false;
            TextBox T = sender as TextBox;
            if (T.Text.Contains(" "))
                T.Text = T.Text.Replace(" ", "");
            if (T.Text.Length > 6)
                e.Handled = true;
            if (T.Text.Contains(','))
                iscontaincoma = true;
            int comanumber = 0;
            foreach (char c in e.Text)
            {
                if (c == ',')
                    ++comanumber;
                if (iscontaincoma)
                {
                    if (!(c >= '0' && c <= '9'))
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    if (!((c >= '0' && c <= '9')||((c==',')&&(comanumber<2))))
                    {
                        e.Handled = true;
                    }
                }

            }
        }


        //Change rownumber property
        private void RowNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            int a;
            if (RowNumber.Text.Contains(" "))
                RowNumber.Text = RowNumber.Text.Replace(" ", "");
            if (RowNumber.Text == "")
                a = 0;
            else
            {
                a = Convert.ToInt32(RowNumber.Text);
            }
            this.sheetproperties.rownumber = a;
            if (rownumberchanged != null)
                rownumberchanged(a);
        }
        //change collnumber property
        private void CollNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            int a;
            if (CollNumber.Text.Contains(" "))
                CollNumber.Text = CollNumber.Text.Replace(" ", "");
            if (CollNumber.Text == "")
                a = 0;
            else
            {
                a = Convert.ToInt32(CollNumber.Text);
            }
            this.sheetproperties.collnumber = a;
            if (collnumberchanged != null)
                collnumberchanged(a);
        }
        //change sheetheight
        private void SheetHeightTextChanged(object sender, TextChangedEventArgs e)
        {
            float a;
            if (SheetHeight.Text.Contains(" "))
                SheetHeight.Text = SheetHeight.Text.Replace(" ", "");
            if (SheetHeight.Text == ""||SheetHeight.Text==",")
                a = 0;
            else
            {
                a = Convert.ToSingle(SheetHeight.Text);
            }
            this.sheetproperties.sheetheight = a;
            if (sheetheightchanged != null)
                sheetheightchanged(a);
        }
        //changesheetwidth
        private void SheetWidthTextChanged(object sender, TextChangedEventArgs e)
        {
            float a;
            if (SheetWidth.Text.Contains(" "))
                SheetWidth.Text = SheetWidth.Text.Replace(" ", "");
            if (SheetWidth.Text == ""||SheetWidth.Text==",")
                a = 0;
            else
            {
                a = Convert.ToSingle(SheetWidth.Text);
            }
            this.sheetproperties.sheetwidth= a;
            if (sheetwidthchanged != null)
                sheetwidthchanged(a);
        }
        //changebanknoteheight
        private void BanknoteHeightTextChanged(object sender, TextChangedEventArgs e)
        {
            float a;

            if (BanknoteHeight.Text.Contains(" "))
                BanknoteHeight.Text = BanknoteHeight.Text.Replace(" ", "");
            if (BanknoteHeight.Text == ""||BanknoteHeight.Text==",")
                a = 0;
            else
            {
                a = Convert.ToSingle(BanknoteHeight.Text);
            }
            this.sheetproperties.banknoteheight = a;
            if (banknoteheightchanged != null)
                banknoteheightchanged(a);
        }
        //changebanknotewidth
        private void BanknoteWidthTextChanged(object sender, TextChangedEventArgs e)
        {
            float a;
            if (BanknoteWidth.Text.Contains(" "))
                BanknoteWidth.Text = BanknoteWidth.Text.Replace(" ", "");
            if (BanknoteWidth.Text == ""||BanknoteWidth.Text==",")
                a = 0;
            else
            {
                a = Convert.ToSingle(BanknoteWidth.Text);
            }
            this.sheetproperties.banknotewidth = a;
            if (banknotewidthchanged != null)
                banknotewidthchanged(a);
        }
    }
}
