using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using _3DViewer.Interactors;
using _3DViewer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Kitware.VTK;

namespace _3DViewer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public enum InteractionMode
        {
            Default,
            PointMeasure,
            CrossSection
        }

        private vtkRenderWindow _renderWindow;
        private vtkRenderer _mainRenderer;
        private vtkRenderer _secondRenderer = vtkRenderer.New();
        private vtkRenderer _thirdRenderer = vtkRenderer.New();

        //We need to store the reference to the Interactor style, otherwise the GarbageCollector would collect internal events and VTK throws a Exception afterwards
        private BaseInteractorStyle _interactorStyle;
        private vtkRenderWindowInteractor _interactor = vtkRenderWindowInteractor.New();

        private bool _fileLoaded;
        public bool FileLoaded
        {
            get { return _fileLoaded; }
            set { SetProperty(ref _fileLoaded, value); }
        }

        public void SetRenderWindow(vtkRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;
            InitializeRenderWindow();
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            _interactorStyle?.Clear();

            switch (mode)
            {
                case InteractionMode.Default:
                    _interactorStyle = new BaseInteractorStyle();
                    break;
                case InteractionMode.PointMeasure:
                    _interactorStyle = new PointMeasureInteractorStyle();
                    break;
                case InteractionMode.CrossSection:
                    _interactorStyle = new CrossSectionInteractorStyle();
                    break;
                default:
                    throw new NotImplementedException();
            }

            _interactor.SetInteractorStyle(_interactorStyle);
            _renderWindow.SetInteractor(_interactor);

            _interactorStyle.SetMainRenderer(_mainRenderer);
            _interactorStyle.SetSecondRenderer(_secondRenderer);
        }

        public void ImportFile(string filePath)
        {
            InitializeRenderWindow();

            FileLoaded = false;
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

            if (HasScalar(polydata,"Dicke"))
                polydata.GetCellData().SetActiveScalars("Dicke");

            _mainRenderer.AddActor(CreateActorFromPolyData(polydata));
            SetInteractionMode(InteractionMode.Default);

            SetupScalarRenderer(polydata);

            _renderWindow.Render();

            FileLoaded = true;
        }

        private bool HasScalar(vtkPolyData polyData, string Name)
        {
            try
            {
                var nums = polyData.GetCellData().GetScalars(Name).GetNumberOfComponents();

                return nums > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private vtkActor CreateActorFromPolyData(vtkPolyData polydata)
        {
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

            return actor;
        }

        /// <summary>
        /// Enables the Scalars if the PolyData has it (should only happen if it is a intern generated .vtp file)
        /// </summary>
        /// <param name="polyData"></param>
        private void SetupScalarRenderer(vtkPolyData polyData)
        {
            if (HasScalar(polyData, "Selbstueberschneidung") &&
                HasScalar(polyData, "svMax"))
            {
                _secondRenderer.SetViewport(0, 0, 0.8, 1);
                _mainRenderer.SetViewport(0, 0, 0.8, 1);
                var renderer1 = vtkRenderer.New();
                var renderer2 = vtkRenderer.New();
                renderer1.SetViewport(0.8, 0.5, 1, 1);
                renderer2.SetViewport(0.8, 0.0, 1, 0.5);

                _renderWindow.AddRenderer(renderer1);
                _renderWindow.AddRenderer(renderer2);
                renderer1.SetActiveCamera(_mainRenderer.GetActiveCamera());
                renderer2.SetActiveCamera(_mainRenderer.GetActiveCamera());

                vtkPolyData polyDataCuts = vtkPolyData.New();
                polyDataCuts.DeepCopy(polyData);
                polyDataCuts.GetCellData().SetActiveScalars("Selbstueberschneidung");

                vtkPolyData polyDataSvMax = vtkPolyData.New();
                polyDataSvMax.DeepCopy(polyData);
                polyDataSvMax.GetCellData().SetActiveScalars("svMax");

                renderer1.AddActor(CreateActorFromPolyData(polyDataCuts));

                renderer2.AddActor(CreateActorFromPolyData(polyDataSvMax));
            }
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
            SetInteractionMode(InteractionMode.Default);

            _renderWindow.Render();
        }

        private void SetupRenderer()
        {
            RemoveAllRenderers();

            _mainRenderer = vtkRenderer.New();
            _secondRenderer = vtkRenderer.New();
            _thirdRenderer = vtkRenderer.New();

            _renderWindow.SetMultiSamples(0);
            _mainRenderer.SetBackground(0.2, 0.2, 0.2);

            //Initialize the Second and third renderer
            _renderWindow.SetNumberOfLayers(2);

            _secondRenderer.SetBackground(0.2, 0.2, 0.2);

            _secondRenderer.SetLayer(1);
            _thirdRenderer.SetLayer(1);
            _mainRenderer.SetLayer(0);

            _renderWindow.AddRenderer(_secondRenderer);
            _renderWindow.AddRenderer(_mainRenderer);
        }

        private void RemoveAllRenderers()
        {
            var renderCollection = _renderWindow.GetRenderers();
            renderCollection.InitTraversal();

            for (int i = 0; i < renderCollection.GetNumberOfItems(); i++)
                _renderWindow.RemoveRenderer(renderCollection.GetNextItem());
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
