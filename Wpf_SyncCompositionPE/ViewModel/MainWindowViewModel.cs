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

namespace Wpf_SyncCompositionPE.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        MainWindow mainWindow;

        public MainWindowViewModel(ReferenceObject startRefObject, MainWindow mw)
        {
            this.startRefObject = startRefObject;

            if (IsListNullOrEmpty(DetailingProjects))
                ShowError("Синхронизация состава работа", "Ошибка, выбранный элемент проекта не имеет детализаций!");

            if (mainWindow == null)
                mainWindow = mw;

            return;
        }

        private ReferenceObject startRefObject;

        /// <summary>
        /// Выбранный элемент проекта с которого был запущен диалог
        /// </summary>
        public ReferenceObject StartRefObject
        {
            get
            {
                if (startRefObject == null)
                    startRefObject = References.ProjectManagementReference.Find(startRefObject.SystemFields.Guid);

                return startRefObject;
            }
            private set
            {
                startRefObject = value;
                RaisePropertyChanged("StartObject");
            }
        }

        /// <summary>
        /// Детализация  выбранного элемента с которого был запущен диалог
        /// </summary>
        public ReferenceObject StartObjectDetail
        {
            get
            {
                return References.ProjectManagementReference.Find(Зависимость[Synchronization.SynchronizationParameterGuids.param_SlaveWork_Guid].GetGuid());
            }
        }

        #region  Properties

        private bool _IsSyncRes = false;

        public bool IsSyncRes
        {
            get { return _IsSyncRes; }
            set { _IsSyncRes = value; RaisePropertyChanged("IsSyncRes"); }
        }

        private bool _IsSyncOnlyPlanRes = true;

        public bool IsSyncOnlyPlanRes
        {
            get { return _IsSyncOnlyPlanRes; }
            set { _IsSyncOnlyPlanRes = value; RaisePropertyChanged("IsSyncOnlyPlanRes"); }
        }

        private ReferenceObject selectedDetailingProject_refObj;

        public ReferenceObject SelectedDetailingProject_refObj
        {
            get
            {
                if (selectedDetailingProject_refObj == null)
                    selectedDetailingProject_refObj = SelectedDetailingProject as ReferenceObject;

                return selectedDetailingProject_refObj;
            }
        }

        private object selectedDetailingProject;

        public object SelectedDetailingProject
        {
            get
            {
                string SelectedName = string.Empty;

                if (selectedDetailingProject != null)
                    SelectedName = selectedDetailingProject.ToString();

                ReferenceObject data = null;

                if (!string.IsNullOrEmpty(SelectedName))
                    data = DetailingProjects.FirstOrDefault(d => d.ToString() == SelectedName);

                return data;
            }
            set
            {
                if (selectedDetailingProject == value) return;

                selectedDetailingProject = value;

                selectedDetailingProject_refObj = selectedDetailingProject as ReferenceObject;

                Tree = null;

                RaisePropertyChanged("SelectedDetailingProject");
            }
        }

        #endregion

        ObservableCollection<ReferenceObject> _detailingProjects;

        /// <summary>
        /// Список проектов (детализаций) выбронного элемента проекта
        /// </summary>
        public ObservableCollection<ReferenceObject> DetailingProjects
        {
            get
            {
                if (IsListNullOrEmpty(_detailingProjects))
                {
                    _detailingProjects = LoadDetailingProjects();
                }
                return _detailingProjects;
            }
        }

        private ObservableCollection<ReferenceObject> LoadDetailingProjects()
        {
            return ProjectManagementWork.AllDetailingProjects(ЗависимостиДетализации);
        }


        private List<ReferenceObject> _зависимостиДетализации;

        /// <summary>
        /// Зависимости детализации выбранного элемента проекта с которого был запущен диалог
        /// </summary>
        public List<ReferenceObject> ЗависимостиДетализации
        {
            get
            {
                if (IsListNullOrEmpty(_зависимостиДетализации))
                {
                    _зависимостиДетализации = new List<ReferenceObject>();
                    _зависимостиДетализации = Synchronization.GetDependenciesObjects(false, StartRefObject);
                }
                return _зависимостиДетализации;
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
                    _tree = new ObservableCollection<TreeViewModel>();

                return _tree;
            }
            private set
            {
                _tree = value;
                RaisePropertyChanged("Tree");
            }
        }

        private ReferenceObject _зависимость;

        /// <summary>
        /// Зависимость выбранного в диалоге проекта детализации
        /// </summary>
        public ReferenceObject Зависимость
        {
            get
            {
                _зависимость = ЗависимостиДетализации.FirstOrDefault(d => d[Synchronization.SynchronizationParameterGuids.param_SlaveProject_Guid].GetGuid() == selectedDetailingProject_refObj.SystemFields.Guid);
                return _зависимость;
            }
        }

        #region MessageBox

        void ShowError(string caption, string message)
        {
            System.Windows.Forms.MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.ServiceNotification);
        }

        void ShowInfo(string caption, string message)
        {
            System.Windows.Forms.MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.ServiceNotification);
        }

        #endregion

        public static bool IsListNullOrEmpty<T>(IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
                return true;

            return false;
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
            if (IsListNullOrEmpty(_detailingProjects))
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
            if (!IsSyncRes || IsListNullOrEmpty(_detailingProjects))
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
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => Close(), param => CanClose);
                }
                return _closeCommand;
            }

        }

        public void Close()
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
                CloseAction(); // Invoke the Action previously defined by the View
            }
        }

        #endregion

        #region кнопка Загрузить

        RelayCommand _buildTreeCommand;

        /// <summary>
        /// команда кнопки "Загрузить"
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

        TreeViewModel TreeViewModel = null;

        /// <summary>
        /// Действие кнопки "Загрузить"
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteBuildTreeCommand(object parameter)
        {
            StartWorkLoading(TreeBuild);
        }


        //private int amountAllItems;

        ///// <summary>
        ///// Количество элементов в дереве
        ///// </summary>
        //public int AmountAllItemsTree
        //{
        //    get { return amountAllItems; }
        //    set { amountAllItems = value; RaisePropertyChanged("AmountAllItems"); }
        //}

        //private double percent;

        //public double Percent
        //{
        //    get
        //    {
        //        return percent;
        //    }
        //    set
        //    {

        //        percent = value;

        //        if (percent == 0)
        //        {
        //            TreeViewModel.PercentTreeBuild = percent;
        //            TreeViewModel.Instances = 0;

        //        }
        //        RaisePropertyChanged("Percent");
        //    }
        //}


        //private int instanceNumber;

        ///// <summary>
        ///// Номер итерации действия в контексте
        ///// </summary>
        //public int InstanceNumber
        //{
        //    get { return instanceNumber; }
        //    set 
        //    {
        //        instanceNumber = value;
        //        RaisePropertyChanged("InstanceNumber");
        //    }
        //}

        //private string _visibilityLoading = "Collapsed";

        ///// <summary>
        ///// Свойство отображения контрола загрузки
        ///// </summary>
        //public string VisibilityLoading
        //{
        //    get
        //    {
        //        //если отображается контрол загрузки, скрыть контрол дерева
        //        if (_visibilityLoading == "Visible")
        //            VisibilityTree = "Collapsed";

        //        //иначе
        //        else VisibilityTree = "Visible";

        //        return _visibilityLoading;
        //    }
        //    set { _visibilityLoading = value; RaisePropertyChanged("VisibilityLoading"); }
        //}

        //private string _visibilityTree = "Visible";

        ///// <summary>
        ///// Свойство отображения дерева
        ///// Если отображается контрол загрузки, то дерево скрыто
        ///// </summary>
        //public string VisibilityTree
        //{
        //    get
        //    {
        //        return _visibilityTree;
        //    }
        //    set { _visibilityTree = value; RaisePropertyChanged("VisibilityTree"); }
        //}


        // Задача объекта типа BackgroundWorker захватить свободный поток из пула потоков CLR и затем из
        // этого потока вызвать событие DoWork;
        //static private BackgroundWorker _bgWorkerTreeViewBuild = null;

        Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        BlurEffect bf;
        private void TreeBuild()
        {
            Tree.Clear();

            using (LoadingMainWindow lw = new LoadingMainWindow(TreeViewBuild_DoWork, "Загрузка дерева элементов проекта"))
            {
                lw.Owner = mainWindow;
                lw.ShowDialog();
            }


            //if (_bgWorkerTreeViewBuild != null) { return; }


            //_bgWorkerTreeViewBuild = new BackgroundWorker();
            //// Метод, который будет выполнятся в отдельном потоке. Событие DoWork срабатывает при вызове RunWorkerAsync
            //_bgWorkerTreeViewBuild.DoWork += new DoWorkEventHandler(bgWorkerTreeViewBuild_DoWork);
            //// Метод, который сработает в момент завершения BackgroundWorker
            //_bgWorkerTreeViewBuild.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            //// Для отслеживания выполнения хода работ свойство WorkerReportsProgress устанавливаем true
            //_bgWorkerTreeViewBuild.WorkerReportsProgress = true;
            //// Поддержка отмены выполнения фоновой операции с помощью метода CancelAsync()
            //_bgWorkerTreeViewBuild.WorkerSupportsCancellation = true;

            //// Запуск выполнения фоновой операции. Событие DoWork.
            //// Вторая перегрузка RunWorkerAsync позволяет передать объект событию DoWork для его последующей обработки в потоке.
            //_bgWorkerTreeViewBuild.RunWorkerAsync();
        }

        void TreeViewBuild_DoWork(/*object sender, DoWorkEventArgs e*/)
        {
            //VisibilityLoading = "Visible";

            //// Отмена выполнения фоновой задачи, сработает при вызове CancelAsync
            //if (_bgWorkerTreeViewBuild.CancellationPending)
            //{
            //    Console.WriteLine("Отмена выполнения фоновой задачи, сработает при вызове CancelAsync");
            //    e.Cancel = true; // значение нужно установить для того что бы при событии RunWorkerCompleted определить почему оно было вызвано, из-за того что закончилась операция или из-за отмены.
            //    ShowInfo("Загрузка", "Отмена выполнения фоновой операции.");
            //    return; // Отмена выполнения фоновой операции.
            //}

            //if (e.Cancel)
            //{
            //    Console.WriteLine("e.Cancel");
            //}

            if (_dispatcher == null)
                _dispatcher = Dispatcher.CurrentDispatcher;

            TreeViewModel = new TreeViewModel(StartObjectDetail, null, startRefObject/*, this*/);

            _dispatcher.Invoke(new Action(() =>
            {
                Tree.Add(TreeViewModel);
            }));
        }

        //// Метод, который сработает в момент завершения BackgroundWorker
        //private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{

        //    CommandManager.InvalidateRequerySuggested();


        //    //VisibilityLoading = "Collapsed";

        //    _bgWorkerTreeViewBuild = null;
        //    //ShowInfo("Загрузка", Title);
        //}


        // Метод работает из потока Dispetcher. Он может получать доступ к переменным окна.
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Условия выполнения кнопки "Загрузить"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteBuildTreeCommand(object parameter)
        {
            //if (SelectedDetailingProject == null || !IsListNullOrEmpty(Tree))
            //    return false;
            //else
            if (selectedDetailingProject_refObj == null)
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
            using (LoadingMainWindow lw = new LoadingMainWindow(bgWorkerSynchronizingСomposition_DoWork, "Синхронизация состава работ"))
            {
                lw.Owner = mainWindow;
                lw.ShowDialog();
            }
        }

        void bgWorkerSynchronizingСomposition_DoWork(/*object sender, DoWorkEventArgs e*/)
        {

            //VisibilityLoading = "Visible";

            //// Отмена выполнения фоновой задачи, сработает при вызове CancelAsync
            //if (_bgWorkerSynchronizingСomposition.CancellationPending)
            //{
            //    Console.WriteLine("Отмена выполнения фоновой задачи, сработает при вызове CancelAsync");
            //    e.Cancel = true; // значение нужно установить для того что бы при событии RunWorkerCompleted определить почему оно было вызвано, из-за того что закончилась операция или из-за отмены.
            //    ShowInfo("Загрузка", "Отмена выполнения фоновой операции.");
            //    return; // Отмена выполнения фоновой операции.
            //}

            //if (e.Cancel)
            //{
            //    Console.WriteLine("e.Cancel");
            //}

            //int amountAddObjects = 0;

            SynchronizingСomposition(TreeViewModel, IsCopyRes: IsSyncRes, IsCopyOnlyPlan: IsSyncOnlyPlanRes/*, ref amountAddObjects*//*, this*/);

            //ShowInfo("Синхронизация завершена", string.Format("Добавлено {0} элемента(ов) проекта", amountAddObjects));
        }


        delegate void StarterWorkLoading();

        void StartWorkLoading(StarterWorkLoading starterWorkLoading)
        {
            if (bf == null)
                bf = new BlurEffect();

            bf.Radius = 10;
            mainWindow.Effect = bf;

            starterWorkLoading();

            bf.Radius = 0;
            mainWindow.Effect = bf;
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

        // Данный метод работает в отдельном потоке.

        // Метод, который сработает в момент завершения BackgroundWorker
        private void bgWorkerSynchComposition_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            CommandManager.InvalidateRequerySuggested();

            //VisibilityLoading = "Collapsed";

            //_bgWorkerTreeViewBuild = null;
            //ShowInfo("Загрузка", Title);
        }

        /// <summary>
        /// Условия нажатия кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public bool CanExecuteSyncCommand(object parameter)
        {
            if (TreeViewModel == null)
                return false;

            if (IsListNullOrEmpty(_detailingProjects) || !TreeViewModel.ContainsObjSync)
                return false;

            return true;
        }

        #endregion

        #endregion

        void Clear<T>(ObservableCollection<T> list)
        {
            if (list != null)
            {
                list.Clear();
                list = null;
            }
        }


        public void SynchronizingСomposition(TreeViewModel treeViewModel, bool IsCopyRes, bool IsCopyOnlyPlan/*, ref int amountAddObjects*//*, MainWindowViewModel mainWindowViewModel = null*/)
        {
            //if (MainWindowViewModel == null)
            //    MainWindowViewModel = mainWindowViewModel;

            ReferenceObject Parent = treeViewModel.PEForSync;

            if (Parent != null)
                Parent = syncComposition(treeViewModel, IsCopyRes: IsCopyRes, IsCopyOnlyPlan: IsCopyOnlyPlan/*, ref amountAddObjects*/);

            foreach (TreeViewModel treeViewModelItem in treeViewModel.Children)
            {
                if (Parent != null && treeViewModelItem.PEForSync == null)
                {
                    treeViewModelItem.PEForSync = Parent;
                }
                SynchronizingСomposition(treeViewModelItem, IsCopyRes: IsCopyRes, IsCopyOnlyPlan: IsCopyOnlyPlan/*, ref amountAddObjects*/);
            }
        }

        private ReferenceObject syncComposition(TreeViewModel pe_treeItem, bool IsCopyRes, bool IsCopyOnlyPlan/*, ref int amountAddObjects*/)
        {

            #region
            //Объект   для синхронизации 
            if (pe_treeItem.IsObjectToSync/* && (bool)pe_treeItem.IsSelectObjToSynch*/)
            {
                /* Если синхронизация отсутствует, то создаём новую работу в плане РП 
                   в синхронизированной с Текущей и устанавливаем синхронизацию с дочерней из плана детализации.
               */

                ClassObject TypePE = pe_treeItem.ProjectElement.Class;

                List<Guid> GuidsLinks = new List<Guid>() { new Guid("063df6fa-3889-4300-8c7a-3ce8408a931a"),
                new Guid("68989495-719e-4bf3-ba7c-d244194890d5"), new Guid("751e602a-3542-4482-af40-ad78f90557ad"),
                new Guid("df3401e2-7dc6-4541-8033-0188a8c4d4bf"),new Guid("58d2e256-5902-4ed4-a594-cf2ba7bd4770")
                ,new Guid("0e1f8984-5ebe-4779-a9cd-55aa9c984745") ,new Guid("79b01004-3c10-465a-a6fb-fe2aa95ae5b8")
                ,new Guid("339ffc33-55b2-490f-b608-a910c1f59f51")};

                //Console.WriteLine(pe_treeItem.ReferenceObject + " " + pe_treeItem.PEForSync);
                //Console.WriteLine("Что копируем {0}, Тип нового объекта {1}, где создаем {2}", pe_treeItem.ReferenceObject, TypePE, pe_treeItem.PEForSync);

                //pe_treeItem.ProjectElement.Refresh(pe_treeItem.ProjectElement);

                //var newPE = new ProjectManagementWork(pe_treeItem.ReferenceObject.CreateCopy(TypePE, pe_treeItem.PEForSync, GuidsLinks, false));
                var newPE = ProjectManagementWork.CopyPE(pe_treeItem.ProjectElement, pe_treeItem.PEForSync, GuidsLinks);

                if (newPE != null)
                {

                    //amountAddObjects++;

                    ProjectManagementWork.RecalcResourcesWorkLoad(newPE);
                    //Console.WriteLine("Синхронизирование элемент {0} в укрупнении {1}", pe_treeItem.Name, pe_treeItem.PEForSync.ToString());
                    /*   newPE.ReferenceObject.EndChanges()*/
                    ;
                    //amountCreate++;

                    // text = string.Format("Добавление элемента проекта {0}", newPE.ToString());
                    // WaitingHelper.SetText(text);

                    if (IsCopyRes)
                    {
                        ProjectManagementWork.СкопироватьИспользуемыеРесурсы_изЭлементаПроекта_вЭлементПроекта
    (newPE, pe_treeItem.ProjectElement, onlyPlanningRes: IsCopyOnlyPlan, PlanningSpaceForNewRes_Guid: ProjectManagementWork.GetProject(newPE)[ProjectManagementWork.PM_param_PlanningSpace_GUID].GetGuid());
                    }

                    Synchronization.SyncronizeWorks(newPE, pe_treeItem.ProjectElement);

                    //pe_treeItem.IsSelectObjToSynch = false;
                    //pe_treeItem.IsObjectToSync = false;

                    return newPE;
                }
            }
            #endregion
            return null;
        }



        ///// <summary>
        ///// Очистка коллекции
        ///// </summary>
        public override void Cleanup()
        {

            Clear(Tree);
            Clear(_detailingProjects);

            startRefObject = null;
            StartRefObject = null;
            SelectedDetailingProject = null;
            //_bgWorkerTreeViewBuild = null;
            //_bgWorkerSynchronizingСomposition = null;
            _dispatcher = null;
        }

    }
}
