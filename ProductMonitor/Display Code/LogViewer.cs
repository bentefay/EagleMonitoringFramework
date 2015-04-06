using System;
using System.Windows.Forms;

namespace ProductMonitor.Display_Code
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            logBox.Text = Product_Monitor.Generic.Logger.getInstance().GetLog();
        }
    }
}