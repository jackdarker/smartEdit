using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using smartEdit.Core;

namespace smartEdit {
    public partial class VwCode : Form, Core.IDataView {

        private ViewData m_Data = new ViewData();

        public VwCode() {
            InitializeComponent();
            toolStrip.Visible = false; //toolstrip is merged in MDIParent
        }
        public virtual ViewData GetViewData() {
            return m_Data;
        }
        public void LoadFile(String File) {
            this.m_Data.File = File;
            this.Text = Path.GetFileName(File);
            this.widgetCode.LoadFile(File);
        }
        public void SaveFile(String File) {
            if (File != String.Empty) {
                this.m_Data.File = File;
                this.Text = Path.GetFileName(File);
            }
            //Todo show dialog for SaveAs...
            if (this.m_Data.File != String.Empty) {
                this.widgetCode.SaveFile(this.m_Data.File);
            }
        }
        public virtual ToolStrip GetToolbar() {
            return this.toolStrip;
        }
        CmdStack m_CmdStack = new CmdStack();
        public virtual CmdStack GetCmdStack() {
            return m_CmdStack;
        }
        public void OnToolChanged(object sender, smartEdit.Core.MouseOperation Op) { }
        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            if (m_Data.Modified && e.CloseReason == CloseReason.UserClosing) {
                if (DialogResult.Cancel == MessageBox.Show(this, "Data modified. Close without saving?", "Close without saving", MessageBoxButtons.OKCancel)) {
                    e.Cancel = true;
                } else {
                    DialogResult = DialogResult.OK;
                }
                return;
            }

            if (!m_Data.Modified) {
                DialogResult = DialogResult.OK;
                return;
            }
        }

        public void SetController(Core.ControllerDocument Controller) {
            this.widgetCode.SetController(Controller);
        }
        public void OnUpdateEvent(object sender, EventArgs e) {
        }

        private void saveToolStripButton_Click(object sender, EventArgs e) {
            SaveFile("");
        }

        private void toolStripButton1_Click(object sender, EventArgs e) {
            MessageBox.Show(this.Text);
        }

    }



}
