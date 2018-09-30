using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace smartEdit.Core {
    public class ShapeActionHandleBase : ShapeInterface {
        public ShapeActionHandleBase(ShapeInterface Parent)
            : base() {
            m_Parent = Parent;
            m_SelectPen = new System.Drawing.Pen(System.Drawing.Color.Gray);
            m_Connectors = new List<ShapeActionHandleBase>();
        }
        public override event UpdateEventHandler EventUpdate;
        protected virtual void FireUpdate(object sender, EventArgs e) {
            UpdateEventHandler handler = EventUpdate;
            if (handler != null) handler(sender, e);
        }
        protected virtual void FireUpdateEvent(object sender, EventArgs e) { ;     }
        public override ShapeInterface.ShapeClass GetShapeClass() {
            return ShapeClass.ActionHandle;
        }
        public override string GetShapeTypeName() {
            return "ShapeActionHandle";
        }
        public override void SetBoundingBox(Rectangle Rect) {
            m_BoundingBox = (Rect);
            FireUpdate(this, null);
        }
        public override bool Intersects(Point Position) {
            return GetBoundingBox().Contains(Position);
        }
        public virtual Cursor GetActionCursor() {
            return Cursors.Cross;
        }
        public virtual void RecalculatePositionFromParent() { }
        public virtual void DrawShape(System.Drawing.Graphics Graphic) {
            Graphic.DrawRectangle(m_SelectPen, GetBoundingBox());
        }
        public virtual void RegisterConnector(ShapeActionHandleBase Handle) {
            EventUpdate += Handle.FireUpdateEvent;
            m_Connectors.Add(Handle);
        }
        public virtual void UnregisterConnector(ShapeActionHandleBase Handle) {
            EventUpdate -= Handle.FireUpdateEvent;
            m_Connectors.Remove(Handle);
        }
        public virtual ShapeActionHandleBase GetTopConnector() {
            if (m_Connectors.Count > 0) {
                return m_Connectors[m_Connectors.Count - 1];
            } else {
                return null;
            }
        }
        public virtual bool IsPlug() { return false; }
        public virtual bool IsReceptable() { return false; }

        public Size GetBoundingBoxSize() { return new Size(10, 10); }
        public Size GetBoundingBoxHalfSize() { return new Size(5, 5); }
        public ShapeInterface GetParent() { return m_Parent; }
        protected List<ShapeActionHandleBase> m_Connectors = null;
        protected ShapeInterface m_Parent;
    }
    public class ShapeActionSizer : ShapeActionHandleBase {
        public enum Sizehandle {
            None = 0,
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }
        public ShapeActionSizer(ShapeInterface Parent, Sizehandle Direction)
            : base(Parent) {
            m_Direction = Direction;
        }
        public override void RecalculatePositionFromParent() {
            Rectangle Item = new Rectangle(0, 0, 10, 10);
            switch (m_Direction) {
                case Sizehandle.TopLeft:
                    Item.X = m_Parent.GetBoundingBox().X;
                    Item.Y = m_Parent.GetBoundingBox().Y;
                    break;
                case Sizehandle.TopRight:
                    Item.X = m_Parent.GetBoundingBox().Right - Item.Width;
                    Item.Y = m_Parent.GetBoundingBox().Y;
                    break;
                case Sizehandle.BottomRight:
                    Item.X = m_Parent.GetBoundingBox().Right - Item.Width;
                    Item.Y = m_Parent.GetBoundingBox().Bottom - Item.Height;
                    break;
                case Sizehandle.BottomLeft:
                    Item.X = m_Parent.GetBoundingBox().X;
                    Item.Y = m_Parent.GetBoundingBox().Bottom - Item.Height;
                    break;
                default:
                    break;
            }
            SetBoundingBox(Item);
        }
        public override void MoveBoundingBox(Point Position, bool relative) {
            Point _delta = Position;
            if (relative) {
                m_BoundingBox.X = m_BoundingBox.X + Position.X;
                m_BoundingBox.Y = m_BoundingBox.Y + Position.Y;
            } else {
                _delta.X = _delta.X - GetBoundingBox().X;
                _delta.Y = _delta.Y - GetBoundingBox().Y;
                m_BoundingBox.Location = Position;
            }
            Rectangle _NewBoundingBox = m_Parent.GetBoundingBox();
            switch (m_Direction) {
                case Sizehandle.TopLeft:
                    _NewBoundingBox.X = GetBoundingBox().X;
                    _NewBoundingBox.Y = GetBoundingBox().Y;
                    _NewBoundingBox.Width = _NewBoundingBox.Width - _delta.X;
                    _NewBoundingBox.Height = _NewBoundingBox.Height - _delta.Y;
                    break;
                case Sizehandle.TopRight:
                    _NewBoundingBox.Y = GetBoundingBox().Y;
                    _NewBoundingBox.Width = _NewBoundingBox.Width + _delta.X;
                    _NewBoundingBox.Height = _NewBoundingBox.Height - _delta.Y;
                    break;
                case Sizehandle.BottomRight:
                    _NewBoundingBox.Width = _NewBoundingBox.Width + _delta.X;
                    _NewBoundingBox.Height = _NewBoundingBox.Height + _delta.Y;
                    break;
                case Sizehandle.BottomLeft:
                    _NewBoundingBox.X = GetBoundingBox().X;
                    _NewBoundingBox.Width = _NewBoundingBox.Width - _delta.X;
                    _NewBoundingBox.Height = _NewBoundingBox.Height + _delta.Y;
                    break;
                default:
                    break;
            }
            m_Parent.SetBoundingBox(_NewBoundingBox);
        }
        protected Sizehandle m_Direction;
    }
    public class ShapeActionReceptabel : ShapeActionHandleBase {
        public enum RouteDirection {
            None = 0,
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft,
            Top,
            Right,
            Bottom,
            Left
        }
        public ShapeActionReceptabel(ShapeInterface Parent, RouteDirection Direction)
            : base(Parent) {
            m_RouteDirection = Direction;
        }
        public override void RecalculatePositionFromParent() {
            Rectangle Item = new Rectangle(0, 0, 10, 10);
            switch (m_RouteDirection) {
                case RouteDirection.Left:
                    Item.X = m_Parent.GetBoundingBox().X;
                    Item.Y = m_Parent.GetBoundingBox().Y - Item.Height / 2 + m_Parent.GetBoundingBox().Height / 2;
                    break;
                case RouteDirection.Right:
                    Item.X = m_Parent.GetBoundingBox().Right - Item.Width;
                    Item.Y = m_Parent.GetBoundingBox().Y - Item.Height / 2 + m_Parent.GetBoundingBox().Height / 2;
                    break;
                case RouteDirection.Bottom:
                    Item.X = m_Parent.GetBoundingBox().X - Item.Width / 2 + m_Parent.GetBoundingBox().Width / 2;
                    Item.Y = m_Parent.GetBoundingBox().Bottom - Item.Height;
                    break;
                case RouteDirection.Top:
                    Item.X = m_Parent.GetBoundingBox().X - Item.Width / 2 + m_Parent.GetBoundingBox().Width / 2;
                    Item.Y = m_Parent.GetBoundingBox().Y;
                    break;
                default:
                    break;
            }
            SetBoundingBox(Item);
        }
        public override bool IsReceptable() { return true; }
        public override void MoveBoundingBox(Point Position, bool relative) { }
        public override bool AcceptDrop(ShapeInterface Object) {
            bool Accept = false;
            if (Object != null) {
                if (Object.GetShapeClass() == ShapeClass.ActionHandle) {
                    Accept = ((ShapeActionHandleBase)Object).IsPlug();
                };
            }
            return Accept;
        }
        protected RouteDirection m_RouteDirection;

    }
    public class ShapeActionPlug : ShapeActionHandleBase {
        public ShapeActionPlug(ShapeInterface Parent)
            : base(Parent) {
            SetBoundingBox(new Rectangle(Point.Empty, GetBoundingBoxSize()));
        }
        protected override void FireUpdateEvent(object sender, EventArgs e) {
            RecalculatePosition();
        }
        public override bool IsPlug() {
            return true;
        }
        public override bool AcceptDrag() {
            return true;
        }
        protected void RecalculatePosition() {//Position neu ermitteln wenn verknüpftes Ziel verschoben
            if (m_Target != null) {
                MoveBoundingBox(m_Target.GetBoundingBox().Location, false);
            }
        }
        public override string GetShapeTypeName() {
            return "ShapeActionPlug";
        }
        public override void RecalculatePositionFromParent() { }
        public override void MoveBoundingBox(Point Position, bool relative) {
            Point _delta = Position;
            bool _NoChange = true;
            bool _Connected = false;
            if (relative) {
                _NoChange = (Position == Point.Empty);
                m_BoundingBox.X = m_BoundingBox.X + Position.X;
                m_BoundingBox.Y = m_BoundingBox.Y + Position.Y;
            } else {
                _NoChange = (Position == m_BoundingBox.Location);
                _delta.X = _delta.X - GetBoundingBox().X;
                _delta.Y = _delta.Y - GetBoundingBox().Y;
                m_BoundingBox.Location = Position;
            }
            ModelDiagram Model = GetModel();
            ShapeActionHandleBase _OldTarget = m_Target;
            if (Model != null) {   ///wenn das Handle verschoben wird, wird autom. eine Verbindung zu dem Handle in der Position
                ///hergestellt. Dabei wird das Handle gewählt das 
                ///- Ein Drop aktzeptiert
                ///- ein Receptable oder Plug ist
                ///

                ShapeInterface Shape = Model.GetShapeAtPoint(GetBoundingBoxCenter(), m_Parent);
                if (Shape != null) {

                    ShapeActionHandleBase TargetHandle = Shape.IntersectsHandle(GetBoundingBoxCenter());
                    if (TargetHandle != null) {
                        if (TargetHandle.AcceptDrop(this)) {
                            if (TargetHandle != _OldTarget) {
                                TargetHandle.UnregisterConnector(this);
                                m_Target = TargetHandle;
                                TargetHandle.RegisterConnector(this);
                            }
                            _Connected = true;
                            if (m_BoundingBox.Location != TargetHandle.GetBoundingBox().Location) RecalculatePosition();
                        }

                    }

                }
            }
            if (!_Connected) {
                if (_OldTarget != null) _OldTarget.UnregisterConnector(this); ;
                m_Target = null;
            }
            this.FireUpdate(this, null);
        }
        public override void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteData("Type", GetShapeTypeName());
            Stream.WriteData("ObjName", GetName());
            Stream.WriteData("Pos", GetBoundingBox().Location);
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            SetName(Stream.ReadAsString("ObjName"));
            MoveBoundingBox(Stream.ReadAsPoint("Pos"), false);
        }
        protected ShapeActionHandleBase m_Target = null;
    }
}