namespace smartEdit {
    partial class WidgetShapePalette {
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
            this.ShapeList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // ShapeList
            // 
            this.ShapeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShapeList.FormattingEnabled = true;
            this.ShapeList.Items.AddRange(new object[] {
            "Pointer",
            "ConnectorStraight",
            "Rectangle",
            "Ellipse"});
            this.ShapeList.Location = new System.Drawing.Point(0, 0);
            this.ShapeList.Name = "ShapeList";
            this.ShapeList.Size = new System.Drawing.Size(150, 147);
            this.ShapeList.TabIndex = 0;
            this.ShapeList.Click += new System.EventHandler(this.ShapeList_Click);
            // 
            // WidgetShapePalette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ShapeList);
            this.Name = "WidgetShapePalette";
            this.Size = new System.Drawing.Size(150, 158);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ShapeList;
    }
}
