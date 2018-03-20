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
                Console.WriteLine("StartNew");
                Task.Factory.StartNew(Work).ContinueWith(t => { Finish(); }, TaskScheduler.FromCurrentSynchronizationContext());
                Console.WriteLine("FinishWork");
            }
        }


        void Finish()
        {
            Console.WriteLine("Finish");
            IsWorkComplet = true;
            Work = null;
            _currentNumberIterat = 0;
            _textProcess = string.Empty;
            _numberAllIterat = 0;
            _percent = "0%";
            Cancel = false;
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
        /// Получить процент выполнения
        /// </summary>
        /// <param name="current"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private double GetPercent(int current, int count)
        {
            return Math.Round(100.0 * current / count);
        }

        private int _numberAllIterat = 0;

        /// <summary>
        /// Количество итераций всего
        /// </summary>
        public int numberAllIterat
        {
            get { return _numberAllIterat; }
            set { _numberAllIterat = value; }
        }

        private int _currentNumberIterat = 0;

        /// <summary>
        /// Номер текущей итерации
        /// </summary>
        public int CurrentNumberIterat
        {
            get { return _currentNumberIterat; }
            set { _currentNumberIterat = value; RaisePropertyChanged("Percent"); }
        }


        private string _percent = "0%";

        public string Percent
        {
            get
            {
                if (numberAllIterat == 0)
                    _percent = "0%";
                else
                    _percent = GetPercent(CurrentNumberIterat, numberAllIterat ).ToString() + "%";
                return _percent;
            }
        }

        private bool _cancel = false;
        /// <summary>
        /// Отмена
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; if (_cancel) { TextProcess = "Отмена процесса";  } }
        }


        /// <summary>
        /// завершена ли работа
        /// </summary>
        public bool IsWorkComplet
        {
            get { return _isWorkComplet; }
            set
            {
                _isWorkComplet = value;

                if (_isWorkComplet)
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
