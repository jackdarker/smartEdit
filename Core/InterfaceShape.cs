using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.ComponentModel;


namespace smartEdit.Core {
    public class ShapeDrawingContext {
        public ShapeDrawingContext() {
            m_Scale = 1f;
        }
        public float GetScale() { return m_Scale; }
        public void SetScale(float Scale) { m_Scale = Scale; }
        public int FromScreen(int Value) {
            return (Value * 100 / ((int)(GetScale() * 100f)));
        }
        public Point FromScreen(Point Value) {
            Point _Point = Value;
            _Point.X = FromScreen(_Point.X);
            _Point.Y = FromScreen(_Point.Y);
            return _Point;
        }
        public Rectangle FromScreen(Rectangle Value) {
            Rectangle _Rect = Value;
            _Rect.X = FromScreen(_Rect.X);
            _Rect.Y = FromScreen(_Rect.Y);
            _Rect.Width = FromScreen(_Rect.Width);
            _Rect.Height = FromScreen(_Rect.Height);
            return _Rect;
        }
        public Size FromScreen(Size Value) {
            Size _Rect = Value;
            _Rect.Width = FromScreen(_Rect.Width);
            _Rect.Height = FromScreen(_Rect.Height);
            return _Rect;
        }
        public int ToScreen(int Value) {
            //return (Value * ((int)(GetScale() * 100f))) / 100;
            return Value;  //scaling is done in Paint via Graphics.ScaleTransform
        }
        public Point ToScreen(Point Value) {
            Point ScrPoint = Value;
            ScrPoint.X = ToScreen(ScrPoint.X);
            ScrPoint.Y = ToScreen(ScrPoint.Y);
            return ScrPoint;
        }
        public Rectangle ToScreen(Rectangle Value) {
            Rectangle ScrRect = Value;
            ScrRect.X = ToScreen(ScrRect.X);
            ScrRect.Y = ToScreen(ScrRect.Y);
            ScrRect.Width = ToScreen(ScrRect.Width);
            ScrRect.Height = ToScreen(ScrRect.Height);
            return ScrRect;
        }
        public Size ToScreen(Size Value) {
            Size ScrRect = Value;
            ScrRect.Width = ToScreen(ScrRect.Width);
            ScrRect.Height = ToScreen(ScrRect.Height);
            return ScrRect;
        }
        private float m_Scale;
    }

    /// <summary>
    /// base class for all graphic objects
    /// </summary>
    public class ShapeInterface : ISerializable, ICloneable {
        public enum ShapeClass {
            ActionHandle,
            Shape,
            Connector
        }
        #region constructors
        public ShapeInterface() {
            SetBoundingBox(new Rectangle(0, 0, 50, 70));
            m_Selected = false;
            m_SelectPen = new System.Drawing.Pen(System.Drawing.Color.Gray);
            m_SelectPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        }
        public virtual object Clone() {
            ShapeInterface NewShape = new ShapeInterface();
            return NewShape;
        }
        #endregion
        #region events & delegates
        public virtual event UpdateEventHandler EventUpdate;
        #endregion
        public virtual bool AcceptDrop(ShapeInterface Object) { return false; }
        public virtual bool ExecuteDrop(ShapeInterface Object) { return false; }
        public virtual bool AcceptDrag() { return false; }
        public virtual bool ExecuteDrag(Point Destination) { return false; }
        public virtual void WriteToSerializer(SerializerBase Stream) { }
        public virtual void ReadFromSerializer(SerializerBase Stream) { }
        public virtual ShapeClass GetShapeClass() {
            throw (new NotImplementedException());
        }
        public virtual void SetModel(ModelDiagram Model) {
            m_Model = Model;
        }
        public ModelDiagram GetModel() { return m_Model; }
        public virtual void SetBoundingBox(Point Position1, Point Position2) {
            int x1, x2, y1, y2;
            if (Position2.X < Position1.X) {
                x1 = Position2.X;
                x2 = Position1.X;
            } else if (Position2.X == Position1.X) {
                x1 = x2 = Position1.X;
            } else {
                x1 = Position1.X;
                x2 = Position2.X;
            }
            if (Position2.Y < Position1.Y) {
                y1 = Position2.Y;
                y2 = Position1.Y;
            } else if (Position2.Y == Position1.Y) {
                y1 = y2 = Position1.Y;
            } else {
                y1 = Position1.Y;
                y2 = Position2.Y;
            }
            Rectangle _Rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            SetBoundingBox(_Rect);
        }
        public virtual void SetBoundingBox(Rectangle Rect) {
            m_BoundingBox = (Rect);
        }
        public Rectangle GetBoundingBox() {
            return m_BoundingBox;
        }
        public Point GetBoundingBoxCenter() {
            Rectangle Box = GetBoundingBox();
            Point Center = Point.Empty;
            Center.X = Box.Left + Box.Width / 2;
            Center.Y = Box.Top + Box.Height / 2;
            return Center;
        }
        public virtual void ShowHandles(bool Flag) { }
        public virtual void MoveBoundingBox(Point Position, bool relative) {
            if (relative) {
                m_BoundingBox.X = m_BoundingBox.X + Position.X;
                m_BoundingBox.Y = m_BoundingBox.Y + Position.Y;
            } else {
                m_BoundingBox.Location = Position;
            }
        }
        public virtual bool Intersects(Point Position) { return false; }
        public virtual ShapeActionHandleBase IntersectsHandle(Point Position) { return null; }
        public virtual void Select(bool Select) {
            m_Selected = Select;
        }
        public virtual void ShowEditor() { }
        public bool IsSelected() { return m_Selected; }
        public virtual void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            DrawShape(Graphic, Context);
            if (m_Selected) DrawSelect(Graphic, Context);
        }
        public virtual void DrawShape(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) { }
        public virtual void DrawSelect(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            Graphic.DrawRectangle(m_SelectPen, Context.ToScreen(GetBoundingBox()));
        }
        public virtual ElementEnumerator<ShapeActionHandleBase> GetSelectedHandles() { throw new NotImplementedException(); }
        public virtual ElementEnumerator<ShapeActionHandleBase> GetHandles() { throw new NotImplementedException(); }
        public void SetName(string Name) { m_Name = Name; }
        public string GetName() { return m_Name; }
        public virtual string GetShapeTypeName() { return "ShapeInterface"; }

        #region properties
        [CategoryAttribute("Layout")]
        public Rectangle BoundingBox {
            get { return GetBoundingBox(); }
            set { SetBoundingBox(value); }
        }
        [CategoryAttribute("Data")]
        public string Name {
            get { return GetName(); }
            set { SetName(value); }
        }


        #endregion

        #region fields
        protected Rectangle m_BoundingBox;
        protected bool m_Selected;
        protected string m_Name;
        private ModelDiagram m_Model;
        protected System.Drawing.Pen m_SelectPen;
        #endregion
    }
}
