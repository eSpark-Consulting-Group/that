
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace DDT.Application.Transitions
{
    public enum Direction
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public class CubeTransition : Transition
    {
        private readonly Model3DGroup _cubeModelContainer;
        private readonly Model3DGroup _rootModel;
        private readonly Viewport3D _viewport;
        private Direction _direction = Direction.TopToBottom;

        public CubeTransition()
        {
            _viewport = TransitionResources["3DCube"] as Viewport3D;

            _rootModel = (_viewport.Children[0] as ModelVisual3D).Content as Model3DGroup;
            _cubeModelContainer = _rootModel.Children[1] as Model3DGroup;

        }

        public Direction Rotation
        {
            get { return _direction; }
            set { _direction = value; }
        }

        private void RegisterNameScope()
        {
            Transform3DGroup groupTrans = _rootModel.Transform as Transform3DGroup;
            AxisAngleRotation3D rotator = (groupTrans.Children[0] as RotateTransform3D).Rotation as
                                          AxisAngleRotation3D;
            TranslateTransform3D translator = groupTrans.Children[1] as TranslateTransform3D;

            NameScope scope = GetNameScope();
            scope.RegisterName("XFORM_Rotate", rotator);
            scope.RegisterName("XFORM_Translate", translator);
        }

        public override void Setup(Brush prevBrush, Brush nextBrush)
        {
            RegisterNameScope();

            Owner.AddTransitionElement(_viewport);

            AdjustViewport(prevBrush, nextBrush);
        }

        private void AdjustViewport(Brush prevBrush, Brush nextBrush)
        {
            double aspect = Owner.ActualWidth / Owner.ActualHeight;

            PrepareCubeFaces(aspect, prevBrush, nextBrush);
            AdjustCamera(aspect);

            AdjustTransforms(aspect);
        }

        private void PrepareCubeFaces(double aspect, Brush prevBrush, Brush nextBrush)
        {
            GeometryModel3D face1 = PrepareCubeFace(CubeFaceType.Front, prevBrush, aspect);

            GeometryModel3D face2 = null;
            switch (Rotation)
            {
                case Direction.LeftToRight:
                    face2 = PrepareCubeFace(CubeFaceType.Left, nextBrush, aspect);
                    break;
                case Direction.RightToLeft:
                    face2 = PrepareCubeFace(CubeFaceType.Right, nextBrush, aspect);
                    break;
                case Direction.TopToBottom:
                    face2 = PrepareCubeFace(CubeFaceType.Top, nextBrush, aspect);
                    break;
                case Direction.BottomToTop:
                    face2 = PrepareCubeFace(CubeFaceType.Bottom, nextBrush, aspect);
                    break;
            }

            _cubeModelContainer.Children.Add(face1);
            _cubeModelContainer.Children.Add(face2);
        }

        private void AdjustTransforms(double aspect)
        {
            RotateTransform3D rotateXForm =
                (_rootModel.Transform as Transform3DGroup).Children[0] as RotateTransform3D;
            TranslateTransform3D translateXForm =
                (_rootModel.Transform as Transform3DGroup).Children[1] as TranslateTransform3D;

            Storyboard animator = _viewport.Resources["STORYBOARD_CubeAnimator"] as Storyboard;
            DoubleAnimationUsingKeyFrames rotateAnim = animator.Children[0] as DoubleAnimationUsingKeyFrames;
            DoubleAnimationUsingKeyFrames translateAnim =
                animator.Children[1] as DoubleAnimationUsingKeyFrames;
            rotateAnim.Duration = Duration;
            translateAnim.Duration = Duration;

            switch (Rotation)
            {
                case Direction.LeftToRight:
                    rotateAnim.KeyFrames[1].Value = 90;
                    rotateXForm.CenterX = -aspect / 2;
                    (rotateXForm.Rotation as AxisAngleRotation3D).Axis = new Vector3D(0, 1, 0);

                    translateXForm.OffsetY = 0;
                    Storyboard.SetTargetProperty(translateAnim, new PropertyPath(TranslateTransform3D.OffsetXProperty));
                    translateAnim.KeyFrames[1].Value = aspect;
                    break;

                case Direction.RightToLeft:
                    rotateAnim.KeyFrames[1].Value = -90;
                    rotateXForm.CenterX = aspect / 2;
                    (rotateXForm.Rotation as AxisAngleRotation3D).Axis = new Vector3D(0, 1, 0);

                    Storyboard.SetTargetProperty(translateAnim, new PropertyPath(TranslateTransform3D.OffsetXProperty));
                    translateAnim.KeyFrames[1].Value = -aspect;
                    break;

                case Direction.TopToBottom:
                    rotateAnim.KeyFrames[1].Value = 90;
                    rotateXForm.CenterY = 0.5;
                    (rotateXForm.Rotation as AxisAngleRotation3D).Axis = new Vector3D(1, 0, 0);

                    Storyboard.SetTargetProperty(translateAnim, new PropertyPath(TranslateTransform3D.OffsetYProperty));
                    translateAnim.KeyFrames[1].Value = -1;
                    break;

                case Direction.BottomToTop:
                    rotateAnim.KeyFrames[1].Value = -90;
                    rotateXForm.CenterY = -0.5;
                    (rotateXForm.Rotation as AxisAngleRotation3D).Axis = new Vector3D(1, 0, 0);

                    Storyboard.SetTargetProperty(translateAnim, new PropertyPath(TranslateTransform3D.OffsetYProperty));
                    translateAnim.KeyFrames[1].Value = 1;
                    break;
            }
        }

        private void AdjustCamera(double aspect)
        {
            PerspectiveCamera camera = _viewport.Camera as PerspectiveCamera;
            double angle = camera.FieldOfView / 2;
            double cameraZPos = (aspect / 2) / Math.Tan(angle * Math.PI / 180);
            camera.Position = new Point3D(0, 0, cameraZPos);
        }

        private GeometryModel3D PrepareCubeFace(CubeFaceType face, Brush brush, double aspect)
        {
            MeshGeometry3D mesh = (_viewport.Resources["CubeFace"] as MeshGeometry3D).Clone();
            switch (face)
            {
                case CubeFaceType.Left:
                    mesh.Positions[0] = new Point3D(-aspect / 2, 0.5, -aspect);
                    mesh.Positions[1] = new Point3D(-aspect / 2, 0.5, 0);
                    mesh.Positions[2] = new Point3D(-aspect / 2, -0.5, 0);
                    mesh.Positions[3] = new Point3D(-aspect / 2, -0.5, -aspect);
                    break;
                case CubeFaceType.Right:
                    mesh.Positions[0] = new Point3D(aspect / 2, 0.5, 0);
                    mesh.Positions[1] = new Point3D(aspect / 2, 0.5, -aspect);
                    mesh.Positions[2] = new Point3D(aspect / 2, -0.5, -aspect);
                    mesh.Positions[3] = new Point3D(aspect / 2, -0.5, 0);
                    break;
                case CubeFaceType.Top:
                    mesh.Positions[0] = new Point3D(-aspect / 2, 0.5, -1);
                    mesh.Positions[1] = new Point3D(aspect / 2, 0.5, -1);
                    mesh.Positions[2] = new Point3D(aspect / 2, 0.5, 0);
                    mesh.Positions[3] = new Point3D(-aspect / 2, 0.5, 0);
                    break;
                case CubeFaceType.Bottom:
                    mesh.Positions[0] = new Point3D(-aspect / 2, -0.5, 0);
                    mesh.Positions[1] = new Point3D(aspect / 2, -0.5, 0);
                    mesh.Positions[2] = new Point3D(aspect / 2, -0.5, -1);
                    mesh.Positions[3] = new Point3D(-aspect / 2, -0.5, -1);
                    break;
                case CubeFaceType.Front:
                    mesh.Positions[0] = new Point3D(-aspect / 2, 0.5, 0);
                    mesh.Positions[1] = new Point3D(aspect / 2, 0.5, 0);
                    mesh.Positions[2] = new Point3D(aspect / 2, -0.5, 0);
                    mesh.Positions[3] = new Point3D(-aspect / 2, -0.5, 0);
                    break;
            }

            DiffuseMaterial material = new DiffuseMaterial(brush);

            GeometryModel3D model = new GeometryModel3D(mesh, material);
            return model;
        }

        public override Storyboard PrepareStoryboard()
        {
            Storyboard animator = _viewport.Resources["STORYBOARD_CubeAnimator"] as Storyboard;
            return animator;
        }

        public override void Cleanup()
        {
            _cubeModelContainer.Children.Clear();
        }

        private enum CubeFaceType
        {
            Left,
            Right,
            Top,
            Bottom,
            Front
        }
    }
}