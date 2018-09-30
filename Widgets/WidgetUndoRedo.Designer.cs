namespace smartEdit.Widgets {
    partial class WidgetUndoRedo {
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btUndo = new System.Windows.Forms.Button();
            this.btRedo = new System.Windows.Forms.Button();
            this.cbUndoRedo = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btUndo);
            this.flowLayoutPanel1.Controls.Add(this.btRedo);
            this.flowLayoutPanel1.Controls.Add(this.cbUndoRedo);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(247, 32);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btUndo
            // 
            this.btUndo.Enabled = false;
            this.btUndo.Location = new System.Drawing.Point(3, 3);
            this.btUndo.Name = "btUndo";
            this.btUndo.Size = new System.Drawing.Size(52, 23);
            this.btUndo.TabIndex = 0;
            this.btUndo.Text = "Undo";
            this.btUndo.UseVisualStyleBackColor = true;
            // 
            // btRedo
            // 
            this.btRedo.Enabled = false;
            this.btRedo.Location = new System.Drawing.Point(61, 3);
            this.btRedo.Name = "btRedo";
            this.btRedo.Size = new System.Drawing.Size(52, 23);
            this.btRedo.TabIndex = 1;
            this.btRedo.Text = "Redo";
            this.btRedo.UseVisualStyleBackColor = true;
            // 
            // cbUndoRedo
            // 
            this.cbUndoRedo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUndoRedo.Enabled = false;
            this.cbUndoRedo.FormattingEnabled = true;
            this.cbUndoRedo.Location = new System.Drawing.Point(119, 3);
            this.cbUndoRedo.Name = "cbUndoRedo";
            this.cbUndoRedo.Size = new System.Drawing.Size(121, 21);
            this.cbUndoRedo.TabIndex = 2;
            // 
            // WidgetUndoRedo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "WidgetUndoRedo";
            this.Size = new System.Drawing.Size(247, 32);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btUndo;
        private System.Windows.Forms.Button btRedo;
        private System.Windows.Forms.ComboBox cbUndoRedo;
    }
}
