using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TFlex.DOCs.Model.References;
using WpfApp_TreeSyncCompositionWork.Infrastructure;
using WpfApp_TreeSyncCompositionWork.Model;

using System.Windows.Input;


namespace Wpf_SyncCompositionPE.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        private ReferenceObject startRefObject;

        private ProjectManagementWork startObject;

        public ProjectManagementWork StartObject
        {
            get
            {
                if (startObject == null)
                    startObject = new ProjectManagementWork(startRefObject);
                return startObject;
            }
            private set
            {
                startObject = value;
                OnPropertyChanged("StartObject");
            }
        }
        private ReferenceObject startObjectDetail;

        public ReferenceObject StartObjectDetail
        {
            get
            {
                if (startObjectDetail == null)
                {
                    var dependcy = ЗависимостиДетализации.FirstOrDefault(d => d[Synchronization.SynchronizationParameterGuids.param_SlaveProject_Guid].GetGuid() == SelectedDetailingProject_refObj.SystemFields.Guid);

                    startObjectDetail = References.ProjectManagementReference.Find(dependcy[Synchronization.SynchronizationParameterGuids.param_SlaveWork_Guid].GetGuid());
                }
                return startObjectDetail;
            }

        }



        public MainWindowViewModel(ReferenceObject startRefObject)
        {
            this.startRefObject = startRefObject;

            if (startObject == null)
                startObject = new ProjectManagementWork(startRefObject);

            if (IsListNullOrEmpty(DetailingProjects))
                ShowError("Синхронизация состава работа", "Ошибка, выбранная детализация не найдена!");

            return;
        }

        #region  

        private bool _IsSyncRes = false;

        public bool IsSyncRes
        {
            get { return _IsSyncRes; }
            set { OnPropertyChanged("IsSyncRes"); _IsSyncRes = value; }
        }
        private bool _IsSyncOnlyPlanRes = false;

        public bool IsSyncOnlyPlanRes
        {
            get { return _IsSyncOnlyPlanRes; }
            set { OnPropertyChanged("IsSyncOnlyPlanRes"); _IsSyncOnlyPlanRes = value; }
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

                //if (selectedDetailingProject != data as Object)
                //    Tree = null;

                return data;

            }
            set
            {
                OnPropertyChanged("SelectedDetailingProject");

                if (selectedDetailingProject == value) return;

                Tree = null;

                selectedDetailingProject = value;


                //selectedDetailingProject = DetailingProjects.FirstOrDefault(d=>d.Name == value.ToString()).ReferenceObject;

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
        ObservableCollection<ReferenceObject> LoadDetailingProjects()
        {
            return ProjectManagementWork.AllDetailingProjects(ЗависимостиДетализации);
        }


        /// <summary>
        /// Дерево 
        /// </summary>
        private ObservableCollection<TreeViewModel> _tree;

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
                OnPropertyChanged("Tree");
                Console.WriteLine("Tree");
            }
        }

        private List<ReferenceObject> _зависимостиДетализации;

        public List<ReferenceObject> ЗависимостиДетализации
        {
            get
            {
                if (IsListNullOrEmpty(_зависимостиДетализации))
                {
                    _зависимостиДетализации = new List<ReferenceObject>();
                    _зависимостиДетализации = Synchronization.GetDependenciesObjects(false, StartObject);
                }
                return _зависимостиДетализации;
            }
        }

        void ShowError(string caption, string message)
        {
            System.Windows.Forms.MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.ServiceNotification);
        }

        public static bool IsListNullOrEmpty<T>(IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
                return true;

            return false;
        }


        #region Commands



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
                IsSyncOnlyPlanRes = false;
                return false;
            }
            else
                return true;
        }

        #region Кнопка Отмена
        public Action CloseAction { get; set; }

        public bool CanClose { get; set; } = true;


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
            OnDispose();
            CloseAction(); // Invoke the Action previously defined by the View
        }

        #endregion

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


        /// <summary>
        /// Действие по нажатию кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSyncCommand(object parameter)
        {
            //Console.WriteLine("Синхронизация");
            Synchronization.SynchronizingСomposition(TreeViewModel, IsSyncRes, IsSyncOnlyPlanRes);
            TreeRefresh();

            //Console.WriteLine("Синхронизация завершена");
        }

        /// <summary>
        /// Условия нажатия кнопки
        /// </summary>
        /// <param name="parameter"></param>
        public bool CanExecuteSyncCommand(object parameter)
        {
            if (IsListNullOrEmpty(_detailingProjects) || TreeViewModel == null || !TreeViewModel.ContainsObjSync)
                return false;
            else
                return true;
        }

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
            Console.WriteLine("Загрузить");
            TreeRefresh();
        }



        private void TreeRefresh()
        {
            Tree.Clear();
            //PE_treeItem PE_treeItem = new ElementProject(StartObjectDetail,false, startRefObject);
            TreeViewModel = new TreeViewModel(StartObjectDetail, null);
            TreeViewModel.IsExpanded = true;
            TreeViewModel.StartSelectBiggerPE = startRefObject;
            //foreach (TreeViewModel item in TreeViewModel.Children)
            //{
            //    Console.WriteLine(item.Name);
            //}
            Tree.Add(TreeViewModel);
        }

        /// <summary>
        /// Условия выполнения кнопки "Загрузить"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecuteBuildTreeCommand(object parameter)
        {
            if (SelectedDetailingProject == null || !IsListNullOrEmpty(Tree))
                return false;
            else
                return true;
        }

        #endregion


        void Clear<T>(ObservableCollection<T> list)
        {
            if (list != null)
            {
                list.Clear();
                list = null;
            }
        }
        /// <summary>
        /// Очистка коллекции
        /// </summary>
        protected override void OnDispose()
        {
            Clear(Tree);
            Clear(_detailingProjects);

            startRefObject = null;
            StartObject = null;
            SelectedDetailingProject = null;
        }
    }
}
