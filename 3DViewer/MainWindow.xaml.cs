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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainRenderControl_Load(object sender, EventArgs e)
        {
            vtkObject.GlobalWarningDisplayOff();

            _viewModel = new MainWindowViewModel(MainRenderControl.RenderWindow);
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            //Open File Dialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "stl, obj, ply (*.stl, *.obj, *.ply)| *.stl; *.obj; *.ply"; 

            var result = dialog.ShowDialog();

            if (result == true)
                _viewModel.ImportFile(dialog.FileName);
        }

        private void Measure_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SetMode(MainWindowViewModel.Modes.PointMeasure);
        }
    }
}
