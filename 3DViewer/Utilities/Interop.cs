using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _3DViewer.Utilities
{
    public static class Interop
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
    }
}
