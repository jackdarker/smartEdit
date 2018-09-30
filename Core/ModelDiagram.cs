using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.IO;


namespace smartEdit.Core {
    public class ModelDiagram : ISerializable {
        public ModelDiagram() {
            m_Shapes = new List<ShapeInterface>();
            m_UndoStack = new CmdStack();
            m_Page = new Page(this);
            m_Page.SetPageArea(new Rectangle(0, 0, 600, 400));
        }
        #region EventDelegates
        public event UpdateEventHandler EventUpdate;
        #endregion
        protected virtual void FireUpdateEvent(object sender, EventArgs e) {
            m_UpdateCounter = 0;
            UpdateEventHandler handler = EventUpdate;
            if (handler != null) handler(sender, e);
        }
        private int m_UpdateCounter = 0;
        protected void BeginUpdate() {
            m_UpdateCounter++;
        }
        protected void EndUpdate() {//??
            m_UpdateCounter--;
            if (m_UpdateCounter <= 0) {
                m_UpdateCounter = 0;
                FireUpdateEvent(this, null);
            }
        }
        public void DeleteShape(ShapeInterface Shape) {
            Shape.EventUpdate -= FireUpdateEvent;
            m_Shapes.Remove(Shape);
            FireUpdateEvent(this, null);
        }
        public void AddShape(ShapeInterface Shape) {
            Shape.SetModel(this);
            m_Shapes.Add(Shape);
            Shape.EventUpdate += new UpdateEventHandler(FireUpdateEvent);
            FireUpdateEvent(this, null);
        }
        public void AddShape(ShapeInterface Shape, Point Position1, Point Position2) {
            if (Shape != null) {
                /*int x1,x2,y1,y2;
                if (Position2.X < Position1.X) 
                {
                    x1 = Position2.X;
                    x2 = Position1.X;
                }
                else if(Position2.X == Position1.X)
                {
                    x1 = x2 = Position1.X;
                }
                else
                {
                    x1 = Position1.X;
                    x2 = Position2.X;
                }
                if (Position2.Y < Position1.Y) 
                {
                    y1 = Position2.Y;
                    y2 = Position1.Y;
                }
                else if(Position2.Y == Position1.Y)
                {
                    y1 = y2 = Position1.Y;
                }
                else
                {
                    y1 = Position1.Y;
                    y2 = Position2.Y;
                }
                Rectangle _Rect = new Rectangle(x1, y1, x2 - x1, y2 - y1); 
                Rectangle _Rect = new Rectangle(Position1.X, Position1.Y,
                    Position2.X- Position1.X,
                    Position2.Y - Position1.Y) ;*/
                Shape.SetBoundingBox(Position1, Position2);
                AddShape(Shape);
            };
        }
        public InterfaceDiagram GetPage() { return m_Page; }
        public ElementEnumerator<ShapeInterface> GetShapeEnumerator() {
            return new ElementEnumerator<ShapeInterface>(m_Shapes);
        }
        public ShapeInterface GetShapeAtPoint(Point Position, bool SelectedOnly) {
            Core.ShapeInterface Shape = null;
            List<ElementEnumeratorFilterStrategyBase<ShapeInterface>> SubFilter = new List<ElementEnumeratorFilterStrategyBase<ShapeInterface>>();
            SubFilter.Add(new ShapeEnumeratorFilterByLocation(Position));
            if (SelectedOnly) SubFilter.Add(new ShapeEnumeratorFilterBySelection());

            ElementEnumeratorFilterMultipleAnd<ShapeInterface> Filter = new ElementEnumeratorFilterMultipleAnd<ShapeInterface>(SubFilter);
            ElementEnumeratorWithFilter<ShapeInterface> Enumerator = new ElementEnumeratorWithFilter<ShapeInterface>(m_Shapes, Filter);
            while (Enumerator.MoveNext()) {
                Shape = Enumerator.Current;
            }
            return Shape;
        }
        public ShapeInterface GetShapeAtPoint(Point Position, ShapeInterface NotThis) {
            Core.ShapeInterface Shape = null;
            List<ElementEnumeratorFilterStrategyBase<ShapeInterface>> SubFilter = new List<ElementEnumeratorFilterStrategyBase<ShapeInterface>>();
            SubFilter.Add(new ShapeEnumeratorFilterByLocation(Position));
            SubFilter.Add(new ElementEnumeratorFilterExclude<ShapeInterface>(NotThis));

            ElementEnumeratorFilterMultipleAnd<ShapeInterface> Filter = new ElementEnumeratorFilterMultipleAnd<ShapeInterface>(SubFilter);
            ElementEnumeratorWithFilter<ShapeInterface> Enumerator = new ElementEnumeratorWithFilter<ShapeInterface>(m_Shapes, Filter);
            if (Enumerator.MoveNext()) {
                Shape = Enumerator.Current;
            }
            return Shape;
        }
        public ElementEnumerator<ShapeInterface> GetShapesInRect(Rectangle Rect, bool SelectedOnly) {
            List<ElementEnumeratorFilterStrategyBase<ShapeInterface>> SubFilter = new List<ElementEnumeratorFilterStrategyBase<ShapeInterface>>();
            SubFilter.Add(new ShapeEnumeratorFilterByLocation(Rect));
            if (SelectedOnly) SubFilter.Add(new ShapeEnumeratorFilterBySelection());
            ElementEnumeratorFilterMultipleAnd<ShapeInterface> Filter = new ElementEnumeratorFilterMultipleAnd<ShapeInterface>(SubFilter);
            return new ElementEnumeratorWithFilter<ShapeInterface>(m_Shapes, Filter);
        }
        public ElementEnumerator<ShapeInterface> GetSelectedShapes() {
            return new ElementEnumeratorWithFilter<ShapeInterface>(m_Shapes, new ShapeEnumeratorFilterBySelection());
        }
        public void UnselectAll() {
            ElementEnumerator<ShapeInterface> Enumerator = GetShapeEnumerator();
            while (Enumerator.MoveNext()) {
                Enumerator.Current.Select(false);
            }
            FireUpdateEvent(this, null);
        }
        public ShapeInterface FindShape(string Path) {
            ShapeInterface Shape = null;
            ElementEnumerator<ShapeInterface> Enumerator = GetShapeEnumerator();
            while (Enumerator.MoveNext()) { //?? browse hierarchy

                if (Enumerator.Current.GetName() == Path) {
                    Shape = Enumerator.Current;
                    break;
                }

            }
            return Shape;
        }
        public CmdStack GetUndoStack() { return m_UndoStack; }
        public Rectangle GetPageArea() {
            return GetPage().GetPageArea();
        }
        public void SetPageArea(Rectangle PageSize) {
            GetPage().SetPageArea(PageSize);
            FireUpdateEvent(this, new EventArgs());
        }

        #region Save/Load
        public void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteElementStart("Model");
            GetPage().WriteToSerializer(Stream);
            //Stream.WriteData("PageArea", GetPageArea());
            Core.ElementEnumerator<ShapeInterface> Iterator = GetShapeEnumerator();
            string Node = "Shape";
            while (Iterator.MoveNext()) {
                Stream.WriteElementStart(Node);
                Iterator.Current.WriteToSerializer(Stream);
                Stream.WriteElementEnd(Node);
            }
            Stream.WriteElementEnd("Model");
        }
        public void ReadFromSerializer(SerializerBase Stream) {
            string NodeGroup;
            int StartNodeLevel = 0, CurrNodeLevel = 0;
            do {
                NodeGroup = Stream.GetNodeName();
                CurrNodeLevel = Stream.GetNodeLevel();
                if (CurrNodeLevel < StartNodeLevel) { break; }
                if (Stream.GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                    if (NodeGroup == "Shape") {
                        ShapeInterface _Shape = ShapeFactory.DeserializeShape(Stream);
                        AddShape(_Shape);
                    } else if (NodeGroup == "Page") {
                        GetPage().ReadFromSerializer(Stream);
                        //SetPageArea(Stream.ReadAsRect("PageArea"));
                    } else if (NodeGroup == SerializerXML.FieldName.SerializerDocName.ToString()) {
                        if (NodeGroup != "JKFlOW")
                            throw new Exception(SerializerXML.FieldName.SerializerDocName.ToString() + " unknown");
                    }
                }

            } while (Stream.ReadNext());
            Core.HandleEnumerator Iterator = new HandleEnumerator(GetShapeEnumerator());
            while (Iterator.MoveNext()) {//reconnect Plugs if possible by just calling move
                if (Iterator.Current.IsPlug()) {
                    Iterator.Current.MoveBoundingBox(new Point(0, 0), true);
                }
            }
        }
        public string GetFileName() {
            return m_FileName;
        }
        public virtual void LoadFromFile(string FileName) {
            m_FileName = FileName;
            string DocType = string.Empty;
            SerializerXML _stream = null;
            try {
                _stream = new SerializerXML("JKFLOW", "1.0.0.0");
                _stream.OpenInputStream(FileName);
                ReadFromSerializer(_stream);
                _stream.CloseInputStream();
                _stream = null;
            } catch (Exception e) {
                throw (e);
            } finally {
                if (_stream != null) _stream.CloseInputStream();
            }
        }
        static public ModelDiagram CreateFromFile(string FileName) {
            ModelDiagram _Model = new ModelDiagram();
            _Model.LoadFromFile(FileName);

            return _Model;
        }
        public void SaveToFile(string FileName) {
            m_FileName = FileName;
            SaveToFile();
        }
        public void SaveToFile() {
            SerializerXML _stream = null;
            try {
                _stream = new SerializerXML("JKFLOW", "1.0.0.0");
                _stream.OpenOutputStream(m_FileName);
                WriteToSerializer(_stream);
                _stream.CloseOutputStream();
                _stream = null;
                m_IsModified = false;
            } catch (Exception e) {
                throw (e);
            } finally {
                if (_stream != null) _stream.CloseOutputStream();
            }
        }
        #endregion

        #region fields
        private InterfaceDiagram m_Page;
        private List<ShapeInterface> m_Shapes;
        private CmdStack m_UndoStack;
        protected string m_FileName = "";
        private bool m_IsModified = false;
        protected Core.ControllerDocument m_Controller = null;
        #endregion
    }

}
