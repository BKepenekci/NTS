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
using System.Xml;

namespace NumaratorInterface.Controls.OperatorController
{
    // ===============================
    // AUTHOR     :  Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : this UserControl is used for inspection of FalseSheet. UserControl Has events for zoom and pan for better inspection
    // ===============================
    public partial class FalseSheetViewer : UserControl
    {
        public System.Windows.Point? MidPointPosition = null;
        
        public FalseSheetViewer()
        {
            InitializeComponent();
            BringImage();
            
        }

    
        //Event for pan ability
        private void TemplateGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                if (MidPointPosition != null)
                {
                    System.Windows.Point p = e.GetPosition(TemplateGrid);
                    XO1.TransformationOperations.MoveUIElement(TemplateStackPanel, p.X - MidPointPosition.Value.X, p.Y - MidPointPosition.Value.Y);
                    MidPointPosition = p;
                }
                else
                {
                    MidPointPosition = e.GetPosition(TemplateGrid);
                }
            }
            else
            {
                MidPointPosition = e.GetPosition(TemplateGrid);
            }
        }

        //event for zoom ability
        private void TemplateGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            XO1.TransformationOperations.ScaleUIElement(TemplateStackPanel, e.Delta > 0, e.GetPosition(TemplateStackPanel));
        }

        //if mouse leave the area, MidPointProperty should be null for pan ability to work correctly
        private void TemplateGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MidPointPosition = null;
        }

        //defines the openning transform of the false sheet
        private void BringImage()
        {
            TransformGroup TG = new TransformGroup();
            ScaleTransform ST = new ScaleTransform();
            ST.ScaleX = 0.085; //changing scale X and Y changes the initial size of the image when the Viewer is opened.
            ST.ScaleY = 0.085; //
            TG.Children.Insert(0, ST);
            TemplateStackPanel.RenderTransform = TG;
        }

        private void SaveCanvas(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog SFD = new System.Windows.Forms.SaveFileDialog();
            SFD.AddExtension = true;
            SFD.DefaultExt = "png";
            if (SFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XO1.TransformationOperations.SaveCanvastoFile(TemplateCanvas, SFD.FileName, 4096,5248);
            }
        }
    }
}
