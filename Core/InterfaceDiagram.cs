using System;
using System.Collections.Generic;
using System.Text;

namespace smartEdit.Core {
    public class InterfaceDiagram : ICloneable, ISerializable {
        #region constructors
        public InterfaceDiagram() { }
        public virtual object Clone() {
            InterfaceDiagram Clone = new InterfaceDiagram();
            return Clone;
        }
        #endregion

        public virtual void SetModel(ModelDiagram Model) {
            m_Model = Model;
        }
        public ModelDiagram GetModel() { return m_Model; }
        public void SetName(string Name) { m_Name = Name; }
        public string GetName() { return m_Name; }
        public virtual void SetPageArea(System.Drawing.Rectangle PageSize) {
            m_PageArea = PageSize;
        }
        public virtual System.Drawing.Rectangle GetPageArea() {
            return m_PageArea;
        }
        public virtual string GetTypeName() { return "InterfaceDiagram"; }
        public virtual void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) { }
        public virtual System.Drawing.Point ConstrainPosition(System.Drawing.Point Pos) {
            return Pos;
        }
        public virtual System.Drawing.Rectangle ConstrainPosition(System.Drawing.Rectangle Pos, bool Shrink) {
            return Pos;
        }
        public virtual System.Drawing.Rectangle ConstrainPosition(System.Drawing.Rectangle Pos) {
            return ConstrainPosition(Pos, false);
        }
        public virtual void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteElementStart("Page");
            Stream.WriteData("PageSize", GetPageArea());
            Stream.WriteElementEnd("Page");
        }
        public virtual void ReadFromSerializer(SerializerBase Stream) {
            SetPageArea(Stream.ReadAsRect("PageSize"));
        }

        #region fields
        private string m_Name;
        protected ModelDiagram m_Model;
        protected System.Drawing.Rectangle m_PageArea;
        #endregion

    }
}
