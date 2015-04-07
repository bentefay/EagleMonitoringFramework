using System;
using System.Windows.Forms;
using ProductMonitor.Generic;

namespace ProductMonitor.Display_Code
{
    public partial class LogViewer : Form
    {
        public LogViewer()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            logBox.Text = "";
        }
    }
}