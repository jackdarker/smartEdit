using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace smartEdit {
    public partial class VwDiagram : Form, Core.IDataView {
        [Serializable]
        public class VwDiagramData {
            private string m_Query;
            public string Query {
                get {
                    return m_Query;
                }
                set {
                    m_Query = value;
                    Modified = true;
                }
            }
            private DBConnection.DBConnectionData m_Conn;
            public DBConnection.DBConnectionData ConnString {
                get {
                    return m_Conn;
                }
                set {
                    m_Conn = value;
                    Modified = true;
                }
            }

            [NonSerialized]
            public bool Modified;
        }

        private VwDiagramData m_Data = new VwDiagramData();

        public VwDiagram() {
            InitializeComponent();
        }

        protected event smartEdit.Core.ShapeSelectedEventHandler EventSelect;
        public void RegisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect += new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        public void UnregisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect -= new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        protected virtual void FireShapeSelected(object sender, smartEdit.Core.ShapeInterface Shape) {//??notused
            smartEdit.Core.ShapeSelectedEventHandler handler = EventSelect;
            if (handler != null) {
                handler(sender, Shape);
            }
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
            this.widgetDiagramPage1.SetController(Controller);
        }
        public void OnUpdateEvent(object sender, EventArgs e) {
            this.Text = "123"; // GetController().GetCmdStack().GetActiveStack().CanUndo();
        }
        Core.ControllerDocument m_Controller = null;

        private void widgetDiagramPage1_Click(object sender, EventArgs e) {
            MessageBox.Show("test");
        }

    }



}
