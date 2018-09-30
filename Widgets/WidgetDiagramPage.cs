using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace smartEdit.Widgets {
    public partial class WidgetDiagramPage : UserControl, Core.IGraphicView {

        public WidgetDiagramPage() {
            InitializeComponent();
            this.m_Canvas.panel1.BackColor = Color.White;
            this.m_Canvas.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_Paint);
            this.m_Canvas.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseMove);
            this.m_Canvas.panel1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDoubleClick);
            this.m_Canvas.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDown);
            this.m_Canvas.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseUp);
            m_NothingSelected = new smartEdit.Core.NothingSelected(this);
            SetState(m_NothingSelected);
            m_ElementSelected = new smartEdit.Core.ElementSelected(this);
            m_HandleSelected = new Core.HandleSelected(this);
            m_StartSelection = new Core.StartSelection(this);
            m_ToolAddShape = new smartEdit.Core.ToolAddElement(this);

            m_DrawingContext = new smartEdit.Core.ShapeDrawingContext();
            //SetMouseMode(smartEdit.Core.MouseOperation.None);
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
                Size PageSizeScreen = new Size();
                PageSizeScreen.Width = (int)(m_DrawingContext.GetScale() * (float)GetDiagram().GetPageArea().Width);
                PageSizeScreen.Height = (int)(m_DrawingContext.GetScale() * (float)GetDiagram().GetPageArea().Height);
                if (this.m_Canvas.panel1.ClientSize != PageSizeScreen) {
                    this.m_Canvas.panel1.ClientSize = PageSizeScreen;
                }
                this.m_Canvas.Invalidate(true);
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
            this.m_Canvas.Cursor = State.GetCursor();
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

        public Core.ModelDiagram GetDiagram() {
            if (m_Controller == null) return null;
            return m_Controller.GetModel();
        }
        private void cbPageZoom_SelectedIndexChanged(object sender, EventArgs e) {
            m_DrawingContext.SetScale((float)Convert.ToDouble(this.cbPageZoom.Text) / 100);
            OnUpdateEvent(this, e);
        }
        private void btDeleteShape_Click(object sender, EventArgs e) {
            Core.ElementEnumerator<Core.ShapeInterface> Iterator = GetDiagram().GetSelectedShapes();
            Core.CmdMacro Cmd = new Core.CmdMacro();
            int i = 0;
            while (Iterator.MoveNext()) {
                Cmd.AddCmd(new Core.CmdDeleteShape(GetDiagram(), Iterator.Current));
                i++;
            }
            if (i != 0) GetDiagram().GetUndoStack().Push(Cmd);
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
            m_Canvas.ContextMenuStrip.Show(Location);
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

    }
}
