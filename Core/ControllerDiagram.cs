using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace smartEdit.Core {
    public class ControllerDocument : IController {
        public ControllerDocument() {
            m_CmdStackGroup = new CmdStackGroup();
            //m_CmdStackGroup.EventUpdate += new EventHandler<EventArgs>(OnUpdate);
            Core.ShapeFactory.InitShapeFactory();
            SetActiveModel(new smartEdit.Core.ModelDiagram());
        }
        #region Event & Delegates
        public virtual void OnShapeSelected(object sender, ShapeInterface Shape) {
            if (Shape == null) return;
            GetModel().UnselectAll();
            Shape.Select(true);
        }
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
        public event ToolSelectedEventHandler EventToolChanged;
        public virtual void OnToolChanged(object sender, smartEdit.Core.MouseOperation Tool) {
            ToolSelectedEventHandler handler = EventToolChanged;
            if (handler != null) {
                handler(sender, Tool);
            }
        }
        public event MouseFeedbackEventHandler EventMouseFeedback;
        public virtual void OnMouseInput(object sender, smartEdit.Core.MouseOperation e) {
            MouseFeedbackEventHandler handler = EventMouseFeedback;
            if (handler != null) handler(sender, e);

        }
        public virtual void OnSetShapeTemplate(object sender, ShapeInterface Template) {
            Core.ShapeFactory.SetShapeTemplate(Template);
        }
        #endregion
        #region Save/Load
        public virtual void LoadFromFile(string FileName) {
            SetActiveModel(ModelDiagram.CreateFromFile(FileName));
        }
        public virtual void SaveToFile(string FileName) {
            GetModel().SaveToFile(FileName);
        }
        public virtual void SaveToFile() {
            GetModel().SaveToFile();
        }
        public virtual void ExportToFile(string FileName) {
            SerializerBMP _stream = null;
            try {
                _stream = new SerializerBMP("JKFLOW", "1.0.0.0");
                _stream.OpenOutputStream(GetModel(), FileName, GetModel().GetPageArea());
                _stream.CloseOutputStream();
                _stream = null;
            } catch (Exception e) {
                throw (e);
            } finally {
                if (_stream != null) _stream.CloseOutputStream();
            }
        }

        #endregion
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
            View.RegisterShapeSelected(this);
            EventUpdate += View.OnUpdateEvent;
            EventToolChanged += new smartEdit.Core.ToolSelectedEventHandler(View.OnToolChanged);
            GetCmdStack().EventUpdate += View.OnUpdateEvent;
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IDataView View) {
            View.UnregisterShapeSelected(this);
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
            View.RegisterShapeSelected(this);
            View.RegisterToolSelected(this);
            FireUpdateEvent(this, new EventArgs());
        }
        public virtual void UnregisterView(IToolView View) {
            EventUpdate -= View.OnUpdateEvent;
            View.UnregisterShapeSelected(this);
        }
        public virtual void SetActiveModel(ModelDiagram Model) {
            if (m_Model != null) {
                m_Model.EventUpdate -= new UpdateEventHandler(FireUpdateEvent);
                m_CmdStackGroup.RemoveStack(m_Model.GetUndoStack());
                m_Model = null;
            }
            m_Model = Model;
            m_Model.EventUpdate += new UpdateEventHandler(FireUpdateEvent);
            m_CmdStackGroup.AddStack(m_Model.GetUndoStack());
            m_CmdStackGroup.SetActiveStack(m_Model.GetUndoStack());
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

                Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetModel().GetSelectedShapes();
                while (Iterator.MoveNext()) {
                    Selected++;
                    ;//Iterator.Current.Draw(e.Graphics, m_DrawingContext);     
                }
            }

            Item = new ToolStripMenuItem("&Kopieren", Properties.Resources.SymbolCopy, btCopyShape_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            Item.Name = "copyToolStripMenuItem";
            Item.Enabled = (Selected > 0);
            Items.Add(Item);

            Item = new ToolStripMenuItem("&Einfügen", Properties.Resources.SymbolPaste, btPasteShape_Click);
            Item.ImageTransparentColor = System.Drawing.Color.Black;
            Item.MergeAction = MergeAction.Replace;
            Item.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            Item.Name = "pasteToolStripMenuItem";
            Item.Enabled = true;
            Item.Tag = ExtData; //stores MouseLocation
            Items.Add(Item);

            Item = new ToolStripMenuItem("&Löschen", Properties.Resources.SymbolDelete, btDeleteShape_Click);
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
        private void btDeleteShape_Click(object sender, EventArgs e) {
            Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetModel().GetSelectedShapes();
            Core.CmdMacro Cmd = new Core.CmdMacro();
            int i = 0;
            while (Iterator.MoveNext()) {
                Cmd.AddCmd(new Core.CmdDeleteShape(GetModel(), Iterator.Current));
                i++;
                //Iterator = GetDiagram().GetSelectedShapes();
            }
            if (i != 0) GetModel().GetUndoStack().Push(Cmd);
        }
        private void btCopyShape_Click(object sender, EventArgs e) {
            try {
                ToolStripItem contextMenuItem = (ToolStripItem)sender;
                Control contextMenu = (Control)contextMenuItem.GetCurrentParent();
                System.Drawing.Point Location = (contextMenu.Location);
                System.Drawing.Point PointA, PointB;
                Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetModel().GetSelectedShapes();
                Core.CmdMacro Cmd = new Core.CmdMacro();
                List<ShapeInterface> Shapes = new List<ShapeInterface>();
                ShapeInterface Shape = null;
                DataObject _Data = new DataObject();
                int i = 0;
                SerializerXML _Stream = new SerializerXML("JKFLOW", "1.0.0.0");
                MemoryStream MemStream = new MemoryStream();
                _Stream.OpenOutputStream(MemStream);
                string Node = "Shape";
                while (Iterator.MoveNext()) {
                    PointA = new System.Drawing.Point(Iterator.Current.GetBoundingBox().Left + 20, Iterator.Current.GetBoundingBox().Top + 20);
                    PointB = new System.Drawing.Point(Iterator.Current.GetBoundingBox().Right + 20, Iterator.Current.GetBoundingBox().Bottom + 20);
                    Shape = ShapeFactory.CreateShape(Iterator.Current.GetShapeTypeName());
                    Shapes.Add(Shape);
                    _Stream.WriteElementStart(Node);
                    Iterator.Current.WriteToSerializer(_Stream);
                    _Stream.WriteElementEnd(Node);
                    Cmd.AddCmd(new Core.CmdAddShape(GetModel(), Shape, PointA, PointB));
                    i++;
                }
                _Stream.CloseOutputStream();
                //if (i != 0) GetModel().GetUndoStack().Push(Cmd);
                _Data.SetData(DataFormats.Text, Cmd.GetText());
                _Data.SetData(Cmd.GetType(), Cmd);
                _Data.SetData("stream", MemStream.ToArray());
                Clipboard.SetDataObject(_Data);
            } catch (System.Runtime.InteropServices.ExternalException) {
                MessageBox.Show("The Clipboard could not be accessed. Please try again.");
            }
        }
        private void btPasteShape_Click(object sender, EventArgs e) {
            ToolStripItem contextMenuItem = (ToolStripItem)sender;
            System.Drawing.Point NewPos, Ctxtpos;
            if (contextMenuItem.Tag is System.Drawing.Point) {
                Ctxtpos = (System.Drawing.Point)contextMenuItem.Tag;  //Location is stored as point in tag
            } else {
                Ctxtpos = new System.Drawing.Point(0, 0);
            }
            System.Drawing.Size PosOffset = System.Drawing.Size.Empty;
            IDataObject Data = Clipboard.GetDataObject();
            ShapeInterface Shape = null;
            Core.CmdMacro Cmd = new Core.CmdMacro();
            String[] Formats = Data.GetFormats(false);
            // Determines whether the data is in a format you can use.
            if (Data.GetDataPresent("stream")) {
                //string test = Data.GetData(DataFormats.Text).ToString();
                SerializerXML _Stream = new SerializerXML("JKFLOW", "1.0.0.0");
                MemoryStream _MemStream = new MemoryStream((byte[])Data.GetData("stream"));
                _Stream.OpenInputStream(_MemStream);
                string NodeGroup;
                int StartNodeLevel = 0, CurrNodeLevel = 0;
                do {
                    NodeGroup = _Stream.GetNodeName();
                    CurrNodeLevel = _Stream.GetNodeLevel();
                    if (CurrNodeLevel < StartNodeLevel) { break; }
                    if (_Stream.GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                        if (NodeGroup == "Shape") {
                            ShapeInterface SourceShape = ShapeFactory.DeserializeShape(_Stream);
                            if (PosOffset.IsEmpty) {
                                PosOffset.Width = Ctxtpos.X - SourceShape.GetBoundingBox().Location.X;
                                PosOffset.Height = Ctxtpos.Y - SourceShape.GetBoundingBox().Location.Y;
                            }
                            NewPos = SourceShape.GetBoundingBox().Location + PosOffset;
                            Shape = ShapeFactory.CreateShape(SourceShape.GetShapeTypeName());
                            Cmd.AddCmd(new Core.CmdAddShape(GetModel(),
                                Shape, NewPos, NewPos + SourceShape.GetBoundingBox().Size));
                        }
                    }
                } while (_Stream.ReadNext());
                _Stream.CloseInputStream();
                _Stream = null;
                GetModel().GetUndoStack().Push(Cmd);
            }
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
                ExportToFile(saveFileDialog.FileName);
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
            SetActiveModel(new smartEdit.Core.ModelDiagram());
        }
        private void ShowProperties_Click(object sender, EventArgs e) {
            Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetModel().GetSelectedShapes();
            List<ShapeInterface> Shapes = new List<ShapeInterface>(0);
            int i = 0;
            while (Iterator.MoveNext()) {
                Shapes.Add(Iterator.Current);
                i++;
            }
            if (i > 0) {
                Widgets.FormShapeSettings Editor = new Widgets.FormShapeSettings();
                Editor.SetObject(Shapes);
                Editor.Show();
            }
        }
        private void ShowPageProperties_Click(object sender, EventArgs e) {
            Widgets.FormShapeSettings Editor = new Widgets.FormShapeSettings();
            Editor.SetObject(GetModel().GetPage());
            Editor.Show();
        }
        #endregion
        public virtual Core.ModelDiagram GetModel() { return m_Model; }
        public virtual CmdStackGroup GetCmdStack() { return m_CmdStackGroup; }
        public virtual void OnMouseInput(object sender, MouseInputEventArgs e) {
            if (e.MouseArg.Clicks == 2) {//doubleclick
                Core.ShapeInterface Shape = GetModel().GetShapeAtPoint(
                         e.MouseArg.Location, false);
                if (Shape != null) {
                    Shape.ShowEditor();
                };
            } else if (e.MouseArg.Clicks == 0) {//MouseHoover 
                Core.ShapeInterface Shape = GetModel().GetShapeAtPoint(
                        e.MouseArg.Location, false);
                /* if (Shape != null)
                 {
                     Shape.ShowSizers(true);
                 };*/
            }
        }
        public virtual void ViewFocusChanged(IView View) { }
        public virtual void ToolChanged() { }
        #region fields
        protected Core.ModelDiagram m_Model = null;
        protected Core.CmdStackGroup m_CmdStackGroup;
        #endregion

}


