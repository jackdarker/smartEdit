using System;
using System.Collections.Generic;
using System.Text;
using smartEdit.Core;

namespace smartEdit.Cmds {
    //Opens a file in the editor.
    public class CmdOpenFile : CmdBase {
        public CmdOpenFile(String Filename)
            : base(null) {
                SetContext(Filename);
        }

        public override void Undo() {

        }
        public override void Redo() {
            ControllerDocument.Instance.OpenEditorForFile(m_FilePath);
        }
        private void SetContext( String Filename) {
            m_FilePath = Filename;
            SetText("Open " + Filename);
        }
        String m_FilePath;
    }
}
