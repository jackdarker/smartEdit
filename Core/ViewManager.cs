using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace smartEdit.Core {

    //Helper class for Controller to keep track which file is already shown
    public class ViewManager {
        public ViewManager() {
            m_FormList = new Dictionary<String, Form>();
        }

        public Form GetFormForFile(String Path) {
            Form _frm = null;
            m_FormList.TryGetValue(Path,out _frm);
            return _frm;
        }
        public void AddForm(String Path, Form Form) {
            m_FormList.Add(Path, Form);
        }
        public void RemoveForm(Form Form) {
            String _Key = String.Empty;
            foreach (KeyValuePair<String, Form> _entry in m_FormList){
                if (_entry.Value.Equals(Form)) {
                    _Key = _entry.Key;
                    break;
                }
            }
            if (_Key != String.Empty) m_FormList.Remove(_Key);
        }
        #region fields
        System.Collections.Generic.Dictionary<String, Form> m_FormList;
        #endregion
    }
}
