﻿namespace smartEdit.Widgets
{
    partial class frmTagList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbar = new System.Windows.Forms.ToolStrip();
            this.ttxtSearchText = new System.Windows.Forms.ToolStripTextBox();
            this.tbtnSearch = new System.Windows.Forms.ToolStripButton();
            this.tbtnPrev = new System.Windows.Forms.ToolStripButton();
            this.tbtnNext = new System.Windows.Forms.ToolStripButton();
            this.tbtnRefresh = new System.Windows.Forms.ToolStripButton();
            this.tvClassView = new System.Windows.Forms.TreeView();
            this.tbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbar
            // 
            this.tbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ttxtSearchText,
            this.tbtnSearch,
            this.tbtnPrev,
            this.tbtnNext,
            this.tbtnRefresh});
            this.tbar.Location = new System.Drawing.Point(0, 0);
            this.tbar.Name = "tbar";
            this.tbar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.tbar.Size = new System.Drawing.Size(221, 25);
            this.tbar.TabIndex = 0;
            this.tbar.Text = "toolStrip1";
            // 
            // ttxtSearchText
            // 
            this.ttxtSearchText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ttxtSearchText.Name = "ttxtSearchText";
            this.ttxtSearchText.Size = new System.Drawing.Size(100, 25);
            this.ttxtSearchText.ToolTipText = "Search";
            this.ttxtSearchText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tcboxSearchText_KeyPress);
            // 
            // tbtnSearch
            // 
            this.tbtnSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
//            this.tbtnSearch.Image = global::NppProject.Properties.Resources.Search;
            this.tbtnSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnSearch.Name = "tbtnSearch";
            this.tbtnSearch.Size = new System.Drawing.Size(23, 22);
            this.tbtnSearch.Text = "Search";
            this.tbtnSearch.Click += new System.EventHandler(this.tbtnSearch_Click);
            // 
            // tbtnPrev
            // 
            this.tbtnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
//            this.tbtnPrev.Image = global::NppProject.Properties.Resources.Prev;
            this.tbtnPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnPrev.Name = "tbtnPrev";
            this.tbtnPrev.Size = new System.Drawing.Size(23, 22);
            this.tbtnPrev.Text = "Previous Tag";
            this.tbtnPrev.Click += new System.EventHandler(this.tbtnPrev_Click);
            // 
            // tbtnNext
            // 
            this.tbtnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
 //           this.tbtnNext.Image = global::NppProject.Properties.Resources.Next;
            this.tbtnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnNext.Name = "tbtnNext";
            this.tbtnNext.Size = new System.Drawing.Size(23, 22);
            this.tbtnNext.Text = "Next Tag";
            this.tbtnNext.Click += new System.EventHandler(this.tbtnNext_Click);
            // 
            // tbtnRefresh
            // 
            this.tbtnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
 //           this.tbtnRefresh.Image = global::NppProject.Properties.Resources.Refresh;
            this.tbtnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnRefresh.Name = "tbtnRefresh";
            this.tbtnRefresh.Size = new System.Drawing.Size(23, 22);
            this.tbtnRefresh.Text = "Refresh";
            this.tbtnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // tvClassView
            // 
            this.tvClassView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvClassView.Location = new System.Drawing.Point(0, 25);
            this.tvClassView.Name = "tvClassView";
            this.tvClassView.ShowNodeToolTips = true;
            this.tvClassView.Size = new System.Drawing.Size(221, 372);
            this.tvClassView.TabIndex = 1;
            this.tvClassView.DoubleClick += new System.EventHandler(this.tvClassView_DoubleClick);
            this.tvClassView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvClassView_KeyDown);
            // 
            // frmTagList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(221, 397);
            this.Controls.Add(this.tvClassView);
            this.Controls.Add(this.tbar);
            this.Name = "frmTagList";
            this.Text = "frmTagList";
            this.tbar.ResumeLayout(false);
            this.tbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tbar;
        private System.Windows.Forms.ToolStripButton tbtnSearch;
        private System.Windows.Forms.ToolStripButton tbtnRefresh;
        private System.Windows.Forms.TreeView tvClassView;
        private System.Windows.Forms.ToolStripButton tbtnPrev;
        private System.Windows.Forms.ToolStripButton tbtnNext;
        private System.Windows.Forms.ToolStripTextBox ttxtSearchText;
    }
}