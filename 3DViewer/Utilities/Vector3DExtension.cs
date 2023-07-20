using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DViewer.Utilities
{
    public static class Vector3DExtension
    {
        public static Vector3D FromDouble3Array(double[] array)
        {
            return new Vector3D(array[0], array[1], array[2]);
        }
    }
}
