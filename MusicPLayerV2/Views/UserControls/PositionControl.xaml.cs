using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MusicPLayerV2.Views.UserControls
{
    /// <summary>
    /// PositionControl.xaml 的互動邏輯
    /// </summary>
    public partial class PositionControl : UserControl
    {
        public PositionControl()
        {
            InitializeComponent();
        }
        #region Propertys
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min), typeof(double), typeof(PositionControl),
            new FrameworkPropertyMetadata(0d));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(double), typeof(PositionControl),
            new FrameworkPropertyMetadata(100d));
        public static readonly DependencyProperty NowValueProperty = DependencyProperty.Register(nameof(NowValue), typeof(double), typeof(PositionControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
                {
                    (obj as PositionControl).OnNowValueSet();
                }));
        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(PositionControl),
            new FrameworkPropertyMetadata(Brushes.LightGray,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    (obj as PositionControl).OnLineBrushSet();
                }));
        public static readonly DependencyProperty IsThumbMouseOverProperty = DependencyProperty.Register(nameof(IsThumbMouseOver), typeof(bool), typeof(PositionControl),
            new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsThumbDraggingProperty = DependencyProperty.Register(nameof(IsThumbDragging), typeof(bool), typeof(PositionControl),
            new FrameworkPropertyMetadata(false));
        #endregion
        public bool IsThumbMouseOver
        {
            get => (bool)GetValue(IsThumbMouseOverProperty);
            private set => SetValue(IsThumbMouseOverProperty,value);
        }

        public bool IsThumbDragging
        {
            get => (bool)GetValue(IsThumbDraggingProperty);
            private set => SetValue(IsThumbDraggingProperty, value);
        }

        public double Min
        {
            get => (double)GetValue(MinProperty); set
            {
                SetValue(MinProperty, Math.Min(value, Max));
            }
        }

        public double Max
        {
            get => (double)GetValue(MaxProperty); set
            {
                SetValue(MaxProperty, Math.Max(value, Min));
            }
        }

        public double NowValue
        {
            get { return (double)GetValue(NowValueProperty); }
            set
            {
                if (value != NowValue)
                    SetValue(NowValueProperty, Math.Max(Math.Min(value, Max), Min));
            }
        }
        private void OnNowValueSet()
        {
            if (!IsThumbDragging)
                SetPosition(NowValue);
        }

        public string ValueString => ValueToStringConverter(NowValue);
        public delegate string ValueToStringFunc(double value);
        public ValueToStringFunc ValueToStringConverter { get; set; } = (s) => s.ToString(".00");

        public Brush LineBrush
        {
            get => (SolidColorBrush)GetValue(LineBrushProperty); set
            {
                SetValue(LineBrushProperty, value);
            }
        }
        private void OnLineBrushSet()
        {
            LineBorder.Background = LineBrush;
        }


        private void TheC_DragStarted(object sender, DragStartedEventArgs e)
        {
            IsThumbDragging = true;
        }

        private void TheC_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var w = (TheC.Parent as Canvas).ActualWidth;
            var newLeft = Math.Max(Math.Min(Canvas.GetLeft(TheC) + e.HorizontalChange, w),0);
            PreviewSetPosition(newLeft);
        }

        private void TheC_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsThumbDragging = false;
            var w = (TheC.Parent as Canvas).ActualWidth;
            NowValue = Left2Value(Canvas.GetLeft(TheC));
        }

        private void LineBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPosition(NowValue);
        }

        bool IsTimeShowValueText => IsThumbMouseOver || IsValueTooltipMouseOver || IsLineMouseOver;
        bool IsLineMouseOver { get; set; } = false;
        bool IsValueTooltipMouseOver { get; set; } = false;

        private void TheC_MouseEnter(object sender, MouseEventArgs e)
        {
            IsThumbMouseOver = true;
            SetIsOpened();
        }

        private void TheC_MouseLeave(object sender, MouseEventArgs e)
        {
            IsThumbMouseOver = false;
            SetIsOpened();
        }

        private void LineBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            IsLineMouseOver = true;
            SetIsOpened();
        }

        private void LineBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            IsLineMouseOver = false;
            SetIsOpened();
        }

        private void ValueTooltip_MouseEnter(object sender, MouseEventArgs e)
        {
            IsValueTooltipMouseOver = true;
            SetIsOpened();
        }

        private void ValueTooltip_MouseLeave(object sender, MouseEventArgs e)
        {
            IsValueTooltipMouseOver = false;
            SetIsOpened();
        }

        private void SetIsOpened()
        {
            ValueTooltip.IsOpen = IsTimeShowValueText;
        }

        private void uc_MouseMove(object sender, MouseEventArgs e)
        {
            ValueTooltip.HorizontalOffset = e.GetPosition(this).X-25;
            ValueText.Text = ValueToStringConverter(Left2Value(Mouse.GetPosition(LineBorder).X));
        }



        private double Left2Value(double left)
        {
            if (Max - Min <= 0)
                return 0d;
            var w = (TheC.Parent as Canvas).ActualWidth;
            return Math.Max(Math.Min(((left ) / w * (Max - Min)) + Min, Max), Min);
        }
        private double Value2Left(double value)
        {
            if (Max - Min <= 0)
                return 0;
            var w = (TheC.Parent as Canvas).ActualWidth;
            return Math.Max(Math.Min(value / (Max - Min) * w, w), 0);
        }
        private void SetPosition(double value)
        {
            Canvas.SetLeft(TheC, Value2Left(value));
        }
        private void PreviewSetPosition(double left)
        {
            var w = (TheC.Parent as Canvas).ActualWidth;
            Canvas.SetLeft(TheC, Math.Max(Math.Min(left, w), 0));
        }

        private void LineBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewSetPosition(e.GetPosition(TheC).X);
            TheC.CaptureMouse();
        }

        private void LineBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                PreviewSetPosition(e.GetPosition(LineBorder).X);
        }
    }
}
