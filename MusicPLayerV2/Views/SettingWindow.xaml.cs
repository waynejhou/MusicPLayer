﻿using System;
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
using System.Windows.Shapes;

namespace MusicPLayerV2.Views
{
    /// <summary>
    /// SettingWindow.xaml 的互動邏輯
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void LeftPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedIndex == -1)
                return;
            if ((sender as ListView).SelectedIndex >= TagCtrl.Items.Count)
                return;
            TagCtrl.SelectedIndex = (sender as ListView).SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Utils.MusicDatabase.ExportTables($@"{App.ExecuteFilePath}DB.json", Utils.ExportType.JSON);
            //App.Library.SaveLibrary();
        }

        private void SettingWin_Closed(object sender, EventArgs e)
        {
            App.MainWin.Activate();
        }
    }

}
