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
        [Serializable]
        public class VwData {
            private string m_File;
            public string File {
                get {
                    return m_File;
                }
                set {
                    m_File = value;
                    Modified = true;
                }
            }

            [NonSerialized]
            public bool Modified;
        }

        private VwData m_Data = new VwData();

        public VwCode() {
            InitializeComponent();

        }

        public void LoadFile(String File) {
            this.m_Data.File = File;
            this.Text = File;
            this.widgetCode.LoadFile(File);
        }
        CmdStack m_CmdStack = new CmdStack();
        public virtual CmdStack GetCmdStack() {
            return m_CmdStack;
        }
        public void OnToolChanged(object sender, smartEdit.Core.MouseOperation Op) { }
        private void DBQuery_FormClosing(object sender, FormClosingEventArgs e) {
            if (m_Data.Modified && e.CloseReason == CloseReason.UserClosing) {
                if (DialogResult.Cancel == MessageBox.Show(this, "Abfrage modifiziert. Schließen ohne speichern?", "Schließen ohne speichern", MessageBoxButtons.OKCancel)) {
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
        private void DBQuery_FormClosed(object sender, FormClosedEventArgs e) {
            ;
        }
        public Core.ControllerDocument GetController() { return m_Controller; }
        public void SetController(Core.ControllerDocument Controller) {
            m_Controller = Controller;  //dont register??
            this.widgetCode.SetController(Controller);
        }
        public void OnUpdateEvent(object sender, EventArgs e) {
        }
        Core.ControllerDocument m_Controller = null;

        private void widgetDiagramPage1_Click(object sender, EventArgs e) {
            MessageBox.Show("test");
        }

    }



}
