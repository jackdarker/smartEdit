using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace smartEdit.Core {

    /// <summary>
    /// helper class to store info for a view/Form
    /// </summary>
    [Serializable]
    public class ViewData {
        private string m_File = "";
        public string File {
            get {
                return m_File;
            }
            set {
                m_File = value;
                Modified = true;
            }
        }

        [NonSerialized]
        public bool Modified;
    }
}
