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
    // PURPOSE     : Use this static class for writing a document in *.csv format
    // ===============================
    public static class CSV
    {
        private static void WriteRow(List<string> row,string path)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path,true))
            {
                string line = "";
                int i = 0;
                foreach (string data in row)
                {
                    if (i == row.Count - 1)
                        line = line + data+ ";";
                    else
                        line = line + data + ",";
                    ++i;
                }
                writer.WriteLine(line);
            }      
        }
        public static void WriteRows(List<List<string>> rows,string path)
        {
            foreach (List<string> row in rows)
            {
                WriteRow(row,path);
            }
        }
    }
}
