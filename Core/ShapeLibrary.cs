using System;
using System.Collections.Generic;
using System.Text;

namespace smartEdit.Core {
    public class ShapeFactory {
        public static void InitShapeFactory() {
            m_ShapeLibrary = new ShapeLibrary();
            m_ShapeLibrary.AddShape(new ShapeRect());
            m_ShapeLibrary.AddShape(new ShapeEllipse());
            m_ShapeLibrary.AddShape(new ShapeImage());
            m_ShapeLibrary.AddShape(new smartEdit.Shapes.ShapeText());
            m_ShapeLibrary.AddShape(new ConnectorStraight());
            m_ShapeLibrary.AddShape(new ConnectorRect());
        }
        public static ShapeInterface DeserializeShape(SerializerBase Stream) {
            string TypeOfShape = Stream.ReadAsString("Type");
            if (TypeOfShape == string.Empty) return null;
            ShapeInterface Shape = CreateShape(TypeOfShape);
            Shape.ReadFromSerializer(Stream);
            return Shape;
        }
        public static ShapeInterface CreateShape(string ShapeName) {
            if (ShapeName == "") ShapeName = m_DefaultShape;
            m_ShapeCounter++;
            ShapeInterface Shape = m_ShapeLibrary.GetShape(ShapeName);
            Shape.SetName(Shape.GetShapeTypeName() + m_ShapeCounter.ToString());
            return Shape;
        }
        public static void SetShapeTemplate(ShapeInterface Shape) {
            m_DefaultShape = Shape.GetShapeTypeName();
        }
        public static IEnumerator<string> GetListOfShapes() { return m_ShapeLibrary.GetListOfShapes(); }
        private static string m_DefaultShape = "";
        private static UInt32 m_ShapeCounter = 0;
        private static ShapeLibrary m_ShapeLibrary = new ShapeLibrary();
    }
    public class ShapeLibrary {
        public ShapeLibrary() {
            m_Library = new Dictionary<string, ShapeInterface>();
        }
        public void AddShape(ShapeInterface Shape, string Name) {
            RemoveShape(Name);
            m_Library.Add(Name, Shape);
        }
        public void AddShape(ShapeInterface Shape) {
            AddShape(Shape, Shape.GetShapeTypeName());
        }
        public void RemoveShape(string Name) {
            if (m_Library.ContainsKey(Name)) {
                m_Library.Remove(Name);
            }
        }
        public ShapeInterface GetShape(string Name) {
            ShapeInterface Shape = null;
            if (m_Library.ContainsKey(Name)) Shape = (ShapeInterface)m_Library[Name].Clone();
            return Shape;
        }
        public IEnumerator<string> GetListOfShapes() {
            return m_Library.Keys.GetEnumerator();
        }
        private Dictionary<string, ShapeInterface> m_Library;
    }
}
