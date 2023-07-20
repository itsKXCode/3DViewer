using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DViewer.Utilities;
using Kitware.VTK;
using static System.Net.Mime.MediaTypeNames;

namespace _3DViewer.Interactors
{
    /// <summary>
    /// Enables Measurement between two Point which can be set anywhere on a Actor
    /// </summary>
    public class PointMeasureInteractorStyle : BaseInteractorStyle
    {
        private vtkCellPicker _picker = vtkCellPicker.New();

        private vtkActor _sphere1 = null;
        private vtkActor _sphere2 = null;
        private vtkActor _line = null;

        private vtkTextActor _distanceText = null;
        private vtkTextWidget _distanceTextWidget = null;

        private vtkActor _sphereToMove = null;

        private Color _sphereColor = new Color() { R = 255, G = 150, B = 0 };

        public PointMeasureInteractorStyle() : base()
        {
            _picker.SetTolerance(0.0005);
        }

        public override void Clear()
        {
            GetSecondRenderer().RemoveAllViewProps();
            GetMainRenderWindow().Render();
            base.Clear();
        }

        /// <summary>
        /// Creates Measurement Points an displays them as a Sphere
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void BaseLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            _sphereToMove = null;

            var mainRenderer = GetMainRenderer();
            var secondRenderer = GetSecondRenderer();

            //2D Position
            int[] eventPosition = base.GetInteractor().GetEventPosition();
            _picker.Pick(eventPosition[0], eventPosition[1], 0, mainRenderer);

            //3D Position
            var worldPosition = Vector3DExtension.FromDouble3Array(_picker.GetPickPosition());

            if (_picker.GetCellId() > -1)
            {
                //If Sphere1 is null, Measurement has not begun yet
                if (_sphere1 == null)
                {
                    _sphere1 = VTKHelper.CreateSphere(worldPosition,0.04, _sphereColor);

                    secondRenderer.AddActor(_sphere1);
                    base.GetInteractor().GetRenderWindow().Render();
                }
                else if(_sphere2 == null)
                {
                    //First sphere has been set but Second sphere is null, now set the second
                    _sphere2 = VTKHelper.CreateSphere(worldPosition, 0.04, _sphereColor);

                    var center1 = new Vector3D(_sphere1.GetCenter()[0], _sphere1.GetCenter()[1], _sphere1.GetCenter()[2]);
                    var center2 = new Vector3D(_sphere2.GetCenter()[0], _sphere2.GetCenter()[1], _sphere2.GetCenter()[2]);
                    
                    SetDistanceText((center2 - center1).Length);

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

        /// <summary>
        /// Moves a previous selected measurement Sphere
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    int[] eventPosition = base.GetInteractor().GetEventPosition();
                    _picker.Pick(eventPosition[0], eventPosition[1], 0, mainRenderer);

                    //Check if the Pick is on an Actor, otherwise the Sphere would be in the Middle of the Air
                    if (_picker.GetActor() != null)
                    {
                        var worldPosition = Vector3DExtension.FromDouble3Array(_picker.GetPickPosition());
                        _sphereToMove.SetPosition(worldPosition.X, worldPosition.Y, worldPosition.Z);

                        var pos1 = new Vector3D(_sphere1.GetPosition()[0], _sphere1.GetPosition()[1], _sphere1.GetPosition()[2]);
                        var pos2 = new Vector3D(_sphere2.GetPosition()[0], _sphere2.GetPosition()[1], _sphere2.GetPosition()[2]);

                        //Downcast the Line Actor to its Original so we can manipulate it
                        vtkAlgorithm algorithm = _line.GetMapper().GetInputConnection(0, 0).GetProducer();
                        vtkLineSource lineSource = vtkLineSource.SafeDownCast(algorithm);

                        lineSource.SetPoint1(pos1.X, pos1.Y, pos1.Z);
                        lineSource.SetPoint2(pos2.X, pos2.Y, pos2.Z);
                        
                        SetDistanceText((pos2 - pos1).Length);

                        base.GetInteractor().GetRenderWindow().Render();
                    }
                }
            }

            base.BaseMouseMove(sender, e);
        }

        protected override void BaseLeftButtonUp(vtkObject sender, vtkObjectEventArgs e)
        {
            _sphereToMove = null;
            base.BaseLeftButtonUp(sender, e);
        }

        /// <summary>
        /// Displays the Distance
        /// </summary>
        /// <param name="distance"></param>
        private void SetDistanceText(double distance)
        {
            if (_distanceText == null)
            {
                _distanceText = vtkTextActor.New();
                vtkTextRepresentation textRepresentation = vtkTextRepresentation.New();
                textRepresentation.GetPositionCoordinate().SetValue(0.15, 0.15);
                textRepresentation.GetPosition2Coordinate().SetValue(0.7, 0.05);

                _distanceTextWidget = vtkTextWidget.New();
                _distanceTextWidget.SetRepresentation(textRepresentation);
                _distanceTextWidget.On();
            }

            _distanceText.SetInput(Math.Round(distance, 2) + "mm");
            _distanceText.GetTextProperty().SetColor(1.0, 1.0, 1.0);
            _distanceTextWidget.SetInteractor(this.GetInteractor());
            _distanceTextWidget.ProcessEventsOff();
            _distanceTextWidget.SetTextActor(_distanceText);
            _distanceTextWidget.On();
        }

    }
}
