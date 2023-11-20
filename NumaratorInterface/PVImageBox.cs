using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DALSA.SaperaLT.SapClassBasic;

namespace NumaratorInterface
{
    public partial class PVImageBox : UserControl
    {

        //   private PictureBox picBox;
        protected SapView m_pView;
        private int RightOffset, BottomOffset;
       // public Window WND = null;

        public PVImageBox()
        {
            InitializeComponent();
            RightOffset = 500;
            BottomOffset = 0;


        }
        private void PVImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (m_pView != null && m_pView.Initialized)
            {
                //  FitImageBoxToBottomRight();
                m_pView.OnPaint();
            }
            //base.OnPaint(e);
        }



        public void OnSize()
        {
            if (m_pView != null && m_pView.Initialized)
            {
                //FitImageBoxToBottomRight();
                m_pView.OnSize();

            }
        }
        public SapView View
        {
            get { return m_pView; }
            set
            {
                m_pView = value;
                if (m_pView != null)
                {
                    m_pView.Window = picBox;
                }
            }
        }
        public Rectangle ViewRectangle
        {
            get
            {
                System.Drawing.Size ViewSize = new System.Drawing.Size(1800, 1000);
                return new Rectangle(this.Location, ViewSize);
            }
        }
    }

    
}
