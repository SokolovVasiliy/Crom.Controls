using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
    internal static class RecursiveForeach
    {
        /// <summary>
        /// Make action 'controlProcess' foreach child element in control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="controlProcess"></param>
        public static void ForeAllElements(this Control control, Action<Control> controlProcess)
        {
            foreach(Control c in control.Controls)
                ForeAllElements(c, controlProcess);
            controlProcess(control);
        }
    }
}
