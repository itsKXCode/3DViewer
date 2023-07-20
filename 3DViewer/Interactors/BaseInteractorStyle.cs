using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace _3DViewer.Interactors
{
    /// <summary>
    /// The Base Interactor every other interactor should inherit from
    /// </summary>
    public class BaseInteractorStyle : vtkInteractorStyleTrackballCamera
    {
        public BaseInteractorStyle() : base()
        {
            SetEventHandlers();
            this.SetMotionFactor(30);
            this.SetMouseWheelMotionFactor(0.5);
        }
        protected vtkRenderer GetMainRenderer()
        {
            return base.GetInteractor().GetRenderWindow().GetRenderers().GetFirstRenderer();
        }

        protected vtkRenderer GetSecondRenderer()
        {
            base.GetInteractor().GetRenderWindow().GetRenderers().InitTraversal();
            base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
            return base.GetInteractor().GetRenderWindow().GetRenderers().GetNextItem();
        }

        protected vtkRenderWindow GetMainRenderWindow()
        {
            return base.GetInteractor().GetRenderWindow();
        }

        public virtual void Clear() { }

        private void SetEventHandlers()
        {
            //Set Custom Eventhandlers so we can override the Default behaviour of the TrackballCamera und use Function overriding in Child classes to manipulate
            this.RightButtonPressEvt += new vtkObjectEventHandler(BaseRightButtonDown);
            this.RightButtonReleaseEvt += new vtkObjectEventHandler(BaseRightButtonUP);

            this.MouseMoveEvt += new vtkObjectEventHandler(BaseMouseMove);
            this.MiddleButtonPressEvt += new vtkObjectEventHandler(BaseMouseButtonDown);
            this.MiddleButtonReleaseEvt += new vtkObjectEventHandler(BaseMouseButtonUp);

            this.LeftButtonPressEvt += new vtkObjectEventHandler(BaseLeftButtonDown);
            this.LeftButtonReleaseEvt += new vtkObjectEventHandler(BaseLeftButtonUp);

            this.MouseWheelForwardEvt += new vtkObjectEventHandler(BaseMouseWheelForward);
            this.MouseWheelBackwardEvt += new vtkObjectEventHandler(BaseMouseWheelBackward);

            this.KeyPressEvt += new vtkObjectEventHandler(BaseKeyPress);
        }

        protected virtual void BaseMouseButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnMiddleButtonDown();
        }
        protected virtual void BaseMouseButtonUp(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnMiddleButtonUp();
        }

        protected virtual void BaseLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            //Dont do anything on Default on Left Click
        }
        protected virtual void BaseLeftButtonUp(vtkObject sender, vtkObjectEventArgs e)
        {
            //Dont do anything on Default on Left Click
        }

        protected virtual void BaseMouseMove(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnMouseMove();
        }
        protected virtual void BaseRightButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            //Originally Left Mouse Button is used to rotate the Object, Employees are used to use the Right Mouse Button
            base.OnLeftButtonDown();
        }
        protected virtual void BaseRightButtonUP(vtkObject sender, vtkObjectEventArgs e)
        {
            //Originally Left Mouse Button is used to rotate the Object, Employees are used to use the Right Mouse Button
            base.OnLeftButtonUp();
        }

        protected virtual void BaseKeyPress(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnKeyPress();
        }
        protected virtual void BaseMouseWheelForward(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnMouseWheelForward();
        }
        protected virtual void BaseMouseWheelBackward(vtkObject sender, vtkObjectEventArgs e)
        {
            base.OnMouseWheelBackward();
        }
    }
}
