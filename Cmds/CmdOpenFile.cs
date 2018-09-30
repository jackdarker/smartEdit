using System;
using System.Collections.Generic;
using System.Text;
using smartEdit.Core;

namespace smartEdit.Cmds {
    //Opens a file in the editor.
    public class CmdOpenFile : CmdBase {
        public CmdOpenFile(ModelDocument Project, String Filename)
            : base(null) {
                SetContext(Project, Filename);
        }

        public override void Undo() {

        }
        public override void Redo() {

        }
        private void SetContext(ModelDocument Project, String Filename) {
            m_Project = Project;
            m_FilePath = Filename;
            SetText("Edit " + Filename);
        }
        ModelDocument m_Project;
        String m_FilePath;
    }
}
