using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace smartEdit {
    public class Log {  //Todo put in own class file
        private Log() {
            m_Data = new List<LogData>();
        }
        static Log s_Instance;
        static public Log getInstance() {
            if (s_Instance == null) {
                s_Instance = new Log();
            }
            return s_Instance;
        }
        public struct LogData {
            public String Message;
            public DateTime Time;
            public EnuSeverity Severity;
            public String Source;
        }
        public enum EnuSeverity { Error = -1, Warn = 1, Info = 2 };

        protected List<LogData> m_Data;
        protected int m_LastID = 1;
        public void Add(String Message, EnuSeverity Severity, String Source) {
            LogData _l = new LogData();
            _l.Message = Message;
            _l.Severity = Severity;
            _l.Time = DateTime.Now;
            _l.Source = Source;
            if (m_Data.Count > 1000) {
                m_Data.RemoveRange(0, 900);
            }

            m_Data.Add(_l);
            if (EvtLogChanged != null) EvtLogChanged();
        }

        public List<LogData> GetMessages() {
            return m_Data;
        }
        public void Clear() {
            m_Data.Clear();
        }
        public event LogChanged EvtLogChanged;
        public delegate void LogChanged();

    }
}
