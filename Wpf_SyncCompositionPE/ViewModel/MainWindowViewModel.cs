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


        public Worker Worker
        {
            get { return Worker.Instance; }
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


        public MainWindowViewModel()
        {

        }

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
            if (!IsSyncRes || _detailingProjects.IsListNullOrEmpty())
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

                    }//, (obj) =>
                    //{
                    //    return Worker.IsWorkComplet;

                    //}
                    );
                }
                return _closeCommand;
            }

        }

        private RelayCommand _cancelCommand;

        /// <summary>
        /// Отмена
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand((obj) =>
                    {
                     
                        Worker.Cancel = true;
                   
                    }
                    );
                }
                return _cancelCommand;
            }

        }


        private void Waite()
        {
            while (!Worker.IsWorkComplet) { }


            return;

        }
        public void Close(Window w)
        {
            //bool exit = true;

            Worker.Cancel = true;

            System.Threading.Tasks.Task.Run(() => Waite())
                .ContinueWith(t =>
                {
                    Cleanup();

                    w.Close();
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext()); ;


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
            Worker.Cancel = false;
            Worker.StartWork(TreeBuild, "Загрузка дерева элементов проекта");
        }


        Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;


        private void TreeBuild()
        {

            if (_dispatcher == null)
                _dispatcher = Dispatcher.CurrentDispatcher;

            _dispatcher.Invoke(new Action(() =>
            {
                Tree.Clear();
            }));

            treeViewModel = new TreeViewModel(syncCompositionPE.StartObjectDetail, null, syncCompositionPE.StartRefObject/*, this*/);

            if (Worker.Cancel) return;

            _dispatcher.Invoke(new Action(() =>
            {
                Tree.Add(treeViewModel);
            }));
        }



        /// <summary>
        /// Условия выполнения кнопки "Загрузить"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteBuildTreeCommand(object parameter)
        {
            if (selectedDetailingProject == null)
                return false;
            else
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



        delegate void StarterWorkLoading();

        void StartWorkLoading(StarterWorkLoading starterWorkLoading)
        {
            starterWorkLoading();
        }

        /// <summary>
        /// Действие по нажатию кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSyncCommand(object parameter)
        {
            Worker.Cancel = false;
            Worker.StartWork(Sync, "Синхронизация состава работ");
            ExecuteBuildTreeCommand(null);
        }


        void Sync()
        {
            syncCompositionPE.SynchronizingСomposition(treeViewModel, IsCopyRes: IsSyncRes, IsCopyOnlyPlan: IsSyncOnlyPlanRes);
        }

        /// <summary>
        /// Условия нажатия кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public bool CanExecuteSyncCommand(object parameter)
        {
            if (treeViewModel == null || !Worker.IsWorkComplet)
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
            syncCompositionPE = null;
            SelectedDetailingProject = null;
            _dispatcher = null;
        }
    }
}
