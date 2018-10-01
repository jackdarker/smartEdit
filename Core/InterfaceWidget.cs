using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace smartEdit.Core {

    public delegate void ToolSelectedEventHandler(object sender, MouseOperation Tool);
    public delegate void UpdateEventHandler(object sender, EventArgs e);
    public delegate void MouseInputEventHandler(object sender, MouseInputEventArgs e);
    public delegate void MouseFeedbackEventHandler(object sender, MouseOperation e);
    /// <summary>
    /// Interface for the Controller
    /// </summary>
    public interface IController {
        void OnToolChanged(object sender, smartEdit.Core.MouseOperation Tool);
        void OnMouseInput(object sender, MouseInputEventArgs e);
        CmdStackGroup GetCmdStack();
    }
    /// <summary>
    /// Basic-Interface for Views that interoperate with Controller
    /// </summary>
    public interface IView {
        /// <summary>
        /// Attaches View to new Controller
        void SetController(Core.ControllerDocument Ctrl);
        void OnUpdateEvent(object sender, EventArgs e);
        CmdStack GetCmdStack();
    }
    /// <summary>
    /// Basic-Interface for Views that visualize model data with standard controlls like buttons, Listboxes, Combos...
    /// </summary>
    public interface IDataView : IView {
        void OnToolChanged(object sender, smartEdit.Core.MouseOperation Op);
    }
    /// <summary>
    /// Interface for Views that visualize model data graphically, that means additional support of Mouse-Input
    /// </summary>
    public interface IGraphicView : IDataView {
        void RegisterMouseInput(ControllerDocument Listener);
        void UnregisterMouseInput(ControllerDocument Listener);
        void OnMouseFeedback(object sender, smartEdit.Core.MouseOperation Op);
    }
    /// <summary>
    /// Basic-Interface for widgets that visualize operating tools and library
    /// </summary>
    public interface IToolView : IView {
        void RegisterToolSelected(ControllerDocument Listener);
        void UnregisterToolSelected(ControllerDocument Listener);
    }

}
