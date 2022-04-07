using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
    internal static class BringToFrontHelper
    {
        public static void OnBringToFront(object sender, MouseEventArgs e)
        {
            List<Control> controls = new List<Control>();
            Control cnt = (Control)sender;
            for (Control c = cnt; c.Parent != null; c = c.Parent)
                controls.Add(c);
            if (controls.Count < 2)
                return;
            DockContainer dock = controls[controls.Count - 1] as DockContainer;
            DockableContainer dockContainer = controls[controls.Count - 2] as DockableContainer;
            if (dock != null && dockContainer != null)
            {
                dock.Controls.SetChildIndex(dockContainer, 0);
            }
        }

        private static void BringToFront(Control.ControlCollection collection, Control control)
        {
            Control[] array = new Control[collection.Count];
            collection.Remove(control);
            array[0] = control;
            collection.CopyTo(array, 1);
            collection.Clear();
            collection.AddRange(array);
        }

    }
}
