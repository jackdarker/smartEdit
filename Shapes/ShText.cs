using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.ComponentModel;
using smartEdit.Core;

namespace smartEdit.Shapes {
    public class ShapeText : ShapeBase {
        public ShapeText()
            : base() {
            m_Pen = new System.Drawing.Pen(System.Drawing.Color.Black);
            m_StringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
            m_Brush = new SolidBrush(Color.Black);
            m_Font = new Font("Arial", 16);
        }
        public override object Clone() {
            ShapeText NewShape = new ShapeText();
            return NewShape;
        }
        public override void DrawShape(Graphics Graphic, ShapeDrawingContext Context) {

            Graphic.DrawString(m_Text, m_Font, m_Brush, Context.ToScreen(GetBoundingBox()));
        }
        public override ShapeBase.ShapeClass GetShapeClass() {
            return ShapeClass.Shape;
        }
        public override string GetShapeTypeName() { return "Text"; }
        public override void WriteToSerializer(SerializerBase Stream) {
            base.WriteToSerializer(Stream);
            Stream.WriteData("Text", Text);
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            base.ReadFromSerializer(Stream);
            Text = Stream.ReadAsString("Text");
        }
        [CategoryAttribute("Data")]
        public string Text {
            get { return m_Text; }
            set { m_Text = (value); }
        }
        string m_Text = "Test Text";
        StringFormat m_StringFormat = null;
        SolidBrush m_Brush = null;
        Font m_Font = null;
        private System.Drawing.Pen m_Pen;
    }
}
