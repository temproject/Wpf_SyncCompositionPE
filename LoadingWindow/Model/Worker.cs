using GalaSoft.MvvmLight;
using LoadingWindow.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LoadingWindow.Model
{

    public delegate void WorkerEventHandler(object sender, EventArgs e);

    public class Worker : ViewModelBase
    {

        #region "Singleton"

        private static Worker instance = null;

        // Конструктор - "protected" 
        protected Worker()
        {
        }

        // Фабричное свойство x)
        public static Worker Instance
        {
            get
            {
                // Если: объект еще не создан    (1)         
                if (instance == null)
                {
                    // То: создаем новый экземпляр  (2)
                    instance = new Worker();
                }
                // Иначе: возвращаем ссылку на существующий объект  (3)
                return instance;
            }

        }

        #endregion


        public Action Work { get; set; }


        public void StartWork(Action work, string nameProcess)
        {
            if (work == null)
            {
                throw new ArgumentNullException();
            }

            if (Work != null)
                return;

            Work = work;

            //присваиваем текст процессу
            TextProcess = nameProcess;

            if (IsWorkComplet)//если процесс завершен можно запустить новый
            {
                IsWorkComplet = false;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                Task.Factory.StartNew(Work).ContinueWith(t => { Finish(); }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

 
        void Finish()
        {
            IsWorkComplet = true;
            Work = null;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        #region properties
        private string _textProcess = string.Empty;

        /// <summary>
        /// Наименование процесса работы
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
                RaisePropertyChanged();
                //ChangesTextProcessEvent(null, EventArgs.Empty);
            }
        }

        private bool _isWorkComplet = true;

        /// <summary>
        /// завершена ли работа
        /// </summary>
        public bool IsWorkComplet
        {
            get { return _isWorkComplet; }
            set
            {
                _isWorkComplet = value;

                if(_isWorkComplet)
                    VisibilityDownloadControl = Visibility.Hidden.ToString();
                else
                    VisibilityDownloadControl = Visibility.Visible.ToString();

                RaisePropertyChanged();
                //OnChangesShowDownloadControlEvent(null, EventArgs.Empty);
            }
        }

        private string _visibilityDownloadControl = Visibility.Hidden.ToString();

        public string VisibilityDownloadControl
        {
            get
            {
                Console.WriteLine("Показать контрол !!!!загрузки {0}", _visibilityDownloadControl);
                return _visibilityDownloadControl;
            }
            set
            {
                _visibilityDownloadControl = value;
                RaisePropertyChanged();
                //OnChangesShowDownloadControlEvent(null, EventArgs.Empty);
            }
        }

        #endregion

    }
}
