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
    // PURPOSE     : Decides the signal type (warn or stop accoriding to error)
    // ===============================
    public partial class ReactionController : UserControl
    {
        //User inputs for each error type
        public enum Reaction : int { Stop = 0, Warn = 1, Ignore = 2 };
        public Reaction FalseNumber;
        public Reaction WeakPrint;
        public Reaction ExcessivePrint;
        public Reaction Position;
        public Reaction MissingChar;

        //initiliazation
        public ReactionController()
        {
            InitializeComponent();
            FalseNumber = Reaction.Stop;
            WeakPrint = Reaction.Warn;
            ExcessivePrint = Reaction.Warn;
            Position = Reaction.Warn;
            MissingChar = Reaction.Warn;
        }

        //returns 0,1 or 2 according to error and input
        public int ReactToSheet(FrameResult FR)//0 durdur 1 uyar 2 yoksay
        {
            int result = 3; 
            if (FR.result.Exists(o => o.data == -1))
            {
                if (result > (int) this.FalseNumber)
                    result = (int) this.FalseNumber;
                
            }
            if (FR.result.Exists(o => o.data == -2))
            {
                if (result > (int)this.WeakPrint)
                    result = (int)this.WeakPrint;
            }
            if (FR.result.Exists(o => o.data == -3))
            {
                if (result > (int)this.ExcessivePrint)
                    result = (int)this.ExcessivePrint;
            }
            if (FR.result.Exists(o => o.data == -4))
            {
                if (result > (int)this.Position)
                    result = (int)this.Position;
            }
            if (FR.result.Exists(o => o.data == -5))
            {
                if (result > (int)this.WeakPrint)
                    result = (int)this.WeakPrint;
            }
            return result;
        }


        //events for radioButtons
        private void ChangeReaction(out Reaction reactionToBeChanged , Reaction value)
        {
            reactionToBeChanged = value;
        }

        private void FStopCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.FalseNumber, Reaction.Stop);
        }

        private void FWarnCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.FalseNumber, Reaction.Warn);
        }

        private void WStopCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.WeakPrint, Reaction.Stop);
        }

        private void WWarnCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.WeakPrint, Reaction.Warn);
        }

        private void WIgnoreCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.WeakPrint, Reaction.Ignore);
        }

        private void EStopCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.ExcessivePrint, Reaction.Stop);
        }

        private void EWarnCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.ExcessivePrint, Reaction.Warn);
        }

        private void EIgnoreCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.ExcessivePrint, Reaction.Ignore);
        }

        private void PStopCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.Position, Reaction.Stop);
        }

        private void PWarnCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.Position, Reaction.Warn);
        }

        private void PIgnoreCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.Position, Reaction.Ignore);
        }

        private void MStopCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.MissingChar, Reaction.Stop);
        }

        private void MWarnCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.MissingChar, Reaction.Warn);
        }

        private void MIgnoreCB_Checked(object sender, RoutedEventArgs e)
        {
            ChangeReaction(out this.MissingChar, Reaction.Ignore);
        }
    }
}
