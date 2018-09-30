using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace smartEdit.Widgets {

    /// <summary>
    /// User Control to display Image in different scalings
    /// </summary>
    public partial class WidgetDiagramCanvas : UserControl {
        public WidgetDiagramCanvas() {
            InitializeComponent();
            //surpress flicker by disabling auto-clearbackground
            this.SetStyle(ControlStyles.EnableNotifyMessage | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();

            //this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_Paint);
            //  this.Resize += new System.EventHandler(this.Canvas_Resize);
        }


        /*public void SetScaling(int Zoom)
        {
            m_ImageScalingMode = Zoom;
            ScaleImage();
        }
        public int GetScaling()
        {
            return m_ImageScalingMode;
        }
        public Point GetScrollPosition()
        {
            return new Point(this.HorizontalScroll.Value, this.VerticalScroll.Value);
        }
        public void SetScrollPosition(Point Pos)
        {
            this.AutoScrollPosition = Pos;
        }
        private void ScaleImage()
        {
            bool _ImageLoaded = (m_CurrImage != null);
            m_ScaledImage = null;
            if (_ImageLoaded)
            {
                int ClientHeight = this.ClientSize.Height;
                int ClientWidth = this.ClientSize.Width;
                int ImageHeight = m_CurrImage.Height;
                int ImageWidth = m_CurrImage.Width;
                float scale1 = (float)ClientWidth / (float)ImageWidth;
                float scale2 = (float)ClientHeight / (float)ImageHeight;
                float scale = Math.Min(scale1, scale2);
                if (m_ScaledImage != null)
                {
                    m_ScaledImage.Dispose();
                    m_ScaledImage = null;
                };

                if ((m_ImageScalingMode == -1) || (m_ImageScalingMode == -2 && scale < 1))
                {
                    m_ScaledImage = new Bitmap(m_CurrImage, (int)((float)ImageWidth * scale),
                        (int)((float)ImageHeight * scale));
                }
                else if ((1 <= m_ImageScalingMode) && (m_ImageScalingMode <= 1000))
                {
                    m_ScaledImage = new Bitmap(m_CurrImage,
                        (ImageWidth * m_ImageScalingMode) / 100, (ImageHeight * m_ImageScalingMode) / 100);
                }
                else
                {
                    m_ScaledImage = new Bitmap(m_CurrImage);
                }
                this.panel1.ClientSize = m_ScaledImage.Size;
            }
            this.Invalidate(true);
        }
        private void Canvas_Resize(object sender, EventArgs e)
        {
            ScaleImage();
        }

        private int m_ImageScalingMode = 100;
        private Bitmap m_ScaledImage = null;
        private Image m_CurrImage = null;
        */


    }
    public class WidgetDiagramCanvasCore : Panel {
        public WidgetDiagramCanvasCore()
            : base() {
            //surpress flicker by disabling auto-clearbackground
            this.SetStyle(ControlStyles.EnableNotifyMessage | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();
        }
    }

}
