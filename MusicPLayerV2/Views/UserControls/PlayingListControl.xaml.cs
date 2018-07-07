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
        private SongEntity NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;

        public PlayingListControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            L.LoadEntity(ListViewL.SelectedItem as SongEntity);
            C.PlayCmd.Execute(null);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //var selectIndex = ListViewL.SelectedItems.Cast<MusicItem>().ToList().ConvertAll(x => L.PlayingList.IndexOf(x)).OrderByDescending(x => x).ToList();
            //L.RemoveIndexItemFromListCmd.Execute(selectIndex);
        }

        private void ListViewL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (L != null)
            {
                L.SelectedItems = ListViewL.SelectedItems;
                foreach(SongEntity ee in ListViewL.SelectedItems)
                {
                    Console.WriteLine(ee.Name);
                }
            }
            Console.WriteLine(
                $"Left shift: {Keyboard.IsKeyDown(Key.LeftShift)}\n" +
                $"Right Shift: {Keyboard.IsKeyDown(Key.RightShift)}\n" +
                $"Left Crtl: {Keyboard.IsKeyDown(Key.LeftCtrl)}\n" +
                $"Right Crtl: { Keyboard.IsKeyDown(Key.RightCtrl)}\n"+
                $"Left shift: {Keyboard.IsKeyToggled(Key.LeftShift)}\n" +
                $"Right Shift: {Keyboard.IsKeyToggled(Key.RightShift)}\n" +
                $"Left Crtl: {Keyboard.IsKeyToggled(Key.LeftCtrl)}\n" +
                $"Right Crtl: { Keyboard.IsKeyToggled(Key.RightCtrl)}\n");
        }
    }
}
