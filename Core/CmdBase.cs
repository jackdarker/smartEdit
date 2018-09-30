using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace smartEdit.Core {
    /// <summary>
    /// Base class that for commands. Works together with CmdStack.
    /// </summary>
    public class CmdBase {
        public CmdBase() {
            SetText("");
        }
        public CmdBase(CmdBase Parent) {
            m_Parent = Parent;
            SetText("");
        }
        public CmdBase(string Text, CmdBase Parent) {
            m_Parent = Parent;
            SetText(Text);
        }
        public virtual int GetID() { return -1; }
        public virtual bool MergeCmd(CmdBase OtherCmd) {

            return false;
        }
        public virtual void Redo() { }
        public virtual void Undo() { }
        public void SetText(string Text) {
            if (Text != "") {
                m_Text = Text;
            } else {
                m_Text = "Command ID " + GetID().ToString();
            }
        }
        public string GetText() {
            return m_Text;
        }

        #region fields
        string m_Text;
        CmdBase m_Parent;
        #endregion

    }

    /// <summary>
    /// Base class that for commands. Works together with CmdStack.
    /// </summary>
    public class CmdMacro : CmdBase {
        public CmdMacro()
            : base() {
            SetText("");
            ClearCmds();
        }
        public CmdMacro(CmdBase Parent)
            : base(Parent) {
            SetText("");
            ClearCmds();
        }
        public CmdMacro(string Text, CmdBase Parent)
            : base(Parent) {
            SetText(Text);
            ClearCmds();
        }
        protected void ClearCmds() {
            if (m_CmdStack != null) {
                m_CmdStack.Clear();
            }
        }
        /// <summary>
        /// Add command to this command. 
        /// Undo will be executed in the order off add. Redo in backward order.
        /// </summary>
        /// <param name="Cmd"></param>
        public virtual void AddCmd(CmdBase Cmd) {
            if (m_CmdStack == null) m_CmdStack = new List<CmdBase>(1);
            m_CmdStack.Add(Cmd);
            SetText(Cmd.GetText());
            if (m_CmdStack.Count > 1) {
                SetText(GetText() +
                   "+" + (m_CmdStack.Count - 1).ToString() + " other operations");
            }
        }
        public override int GetID() { return -1; }
        public override bool MergeCmd(CmdBase OtherCmd) {
            return false;
        }
        public override void Redo() {
            if (m_CmdStack == null) return;
            List<CmdBase>.Enumerator Iter = m_CmdStack.GetEnumerator();
            int i = m_CmdStack.Count;  //going forward
            while (i > 0) {
                i--;
                m_CmdStack[i].Redo();
            }
        }
        public override void Undo() {
            if (m_CmdStack == null) return;
            List<CmdBase>.Enumerator Iter = m_CmdStack.GetEnumerator();
            int i = 0;
            int k = m_CmdStack.Count;  //going forward
            while (i < k) {
                m_CmdStack[i].Undo();
                i++;
            }
        }

        #region fields
        protected List<CmdBase> m_CmdStack = null;
        #endregion

    }

}
