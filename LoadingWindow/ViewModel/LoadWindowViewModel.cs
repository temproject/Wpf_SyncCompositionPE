
using GalaSoft.MvvmLight;
using LoadingWindow.Model;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoadingWindow.ViewModel
{
    /// <summary>
    /// Этот класс содержит свойства, с которыми может связываться основной вид.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class LoadWindowViewModel : ViewModelBase
    {

        private string _textProcess = string.Empty;

        /// <summary>
        /// Sets and gets the  property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TextProcess
        {
            get
            {
                return _textProcess;
            }

            set
            {
                if (_textProcess == value)
                {
                    return;
                }

                _textProcess = value;
                RaisePropertyChanged(nameof(TextProcess));
            }
        }


        /// <summary>
        /// Инициализирует новый экземпляр класса MainViewModel.
        /// </summary>
        public LoadWindowViewModel()
        {
            if (IsInDesignMode)
            {
                // Код работает в Blend -> создает данные о времени разработки.
            }
            else // Код работает «по-настоящему»
            {
                Worker.WorkerEvent += OnWorkerEvent();

                //Percent = "0%";
                if (LoadingWindow.Model.Worker.IsWorkComplet)
                {
                    TextProcess = LoadingWindow.Model.Worker.TextProcess;
                    StartWork();
                }
            }
        }

        private Worker.WorkerEventHandler OnWorkerEvent()
        {
            Console.WriteLine("");
        }

        private void StartWork()
        {
            if (LoadingWindow.Model.Worker.Work == null)
                throw new ArgumentNullException();

            Task.Factory.StartNew(LoadingWindow.Model.Worker.Work).ContinueWith(t => { Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #region Кнопка Отмена

        public Action CloseAction { get; set; }


        public void Close()
        {
            LoadingWindow.Model.Worker.IsWorkComplet = true;

            if (LoadingWindow.Model.Worker.IsWorkComplet)
            {
                Cleanup();
                CloseAction(); // Invoke the Action previously defined by the View
            }
        }


        #endregion


        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
        }
    }
}