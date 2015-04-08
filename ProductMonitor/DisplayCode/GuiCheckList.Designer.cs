namespace ProductMonitor.DisplayCode
{
    partial class GuiCheckList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checksGrid = new SourceGrid.Grid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checksGrid
            // 
            this.checksGrid.AutoSize = true;
            this.checksGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.checksGrid.Location = new System.Drawing.Point(0, 0);
            this.checksGrid.Name = "checksGrid";
            this.checksGrid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            this.checksGrid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.checksGrid.Size = new System.Drawing.Size(521, 407);
            this.checksGrid.TabIndex = 0;
            this.checksGrid.TabStop = true;
            this.checksGrid.ToolTipText = "";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.checksGrid);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(521, 407);
            this.panel1.TabIndex = 4;
            // 
            // GUI_Check_List
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 431);
            this.Controls.Add(this.panel1);
            this.Name = "GuiCheckList";
            this.Text = "GUI_Check_List";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GUI_Check_List_FormClosed);
            this.Load += new System.EventHandler(this.GUI_Check_List_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SourceGrid.Grid checksGrid;
        private System.Windows.Forms.Panel panel1;
    }
}