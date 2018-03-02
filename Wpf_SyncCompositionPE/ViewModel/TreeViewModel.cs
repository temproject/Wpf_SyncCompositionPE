using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;
using WpfApp_TreeSyncCompositionWork.Model;

namespace Wpf_SyncCompositionPE.ViewModel
{
    public class TreeViewModel : TreeViewItemViewModel
    {
        ReferenceObject _projectElement;

        public ReferenceObject ProjectElement
        {
            get { return _projectElement; }
            set { _projectElement = value; }
        }

        public TreeViewModel(ReferenceObject projectElement, TreeViewModel parent)
         : base(parent, true)
        {
            ProjectElement = projectElement;

            if (parent == null)
                IsObjectToSync = false;

            // Начинаем обрабатывать элементы начиная с корня.
            LoadChildren();
        }
        protected override void LoadChildren()
        {
            foreach (var child in ProjectElement.Children)
            {
                var d = new TreeViewModel(child, this);
                base.Children.Add(d);
                d.IsExpanded = true;
            }
        }

        public override string ToString()
        {
            return Name;
        }


        private List<ReferenceObject> укрупнения;

        public List<ReferenceObject> Укрупнения
        {
            get
            {
                if (укрупнения == null)
                    укрупнения = new List<ReferenceObject>();

                укрупнения = Synchronization.GetSynchronizedWorksFromSpace(ProjectElement, null, true);
                return укрупнения;
            }
            set
            {
                if (укрупнения == value) return;
                укрупнения = value;
            }
        }


        static private ReferenceObject startSelectBiggerPE;

        /// <summary>
        /// Выбранный элемент проекта с которого запускается диалог
        /// </summary>
        public ReferenceObject StartSelectBiggerPE
        {
            get { return startSelectBiggerPE; }
            set
            {

                startSelectBiggerPE = value;
                if (_projectStartSelectBiggerPE == null)
                    _projectStartSelectBiggerPE = ProjectStartSelectBiggerPE;
            }
        }

        private ReferenceObject _projectStartSelectBiggerPE;

        /// <summary>
        /// Проект РП
        /// </summary>
        public ReferenceObject ProjectStartSelectBiggerPE
        {
            get
            {
                if (_projectStartSelectBiggerPE == null)
                {
                    if (StartSelectBiggerPE.Class.Name != "Проект")
                        _projectStartSelectBiggerPE = ProjectManagementWork.GetProject(StartSelectBiggerPE);
                    else
                        _projectStartSelectBiggerPE = StartSelectBiggerPE;

                    return _projectStartSelectBiggerPE;
                }

                return _projectStartSelectBiggerPE;
            }
        }

        /// <summary>
        /// Укрупнение с которым нужно синхронизировать
        /// </summary>
        public ReferenceObject PEForSync { get; set; }

        void check()
        {



            //если дочерний элементе детализации не синхронизирован укрупнением родительского элемента
            //то такой дочерний элемент доступен для синхронизации 
            if ((MainWindowViewModel.IsListNullOrEmpty(Укрупнения) ||
              !Укрупнения.Any(pmw => ProjectManagementWork.GetProject(pmw) == ProjectStartSelectBiggerPE)))
            {


                if (!ProjectElement.Children.Any(ch => ch == ProjectStartSelectBiggerPE))
                {
                    ContainsObjSync = true;

                    _isObjectToSync = true;
                }


                List<ReferenceObject> УкрупненияДетализации = Synchronization.GetSynchronizedWorksFromSpace(ProjectElement.Parent, null, true);

                if (!MainWindowViewModel.IsListNullOrEmpty(УкрупненияДетализации))
                    PEForSync = УкрупненияДетализации.FirstOrDefault(pe => ProjectManagementWork.GetProject(pe) == ProjectStartSelectBiggerPE);
            }
            else
            {
                _isObjectToSync = false;
            }
        }

        private bool? _isObjectToSync = null;

        /// <summary>
        /// Флаг элемента проекта который нужно синхронизировать
        /// </summary>
        public bool? IsObjectToSync
        {
            get
            {
                if (_isObjectToSync == null)
                    check();

                return _isObjectToSync;
            }
            set
            {
                OnPropertyChanged("IsObjectToSync");
                _isObjectToSync = value;
            }
        }

        /// <summary>
        /// Соддержит данные для синхронизации
        /// </summary>
        public bool ContainsObjSync { get; set; }


        private string visibility = "Collapsed";

        /// <summary>
        /// Флаг отвечающий за отображение чекбокса в дереве
        /// </summary>
        public string Visibility
        {
            get
            {
                if ((bool)IsObjectToSync)
                    visibility = "Visible";
                else
                    visibility = "Collapsed";

                return visibility;
            }
        }

        //public int Id
        //{
        //    get { return _element.Id; }
        //    set { _element.Id = value; }
        //}




        public string PathIcon
        {
            get
            {
                if (IsProject)
                    return @"C:\Users\Kachkov\Source\Repos\First\WpfApp_TreeSyncCompositionWork\WpfApp_TreeSyncCompositionWork\Images\Project.ico";
                else
                {
                    return @"C:\Users\Kachkov\Source\Repos\First\WpfApp_TreeSyncCompositionWork\WpfApp_TreeSyncCompositionWork\Images\PE.ico";
                }
            }

        }

        bool? _IsProject;
        public bool IsProject
        {
            get
            {

                if (_IsProject == null)
                {
                    _IsProject = this.ProjectElement.Class.IsInherit(ProjectManagementWork.PM_class_Project_Guid);
                }
                return (bool)_IsProject;
            }
        }

        ProjectManagementWork _ParentWork;
        public ProjectManagementWork ParentWork
        {
            get
            {
                if (_ParentWork == null && this.ProjectElement.Parent != null)
                    if (this.ProjectElement.Parent.Class.IsInherit(ProjectManagementWork.PM_class_ProjectElement_Guid))
                        _ParentWork = new ProjectManagementWork/* Factory.Create_ProjectManagementWork*/(this.ProjectElement.Parent);
                return _ParentWork;
            }
        }

        string _Number;

        /// <summary>
        /// номер работы
        /// </summary>
        public string Number
        {
            get
            {
                if (this.IsProject || this.ParentWork == null)
                { _Number = ""; }
                else if (this.ParentWork != null)
                {
                    if (string.IsNullOrWhiteSpace(this.ParentWork.Number))
                        _Number = this.ProjectElement.SystemFields.Order.ToString();
                    else
                        _Number = this.ParentWork.Number + "." + this.ProjectElement.SystemFields.Order.ToString();
                }
                return _Number;
            }
        }

        private string _class;

        public string Class
        {
            get
            {

                if (string.IsNullOrEmpty(_class))
                    if (ProjectElement != null)
                        _class = ProjectElement.Class.Name;

                return _class;
            }

            set { _name = value; }
        }

        private string _name;


        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    if (this.ProjectElement != null)
                        _name = this.ProjectElement[ProjectManagementWork.PM_param_Name_GUID].GetString();
                    else _name = "null";
                }

                return _name;
            }
            set
            {
                if (_name != value)
                    _name = value;
            }
        }
    }
}
