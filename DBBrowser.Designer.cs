namespace smartEdit {
    partial class DBBrowser {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuTable = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenView = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(150, 509);
            this.treeView1.TabIndex = 0;
            // 
            // contextMenuTable
            // 
            this.contextMenuTable.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenView});
            this.contextMenuTable.Name = "contextMenuTable";
            this.contextMenuTable.Size = new System.Drawing.Size(135, 26);
            this.contextMenuTable.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuTable_ItemClicked);
            this.contextMenuTable.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuTable_Opening);
            // 
            // OpenView
            // 
            this.OpenView.Name = "OpenView";
            this.OpenView.Size = new System.Drawing.Size(134, 22);
            this.OpenView.Text = "open View";
            // 
            // DBBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeView1);
            this.Name = "DBBrowser";
            this.Size = new System.Drawing.Size(150, 509);
            this.contextMenuTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuTable;
        private System.Windows.Forms.ToolStripMenuItem OpenView;
    }
}
