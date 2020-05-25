using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RawViewer
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public static readonly DependencyProperty MoveProperty =
            DependencyProperty.Register("Move", typeof(bool), typeof(ZoomBorder),
            new UIPropertyMetadata(true));

        public bool Move
        {
            get { return (bool)base.GetValue(MoveProperty); }
            set { this.SetValue(MoveProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("ZoomProperty", typeof(double), typeof(ZoomBorder),
            new UIPropertyMetadata(1.0));

        public double Zoom
        {
            get { return (double)base.GetValue(ZoomProperty); }
            set
            {
                this.SetValue(ZoomProperty, value);

                var st = GetScaleTransform(child);
                st.ScaleX = value;
                st.ScaleY = value;
            }
        }

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register("OffsetXProperty", typeof(double), typeof(ZoomBorder),
            new UIPropertyMetadata(0.0));

        public double OffsetX
        {
            get { return (double)base.GetValue(OffsetXProperty); }
            set
            {
                this.SetValue(OffsetXProperty, value);

                var tt = GetTranslateTransform(child);
                tt.X = value;
            }
        }

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register("OffsetYProperty", typeof(double), typeof(ZoomBorder),
            new UIPropertyMetadata(0.0));

        public double OffsetY
        {
            get { return (double)base.GetValue(OffsetYProperty); }
            set
            {
                this.SetValue(OffsetYProperty, value);

                var tt = GetTranslateTransform(child);
                tt.Y = value;
            }
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();        
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();                
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);                
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                Zoom = 1.0;
                OffsetX = 0.0;
                OffsetY = 0.0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null && Move)
            {
                double zoom = e.Delta > 0 ? 0.2 : -0.2;
                if (!(e.Delta > 0) && (Zoom < 0.4))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX = relative.X * Zoom + OffsetX;
                double abosuluteY = relative.Y * Zoom + OffsetY;

                Zoom += zoom;

                OffsetX = abosuluteX - relative.X * Zoom;
                OffsetY = abosuluteY - relative.Y * Zoom;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null && Move)
            {
                start = e.GetPosition(this);
                origin = new Point(OffsetX, OffsetY);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null && Move)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
            e.Handled = false;
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Move)
            {
                this.Reset();
            }
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null && Move)
            {
                if (child.IsMouseCaptured)
                {
                    Vector v = start - e.GetPosition(this);
                    OffsetX = origin.X - v.X;
                    OffsetY = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }
}
