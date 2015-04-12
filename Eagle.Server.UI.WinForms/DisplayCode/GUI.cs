using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Eagle.Server.Framework;
using Serilog;
using SourceGrid;
using SourceGrid.Cells;

namespace Eagle.Server.UI.WinForms.DisplayCode
{
    public partial class GUI : Form
    {
        // This delegate enables asynchronous calls for setting
        // the propertys of a grid control.
        delegate void SetColorCallback(Color colour, Cell cell);
        delegate void TakeScreenshotCallback(string tab, string savefilelocation);
        delegate void SetValueCallback(String value, Cell cell);
        delegate void SetToolTipCallback(String toolTip, Cell cell);
        delegate void SetNumberOfColumnsAndRows(string[] Columns, string[] Rows, Grid grid);
        delegate void MakeTab(string name);
        SourceGrid.Cells.Controllers.ToolTipText toolTipController = new SourceGrid.Cells.Controllers.ToolTipText();

        ArrayList grids = new ArrayList();
        private readonly Func<Check[]> _getChecks;

        public GUI(Func<Check[]> getChecks)
        {
            _getChecks = getChecks;
            InitializeComponent();
            FormClosed += GUI_FormClosed;
            FormClosing += GUI_FormClosing;
            toolTipController.ToolTipTitle = "This cell has something to do with...";
            toolTipController.ToolTipIcon = ToolTipIcon.Info;
            toolTipController.IsBalloon = true;

            Text = string.Format("Product Monitor v{0}", Application.ProductVersion);

            try
            {
                WindowHelper.LoadPosition(this, "MainWindow");
            }
            catch (Exception e)
            {
                Log.Information("Failed to load window position with error: " + e.Message);
            }
        }

        void GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WindowHelper.SavePosition(this, "MainWindow");
            }
            catch (Exception ex)
            {
                Log.Information("Failed to save window position with error: " + ex.Message);
            }
        }

        void GUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            //perform the shut down tasks in a different thread to maintain GUI responciveness
            System.Threading.ThreadStart threadStarter =
                new System.Threading.ThreadStart(Program.Exit);
            System.Threading.Thread thread = new System.Threading.Thread(threadStarter);
            thread.Start();
        }

        public void AddTab(string name)
        {
            MakeNewTable(name);
        }

        public void DrawTable(string tab, IReadOnlyList<string> locations, IReadOnlyList<string> types)
        {
            Grid tabGrid = null;
            foreach (Grid g in grids)
            {
                if (g.Parent.Name == tab)
                {
                    tabGrid = g;
                }
            }

            if (tabGrid != null)
            {
                SetColumnsAndRows(locations.ToArray(), types.ToArray(), tabGrid);
            }
        }

        public void SetCell(string tab, int row, int column, string value, Color colour, string tooltip)
        {
            Grid tabGrid = null;
            foreach (Grid g in grids)
            {
                if (g.Parent.Name == tab)
                {
                    tabGrid = g;
                }
            }
            if (tabGrid != null)
            {
                SetValue(value, tabGrid[row + 1, column + 1]);
                SetBackColor(colour, tabGrid[row + 1, column + 1]);
                SetToolTip(tooltip, tabGrid[row + 1, column + 1]);
            }
        }

        private void MakeNewTable(string name)
        {
            if (tabContainer.InvokeRequired)
            {
                MakeTab d = new MakeTab(MakeNewTable);
                this.Invoke(d, new object[] { name });
            }
            else
            {
                Grid formatProvider = null;
                if (grids.Count == 0)
                {
                    formatProvider = grid1;
                }
                else
                {
                    formatProvider = (Grid)grids[0];
                }

                var newPage = new TabPage(name);
                newPage.Name = name;
                var newGrid = new Grid();
                newGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                newGrid.Parent = newPage;
                newGrid.BackColor = formatProvider.BackColor;
                newGrid.BorderStyle = formatProvider.BorderStyle;
                newGrid.Bounds = formatProvider.Bounds;
                newGrid.ForeColor = formatProvider.ForeColor;
                newGrid.Location = formatProvider.Location;
                tabContainer.TabPages.Add(newPage);
                grids.Add(newGrid);

                if (grids.Count != tabContainer.TabPages.Count)
                {
                    tabContainer.TabPages.RemoveAt(0);
                }
            }
        }

        private void SetColumnsAndRows(string[] columns, string[] rows, Grid grid)
        {

            if (grid.InvokeRequired)
            {
                SetNumberOfColumnsAndRows d = SetColumnsAndRows;
                Invoke(d, columns, rows, grid);
            }
            else
            {

                grid.Rows.Clear();
                grid.Columns.Clear();


                grid.Rows.Insert(0);
                grid.Columns.Insert(0);


                for (int row = 1; row <= rows.Length; row++)
                {
                    grid.Rows.Insert(row);
                    grid[row, 0] = new RowHeader(rows[row - 1]);

                    for (int column = 1; column <= columns.Length; column++)
                    {
                        if (row == 1)
                        {
                            grid.Columns.Insert(column);
                            grid[0, column] =
                                new SourceGrid.Cells.ColumnHeader(columns[column - 1]);
                        }

                        grid[row, column] = new Cell();
                        grid[row, column].View = new SourceGrid.Cells.Views.Cell();
                    }
                }

                grid.AutoSizeCells();

            }
        }




        private void SetBackColor(Color colour, ICell cell)
        {

            if (cell.Grid.InvokeRequired)
            {
                SetColorCallback d = new SetColorCallback(SetBackColor);
                this.Invoke(d, new object[] { colour, cell });
            }
            else
            {

                cell.View.BackColor = colour;

            }
        }

        private void SetToolTip(String toolTip, ICell cell)
        {

            if (cell.Grid.InvokeRequired)
            {
                SetToolTipCallback d = new SetToolTipCallback(SetToolTip);
                this.Invoke(d, new object[] { toolTip, cell });
            }
            else
            {

                cell.ToolTipText = toolTip;
                cell.AddController(toolTipController);
            }
        }

        private void SetValue(string value, ICell cell)
        {

            if (cell.Grid.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetValue);
                this.Invoke(d, new object[] { value, cell });
            }
            else
            {

                cell.Value = value;
                grid1.AutoSizeCells();
            }
        }

        private void viewLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LogViewer().ShowDialog();
        }

        private void pauseChecksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GuiCheckList(_getChecks).ShowDialog();
        }

        public void TakeScreenshot(string tab, string saveLocation)
        {
            if (tabContainer.InvokeRequired)
            {
                TakeScreenshotCallback d = new TakeScreenshotCallback(TakeScreenshot);
                this.Invoke(d, new object[] { tab, saveLocation });
            }
            else
            {
                try
                {
                    TabPage currentTab = tabContainer.SelectedTab;

                    tabContainer.SelectTab(tab);
                    this.Refresh();

                    this.Focus();
                    this.BringToFront();

                    Bitmap bmpScreenshot;
                    Graphics gfxScreenshot;

                    bmpScreenshot = new Bitmap(this.Width, this.Height);
                    gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                    gfxScreenshot.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, this.Size);

                    System.IO.File.Create(saveLocation).Close();

                    bmpScreenshot.Save(saveLocation, System.Drawing.Imaging.ImageFormat.Png);

                    tabContainer.SelectedTab = currentTab;
                }
                catch { }
            }
        }
    }
}