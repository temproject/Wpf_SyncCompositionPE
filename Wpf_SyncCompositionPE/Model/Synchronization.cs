using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Classes;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Search;
using TFlex.DOCs.References.ProjectManagement;
using Wpf_SyncCompositionPE.ViewModel;
using Wpf_SyncCompositionPE.Model;
using System.Threading;

namespace Wpf_SyncCompositionPE.Model
{
    /// <summary>
    /// Для работы с синхронизацией в УП
    /// ver 1.3
    /// </summary>
    public class Synchronization
    {


        /// <summary>
        /// Возвращает список синхронизированных работ из указанного пространства планирования
        /// </summary>
        /// <param name="work"></param>
        /// <param name="planningSpaceGuidString"></param>
        /// <param name="returnOnlyMasterWorks"> если true только укрупнения, если false только детализации</param>
        /// <returns></returns>
        public static List<ProjectManagementWork> GetSynchronizedWorksInProject(ProjectManagementWork work, ProjectManagementWork project,
            bool? returnOnlyMasterWorks = null)
        {
            string guidWorkForSearch = work.Guid.ToString();
            string guidProjectForSearch = project.Guid.ToString();

            Filter filter = new Filter(ProjectDependenciesReferenceInfo);

            AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_DependencyType_Guid, 5, (int)LogicalOperator.And);
            // Условие: в названии содержится "является укрупнением"
            //ReferenceObjectTerm termMasterWork = new ReferenceObjectTerm(LogicalOperator.Or);
            ReferenceObjectTerm termMasterWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_MasterWork_Guid, guidWorkForSearch);

            ReferenceObjectTerm termSlaveProjectWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_SlaveProject_Guid,
                guidProjectForSearch, (int)LogicalOperator.And);

            // Условие: в названии содержится "является детализацией"
            ReferenceObjectTerm termSlaveWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_SlaveWork_Guid,
                guidWorkForSearch, (int)LogicalOperator.Or);

            ReferenceObjectTerm termMasterProjectWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_MasterProject_Guid,
                guidProjectForSearch, (int)LogicalOperator.And);

            // Группируем условия в отдельную группу (другими словами добавляем скобки)
            TermGroup group1 = filter.Terms.GroupTerms(new Term[] { termMasterWork, termSlaveProjectWork });
            TermGroup group2 = filter.Terms.GroupTerms(new Term[] { termSlaveWork, termMasterProjectWork });

            //редактируем при необходимости
            if (returnOnlyMasterWorks != null)
            {
                if ((bool)returnOnlyMasterWorks) group1.Clear();
                else group2.Clear();
            }

            //Получаем список объектов, в качестве условия поиска – сформированный фильтр
            List<ReferenceObject> DependencyObjects = ProjectDependenciesReference.Find(filter);

            List<ProjectManagementWork> result = new List<ProjectManagementWork>();

            foreach (var item in getListGuidObjectsByFilter(DependencyObjects, work))
            {
                ProjectManagementWork tempWork = new ProjectManagementWork(item);
                result.Add(new ProjectManagementWork(item));

            }
            return result;


        }


        /// <summary>
        /// Use GetSyncronizedWorks
        /// Возвращает список синхронизированных работ из указанного пространства планирования
        /// </summary>
        /// <param name="work"></param>
        /// <param name="planningSpaceGuidString"></param>
        /// <param name="returnOnlyMasterWorks"> если true только укрупнения, если false только детализации</param>
        /// <returns></returns>
        public static List<ReferenceObject> GetSynchronizedWorksFromSpace(ProjectManagementWork work,
            string planningSpaceGuidString, bool? returnOnlyMasterWorks = null)
        {
            if (string.IsNullOrEmpty(planningSpaceGuidString))
                planningSpaceGuidString = Guid.Empty.ToString();

            return GetSynchronizedWorksFromSpace(work.ReferenceObject, planningSpaceGuidString, returnOnlyMasterWorks);
        }

        /// <summary>
        /// Use GetSyncronizedWorks
        /// Возвращает список синхронизированных работ из указанного пространства планирования
        /// </summary>
        /// <param name="work"></param>
        /// <param name="planningSpaceGuidString"></param>
        /// <param name="returnOnlyMasterWorks"> если true только укрупнения, если false только детализации</param>
        /// <returns></returns>
        public static List<ReferenceObject> GetSynchronizedWorksFromSpace(ReferenceObject work,
            string planningSpaceGuidString, bool? returnOnlyMasterWorks = null)
        {
            if (string.IsNullOrEmpty(planningSpaceGuidString))
                planningSpaceGuidString = Guid.Empty.ToString();

            string guidStringForSearch = work.SystemFields.Guid.ToString();
            List<ReferenceObject> DependenciesObjects = GetDependenciesObjects(returnOnlyMasterWorks, guidStringForSearch);

            List<ReferenceObject> result = new List<ReferenceObject>();
            foreach (var guidPE in getListGuidObjectsByFilter(DependenciesObjects, work))
            {
                var PE = References.ProjectManagementReference.Find(guidPE);

                if (PE == null) { continue; }

                ProjectManagementWork tempWork = new ProjectManagementWork(PE);

                if (tempWork.PlanningSpace.ToString() == planningSpaceGuidString)
                {
                    result.Add(References.ProjectManagementReference.Find(guidPE));
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает список синхронизированных работ из указанного пространства планирования
        /// </summary>
        /// <param name="work"></param>
        /// <param name="planningSpaceGuidString"></param>
        /// <param name="returnOnlyMasterWorks"> если true только укрупнения, если false только детализации</param>
        /// <returns></returns>
        public static List<ReferenceObject> GetSynchronizedWorks(ProjectManagementWork work, bool? returnOnlyMasterWorks)
        {
            return GetSynchronizedWorks(work.ReferenceObject, returnOnlyMasterWorks);

        }

        /// <summary>
        /// Возвращает список синхронизированных работ из указанного пространства планирования
        /// </summary>
        /// <param name="work"></param>
        /// <param name="planningSpaceGuidString"></param>
        /// <param name="returnOnlyMasterWorks"> если true только укрупнения, если false только детализации</param>
        /// <returns></returns>
        public static List<ReferenceObject> GetSynchronizedWorks(ReferenceObject work, bool? returnOnlyMasterWorks)
        {
            //if (string.IsNullOrEmpty(planningSpaceGuidString))
            //    planningSpaceGuidString = Guid.Empty.ToString();

            string guidStringForSearch = work.SystemFields.Guid.ToString();

            //Получаем список объектов, в качестве условия поиска – сформированный фильтр
            List<ReferenceObject> DependencyObjects = GetDependenciesObjects(returnOnlyMasterWorks, guidStringForSearch);

            List<ReferenceObject> result = new List<ReferenceObject>();
            //bool isNeedToFilterByPlanningSpace = !string.IsNullOrWhiteSpace(planningSpaceGuidString);
            foreach (var guid in getListGuidObjectsByFilter(DependencyObjects, work))
            {
                ProjectManagementWork tempWork = new ProjectManagementWork(guid);
                //if (isNeedToFilterByPlanningSpace && tempWork.PlanningSpace.ToString() != planningSpaceGuidString)
                //{
                //    continue;
                //}
                result.Add(References.ProjectManagementReference.Find(guid));

            }
            return result;

        }

        /// <summary>
        /// Получаем список объектов, в качестве условия поиска – сформированный фильтр
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        static private List<Guid> getListGuidObjectsByFilter(List<ReferenceObject> DependencyObjects, ProjectManagementWork work)
        {
            return getListGuidObjectsByFilter(DependencyObjects, work.ReferenceObject);
        }
        /// <summary>
        /// Получаем список объектов, в качестве условия поиска – сформированный фильтр
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        static private List<Guid> getListGuidObjectsByFilter(List<ReferenceObject> DependencyObjects, ReferenceObject work)
        {

            List<Guid> listSyncWorkGuids = new List<Guid>();
            //#if test
            //            System.Windows.Forms.MessageBox.Show(filter.ToString() + "\nlistObj.Count = " + listObj.Count.ToString());
            //#endif
            foreach (var dependencyObject in DependencyObjects) // выбираем все отличные от исходной работы guids
            {
                Guid slaveGuid = dependencyObject[SynchronizationParameterGuids.param_SlaveWork_Guid].GetGuid();
                Guid masterGuid = dependencyObject[SynchronizationParameterGuids.param_MasterWork_Guid].GetGuid();

                if (slaveGuid != work.SystemFields.Guid)
                {
                    listSyncWorkGuids.Add(slaveGuid);
                }
                if (masterGuid != work.SystemFields.Guid)
                {
                    listSyncWorkGuids.Add(masterGuid);
                }
            }
            listSyncWorkGuids = listSyncWorkGuids.Distinct().ToList();
            return listSyncWorkGuids;
        }

        /// <summary>
        /// Получить список зависимостей типа синхранизация
        /// </summary>
        /// <param name="returnOnlyMasterWorks">true - список зависимостей (укрупнение)</param>
        /// <param name="work"></param>
        /// <returns></returns>
        public static List<ReferenceObject> GetDependenciesObjects(bool? returnOnlyMasterWorks, ProjectManagementWork work)
        {
            return GetDependenciesObjects(returnOnlyMasterWorks, work.Guid.ToString());
        }
        /// <summary>
        /// Получить список зависимостей типа синхранизация
        /// </summary>
        /// <param name="returnOnlyMasterWorks">true - список зависимостей (укрупнение)</param>
        /// <param name="work"></param>
        /// <returns></returns>
        public static List<ReferenceObject> GetDependenciesObjects(bool? returnOnlyMasterWorks, ReferenceObject work)
        {
            return GetDependenciesObjects(returnOnlyMasterWorks, work.SystemFields.Guid.ToString());
        }

        //static MainWindowViewModel MainWindowViewModel = null;

        public static void SynchronizingСomposition(TreeViewModel treeViewModel, bool IsCopyRes, bool IsCopyOnlyPlan/*, ref int amountAddObjects*//*, MainWindowViewModel mainWindowViewModel = null*/)
        {
            //if (MainWindowViewModel == null)
            //    MainWindowViewModel = mainWindowViewModel;

            ReferenceObject Parent = treeViewModel.PEForSync;

            if (Parent != null)
                Parent = syncComposition(treeViewModel, IsCopyRes : IsCopyRes, IsCopyOnlyPlan : IsCopyOnlyPlan/*, ref amountAddObjects*/);

            foreach (TreeViewModel treeViewModelItem in treeViewModel.Children)
            {
                if (Parent != null && treeViewModelItem.PEForSync == null)
                {
                    treeViewModelItem.PEForSync = Parent;
                }
                SynchronizingСomposition(treeViewModelItem, IsCopyRes: IsCopyRes, IsCopyOnlyPlan: IsCopyOnlyPlan/*, ref amountAddObjects*/);
            }
        }

        static private ReferenceObject syncComposition(TreeViewModel pe_treeItem, bool IsCopyRes, bool IsCopyOnlyPlan/*, ref int amountAddObjects*/)
        {

            #region
            if ((bool)pe_treeItem.IsObjectToSync)
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

                    SyncronizeWorks(newPE, pe_treeItem.ProjectElement);

                    return newPE;
                }
            }
            #endregion
            return null;
        }

        //создаём синхронизацию
        public static void SyncronizeWorks(ProjectManagementWork masterWork, ProjectManagementWork slaveWork)
        {
            SyncronizeWorks(masterWork.ReferenceObject, slaveWork.ReferenceObject);
        }

        //создаём синхронизацию
        public static void SyncronizeWorks(ReferenceObject masterWork, ReferenceObject slaveWork)
        {
            ReferenceObject синхронизация = ProjectDependenciesReference.CreateReferenceObject(Class_ProjectDependency);
            синхронизация[SynchronizationParameterGuids.param_DependencyType_Guid].Value = 5;// DependencyType.Synchronization;
            синхронизация[SynchronizationParameterGuids.param_MasterWork_Guid].Value = masterWork.SystemFields.Guid;
            синхронизация[SynchronizationParameterGuids.param_SlaveWork_Guid].Value = slaveWork.SystemFields.Guid;
            синхронизация[SynchronizationParameterGuids.param_MasterProject_Guid].Value = ProjectManagementWork.GetProject(masterWork).SystemFields.Guid;
            синхронизация[SynchronizationParameterGuids.param_SlaveProject_Guid].Value = ProjectManagementWork.GetProject(slaveWork).SystemFields.Guid;
            синхронизация.EndChanges();
            //return синхронизация;
        }


        public static List<ReferenceObject> GetDependenciesObjects(bool? returnOnlyMasterWorks, string guidStringForSearch)
        {
            if (string.IsNullOrEmpty(guidStringForSearch))
                guidStringForSearch = Guid.Empty.ToString();

            Filter filter = new Filter(ProjectDependenciesReferenceInfo);
            AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_DependencyType_Guid, 5, (int)LogicalOperator.And);

            // Условие: в названии содержится "является укрупнением"
            //ReferenceObjectTerm termMasterWork = new ReferenceObjectTerm(LogicalOperator.Or);

            ReferenceObjectTerm termMasterWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_MasterWork_Guid, guidStringForSearch);

            // Условие: в названии содержится "является детализацией"
            ReferenceObjectTerm termSlaveWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_SlaveWork_Guid,
                guidStringForSearch, (int)LogicalOperator.Or);

            // Группируем условия в отдельную группу (другими словами добавляем скобки)
            TermGroup group1 = filter.Terms.GroupTerms(new Term[] { termMasterWork, termSlaveWork });

            //редактируем при необходимости
            if (returnOnlyMasterWorks != null)
            {
                if (!(bool)returnOnlyMasterWorks) group1.Remove(termSlaveWork);
                else group1.Remove(termMasterWork);
            }

            return ProjectDependenciesReference.Find(filter);

        }



        /// <summary>
        /// Удаляет синхронизацию между указанными работами
        /// </summary>
        /// <param name="masterWork"></param>
        /// <param name="slaveWork"></param>
        public static void DeleteSynchronisationBetween(ProjectManagementWork masterWork, ProjectManagementWork slaveWork)
        {

            Filter filter = new Filter(ProjectDependenciesReferenceInfo);

            AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_DependencyType_Guid, 5, (int)LogicalOperator.And);

            // Условие: в названии содержится "является укрупнением"
            string masterWorkGuidStringForSearch = masterWork.Guid.ToString();

            // Условие: в названии содержится "является детализацией"
            string slaveWorkGuidStringForSearch = slaveWork.Guid.ToString();

            ReferenceObjectTerm termSlaveWork = AddTermByGuidParamPE(ref filter, SynchronizationParameterGuids.param_SlaveWork_Guid,
                slaveWorkGuidStringForSearch, (int)LogicalOperator.And);

            //Получаем список объектов, в качестве условия поиска – сформированный фильтр
            List<ReferenceObject> listObj = ProjectDependenciesReference.Find(filter);
            //#if test
            //            MessageBox.Show(filter.ToString() + "\nlistObj.Count = " + listObj.Count.ToString());
            //#endif
            foreach (var item in listObj) // удаляем всё что нашли
            {
                item.Delete();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="guidStringForSearch"></param>
        /// <param name="lOperator">(-1) - null, 0- and, 1 = or</param>
        /// <returns></returns>
        private static ReferenceObjectTerm AddTermByGuidParamPE<T>(ref Filter filter, Guid ParamPE, T value, int lOperator = -1)
        {
            ReferenceObjectTerm termPE = null;

            if (lOperator == -1)
                termPE = new ReferenceObjectTerm(filter.Terms);
            else if (lOperator == 0)
                termPE = new ReferenceObjectTerm(filter.Terms, LogicalOperator.And);
            else if (lOperator == 1)
                termPE = new ReferenceObjectTerm(filter.Terms, LogicalOperator.Or);

            // устанавливаем параметр
            termPE.Path.AddParameter(ProjectDependenciesReference.ParameterGroup.OneToOneParameters.Find(ParamPE));
            // устанавливаем оператор сравнения
            termPE.Operator = ComparisonOperator.Equal;
            // устанавливаем значение для оператора сравнения
            termPE.Value = value;

            return termPE;
        }

        private static ClassObject _Class_ProjectDependency;
        /// <summary>
        /// класс Зависимости проектов
        /// </summary>
        public static ClassObject Class_ProjectDependency
        {
            get
            {
                if (_Class_ProjectDependency == null)
                    _Class_ProjectDependency = ProjectDependenciesReference.Classes.Find(SynchronizationParameterGuids.class_Dependency_GUID);

                return _Class_ProjectDependency;
            }
        }

        static Reference _ProjectDependenciesReference;
        /// <summary>
        /// Справочник "Зависимости проектов"
        /// </summary>
        public static Reference ProjectDependenciesReference
        {
            get
            {
                if (_ProjectDependenciesReference == null)
                    _ProjectDependenciesReference = GetReference(ref _ProjectDependenciesReference, ProjectDependenciesReferenceInfo);

                return _ProjectDependenciesReference;
            }
        }

        static ReferenceInfo _ProjectDependenciesReferenceInfo;
        /// <summary>
        /// Справочник "Зависимости проектов"
        /// </summary>
        private static ReferenceInfo ProjectDependenciesReferenceInfo
        {
            get { return GetReferenceInfo(ref _ProjectDependenciesReferenceInfo, SynchronizationParameterGuids.ref_ProjDependencies_Guid); }
        }

        private static Reference GetReference(ref Reference reference, ReferenceInfo referenceInfo)
        {
            if (reference == null)
                reference = referenceInfo.CreateReference();


            return reference;
        }
        private static ReferenceInfo GetReferenceInfo(ref ReferenceInfo referenceInfo, Guid referenceGuid)
        {
            if (referenceInfo == null)
                referenceInfo = References.Connection.ReferenceCatalog.Find(referenceGuid);

            return referenceInfo;
        }
        public struct SynchronizationParameterGuids
        {
            #region Справочник - "Зависимости проектов"
            /// <summary>
            /// Guid справочника - "Зависимости проектов"
            /// </summary>
            public static readonly Guid ref_ProjDependencies_Guid = new Guid("e13cee45-39fa-43ff-ba2a-957294d975bf");
            /// <summary>
            /// Guid типа "Зависимости проектов" справочника - "Зависимости проектов"
            /// </summary>
            public static readonly Guid class_Dependency_GUID = new Guid("0ed0174e-5392-460f-8b53-5a2e52c26f9b");
            /// <summary>
            /// Guid параметра "Проект 1" (укрупнение)
            /// </summary>
            public static readonly Guid param_MasterProject_Guid = new Guid("087648ca-6269-4e7f-8e5a-9fabbab5fafd");
            /// <summary>
            /// Guid параметра "Работа 1" (укрупнение)
            /// </summary>
            public static readonly Guid param_MasterWork_Guid = new Guid("17feb793-7656-475a-b7b0-61672ba813fa");
            /// <summary>
            /// Guid параметра "Проект 2" (детализация)
            /// </summary>
            public static readonly Guid param_SlaveProject_Guid = new Guid("a5590629-9469-4c39-87e6-169b60678abb");
            /// <summary>
            /// Guid параметра "Работа 2" (детализация)          
            /// </summary>
            public static readonly Guid param_SlaveWork_Guid = new Guid("5baf8272-f639-4fc8-af65-6f0af32647fd");
            /// <summary>
            /// Guid параметра "Тип зависимости"
            /// </summary>
            public static readonly Guid param_DependencyType_Guid = new Guid("45bc3bf0-88cf-4b46-b951-b1e07a5d7fcb");
            #endregion
        }
    }
}
