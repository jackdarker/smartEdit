using System;
using System.Collections.Generic;
using System.Text;

namespace smartEdit.Core {
    /// <summary>
    /// Undo-Command-Stack
    /// Stores and Executes Commands for Undo and Redo
    /// </summary>
    public class CmdStack {
        public CmdStack() {
            m_CmdStack = new List<CmdBase>();
            m_CurrentCmd = -1;
        }

        #region Event & Delegates
        public class BoolEventArgs : EventArgs {
            public BoolEventArgs(bool BoolState) {
                State = BoolState;
            }
            public bool State;
        }
        public event EventHandler<EventArgs> EventUpdate;
        public event EventHandler<BoolEventArgs> EventCanRedoChanged;
        public event EventHandler<BoolEventArgs> EventCanUndoChanged;
        /// <summary>
        /// Connect this to an Event-Source (f.e. button click) to trigger Redo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RedoEvent(object sender, EventArgs e) {
            if (this != null) Redo();
        }
        /// <summary>
        /// Connect this to an Event-Source (f.e. button click) to trigger Undo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UndoEvent(object sender, EventArgs e) {
            if (this != null) Undo();
        }
        protected void FireUpdateEvent() {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EventArgs> handler = EventUpdate;
            EventArgs e = new EventArgs();
            // Event will be null if there are no subscribers
            if (handler != null) {
                // Format the string to send inside the EventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        protected void FireCanRedoChanged() {
            EventHandler<BoolEventArgs> handler = EventCanRedoChanged;
            BoolEventArgs e = new BoolEventArgs(CanRedo());
            if (handler != null) handler(this, e);
        }
        protected void FireCanUndoChanged() {
            EventHandler<BoolEventArgs> handler = EventCanUndoChanged;
            BoolEventArgs e = new BoolEventArgs(CanUndo());
            if (handler != null) handler(this, e);
        }
        #endregion
        /// <summary>
        /// Pushs a command on the undo-stack and executes redo
        /// </summary>
        /// <param name="Command"></param>
        public virtual void Push(CmdBase Command) {
            if (CanRedo()) {
                m_CmdStack.RemoveRange((m_CurrentCmd + 1), (m_CmdStack.Count - m_CurrentCmd - 1));
            };
            m_CmdStack.Add(Command);
            if (m_CmdStack.Count > m_MaxUndo) {
                m_CmdStack.RemoveRange(0, m_CmdStack.Count - m_MaxUndo);
            }
            m_CurrentCmd = Math.Max(-1, m_CmdStack.Count - 2);
            Redo();
        }
        public virtual bool CanRedo() {
            return m_CurrentCmd < m_CmdStack.Count - 1;
        }
        public virtual bool CanUndo() {
            return m_CurrentCmd >= 0;
        }
        /// <summary>
        /// Redos the last command on the stack if there is one
        /// </summary>
        public virtual void Redo() {

            if (CanRedo()) {
                m_CurrentCmd++;
                m_CmdStack[m_CurrentCmd].Redo();
                FireCanRedoChanged();
                FireCanUndoChanged();
                FireUpdateEvent();
            }
        }
        /// <summary>
        /// Undos the last command on the stack if there is one
        /// </summary>
        public virtual void Undo() {
            if (CanUndo()) {
                m_CmdStack[m_CurrentCmd].Undo();
                m_CurrentCmd--;
                FireCanRedoChanged();
                FireCanUndoChanged();
                FireUpdateEvent();
            }
        }
        /// <summary>
        /// Get textinfo of the Undo of the current command on the stack
        /// </summary>
        /// <returns></returns>
        public virtual string GetUndoText() {
            if (!CanUndo()) return "Undo";
            return "Undo " + m_CmdStack[m_CurrentCmd].GetText();
        }
        /// <summary>
        /// Get textinfo of the Redo of the current command on the stack
        /// </summary>
        /// <returns></returns>
        public virtual string GetRedoText() {
            if (!CanRedo()) return "Redo";
            return "Redo " + m_CmdStack[m_CurrentCmd + 1].GetText();
        }
        private const int m_MaxUndo = 10;
        private int m_CurrentCmd;
        private List<CmdBase> m_CmdStack;
    }

    /// <summary>
    /// Manages several CmdStack. 
    /// Mainly used for MDI application where you want to use one Stack for each document.
    /// </summary>
    public class CmdStackGroup : CmdStack {
        public CmdStackGroup() {
            m_Stacks = new List<CmdStack>();
            m_ActiveStack = null;
        }
        #region Event & Delegates

        #endregion

        public CmdStack GetActiveStack() {
            return m_ActiveStack;
        }
        public override string GetUndoText() {
            if (!CanUndo()) return "";
            return m_ActiveStack.GetUndoText();
        }
        public override string GetRedoText() {
            if (!CanRedo()) return "";
            return m_ActiveStack.GetRedoText();
        }
        public void SetActiveStack(CmdStack Stack) {
            if (m_Stacks.Contains(Stack)) {
                m_ActiveStack = Stack;
                FireCanRedoChanged();
                FireCanUndoChanged();
                FireUpdateEvent();
            }
        }
        public void AddStack(CmdStack Stack) {
            if (!m_Stacks.Contains(Stack)) {
                m_Stacks.Add(Stack);
                Stack.EventCanRedoChanged += new EventHandler<BoolEventArgs>(Stack_EventCanRedoChanged);
                Stack.EventCanUndoChanged += new EventHandler<BoolEventArgs>(Stack_EventCanUndoChanged);
                Stack.EventUpdate += new EventHandler<EventArgs>(Stack_EventUpdate);
            }
        }

        private void Stack_EventUpdate(object sender, EventArgs e) {
            FireUpdateEvent();
        }
        private void Stack_EventCanUndoChanged(object sender, CmdStack.BoolEventArgs e) {
            FireCanUndoChanged();
        }
        private void Stack_EventCanRedoChanged(object sender, CmdStack.BoolEventArgs e) {
            FireCanRedoChanged();
        }
        public override void Push(CmdBase Command) {
            if (m_ActiveStack == null) return;
            m_ActiveStack.Push(Command);
        }
        public void RemoveStack(CmdStack Stack) {
            if (m_Stacks.Contains(Stack)) {
                Stack.EventUpdate -= Stack_EventUpdate;
                Stack.EventCanRedoChanged -= Stack_EventCanRedoChanged;
                Stack.EventCanUndoChanged -= Stack_EventCanUndoChanged;
                m_Stacks.Remove(Stack);
            };
            if (m_ActiveStack == Stack)
                m_ActiveStack = null;
        }
        public override bool CanRedo() {
            if (m_ActiveStack == null) return false;
            return m_ActiveStack.CanRedo();
        }
        public override bool CanUndo() {
            if (m_ActiveStack == null) return false;
            return m_ActiveStack.CanUndo();
        }
        public override void Undo() {
            if (CanUndo()) {
                m_ActiveStack.Undo();
                FireCanRedoChanged();
                FireCanUndoChanged();
                FireUpdateEvent();
            }
        }
        public override void Redo() {
            if (CanRedo()) m_ActiveStack.Redo();
            FireCanRedoChanged();
            FireCanUndoChanged();
            FireUpdateEvent();
        }

        CmdStack m_ActiveStack;
        List<CmdStack> m_Stacks;
    }
}