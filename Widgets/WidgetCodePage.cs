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
            

            // BASIC CONFIG
            TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
            TextArea.TextChanged += this.OnTextChanged;

            // INITIAL VIEW CONFIG
            TextArea.WrapMode = WrapMode.None;
            TextArea.IndentationGuides = IndentView.LookBoth;
            TextArea.UpdateUI += this.TextArea_UpdateUI;

            InitColors();
            InitSyntaxColoring();
            InitNumberMargin();
            InitBookmarkMargin();
            InitCodeFolding();
            InitAutoComplete();

            TextArea.UsePopup(false); //disable buildin-context menu; we want to handle this ourself
        }
        /// <summary>
        /// manages Brace highlighting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextArea_UpdateUI(object sender, UpdateUIEventArgs e) {
            // Has the caret changed position?
            var caretPos = TextArea.CurrentPosition;
            if (lastCaretPos != caretPos) {
                UpdateStatusEventArgs evt = new UpdateStatusEventArgs();
                evt.Text = "Pos: " + caretPos.ToString();
                FireUpdateStatus(this, evt);
                lastCaretPos = caretPos;
                var bracePos1 = -1;
                var bracePos2 = -1;
                //TODO Brace higligthing not working or overridden by Lexer??
                // Is there a brace to the left or right?
                if (caretPos > 0 && IsBrace(TextArea.GetCharAt(caretPos - 1)))
                    bracePos1 = (caretPos - 1);
                else if (IsBrace(TextArea.GetCharAt(caretPos)))
                    bracePos1 = caretPos;

                if (bracePos1 >= 0) {
                    // Find the matching brace
                    bracePos2 = TextArea.BraceMatch(bracePos1);
                    if (bracePos2 == Scintilla.InvalidPosition) {
                        TextArea.BraceBadLight(bracePos1);
                        TextArea.HighlightGuide = 0;
                    } else {
                        TextArea.BraceHighlight(bracePos1, bracePos2);
                        TextArea.HighlightGuide = TextArea.GetColumn(bracePos1);
                    }
                } else {
                    // Turn off brace matching
                    TextArea.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                    TextArea.HighlightGuide = 0;
                }
            }
        }

        void TextArea_AutoCCharDeleted(object sender, EventArgs e) {
            
        }
        private void OnTextChanged(object sender, EventArgs e) {

        }
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
        void TextArea_CharAdded(object sender, CharAddedEventArgs e) {
            String _word =GetCurrentWord();            
            if (_word.Length>=2) {
                Project _proj = ProjectManager.GetProjectByItsFile(GetViewData().File);
                if (_proj == null)
                    return;
                String _line = GetLine(GetCurrentLine());
                List<ObjDecl> _lst = _proj.Model.lookupAll(
                    _word, _line, _proj.Model.GetRelativePath(GetViewData().File));
                ShowAutoCompletion(_word.Length, _lst);
                //TODO cannot show AC & CT at same time -> build own solution
                //TextArea.CallTipShow(5, "\x01 1 of 3 \x02 \ntestA\ntest"); 
                //TextArea.CharPositionFromPointClose(..)

                // MyClass.Init( bool Start, int End) -> bool
                // user enters this | AC                |  CT
                // MyC              | MyClass           | Info to preselected object  
                // MyClass.         | list of functions | info to preselected function
                // MyClass.Ini      | Init              | like above
                // MyClass.Init(Xpo | Xposition         | Info to operand
            }
            
        }
        #region SCINTILA-Stuff
        ////////////////////////////////////////////////////////////////
        int lastCaretPos = 0;
        private static bool IsBrace(int c) {
            switch (c) {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                    return true;
            }

            return false;
        }
        void ShowAutoCompletion(int count, List<ObjDecl> lst) {
            if (lst.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (ObjDecl item in lst) {
                if (sb.Length == 0)
                    sb.Append(item.Function());
                else
                    sb.Append(" " + item.Function());
            }

            TextArea.AutoCShow(count, sb.ToString()); 
            //?? can only display AC or CT  Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_CALLTIPSHOW, 1, sb.ToString());

        }
        void ShowAutoCompletion(int count, List<string> lst) {
            if (lst.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (string item in lst) {
                if (sb.Length == 0)
                    sb.Append(item);
                else
                    sb.Append(" " + item);
            }

            TextArea.AutoCShow(count, sb.ToString()); 
        }
        static char[] _Spliter = { ' ', '.', '\n', '\t', '(', ')' };
        /// <summary>
        /// Get the current cursor at the forward word
        /// </summary>
        /// <returns></returns>
        public string GetCurrentWord() {
            int currentPos = TextArea.CurrentPosition;
            int size = 64;
            int beg = currentPos - (size - 1);
            beg = beg > 0 ? beg : 0;
            int end = currentPos;
            size = end - beg;
            String txtRange = TextArea.GetTextRange(beg, size);
            string[] arr = txtRange.Split(_Spliter);
            return arr[arr.Length - 1];
        }
        /// <summary>
        /// Gets the current row, starting with 0
        /// </summary>
        /// <returns></returns>
        public int GetCurrentLine() {
            return TextArea.CurrentLine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLineFromPosition(int pos) {
            return TextArea.LineFromPosition(pos);
        }
        /// <summary>
        /// Get the specified row contents
        /// </summary>
        /// <param name="lineNo">行号，从0开始</param>
        /// <returns></returns>
        public string GetLine(int lineNo) {
           return TextArea.Lines[lineNo].Text;
        }
        /// ///////////////////////////////////////////////////////////////
        #endregion
        
        
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

        private void InitAutoComplete() {
            TextArea.CharAdded += (TextArea_CharAdded);
            TextArea.AutoCCharDeleted += (TextArea_AutoCCharDeleted);
            TextArea.AutoCAutoHide = false;
            TextArea.AutoCCancelAtStart = false;
            TextArea.AutoCChooseSingle = false;
            TextArea.AutoCMaxHeight = 5;
            TextArea.AutoCMaxWidth = 100;

        }
        private void InitColors() {

            TextArea.SetSelectionBackColor(true, IntToColor(0x114D9C)); 

        }
        private void InitSyntaxColoring() {

            // Configure the default style
            TextArea.StyleResetDefault();
            TextArea.Styles[Style.Default].Font = "Consolas";
            TextArea.Styles[Style.Default].Size = 10;
            TextArea.Styles[Style.Default].BackColor = Color.GhostWhite;// IntToColor(0x212121);
            TextArea.Styles[Style.Default].ForeColor = Color.Black ;// IntToColor(0x0);
            TextArea.Styles[Style.BraceLight].BackColor = TextArea.Styles[Style.Default].BackColor;
            TextArea.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            TextArea.Styles[Style.BraceBad].ForeColor = Color.Red;
            TextArea.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            TextArea.Styles[Style.Cpp.Identifier].ForeColor = Color.Purple;//IntToColor(0xD0DAE2);
            TextArea.Styles[Style.Cpp.Comment].ForeColor = Color.Green;//IntToColor(0xBD758B);
            TextArea.Styles[Style.Cpp.CommentLine].ForeColor = TextArea.Styles[Style.Cpp.Comment].ForeColor;//IntToColor(0x40BF57);
            TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = TextArea.Styles[Style.Cpp.Comment].ForeColor;//IntToColor(0x2FAE35);
            TextArea.Styles[Style.Cpp.Number].ForeColor = Color.DarkOrange;//IntToColor(0xFFFF00);
            TextArea.Styles[Style.Cpp.String].ForeColor = Color.Fuchsia;//IntToColor(0xFFFF00);
            TextArea.Styles[Style.Cpp.Character].ForeColor = Color.DeepPink;//IntToColor(0xE95454);
            TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Blue;//IntToColor(0x8AAFEE);
            TextArea.Styles[Style.Cpp.Operator].ForeColor = Color.Black;//IntToColor(0xE0E0E0);
            TextArea.Styles[Style.Cpp.Regex].ForeColor = Color.Black;//IntToColor(0xff00ff);
            TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = TextArea.Styles[Style.Cpp.CommentLine].ForeColor;//IntToColor(0x77A7DB);
            TextArea.Styles[Style.Cpp.Word].ForeColor = TextArea.Styles[Style.Cpp.Preprocessor].ForeColor;// IntToColor(0x48A8EE);
            TextArea.Styles[Style.Cpp.Word2].ForeColor = TextArea.Styles[Style.Cpp.Preprocessor].ForeColor;//IntToColor(0xF98906);
            TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Color.Black;//IntToColor(0xB3D991);
            TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Color.Black;//IntToColor(0xFF0000);
            TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = Color.OrangeRed;//IntToColor(0x48A8EE);

            TextArea.Lexer = Lexer.Cpp;

            TextArea.SetKeywords(0, "case do while else if for switch throw function var try catch while default break continue return include using");
            TextArea.SetKeywords(1, "bool int string variant double");
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
        public virtual CmdStack GetCmdStack() { 
            IView _parent = (IView) this.ParentForm;
            if (_parent != null) return _parent.GetCmdStack();
            return null;
        }
        public virtual ToolStrip GetToolbar() {
            return null;
        }
        IView m_ParentForm = null;
        public virtual ViewData GetViewData() {
            if (m_ParentForm == null)
                m_ParentForm = (IView)this.ParentForm;
            
            if (m_ParentForm != null)
                return m_ParentForm.GetViewData();   
            return null;
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
        
        public event smartEdit.Core.UpdateStatusEventHandler EventUpdateStatus;
        protected virtual void FireUpdateStatus(object sender, UpdateStatusEventArgs e) {
            smartEdit.Core.UpdateStatusEventHandler handler = EventUpdateStatus;
            if (handler != null) handler(sender, e);
        }

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
