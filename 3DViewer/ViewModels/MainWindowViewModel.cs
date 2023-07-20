using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using _3DViewer.Interactors;
using _3DViewer.Utilities;
using Kitware.VTK;

namespace _3DViewer.ViewModels
{
    public class MainWindowViewModel
    {
        public enum Modes
        {
            Default,
            PointMeasure
        }

        private vtkRenderWindow _renderWindow;
        private vtkRenderer _mainRenderer { get => _renderWindow.GetRenderers().GetFirstRenderer(); }
        private vtkRenderer _secondRenderer = vtkRenderer.New();
        private vtkRenderer _thirdRenderer = vtkRenderer.New();

        //We need to store the reference to the Interactor style, otherwise the GarbageCollector would collect internal events and VTK throws a Exception afterwards
        private BaseInteractorStyle _interactorStyle;
        private vtkRenderWindowInteractor _interactor = vtkRenderWindowInteractor.New();


        public MainWindowViewModel(vtkRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            InitializeRenderWindow();
        }

        public void SetMode(Modes mode)
        {
            _interactorStyle?.Clear();

            switch (mode)
            {
                case Modes.Default:
                    _interactorStyle = new BaseInteractorStyle();
                    break;
                case Modes.PointMeasure:
                    _interactorStyle = new PointMeasureInteractorStyle();
                    break;
                default:
                    throw new NotImplementedException();
            }

            _interactor.SetInteractorStyle(_interactorStyle);
            _renderWindow.SetInteractor(_interactor);

            _interactorStyle.SetDefaultRenderer(_mainRenderer);
        }

        public void ImportFile(string filePath)
        {
            vtkPolyData polydata = null;

            try
            {
                polydata = ModelImporter.ImportFile(filePath);
            }
            catch (NotImplementedException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            //Convert Polydata to Triangles
            vtkTriangleFilter triangle = vtkTriangleFilter.New();
            triangle.SetInputData(polydata);
            triangle.Update();

            //Removes duplicated points
            vtkCleanPolyData cleaner = vtkCleanPolyData.New();
            cleaner.SetInputData(triangle.GetOutput());
            cleaner.Update();

            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputData(cleaner.GetOutput());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0.9, 0.9, 0.9);
            actor.GetProperty().SetSpecular(0.3);
            actor.GetProperty().SetSpecularColor(1, 1, 1);
            actor.GetProperty().SetSpecularPower(30);
            actor.GetProperty().BackfaceCullingOn();

            CenterActor(actor);

            _mainRenderer.AddActor(actor);

            _renderWindow.Render();
        }

        private void CenterActor(vtkActor actor)
        {
            var transform = vtkTransform.New();

            transform.Translate(-actor.GetCenter()[0],
                       -actor.GetCenter()[1],
                       -actor.GetCenter()[2]);
            actor.SetUserTransform(transform);
        }

        private void InitializeRenderWindow()
        {
            SetupRenderer();
            SetupCamera();
            SetupLight();
            SetMode(Modes.Default);

            _renderWindow.Render();
        }

        private void SetupRenderer()
        {
            _renderWindow.SetMultiSamples(0);
            _mainRenderer.SetBackground(0.2, 0.2, 0.2);

            //Initialize the Second and third renderer
            _renderWindow.SetNumberOfLayers(2);

            _secondRenderer = vtkRenderer.New();
            _thirdRenderer = vtkRenderer.New();

            _secondRenderer.SetLayer(1);
            _thirdRenderer.SetLayer(1);
            _mainRenderer.SetLayer(0);

            _renderWindow.AddRenderer(_secondRenderer);
        }

        private void SetupCamera()
        {
            var camera = vtkCamera.New();
            camera.SetClippingRange(0.1, 100);
            camera.SetFocalPoint(0, 0, 0);
            camera.SetPosition(0, 0, 100);
            camera.SetThickness(1000);

            //camera.SetViewUp(0, 1, 0);
            _mainRenderer.SetActiveCamera(camera);
            _secondRenderer.SetActiveCamera(camera);
        }

        private void SetupLight()
        {
            var light = vtkLight.New();
            light.SetPosition(0, 0, 100);

            light.SetConeAngle(10);
            light.SetFocalPoint(0, 0, 0);
            light.SetDiffuseColor(1, 1, 1);
            light.SetAmbientColor(1, 1, 1);
            light.SetSpecularColor(1, 1, 1);
            light.SetLightTypeToCameraLight();

            _mainRenderer.AddLight(light);
            _secondRenderer.AddLight(light);
        }
    }
}
