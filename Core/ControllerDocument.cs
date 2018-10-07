using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace smartEdit.Core {
    //this is the controller that connects the Editor with the Model
    public class ControllerDocument : IController {
        private static volatile ControllerDocument instance;
        private static object syncRoot = new Object();
        public static ControllerDocument Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null)
                            instance = new ControllerDocument();
                    }
                }

                return instance;
            }
        }
        private ControllerDocument() {
            m_CmdStackGroup = new CmdStackGroup();
            m_Views = new ViewManager();
            //m_CmdStackGroup.EventUpdate += new EventHandler<EventArgs>(OnUpdate);
          //  SetActiveModel(new smartEdit.Core.ModelDocument());
        }
        private IView m_ActiveView = null;

        public IView GetActiveView() {
            return m_ActiveView;
        }

        #region Event & Delegates

        /*public event EventHandler<smartEdit.Core.CmdStackGroup.BoolEventArgs> EventCanRedoChanged;
        public event EventHandler<smartEdit.Core.CmdStackGroup.BoolEventArgs> EventCanUndoChanged;
        /// <summary>
        /// Connect this to an Event-Source (f.e. button click) to trigger Redo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RedoEvent(object sender, EventArgs e)
        {
            if (this != null) m_CmdStackGroup.Redo();
        }
        /// <summary>
        /// Connect this to an Event-Source (f.e. button click) to trigger Undo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UndoEvent(object sender, EventArgs e)
        {
            if (this != null) m_CmdStackGroup.Undo();
        }*/
        public event UpdateEventHandler EventUpdate;
        protected virtual void FireUpdateEvent(object sender, EventArgs e) {
            UpdateEventHandler handler = EventUpdate;
            if (handler != null) {
                handler(sender, e);
            }
        }

        public event MouseFeedbackEventHandler EventMouseFeedback;
        public virtual void OnMouseInput(object sender, smartEdit.Core.MouseOperation e) {
            MouseFeedbackEventHandler handler = EventMouseFeedback;
            if (handler != null) handler(sender, e);

        }
        
        public event ViewChangedEventHandler EventViewChanged;
        public virtual void OnViewChanged(object sender, IView View) {
            m_ActiveView = View;
            ViewChangedEventHandler handler = EventViewChanged;
            if (handler != null) {
                handler(sender, View);
            }
        }

        public virtual void ToolChanged() { }
        #endregion
        #region Save/Load
        public virtual void LoadFromFile(string FileName) {
            SetActiveModel(ModelDocument.CreateFromFile(FileName));
        }
        public virtual void SaveToFile(string FileName) {
            if(GetModel()!= null)
                GetModel().SaveToFile(FileName);
        }
        public virtual void SaveToFile() {
            if (GetModel() != null)
                GetModel().SaveToFile();
        }

        #endregion
        #region Viewmanagement
        public void OpenEditorForFile(String File) {
            Form _frm = m_Views.GetFormForFile(File);
            if (_frm == null) {
                VwCode _Editor = ((MDIParent)GetTopLevelForm()).NewEditor();
                _Editor.LoadFile(File);
                _Editor.FormClosed+= new FormClosedEventHandler(
                    delegate(object sender, FormClosedEventArgs e) {
                        m_Views.RemoveForm((Form)sender); });
                m_Views.AddForm(File, _Editor);
                _Editor.Activate();
            } else {
                _frm.Activate();
            }
            //todo query controller which editor to use for this kind of file

            //if there is already the file open in editor view just push it to top
        }
        public virtual void RegisterView(IView View) {

            EventUpdate += View.OnUpdateEvent;
            GetCmdStack().EventUpdate += View.OnUpdateEvent;
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IView View) {
            GetCmdStack().EventUpdate -= View.OnUpdateEvent;
            EventUpdate -= View.OnUpdateEvent;
        }
        public virtual void RegisterView(IDataView View) {
            //View.RegisterShapeSelected(this);
            EventUpdate += View.OnUpdateEvent;
            GetCmdStack().EventUpdate += View.OnUpdateEvent;
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IDataView View) {
           // View.UnregisterShapeSelected(this);
            GetCmdStack().EventUpdate -= View.OnUpdateEvent;
            EventUpdate -= View.OnUpdateEvent;
        }
        public virtual void RegisterView(IGraphicView View) {
            RegisterView((IDataView)View);
            View.RegisterMouseInput(this);
            EventMouseFeedback += View.OnMouseFeedback;
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IGraphicView View) {
            UnregisterView((IDataView)View);
            View.UnregisterMouseInput(this);
            EventMouseFeedback -= View.OnMouseFeedback;
        }
        public virtual void RegisterView(IToolView View) {
            //EventUpdate += View.OnUpdateEvent; ??not required?
       //     View.RegisterShapeSelected(this);
            View.RegisterToolSelected(this);
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IToolView View) {
            EventUpdate -= View.OnUpdateEvent;
     //       View.UnregisterShapeSelected(this);
        }
        Form m_ParentForm;
        public void SetTopLevelForm(Form Parent) {
            m_ParentForm = Parent;
        }
        public Form GetTopLevelForm() {
            return m_ParentForm;
        }
        #endregion
        public virtual void SetActiveModel(ModelDocument Model) {
            if (m_Model != null) {
                m_Model.EventUpdate -= new UpdateEventHandler(FireUpdateEvent);
                m_Model = null;
            }
            m_Model = Model;
            m_Model.EventUpdate += new UpdateEventHandler(FireUpdateEvent);
        }
        /// <summary>
        /// Returns ToolMenuItems for the given TopLevel-Menu.  
        /// </summary>
        /// <param name="View"></param>
        /// <param name="Parent"></param>
        /// <returns></returns>
        public virtual ToolStripItem[] GetViewMenuStrip(IView View, ToolStripMenuItem Parent) {
            List<ToolStripItem> Items = new List<ToolStripItem>();
            ToolStripMenuItem Item;
            if (Parent != null) {
                if (Parent.Text == "&Edit") {
                    Item = new ToolStripMenuItem("&Undo", Properties.Resources.SymbolUndo, GetCmdStack().UndoEvent);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
                    Item.Name = "undoToolStripMenuItem";
                    Item.ToolTipText = GetCmdStack().GetUndoText();
                    Item.Enabled = GetCmdStack().CanUndo();
                    Items.Add(Item);
                    Item = new ToolStripMenuItem("&Redo", Properties.Resources.SymbolUndo, GetCmdStack().RedoEvent);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
                    Item.Name = "redoToolStripMenuItem";
                    Item.ToolTipText = GetCmdStack().GetRedoText();
                    Item.Enabled = GetCmdStack().CanRedo();
                    Items.Add(Item);
                    Items.Add(new ToolStripSeparator());
                    Items.AddRange(GetViewContextMenu(View, null));
                    Item = new ToolStripMenuItem("Page Properties", Properties.Resources.SymboEmpty, ShowPageProperties_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.Name = "ShowPageProperties";
                    Items.Add(Item);
                    return Items.ToArray();
                } else if (Parent.Text == "&File") {
                    Item = new ToolStripMenuItem("&New...", Properties.Resources.SymbolNewFile, NewModel_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
                    Item.Name = "openToolStripMenuItem";
                    Items.Add(Item);
                    Item = new ToolStripMenuItem("&Open...", Properties.Resources.SymbolOpen, Open_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
                    Item.Name = "openToolStripMenuItem";
                    Items.Add(Item);
                    Item = new ToolStripMenuItem("&Save", Properties.Resources.SymbolSave, Save_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
                    Item.Name = "saveToolStripMenuItem";
                    Items.Add(Item);
                    Item = new ToolStripMenuItem("&Save as...", Properties.Resources.SymbolSave, SaveAs_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.Name = "saveAsToolStripMenuItem";
                    Items.Add(Item);
                    Items.Add(new ToolStripSeparator());
                    Item = new ToolStripMenuItem("Export...", Properties.Resources.SymbolSave, ExportAs_Click);
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.Enabled = GetModel() != null;
                    Item.Name = "exportAsToolStripMenuItem";
                    Items.Add(Item);
                } else if (Parent.Text == "&Tools") {
                    Item = new ToolStripMenuItem("PageSettings...");
                    Item.ImageTransparentColor = System.Drawing.Color.Black;
                    Item.MergeAction = MergeAction.Replace;
                    Item.Name = "PageSetupToolStripMenuItem";
                    Items.Add(Item);
                }
            } else {//return top level Menus

            }
            return Items.ToArray();
        }
        /// <summary>
        /// Returns the available context-menu operations
        /// </summary>
        /// <param name="View"></param>
        /// <returns></returns>
        public virtual ToolStripItem[] GetViewContextMenu(IView View, Object ExtData) {
            ToolStripMenuItem Item;
            List<ToolStripItem> Items = new List<ToolStripItem>();
            int Selected = 0;
            if (GetModel() != null) {
            }

            Item = new ToolStripMenuItem("&Kopieren", Properties.Resources.SymbolCopy, Copy_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            Item.Name = "copyToolStripMenuItem";
            Item.Enabled = (Selected > 0);
            Items.Add(Item);

            Item = new ToolStripMenuItem("&Einfügen", Properties.Resources.SymbolPaste, Paste_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            Item.Name = "pasteToolStripMenuItem";
            Item.Enabled = true;
            Item.Tag = ExtData; //stores MouseLocation
            Items.Add(Item);

            Item = new ToolStripMenuItem("&Löschen", Properties.Resources.SymbolDelete, Delete_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            Item.Name = "deleteToolStripMenuItem";
            Item.Enabled = (Selected > 0);
            Items.Add(Item);

            Items.Add(new ToolStripSeparator());
            Item = new ToolStripMenuItem("Shape Properties...", Properties.Resources.SymbolOpen, ShowProperties_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.Name = "ShowProperties";
            Item.Enabled = (Selected > 0);
            Items.Add(Item);
            return Items.ToArray();
        }

        #region UIEvents
        private void Copy_Click(object sender, EventArgs e) {
        }
        private void Paste_Click(object sender, EventArgs e) {
        }
        private void Delete_Click(object sender, EventArgs e) {
        }
        private void Save_Click(object sender, EventArgs e) {
            if (GetModel().GetFileName() == "") {
                SaveAs_Click(sender, e);
            } else {
                SaveToFile();
            }
        }
        private void SaveAs_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Textdateien (*.txt)|*.txt|Alle Dateien (*.*)|*.*";
            if (saveFileDialog.ShowDialog(/*this*/) == DialogResult.OK) {
                SaveToFile(saveFileDialog.FileName);
            };
        }
        private void ExportAs_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Textdateien (*.bmp)|*.bmp|Alle Dateien (*.*)|*.*";
            if (saveFileDialog.ShowDialog(/*this*/) == DialogResult.OK) {
             //   ExportToFile(saveFileDialog.FileName);
            };
        }
        private void Open_Click(object sender, EventArgs e) {
            try {

                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                FileDialog.Filter = "Textdateien (*.txt)|*.txt|Alle Dateien (*.*)|*.*";
                if (FileDialog.ShowDialog(/*this*/) == DialogResult.OK) {
                    LoadFromFile(FileDialog.FileName);
                }
            } finally { }
        }
        private void NewModel_Click(object sender, EventArgs e) {
           //todo SetActiveModel(new smartEdit.Core.ModelDocument());
        }
        private void ShowProperties_Click(object sender, EventArgs e) {
            int i = 0;
            if (i > 0) {
                Widgets.FormShapeSettings Editor = new Widgets.FormShapeSettings();
                Editor.SetObject(sender);
                Editor.Show();
            }
        }
        private void ShowPageProperties_Click(object sender, EventArgs e) {
            Widgets.FormShapeSettings Editor = new Widgets.FormShapeSettings();
            Editor.SetObject(sender);
            Editor.Show();
        }
        #endregion
        public virtual Core.ModelDocument GetModel() { return m_Model; }
        public virtual CmdStackGroup GetCmdStack() { return m_CmdStackGroup; }
        public virtual void OnMouseInput(object sender, MouseInputEventArgs e) {
            if (e.MouseArg.Clicks == 2) {//doubleclick

            } else if (e.MouseArg.Clicks == 0) {//MouseHoover 

            }
        }
        
        #region fields
        protected Core.ModelDocument m_Model = null;
        protected Core.CmdStackGroup m_CmdStackGroup;
        protected Core.ViewManager m_Views = null;
        #endregion
    }

}
