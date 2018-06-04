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
                    (obj as PositionControl).OnNowValueSet((double)args.NewValue);
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


        public double MouseDeltaChange
        {
            get { return (double)GetValue(MouseDeltaChangeProperty); }
            set { SetValue(MouseDeltaChangeProperty, value); }
        }
        public static readonly DependencyProperty MouseDeltaChangeProperty =
            DependencyProperty.Register("MouseDeltaChange", typeof(double), typeof(PositionControl), new PropertyMetadata(0.1));


        public bool IsSetValueOnlyUuholdThumb
        {
            get { return (bool)GetValue(IsSetValueOnlyUuholdThumbProperty); }
            set { SetValue(IsSetValueOnlyUuholdThumbProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSetValueOnlyUuholdThumb.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSetValueOnlyUuholdThumbProperty =
            DependencyProperty.Register(nameof(IsSetValueOnlyUuholdThumb), typeof(bool), typeof(PositionControl),
                new FrameworkPropertyMetadata(true));




        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            set { SetValue(ThumbSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThumbSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(nameof(ThumbSize), typeof(double), typeof(PositionControl),
                new FrameworkPropertyMetadata(40d,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    (obj as PositionControl).OnThumbSizeSet();
                }));
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
                OnNowValueSet(value);
            }
        }
        private void OnNowValueSet(double NewValue)
        {
            if (NewValue != NowValue)
                SetValue(NowValueProperty, Math.Max(Math.Min(NewValue, Max), Min));
            if (!IsThumbDragging)
                SetPositionByValue(NowValue);
        }

        private void OnThumbSizeSet()
        {
            TheC.Width = ThumbSize;
            TheC.Height = ThumbSize;
            Canvas.SetTop(TheC, -ThumbSize / 2+2.5);
            canvas.Margin = new Thickness(-ThumbSize / 2, 0, ThumbSize / 2, 0);
            ValueTooltip.VerticalOffset = -50;
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
            SetPositionLeft(newLeft);
            if (!IsSetValueOnlyUuholdThumb)
                NowValue = Left2Value(newLeft);
        }

        private void TheC_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsThumbDragging = false;
            var w = (TheC.Parent as Canvas).ActualWidth;
            NowValue = Left2Value(Canvas.GetLeft(TheC));
        }

        private void LineBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPositionByValue(NowValue);
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
            var v = Math.Max(Math.Min(((left) / w * (Max - Min)) + Min, Max), Min);
            return v;
        }
        private double Value2Left(double value)
        {
            if (Max - Min <= 0)
                return 0;
            var w = (TheC.Parent as Canvas).ActualWidth;
            var l = Math.Max(Math.Min(value / (Max - Min) * w, w), 0);
            return l;
        }

        private void SetPositionByValue(double value)
        {
            Canvas.SetLeft(TheC, Value2Left(value));
        }
        private void SetPositionLeft(double left)
        {
            var w = (TheC.Parent as Canvas).ActualWidth;
            Canvas.SetLeft(TheC, Math.Max(Math.Min(left, w), 0));
        }

        private void LineBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetPositionLeft(e.GetPosition(LineBorder).X);
            TheC.CaptureMouse();
        }

        private void LineBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                SetPositionLeft(e.GetPosition(LineBorder).X);
        }

        private void uc_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                NowValue += MouseDeltaChange;
            }
            if (e.Delta < 0)
            {
                NowValue -= MouseDeltaChange;
            }
        }
    }
}
