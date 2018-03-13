using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;

namespace LoadingWindow.ViewModel
{
    /// <summary>
    /// Этот класс содержит свойства, с которыми может связываться основной вид.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        //#region свойствва для вычисления процента
        ///// <summary>
        ///// The <see cref="NumberAllIterations" /> property's name.
        ///// </summary>
        //public const string NumberAllIterationsPropertyName = "NumberAllIterations";

        //private int _numberAllIterationsProperty = 0;

        ///// <summary>
        ///// Sets and gets the NumberAllIterations property.
        ///// Changes to that property's value raise the PropertyChanged event. 
        ///// </summary>
        //public int NumberAllIterations
        //{
        //    get
        //    {
        //        return _numberAllIterationsProperty;
        //    }

        //    set
        //    {
        //        if (_numberAllIterationsProperty == value)
        //        {
        //            return;
        //        }

        //        _numberAllIterationsProperty = value;
        //        RaisePropertyChanged(NumberAllIterationsPropertyName);
        //    }
        //}

        ///// <summary>
        ///// The <see cref="CurrentIterationNumberPropertyName" /> property's name.
        ///// </summary>
        //public const string CurrentIterationNumberPropertyName = "CurrentIterationNumberPropertyName";

        //private int _currentIterationNumber = 0;

        ///// <summary>
        ///// Sets and gets the CurrentIterationNumberPropertyName property.
        ///// Changes to that property's value raise the PropertyChanged event. 
        ///// </summary>
        //public int CurrentIterationNumber
        //{
        //    get
        //    {
        //        return _currentIterationNumber;
        //    }

        //    set
        //    {
        //        if (_currentIterationNumber == value)
        //        {
        //            return;
        //        }

        //        _currentIterationNumber = value;
        //        RaisePropertyChanged(CurrentIterationNumberPropertyName);
        //    }
        //}

        ///// <summary>
        ///// The <see cref="Percent" /> property's name.
        ///// </summary>
        //public const string PercentProperty = "Percent";

        //private string _percent = string.Empty;

        ///// <summary>
        ///// Возвращает свойство WelcomeTitle.
        ///// Изменения значения этого свойства вызывает событие PropertyChanged.
        ///// </summary>
        //public string Percent
        //{
        //    get
        //    {
        //        return _percent = Math.Round(100.0 * CurrentIterationNumber / NumberAllIterations).ToString() + "%"; 
        //    }
        //    set
        //    {
        //        Set(ref _percent, value, PercentProperty);
        //        //RaisePropertyChanged(PercentProperty);
        //        //Messenger.Default.Send(Percent, "Hello!");
        //    }
        //}

        //#endregion

        /// <summary>
        /// The <see cref="textProcess" /> property's name.
        /// </summary>
        public const string PropertyTextProcess = "TextProcess";

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
                RaisePropertyChanged(PropertyTextProcess);
            }
        }

        public Action Worker { get; set; }

        //private readonly IDataService _dataService;

        ///// <summary>
        ///// The <see cref="WelcomeTitle" /> property's name.
        ///// </summary>
        //public const string WelcomeTitlePropertyName = "WelcomeTitle";

        //private string _welcomeTitle = string.Empty;

        ///// <summary>
        ///// Возвращает свойство WelcomeTitle.
        ///// Изменения значения этого свойства вызывает событие PropertyChanged.
        ///// </summary>
        //public string WelcomeTitle
        //{
        //    get
        //    {
        //        return _welcomeTitle;
        //    }
        //    set
        //    {
        //        Set(ref _welcomeTitle, value);
        //    }
        //}

        /// <summary>
        /// Инициализирует новый экземпляр класса MainViewModel.
        /// </summary>
        public MainViewModel(Action worker, string nameProcess)
        {
            if (IsInDesignMode)
            {
                // Код работает в Blend -> создает данные о времени разработки.
            }
            else // Код работает «по-настоящему»
            {
                //Percent = "0%";

                if (worker == null)
                    throw new ArgumentNullException();
                else
                    Worker = worker;

                TextProcess = nameProcess;

                Task.Factory.StartNew(Worker).ContinueWith(t => { CloseAction(); }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }


        #region Кнопка Отмена

        public Action CloseAction { get; set; }

   
        #endregion

        ///// <summary>
        ///// Инициализирует новый экземпляр класса MainViewModel.
        ///// </summary>
        //public MainViewModel()
        //{
        //    if (IsInDesignMode)
        //    {
        //        // Код работает в Blend -> создает данные о времени разработки.
        //    }
        //    else // Код работает «по-настоящему»
        //    {
        //        Percent = "0%";

        //        //if (worker == null)
        //        //    throw new ArgumentNullException();
        //        //else
        //        //    Worker = worker;

        //        //TextProcess = nameProcess;

        //        //for (int i = 0; i < 100; i++)
        //        //{
        //        //    Thread.Sleep(100);

        //        //    Messenger.Default.Register<string>(this, string.Format("{0}%", i), message => { Percent = message; });
        //        //}

        //    }
        //}


        ///// <summary>
        ///// Инициализирует новый экземпляр класса MainViewModel.
        ///// </summary>
        //public MainViewModel(IDataService dataService)
        //{
        //    _dataService = dataService;
        //    _dataService.GetData(
        //        (item, error) =>
        //        {
        //            if (error != null)
        //            {
        //                // Report error here
        //                return;
        //            }

        //            WelcomeTitle = item.Title;
        //        });
        //}

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
        }
    }
}