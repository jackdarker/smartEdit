using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace smartEdit {
    static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MDIParent());
            //Application.Run(new Form1());

        }
    }
}
