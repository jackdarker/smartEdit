namespace smartEdit.Widgets {
    partial class Component1 {
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
            this.widgetPropertyBrowser1 = new smartEdit.PropertyBrowser.WidgetPropertyBrowser();
            this.button1 = new System.Windows.Forms.Button();
            // 
            // widgetPropertyBrowser1
            // 
            this.widgetPropertyBrowser1.Location = new System.Drawing.Point(0, 0);
            this.widgetPropertyBrowser1.Name = "widgetPropertyBrowser1";
            this.widgetPropertyBrowser1.Size = new System.Drawing.Size(379, 564);
            this.widgetPropertyBrowser1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;

        }

        #endregion

        private PropertyBrowser.WidgetPropertyBrowser widgetPropertyBrowser1;
        private System.Windows.Forms.Button button1;
    }
}
