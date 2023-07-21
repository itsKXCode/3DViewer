using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Kitware.VTK;

namespace _3DViewer.Utilities
{
    public static class VTKHelper
    {
        /// <summary>
        /// Creates a Sphere for VTK
        /// </summary>
        /// <param name="position">The Position where the sphere will be Located at</param>
        /// <param name="radius">The Radius of the Sphere</param>
        /// <param name="color">The Color of the Sphere</param>
        /// <returns></returns>
        public static vtkActor CreateSphere(Vector3D position, double radius, Color color = new Color())
        {
            vtkSphereSource sphere = vtkSphereSource.New();
            sphere.SetCenter(0, 0, 0);
            sphere.SetRadius(radius);
            sphere.SetPhiResolution(50);
            sphere.SetThetaResolution(50);

            vtkDataSetMapper SphereMapper = vtkDataSetMapper.New();
            SphereMapper.SetInputConnection(sphere.GetOutputPort());

            vtkActor sphereActor = vtkActor.New();
            sphereActor.SetMapper(SphereMapper);
            sphereActor.GetProperty().SetOpacity(0.9);
            sphereActor.GetProperty().SetDiffuseColor(color.R, color.G, color.B);
            sphereActor.SetPosition(position.X, position.Y, position.Z);

            return sphereActor;
        }

        /// <summary>
        /// Creates a 3D Line for VTK
        /// </summary>
        /// <param name="position1">The beginning of the Line</param>
        /// <param name="position2">The end of the Line</param>
        /// <returns></returns>
        public static vtkActor Create3DLine(Vector3D position1, Vector3D position2)
        {
            vtkLineSource line = vtkLineSource.New();

            line.SetPoint1(position1.X, position1.Y, position1.Z);
            line.SetPoint2(position2.X, position2.Y, position2.Z);
            line.Update();

            vtkDataSetMapper lineMapper = vtkDataSetMapper.New();
            lineMapper.SetInputConnection(line.GetOutputPort());

            vtkActor LineActor = vtkActor.New();

            LineActor.SetMapper(lineMapper);

            return LineActor;

        }

        public static vtkActor CreateCut(Vector3D direction, Vector3D position, vtkActor actor)
        {
            vtkPlane plane = vtkPlane.New();
            plane.SetOrigin(position.X, position.Y, position.Z);
            plane.SetNormal(direction.X, direction.Y, direction.Z);

            vtkPolyData polyDataCopy = vtkPolyData.New();
            polyDataCopy.DeepCopy(actor.GetMapper().GetInput());

            //Apply the Current Transform to the Polydata
            vtkTransformPolyDataFilter transformation = vtkTransformPolyDataFilter.New();
            transformation.SetInputData(polyDataCopy);
            transformation.SetTransform(actor.GetUserTransform());
            transformation.Update();

            vtkCutter cutter = vtkCutter.New();
            cutter.SetCutFunction(plane);
            cutter.SetInputData(transformation.GetOutput());
            cutter.Update();
            
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputData(cutter.GetOutput());

            vtkActor crossSectionActor = vtkActor.New();
            crossSectionActor.GetProperty().SetColor(1.0, 0.5, 0);
            crossSectionActor.GetProperty().SetLineWidth(2);
            crossSectionActor.SetMapper(mapper);

            return crossSectionActor;
        }

        public static Vector3D CalculateNormalVector(Vector3D point1, Vector3D point2, Vector3D point3)
        {
            Vector3D retValue = Vector3D.CrossProduct((point3 - point2), (point1 - point2));
            retValue.Normalize();
            return retValue;
        }

        public static IntPtr CreateIntPtrFromArray(double[] obj)
        {
            System.IntPtr returnValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * obj.Length);
            Marshal.Copy(obj, 0, returnValue, obj.Length);
            return returnValue;
        }
    }
}
