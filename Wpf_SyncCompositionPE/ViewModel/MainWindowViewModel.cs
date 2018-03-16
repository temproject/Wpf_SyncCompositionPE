using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE.Infrastructure;
using Wpf_SyncCompositionPE.Model;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media.Effects;
using GalaSoft.MvvmLight;
using LoadingWindow;
using TFlex.DOCs.Model.Classes;
using System.Windows;

using System.Globalization;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Reflection;

using LoadingWindow.ViewModel;
using LoadingWindow.Model;

namespace Wpf_SyncCompositionPE.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        private MainWindow _mainWindow;

        /// <summary>
        /// Синхронизировать ресурсы
        /// </summary>
        public MainWindow MainWindow
        {
            get { return _mainWindow; }
            set { _mainWindow = value; RaisePropertyChanged(nameof(MainWindow)); }
        }

        //public static readonly DependencyProperty MainWindowProperty = DependencyProperty.Register("MainWindow", typeof(MainWindow), typeof(MainWindowViewModel));

        //public MainWindow MainWindow
        //{
        //    get { return (MainWindow)this.GetValue(MainWindowProperty); }
        //    set { this.SetValue(MainWindowProperty, value); }
        //}

        /// <summary>
        /// Данные для синхронизации состава работа
        /// </summary>
        public SyncCompositionPE syncCompositionPE { get; set; }


        //private readonly IDialogService dialogService;

        public MainWindowViewModel(/*ReferenceObject startRefObject,*/ MainWindow mw)
        {
            //IDialogService dialogService = new DialogService(null);

            //dialogService.Register<LoadWindowViewModel, LoadingMainWindow>();

            //this.dialogService = dialogService;
            //DisplayMessageCommand = new ActionCommand(p => DisplayMessage());

            //this.startRefObject = startRefObject;

            //if (HelperMethod.IsListNullOrEmpty(DetailingProjects))
            //{
            //    ShowError("Синхронизация состава работа", "Ошибка, выбранный элемент проекта не имеет детализаций!");
            //    return;
            //}

            //isHasDetailingProjects = true;

            if (MainWindow == null)
                MainWindow = mw;

        }

        //public ICommand DisplayMessageCommand { get; }

        //private void DisplayMessage()
        //{
        //    var viewModel = new LoadWindowViewModel("Hello!");

        //    bool? result = dialogService.ShowDialog(viewModel);

        //    if (result.HasValue)
        //    {
        //        if (result.Value)
        //        {
        //            // Accepted
        //        }
        //        else
        //        {
        //            // Cancelled
        //        }
        //    }
        //}
        #region  Properties

        private bool _IsSyncRes = false;

        /// <summary>
        /// Синхронизировать ресурсы
        /// </summary>
        public bool IsSyncRes
        {
            get { return _IsSyncRes; }
            set { _IsSyncRes = value; RaisePropertyChanged(nameof(IsSyncRes)); }
        }

        private bool _IsSyncOnlyPlanRes = true;

        /// <summary>
        /// Только плановые ресурсы
        /// </summary>
        public bool IsSyncOnlyPlanRes
        {
            get { return _IsSyncOnlyPlanRes; }
            set { _IsSyncOnlyPlanRes = value; RaisePropertyChanged(nameof(IsSyncOnlyPlanRes)); }
        }

        private bool _isApplyBlurEffect = false;

        /// <summary>
        /// примененный эффекта размытия 
        /// </summary>
        public bool IsApplyBlurEffect
        {
            get { return _isApplyBlurEffect; }
            set { _isApplyBlurEffect = value; RaisePropertyChanged(nameof(IsApplyBlurEffect)); }
        }

        //private ReferenceObject selectedDetailingProject_refObj;

        //public ReferenceObject SelectedDetailingProject_refObj
        //{
        //    get
        //    {
        //        if (selectedDetailingProject_refObj == null)
        //            selectedDetailingProject_refObj = SelectedDetailingProject as ReferenceObject;

        //        return selectedDetailingProject_refObj;
        //    }
        //}

        private object selectedDetailingProject;

        /// <summary>
        /// Выбранный в диалоге проект детализации
        /// </summary>
        public object SelectedDetailingProject
        {
            get
            {
                string SelectedName = string.Empty;

                if (selectedDetailingProject != null)
                    SelectedName = selectedDetailingProject.ToString();

                if (!string.IsNullOrEmpty(SelectedName))
                {
                    if (!_detailingProjects.IsListNullOrEmpty())
                        return _detailingProjects.FirstOrDefault(d => d.ToString() == SelectedName);
                    else
                    {
                        return DetailingProjects.FirstOrDefault(d => d.ToString() == SelectedName);
                    }

                }

                return null;
            }
            set
            {
                if (selectedDetailingProject == value || value == null) return;

                selectedDetailingProject = value;



                if (selectedDetailingProject != null)
                {
                    Tree = null;

                    //Console.WriteLine(" set SelectedDetailingProject {0}", selectedDetailingProject);

                    syncCompositionPE.SelectedDetailingProject = selectedDetailingProject as ReferenceObject;

                    ExecuteBuildTreeCommand(null);
                   
                }
                RaisePropertyChanged(nameof(SelectedDetailingProject));
            }
        }



        #endregion

        ObservableCollection<ReferenceObject> _detailingProjects;

        /// <summary>
        /// Список проектов (детализаций) выбронного в TFLEX DOCS элемента проекта
        /// </summary>
        public ObservableCollection<ReferenceObject> DetailingProjects
        {
            get
            {
                if (HelperMethod.IsListNullOrEmpty(_detailingProjects))
                {
                    _detailingProjects = syncCompositionPE.DetailingProjects;
                }
                return _detailingProjects;
            }
        }


        /// <summary>
        /// Дерево 
        /// </summary>
        private ObservableCollection<TreeViewModel> _tree;

        /// <summary>
        /// Данные отображения в дереве
        /// Дерево детализации выбранного элемента проекта
        /// </summary>
        public ObservableCollection<TreeViewModel> Tree
        {
            get
            {
                if (_tree == null)
                    Tree = new ObservableCollection<TreeViewModel>();

                return _tree;
            }
            private set
            {
                _tree = value;
                RaisePropertyChanged(nameof(Tree));
            }
        }

        #region Commands

        #region checkbox Синхронизировать ресурсы
        RelayCommand _syncResCommand;

        /// <summary>
        /// Синхронизировать ресурсы
        /// </summary>
        public ICommand SyncResCommand
        {
            get
            {
                if (_syncResCommand == null)
                    _syncResCommand = new RelayCommand(ExecuteSyncResCommand, CanExecuteSyncResCommand);
                return _syncResCommand;
            }
        }

        /// <summary>
        /// Смена флага на противоположное значение
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSyncResCommand(object parameter)
        {
            IsSyncRes = !IsSyncRes;
        }

        /// <summary>
        /// Проверка на возможность смены флага
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteSyncResCommand(object parameter)
        {
            if (HelperMethod.IsListNullOrEmpty(_detailingProjects))
                return false;
            else
                return true;
        }

        #endregion

        #region checkbox Синхронизировать только плановые ресурсы

        RelayCommand _syncOnlyPlanResCommand;

        /// <summary>
        /// Синхронизировать только плановые ресурсы
        /// </summary>
        public ICommand SyncOnlyPlanResCommand
        {
            get
            {
                if (_syncOnlyPlanResCommand == null)
                    _syncOnlyPlanResCommand = new RelayCommand(ExecuteSyncOnlyPlanResCommand, CanExecuteSyncOnlyPlanResCommand);
                return _syncOnlyPlanResCommand;
            }
        }

        /// <summary>
        /// Смена флага на противоположное значение
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSyncOnlyPlanResCommand(object parameter)
        {
            IsSyncOnlyPlanRes = !IsSyncOnlyPlanRes;
        }

        /// <summary>
        /// Проверка на возможность смены флага
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteSyncOnlyPlanResCommand(object parameter)
        {
            if (!IsSyncRes || HelperMethod.IsListNullOrEmpty(_detailingProjects))
            {
                //IsSyncOnlyPlanRes = false;
                return false;
            }
            else
                return true;
        }

        #endregion

        #region Кнопка Отмена

        public Action CloseAction { get; set; }

        public bool CanClose { get; set; } = true/*_bgWorkerTreeViewBuild == null && _bgWorkerSynchronizingСomposition == null*/;


        private RelayCommand _closeCommand;

        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseCommand
        {
            //get
            //{
            //    if (_closeCommand == null)
            //    {
            //        _closeCommand = new RelayCommand(param => Close(), param => CanClose);
            //    }
            //    return _closeCommand;
            //}
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand((obj) =>
                    {
                        Close(obj as Window);
                    });
                }
                return _closeCommand;
            }

        }

        public void Close(Window w)
        {
            bool exit = true;

            //if (_bgWorkerSynchronizingСomposition != null /*&& Percent != 100*/)
            //{
            //    _bgWorkerSynchronizingСomposition.CancelAsync();
            //    exit = false;
            //}


            //if (_bgWorkerTreeViewBuild != null/* && Percent != 100*/)
            //{
            //    _bgWorkerTreeViewBuild.CancelAsync();
            //    exit = false;
            //}

            if (exit)
            {
                Cleanup();
                w.Close(); // Invoke the Action previously defined by the View
            }
        }

        #endregion

        #region кнопка Обновить

        RelayCommand _buildTreeCommand;

        /// <summary>
        /// команда кнопки "Обновить"
        /// </summary>
        public ICommand BuildTreeCommand
        {
            get
            {
                if (_buildTreeCommand == null)
                    _buildTreeCommand = new RelayCommand(ExecuteBuildTreeCommand, CanExecuteBuildTreeCommand);
                return _buildTreeCommand;
            }
        }

        TreeViewModel treeViewModel = null;

        /// <summary>
        /// Действие кнопки "Обновить"
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteBuildTreeCommand(object parameter)
        {
           // StartWorkLoading(TreeBuild);
            //TreeBuild();

    

            StartWorkLoading(TreeBuild);
        }


        Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        BlurEffect bf;
        private void TreeBuild()
        {
            Tree.Clear();

            Worker.AddWork(TreeViewBuild_DoWork, "Загрузка дерева элементов проекта");

            using (LoadingMainWindow lw = new LoadingMainWindow())
            {
                lw.Owner = MainWindow;
                lw.ShowDialog();
            }
        }

        void TreeViewBuild_DoWork()
        {

            if (_dispatcher == null)
                _dispatcher = Dispatcher.CurrentDispatcher;

            treeViewModel = new TreeViewModel(syncCompositionPE.StartObjectDetail, null, syncCompositionPE.StartRefObject/*, this*/);

            _dispatcher.Invoke(new Action(() =>
            {
                Tree.Add(treeViewModel);
            }));

            if (treeViewModel.ContainsObjSync)
                System.Threading.Thread.Sleep(3500);
        }

        /// <summary>
        /// Условия выполнения кнопки "Загрузить"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteBuildTreeCommand(object parameter)
        {
            if (SelectedDetailingProject == null || !Tree.IsListNullOrEmpty())
                return false;
            else

            if (selectedDetailingProject == null)
                return false;

            return true;
        }

        #endregion

        #region Синхронизировать

        RelayCommand _syncCommand;

        /// <summary>
        /// Команда кнопки Синхронизировать
        /// </summary>
        public ICommand SyncCommand
        {
            get
            {
                if (_syncCommand == null)
                    _syncCommand = new RelayCommand(ExecuteSyncCommand, CanExecuteSyncCommand);

                return _syncCommand;
            }
        }


        // Задача объекта типа BackgroundWorker захватить свободный поток из пула потоков CLR и затем из
        // этого потока вызвать событие DoWork;
        //static private BackgroundWorker _bgWorkerSynchronizingСomposition = null;


        void Sync()
        {
            Worker.AddWork(bgWorkerSynchronizingСomposition_DoWork, "Синхронизация состава работ");
            using (LoadingMainWindow lw = new LoadingMainWindow())
            {
                lw.Owner = MainWindow;
                lw.ShowDialog();
            }
        }

        void bgWorkerSynchronizingСomposition_DoWork()
        {
            syncCompositionPE.SynchronizingСomposition(treeViewModel, IsCopyRes: IsSyncRes, IsCopyOnlyPlan: IsSyncOnlyPlanRes);
        }


        delegate void StarterWorkLoading();

        void StartWorkLoading(StarterWorkLoading starterWorkLoading)
        {

            if (bf == null)
                bf = new BlurEffect();

            bf.Radius = 10;
            MainWindow.Effect = bf;

            starterWorkLoading();

            bf.Radius = 0;
            MainWindow.Effect = bf;
        }

        /// <summary>
        /// Действие по нажатию кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSyncCommand(object parameter)
        {

            StartWorkLoading(Sync);
            StartWorkLoading(TreeBuild);

            //if (_bgWorkerSynchronizingСomposition != null) { return; }


            //_bgWorkerSynchronizingСomposition = new BackgroundWorker();
            //// Метод, который будет выполнятся в отдельном потоке. Событие DoWork срабатывает при вызове RunWorkerAsync
            //_bgWorkerSynchronizingСomposition.DoWork += new DoWorkEventHandler(bgWorkerSynchronizingСomposition_DoWork);
            //// Метод, который сработает в момент завершения BackgroundWorker
            //_bgWorkerSynchronizingСomposition.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerSynchComposition_RunWorkerCompleted);

            //// Для отслеживания выполнения хода работ свойство WorkerReportsProgress устанавливаем true
            //_bgWorkerSynchronizingСomposition.WorkerReportsProgress = true;
            //// Поддержка отмены выполнения фоновой операции с помощью метода CancelAsync()
            //_bgWorkerSynchronizingСomposition.WorkerSupportsCancellation = true;

            //// Запуск выполнения фоновой операции. Событие DoWork.
            //// Вторая перегрузка RunWorkerAsync позволяет передать объект событию DoWork для его последующей обработки в потоке.
            //_bgWorkerSynchronizingСomposition.RunWorkerAsync();

            //// Запуск выполнения фоновой операции. Событие DoWork.
            //// Вторая перегрузка RunWorkerAsync позволяет передать объект событию DoWork для его последующей обработки в потоке.
            //_bgWorkerSynchronizingСomposition.RunWorkerAsync();
        }

   

        /// <summary>
        /// Условия нажатия кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public bool CanExecuteSyncCommand(object parameter)
        {
            if (treeViewModel == null)
                return false;

            if (HelperMethod.IsListNullOrEmpty(_detailingProjects) || !treeViewModel.ContainsObjSync)
                return false;

            return true;
        }

        #endregion

        #endregion





        ///// <summary>
        ///// Очистка коллекции
        ///// </summary>
        public override void Cleanup()
        {

            //Clear(Tree);
            //Clear(_detailingProjects);

            //startRefObject = null;
            //StartRefObject = null;
            SelectedDetailingProject = null;
            //_bgWorkerTreeViewBuild = null;
            //_bgWorkerSynchronizingСomposition = null;
            _dispatcher = null;
        }

    }
}
