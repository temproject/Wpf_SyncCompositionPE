using HierarchicalTreeControl.ViewModel;
using LoadingWindow.Model;
using System;
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

        private static int _instances;
        public static int Instances 
        {
            get
            {
                return _instances;
            }
        }

        /// <summary>
        /// Ссылка на экземпляр класса Worker
        /// </summary>
        public Worker Worker
        {
            get { return Worker.Instance; }
        }





        ~TreeViewModel()
        {
            Interlocked.Decrement(ref _instances);
        }

        ReferenceObject _projectElement;

        public ReferenceObject ProjectElement
        {
            get { return _projectElement; }
            set { _projectElement = value; }
        }

        static TreeViewModel()
        {
            _instances = 0;
        }



        public TreeViewModel(ReferenceObject projectElement, TreeViewModel parent, ReferenceObject startSelectBiggerPE = null/*, MainWindowViewModel mainWindowViewModel = null*/)
         : base(parent, false)
        {
            //если отмена, завершаем построение дерева
            if (Worker.Cancel) {  return; }

            #region вычисления процента построения дерева


            //при изменении номера итерации происходит пересчет процента выполнения
            //предварительно перед изменением номера итерации, нужно вычислить "количество итераций всего"
            Worker.CurrentNumberIterat++;

            if (Worker.Percent == "0%")
                Worker.numberAllIterat = projectElement.Children.RecursiveLoad().Count + 1;
            
           

            #endregion

            //увеличиваем счетчик количества свозданных ссылок на экземпляр
            Interlocked.Increment(ref _instances);

            if (startSelectBiggerPE != null)
                _startSelectBiggerPE = startSelectBiggerPE;

            ProjectElement = projectElement;

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

        #region свойства

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
                       clearAllCheckboxes(this);
                    }
                    else
                    {
                        MarkAllParents(this.Parent as TreeViewModel);
                    }
                }
                RaisePropertyChanged("IsObjectToSync");

            }
        }

        bool? _isSelectObjToSynch = null;

        /// <summary>
        /// Этот объект можно выбрать для синхронизации
        /// С помощью чекбокса
        /// <summary>
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
        /// В дереве содержаться данные для синхронизации
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



        #endregion

        #region работа с чекбоксом

        /// <summary>
        /// Снять галочку в дочернего элемента, если сняли с родителя
        /// </summary>
        /// <param name="data"></param>
        public void clearAllCheckboxes(TreeViewModel data)
        {
            if (data == null) return;

            if ((bool)data.IsSelectObjToSynch == false) return;

            if ((bool)data.IsObjectToSync == true)
                data.IsObjectToSync = false;

            foreach (var child in data.Children.OfType<TreeViewModel>())
                clearAllCheckboxes(child);
        }

        /// <summary>
        /// Проставить галочку родителю, если проставили дочернему
        /// </summary>
        /// <param name="data"></param>
        public void MarkAllParents(TreeViewModel data)
        {
            if (data == null) { return; }

            if ((bool)data.IsSelectObjToSynch == false) { return; }

            if ((bool)data.IsObjectToSync == false)
                data.IsObjectToSync = true;

            MarkAllParents(data.Parent as TreeViewModel);
        }
        #endregion

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

      
        //public int Id
        //{
        //    get { return _element.Id; }
        //    set { _element.Id = value; }
        //}



        public override string ToString()
        {
            return Name;
        }
    }
}
