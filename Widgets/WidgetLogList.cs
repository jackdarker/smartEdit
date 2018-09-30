using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace smartEdit.Widgets
{
    public partial class WidgetLogList : UserControl {
        delegate void UpdateViewDelegate();
        UpdateViewDelegate _updateView;

        public WidgetLogList() {
            InitializeComponent();

            lviewTaskList.Columns.Add("Time", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Severity", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Source", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Info", -2, HorizontalAlignment.Left);

            _updateView = new UpdateViewDelegate(_UpdateView);
            Log.getInstance().EvtLogChanged += new Log.LogChanged(OnLogChanged);
        }

        //called by Event
        void OnLogChanged() {
            while(!this.IsHandleCreated)
                Thread.Sleep(1);
            this.Invoke(_updateView);
        }

        void _UpdateView() {
            //Utility.Debug("update log view: ");
            lviewTaskList.Items.Clear();
            //TODO make threadsafe
            ListViewItem _item;
            List<Log.LogData>.Enumerator x = Log.getInstance().GetMessages().GetEnumerator();
            while(x.MoveNext()) {
                String[] y = {x.Current.Time.ToLongTimeString(),
                   x.Current.Severity.ToString(),x.Current.Source, x.Current.Message };
                _item= new ListViewItem(y);
                _item.Tag = x.Current.Source;
                lviewTaskList.Items.Insert(0,_item);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) {
            Log.getInstance().Clear();
        }

        private void lviewTaskList_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (this.lviewTaskList.SelectedItems.Count>0 && this.lviewTaskList.SelectedItems[0] == null)
                return;
            String tag = this.lviewTaskList.SelectedItems[0].Tag as String;
            if (tag == null)
                return;
            String[] _file = tag.Split('@');
            int _Pos = 0;
            if(_file.Length>1)
                _Pos=Convert.ToInt32(_file[1]);
            if (File.Exists(_file[0])) {
           //??     Jump.Add("", _file[0], _Pos);
            //    Jump.Cursor.Go();
                //NPP.GoToDefinition(tag.SourceFile, tag.LineNo - 1, tag.TagName);
            }
        }
    }

    
}
