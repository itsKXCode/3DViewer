using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Kitware.VTK;

namespace _3DViewer.Interactors
{
    public class PointMeasureInteractor : BaseInteractor
    {
        private vtkCellPicker _picker = vtkCellPicker.New();

        private vtkActor _sphere1 = null;
        private vtkActor _sphere2 = null;
        private vtkActor _line = null;

        private vtkActor _sphereToMove = null;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);

        public PointMeasureInteractor() : base()
        {
            _picker.SetTolerance(0.0005);
        }

        protected override void BaseLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            _sphereToMove = null;

            int[] pos = this.GetInteractor().GetEventPosition();

            var mainRenderer = base.GetInteractor().GetRenderWindow().GetRenderers().GetFirstRenderer();
            base.GetInteractor().GetRenderWindow().GetRenderers().InitTraversal();
            base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
            var topRenderer = base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();

            _picker.Pick(pos[0], pos[1], 0, mainRenderer);

            //3D Position holen
            double[] worldPosition = _picker.GetPickPosition();

            if (_picker.GetCellId() > -1)
            {
                //If Sphere1 is null, Measurement has not begun yet
                if (_sphere1 == null)
                {
                    _sphere1 = VTKHelper.CreateSphere(new Vector3D(worldPosition[0], worldPosition[1], worldPosition[2]),0.04,new Color() {R = 255, G = 150, B = 0 });

                    topRenderer.AddActor(_sphere1);
                    base.GetInteractor().GetRenderWindow().Render();
                }
                else if(_sphere2 == null)
                {
                    //First sphere has been set but Second sphere is null, now set the second
                    _sphere2 = VTKHelper.CreateSphere(new Vector3D(worldPosition[0], worldPosition[1], worldPosition[2]), 0.04, new Color() { R = 255, G = 150, B = 0 });

                    var center1 = new Vector3D(_sphere1.GetCenter()[0], _sphere1.GetCenter()[1], _sphere1.GetCenter()[2]);
                    var center2 = new Vector3D(_sphere2.GetCenter()[0], _sphere2.GetCenter()[1], _sphere2.GetCenter()[2]);
                    var distance = (center2 - center1).Length;

                    _line = VTKHelper.Create3DLine(center1, center2);

                    topRenderer.AddActor(_sphere2);
                    topRenderer.AddActor(_line);
                    base.GetInteractor().GetRenderWindow().Render();
                }
            }

            if (_sphere1 != null && _sphere2 != null)
            {
                //Bot spheres are set
                _picker.Pick(pos[0], pos[1], 0, topRenderer);
                _sphereToMove = _picker.GetActor();
            }

            base.BaseLeftButtonDown(sender, e);
        }

        public override void OnLeftButtonUp()
        {
            _sphereToMove = null;
            base.OnLeftButtonUp();
        }

        protected override void BaseMouseMove(vtkObject sender, vtkObjectEventArgs e)
        {
            var mainRenderer = base.GetInteractor().GetRenderWindow().GetRenderers().GetFirstRenderer();
            base.GetInteractor().GetRenderWindow().GetRenderers().InitTraversal();
            base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
            var topRenderer = base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();

            if (_sphere1 != null && _sphere2 != null && _sphereToMove != null)
            {
                //Check if Left Mouse button is currently down
                if (GetAsyncKeyState(0x01) != 0)
                {
                    int[] tpos = this.GetInteractor().GetEventPosition();
                    _picker.Pick(tpos[0], tpos[1], 0, mainRenderer);

                    vtkActor tAct = _picker.GetActor();

                    double[] worldPosition = _picker.GetPickPosition();

                    if (tAct != null)
                    {
                        _sphereToMove.SetPosition(worldPosition[0], worldPosition[1], worldPosition[2]);

                        vtkAlgorithm algo = _line.GetMapper().GetInputConnection(0, 0).GetProducer();
                        vtkLineSource tLine = vtkLineSource.SafeDownCast(algo);

                        tLine.SetPoint1(_sphere1.GetPosition()[0], _sphere1.GetPosition()[1], _sphere1.GetPosition()[2]);
                        tLine.SetPoint2(_sphere2.GetPosition()[0], _sphere2.GetPosition()[1], _sphere2.GetPosition()[2]);

                        base.GetInteractor().GetRenderWindow().Render();
                    }
                }
            }

            base.BaseMouseMove(sender, e);
        }
    }
}
