using System;
using System.Windows;
using _3DViewer.ViewModels;
using Kitware.VTK;

namespace _3DViewer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public bool test;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void MainRenderControl_Load(object sender, EventArgs e)
        {
            vtkObject.GlobalWarningDisplayOff();
            _viewModel.SetRenderWindow(MainRenderControl.RenderWindow);
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            //Open File Dialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "stl, obj, ply, vtp (*.stl, *.obj, *.ply, *.vtp)| *.stl; *.obj; *.ply; *.vtp"; 

            var result = dialog.ShowDialog();

            if (result == true)
                _viewModel.ImportFile(dialog.FileName);
        }

        private void Measure_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SetInteractionMode(MainWindowViewModel.InteractionMode.PointMeasure);
        }

        private void CrossSection_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SetInteractionMode(MainWindowViewModel.InteractionMode.CrossSection);
        }
    }
}
