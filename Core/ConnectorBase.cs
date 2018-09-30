using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace smartEdit.Core {
    public class ConnectorBase : ShapeInterface {
        public ConnectorBase()
            : base() {
            m_Pen = System.Drawing.Pens.Black;
            m_BrushConnected = System.Drawing.Brushes.Black;
            m_PlugHandles = new List<ShapeActionHandleBase>();
        }

        public override object Clone() {
            ConnectorBase NewShape = new ConnectorBase();
            return NewShape;
        }
        public override string GetShapeTypeName() { return "ConnectorBase"; }
        public override void SetBoundingBox(Point Position1, Point Position2) {
            base.SetBoundingBox(Point.Empty, Point.Empty);//??
            m_PlugHandles[0].MoveBoundingBox(Position1 + m_PlugHandles[0].GetBoundingBoxHalfSize(), false);
            m_PlugHandles[1].MoveBoundingBox(Position2 - m_PlugHandles[1].GetBoundingBoxHalfSize(), false);
        }
        public override void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            DrawShape(Graphic, Context);
            //if (IsSelected()) DrawSelect(Graphic, Context);
            if (IsSelected()) DrawSizers(Graphic, Context);
        }
        public override bool Intersects(Point Position) {//??
            return (IntersectsHandle(Position) != null);
        }
        protected void RebuildSizers() {
            if (m_PlugHandles == null) return;
            foreach (ShapeActionHandleBase Handle in m_PlugHandles) {
                Handle.RecalculatePositionFromParent();
            }
        }
        public override ShapeActionHandleBase IntersectsHandle(Point Position) {
            int i = 0;
            //Sizehandle Sizer = Sizehandle.None;
            //foreach(Rectangle Rect in m_SizeHandles)
            ShapeActionHandleBase Sizer = null;
            foreach (ShapeActionHandleBase Handle in m_PlugHandles) {
                i++;
                if (Handle.GetBoundingBox().Contains(Position)) Sizer = Handle;
            }
            return Sizer;
        }
        public override void Select(bool Select) {
            bool _Modified = (m_Selected != Select);
            m_Selected = Select;
            ElementEnumerator<ShapeActionHandleBase> Iterator = GetHandles();
            while (Iterator.MoveNext()) {
                Iterator.Current.Select(false);
            }
            if (_Modified) FireUpdateEvent(this, null);
        }
        public override ElementEnumerator<ShapeActionHandleBase> GetHandles() {
            return new ElementEnumerator<ShapeActionHandleBase>(m_PlugHandles);
        }
        public override ElementEnumerator<ShapeActionHandleBase> GetSelectedHandles() {
            return new ElementEnumeratorWithFilter<ShapeActionHandleBase>(m_PlugHandles, new HandleEnumeratorFilterBySelection());
        }
        public bool IsHandleConnected(ShapeActionHandleBase Handle) { return false; }
        public void ConnectHandle(ShapeActionHandleBase Handle, ShapeActionHandleBase Other) { }
        public void DisconnectHandle(ShapeActionHandleBase Handle) { }
        public override ShapeBase.ShapeClass GetShapeClass() { return ShapeClass.Connector; }
        #region events & delegates
        public override event UpdateEventHandler EventUpdate;
        protected virtual void FireUpdateEvent(object sender, EventArgs e) {
            UpdateEventHandler handler = EventUpdate;
            if (handler != null) {
                handler(sender, e);
            }
        }
        public virtual void OnHandleModified(object sender, EventArgs e) {
            int X0, X1, Y0, Y1;
            X0 = Math.Min(m_PlugHandles[0].GetBoundingBox().X, m_PlugHandles[1].GetBoundingBox().X);
            X1 = Math.Max(m_PlugHandles[0].GetBoundingBox().X, m_PlugHandles[1].GetBoundingBox().X);
            Y0 = Math.Min(m_PlugHandles[0].GetBoundingBox().Y, m_PlugHandles[1].GetBoundingBox().Y);
            Y1 = Math.Max(m_PlugHandles[0].GetBoundingBox().Y, m_PlugHandles[1].GetBoundingBox().Y);
            base.SetBoundingBox(new Point(X0, Y0), new Point(X1, Y1));
            //??RebuildSizers();
            FireUpdateEvent(this, e);
        }
        #endregion
        public virtual void DrawSizers(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            foreach (ShapeActionHandleBase Handle in m_PlugHandles) {
                Graphic.DrawRectangle(m_SelectPen, Context.ToScreen(Handle.GetBoundingBox()));
            }
        }

        #region fields
        protected System.Drawing.Pen m_Pen;
        protected System.Drawing.Brush m_BrushConnected;
        protected List<Point> m_Points;
        protected List<ShapeActionHandleBase> m_PlugHandles = null;
        #endregion
    }
    public class ConnectorStraight : ConnectorBase {
        public ConnectorStraight()
            : base() {
            m_PlugHandles.Clear();
            m_PlugHandles.Add(new ShapeActionPlug(this));
            m_PlugHandles.Add(new ShapeActionPlug(this));
            foreach (ShapeActionPlug Handle in m_PlugHandles) {
                Handle.EventUpdate += new UpdateEventHandler(OnHandleModified);
            }
        }
        public override void SetModel(ModelDiagram Model) {
            foreach (ShapeActionPlug Handle in m_PlugHandles) {
                Handle.SetModel(Model);
            }
            base.SetModel(Model);
        }
        public override object Clone() {
            ConnectorStraight NewShape = new ConnectorStraight();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {
            Point[] Points = new Point[m_PlugHandles.Count];
            Rectangle Box = new Rectangle(0, 0, 6, 6);
            for (int i = 0; i < m_PlugHandles.Count; i++) {
                Points[i] = Context.ToScreen(m_PlugHandles[i].GetBoundingBoxCenter());
                Box.Location = Points[i];
                Box.X -= 3;
                Box.Y -= 3;
                if (m_PlugHandles[i].GetTopConnector() == null)
                    Graphic.DrawEllipse(m_Pen, Box);
                else
                    Graphic.FillEllipse(m_BrushConnected, Box);

            }
            Graphic.DrawLines(m_Pen, Points);
        }
        public override string GetShapeTypeName() { return "ConnectorStraight"; }
        public override void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteData("Type", GetShapeTypeName());
            Stream.WriteData("ObjName", GetName());
            ElementEnumerator<ShapeActionHandleBase> Handles = GetHandles();
            string Node = "Plugs";
            while (Handles.MoveNext()) {
                Stream.WriteElementStart(Node);
                Handles.Current.WriteToSerializer(Stream);
                Stream.WriteElementEnd(Node);
            }
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            string NodeGroup;
            int StartNodeLevel = 0, CurrNodeLevel = 0;
            ElementEnumerator<ShapeActionHandleBase> Iter = GetHandles();
            StartNodeLevel = Stream.GetNodeLevel();
            do {
                NodeGroup = Stream.GetNodeName();
                CurrNodeLevel = Stream.GetNodeLevel();
                if (CurrNodeLevel < StartNodeLevel) break;
                if (Stream.GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                    if (NodeGroup == "Plugs") {
                        if (Iter.MoveNext()) {
                            Iter.Current.MoveBoundingBox(Stream.ReadAsPoint("Pos"), false);
                        }
                    } else if (NodeGroup == "Type") {
                        ;
                    } else if (NodeGroup == "ObjName") {
                        SetName(Stream.ReadAsString("ObjName"));
                    }
                }
            } while (Stream.ReadNext());

        }
    }
    public class ConnectorRect : ConnectorBase {
        public ConnectorRect()
            : base() {
            m_PlugHandles.Clear();
            m_PlugHandles.Add(new ShapeActionPlug(this));
            m_PlugHandles.Add(new ShapeActionSizer(this, ShapeActionSizer.Sizehandle.TopLeft));
            m_PlugHandles.Add(new ShapeActionPlug(this));

            foreach (ShapeActionHandleBase Handle in m_PlugHandles) {
                Handle.EventUpdate += new UpdateEventHandler(OnHandleModified);
            }
        }
        public override void SetModel(ModelDiagram Model) {
            foreach (ShapeActionHandleBase Handle in m_PlugHandles) {
                Handle.SetModel(Model);
            }
            base.SetModel(Model);
        }
        public override object Clone() {
            ConnectorRect NewShape = new ConnectorRect();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {
            Point[] Points = new Point[m_PlugHandles.Count];
            for (int i = 0; i < m_PlugHandles.Count; i++) {
                Points[i] = Context.ToScreen(m_PlugHandles[i].GetBoundingBoxCenter());
            }
            Graphic.DrawLines(m_Pen, Points);
        }
        public override string GetShapeTypeName() { return "ConnectorRect"; }
        public override void WriteToSerializer(SerializerBase Stream) {
            throw new NotImplementedException();
            Stream.WriteData("Type", GetShapeTypeName());
            Stream.WriteData("ObjName", GetName());
            ElementEnumerator<ShapeActionHandleBase> Handles = GetHandles();
            string Node = "Plugs";
            while (Handles.MoveNext()) {
                Stream.WriteElementStart(Node);
                Handles.Current.WriteToSerializer(Stream);
                Stream.WriteElementEnd(Node);
            }
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            throw new NotImplementedException();
            string NodeGroup;
            int StartNodeLevel = 0, CurrNodeLevel = 0;
            ElementEnumerator<ShapeActionHandleBase> Iter = GetHandles();
            StartNodeLevel = Stream.GetNodeLevel();
            do {
                NodeGroup = Stream.GetNodeName();
                CurrNodeLevel = Stream.GetNodeLevel();
                if (CurrNodeLevel < StartNodeLevel) break;
                if (Stream.GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                    if (NodeGroup == "Plugs") {
                        if (Iter.MoveNext()) {
                            Iter.Current.MoveBoundingBox(Stream.ReadAsPoint("Pos"), false);
                        }
                    } else if (NodeGroup == "Type") {
                        ;
                    } else if (NodeGroup == "ObjName") {
                        SetName(Stream.ReadAsString("ObjName"));
                    }
                }
            } while (Stream.ReadNext());

        }
        /*public override void OnHandleModified(object sender, EventArgs e)
        {
            int X0, X1, Y0, Y1;
            X0 = Math.Min(m_PlugHandles[0].GetBoundingBox().X, m_PlugHandles[1].GetBoundingBox().X);
            X1 = Math.Max(m_PlugHandles[0].GetBoundingBox().X, m_PlugHandles[1].GetBoundingBox().X);
            Y0 = Math.Min(m_PlugHandles[0].GetBoundingBox().Y, m_PlugHandles[1].GetBoundingBox().Y);
            Y1 = Math.Max(m_PlugHandles[0].GetBoundingBox().Y, m_PlugHandles[1].GetBoundingBox().Y);
            base.SetBoundingBox(new Point(X0, Y0), new Point(X1, Y1));
            m_PlugHandles[2].MoveBoundingBox(new Point(X1 - X0, Y1 - Y0), false);
            //FireUpdateEvent(this, e);
        }*/
    }
}
