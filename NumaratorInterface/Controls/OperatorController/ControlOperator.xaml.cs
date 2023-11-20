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

namespace NumaratorInterface.Controls.OperatorController
{
    // ===============================
    // AUTHOR      : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Combine UserControls (FalseSheetController, ImageViewer, OperatorSettings, ReactionController)
    // in one UserControl
    // ===============================
    public partial class ControlOperator : UserControl
    {
        public ControlOperator()
        {
            InitializeComponent();
            //OperatorSettingsControl's referances assignments//
            SettingsControl.ViewControl = ViewControl;
            SettingsControl.FSController = FalseSheetControl;
            SettingsControl.FSController.MakeButtonInvisible();
            SettingsControl.reactionController = ReactionControls;

            //load the last entered parameters of OperatorSettingsControl
            //SettingsControl.LoadParameters(); //boş gelecek kullanıcı dosyadan da seçebilecek

            this.SettingsControl.ImgBox = new PVImageBox();
            this.SettingsControl.ImgBox.Location = new System.Drawing.Point(0, 0);
            this.SettingsControl.ImgBox.Name = "m_ImageBox";

            this.SettingsControl.ImgBox.Size = new System.Drawing.Size(1855, 1045);
            this.SettingsControl.ImgBox.TabIndex = 15;
            this.SettingsControl.ImgBox.View = null;

            System.Windows.Forms.Integration.WindowsFormsHost host =
               new System.Windows.Forms.Integration.WindowsFormsHost();

            host.Child = this.SettingsControl.ImgBox;// m_ImageView;
            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.SettingsControl.ViewControl.TemplateGrid.Children.Add(host);
        }
    }
}
