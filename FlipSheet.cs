using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace smartEdit {
    public partial class FlipSheet : UserControl {
        class FlipState {
            public bool IsOpen;
        }

        List<FlipState> FlipControls;

        //gets Control Index of UserControl
        private static int GetRealIndex(int Index) { return Index * 3 + 1 + 1; }
        public FlipSheet() {
            InitializeComponent();
            FlipControls = new List<FlipState>();
        }
        public void Minimize() {
            WidthMax = this.Size.Width;
            SuspendLayout();
            int i = 0;
            foreach (Control Ctrl in tableLayoutPanel1.Controls) {
                if (i == 0) { } else {
                    Ctrl.Visible = false;
                }
                i++;
            }
            if (this.Dock == DockStyle.Bottom || this.Dock == DockStyle.Top) {
                this.Size = new Size(this.Size.Width, HeightMinimized);
            } else {
                this.Size = new Size(WidthMinimized, this.Size.Height);
            }
            tableLayoutPanel1.AutoScroll = false;
            btMinimize.Text = "<>";

            ResumeLayout();
            IsMinimized = true;

        }
        public void SlideOut() {
            SuspendLayout();
            int i = 0;
            foreach (FlipState state in FlipControls) {
                tableLayoutPanel1.Controls[GetRealIndex(i) - 1].Visible = true;
                tableLayoutPanel1.Controls[GetRealIndex(i) + 1].Visible = true;
                HideTab(i, !state.IsOpen);
                i++;
            }
            if (this.Dock == DockStyle.Bottom || this.Dock == DockStyle.Top) {
                this.Size = new Size(this.Size.Width, HeightMax);
            } else {
                this.Size = new Size(WidthMax, this.Size.Height);
            }
            btMinimize.Text = "><";
            SizeToControl();

            //tableLayoutPanel1.AutoScroll = true; //??autoscroll funktioniert nicht richtig
            ResumeLayout();
            IsMinimized = false;
        }
        public void CloseAllTabs() {
            int i = 0;
            foreach (FlipState state in FlipControls) {
                HideTab(i, true);
                i++;
            }
        }

        public void AddControl(Control Ctrl, string Text) {
            if (Ctrl == null) return;
            SuspendLayout();

            //NumCtrls++;
            FlipControls.Add(new FlipState());
            Button Bt = new Button();
            Bt.Text = Text;
            Bt.Dock = System.Windows.Forms.DockStyle.Fill;
            Bt.TabIndex = FlipControls.Count;//NumCtrls;
            Bt.UseVisualStyleBackColor = true;
            Bt.Margin = new System.Windows.Forms.Padding(0);

            Panel Pnl = new Panel();
            Pnl.Dock = DockStyle.Fill;
            Pnl.Size = new Size(0, 0);
            Pnl.Margin = new System.Windows.Forms.Padding(0);



            int RealIndex = GetRealIndex(FlipControls.Count - 1);
            Ctrl.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Controls.Add(Bt, 0, RealIndex - 1);
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, (float)22));
            this.tableLayoutPanel1.Controls.Add(Ctrl, 0, RealIndex);
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.AutoSize));
            this.tableLayoutPanel1.Controls.Add(Pnl, 0, RealIndex + 1);
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, (float)0));
            HideTab(FlipControls.Count - 1, true);
            ResumeLayout();
            SizeToControl();
            Bt.Click += new System.EventHandler(this.Bt_Click);
        }
        public void HideTab(int Index, bool Hide) {
            int RealIndex = GetRealIndex(Index);
            if (RealIndex >= tableLayoutPanel1.Controls.Count) return;
            //??tableLayoutPanel1.VerticalScroll.Enabled = true;
            //tableLayoutPanel1.VerticalScroll.Visible = true;

            tableLayoutPanel1.Controls[RealIndex].Visible = !Hide;
            FlipControls[Index].IsOpen = !Hide;
            SizeToControl();
        }
        public void ToggleTab(int Index) {
            HideTab(Index, FlipControls[Index].IsOpen);
        }
        private void SizeToControl() {
            int i = 0;
            int MaxWitdh = WidthMax;
            int MaxHeight = HeightMax;

            foreach (FlipState state in FlipControls) {
                if (state.IsOpen) {
                    MaxWitdh = Math.Max(MaxWitdh,
                        tableLayoutPanel1.Controls[GetRealIndex(i)].Margin.Size.Width +
                        tableLayoutPanel1.Controls[GetRealIndex(i)].Bounds.Width);
                    MaxHeight = Math.Max(MaxHeight,
                        tableLayoutPanel1.Controls[GetRealIndex(i)].Margin.Size.Height +
                        tableLayoutPanel1.Controls[GetRealIndex(i)].Bounds.Height);
                };
                i++;
            }

            if (this.Dock == DockStyle.Bottom || this.Dock == DockStyle.Top) {
                this.ClientSize = new Size(this.Size.Width, MaxHeight);
            } else {
                this.ClientSize = new Size(MaxWitdh, this.Size.Height);
            }
        }
        private void btMinimize_Click(object sender, EventArgs e) {
            if (IsMinimized) SlideOut();
            else Minimize();
        }

        private void Bt_Click(object sender, EventArgs e) {
            Control Bt = (Control)sender;
            int BtIndex = -1;
            int i = 0;
            foreach (Control Ctrl in tableLayoutPanel1.Controls) {
                if (Ctrl.Equals(Bt)) BtIndex = i;
                i++;
            }
            BtIndex--;
            BtIndex = BtIndex / 2;
            ToggleTab(BtIndex);
        }
        private bool IsMinimized;
        //private int NumCtrls;
        private int WidthMax = 100;
        private int WidthMinimized = 36;
        private int HeightMax = 100;
        private int HeightMinimized = 36;
    }
}
