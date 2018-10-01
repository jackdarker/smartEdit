using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Windows.Forms;

namespace smartEdit {
    public partial class DBBrowser : UserControl {
        public class BrowserNode : TreeNode {
            public bool IsTable;
        }
        public DBBrowser() {
            InitializeComponent();
            UpdateView();
        }
        public void UpdateView() {
            treeView1.CollapseAll();
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            //if (m_Adapter!= null) m_Adapter.Fill(table);

            BrowserNode Root = new BrowserNode();
            Root.Name = "Database";
            Root.Text = "MyDatabase";

            BrowserNode Tables = BuildTablesInfo(m_Tables);
            Root.Nodes.Add(Tables);

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(Root);

            treeView1.EndUpdate();

        }
        public BrowserNode GetTableInfo(DataTable Table) {
            BrowserNode TableNode = new BrowserNode();
            if (Table == null) return TableNode;
            TableNode.Name = TableNode.Text = Table.TableName;
            TableNode.IsTable = true;
            TableNode.ContextMenuStrip = this.contextMenuTable;
            foreach (DataColumn col in Table.Columns) {
                TableNode.Nodes.Add(col.ColumnName.ToString() + " (" + col.DataType.Name + ")");
            }
            return TableNode;
        }
        public BrowserNode BuildTablesInfo(DataTable TableCatalog) {
            BrowserNode RootNode = new BrowserNode();
            if (TableCatalog == null) return RootNode;
            RootNode.Name = RootNode.Text = TableCatalog.TableName;
            foreach (DataRow row in TableCatalog.Rows) {
                DataTable InfoTbl = new DataTable();
                InfoTbl.TableName = (string)row["Table_Name"];
                OleDbDataAdapter Adapter = new OleDbDataAdapter("select * from " + InfoTbl.TableName, m_ConnHelper.GetConnection());
                Adapter.Fill(InfoTbl);
                BrowserNode TableNode = GetTableInfo(InfoTbl);
                RootNode.Nodes.Add(TableNode);
            }
            return RootNode;
        }
        public void ConnectToDB(DBConnection.DBConnectionData connectionString) {
            DisconnectFromDB();
            m_ConnHelper.SetCfgData(connectionString);
            m_ConnHelper.ConnectToDB();

            m_Tables = m_ConnHelper.GetConnection().GetOleDbSchemaTable(
                            OleDbSchemaGuid.Tables,
                            new object[] { null, null, null, "TABLE" });
            UpdateView();
        }
        public void DisconnectFromDB() {
            if (m_Adapter != null) m_Adapter.Dispose();
            m_Adapter = null;
            if (m_Tables != null) m_Tables.Dispose();
            m_Tables = null;
            m_ConnHelper.DisconnectDB();
            UpdateView();
        }

        private OleDbDataAdapter m_Adapter = null;
        private DataTable m_Tables = null;
        private DBConnection m_ConnHelper = new DBConnection();

        private void contextMenuTable_Opening(object sender, CancelEventArgs e) {
            e.Cancel = false;
        }

        private void contextMenuTable_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (sender == null) return;

            if (e.ClickedItem.Equals(OpenView)) {
                ((MDIParent)this.ParentForm).NewEditor();
                return;
            }

        }
    }
}
