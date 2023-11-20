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
    // PURPOSE     : Class for SheetSetting consists of SheetProperties, SerialNumberPositions, and template properties
    // ===============================   
    public class SheetSettings
    {
        public string settingname;
        public SheetProperties sheetproperties; //Holds the data of sheet type like witdh, height, row, coll, etc.
        public SerialNumberPositions serialnumberpositions; //holds the data of positions of serial numbers

        //template properties
        public List<IntPoint> templatePoint;
        public int templateWidth;
        public int templateHeight;
        //
        public SheetSettings()
        {
            sheetproperties = new SheetProperties();
            serialnumberpositions = new SerialNumberPositions();
            templatePoint = new List<IntPoint>();
        }
    }
}
