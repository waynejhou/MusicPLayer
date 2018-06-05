using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using MusicPLayerV2.ViewModels;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPLayerV2.Views.UserControls
{
    /// <summary>
    /// PlayingListControl.xaml 的互動邏輯
    /// </summary>
    public partial class PlayingListControl : UserControl
    {

        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;

        public PlayingListControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            L.Load(ListViewL.SelectedItem as MusicItem);
            C.PlayCmd.Execute(null);
        }
    }
}
