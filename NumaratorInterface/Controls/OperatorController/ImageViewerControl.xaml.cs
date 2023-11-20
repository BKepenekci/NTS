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



using System.Windows.Forms;
using Microsoft.Win32;

using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace NumaratorInterface.Controls.OperatorController
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : this UserControl is used for live view of sheets
    public partial class ImageViewerControl : System.Windows.Controls.UserControl
    {
        //this class is like FalseSheetViewer without pan and zoom ability (Since it is continuously changing (image is changing during the inspection mode))
        public ImageViewerControl()
        {
            InitializeComponent();          
        }      
    }
}
