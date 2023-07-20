﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using _3DViewer.Utilities;
using Kitware.VTK;

namespace _3DViewer.Interactors
{
    public class CrossSectionInteractorStyle : BaseInteractorStyle
    {
        private vtkCellPicker _picker = vtkCellPicker.New();

        private Vector3D? _crossSectionStartPosition = null;
        private Vector3D? _crossSectionEndPosition = null;



        private vtkActor _crossSectionLineActor = null;
        private vtkLineSource _crossSectionLineSource = null;

        public CrossSectionInteractorStyle()
        {
            _picker.SetTolerance(0.0005);
        }

        public override void Clear()
        {
            GetSecondRenderer().RemoveAllViewProps();
            SetMainActorsOpacity(1);
            GetMainRenderWindow().Render();
            base.Clear();
        }

        protected override void BaseLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            int[] pos = this.GetInteractor().GetEventPosition();

            _picker.Pick(pos[0], pos[1], 0, base.GetMainRenderer());

            var currentPosition = Vector3DExtension.FromDouble3Array(_picker.GetPickPosition());

            if (_crossSectionStartPosition == null)
            {
                _crossSectionStartPosition = currentPosition;

                _crossSectionLineActor = VTKHelper.Create3DLine(_crossSectionStartPosition.Value, _crossSectionStartPosition.Value);

                _crossSectionLineSource = vtkLineSource.SafeDownCast(_crossSectionLineActor.GetMapper().GetInputConnection(0, 0).GetProducer());

                _crossSectionLineActor.GetProperty().SetLineWidth(2);
                _crossSectionLineActor.GetProperty().SetColor(1, 0.5, 0);
                GetSecondRenderer().AddActor(_crossSectionLineActor);
            }

            base.BaseLeftButtonDown(sender, e);
        }

        protected override void BaseMouseMove(vtkObject sender, vtkObjectEventArgs e)
        {
            //Check if CrossSection is Currently in work
            if (_crossSectionStartPosition != null & _crossSectionEndPosition == null)
            {
                int[] pos = this.GetInteractor().GetEventPosition();

                _picker.Pick(pos[0], pos[1], 0, base.GetMainRenderer());

                var currentPosition = Vector3DExtension.FromDouble3Array(_picker.GetPickPosition());

                _crossSectionLineSource.SetPoint2(currentPosition.X,currentPosition.Y,currentPosition.Z);
                _crossSectionLineSource.Update();

                GetMainRenderWindow().Render();
            }

            base.BaseMouseMove(sender, e);
        }

        protected override void BaseLeftButtonUp(vtkObject sender, vtkObjectEventArgs e)
        {
            //Check if CrossSection selection is finished
            if (_crossSectionStartPosition != null && _crossSectionEndPosition == null)
            {
                int[] pos = this.GetInteractor().GetEventPosition();

                _picker.Pick(pos[0], pos[1], 0, base.GetMainRenderer());

                _crossSectionEndPosition = Vector3DExtension.FromDouble3Array(_picker.GetPickPosition());

                if (_crossSectionEndPosition != _crossSectionStartPosition)
                {
                    var camPosition = Vector3DExtension.FromDouble3Array(GetMainRenderer().GetActiveCamera().GetPosition());
                    var normal = VTKHelper.CalculateNormalVector(_crossSectionStartPosition.Value, _crossSectionEndPosition.Value, camPosition);

                    var actor = VTKHelper.CreateCut(normal, _crossSectionStartPosition.Value, GetMainRenderer().GetActors().GetLastActor());

                    GetSecondRenderer().RemoveAllViewProps();
                    GetSecondRenderer().AddActor(actor);
                    SetMainActorsOpacity(0.05);
                    GetMainRenderWindow().Render();
                }
                else
                {
                    _crossSectionStartPosition = null;
                    _crossSectionEndPosition = null;
                }
            }

            base.BaseLeftButtonUp(sender, e);
        }

        /// <summary>
        /// Sets all Main Actors to an Opacity
        /// </summary>
        private void SetMainActorsOpacity(double Opacity)
        {
            var actors = GetMainRenderer().GetActors();
            actors.InitTraversal();

            for (int i = 0; i < actors.GetNumberOfItems(); i++)
            {
                var curActor = actors.GetNextItem();
                curActor.GetProperty().SetOpacity(Opacity);
            }
        }
    }
}
