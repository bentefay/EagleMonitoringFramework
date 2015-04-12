using System;
using System.Windows.Forms;

namespace Eagle.Server.UI.WinForms.DisplayCode
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