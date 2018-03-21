using GalaSoft.MvvmLight;
using LoadingWindow.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;

namespace LoadingWindow.Model
{

    public enum TypeWorker
    {
        LoadTree = 0,
        SyncCompositionPE = 1
    }

    public delegate void WorkerEventHandler(object sender, EventArgs e);

    public class Worker : ViewModelBase
    {

        readonly object Lock = new object();

        Queue works = new Queue();

        public TypeWorker TypeWorker { get; set; }

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


        public void Produce(Action work)
        {
            lock (Lock)
            {
                works.Enqueue(work);
            }
        }

        void Consume(string nameProcess)
        {
            ParameterizedThreadStart _startWork = new ParameterizedThreadStart(startWork);
            Thread thread = new Thread(_startWork);
            thread.Start(nameProcess);
        }

        public void StartWork(Action work, string nameProcess, TypeWorker TypeWorker)
        {
            this.TypeWorker = TypeWorker;
            StartWork(work, nameProcess);
        }
        public void StartWork(Action work, string nameProcess)
        {
            Produce(work);
            Consume(nameProcess);
        }

        /// <summary>
        /// Завершение процеса
        /// </summary>
        void startWork(object nameProcess)
        {
            lock (Lock)
            {
                TextProcess = nameProcess.ToString();

                IsWorkComplet = false;

                (works.Dequeue() as Action).Invoke();

                TextProcess = string.Empty;

                CurrentNumberIterat = 0;

                _percent = "0%";
 
                Cancel = false;

                IsWorkComplet = true;

                numberAllIterat = 0;
            }
        }


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
            }
        }

        private bool _isWorkComplet = true;



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
        /// При увеличении номера итерации пересчитывается процент
        /// </summary>
        public int CurrentNumberIterat
        {
            get { return _currentNumberIterat; }
            set { _currentNumberIterat = value; RaisePropertyChanged("Percent"); }
        }


        private string _percent = "0%";

        /// <summary>
        /// Процент выполнения процесса
        /// </summary>
        public string Percent
        {
            get
            {

                if (_currentNumberIterat == 0 || numberAllIterat == 0)
                    _percent = "0%";
                else
                    _percent = GetPercent(_currentNumberIterat, numberAllIterat).ToString() + "%";

                //обновление команд
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();

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
            set { _cancel = value; if (_cancel) { TextProcess = "Отмена процесса"; } }
        }


        /// <summary>
        /// Признак завершения процесса
        /// </summary>
        public bool IsWorkComplet
        {
            get { return _isWorkComplet; }
            set
            {
                _isWorkComplet = value;

                RaisePropertyChanged();
            }
        }

        #endregion

    }
}
