using LoadingWindow.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.Classes;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE.ViewModel;

namespace Wpf_SyncCompositionPE.Model
{
    public class SyncCompositionPE : System.ComponentModel.INotifyPropertyChanged
    {

        #region реализация интерфейса INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /*[System.Runtime.CompilerServices.CallerMemberName] - этот атрибут служит для того  не передавать имя переменной
         в параметр метода OnPropertyChanged, просто помещаем метод в set и set сам передаст имя нашей переменной
        */
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion

        #region свойства

        private ReferenceObject _startRefObject;

        /// <summary>
        /// Выбранный в TFLEX DOCS элемент проекта
        /// </summary>
        public ReferenceObject StartRefObject
        {
            get
            {
                if (_startRefObject == null)
                    _startRefObject = References.ProjectManagementReference.Find(_startRefObject.SystemFields.Guid);


                Console.WriteLine("Выбранный в TFLEX DOCS элемент проекта: {0}", _startRefObject);
                return _startRefObject;
            }
            set
            {
                _startRefObject = value;
                OnPropertyChanged();
                //OnPropertyChanged("StartRefObject");

                //nameof возвращает имя объекта
                //OnPropertyChanged(nameof(StartRefObject));
            }
        }


        /// <summary>
        /// Детализация выбранного в TFLEX DOCS элемента 
        /// </summary>
        public ReferenceObject StartObjectDetail
        {
            get
            {
                var d = References.ProjectManagementReference.Find(ЗависимостьПроектаДетализации[Synchronization.SynchronizationParameterGuids.param_SlaveWork_Guid].GetGuid());
                Console.WriteLine("Детализация выбранного в TFLEX DOCS элемента : {0}", d);
                return d;
            }
        }


        private List<ReferenceObject> _зависимостиСтартовогоЭлементаРП;

        /// <summary>
        /// Зависимости  выбранного в TFLEX DOCS элемента 
        /// </summary>
        public List<ReferenceObject> ЗависимостиСтартовогоЭлементаРП
        {
            get
            {
                if (HelperMethod.IsListNullOrEmpty(_зависимостиСтартовогоЭлементаРП))
                {
                    _зависимостиСтартовогоЭлементаРП = new List<ReferenceObject>();
                    _зависимостиСтартовогоЭлементаРП = Synchronization.GetDependenciesObjects(false, StartRefObject);
                }
                return _зависимостиСтартовогоЭлементаРП;
            }
        }

        private object _selectedDetailingProject;

        /// <summary>
        /// Выбранный в диалоге проект детализации
        /// </summary>
        public object SelectedDetailingProject
        {
            get
            {

                string SelectedName = string.Empty;

                //есть выбранный элемент в combobox
                if (_selectedDetailingProject != null)
                    SelectedName = _selectedDetailingProject.ToString();//получаем его наименование


                //если наименование не пустое
                if (!string.IsNullOrEmpty(SelectedName))
                {
                    if (!_detailingProjects.IsListNullOrEmpty())
                    {
                        _selectedDetailingProject = _detailingProjects.FirstOrDefault(d => d.ToString() == SelectedName);
                    }
                    else
                    {
                        _selectedDetailingProject = DetailingProjects.FirstOrDefault(d => d.ToString() == SelectedName);
           
                    }

                }
                else // нет наименования выбранного элемента проекта в combobox
                {

                }
                Console.WriteLine("Выбранный в диалоге проект детализации : {0}", _selectedDetailingProject);
                return _selectedDetailingProject;
            }
            set
            {
                if (_selectedDetailingProject == value) return;

                _selectedDetailingProject = value;

                //Tree = null;

                //if (value != null)
                //    StartWorkLoading(TreeBuild);

                //RaisePropertyChanged(nameof(SelectedDetailingProject));
            }
        }


        private ReferenceObject _зависимость;

        /// <summary>
        /// Зависимость выбранного в диалоге проекта детализации
        /// </summary>
        public ReferenceObject ЗависимостьПроектаДетализации
        {
            get
            {
                _зависимость = ЗависимостиСтартовогоЭлементаРП.FirstOrDefault(d => d[Synchronization.SynchronizationParameterGuids.param_SlaveProject_Guid].GetGuid() == (_selectedDetailingProject as ReferenceObject).SystemFields.Guid);
                return _зависимость;
            }
        }

        private bool _hasDetailingProjects = false;

        /// <summary>
        /// Выбранный элемент проекта в TFLEX DOCS имеет проеты детализации
        /// </summary>
        public bool HasDetailingProjects
        {
            get
            {
                return _hasDetailingProjects;
            }
        }


        private ObservableCollection<ReferenceObject> _detailingProjects;
        /// <summary>
        /// Получить проекты детализаций
        /// </summary>
        public ObservableCollection<ReferenceObject> DetailingProjects
        {
            get
            {
                if (_detailingProjects == null)
                    _detailingProjects = ProjectManagementWork.AllDetailingProjects(ЗависимостиСтартовогоЭлементаРП);

                _hasDetailingProjects = HelperMethod.IsListNullOrEmpty(_detailingProjects);

                Console.WriteLine("Получаем проекты детализации");
                foreach (var item in _detailingProjects)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(); Console.WriteLine();
                return _detailingProjects;
            }
        }

        #endregion

        public Worker Worker
        {
            get { return Worker.Instance; }
        }

        public void SynchronizingСomposition(TreeViewModel treeViewModel, bool IsCopyRes, bool IsCopyOnlyPlan/*, ref int amountAddObjects*//*, MainWindowViewModel mainWindowViewModel = null*/)
        {
            //if (MainWindowViewModel == null)
            //    MainWindowViewModel = mainWindowViewModel;

            if (Worker.Cancel) return;

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
    }

}
