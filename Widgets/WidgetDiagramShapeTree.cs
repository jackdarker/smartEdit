using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace smartEdit.Widgets {
    public partial class WidgetDiagramShapeTree : UserControl, Core.IDataView {
        public class BrowserNode : TreeNode {
            public bool IsTable;
        }
        public WidgetDiagramShapeTree() {
            InitializeComponent();
        }
        #region event & delegates
        protected event smartEdit.Core.ShapeSelectedEventHandler EventSelect;
        public void RegisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect += new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        public void UnregisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect -= new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        protected virtual void FireShapeSelected(object sender, smartEdit.Core.ShapeInterface Shape) {
            smartEdit.Core.ShapeSelectedEventHandler handler = EventSelect;
            if (handler != null) {
                handler(sender, Shape);
            }
        }
        public void OnUpdateEvent(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new smartEdit.Core.UpdateEventHandler(OnUpdateEvent));
            } else {
                UpdateView();
            }
        }
        public void OnToolChanged(object sender, smartEdit.Core.MouseOperation Op) { }

        #endregion
        Core.ControllerDocument m_Controller = null;
        public Core.ControllerDocument GetController() { return m_Controller; }
        public void SetController(Core.ControllerDocument Ctrl) {
            if (m_Controller != null) {
                m_Controller.UnregisterView(this);
            }
            m_Controller = Ctrl;
            m_Controller.RegisterView(this);
        }
        protected void UpdateView() {
            //treeView1.CollapseAll();
            string _tmp = "";
            if (treeView1.SelectedNode != null) _tmp = treeView1.SelectedNode.Name;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(BuildTablesInfo());
            treeView1.ExpandAll();
            /*System.Collections.IEnumerator myEnumerator = treeView1.Nodes.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                string test = ((TreeNode)myEnumerator.Current).Text + "\n";
            }*/
            Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetDiagram().GetSelectedShapes();
            while (Iterator.MoveNext()) {
                _tmp = Iterator.Current.GetName();
            }
            if (_tmp != "" && (treeView1.Nodes[0] != null)) {
                int i = treeView1.Nodes[0].Nodes.IndexOfKey(_tmp);
                if (i >= 0) {
                    treeView1.SelectedNode = treeView1.Nodes[0].Nodes[i];
                }

            }
            if (treeView1.SelectedNode != null) treeView1.SelectedNode.Expand();
            treeView1.EndUpdate();
        }
        public BrowserNode GetTableInfo(Core.ShapeBase Shape) {
            BrowserNode TableNode = new BrowserNode();
            if (Shape == null) return TableNode;
            TableNode.Name = TableNode.Text = Shape.GetShapeTypeName();
            //TableNode.IsTable = true;
            //TableNode.ContextMenuStrip = this.contextMenuTable;
            //foreach (DataColumn col in Table.Columns)
            //{
            //  TableNode.Nodes.Add(col.ColumnName.ToString() + " (" + col.DataType.Name + ")");
            //}
            return TableNode;
        }
        public BrowserNode BuildTablesInfo() {
            BrowserNode Root = new BrowserNode();
            Root.Name = "Diagram";
            Root.Text = "MyDiagram";

            if (GetDiagram() == null) return Root;

            Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetDiagram().GetShapeEnumerator();
            while (Iterator.MoveNext()) {
                BrowserNode Node = new BrowserNode();
                Node.Name = Iterator.Current.GetName();
                Node.Text = Iterator.Current.GetName() +
                    " [" + Iterator.Current.GetShapeTypeName() + "]";
                Root.Nodes.Add(Node);
            }

            return Root;
        }
        public Core.ModelDiagram GetDiagram() {
            return GetController().GetModel();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {

            Core.ShapeInterface Shape = GetDiagram().FindShape(e.Node.Name);
            //if(Keys.Control != (Control.ModifierKeys & Keys.Control)) GetDiagram().UnselectAll();
            if (Shape != null) {
                FireShapeSelected(this, Shape);//Shape.Select(true);

            }
        }
    }
}
