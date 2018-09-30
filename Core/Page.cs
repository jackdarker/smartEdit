using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace smartEdit.Core {
    public class Page : InterfaceDiagram {
        public Page(ModelDiagram Model) {
            this.SetModel(Model);
            SetPageArea(new System.Drawing.Rectangle(0, 0, 600, 400));
            m_Pen = System.Drawing.Pens.Gray;
            SetGridWidth(10);
            SetGridMode(1);
        }
        public override string GetTypeName() { return "SimplePage"; }
        public override void SetPageArea(System.Drawing.Rectangle PageSize) {
            m_PageArea = PageSize;
            UpdateGridSnapPoints();
        }
        public override void Draw(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            DrawGrid(Graphic, Context);
            //Graphic.DrawLine(m_Pen,GetPageArea().Location,GetPageArea().Location+GetPageArea().Size);
        }
        public override System.Drawing.Point ConstrainPosition(System.Drawing.Point Pos) {
            int _delta, _mindelta = int.MaxValue;
            System.Drawing.Point NewPos = Pos;
            if (GetGridMode() > 0) {
                foreach (System.Drawing.Point Pt in m_GridPoints) {
                    _delta = Math.Abs(Pt.X - Pos.X) + Math.Abs(Pt.Y - Pos.Y);
                    if ((_delta < _mindelta)) {
                        _mindelta = _delta;
                        NewPos = Pt;
                    }
                }
            };
            return NewPos;

        }
        public override System.Drawing.Rectangle ConstrainPosition(System.Drawing.Rectangle Pos, bool Shrink) {
            Pos.Location = (ConstrainPosition(Pos.Location));
            if (Shrink) {
                System.Drawing.Point PtB = System.Drawing.Point.Add(Pos.Location, Pos.Size);
                PtB = ConstrainPosition(PtB);
                Pos.Width = PtB.X - Pos.Location.X;
                Pos.Height = PtB.Y - Pos.Location.Y;
            }
            return Pos;
        }
        public override void WriteToSerializer(SerializerBase Stream) {
            Stream.WriteElementStart("Page");
            Stream.WriteData("PageSize", GetPageArea());
            Stream.WriteData("GridWidth", GridWidth);
            Stream.WriteData("GridMode", GetGridMode());
            Stream.WriteElementEnd("Page");
        }
        public override void ReadFromSerializer(SerializerBase Stream) {
            SetPageArea(Stream.ReadAsRect("PageSize"));
            SetGridWidth(Stream.ReadAsInt("GridWidth"));
            SetGridMode(Stream.ReadAsInt("GridMode"));
        }

        private void DrawGrid(System.Drawing.Graphics Graphic, ShapeDrawingContext Context) {
            if (GetGridMode() > 0) {
                System.Drawing.Point[] Cross = new System.Drawing.Point[4];
                foreach (System.Drawing.Point Pt in m_GridPoints) {
                    Cross[0].X = Pt.X;
                    Cross[0].Y = Pt.Y - 1;
                    Cross[1].X = Pt.X;
                    Cross[1].Y = Pt.Y + 1;
                    Cross[2].X = Pt.X - 1;
                    Cross[2].Y = Pt.Y;
                    Cross[3].X = Pt.X + 1;
                    Cross[3].Y = Pt.Y;
                    Graphic.DrawLine(m_Pen, Cross[0], Cross[1]);
                    Graphic.DrawLine(m_Pen, Cross[2], Cross[3]);
                }
            }
        }
        public void SetGridWidth(int Width) {
            m_GridWidth = Width;
            UpdateGridSnapPoints();
        }
        public int GetGridWidth() { return m_GridWidth; }
        public void SetGridMode(int Mode) {
            m_GridMode = Mode;
            UpdateGridSnapPoints();
        }
        public int GetGridMode() {
            return m_GridMode;
        }
        private void UpdateGridSnapPoints() {
            List<System.Drawing.Point> Points = new List<System.Drawing.Point>(0);
            if (m_GridWidth > 0) {
                int Margin = m_GridWidth / 2;
                int cx = (GetPageArea().Width - Margin) / m_GridWidth;
                int cy = (GetPageArea().Height - Margin) / m_GridWidth;
                for (int x = 0; x <= cx; x++) {
                    for (int y = 0; y <= cy; y++) {
                        Points.Add(new System.Drawing.Point((x * m_GridWidth) + Margin,
                            (y * m_GridWidth) + Margin));
                    }
                }
            } else {
                m_GridMode = 0;
            }
            m_GridPoints = Points;
        }

        #region properties
        [CategoryAttribute("Layout")]
        public System.Drawing.Size PageSize {
            get { return GetPageArea().Size; }
            set { SetPageArea(new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), (value))); }
        }
        [CategoryAttribute("Layout")]
        public int GridWidth {
            get { return m_GridWidth; }
            set { SetGridWidth(value); }
        }
        [CategoryAttribute("Layout")]
        public bool GridMode {
            get { return GetGridMode() > 0; }
            set { SetGridMode(value ? 1 : 0); }
        }
        #endregion

        #region fields
        private System.Drawing.Pen m_Pen;
        private int m_GridWidth;
        private int m_GridMode;
        private List<System.Drawing.Point> m_GridPoints;
        #endregion
    }
}
