using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TreeViewProject.CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TreeViewProject.CustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TreeViewProject.CustomControls;assembly=TreeViewProject.CustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:GreenCheckControl/>
    ///
    /// </summary>
    public class GreenCheckControl : Control
    {
        static GreenCheckControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GreenCheckControl), new FrameworkPropertyMetadata(typeof(GreenCheckControl)));
        }

        // DependencyProperty cho Stroke (màu đường viền + dấu tích)

        public Brush StrokeColor
        {
            get => (Brush)GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }

        public static readonly DependencyProperty StrokeColorProperty =
            DependencyProperty.Register(nameof(StrokeColor), typeof(Brush), typeof(GreenCheckControl),
                new PropertyMetadata(Brushes.Green, OnVisualPropertyChanged));

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GreenCheckControl control)
                control.InvalidateVisual(); // redraw khi đổi màu
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = ActualWidth;
            double h = ActualHeight;
            double size = Math.Min(w, h);

            // Vẽ vòng tròn
            Pen circlePen = new Pen(StrokeColor, 2);
            dc.DrawEllipse(null, circlePen, new Point(w / 2, h / 2), size / 2 - 2, size / 2 - 2);

            // Vẽ dấu tích bằng PathFigure
            Pen checkPen = new Pen(StrokeColor, 2)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };

            PathFigure figure = new PathFigure { StartPoint = new Point(w * 0.3, h * 0.55) };
            figure.Segments.Add(new LineSegment(new Point(w * 0.45, h * 0.7), true));
            figure.Segments.Add(new LineSegment(new Point(w * 0.75, h * 0.35), true));

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            dc.DrawGeometry(null, checkPen, geometry);
        }
    }
}