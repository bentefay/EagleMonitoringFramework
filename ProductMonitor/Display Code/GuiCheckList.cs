using System;
using System.Windows.Forms;
using ProductMonitor.ProgramCode;

namespace ProductMonitor.Display_Code
{
    public partial class GuiCheckList : Form
    {
        public GuiCheckList()
        {
            InitializeComponent();
        }

        private void GUI_Check_List_Load(object sender, EventArgs e)
        {
            //useless corner tile
            checksGrid.Columns.Insert(0);
            checksGrid.Rows.Insert(0);

            //columns
            checksGrid.Columns.Insert(0);
            checksGrid.Columns.Insert(0);
            checksGrid.Columns.Insert(0);

            //add rows
            foreach (Check c in Program.GetChecks())
            {
                checksGrid.Rows.Insert(0);
                SourceGrid.Cells.Views.Cell rowView= new SourceGrid.Cells.Views.Cell();
                SourceGrid.Cells.Views.CheckBox checkView = new SourceGrid.Cells.Views.CheckBox();

                System.Drawing.Color cellColour;
                if (c.IsTriggered())
                {
                    cellColour = System.Drawing.Color.Red;
                }
                else if (c.HasError())
                {
                    cellColour = System.Drawing.Color.Yellow;
                }
                else if (c.IsPaused())
                {
                    cellColour = System.Drawing.Color.LightBlue;
                }
                else
                {
                    cellColour = System.Drawing.Color.White;
                }

                rowView.BackColor = cellColour;
                checkView.BackColor = cellColour;

                checksGrid[1, 0] = new SourceGrid.Cells.RowHeader(c.getIndex());
                checksGrid[1, 1] = new SourceGrid.Cells.Cell(c.GetCheckType());
                checksGrid[1, 1].View = rowView;
                checksGrid[1, 2] = new SourceGrid.Cells.Cell(c.GetLocation());
                checksGrid[1, 2].View = rowView;
                SourceGrid.Cells.CheckBox pauseBox = new SourceGrid.Cells.CheckBox();
                pauseBox.Checked = c.IsPaused();

                checksGrid[1, 3] = pauseBox;
                checksGrid[1, 3].View = checkView;

            }
            
            //add column headers
            checksGrid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
            checksGrid[0, 2] = new SourceGrid.Cells.ColumnHeader("Location");
            checksGrid[0, 3] = new SourceGrid.Cells.ColumnHeader("Paused");

            checksGrid.AutoSizeCells();
        }

        private void GUI_Check_List_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Threading.ThreadStart starter = 
                new System.Threading.ThreadStart(form_closed);

            System.Threading.Thread thread = new System.Threading.Thread(starter);

            thread.IsBackground = true;

            thread.Start();
        }

        void form_closed()
        {
            for (int i = 1; i < checksGrid.RowsCount; i++)
            {
                int index = (int)checksGrid[i, 0].Value;
                if ((bool)checksGrid[i, 3].Value)
                {
                    Program.PauseCheck(index);
                }
                else
                {
                    Program.UnpauseCheck(index);
                }
            }
        }
    }
}