using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Class for Serial Number Style
    // ===============================
    public class SerailNumberStyle
    {
        public delegate void delegatefolderpathchanged();
        public event delegatefolderpathchanged folderchanged; //Event for folderchange
        public string SerialStyleName; //name of the style, ex:"E9"
        public int SerialCharNumber; //number of char in Serial Part, in E9 it is 4
        public int SequenceCharNumber; //number of char in squence part, in E9 it is 6 
        public List<Box> BoxList; // for each char holds the properties like width, height, ofset and Ischar
        public string CharsFolderPath
        {
            get { return _CharsFolderPath; }
            set
            {
                if (value != _CharsFolderPath)
                {
                    _CharsFolderPath = value;
                    if (folderchanged != null)
                    {
                        folderchanged();
                    }
                }
            }
        }
        private string _CharsFolderPath;
        public SerailNumberStyle(List<Box> BoxL, string folderpath, int serialcharnumber, int sequencecharnumber)
        {
            this.BoxList = BoxL;
            this.CharsFolderPath = folderpath;
            this.SerialCharNumber = serialcharnumber;
            this.SequenceCharNumber = sequencecharnumber;
        }

    }
}
