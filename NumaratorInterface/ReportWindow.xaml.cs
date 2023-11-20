﻿using System;
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
using System.Windows.Shapes;

namespace NumaratorInterface
{
    // ===============================
    // AUTHOR     : Sinan KAPOĞLU
    // UPDATE DATE     : 20.08.2016
    // PURPOSE     : Contains "ReportController" UserControl. When it is closed Direct Window is showed.
    // ===============================
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ReportController.FalseSheetController.ClearMemory();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }
}
