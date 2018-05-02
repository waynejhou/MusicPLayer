using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicPLayer.ViewModels
{
    public class LrcEditorViewModel: ViewModelBase
    {
        public ICommand PlayPauseCmd => App.MainWinViewModel.PlayPauseCmd;
        public string PlayBackState => App.MainWinViewModel.PlayBackState;

        public void NotifyAllProperty()
        {
            NotifyPropertyChanged(nameof(PlayPauseCmd));
            NotifyPropertyChanged(nameof(PlayBackState));
        }
    }
}
