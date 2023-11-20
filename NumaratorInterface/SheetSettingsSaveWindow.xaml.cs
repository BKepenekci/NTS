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
using DALSA.SaperaLT.SapClassBasic;
using System.Runtime.InteropServices;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Dialog Window that saves or Replace the SheetSettings.
    // ===============================
    public partial class SheetSettingsSaveWindow : Window
    {
        SheetSettings SS = new SheetSettings();
        NumaratorDataBase D = new NumaratorDataBase();
        NumaratorDllWrapper NumDll = null;
        DALSA.SaperaLT.SapClassBasic.SapBuffer buffer;
        public SheetSettingsSaveWindow(SheetProperties SP, SerialNumberPositions SNP, Rect subImageRect, DALSA.SaperaLT.SapClassBasic.SapBuffer buffer, List<IntPoint> TPL)
        {
            InitializeComponent();
            this.SS.serialnumberpositions = SNP;
            this.SS.sheetproperties = SP;
            this.SS.templateHeight = (int)subImageRect.Height;
            this.SS.templateWidth = (int)subImageRect.Width;
            this.SS.templatePoint = TPL;
            this.buffer = buffer;
            NumDll = new NumaratorDllWrapper();
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (this.SheetSettingName.Text == "")
            {
                MessageBox.Show("Tabaka Ayarı İsmini Girin!");
                return;
            }
            else if (!this.D.IsSheetSettingExist(this.SheetSettingName.Text))
            {
                this.SS.settingname = this.SheetSettingName.Text;
                if (D.SaveSheetSetting(this.SS))
                {
                    if (!System.IO.Directory.Exists("subTemplateDirectory"))
                    {
                        System.IO.Directory.CreateDirectory("subTemplateDirectory");
                    }
                    int i = 0;
                    foreach(IntPoint p in this.SS.templatePoint)
                    {
                        IntPtr dptr = Marshal.AllocHGlobal(SS.templateWidth * SS.templateHeight * buffer.BytesPerPixel);
                        buffer.ReadRect(p.X, p.Y, SS.templateWidth, SS.templateHeight, dptr);
                        //string tmp = NumDll.GetSerialString(NumDll._serialStr, NumDll._serialNum, p.row, p.col, NumDll._numDgtCnt, NumDll._tabakaCnt, NumDll._rowCnt, NumDll._colCnt);
                        NumDll.SaveInput(dptr, SS.templateWidth, SS.templateHeight, "subTemplateDirectory\\" + SS.settingname +i.ToString() +".png");
                        Marshal.FreeHGlobal(dptr);
                        ++i;
                    }

                    MessageBox.Show("Tabaka Ayarı Kaydedildi");
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
                ApproveWindow W = new ApproveWindow("Girilen İsimde Tabaka Ayarı Mevcut!\nKayıtlı Tabaka Üzerine Kaydedilsin mi?");
                W.Height = W.Height + 10;
                W.ShowDialog();
                if ((bool)W.DialogResult)
                {
                    this.SS.settingname = this.SheetSettingName.Text;
                    if (D.ReplaceSheetSetting(this.SS))
                    {
                        if (!System.IO.Directory.Exists("subTemplateDirectory"))
                        {
                            System.IO.Directory.CreateDirectory("subTemplateDirectory");
                        }
                        int i = 0;
                        foreach (IntPoint p in this.SS.templatePoint)
                        {
                            IntPtr dptr = Marshal.AllocHGlobal(SS.templateWidth * SS.templateHeight * buffer.BytesPerPixel);
                            buffer.ReadRect(p.X, p.Y, SS.templateWidth, SS.templateHeight, dptr);
                            //string tmp = NumDll.GetSerialString(NumDll._serialStr, NumDll._serialNum, p.row, p.col, NumDll._numDgtCnt, NumDll._tabakaCnt, NumDll._rowCnt, NumDll._colCnt);
                            NumDll.SaveInput(dptr, SS.templateWidth, SS.templateHeight, "subTemplateDirectory\\" + SS.settingname + i.ToString() + ".png");
                            Marshal.FreeHGlobal(dptr);
                            ++i;
                        }

                        //NumDll.SaveInput(intp, subImageRect.width, subImageRect.height, "C:\\Users\\User\\Documents\\samples\\" + tmp + p.index.ToString() + ".png");
                        MessageBox.Show("Tabaka Ayarı Değiştirildi!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Kayıt Değiştirme İşlemi Başarısız!");
                        this.Close();
                    }
                }
                    
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
