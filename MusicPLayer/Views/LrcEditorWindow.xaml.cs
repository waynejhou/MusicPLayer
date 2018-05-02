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
using MusicPLayer.ViewModels;

namespace MusicPLayer.View
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class LrcEditorWindow : Window
    {
        public LrcEditorWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            (DataContext as LrcEditorViewModel).NotifyAllProperty();
        }
    }
}
