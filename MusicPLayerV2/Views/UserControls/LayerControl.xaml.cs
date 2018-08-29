using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPLayerV2.Views.UserControls
{
    /// <summary>
    /// LayerControl.xaml 的互動邏輯
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class LayerControl : UserControl
    {
        public LayerControl()
        {
            InitializeComponent();
            Border idxZero = new Border()
            {
                Style = Resources["RootBorder"] as Style
            };
            idxZero.Child = new TextBlock() { Text = "HelloWorld" };
            //RootLayer = idxZero;
        }

        //public UIElement RootLayer;



        public UIElement RootLayer
        {
            get { return (UIElement)GetValue(RootLayerProperty); }
            set { SetValue(RootLayerProperty, value); }
        }
        static UIElement DefaultRootLayerValue = null;
        public static readonly DependencyProperty RootLayerProperty =
            DependencyProperty.Register(nameof(RootLayer), typeof(UIElement), typeof(LayerControl),
                new FrameworkPropertyMetadata(DefaultRootLayerValue, FrameworkPropertyMetadataOptions.None,
                    OnRootLayerSet));
        private static void OnRootLayerSet(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ObservableCollection<UIElement> newOne  = new ObservableCollection<UIElement>();
            newOne.Add(args.NewValue as UIElement);
            (obj as LayerControl).Children = newOne;
            (obj as LayerControl)._Content.Content = (obj as LayerControl).RootLayer;
        }


        public ObservableCollection<UIElement> Children
        {
            get { return (ObservableCollection<UIElement>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }
        static ObservableCollection<UIElement> DefaultChildrenValue => new ObservableCollection<UIElement>();
        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register(nameof(Children), typeof(ObservableCollection<UIElement>), typeof(LayerControl),
                new FrameworkPropertyMetadata(DefaultChildrenValue, FrameworkPropertyMetadataOptions.None));


        public int PresentingIndex
        {
            get { return (int)GetValue(PresentingIndexProperty); }
            set { SetValue(PresentingIndexProperty, value); }
        }
        static int DefaultPresentingIndexValue = 0;
        public static readonly DependencyProperty PresentingIndexProperty =
            DependencyProperty.Register(nameof(PresentingIndex), typeof(int), typeof(LayerControl),
                new FrameworkPropertyMetadata(DefaultPresentingIndexValue, FrameworkPropertyMetadataOptions.AffectsRender,
                    OnPresentingIndexSet, OnPresentingIndexCoerce));
        private static void OnPresentingIndexSet(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = obj as LayerControl;
            if ((int)args.NewValue != -1)
                ctrl._Content.Content = ctrl.Children.ElementAt((int)args.NewValue);
        }
        private static object OnPresentingIndexCoerce(DependencyObject d, object baseValue)
        {
            var idx = (int)baseValue;
            var ctrl = d as LayerControl;
            if (idx < 0)
                return 0;
            if (idx >= ctrl.Children.Count)
                return ctrl.Children.Count - 1;
            return baseValue;
        }



    }
}
