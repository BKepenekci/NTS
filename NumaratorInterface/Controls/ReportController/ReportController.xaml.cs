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

namespace NumaratorInterface.Controls.ReportController
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU 
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Gives statistics according to collected datas in DataBase
    // ===============================
    public partial class ReportController : UserControl
    {
        public DateTime ReportStartDate;
        public DateTime ReportEndDate;
        public List<string> namesofSettings = new List<string>();
        public ReportController()
        {
            InitializeComponent();
            StartDate.SelectedDate = DateTime.Now;
            EndDate.SelectedDate = DateTime.Now;
            FillSettingTreeView();
            Report(null, null);
        }

        //Fills Setting TreeView
        public void FillSettingTreeView()
        {
            NumaratorDataBase D = new NumaratorDataBase();
            List<SheetSettings> SSL=new List<SheetSettings>();
            D.GetSheetSettings(SSL);
            foreach (SheetSettings SS in SSL)
            {
                TreeViewItem TVI = new TreeViewItem();
                StackPanel SP = new StackPanel();
                DockPanel DP = new DockPanel();
                CheckBox C = new CheckBox();
                
                SP.Height = 20;
                C.Height = 15;
                C.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                C.Width = 20;

                TextBlock T = new TextBlock();
                T.Height = 20;
                T.FontSize = 16;
                T.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                T.Text = SS.settingname;
                C.Tag = T;
                C.Checked+=C_Checked;
                C.Unchecked+=C_Unchecked;
                DP.Children.Add(C);
                DP.Children.Add(T);
                SP.Children.Add(DP);
                C.IsChecked = true;
                TVI.Header = SP;
                SettingTreeView.Items.Add(TVI);
            
            }
        }

        private void C_Checked(object sender, RoutedEventArgs e)
        {
            string s=((TextBlock)(sender as CheckBox).Tag).Text;
            if (!namesofSettings.Contains(s))
            {
                namesofSettings.Add(s);
            }
        }

        private void C_Unchecked(object sender, RoutedEventArgs e)
        {
            string s = ((TextBlock)(sender as CheckBox).Tag).Text;
            if (namesofSettings.Contains(s))
            {
                namesofSettings.Remove(s);
            }
        }
        //Event for "Raporla" Button
        private void Report(object sender, RoutedEventArgs e)
        {
            if (StartDate.SelectedDate == null)
            {
                MessageBox.Show("Rapor Başlangıç Tarihini Seçin");
                return;
            }
            if (EndDate.SelectedDate == null)
            {
                MessageBox.Show("Rapor Bitiş Tarihini Seçin");
                return;
            }
            DateTime Start = (DateTime)StartDate.SelectedDate;
            ReportStartDate = Start;
            DateTime End = (DateTime)EndDate.SelectedDate;
            ReportEndDate = End;
            if (Start > End)
            {
                MessageBox.Show("Başlangıç Tarihi Bitiş Tarihinden Sonra Olamaz!");
                return;
            }
            int NumberOfSheet = 0;
            int NumberOfFalseSheet=0;
            int red= 0;
            int yellow = 0;
            int violet = 0;
            int blue = 0;
            int gray = 0;
            int green = 0;
            NumaratorDataBase D = new NumaratorDataBase();
            List<Sheet> SL=D.GetSheetsBetween(Start, End);
            List<SheetSettings> LSS=new List<SheetSettings>();

            foreach (string s in namesofSettings)
            {
                LSS.Add(D.GetSheetSetting(s));
            }

            foreach (Sheet s in SL)
            {
                if (namesofSettings.Contains(s.settingname))
                {
                    SheetSettings SS = LSS.Find(o => o.settingname == s.settingname);
                    green += SS.sheetproperties.rownumber * SS.sheetproperties.collnumber * 2;
                    ++NumberOfSheet;
                }
            }
            foreach (Sheet s in SL)
            {
                if(namesofSettings.Contains(s.settingname))
                {
                    if (s.IsFalse)
                        ++NumberOfFalseSheet;
                    int tf1=0, tf2=0, tf3=0, tf4=0, tf5 =0, c=0;
                    D.GetFaultNumber(ref tf1,ref tf2,ref tf3, ref tf4, ref tf5,ref c, s.SheetNumber);
                    red = red + tf1;
                    yellow = yellow + tf2;
                    violet = violet + tf3;
                    blue = blue + tf4;
                    gray = gray + tf5;
                }
            }
            green = green - (red + yellow + violet + blue + gray);
            tm1.Text = Convert.ToString(red);
            tm2.Text = Convert.ToString(yellow);
            tm3.Text = Convert.ToString(violet);
            tm4.Text = Convert.ToString(blue);
            tm5.Text = Convert.ToString(gray);
            cn.Text = Convert.ToString(green);
            TotalNumberFault.Text = Convert.ToString(red + yellow + violet + blue + gray + green);
            TotalSheet.Text = Convert.ToString(NumberOfSheet);
            FalseSheet.Text = Convert.ToString(NumberOfFalseSheet);
            CorrectSheet.Text = Convert.ToString(NumberOfSheet - NumberOfFalseSheet);
            if (NumberOfSheet != 0)
                FalsePercent.Text = "%" + Convert.ToString((double)NumberOfFalseSheet / (double)NumberOfSheet * 100);
            else
                FalsePercent.Text = "%";
            //ZedGraph.GraphPane GP = graph.GraphPane;
            //GP.Title.Text = "Günlere Göre Basılan Tabaka Sayısı";
            //ZedGraph.PointPairList PressedSheet = new ZedGraph.PointPairList();
            //int i=0;
            //while (Start <= End)
            //{

            //    List<Sheet> subSL=SL.FindAll(o => o.sheettime > Start && o.sheettime < Start + new TimeSpan(1, 0, 0, 0));
            //    Start = Start + new TimeSpan(1, 0, 0, 0);
            //    PressedSheet.Add(i,subSL.Count);
            //    ++i;
            //}
            //GP.CurveList.Clear();
            //ZedGraph.LineItem Pressedcurve = GP.AddCurve("Basılan", PressedSheet, System.Drawing.Color.Blue, ZedGraph.SymbolType.Circle);
            
            //graph.AxisChange();
            //graph.ZoomPane(GP, 1, new System.Drawing.PointF(0, 0), false);
            UpDatePieChart(red, yellow,violet,blue, gray,green);
        }

        //Uses ZedGraph Library to draw piechard 
        //tm1: number of -1 (false number)errors
        //tm2: number of -2 (weak print) errors
        //tm3: number of -3 (excessive print) errors
        //tm5: number of -4 (position) errors
        private void UpDatePieChart(int tm1,int tm2, int tm3 ,int tm4, int tm5, int c)
        {
            
            ZedGraph.GraphPane GP = piegraph.GraphPane;
            ZedGraph.GraphPane GPC = piegraphwithcorrect.GraphPane;
            GP.Title.Text = "Hata Oranları";
            GP.Title.FontSpec.FontColor = System.Drawing.Color.White;
            
            

            GPC.Title.Text = "Doğru ve Hata Oranları";
            GPC.Title.FontSpec.FontColor = System.Drawing.Color.White;
            double[] d={tm1,tm2,tm3,tm4,tm5};
            string[] labels = { "Hatalı Numara", "Silik", "Çapak", "Konum", "Eksik" };
            System.Drawing.Color[] CL = { System.Drawing.Color.Red, System.Drawing.Color.Yellow, System.Drawing.Color.Violet, System.Drawing.Color.Blue, System.Drawing.Color.DarkGray };
            GP.CurveList.Clear();
            GPC.CurveList.Clear();
            for (int i = 0; i < 5; ++i)
            {
                if (d[i] != 0)
                {
                    ZedGraph.PieItem PI = GP.AddPieSlice(d[i], CL[i], 0, labels[i]);
                    PI.Fill = new ZedGraph.Fill(System.Drawing.Color.White, CL[i], -20F);
                }
               
            }
            GP.Chart.Fill=new ZedGraph.Fill(System.Drawing.Color.Black, System.Drawing.Color.FromArgb(255,200, 200, 200), -45F);
            GP.Fill = new ZedGraph.Fill(System.Drawing.Color.Black);
            GPC.Chart.Fill = new ZedGraph.Fill(System.Drawing.Color.Black, System.Drawing.Color.FromArgb(255, 200, 200, 200), -45F);
            GPC.Fill = new ZedGraph.Fill(System.Drawing.Color.Black);
            double[] dc = { tm1, tm2, tm3, tm4, tm5,c };
            string[] labelsc = { "Hatalı Numara", "Silik", "Çapak", "Konum", "Eksik", "Doğru"};
            System.Drawing.Color[] CLc = { System.Drawing.Color.Red, System.Drawing.Color.Yellow, System.Drawing.Color.Violet, System.Drawing.Color.Blue, System.Drawing.Color.DarkGray,System.Drawing.Color.Green };

            for (int i = 0; i < 6; ++i)
            {
                if (dc[i] != 0)
                {
                    
                    ZedGraph.PieItem PI = GPC.AddPieSlice(dc[i], CLc[i], 0, labelsc[i]);
                    PI.Fill = new ZedGraph.Fill(System.Drawing.Color.White, CLc[i],-20F);
                }
            }

            GP.Legend.IsHStack = false;
            GP.Legend.Position = ZedGraph.LegendPos.Float;
            GP.Legend.Location = new ZedGraph.Location(0.02,0.02,ZedGraph.CoordType.ChartFraction);

            GPC.Legend.IsHStack = false;
            GPC.Legend.Position = ZedGraph.LegendPos.Float;
            GPC.Legend.Location = new ZedGraph.Location(0.02, 0.02, ZedGraph.CoordType.ChartFraction);

            GraphHost.Visibility = Visibility.Visible;
            CorrectGraphHost.Visibility = Visibility.Visible;
            piegraph.AxisChange();
            piegraphwithcorrect.AxisChange();
            GraphHost.UpdateLayout();
            CorrectGraphHost.UpdateLayout();
            piegraph.Update();
            piegraphwithcorrect.Update();
            piegraph.ZoomPane(GP, 1, new System.Drawing.PointF(0, 0), false);
            piegraphwithcorrect.ZoomPane(GPC, 1, new System.Drawing.PointF(0, 0), false);
            
        }

        //Saves a file which holds the list of faults
        private void CSVclick(object sender, RoutedEventArgs e)
        {
            if (cn.Text == "0" || cn.Text == "")
            {
                MessageBox.Show("Raporlama Verisi Mevcut Değil!");
                return;
            }
            System.Windows.Forms.SaveFileDialog SFD = new System.Windows.Forms.SaveFileDialog();
            if (SFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = SFD.FileName + ".csv";
                NumaratorDataBase D = new NumaratorDataBase();
                List<Sheet> SL = D.GetSheetsBetween(ReportStartDate, ReportEndDate);
                List<List<string>> Title = new List<List<string>>();
                List<string> titlerow = new List<string>();
                titlerow.Add("Raporlama Tarihi");
                titlerow.Add(String.Format("{0:u}", ReportStartDate).Substring(0, 10) + "<->" + String.Format("{0:u}", ReportEndDate).Substring(0, 10));
                titlerow.Add("");
                titlerow.Add("");
                titlerow.Add("");
                titlerow.Add("");
                Title.Add(titlerow);

                List<string> names= new List<string>();
                names.Add("Seri Numarası");
                names.Add("Hata Tipi");
                names.Add("Satır");
                names.Add("Sütun");
                names.Add("Kırmızı/Siyah");
                names.Add("Tarih");
                Title.Add(names);

                CSV.WriteRows(Title, path);
                foreach (Sheet S in SL)
                {
                    if (namesofSettings.Contains(S.settingname))
                    {
                        List<List<string>> rows = D.GetFaultRows(S.SheetNumber);
                        CSV.WriteRows(rows, path);
                    }
                }
            }
            
        }

        //Saves a file which holds the number of faults and types
        private void ReportResult(object sender, RoutedEventArgs e)
        {
            if (cn.Text == "0" || cn.Text == "")
            {
                MessageBox.Show("Raporlama Verisi Mevcut Değil!");
                return;
            }
            System.Windows.Forms.SaveFileDialog SFD = new System.Windows.Forms.SaveFileDialog();
            if (SFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = SFD.FileName + ".txt";
                NumaratorDataBase D = new NumaratorDataBase();
                List<List<string>> results = new List<List<string>>();

                List<string> titlerow = new List<string>();
                titlerow.Add("Raporlama Tarihi");
                titlerow.Add(String.Format("{0:u}", ReportStartDate).Substring(0, 10) + "<->" + String.Format("{0:u}", ReportEndDate).Substring(0, 10));

                List<string> TotalS = new List<string>();
                TotalS.Add("Toplam Basılan Tabaka Sayısı");
                TotalS.Add(TotalSheet.Text);

                List<string> FalseS = new List<string>();
                FalseS.Add("Hatalı Basılan Tabaka Sayısı");
                FalseS.Add(FalseSheet.Text);

                List<string> CorrectS = new List<string>();
                CorrectS.Add("Doğru Basılan Tabaka Sayısı");
                CorrectS.Add(CorrectSheet.Text);

                List<string> PercentS = new List<string>();
                PercentS.Add("Hatalı Tabaka Yüzdesi");
                PercentS.Add(FalsePercent.Text);

                List<string> tm1S = new List<string>();
                tm1S.Add("Seri Sıra No Hatası");
                tm1S.Add(tm1.Text);

                List<string> tm2S = new List<string>();
                tm2S.Add("Silik Basılan Numara");
                tm2S.Add(tm2.Text);

                List<string> tm3S = new List<string>();
                tm3S.Add("Çapak Hatası");
                tm3S.Add(tm3.Text);

                List<string> tm4S = new List<string>();
                tm4S.Add("Konum Hatası");
                tm4S.Add(tm4.Text);

                List<string> tm5S = new List<string>();
                tm5S.Add("Eksik Numara");
                tm5S.Add(tm5.Text);

                List<string> cnS = new List<string>();
                cnS.Add("Doğru Numara");
                cnS.Add(cn.Text);

                results.Add(titlerow);
                results.Add(TotalS);
                results.Add(FalseS);
                results.Add(CorrectS);
                results.Add(PercentS);
                results.Add(tm1S);
                results.Add(tm2S);
                results.Add(tm3S);
                results.Add(tm4S);
                results.Add(tm5S);
                results.Add(cnS);
                CSV.WriteRows(results, path);
            }
        }

        private void PressCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumaratorDataBase D = new NumaratorDataBase();
            DateTime dt=D.GetSheetTime((sender as TextBox).Text.Replace(" ",""));
            if (dt.Equals(new DateTime()))
            {
                PressedDateTime.Text = "Veri Yok";
                SheetSetting.Text = "Veri Yok";
            }
            else
            {
                PressedDateTime.Text = dt.ToString();
                SheetSetting.Text = D.GetSheetSettingNameofSheet((sender as TextBox).Text.Replace(" ", ""));
            }
        }
    }
}
