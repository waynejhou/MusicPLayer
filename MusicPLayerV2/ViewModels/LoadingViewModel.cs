using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPLayerV2.ViewModels
{
    public class LoadingViewModel: ViewModelBase, IModalDialogViewModel
    {
        string _title = "Loading";
        public string Title { get=>_title; set
            {
                _title = value;
                NotifyPropertyChanged(nameof(Title));
            }
        }
        string _message = "";
        public string Message
        {
            get => _message; set
            {
                _message = value;
                NotifyPropertyChanged(nameof(Message));
            }
        }
        double _max = 100d;
        public double Max
        {
            get => _max; set
            {
                _max = value;
                NotifyPropertyChanged(nameof(Max));
            }
        }
        double _min = 0d;
        public double Min
        {
            get => _min; set
            {
                _min = value;
                NotifyPropertyChanged(nameof(Min));
            }
        }
        double _value = 0d;
        public double Value
        {
            get => _value; set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public bool? DialogResult => true;
    }
}
