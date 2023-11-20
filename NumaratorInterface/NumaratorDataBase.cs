using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
namespace NumaratorInterface

{
    // ===============================
    // AUTHOR     :  Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Interface for MySQL Database Functions
    // ===============================
    public class NumaratorDataBase
    {
        private string ConnectionString = "server=127.0.0.1;uid=root;" +
                    "pwd=123456;database=NumaratorV2;";
        private MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

        public NumaratorDataBase()
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader("DBUP.pv", false))
            {
                string username = file.ReadLine();
                string password = file.ReadLine();
                this.ConnectionString = "server=127.0.0.1;uid=" + username + ";" +
                    "pwd=" + password + ";database=NumaratorV2;";
            }
        }
        //Opens Connection
        private void OpenConnection()
        {
            if (this.conn.State == ConnectionState.Open)
                conn.Close();
            this.conn.ConnectionString = ConnectionString;
            conn.Open();
        }
        //Close Connection
        private void CloseConnection()
        {
            if (!(this.conn.State == ConnectionState.Closed))
                this.conn.Close();
        }
        //Returns true if Database has User that name is equal to UserName, false otherwise
        public bool IsUserExist(string UserName)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.users where UserName="+"\""+UserName+"\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            bool result = MSR.HasRows;
            this.CloseConnection();
            return result;
        }
        //Returns true if Database has User, false otherwise
        public bool IsDBHasUser()
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.users";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            bool result = MSR.HasRows;
            this.CloseConnection();
            return result;
        }

        //Gets the User if password matches
        public User GetUser(string UserName, string password)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.users where UserName=" + "\"" + UserName + "\" AND UserPassword="+"\""+User.DecryptString(password,"BlackHawk")+"\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            bool result = MSR.HasRows;
            if (result == false)
            {
                this.CloseConnection();
                return null;
            }
            else
            { 
                User user=new User();
                while (MSR.Read())
                {
                    user.UserName = Convert.ToString(MSR[("UserName")]);
                    user.setUserType((User.Users)Convert.ToInt32(MSR[("UserType")]));
                }
                this.CloseConnection();
                return user;
            }
        }

        //Adds User
        public void InsertUser(User U, string password)
        {
            if (!this.IsUserExist(U.UserName))
            {
                this.OpenConnection();
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "INSERT INTO numaratorv2.users (UserName,UserPassword,UserType) VALUES("+"\""+U.UserName+"\","+"\""+User.EncryptString(password,"BlackHawk")+"\","+"'"+Convert.ToString(U.getUserType())+"')";
                command.Connection = this.conn;
                MySqlDataReader MSR = command.ExecuteReader();
                this.CloseConnection();
            }
            else
            {
                MessageBox.Show("Bu İsimde Kullanıcı Mecvut!");
            }
        }

        //Gets all Sheets in Database between the inputs
        public List<Sheet> GetSheetsBetween(DateTime Start, DateTime End)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheets where SheetTime>='" + String.Format("{0:u}",Start).Substring(0,10) + "' AND SheetTime<'" + String.Format("{0:u}",End+new TimeSpan(1,0,0,0,0)).Substring(0,10)+"'" + "LIMIT 0,  8000000";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            List<Sheet> SL = new List<Sheet>();
            while (MSR.Read())
            {
                Sheet s=new Sheet();
                s.sheettime = Convert.ToDateTime(MSR[("SheetTime")]);
                s.IsFalse = Convert.ToBoolean(MSR["IsFalse"]);
                s.SheetNumber = Convert.ToString(MSR["SheetNumber"]);
                s.settingname = Convert.ToString(MSR["SheetSettingName"]);
                SL.Add(s);
            }
            return SL;
        
        }

        //Gets all off the Failures with serialnumber in a specified sheetnumber
        public List<List<string>> GetFaultRows(string sheetnumber)
        {
            List<List<string>> rows = new List<List<string>>();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.failures where SheetNumber=" + "\"" + sheetnumber + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            while (MSR.Read())
            {
                List<string> row = new List<string>();
                //row.Add(Convert.ToString(MSR[("SheetNumber")]));
                row.Add(Convert.ToString(MSR[("CorretSerialNumber")]));

                int f=Convert.ToInt32(MSR["FailureType"]);
                if (f == -1)
                    row.Add("Hatalı Numara");
                else if (f == -2)
                    row.Add("Kelek");
                else if (f == -3)
                    row.Add("Çapak");
                else
                    row.Add("!!");
                row.Add(Convert.ToString(Convert.ToInt32(MSR["FailureRowPos"])+1));
                row.Add(Convert.ToString(Convert.ToInt32(MSR["FailureCollPos"]) + 1));
                if (Convert.ToInt32(MSR["FailureIndex"]) % 2 == 1)
                    row.Add("Kırmızı");
                else
                    row.Add("Siyah");
                row.Add(Convert.ToString(MSR["FailureTime"]));
                if(Convert.ToInt32(MSR["FailureType"])!=1)
                    rows.Add(row);
            }
            return rows;
        }

        //Gets all of the total number of faults in different type in a specified sheetnumber
        public void GetFaultNumber(ref int tm1, ref int tm2, ref int tm3, ref int tm4,ref int tm5, ref int c,string sheetnumber)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.failures where SheetNumber=" + "\"" + sheetnumber + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            while (MSR.Read())
            {
                int failuretype = Convert.ToInt32(MSR[("failuretype")]);
                if (failuretype == -1)
                {
                    ++tm1;
                }
                else if (failuretype == -2)
                {
                    ++tm2;
                }
                else if (failuretype == -3)
                {
                    ++tm3;
                }
                else if (failuretype == -4)
                { 
                    ++tm4;
                }
                else if (failuretype == -5)
                {
                    ++tm5;
                }
                else if (failuretype == 1)
                {
                    ++c;
                }
            }
        }

        //returns true if SheetExist in Database false OtherWise
        public bool IsSheetExist(string Name)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheets where SheetNumber=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                this.CloseConnection();
                return true;
            }
            else
            {
                this.CloseConnection();
                return false;
            }
        }

        //Deletes Sheet
        public void DeleteSheet(string SheetNumber)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "Delete FROM numaratorv2.failures where SheetNumber=" + "\"" + SheetNumber + "\"";
            command.Connection = this.conn;
            command.ExecuteReader();
            this.CloseConnection();
            //delete correct sheets
            this.OpenConnection();
            command.CommandText = "Delete FROM numaratorv2.correctsheets where SheetNumber=" + "\"" + SheetNumber + "\"";
            command.ExecuteReader();
            this.CloseConnection();
            //delete sheets
            this.OpenConnection();
            command.CommandText = "Delete FROM numaratorv2.sheets where SheetNumber=" + "\"" + SheetNumber + "\"";
            command.ExecuteReader();
            this.CloseConnection();
        }

        //Returns the DateTime when the Sheet is produced
        public DateTime GetSheetTime(string SheetName)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheets where SheetNumber=" + "\"" + SheetName + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MSR.Read();
                return Convert.ToDateTime(MSR[("SheetTime")]);
            }
            else
            {
                return System.DateTime.MinValue ;
            }
        }
        //Returns the SheetSetting Name of The SheetNumber
        public string GetSheetSettingNameofSheet(string number)
        {
            string s = "";

            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheets where SheetNumber=" + "\"" + number + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MSR.Read();
                return Convert.ToString(MSR[("SheetSettingName")]);
            }
            else
            {
                return s;
            }
        }


        //Returns true if SheetSetting Exist in Database, false otherwise
        public bool IsSheetSettingExist(string Name)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheetsettings where SheetSettingName=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                this.CloseConnection();
                return true;
            }
            else
            {
                this.CloseConnection();
                return false;
            }
        }

        //Returns true if SerialNumberStyle exist in Databse, false otherwise
        public bool IsSerialNumberExist(string Name)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.serialstyles where SeriStyleName=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                this.CloseConnection();
                return true;
            }
            else
            {
                this.CloseConnection();
                return false;
            }
        }

        //insert Sheet to Database
        public void InsertSheetV2(string sheetnumber, string banknotetype, string sheetstylename, DateTime now, bool isfalse, int xoffset, int yoffset)
        {
            int i = 0;
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            if (isfalse)
                i = 1;
            command.CommandText = "Call numaratorv2.insertsheetv2(" + "\"" + sheetnumber + "\"" + "," + "\"" + banknotetype + "\"" + "," + "\"" + sheetstylename + "\"" + "," + "'" + String.Format("{0:u}", now).Substring(0, 19) + "'" + "," + "\"" + Convert.ToString(i) + "\","+"'"+Convert.ToString(xoffset)+"',"+"'"+Convert.ToString(yoffset)+"')";
            command.Connection = this.conn;
            command.ExecuteReader();
            this.CloseConnection();
        }

        //insert failure    
        //row:row of failure, 
        //coll: coll of failure, 
        //index: order of serialnumber. (first number is 0, last number is row*coll*2)
        //correctserial: serialnumber that fault has occured, 
        //dtime: time and date when the fault occured
        //failure: type of the failure
        public void InsertFailure(string sheetnumber, int row, int coll,int index, string correctserial, DateTime dtime, int failure)
        {
            this.OpenConnection();

            MySqlCommand command = new MySqlCommand();
            command.CommandText = "Call numaratorv2.insertfailure(" + "\"" + sheetnumber + "\"" + "," + "'" + row + "'" + "," + "'" + coll + "'" + "," + "'" + index + "'" + "," + "\"" + correctserial + "\"" + "," + "'" + String.Format("{0:u}", dtime).Substring(0, 19) + "'" + "," + "'" + failure + "')";
            command.Connection = this.conn;
            command.ExecuteReader();
            this.CloseConnection();
        }

        //Gets all SerialNumberStyles
        public void GetSerialNumberStyles(List<SerailNumberStyle> SNSL)
        {
            SNSL.Clear();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.serialstyles";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            List<string> Lnames = new List<string>();
            while (MSR.Read())
            {
                Lnames.Add(Convert.ToString(MSR["SeriStyleName"]));
            }
            this.CloseConnection();
            foreach (string s in Lnames)
            {
                SNSL.Add(this.GetSerialNumberStyle(s));
            } 
        }

        //Gets the SheetSetting mathces the name
        public SheetSettings GetSheetSetting(string Name)
        {
            SheetSettings S = new SheetSettings();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheetsettings where SheetSettingName=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MSR.Read();
                string sheetsettingname = Convert.ToString(MSR[("SheetSettingName")]);
                string serialstylename = Convert.ToString(MSR["SerialStyleName"]);
                int rownumber=Convert.ToInt32(MSR[("RowNumber")]);
                int collnumber = Convert.ToInt32(MSR[("CollNumber")]);
                int boxwidth= Convert.ToInt32(MSR[("BoxWidth")]);
                int boxheight = Convert.ToInt32(MSR[("BoxHeight")]);
                bool horizontal = Convert.ToBoolean(MSR[("Horizontal")]);
                bool updown = Convert.ToBoolean(MSR[("UpDown")]);
                string positions = Convert.ToString(MSR[("positions")]);
                float banknoteheight = Convert.ToSingle(MSR[("BanknoteHeight")]);
                float banknotewidth = Convert.ToSingle(MSR["BanknoteWidth"]);
                float sheetwidth = Convert.ToSingle(MSR[("SheetWidth")]);
                float sheetheight = Convert.ToSingle(MSR[("SheetHeight")]);
                string templateX = Convert.ToString(MSR[("TemplateX")]);
                string templateY = Convert.ToString(MSR[("TemplateY")]);
                int templateHeight = Convert.ToInt32(MSR[("TemplateHeight")]);
                int templateWidth = Convert.ToInt32(MSR[("TemplateWidth")]);
                string[] properties = positions.Split('_');
                List<IntPoint> IPL= new List<IntPoint>();
                this.CloseConnection();
                for (int i=0;i<properties.Length;++i)
                {
                    IPL.Add(new IntPoint(Convert.ToInt32(properties[i].Split('-')[0]), Convert.ToInt32(properties[i].Split('-')[1])));
                }
                string[] tx = templateX.Split('-');
                string[] ty = templateY.Split('-');
                int y = 0;
                List<IntPoint> TPL = new List<IntPoint>();
                foreach (string s in tx)
                {
                    TPL.Add(new IntPoint(Convert.ToInt32(tx[y]), Convert.ToInt32(ty[y])));
                    ++y;
                }
                S.templatePoint = TPL;
                S.templateHeight = templateHeight;
                S.templateWidth = templateWidth;
                S.settingname = sheetsettingname;
                S.sheetproperties.rownumber = rownumber;
                S.sheetproperties.collnumber = collnumber;
                S.serialnumberpositions.boxwidth = boxwidth;
                S.serialnumberpositions.boxheight = boxheight;
                S.serialnumberpositions.positions = IPL;
                S.sheetproperties.horizantal = horizontal;
                S.sheetproperties.updown = updown;
                S.sheetproperties.banknoteheight = banknoteheight;
                S.sheetproperties.banknotewidth = banknotewidth;
                S.sheetproperties.sheetwidth = sheetwidth;
                S.sheetproperties.sheetheight = sheetheight;

                NumaratorDataBase D = new NumaratorDataBase();
                S.sheetproperties.serialnumberstyle = D.GetSerialNumberStyle(serialstylename);
            }
            return S;
        }

        //Gets all SheetSettings
        public void GetSheetSettings(List<SheetSettings> SSL)
        {
            SSL.Clear();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheetsettings";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            List<string> Lnames = new List<string>();
            while (MSR.Read())
            {
                Lnames.Add(Convert.ToString(MSR["SheetSettingName"]));
            }
            foreach (string s in Lnames)
            {
                SSL.Add(this.GetSheetSetting(s));
            }
        }

        //delete SheetSetting mathes the name
        public void DeleteSheetSetting(string Name)
        {
            try
            {
                this.OpenConnection();
                MySqlCommand command = new MySqlCommand();

                command.CommandText = "DELETE FROM numaratorv2.SheetSettings where SheetSettingName=" + "\"" + Name + "\"";
                command.Connection = this.conn;
                command.ExecuteReader();
                MessageBox.Show(Name + " Adlı Tabaka Ayarı Silindi!");
            }
            catch {
                ApproveWindow W = new ApproveWindow("Silmek İstediğiniz Tabaka Ayarı ile Daha Önce Kontrol Yapılmıştır!\n" +Name+" Adlı Tabaka Ayarı Silindiği Durumda, Bu Ayar İle Kontrolü Yapılmış Tabaka Verileri Silinecektir!\n"+Name+" Adlı Tabaka Ayarını Silmek İstediğinize Emin misiniz?");
                W.Height = W.Height + 30;
                W.Width = W.Width + 200;
                W.ShowDialog();
                if ((bool)W.DialogResult)
                {
                    this.OpenConnection();
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = "Select * FROM numaratorv2.sheets where SheetSettingName=" + "\"" + Name + "\"" +" Limit 99000000";
                    command.Connection = this.conn;
                    MySqlDataReader MSR=command.ExecuteReader();
                    List<String> SheetNumbers = new List<string>();
                    while (MSR.Read())
                    { 
                        SheetNumbers.Add(Convert.ToString(MSR["SheetNumber"]));
                    }
                    this.CloseConnection();
                    foreach (string SheetNumber in SheetNumbers)
                    {
                        //delete falls sheets
                        this.OpenConnection();
                        command.CommandText = "Delete FROM numaratorv2.failures where SheetNumber=" + "\"" + SheetNumber + "\"";
                        command.ExecuteReader();
                        this.CloseConnection();
                        //delete correct sheets
                        this.OpenConnection();
                        command.CommandText = "Delete FROM numaratorv2.correctsheets where SheetNumber=" + "\"" + SheetNumber + "\"";
                        command.ExecuteReader();
                        this.CloseConnection();
                        //delete sheets
                        this.OpenConnection();
                        command.CommandText = "Delete FROM numaratorv2.sheets where SheetNumber=" + "\"" + SheetNumber + "\"";
                        command.ExecuteReader();
                        this.CloseConnection();
                    }
                    // delete sheetsetting
                    this.OpenConnection();
                    command.CommandText = "DELETE FROM numaratorv2.SheetSettings where SheetSettingName=" + "\"" + Name + "\"";
                    command.Connection = this.conn;
                    command.ExecuteReader();
                    this.CloseConnection();
                    MessageBox.Show(Name + " Adlı Tabaka Ayarı Silindi!");
                }
            }
        }

        //Delete SerialNumberStyle matches the name
        public void DeleteSerialNumberStyle(string Name)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "DELETE FROM numaratorv2.serialstyles where SeriStyleName=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            command.ExecuteReader();
        }

        //Get one box of serial numberstyles according to it's id in Database. 
        public Box GetBox(int Id)
        {
            Box B = new Box();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.boxes where BoxId=" + Convert.ToString(Id);
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MSR.Read();
                B.Width= Convert.ToSingle(MSR[("Width")]);
                B.Height= Convert.ToSingle(MSR[("Height")]);
                B.Ofset = Convert.ToSingle(MSR["Ofset"]);
                B.IsChar= Convert.ToBoolean(MSR[("IsChar")]);
            }
            return B;
        }

        //Gets SerialNumberStyle Mathes the Name
        public SerailNumberStyle GetSerialNumberStyle(string Name)
        {
            SerailNumberStyle S = null;
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.serialstyles where SeriStyleName=" + "\"" + Name + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MSR.Read();
                string folderpath=Convert.ToString(MSR[("FolderPath")]);
                string property = Convert.ToString(MSR[("BoxProperties")]);
                string name = Convert.ToString(MSR[("SeriStyleName")]);
                int serial=Convert.ToInt32(MSR["SerialCharNumber"]);
                int sequence = Convert.ToInt32(MSR["SequenceCharNumber"]);
                string[] properties = property.Split('_');
                int i = Convert.ToInt32(properties[0]);
                List<Box> BL = new List<Box>();
                for (int j = 1; j <= i; ++j)
                {
                    BL.Add(GetBox(Convert.ToInt32(properties[j])));
                }
                S = new SerailNumberStyle(BL, folderpath,serial,sequence);
                S.SerialStyleName = name;
            }
            this.CloseConnection();
            return S;
        }

        //Get last 100 FalseSheet in Database
        public List<FalseSheet> GetLastFalseSheets()
        {
            //get last false 100 sheets' names//
            List<FalseSheet> FSL = new List<FalseSheet>();
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheets where IsFalse=1 order by SheetTime desc limit 100";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            List<string> LFalseSheetsName = new List<string>();
            while(MSR.Read())
            {
                string serialName=Convert.ToString(MSR["SheetNumber"]);
                if (System.IO.File.Exists("FalseSheets\\" + serialName + ".jpeg"))
                    LFalseSheetsName.Add(serialName);
            }
            this.CloseConnection();
            //foreach name create FalseSheets
            foreach (string s in LFalseSheetsName)
            {
                //get sheetSettings of the FalseSheet
                FalseSheet FS = new FalseSheet();
                this.OpenConnection();
                command = new MySqlCommand();
                command.CommandText = "SELECT * FROM numaratorv2.sheets where SheetNumber=" + "\""+s+"\"";
                command.Connection = this.conn;
                MSR = command.ExecuteReader();
                if (MSR.HasRows)
                    MSR.Read();
                else
                {
                    this.CloseConnection();
                    continue;
                }
                string sheetsettingname = Convert.ToString(MSR["SheetSettingName"]);
                int xoffset = Convert.ToInt32(MSR["xoffset"]);
                int yoffset = Convert.ToInt32(MSR["yoffset"]);
                this.CloseConnection();
                FS.Settings = this.GetSheetSetting(sheetsettingname);

                //Get error type index vs
                this.OpenConnection();
                command = new MySqlCommand();
                command.CommandText = "SELECT * FROM numaratorv2.failures where SheetNumber=" + "\"" + s + "\"";
                command.Connection = this.conn;
                MSR = command.ExecuteReader();
                //FS.Result.result = new List<CheckResultPair>();
                List<CheckResultPair> LCRP=new List<CheckResultPair>();
                while (MSR.Read())
                {
                    int result=Convert.ToInt32(MSR["FailureType"]);
                    int ind=Convert.ToInt32(MSR["FailureIndex"]);
                    string serial=Convert.ToString(MSR["CorretSerialNumber"]);
                    int indX = Convert.ToInt32(MSR["FailureRowPos"]);
                    int indY = Convert.ToInt32(MSR["FailureCollPos"]);
                    CheckResultPair CRP = new CheckResultPair(result, indX, indY, ind, serial);
                    LCRP.Add(CRP);
                }
                this.CloseConnection();


                //Data Related to Image//

                //Stream jpegStrem = new FileStream("FalseSheets\\" + s + ".jpeg", FileMode.Open, FileAccess.Read, FileShare.Read);
                //JpegBitmapDecoder dec=new JpegBitmapDecoder(jpegStrem,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.Default);
                //BitmapSource bs = dec.Frames[0];
                IntPtr intp=IntPtr.Zero;
                //bs.CopyPixels(new Int32Rect(0,0,BM.Width,BM.Height),intp,lenght,BM.Width);
                FS.imageWidth = 8192;
                FS.imageHeight = 10500;
                FS.Result = new FrameResult(LCRP, 0, intp, 8192*10500,xoffset,yoffset);
                FS.PilotNumber = s;
                FSL.Add(FS);
            }
            return FSL;
        }

        //Replaces SheetSetting (uses SheetSettings' Name for detecting the replacement)
        public bool ReplaceSheetSetting(SheetSettings SS)
        {
            string templatePositionsX = "";
            string templatePositionsY = "";
            foreach (IntPoint P in SS.templatePoint)
            {
                if (P != SS.templatePoint[SS.templatePoint.Count - 1])
                    templatePositionsX = templatePositionsX + Convert.ToString(P.X) + "-";
                else
                    templatePositionsX = templatePositionsX + Convert.ToString(P.X);

                if (P != SS.templatePoint[SS.templatePoint.Count - 1])
                    templatePositionsY = templatePositionsY + Convert.ToString(P.Y) + "-";
                else
                    templatePositionsY = templatePositionsY + Convert.ToString(P.Y);
            }

            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.Connection = this.conn;
            int h = 0;
            int u = 0;
            if (SS.serialnumberpositions.boxheight > SS.serialnumberpositions.boxwidth)
            {
                h = 1;
            }
            if (SS.sheetproperties.updown)
            {
                u = 1;
            }
            string positions = "";
            foreach (IntPoint P in SS.serialnumberpositions.positions)
            {
                if (P != SS.serialnumberpositions.positions[SS.serialnumberpositions.positions.Count - 1])
                    positions = positions + Convert.ToString(P.X) + "-" + Convert.ToString(P.Y) + "_";
                else
                    positions = positions + Convert.ToString(P.X) + "-" + Convert.ToString(P.Y);
            }
            command.CommandText = "UPDATE numaratorv2.sheetsettings SET " +
                                                                        "SerialStyleName=" + "\"" + SS.sheetproperties.serialnumberstyle.SerialStyleName + "\"" + "," + 
                                                                        "RowNumber=" + "'"+Convert.ToString(SS.sheetproperties.rownumber)+"',"+
                                                                        "CollNumber=" + "'" + Convert.ToString(SS.sheetproperties.collnumber) + "'," +
                                                                        "BoxWidth="+"'"+Convert.ToString(SS.serialnumberpositions.boxwidth)+"',"+
                                                                        "BoxHeight=" + "'" + Convert.ToString(SS.serialnumberpositions.boxheight) + "'," +
                                                                        "Horizontal="+"'"+Convert.ToString(h)+"',"+
                                                                        "UpDown="+"'"+Convert.ToString(u)+"',"+
                                                                        "Positions="+"\""+positions+"\""+","+
                                                                        "BanknoteHeight=" + "'" + Convert.ToString(SS.sheetproperties.banknoteheight).Replace(",", ".") + "'," +
                                                                        "BanknoteWidth=" + "'" + Convert.ToString(SS.sheetproperties.banknotewidth).Replace(",", ".") + "'," +
                                                                        "SheetHeight=" + "'" + Convert.ToString(SS.sheetproperties.sheetheight).Replace(",", ".") + "'," +
                                                                        "SheetWidth=" + "'" + Convert.ToString(SS.sheetproperties.sheetwidth).Replace(",", ".") + "'," +
                                                                        "TemplateWidth="+"'" + Convert.ToString(SS.templateWidth)+"',"+
                                                                        "TemplateHeight=" + "'" + Convert.ToString(SS.templateHeight) + "'," +
                                                                        "TemplateX=" + "\"" + templatePositionsX + "\"," +
                                                                        "TemplateY=" + "\"" + templatePositionsY + "\"" +
                                                                        " WHERE SheetSettingName=" + "\"" + SS.settingname + "\"";
            command.ExecuteReader();
            this.CloseConnection();
            return true;

        }

        //Saves the SheetSettings
        public bool SaveSheetSetting(SheetSettings SS)
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.sheetsettings where SheetSettingName=" + "\"" + SS.settingname + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MessageBox.Show("Bu Tabaka Ayarı mevcut");
                this.CloseConnection();
                return false;
            }
            else
            {
                string positions = "";
                foreach (IntPoint P in SS.serialnumberpositions.positions)
                {
                    if (P != SS.serialnumberpositions.positions[SS.serialnumberpositions.positions.Count - 1])
                        positions = positions + Convert.ToString(P.X) + "-" + Convert.ToString(P.Y) + "_";
                    else
                        positions = positions + Convert.ToString(P.X) + "-" + Convert.ToString(P.Y);
                }
                string templatePositionsX = "";
                string templatePositionsY = "";
                foreach (IntPoint P in SS.templatePoint)
                {
                    if (P != SS.templatePoint[SS.templatePoint.Count - 1])
                        templatePositionsX = templatePositionsX + Convert.ToString(P.X) + "-";
                    else
                        templatePositionsX = templatePositionsX + Convert.ToString(P.X);

                    if (P != SS.templatePoint[SS.templatePoint.Count - 1])
                        templatePositionsY = templatePositionsY + Convert.ToString(P.Y) + "-";
                    else
                        templatePositionsY = templatePositionsY + Convert.ToString(P.Y);
                }


                if (IsSerialNumberExist(SS.sheetproperties.serialnumberstyle.SerialStyleName))
                {
                    //insert serialnumberstyle//
                    this.OpenConnection();
                    command.CommandText = "Call numaratorv2.insertsheetsetting(" + "\"" + SS.settingname + "\"" + "," + "\"" + SS.sheetproperties.serialnumberstyle.SerialStyleName + "\"" + "," +
                        Convert.ToString(SS.sheetproperties.rownumber) + "," + Convert.ToString(SS.sheetproperties.collnumber) + "," + Convert.ToString(SS.serialnumberpositions.boxwidth) + "," +
                        Convert.ToString(SS.serialnumberpositions.boxheight) + "," + Convert.ToString(SS.serialnumberpositions.boxwidth > SS.serialnumberpositions.boxheight) + ",TRUE," +
                        "\"" + positions + "\"" + "," + Convert.ToString(SS.sheetproperties.banknotewidth).Replace(",", ".") + "," + Convert.ToString(SS.sheetproperties.banknoteheight).Replace(",", ".") + "," +
                        Convert.ToString(SS.sheetproperties.sheetheight).Replace(",", ".") + "," + Convert.ToString(SS.sheetproperties.sheetwidth).Replace(",", ".") + ",'" + Convert.ToString(SS.templateWidth) + "','"
                        + Convert.ToString(SS.templateHeight) + "',\"" + templatePositionsX + "\",\"" + templatePositionsY + "\")";
                    command.ExecuteReader();
                    this.CloseConnection();
                    return true;
                }
                else
                {
                    MessageBox.Show("İsmi Girilen Seri Numarası Stili Mevcut Değil!");
                    return false;
                }
            }

        }

        //Replaces SerialNumberStyle
        public bool ReplaceSerialNumberStyle(SerailNumberStyle serialnumberstyle, string serialnumbername)
        {
            //İnsert Boxes & make boxproperties string//
            MySqlDataReader MSR;
            MySqlCommand command = new MySqlCommand();
            command.Connection = this.conn;
            string boxproperties = Convert.ToString(serialnumberstyle.BoxList.Count) + "_";
            foreach (Box B in serialnumberstyle.BoxList)
            {
                this.OpenConnection();
                command.CommandText = "Call numaratorv2.insertbox(" + Convert.ToString(B.Width).Replace(",", ".") + "," + Convert.ToString(B.Height).Replace(",", ".") + "," + Convert.ToString(B.Ofset).Replace(",", ".") + "," + Convert.ToString(B.IsChar) + ")";
                command.ExecuteReader();
                this.CloseConnection();

                this.OpenConnection();
                command.CommandText = "SELECT * FROM numaratorv2.boxes where Width=" + Convert.ToString(B.Width).Replace(",", ".") + " AND Height=" + Convert.ToString(B.Height).Replace(",", ".") + " AND IsChar=" + Convert.ToString(B.IsChar) + " AND Ofset=" + Convert.ToString(B.Ofset).Replace(",", ".");
                MSR = command.ExecuteReader();
                if (!MSR.HasRows)
                {
                    MessageBox.Show("kutu numarası alınamadı");
                    return false;
                }
                MSR.Read();
                int i;
                i = Convert.ToInt32(MSR[("BoxId")]);
                if (B != serialnumberstyle.BoxList[serialnumberstyle.BoxList.Count - 1])
                    boxproperties = boxproperties + Convert.ToString(i) + "_";
                else
                    boxproperties = boxproperties + Convert.ToString(i);
                this.CloseConnection();
            }
            //
            this.OpenConnection();
            command.CommandText = "UPDATE numaratorv2.serialstyles SET BoxProperties=" + "\"" + boxproperties + "\"" + "," + "FolderPath=" + "\"" + serialnumberstyle.CharsFolderPath.Replace("\\", "\\\\") + "\"," + "SerialCharNumber=" + Convert.ToString(serialnumberstyle.SerialCharNumber) + ",SequenceCharNumber=" + Convert.ToString(serialnumberstyle.SequenceCharNumber)+" WHERE SeriStyleName="+"\""+serialnumbername+"\"";
            command.ExecuteReader();
            this.CloseConnection();
            return true;
        }

        //Save SerialNumberStyle
        public bool SaveSerialNumberStyle(SerailNumberStyle serialnumberstyle, string serialnumbername)//return true if save is successfull false otherwise
        {
            this.OpenConnection();
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM numaratorv2.serialstyles where SeriStyleName=" + "\"" + serialnumbername + "\"";
            command.Connection = this.conn;
            MySqlDataReader MSR = command.ExecuteReader();
            if (MSR.HasRows)
            {
                MessageBox.Show("Bu isimde serinumarası stili mevcut");
                this.CloseConnection();
                return false;
            }
            else
            {
                //İnsert Boxes & make boxproperties string//
                this.CloseConnection();
                string boxproperties = Convert.ToString(serialnumberstyle.BoxList.Count)+"_";
                foreach (Box B in serialnumberstyle.BoxList)
                {
                    this.OpenConnection();
                      command.CommandText = "Call numaratorv2.insertbox(" + Convert.ToString(B.Width).Replace(",", ".") + "," + Convert.ToString(B.Height).Replace(",", ".") + "," + Convert.ToString(B.Ofset).Replace(",", ".") + "," + Convert.ToString(B.IsChar) + ")";
                      command.ExecuteReader();
                    this.CloseConnection();

                    this.OpenConnection();
                      command.CommandText = "SELECT * FROM numaratorv2.boxes where Width=" + Convert.ToString(B.Width).Replace(",", ".") + " AND Height=" + Convert.ToString(B.Height).Replace(",", ".") + " AND IsChar=" + Convert.ToString(B.IsChar) + " AND Ofset="+Convert.ToString(B.Ofset).Replace(",", ".");
                      MSR=command.ExecuteReader();
                      if (!MSR.HasRows)
                      {
                        MessageBox.Show("kutu numarası alınamadı");
                        return false;
                      }
                      MSR.Read();
                      int i;
                      i = Convert.ToInt32(MSR[("BoxId")]);
                      if(B!=serialnumberstyle.BoxList[serialnumberstyle.BoxList.Count-1])
                          boxproperties=boxproperties+Convert.ToString(i)+"_";
                      else
                          boxproperties=boxproperties+Convert.ToString(i);
                    this.CloseConnection();
                }
                //insert serialnumberstyle//
                this.OpenConnection();
                command.CommandText = "Call numaratorv2.insertserialstyle(" + "\"" + serialnumbername + "\"" + "," + "\"" + boxproperties + "\"" + "," + "\"" + serialnumberstyle.CharsFolderPath.Replace("\\", "\\\\") + "\"," + Convert.ToString(serialnumberstyle.SerialCharNumber) + "," + Convert.ToString(serialnumberstyle.SequenceCharNumber)+")";
                command.ExecuteReader();
                this.CloseConnection();
            }
            return true;
        }
    }
}
