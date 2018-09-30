using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ScintillaNET;

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

            /*this.m_Canvas.panel1.BackColor = Color.White;
            this.m_Canvas.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_Paint);
            this.m_Canvas.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseMove);
            this.m_Canvas.panel1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDoubleClick);
            this.m_Canvas.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDown);
            this.m_Canvas.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseUp);
            
           /*
                m_NothingSelected = new smartEdit.Core.NothingSelected(this);
                SetState(m_NothingSelected);
                m_ElementSelected = new smartEdit.Core.ElementSelected(this);
                m_HandleSelected = new Core.HandleSelected(this);
                m_StartSelection = new Core.StartSelection(this);
                m_ToolAddShape = new smartEdit.Core.ToolAddElement(this); 
            */

            m_DrawingContext = new smartEdit.Core.ShapeDrawingContext();
            //SetMouseMode(smartEdit.Core.MouseOperation.None);
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
        protected event smartEdit.Core.ShapeSelectedEventHandler EventSelect;
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
        }
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
            if (Op == smartEdit.Core.MouseOperation.None) {
                SetState(GetStateNothingSelected());
            } else {
                SetState(GetStateToolAddShape());
            }
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

        private void Canvas_Paint(object sender, PaintEventArgs e) {
            SolidBrush _BrushRed = new SolidBrush(Color.Red);
            SolidBrush _BrushGreen = new SolidBrush(Color.Green);
            SolidBrush _BrushYellow = new SolidBrush(Color.Yellow);
            Pen _PenBlack = new Pen(Color.Black);
            Pen _PenSelect = new Pen(Color.Gray);
            _PenSelect.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            //RectangleF _Area = e.Graphics.ClipBounds;
            e.Graphics.ScaleTransform(m_DrawingContext.GetScale(), m_DrawingContext.GetScale());
            //e.Graphics.FillRectangle(_BrushGreen, _Area);
            if (GetDiagram() != null) {
                GetDiagram().GetPage().Draw(e.Graphics, m_DrawingContext);
                Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetDiagram().GetShapeEnumerator();
                while (Iterator.MoveNext()) {
                    Iterator.Current.Draw(e.Graphics, m_DrawingContext);
                }
                m_CurrentOperation.Draw(e.Graphics, GetDrawingContext());
            }
            e.Graphics.ResetTransform();
            _PenSelect.Dispose();
            _BrushRed.Dispose();
            _BrushGreen.Dispose();
            _BrushGreen.Dispose();
            _PenBlack.Dispose();
        }
        private void Canvas_MouseDown(object sender, MouseEventArgs e) {
            m_ContextMenuLocation = e.Location;
            m_CurrentOperation.MouseDown(e);

        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            //if (m_MouseCurrent == e.Location) return; //Filter if Mouse was moved
            this.toolStripStatusLabel2.Text = (m_DrawingContext.FromScreen(e.Location).ToString());
            m_CurrentOperation.MouseMove(e);

        }
        private void Canvas_MouseUp(object sender, MouseEventArgs e) {
            m_CurrentOperation.MouseUp(e);

        }
        #endregion
        #region states
        public void SetState(Core.WidgetDiagramCanvasStateBase State) {
            m_CurrentOperation = State;
            //this.m_Canvas.Cursor = State.GetCursor();
            Invalidate(true);
            //??Console.WriteLine(State.ToString());
        }
        public Core.WidgetDiagramCanvasStateBase GetCurrentState() { return m_CurrentOperation; }
        public Core.NothingSelected GetStateNothingSelected() {
            return m_NothingSelected;
        }
        public Core.ElementSelected GetStateElementSelected() {
            return m_ElementSelected;
        }
        public Core.HandleSelected GetStateHandleSelected() {
            return m_HandleSelected;
        }
        public Core.StartSelection GetStateStartSelection() {
            return m_StartSelection;
        }
        public Core.ToolAddElement GetStateToolAddShape() {
            return m_ToolAddShape;
        }
        #endregion
        public Core.ShapeDrawingContext GetDrawingContext() { return m_DrawingContext; }

        public Core.ModelDocument GetDiagram() {
            if (m_Controller == null) return null;
            return m_Controller.GetModel();
        }
       
        
        #region fields
        private Core.WidgetDiagramCanvasStateBase m_CurrentOperation = null;
        private Core.NothingSelected m_NothingSelected = null;
        private Core.ElementSelected m_ElementSelected = null;
        private Core.HandleSelected m_HandleSelected = null;
        private Core.StartSelection m_StartSelection = null;
        private Core.ToolAddElement m_ToolAddShape = null;
        private Core.ControllerDocument m_Controller = null;
        private Core.ShapeDrawingContext m_DrawingContext;
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

        private void WidgetDiagramPage_Click(object sender, EventArgs e) {
            MessageBox.Show("test");
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
