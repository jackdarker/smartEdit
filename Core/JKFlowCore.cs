using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
namespace smartEdit.Core {
    public enum MouseOperation {
        None = 0,
        Select,
        Move,
        Grab,
        Size,
        Add
    };
    public class MouseInputEventArgs : EventArgs {
        public MouseInputEventArgs(MouseEventArgs MouseArgs,
        KeyEventArgs KeyArgs, MouseOperation Op) {
            MouseArg = MouseArgs;
            KeyArg = KeyArgs;
            Selection = Rectangle.Empty;
            Tool = Op;
        }
        public MouseInputEventArgs(MouseEventArgs MouseArgs,
            KeyEventArgs KeyArgs, MouseOperation Op,
            Rectangle SelectRect) {
            MouseArg = MouseArgs;
            KeyArg = KeyArgs;
            Selection = SelectRect;
            Tool = Op;
        }
        public MouseEventArgs MouseArg;
        public KeyEventArgs KeyArg;
        public Rectangle Selection;
        public MouseOperation Tool;
    }
    public class BoolEventArgs : EventArgs {
        public BoolEventArgs(bool BoolState) {
            State = BoolState;
        }
        public bool State;
    }

}
