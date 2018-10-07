using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Windows.Forms;
using smartEdit.Core;

namespace smartEdit {
    public partial class MDIParent : Form {
        private class AppData : smartEdit.Core.ISerializable {
            public AppData() {
                m_FullScreen = 0;
                m_WndRectangle = new Rectangle(20, 20, 600, 400);
            }
            public void WriteToSerializer(smartEdit.Core.SerializerBase Stream) {
                Stream.WriteData("FullScreen", m_FullScreen);
                Stream.WriteData("WndRectangle", m_WndRectangle);
            }
            public void ReadFromSerializer(smartEdit.Core.SerializerBase Stream) {
                m_FullScreen = Stream.ReadAsInt("FullScreen");
                m_WndRectangle = Stream.ReadAsRect("WndRectangle");
            }
            public int m_FullScreen;
            public Rectangle m_WndRectangle;
        }
        smartEdit.Widgets.WidgetProject tvProjects;
        public MDIParent() {
            m_AppData = new AppData();
            InitializeComponent();
            menuStrip.MdiWindowListItem = windowsMenu;
          //  Controls.Add( new DockingControl(this,DockStyle.Left,new Widgets.WidgetCodePage()));

            tvProjects = new Widgets.WidgetProject();
            ControllerDocument Controller = ControllerDocument.Instance;
            Controller.SetTopLevelForm(this);
              Controller.GetCmdStack().EventCanRedoChanged += new EventHandler<smartEdit.Core.CmdStackGroup.BoolEventArgs>(
                delegate(object sender, smartEdit.Core.CmdStackGroup.BoolEventArgs e) { btRedo.Enabled = e.State; });
              Controller.GetCmdStack().EventCanUndoChanged += new EventHandler<smartEdit.Core.CmdStackGroup.BoolEventArgs>(
                delegate(object sender, smartEdit.Core.CmdStackGroup.BoolEventArgs e) { btUndo.Enabled = e.State; });
              Controller.GetCmdStack().EventUpdate += new EventHandler<EventArgs>(
                delegate(object sender, EventArgs e) {
                    btRedo.ToolTipText = Controller.GetCmdStack().GetRedoText();
                    btUndo.ToolTipText = Controller.GetCmdStack().GetUndoText();
                });
            btRedo.Click += new EventHandler(Controller.GetCmdStack().RedoEvent);
            btUndo.Click += new EventHandler(Controller.GetCmdStack().UndoEvent);
            this.tabForms.OnClose+=new TabCtlEx.OnHeaderCloseDelegate(tabForms_OnClose);
        }
        private void ShowNewForm(object sender, EventArgs e) {
            NewEditor();
        }
        private string GetIniFileName() {
            string File = Application.StartupPath + Application.ProductName + ".cfg";
            return File;
        }
        private void SaveToFile() {
            smartEdit.Core.SerializerXML _stream = null;
            try {
                _stream = new smartEdit.Core.SerializerXML("SMARTEDIT-APP", "1.0.0.0");
                _stream.OpenOutputStream(GetIniFileName());
                _stream.WriteElementStart("Application");
                m_AppData.WriteToSerializer(_stream);
                _stream.WriteElementEnd("Application");
                _stream.CloseOutputStream();
                _stream = null;
            } catch (Exception e) {
                throw (e);
            } finally {
                if (_stream != null) _stream.CloseOutputStream();
            }
        }
        private void LoadFile() {
            string DocType = string.Empty;
            smartEdit.Core.SerializerXML _stream = null;
            try {
                _stream = new smartEdit.Core.SerializerXML("SMARTEDIT-APP", "1.0.0.0");
                _stream.OpenInputStream(GetIniFileName());
                if (_stream.GetDetectedSerializerName() != "SMARTEDIT-APP")
                    throw new FormatException("");
                string NodeGroup;
                do {
                    NodeGroup = _stream.GetNodeName();
                    if (_stream.GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                        if (NodeGroup == "Application") {
                            m_AppData.ReadFromSerializer(_stream);
                        }

                    }
                } while (_stream.ReadNext());

                _stream.CloseInputStream();
                _stream = null;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            } finally {
                if (_stream != null) _stream.CloseInputStream();
            }
        }
        

        public VwCode NewEditor() {
            VwCode childForm = new VwCode();
            childForm.SetController(ControllerDocument.Instance);  //do we need this?
            childForm.MdiParent = this;
            childForm.Activated += new EventHandler(childForm_Activated);
            childForm.Text = "Window " + (++childFormNumber);
            childForm.WindowState = FormWindowState.Maximized;
            childForm.Show();
            this.ActivateMdiChild(childForm);
            return childForm;
        }
        void tabForms_OnClose(object sender, CloseEventArgs e) {
            if (e.TabIndex < 0) return;
            TabPage _Page = this.tabForms.TabPages[e.TabIndex];
            Form _frm = (Form)_Page.Tag;
            this.tabForms.Controls.Remove(_Page);
            _frm.Close();

        }
        void childForm_Activated(object sender, EventArgs e) {
            if (this.ActiveMdiChild == null) {// If no any child form, hide tabControl 
                tabForms.Visible = false;
                
            } else {// Child form always maximized 
               //?? this.ActiveMdiChild.WindowState = FormWindowState.Maximized;
                // If child form is new and no has tabPage, 
                // create new tabPage 
                if (this.ActiveMdiChild.Tag == null) {
                    // Add a tabPage to tabControl with child 
                    // form caption 
                    TabPage tp = new TabPage(this.ActiveMdiChild.Text);
                    tp.Tag = this.ActiveMdiChild;
                    tp.Parent = tabForms;
                    tabForms.SelectedTab = tp;

                    this.ActiveMdiChild.Tag = tp;
                    this.ActiveMdiChild.FormClosed +=
                        new FormClosedEventHandler( ActiveMdiChild_FormClosed);
                }

                if (!tabForms.Visible) tabForms.Visible = true;

            }
            //revert merged toolbar and merge new
            ToolStripManager.RevertMerge(toolMain);
            IView frmChild = (IView)this.ActiveMdiChild;
            if (frmChild != null) {
                // The frmChild.FormToolStrip is a property that exposes the
                // toolstrip on your child form
                ToolStripManager.Merge(frmChild.GetToolbar(), toolMain);
            }
            ControllerDocument.Instance.OnViewChanged(ActiveMdiChild, (IView)ActiveMdiChild);
        }
        private void ActiveMdiChild_FormClosed(object sender,FormClosedEventArgs e) {
            ((sender as Form).Tag as TabPage).Dispose();
            ToolStripManager.RevertMerge(toolMain);
        }
        private void tabForms_SelectedIndexChanged(object sender, TabControlEventArgs e) {
            if ((tabForms.SelectedTab != null) &&
                (tabForms.SelectedTab.Tag != null))
                (tabForms.SelectedTab.Tag as Form).Select();
        }
        /* private void OpenFile(object sender, EventArgs e)
         {
             if (this.ActiveMdiChild == null) CreateNewDiagram();
             if (this.ActiveMdiChild != null) ((VwDiagram)this.ActiveMdiChild).LoadFile();
         }*/
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }
        private void CutToolStripMenuItem_Click(object sender, EventArgs e) {
        }
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e) {
        }
        private void PasteToolStripMenuItem_Click(object sender, EventArgs e) {
        }
        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e) {
            toolMain.Visible = toolBarToolStripMenuItem.Checked;
        }
        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e) {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.Cascade);
        }
        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileVertical);
        }
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileHorizontal);
        }
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (Form childForm in MdiChildren) {
                childForm.Close();
            }
        }
        private void MDIParent1_Load(object sender, EventArgs e) {
            LoadFile();
            SaveToFile();
            if (!SystemInformation.VirtualScreen.Contains(m_AppData.m_WndRectangle)) {
                m_AppData.m_WndRectangle = Screen.FromPoint(new Point(1, 1)).WorkingArea;
            }
            this.Location = m_AppData.m_WndRectangle.Location;
            this.Size = m_AppData.m_WndRectangle.Size;
            this.WindowState = (m_AppData.m_FullScreen == 1) ? FormWindowState.Maximized : FormWindowState.Normal;
        }
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {

            //FormConn.ShowDialog();

        }
        private void MDIParent1_FormClosing(object sender, FormClosingEventArgs e) {
            bool AskUser = false;
            foreach (Form form in MdiChildren) {
                if (form.DialogResult != DialogResult.OK) AskUser = true;
            }

            /*??if (AskUser)
            {
                if (DialogResult.Cancel == MessageBox.Show(this, "Abfrage modifiziert. Schließen ohne speichern?", "Schließen ohne speichern", MessageBoxButtons.OKCancel))
                {
                    e.Cancel = true;
                }
                else
                {
                    
                }

            }*/
            if (this.WindowState == FormWindowState.Maximized) {
                m_AppData.m_FullScreen = 1;//if Maximized, read Restorebounds
                m_AppData.m_WndRectangle.Location = this.RestoreBounds.Location;
                m_AppData.m_WndRectangle.Size = this.RestoreBounds.Size;
            } else {
                m_AppData.m_FullScreen = 0;
                m_AppData.m_WndRectangle.Location = this.Location;
                m_AppData.m_WndRectangle.Size = this.Size;
            }

            SaveToFile();
            e.Cancel = false;
            //    if (this.ActiveMdiChild != null)
            //    e.Cancel = this.ActiveMdiChild.DialogResult != DialogResult.OK;

        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            new Form1().Show();
        }
        #region field
        private AppData m_AppData;
        private int childFormNumber = 0;
        #endregion

        private void Menu_DropDownOpening(object sender, EventArgs e) {
            ToolStripMenuItem Menu = (ToolStripMenuItem)sender;
            Menu.DropDownItems.Clear();
            if (ActiveMdiChild is smartEdit.Core.IView) {
                Menu.DropDownItems.AddRange(ControllerDocument.Instance.GetViewMenuStrip((smartEdit.Core.IView)ActiveMdiChild, Menu));
            }
        }

        private void widgetDiagramShapeTree1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {

            MessageBox.Show("test");

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) {

        }

        private void mnuItemShowLogListClick(object sender, EventArgs e) {

        }

        
    }
}
