using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.OleDb;
using System.Text;
using System.Windows.Forms;

namespace smartEdit {
    public partial class DBConnection : Form {
        [Serializable]
        public class DBConnectionData {
            private string m_Server;
            public string Server {
                get {
                    return m_Server;
                }
                set {
                    m_Server = value;
                }
            }

            private string m_DataBase;
            public string DataBase {
                get {
                    return m_DataBase;
                }
                set {
                    m_DataBase = value;
                }
            }

            private string m_User;
            public string User {
                get {
                    return m_User;
                }
                set {
                    m_User = value;
                }
            }

            private string m_Pwd;
            public string Pwd {
                get {
                    return m_Pwd;
                }
                set {
                    m_Pwd = value;
                }
            }
        }
        public DBConnection() {
            InitializeComponent();
        }
        public void SetCfgData(DBConnectionData Data) {
            DisconnectDB();
            m_Data = Data;
            ServerName.Text = m_Data.Server;
            DBName.Text = m_Data.DataBase;
            User.Text = m_Data.User;
            Password.Text = m_Data.Pwd;
        }
        public DBConnectionData GetCfgData() {
            return m_Data;
        }
        public void ConnectToDB() {
            try {
                if (m_Conn != null) DisconnectDB();

                string connectionString = m_Data.DataBase + m_Data.Server + m_Data.User + m_Data.Pwd;
                //"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=L:\\Projects\\wxWindows_Projects\\Bilderalbum\\Bilderalbum.mdb;Persist Security Info=True";
                m_Conn = new OleDbConnection(connectionString);
                m_Conn.Open();
            } catch (Exception ex) {
                throw (ex);
            }
        }
        public void DisconnectDB() {
            if (ConnectionOK()) {
                m_Conn.Close();
            };
            if (m_Conn != null) {
                m_Conn.Dispose();
            }
            m_Conn = null;
        }
        public bool ConnectionOK() {
            if (m_Conn == null) return false;

            if (m_Conn.State == ConnectionState.Executing ||
                m_Conn.State == ConnectionState.Fetching ||
                m_Conn.State == ConnectionState.Open)
                return true;

            return false;
        }
        public OleDbConnection GetConnection() {
            return m_Conn;
        }
        private void Form1_Load(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {
            try {
                m_Data.Server = ServerName.Text;
                m_Data.DataBase = DBName.Text;
                m_Data.User = User.Text;
                m_Data.Pwd = Password.Text;
                ConnectToDB();
                //if no exception, close window
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private OleDbConnection m_Conn = null;
        private DBConnectionData m_Data = new DBConnectionData();

        private void button2_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
