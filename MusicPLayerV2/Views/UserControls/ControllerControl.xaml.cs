using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
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
    /// ControllerControl.xaml 的互動邏輯
    /// </summary>
    public partial class ControllerControl : UserControl
    {
        public ControllerControl()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            PosiC.ValueToStringConverter = (s) => TimeSpan.FromMilliseconds(s).ToString((string)App.Current.Resources["TimespanToStringFormat"]);
        }

        bool _isOverVolBtn = false;
        bool _isOverVolPopup = false;

        public bool IsOverVolBtn
        {
            get => _isOverVolBtn;
            set
            {
                _isOverVolBtn = value;
                PopupVol();
            }
        }

        public bool IsOverVolPopup
        {
            get => _isOverVolPopup;
            set
            {
                _isOverVolPopup = value;
                PopupVol();
            }
        }

        private void VolumnPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            IsOverVolBtn = true;
        }

        private void VolumnPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOverVolBtn = false;
        }

        private void VolumnBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            IsOverVolPopup = true;
        }

        private void VolumnBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOverVolPopup = false;
        }
        Timer timer = new Timer();
        private void PopupVol()
        {
            if ((IsOverVolBtn || IsOverVolPopup) == true)
            {
                VolumnPopup.IsOpen = true;
                timer.Stop();
            }
            else
            {
                timer.Interval = 500;
                timer.Start();
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                VolumnPopup.Dispatcher.Invoke(() => { VolumnPopup.IsOpen = false; });
            }
            catch (TaskCanceledException) { }

        }

    }
}
