using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Holds the data( FrameResult, Settings, imageWidth, imageHeight) if any FrameResult object contains data that is not 1
    //               wrap that result using this class.
    // ===============================
    public class FalseSheet
    {
        public string PilotNumber;
        public FrameResult Result;
        public SheetSettings Settings;
        public int imageWidth;
        public int imageHeight;
    }
}
