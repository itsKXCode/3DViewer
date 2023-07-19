using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Kitware.VTK;

namespace _3DViewer
{
    public static class VTKHelper
    {
        public static vtkActor CreateSphere(Vector3D position, double radius, Color color = new Color())
        {
            //Erstellt eine Sphere und gibt den Actor zurück
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

        public static vtkActor Create3DLine(Vector3D position1, Vector3D position2)
        {
            //Erstellt eine 3D Line und gibt den Actor zurück
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
    }
}
