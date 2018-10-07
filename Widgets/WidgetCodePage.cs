using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ScintillaNET;
using smartEdit.Core;

/// see here for documentation of scintilla
/// https://www.scintilla.org/ScintillaDoc.html
/// and here for scintillaNET
/// https://github.com/jacobslusser/ScintillaNET/wiki


namespace smartEdit.Widgets {
    public partial class WidgetCodePage : UserControl, Core.IGraphicView {
        ScintillaNET.Scintilla TextArea;
        public WidgetCodePage() {
            InitializeComponent();
            // CREATE CONTROL
            TextArea = new ScintillaNET.Scintilla();
            this.panel1.Controls.Add(TextArea);

            // BASIC CONFIG
            TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
            TextArea.TextChanged += (this.OnTextChanged);

            // INITIAL VIEW CONFIG
            TextArea.WrapMode = WrapMode.None;
            TextArea.IndentationGuides = IndentView.LookBoth;

            InitColors();
            InitSyntaxColoring();
            InitNumberMargin();
            InitBookmarkMargin();
            InitCodeFolding();

            TextArea.UsePopup(false); //disable buildin-context menu; we want to handle this ourself
        }

        public virtual CmdStack GetCmdStack() { 
            IView _parent = (IView) this.ParentForm;
            if (_parent != null) return _parent.GetCmdStack();
            return null;
        }
        public virtual ToolStrip GetToolbar() {
            return null;
        }
        public virtual ViewData GetViewData() {
            return null;
        }
        private void OnTextChanged(object sender, EventArgs e) {

        }
        #region Numbers, Bookmarks, Code Folding

        /// <summary>
        /// the background color of the text area
        /// </summary>
        private const int BACK_COLOR = 0x2A211C;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0xB7B7B7;

        /// <summary>
        /// change this to whatever margin you want the line numbers to show in
        /// </summary>
        private const int NUMBER_MARGIN = 1;

        /// <summary>
        /// change this to whatever margin you want the bookmarks/breakpoints to show in
        /// </summary>
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;

        /// <summary>
        /// change this to whatever margin you want the code folding tree (+/-) to show in
        /// </summary>
        private const int FOLDING_MARGIN = 3;

        /// <summary>
        /// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
        /// </summary>
        private const bool CODEFOLDING_CIRCULAR = true;

        private void InitColors() {

            TextArea.SetSelectionBackColor(true, IntToColor(0x114D9C));

        }
        private void InitSyntaxColoring() {

            // Configure the default style
            TextArea.StyleResetDefault();
            TextArea.Styles[Style.Default].Font = "Consolas";
            TextArea.Styles[Style.Default].Size = 10;
            TextArea.Styles[Style.Default].BackColor = IntToColor(0x212121);
            TextArea.Styles[Style.Default].ForeColor = IntToColor(0x0);
            TextArea.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            TextArea.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            TextArea.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            TextArea.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            TextArea.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            TextArea.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            TextArea.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            TextArea.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            TextArea.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            TextArea.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            TextArea.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            TextArea.Lexer = Lexer.Cpp;

            TextArea.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            TextArea.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
            string test = TextArea.DescribeKeywordSets();
        }

        private void InitNumberMargin() {

            TextArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            TextArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = TextArea.Margins[NUMBER_MARGIN];
            nums.Width = 20;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            TextArea.MarginClick += TextArea_MarginClick;
        }

        private void InitBookmarkMargin() {

            var margin = TextArea.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);

            var marker = TextArea.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);

        }

        private void InitCodeFolding() {

            TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            TextArea.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

            // Enable code folding
            TextArea.SetProperty("fold", "1");
            TextArea.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            TextArea.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            TextArea.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            TextArea.Margins[FOLDING_MARGIN].Sensitive = true;
            TextArea.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++) {
                TextArea.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                TextArea.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            TextArea.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            TextArea.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            TextArea.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            TextArea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            TextArea.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            TextArea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            TextArea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            TextArea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

        }
        #endregion
        private void TextArea_MarginClick(object sender, MarginClickEventArgs e) {
            if (e.Margin == BOOKMARK_MARGIN) {
                // Do we have a marker for this line?
                const uint mask = (1 << BOOKMARK_MARKER);
                var line = TextArea.Lines[TextArea.LineFromPosition(e.Position)];
                if ((line.MarkerGet() & mask) > 0) {
                    // Remove existing bookmark
                    line.MarkerDelete(BOOKMARK_MARKER);
                } else {
                    // Add bookmark
                    line.MarkerAdd(BOOKMARK_MARKER);
                }
            }
        }
        public void LoadFile(String path) {
            if (File.Exists(path)) {
               // FileName.Text = Path.GetFileName(path);
                TextArea.Text = File.ReadAllText(path);
            }
        }
        public void SaveFile(String path) {
            File.WriteAllText(path,TextArea.Text,Encoding.ASCII);
        }

        public Core.ControllerDocument GetController() { return m_Controller; }
        public void SetController(Core.ControllerDocument Ctrl) {
            if (m_Controller != null) {
                m_Controller.UnregisterView(this);
            }
            m_Controller = Ctrl;
            m_Controller.RegisterView(this);
        }
        #region event & delegates
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
       /* protected event smartEdit.Core.ShapeSelectedEventHandler EventSelect;
        public void RegisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect += new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        public void UnregisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventSelect -= new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnShapeSelected);
        }
        protected virtual void FireShapeSelected(object sender, smartEdit.Core.ShapeInterface Shape) {//?? notused
            smartEdit.Core.ShapeSelectedEventHandler handler = EventSelect;
            if (handler != null) {
                handler(sender, Shape);
            }
        }*/
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        protected event smartEdit.Core.MouseInputEventHandler EventMouseInput;
        public void RegisterMouseInput(smartEdit.Core.ControllerDocument Listener) {
            EventMouseInput += new smartEdit.Core.MouseInputEventHandler(Listener.OnMouseInput);
        }
        public void UnregisterMouseInput(smartEdit.Core.ControllerDocument Listener) {
            EventMouseInput -= new smartEdit.Core.MouseInputEventHandler(Listener.OnMouseInput);
        }
        protected virtual void FireMouseInput(object sender, smartEdit.Core.MouseInputEventArgs e) {
            smartEdit.Core.MouseInputEventHandler handler = EventMouseInput;
            if (handler != null) handler(sender, e);


        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public void OnUpdateEvent(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new smartEdit.Core.UpdateEventHandler(OnUpdateEvent));
            } else {
             /*   Size PageSizeScreen = new Size();
                PageSizeScreen.Width = (int)(m_DrawingContext.GetScale() * (float)GetDiagram().GetPageArea().Width);
                PageSizeScreen.Height = (int)(m_DrawingContext.GetScale() * (float)GetDiagram().GetPageArea().Height);
                if (this.m_Canvas.panel1.ClientSize != PageSizeScreen) {
                    this.m_Canvas.panel1.ClientSize = PageSizeScreen;
                }
                this.m_Canvas.Invalidate(true);*/
                //Invalidate(true);
                //Update();
            }
        }
        public void OnToolChanged(object sender, smartEdit.Core.MouseOperation Op) {

        }
        public void OnMouseFeedback(object sender, smartEdit.Core.MouseOperation Op) {

        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void Canvas_MouseDoubleClick(object sender, MouseEventArgs e) {//?? not used
            /* FireMouseInput(sender, new smartEdit.Core.MouseInputEventArgs(
                 new MouseEventArgs(e.Button,e.Clicks,m_DrawingContext.FromScreen(e.X),m_DrawingContext.FromScreen(e.Y),e.Delta),
                 new KeyEventArgs(Control.ModifierKeys),
                 m_MouseOp));
             /*if (m_MouseOp != smartEdit.Core.MouseOperation.Add)
             {
                 Core.ShapeBase Shape = GetDiagram().GetShapeAtPoint(
                     m_DrawingContext.FromScreen( e.Location),false);
                 if (Shape != null)
                 {
                     Shape.ShowEditor();
                 };
             }*/
        }

        
        #endregion      
        
        #region fields

        private Core.ControllerDocument m_Controller = null;
        private Point m_ContextMenuLocation = new Point(0, 0);
        #endregion


        public void ShowContextMenu(Point Location) {
            //m_Canvas.ContextMenuStrip.Show(Location);
        }
        private void contextMenu_Opening(object sender, CancelEventArgs e) {
            contextMenu.Items.Clear();
            contextMenu.Items.AddRange(GetController().GetViewContextMenu(this, m_ContextMenuLocation));
            e.Cancel = false;
        }

        /*private void ToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem Menu = sender as ToolStripMenuItem;
            Menu.DropDownItems.Clear();
            Menu.DropDownItems.AddRange(GetController().GetViewMenuStrip(this, Menu));
        }*/

        #region Utils   //

        public static Color IntToColor(int rgb) {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        public void InvokeIfNeeded(Action action) {
            if (this.InvokeRequired) {
                this.BeginInvoke(action);
            } else {
                action.Invoke();
            }
        }

        #endregion
    }
}
