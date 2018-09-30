using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace smartEdit.Core {

    interface IWidgetCanvasState {
        void MouseMove(MouseEventArgs e);
        void MouseDown(MouseEventArgs e);
        void MouseUp(MouseEventArgs e);
        void ToolChanged(int Tool);
        Cursor GetCursor();
        void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context);
    }

    public class WidgetDiagramCanvasStateBase : IWidgetCanvasState {
        public WidgetDiagramCanvasStateBase(Widgets.WidgetDiagramPage Parent) {
            m_Form = Parent;
        }
        public virtual void MouseMove(MouseEventArgs e) { }
        public virtual void MouseDown(MouseEventArgs e) { }
        public virtual void MouseUp(MouseEventArgs e) { }
        public virtual void ToolChanged(int Tool) { }
        public virtual void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) { }
        public virtual Cursor GetCursor() { return null; }
        protected Widgets.WidgetDiagramPage GetParent() { return m_Form; }
        protected Widgets.WidgetDiagramPage m_Form = null;
    }
    /// <summary>
    /// Anfangszustand
    /// </summary>
    public class NothingSelected : WidgetDiagramCanvasStateBase {
        public NothingSelected(Widgets.WidgetDiagramPage Parent)
            : base(Parent) { }
        public override void MouseDown(MouseEventArgs e) {

            //Selektiere Element wenn auf Element geklickt
            Point StartPos = GetParent().GetDrawingContext().FromScreen(e.Location);
            Core.ShapeInterface Shape = GetParent().GetDiagram().GetShapeAtPoint(StartPos, false);
            if (Keys.Control != (Control.ModifierKeys & Keys.Control)) GetParent().GetDiagram().UnselectAll();
            if (Shape != null) {
                Shape.Select(true);
                //??GetParent().SetMouseMode(smartEdit.Core.MouseOperation.Move);
                Core.ElementSelected State = GetParent().GetStateElementSelected();
                State.SetStartPoint(StartPos);
                GetParent().SetState(State);
            } else {//Starte Auswahlrechteck wenn nicht auf Element geklickt
                Core.StartSelection State = GetParent().GetStateStartSelection();
                State.SetStartPoint(GetParent().GetDrawingContext().FromScreen(e.Location));
                GetParent().SetState(State);
            }
        }
        public override void MouseUp(MouseEventArgs e) {
            //Schließe Auswahlrechteck 
            //--> ElementSelected wenn Elemente ausgewählt
        }
        public override void MouseMove(MouseEventArgs e) { }
        public override Cursor GetCursor() {
            return Cursors.Default;
        }

    }
    public class StartSelection : WidgetDiagramCanvasStateBase {
        public StartSelection(Widgets.WidgetDiagramPage Parent)
            : base(Parent) { }
        public override void MouseDown(MouseEventArgs e) { }
        public override void MouseUp(MouseEventArgs e) {//Schließe Auswahlrechteck 
            //--> ElementSelected wenn Elemente ausgewählt

            Core.ElementEnumerator<ShapeInterface> Iterator = GetParent().GetDiagram().GetShapesInRect(
                m_SelectRect, false);
            int i = 0;
            while (Iterator.MoveNext()) {
                i++;
                Iterator.Current.Select(true);
            }
            if (i > 0) {
                Core.ElementSelected State = GetParent().GetStateElementSelected();
                GetParent().SetState(State);
            } else {
                GetParent().SetState(GetParent().GetStateNothingSelected());
            }
        }
        public override void MouseMove(MouseEventArgs e) {
            //zeichne Auswahlrechteck
            Point MousePos = GetParent().GetDrawingContext().FromScreen(e.Location);
            m_SelectRect.X = Math.Min(m_MousDownStart.X, MousePos.X);
            m_SelectRect.Y = Math.Min(m_MousDownStart.Y, MousePos.Y);
            m_SelectRect.Width = Math.Abs(MousePos.X - m_MousDownStart.X);
            m_SelectRect.Height = Math.Abs(MousePos.Y - m_MousDownStart.Y);
            GetParent().Invalidate(true);
        }
        public override void Draw(Graphics Graphic, ShapeDrawingContext Context) {
            Pen _PenSelect = new Pen(Color.Gray);
            _PenSelect.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            Graphic.DrawRectangle(_PenSelect, GetParent().GetDrawingContext().ToScreen(m_SelectRect));

            _PenSelect.Dispose();
        }
        public override Cursor GetCursor() {
            return Cursors.Default;
        }
        public void SetStartPoint(Point Start) {
            m_MousDownStart = Start;
            m_SelectRect = Rectangle.Empty;
            m_SelectRect.Location = Start;
        }

        Point m_MousDownStart;
        Rectangle m_SelectRect;
    }
    /// <summary>
    /// Ein/mehrere Elemente ausgewählt
    /// </summary>
    public class ElementSelected : WidgetDiagramCanvasStateBase {
        public ElementSelected(Widgets.WidgetDiagramPage Parent)
            : base(Parent) { }
        public override void MouseDown(MouseEventArgs e) {
            //Selektiere Element wenn auf Element geklickt
            SetStartPoint(GetParent().GetDrawingContext().FromScreen(e.Location));
            Core.ShapeInterface Shape = GetParent().GetDiagram().GetShapeAtPoint(m_MousDownStart, false);
            if (Keys.Control != (Control.ModifierKeys & Keys.Control)) GetParent().GetDiagram().UnselectAll();
            if (Shape != null) {
                Shape.Select(true);
                if (e.Clicks > 1) {
                    Shape.ShowEditor();
                } else {
                    ShapeActionHandleBase Handle = Shape.IntersectsHandle(m_MousDownStart);
                    if (Handle != null) {
                        ShapeActionHandleBase Connector = Handle.GetTopConnector();
                        if (Connector != null) {
                            Handle = Connector;
                            GetParent().GetDiagram().UnselectAll();
                            Handle.GetParent().Select(true);
                        } else { }
                        Handle.Select(true);
                        Core.HandleSelected State = GetParent().GetStateHandleSelected();
                        State.SetStartPoint(m_MousDownStart);
                        GetParent().SetState(State);
                    }
                }
            } else {//Starte Auswahlrechteck wenn nicht auf Element geklickt
                Core.StartSelection State = GetParent().GetStateStartSelection();
                State.SetStartPoint(GetParent().GetDrawingContext().FromScreen(e.Location));
                GetParent().SetState(State);
            }
            int X0 = GetParent().GetDiagram().GetPageArea().Width;
            int Y0 = GetParent().GetDiagram().GetPageArea().Height;
            int X1 = 0, Y1 = 0;
            Rectangle _Box;
            Core.ElementEnumerator<ShapeInterface> Iterator = GetParent().GetDiagram().GetSelectedShapes();
            while (Iterator.MoveNext()) {
                _Box = Iterator.Current.GetBoundingBox();
                X0 = Math.Min(X0, _Box.X);
                Y0 = Math.Min(Y0, _Box.Y);
                X1 = Math.Max(X1, _Box.Right);
                Y1 = Math.Max(Y1, _Box.Bottom);
            }
            m_SelectRect.X = X0;
            m_SelectRect.Y = Y0;
            m_SelectRect.Width = X1 - X0;
            m_SelectRect.Height = Y1 - Y0;
            m_SelectRectOffset.X = GetStartPoint().X - m_SelectRect.X;
            m_SelectRectOffset.Y = GetStartPoint().Y - m_SelectRect.Y;
            //m_SelectRect.X -= GetStartPoint().X;
            //m_SelectRect.Y -= GetStartPoint().Y;

        }
        public override void MouseUp(MouseEventArgs e) {
            //Element verschieben 
            Core.ElementEnumerator<ShapeInterface> Iterator = GetParent().GetDiagram().GetSelectedShapes();
            Point _OldPos, _NewPos, _Delta;
            int _CmdCount = 0;
            Core.CmdMacro _Cmd = new smartEdit.Core.CmdMacro();
            while (Iterator.MoveNext()) {
                _OldPos = Iterator.Current.GetBoundingBox().Location;
                _NewPos = _OldPos;
                _Delta = GetParent().GetDrawingContext().FromScreen(e.Location);
                _Delta.X = _Delta.X - m_MousDownStart.X;
                _Delta.Y = _Delta.Y - m_MousDownStart.Y;
                _NewPos.Offset(_Delta);
                _Cmd.AddCmd(new Core.CmdMoveShape(Iterator.Current, _OldPos, _NewPos));
                _CmdCount++;
            }
            if (_CmdCount > 0) {
                m_SelectRect = Rectangle.Empty;
                GetParent().GetDiagram().GetUndoStack().Push(_Cmd);
            }
        }
        public override void MouseMove(MouseEventArgs e) {
            if (e.Button != MouseButtons.None) {
                //zeichne Verschieberechteck
                Point MousePt = (GetParent().GetDrawingContext().FromScreen(e.Location));
                m_SelectRect.X = MousePt.X - m_SelectRectOffset.X;
                m_SelectRect.Y = MousePt.Y - m_SelectRectOffset.Y;
                GetParent().Invalidate(true);
            }

        }
        public override Cursor GetCursor() {
            return Cursors.SizeAll;
        }
        public override void Draw(Graphics Graphic, ShapeDrawingContext Context) {
            Pen _PenSelect = new Pen(Color.Red);
            _PenSelect.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            Graphic.DrawRectangle(_PenSelect, GetParent().GetDrawingContext().ToScreen(m_SelectRect));
            _PenSelect.Dispose();
        }
        protected Point GetStartPoint() { return m_MousDownStart; }
        public void SetStartPoint(Point Start) {
            m_MousDownStart = Start;
            m_SelectRectOffset = Start;
            m_SelectRect = Rectangle.Empty;
            m_SelectRect.Location = Start;
        }
        Point m_MousDownStart, m_SelectRectOffset;
        Rectangle m_SelectRect;
    }
    public class HandleSelected : WidgetDiagramCanvasStateBase {
        public HandleSelected(Widgets.WidgetDiagramPage Parent)
            : base(Parent) { }
        public override void MouseMove(MouseEventArgs e) {//display Connection-handles of otherShapes
            Point Pos = GetParent().GetDrawingContext().FromScreen(e.Location);
            ShapeInterface Shape = GetParent().GetDiagram().GetShapeAtPoint(Pos, false);
            if (Shape != null) {
                Shape.ShowHandles(true);
            };

        }
        public override void MouseDown(MouseEventArgs e) {
            //Selektiere Element wenn auf Element geklickt
            SetStartPoint(GetParent().GetDrawingContext().FromScreen(e.Location));
            Core.ShapeInterface Shape = GetParent().GetDiagram().GetShapeAtPoint(m_MousDownStart, false);
            GetParent().GetDiagram().UnselectAll();
            if (Shape != null) {
                ShapeActionHandleBase Handle = Shape.IntersectsHandle(m_MousDownStart);
                if (Handle != null) {
                    ShapeActionHandleBase Connector = Handle.GetTopConnector();
                    if (Connector != null) {
                        Handle = Connector;
                        Handle.GetParent().Select(true);
                    } else {
                        Shape.Select(true);
                    }

                    Handle.Select(true);
                } else {
                    Shape.Select(true);
                    Core.ElementSelected State = GetParent().GetStateElementSelected();
                    State.SetStartPoint(m_MousDownStart);
                    GetParent().SetState(State);
                }
                //??GetParent().SetMouseMode(smartEdit.Core.MouseOperation.Move);
                //Core.ElementSelected State = GetParent().GetStateElementSelected();
                //GetParent().SetState(State);
            } else {//Starte Auswahlrechteck wenn nicht auf Element geklickt
                Core.StartSelection State = GetParent().GetStateStartSelection();
                State.SetStartPoint(m_MousDownStart);
                GetParent().SetState(State);
            }
        }
        public override void MouseUp(MouseEventArgs e) {
            //Element verschieben 
            Point _OldPos, _NewPos, _Delta;

            Core.HandleEnumerator Iterator = new HandleEnumerator(GetParent().GetDiagram().GetSelectedShapes());
            Core.ShapeActionHandleBase Handle = null;
            while (Iterator.MoveNext()) {
                if (Iterator.Current.IsSelected()) {
                    Handle = Iterator.Current;
                    break;
                }
            }
            if (Handle != null) {
                _OldPos = Handle.GetBoundingBox().Location;
                _NewPos = _OldPos;
                _Delta = GetParent().GetDrawingContext().FromScreen(e.Location);
                _Delta.X = _Delta.X - m_MousDownStart.X;
                _Delta.Y = _Delta.Y - m_MousDownStart.Y;
                _NewPos.Offset(_Delta);
                GetParent().GetDiagram().GetUndoStack().Push(new Core.CmdSizeShape(Handle, _OldPos, _NewPos));
            }

        }
        public override void ToolChanged(int Tool) { }
        public override void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) { }
        public override Cursor GetCursor() { return Cursors.Cross; }
        public void SetStartPoint(Point Start) {
            m_MousDownStart = Start;

        }
        Point m_MousDownStart;
    }
    /// <summary>
    /// Tool AddElement ausgewählt
    /// </summary>
    public class ToolAddElement : WidgetDiagramCanvasStateBase {
        public ToolAddElement(Widgets.WidgetDiagramPage Parent)
            : base(Parent) { }
        public override void MouseDown(MouseEventArgs e) {
            //Start merken
            SetStartPoint(GetParent().GetDrawingContext().FromScreen(e.Location));
        }
        public override void MouseUp(MouseEventArgs e) {
            //Element erstellen mit Start/-Endkoordinate 
            GetParent().GetDiagram().GetUndoStack().Push(
                        new Core.CmdAddShape(GetParent().GetDiagram(),
                            Core.ShapeFactory.CreateShape(""),
                            m_MousDownStart,
                            GetParent().GetDrawingContext().FromScreen(e.Location)));

        }
        public override void MouseMove(MouseEventArgs e) {
            //zeichne Rechteck
        }
        public override Cursor GetCursor() {
            return Cursors.Cross;
        }
        public void SetStartPoint(Point Start) { m_MousDownStart = Start; }
        Point m_MousDownStart;
    }

}
