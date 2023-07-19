using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DViewer.Utilities;
using Kitware.VTK;

namespace _3DViewer.Interactors
{
    /// <summary>
    /// Enables Measurement between two Point which can be set anywhere on a Actor
    /// </summary>
    public class PointMeasureInteractor : BaseInteractor
    {
        private vtkCellPicker _picker = vtkCellPicker.New();

        private vtkActor _sphere1 = null;
        private vtkActor _sphere2 = null;
        private vtkActor _line = null;

        private vtkActor _sphereToMove = null;

        private Color _sphereColor = new Color() { R = 255, G = 150, B = 0 };

        public PointMeasureInteractor() : base()
        {
            _picker.SetTolerance(0.0005);
        }

        protected override void BaseLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            _sphereToMove = null;

            var mainRenderer = GetMainRenderer();
            var secondRenderer = GetSecondRenderer();

            //2D Position
            int[] eventPosition = this.GetInteractor().GetEventPosition();
            _picker.Pick(eventPosition[0], eventPosition[1], 0, mainRenderer);

            //3D Position
            double[] worldPosition = _picker.GetPickPosition();

            if (_picker.GetCellId() > -1)
            {
                //If Sphere1 is null, Measurement has not begun yet
                if (_sphere1 == null)
                {
                    _sphere1 = VTKHelper.CreateSphere(new Vector3D(worldPosition[0], worldPosition[1], worldPosition[2]),0.04, _sphereColor);

                    secondRenderer.AddActor(_sphere1);
                    base.GetInteractor().GetRenderWindow().Render();
                }
                else if(_sphere2 == null)
                {
                    //First sphere has been set but Second sphere is null, now set the second
                    _sphere2 = VTKHelper.CreateSphere(new Vector3D(worldPosition[0], worldPosition[1], worldPosition[2]), 0.04, _sphereColor);

                    var center1 = new Vector3D(_sphere1.GetCenter()[0], _sphere1.GetCenter()[1], _sphere1.GetCenter()[2]);
                    var center2 = new Vector3D(_sphere2.GetCenter()[0], _sphere2.GetCenter()[1], _sphere2.GetCenter()[2]);
                    var distance = (center2 - center1).Length;

                    _line = VTKHelper.Create3DLine(center1, center2);

                    secondRenderer.AddActor(_sphere2);
                    secondRenderer.AddActor(_line);
                    base.GetInteractor().GetRenderWindow().Render();
                }
            }

            if (_sphere1 != null && _sphere2 != null)
            {
                //Bot spheres are set
                _picker.Pick(eventPosition[0], eventPosition[1], 0, secondRenderer);
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
            //Enables Drag and Drop the Measurement Spheres

            var mainRenderer = GetMainRenderer();
            var secondRenderer = GetSecondRenderer();

            if (_sphere1 != null && _sphere2 != null && _sphereToMove != null)
            {
                //Check if Left Mouse button is currently down
                if (Interop.GetAsyncKeyState(0x01) != 0)
                {
                    int[] eventPosition = this.GetInteractor().GetEventPosition();
                    _picker.Pick(eventPosition[0], eventPosition[1], 0, mainRenderer);

                    //Check if the Pick is on an Actor, otherwise the Sphere would be in the Middle of the Air
                    if (_picker.GetActor() != null)
                    {
                        double[] worldPosition = _picker.GetPickPosition();
                        _sphereToMove.SetPosition(worldPosition[0], worldPosition[1], worldPosition[2]);

                        //Downcast the Line Actor to its Original so we can manipulate it
                        vtkAlgorithm algorithm = _line.GetMapper().GetInputConnection(0, 0).GetProducer();
                        vtkLineSource lineSource = vtkLineSource.SafeDownCast(algorithm);

                        lineSource.SetPoint1(_sphere1.GetPosition()[0], _sphere1.GetPosition()[1], _sphere1.GetPosition()[2]);
                        lineSource.SetPoint2(_sphere2.GetPosition()[0], _sphere2.GetPosition()[1], _sphere2.GetPosition()[2]);

                        base.GetInteractor().GetRenderWindow().Render();
                    }
                }
            }

            base.BaseMouseMove(sender, e);
        }

        private vtkRenderer GetMainRenderer()
        {
            return base.GetInteractor().GetRenderWindow().GetRenderers().GetFirstRenderer();
        }

        private vtkRenderer GetSecondRenderer()
        {
            base.GetInteractor().GetRenderWindow().GetRenderers().InitTraversal();
            base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
            return base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
        }
    }
}
