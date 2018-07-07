using MusicPLayerV2.Views;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPLayerV2.ViewModels
{
    public class LoadingViewModel<T>: ViewModelBase where T :class
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

        private readonly BackgroundWorker BGWorker;

        public LoadingViewModel()
        {
            BGWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
            };
            BGWorker.ProgressChanged += BGWorker_ProgressChanged;
        }


        public void RunWorkerAsync(ShowLoadingWinCondition condition) {
            RunWorkerAsync(null, condition);
        }
        public void RunWorkerAsync(T argument, ShowLoadingWinCondition condition)
        {
            if (argument == null)
                BGWorker.RunWorkerAsync();
            else
                BGWorker.RunWorkerAsync(argument);
            if (condition(argument))
            {
                new LoadingWindow()
                {
                    DataContext = this
                }.ShowDialog();
            }
        }
        private void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Message = e.UserState as string;
            Value = e.ProgressPercentage;
        }
        public delegate bool ShowLoadingWinCondition(T argument);
        public delegate void DoWorkEventHandler(BackgroundWorker bgw, LoadingViewModel<T> vm, T args, DoWorkEventArgs e);
        public event DoWorkEventHandler DoWork
        {
            add
            {
                DoWorkEventLog.Add(value, (sender, e) => value.Invoke(sender as BackgroundWorker, this, e.Argument as T, e));
                BGWorker.DoWork += DoWorkEventLog[value];
            }
            remove
            {
                BGWorker.DoWork -= DoWorkEventLog[value];
                DoWorkEventLog.Remove(value);
            }
        }
        private readonly Dictionary<DoWorkEventHandler, System.ComponentModel.DoWorkEventHandler>
            DoWorkEventLog = new Dictionary<DoWorkEventHandler, System.ComponentModel.DoWorkEventHandler>();

        public delegate void RunWorkerCompletedEventHandler(BackgroundWorker bgw, LoadingViewModel<T> vm, T result, RunWorkerCompletedEventArgs e);
        public event RunWorkerCompletedEventHandler RunWorkerCompleted
        {
            add
            {
                RunWorkerCompletedEventLog.Add(value, (sender, e) => value.Invoke(sender as BackgroundWorker, this, e.Result as T, e));
                BGWorker.RunWorkerCompleted += RunWorkerCompletedEventLog[value];
            }
            remove
            {
                BGWorker.RunWorkerCompleted -= RunWorkerCompletedEventLog[value];
                RunWorkerCompletedEventLog.Remove(value);
            }
        }
        private readonly Dictionary<RunWorkerCompletedEventHandler, System.ComponentModel.RunWorkerCompletedEventHandler>
            RunWorkerCompletedEventLog = new Dictionary<RunWorkerCompletedEventHandler, System.ComponentModel.RunWorkerCompletedEventHandler>();
    }

}
