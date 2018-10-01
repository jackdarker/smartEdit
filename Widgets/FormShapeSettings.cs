using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace smartEdit.Widgets {
    public partial class FormShapeSettings : Form {
        public FormShapeSettings() {
            InitializeComponent();
        }
        public void AddPropertyPage(TabPage[] Pages) {
            if (Pages != null) {
                this.tabControl1.Controls.AddRange(Pages);
            }
        }
        public void SetObject(object EditThis) {
            this.propertyGrid1.SelectedObject = EditThis;
        }
        public void SetObject(object[] EditThis) {
            this.propertyGrid1.SelectedObjects = EditThis;
        }
        private void btOK_Click(object sender, EventArgs e) {
            try {
                //m_Data.Server = ServerName.Text;
                //if no exception, close window
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }
        private void btCancel_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
