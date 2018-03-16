using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE.Model;
using Wpf_SyncCompositionPE.Properties;

namespace Wpf_SyncCompositionPE.ViewModel
{
    public class TreeViewModel : TreeViewItemViewModel
    {

        public static int GetActiveInstances()
        {
            return Instances;
        }

        static public int Instances = 0;
        //static public double PercentTreeBuild = 0;
        //static public int AmountAllItemsTree = 0;

        ~TreeViewModel()
        {
            Interlocked.Decrement(ref Instances);
            //if (MainWindowViewModel != null)
            //    MainWindowViewModel.InstanceNumber = Instances;
        }

        ReferenceObject _projectElement;

        public ReferenceObject ProjectElement
        {
            get { return _projectElement; }
            set { _projectElement = value; }
        }

        //static public MainWindowViewModel MainWindowViewModel;

        static TreeViewModel()
        {
            //MainWindowViewModel = null;
            Instances = 0;
            //PercentTreeBuild = 0;
            //AmountAllItemsTree = 0;
        }

        public TreeViewModel(ReferenceObject projectElement, TreeViewModel parent, ReferenceObject startSelectBiggerPE = null/*, MainWindowViewModel mainWindowViewModel = null*/)
         : base(parent, false)
        {

            //if (MainWindowViewModel == null)
            //    MainWindowViewModel = mainWindowViewModel;

            //if (Instances == 0)
            //    AmountAllItemsTree = projectElement.Children.RecursiveLoad().Count;

            //var percent = Math.Round(100.0 * Instances / AmountAllItemsTree);

            //PercentTreeBuild = percent == 0 ? 1 : percent;

            //MainWindowViewModel.Percent = PercentTreeBuild;

            if (startSelectBiggerPE != null)
                _startSelectBiggerPE = startSelectBiggerPE;

            ProjectElement = projectElement;

            Interlocked.Increment(ref Instances);

            // Начинаем обрабатывать элементы начиная с корня.
            //if (ProjectElement != projectElement)
            LoadChildren();

            if (parent == null && this.IsExpanded == false)
                ContainsObjSync = false;
        }

        //protected async override void LoadChildren()
        //{
        //    foreach (TreeViewModel space in await Task.Run(() => ProjectElement.Children)))
        //        base.Childrens.Add(new SpaceObjectViewModel(space, this));
        //}

        protected override void LoadChildren()
        {
            this.IsExpanded = (bool)this.IsSelectObjToSynch;

            foreach (var child in/* await Task.Run(() => */ProjectElement.Children)
                base.Children.Add(new TreeViewModel(child, this));

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

                укрупнения = Synchronization.GetSynchronizedWorks(ProjectElement, true);

                return укрупнения;
            }
            set
            {
                if (укрупнения == value) return;
                укрупнения = value;
            }
        }


        static private ReferenceObject _startSelectBiggerPE;

        /// <summary>
        /// Выбранный элемент проекта с которого запускается диалог
        /// </summary>
        public ReferenceObject StartSelectBiggerPE
        {
            get { return _startSelectBiggerPE; }
            set
            {
                if (_startSelectBiggerPE == value) return;

                _startSelectBiggerPE = value;

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
                //if (_projectStartSelectBiggerPE == null)
                {
                    if (StartSelectBiggerPE.Class.Name != "Проект")
                        _projectStartSelectBiggerPE = ProjectManagementWork.GetProject(StartSelectBiggerPE);
                    else
                        _projectStartSelectBiggerPE = StartSelectBiggerPE;

                    return _projectStartSelectBiggerPE;
                }
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
            if ((HelperMethod.IsListNullOrEmpty(Укрупнения) ||
              !Укрупнения.Any(pmw => ProjectManagementWork.GetProject(pmw) == ProjectStartSelectBiggerPE)))
            {

                if (!ProjectElement.Children.Any(ch => ch == ProjectStartSelectBiggerPE))
                {
                    _isSelectObjToSynch = true;
                    containsObjSync = true;
                    _isObjectToSync = true;
                }

                List<ReferenceObject> УкрупненияДетализации = Synchronization.GetSynchronizedWorks(ProjectElement.Parent, true);

                if (!HelperMethod.IsListNullOrEmpty(УкрупненияДетализации))
                    PEForSync = УкрупненияДетализации.FirstOrDefault(pe => ProjectManagementWork.GetProject(pe) == ProjectStartSelectBiggerPE);
            }
            else
            {
                _isSelectObjToSynch = false;
                _isObjectToSync = false;
            }
        }

        bool _isObjectToSync = false;

        /// <summary>
        /// Объект для синхронизации
        /// </summary>
        public bool IsObjectToSync
        {
            get
            {
                return _isObjectToSync;
            }
            set
            {
                if (_isObjectToSync == value) return;

                _isObjectToSync = value;

                if ((bool)IsSelectObjToSynch)
                {
                    if (false == _isObjectToSync)
                    {
                        base.clearAllCheckboxes(this);
                    }
                    else
                    {
                        base.MarkAllParents(this.Parent as TreeViewModel);
                    }
                }
                RaisePropertyChanged("IsObjectToSync");

            }
        }

        bool? _isSelectObjToSynch = null;

        /// <summary>
        /// Этот объект можно выбрать для синхронизации
        public bool? IsSelectObjToSynch
        {
            get
            {
                if (_isSelectObjToSynch == null)
                    check();

                return _isSelectObjToSynch;
            }
            set
            {

                if (_isSelectObjToSynch == value) return;

                _isSelectObjToSynch = value;

                RaisePropertyChanged(nameof(IsSelectObjToSynch));
            }
        }

        static bool containsObjSync = false;
        /// <summary>
        /// Соддержит данные для синхронизации
        /// </summary>
        public bool ContainsObjSync
        {
            get
            {
                return containsObjSync;
            }
            set
            {

                containsObjSync = value;

                RaisePropertyChanged(nameof(ContainsObjSync));
            }
        }

        private string visibilityCheckBox = "Collapsed";

        /// <summary>
        /// Флаг отвечающий за отображение чекбокса в дереве
        /// </summary>
        public string VisibilityCheckBox
        {
            get
            {
                bool ShowCheckBox = (bool)IsSelectObjToSynch;//нужно отобразить чекбокс

                if (ShowCheckBox == true)//нужно отобразить - показываем 
                    visibilityCheckBox = "Visible";
                //else if (this.HasChildren)
                //    visibilityCheckBox = "Collapsed";
                else
                    visibilityCheckBox = "Collapsed";

                return visibilityCheckBox;
            }
            //set
            //{
            //    visibilityCheckBox = value;
            //    RaisePropertyChanged(nameof(VisibilityCheckBox));
            //}
        }

        //public int Id
        //{
        //    get { return _element.Id; }
        //    set { _element.Id = value; }
        //}


        private ImageSource _pathIcon = null;

        public ImageSource PathIcon
        {
            get
            {
                if (_pathIcon == null)
                {
                    Icon icon = Resources.PE;

                    if (IsProject)
                        icon = Resources.project;

                    //Bitmap bitmap = icon.ToBitmap();
                    //IntPtr hBitmap = bitmap.GetHbitmap();

                    //ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    //    hBitmap,
                    //    IntPtr.Zero,
                    //    Int32Rect.Empty,
                    //    BitmapSizeOptions.FromEmptyOptions());
                    //return wpfBitmap;
                    using (Bitmap bmp = icon.ToBitmap())
                    {
                        var stream = new MemoryStream();
                        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        _pathIcon = BitmapFrame.Create(stream);
                    }


                }
                return _pathIcon;
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


        public new string Name
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
