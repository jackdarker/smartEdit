using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace smartEdit {


    public partial class WidgetShapePalette : UserControl, Core.IToolView {
        public WidgetShapePalette() {
            InitializeComponent();
        }

        #region events & delegates
        protected event smartEdit.Core.ShapeSelectedEventHandler EventShapeSelected;
        public void RegisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventShapeSelected += new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnSetShapeTemplate);
        }
        public void UnregisterShapeSelected(smartEdit.Core.ControllerDocument Listener) {
            EventShapeSelected -= new smartEdit.Core.ShapeSelectedEventHandler(Listener.OnSetShapeTemplate);
        }
        protected virtual void FireShapeSelected(object sender, smartEdit.Core.ShapeInterface Shape) {
            smartEdit.Core.ShapeSelectedEventHandler handler = EventShapeSelected;
            if (handler != null) {
                handler(sender, Shape);
            }
        }
        protected virtual void OnShapeSelected(Core.ShapeInterface ShapeBase) {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            smartEdit.Core.ShapeSelectedEventHandler handler = EventShapeSelected;
            EventArgs e = new EventArgs();
            // Event will be null if there are no subscribers
            if (handler != null) {
                // Format the string to send inside the EventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, ShapeBase);
            }
        }
        public event smartEdit.Core.ToolSelectedEventHandler EventToolChanged;
        public void RegisterToolSelected(smartEdit.Core.ControllerDocument Listener) {
            EventToolChanged += new smartEdit.Core.ToolSelectedEventHandler(Listener.OnToolChanged);
        }
        public void UnregisterToolSelected(smartEdit.Core.ControllerDocument Listener) {
            EventToolChanged -= new smartEdit.Core.ToolSelectedEventHandler(Listener.OnToolChanged);
        }
        protected virtual void FireToolSelected(smartEdit.Core.MouseOperation Mode) {
            smartEdit.Core.ToolSelectedEventHandler handler = EventToolChanged;
            EventArgs e = new EventArgs();
            if (handler != null) {
                handler(this, Mode);
            }
        }
        public void OnUpdateEvent(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new smartEdit.Core.UpdateEventHandler(OnUpdateEvent));
            } else {
                UpdateView();
            }
        }

        #endregion
        public void UpdateView() {
            this.ShapeList.Items.Clear();
            this.ShapeList.Items.Add("Pointer");
            IEnumerator<string> ShapeNames = Core.ShapeFactory.GetListOfShapes();
            while (ShapeNames.MoveNext()) {
                this.ShapeList.Items.Add(ShapeNames.Current);
            }

        }
        private void ShapeList_Click(object sender, EventArgs e) {
            if (this.ShapeList.SelectedIndex >= 1) {
                OnShapeSelected(Core.ShapeFactory.CreateShape(this.ShapeList.Items[this.ShapeList.SelectedIndex].ToString()));
                FireToolSelected(smartEdit.Core.MouseOperation.Add);
            } else if (this.ShapeList.SelectedIndex == 0) {
                FireToolSelected(smartEdit.Core.MouseOperation.None);
            }
        }
        public Core.ControllerDocument GetController() { return m_Controller; }
        public void SetController(Core.ControllerDocument Ctrl) {
            if (m_Controller != null) {
                m_Controller.UnregisterView(this);
            }
            m_Controller = Ctrl;
            m_Controller.RegisterView(this);
        }
        protected Core.ControllerDocument m_Controller = null;
        protected Core.ShapeBase m_Shape = new Core.ShapeRect();

    }
}
