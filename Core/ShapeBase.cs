using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.ComponentModel;

namespace smartEdit.Core {
    public class ShapeBase : ShapeInterface {
        #region constructors
        public ShapeBase() {
            //m_SizeHandles = new List<Rectangle>();
            m_SizeHandles = new List<ShapeActionHandleBase>();
            m_SizeHandles.Clear();
            m_SizeHandles.Add(new ShapeActionSizer(this, ShapeActionSizer.Sizehandle.TopLeft));
            m_SizeHandles.Add(new ShapeActionSizer(this, ShapeActionSizer.Sizehandle.TopRight));
            m_SizeHandles.Add(new ShapeActionSizer(this, ShapeActionSizer.Sizehandle.BottomRight));
            m_SizeHandles.Add(new ShapeActionSizer(this, ShapeActionSizer.Sizehandle.BottomLeft));
            m_SizeHandles.Add(new ShapeActionReceptabel(this, ShapeActionReceptabel.RouteDirection.Top));
            m_SizeHandles.Add(new ShapeActionReceptabel(this, ShapeActionReceptabel.RouteDirection.Right));
            m_SizeHandles.Add(new ShapeActionReceptabel(this, ShapeActionReceptabel.RouteDirection.Bottom));
            m_SizeHandles.Add(new ShapeActionReceptabel(this, ShapeActionReceptabel.RouteDirection.Left));

            SetBoundingBox(new Rectangle(0, 0, 50, 70));
            m_SubShapes = new List<ShapeBase>();
            m_Editor = null;
            m_SelectPen = new System.Drawing.Pen(System.Drawing.Color.Gray);
            m_SelectPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        }
        public override object Clone() {
            ShapeBase NewShape = new ShapeBase();
            return NewShape;
        }
        public override void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteData("Type", GetShapeTypeName());
            Stream.WriteData("ObjName", GetName());
            Stream.WriteData("Rect", GetBoundingBox());
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            SetName(Stream.ReadAsString("ObjName"));
            SetBoundingBox(Stream.ReadAsRect("Rect"));
        }
        #endregion
        #region events & delegates
        public override event UpdateEventHandler EventUpdate;
        protected virtual void FireUpdateEvent(object sender, EventArgs e) {
            UpdateEventHandler handler = EventUpdate;
            if (handler != null) {
                handler(sender, e);
            }
        }
        private void MouseDown(object sender, MouseEventArgs e) {
            ;
        }
        private void MouseMove(object sender, MouseEventArgs e) {
            ;
        }
        private void MouseUp(object sender, MouseEventArgs e) {
            ;
        }
        #endregion
        public override ShapeClass GetShapeClass() {
            throw (new NotImplementedException());
        }
        protected virtual Rectangle LimitBoundingBox(Rectangle RectIn, bool Shrink) {
            Rectangle RectOut = RectIn;
            RectOut.Width = Math.Max(RectIn.Width, 20);
            RectOut.Height = Math.Max(RectIn.Height, 20);
            if (GetModel() != null) {
                Rectangle PageArea = this.GetModel().GetPageArea();
                RectOut.Intersect(PageArea);
                RectOut = GetModel().GetPage().ConstrainPosition(RectOut, true);
            }

            return RectOut;
        }
        public override void SetBoundingBox(Rectangle Rect) {
            m_BoundingBox = LimitBoundingBox(Rect, true);
            RebuildSizers();
            FireUpdateEvent(this, null);
        }
        public override void MoveBoundingBox(Point Position, bool relative) {
            Rectangle RectOut = m_BoundingBox;
            if (relative) {
                RectOut.X = RectOut.X + Position.X;
                RectOut.Y = RectOut.Y + Position.Y;
            } else {
                RectOut.Location = Position;
            }
            m_BoundingBox = LimitBoundingBox(RectOut, false);
            RebuildSizers();
            FireUpdateEvent(this, null);
        }
        public override bool Intersects(Point Position) {
            return GetBoundingBox().Contains(Position);
        }
        public override ShapeActionHandleBase IntersectsHandle(Point Position) {
            ShapeActionHandleBase Sizer = null, TmpHandle = null, ConnHandle = null;
            ElementEnumerator<ShapeActionHandleBase> Iterator = new ElementEnumerator<ShapeActionHandleBase>(m_SizeHandles);
            bool Found = false;
            while (Iterator.MoveNext() && !Found) {
                TmpHandle = Iterator.Current;
                if (TmpHandle.Intersects(Position)) {
                    Sizer = TmpHandle;
                    /*
                    {//if this is a connection point, try to get the connected plug instead
                        ConnHandle =  TmpHandle.GetTopConnector();
                        if (ConnHandle != null)
                        {

                            Found = true;
                        }
                    };*/
                }
            }
            if (Found) Sizer = ConnHandle;
            return Sizer;
        }
        public override void ShowEditor() {
            if (m_Editor == null) { } else {
                m_Editor.Dispose();
                m_Editor = null;
            }
            m_Editor = new Widgets.FormShapeSettings();
            m_Editor.Text = GetName();
            //m_Editor.AddPropertyPage(GetPropertyTabs());
            m_Editor.SetObject(this);
            m_Editor.Show();
        }
        public virtual TabPage[] GetPropertyTabs() {
            TabPage[] Pages = new TabPage[1];
            TabPage Page = new TabPage("Page 1");
            System.Windows.Forms.NumericUpDown numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            System.Windows.Forms.NumericUpDown numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            numericUpDown1.Location = new System.Drawing.Point(9, 25);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(81, 20);
            numericUpDown1.TabIndex = 0;
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = 10000; //??
            numericUpDown1.Value = ((decimal)GetBoundingBox().Top);

            numericUpDown2.Location = new System.Drawing.Point(96, 25);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new System.Drawing.Size(81, 20);
            numericUpDown2.TabIndex = 0;
            numericUpDown2.Minimum = 0;
            numericUpDown2.Maximum = 10000; //??
            numericUpDown2.Value = ((decimal)GetBoundingBox().Left);

            Page.Controls.Add(numericUpDown2);
            Page.Controls.Add(numericUpDown1);
            Pages[0] = Page;
            return Pages;
        }
        public override void ShowHandles(bool Flag) {
            bool _Modified = (m_ShowHandles != Flag);
            m_ShowHandles = Flag;
            if (_Modified) FireUpdateEvent(this, null);
        }
        public bool IsHandlesVisible() {
            return IsSelected() || m_ShowHandles;
        }
        protected void RebuildSizers() {
            if (m_SizeHandles == null) return;
            foreach (ShapeActionHandleBase Handle in m_SizeHandles) {
                Handle.RecalculatePositionFromParent();
            }
        }
        public IEnumerator<ShapeBase> GetShapeEnumerator() {
            return m_SubShapes.GetEnumerator();
        }
        public override void Select(bool Select) {
            bool _Modified = (m_Selected != Select);
            m_Selected = Select;
            ElementEnumerator<ShapeActionHandleBase> Iterator = GetHandles();
            while (Iterator.MoveNext()) {
                Iterator.Current.Select(false);
            }
            if (!Select) m_ShowHandles = false;
            if (_Modified) FireUpdateEvent(this, null);
        }
        public override ElementEnumerator<ShapeActionHandleBase> GetSelectedHandles() {
            return new ElementEnumeratorWithFilter<ShapeActionHandleBase>(m_SizeHandles, new HandleEnumeratorFilterBySelection());
        }
        public override ElementEnumerator<ShapeActionHandleBase> GetHandles() {
            return new ElementEnumerator<ShapeActionHandleBase>(m_SizeHandles);
        }
        public override void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            DrawShape(Graphic, Context);
            if (IsSelected()) DrawSelect(Graphic, Context);
            if (IsHandlesVisible()) DrawSizers(Graphic, Context);
        }
        public override void DrawShape(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) { }
        public override void DrawSelect(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            Graphic.DrawRectangle(m_SelectPen, Context.ToScreen(GetBoundingBox()));
        }
        public virtual void DrawSizers(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            foreach (ShapeActionHandleBase Handle in m_SizeHandles) {
                Graphic.DrawRectangle(m_SelectPen, Context.ToScreen(Handle.GetBoundingBox()));
            }
        }

        public override string GetShapeTypeName() { return "ShapeBase"; }
        #region fields
        private Widgets.FormShapeSettings m_Editor;
        private List<ShapeBase> m_SubShapes = null;
        private List<ShapeActionHandleBase> m_SizeHandles = null;
        protected bool m_ShowHandles = false;
        #endregion
    }
    public class ShapeRect : ShapeBase {
        public ShapeRect()
            : base() {
            m_Pen = new System.Drawing.Pen(System.Drawing.Color.Black);
        }
        public override object Clone() {
            ShapeRect NewShape = new ShapeRect();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {

            Graphic.DrawRectangle(m_Pen, Context.ToScreen(GetBoundingBox()));

        }
        public override ShapeBase.ShapeClass GetShapeClass() {
            return ShapeClass.Shape;
        }

        public override string GetShapeTypeName() { return "Rectangle"; }
        private System.Drawing.Pen m_Pen;
    }

    public class ShapeEllipse : ShapeBase {
        public ShapeEllipse()
            : base() {
            m_Pen = new System.Drawing.Pen(System.Drawing.Color.Black);
        }
        public override object Clone() {
            ShapeEllipse NewShape = new ShapeEllipse();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {
            Graphic.DrawEllipse(m_Pen, Context.ToScreen(GetBoundingBox()));
        }
        public override ShapeBase.ShapeClass GetShapeClass() {
            return ShapeClass.Shape;
        }
        public override void WriteToSerializer(SerializerBase Stream) {
            base.WriteToSerializer(Stream);
        }
        public override string GetShapeTypeName() { return "Ellipse"; }
        private System.Drawing.Pen m_Pen;
    }
    public class ShapeImage : ShapeBase {
        public ShapeImage()
            : base() {
            m_Pen = new System.Drawing.Pen(System.Drawing.Color.Black);
            SetImage();
        }
        public override object Clone() {
            ShapeImage NewShape = new ShapeImage();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {
            if (m_ScaledImage != null) {
                Graphic.DrawImageUnscaledAndClipped(m_ScaledImage, Context.ToScreen(GetBoundingBox()));
            } else {
                Graphic.DrawRectangle(m_Pen, Context.ToScreen(GetBoundingBox()));
                Graphic.DrawLine(m_Pen,
                    Context.ToScreen(GetBoundingBox().Location),
                    Context.ToScreen(new Point(GetBoundingBox().Right, GetBoundingBox().Bottom)));
            }
        }
        public void SetImage() {
            if (m_ScaledImage != null) {
                m_ScaledImage.Dispose();
                m_ScaledImage = null;
            }
           //?? m_ScaledImage = Image.FromFile("D:\\temp\\msdn\\setup\\watermark.bmp");
            FireUpdateEvent(this, null);
        }
        public override void SetBoundingBox(Rectangle Rect) {
            if (m_ScaledImage == null) {
                base.SetBoundingBox(Rect);
                return;
            }
            if (m_ScaleToBox)
                base.SetBoundingBox(Rect);
            else {
                base.SetBoundingBox(new Rectangle(Rect.Location, m_ScaledImage.Size));
            }
        }
        public override ShapeBase.ShapeClass GetShapeClass() { return ShapeClass.Shape; }
        public override string GetShapeTypeName() { return "Image"; }
        #region properties
        [CategoryAttribute("Data")]
        //[Editor(typeof(System.Drawing.Design.ImageEditor),typeof(System.Drawing.Design.UITypeEditor))]
        public string File {
            get { return m_File; }
            set { m_File = (value); }
        }
        #endregion
        #region fields
        private System.Drawing.Pen m_Pen;
        private Image m_ScaledImage = null;
        private string m_File;
        private bool m_ScaleToBox = false;
        #endregion
    }
}
