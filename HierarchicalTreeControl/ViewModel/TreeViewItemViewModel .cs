using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf_SyncCompositionPE.ViewModel;

namespace HierarchicalTreeControl.ViewModel
{
    /// <summary>
    /// Базовый класс для всех классов ViewModel, отображаемых TreeViewItems.
    /// Это действует как адаптер между необработанным объектом данных и TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase
    {
        #region Data

        static TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        readonly ObservableCollection<TreeViewItemViewModel> _children;

        readonly TreeViewItemViewModel _parent;

        bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Constructors



        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren = false)
        {
            _parent = parent;

            _children = new ObservableCollection<TreeViewItemViewModel>();

       
            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }

        // Этот конструктор используется для создания экземпляра DummyChild.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members


        #region Children

        /// <summary>
        /// Возвращает логические дочерние элементы этого объекта.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Возвращает true, если дети этого объекта еще не заполнены.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        #endregion // HasLoadedChildren

        /// <summary>
        /// Возвращает true, если имеются дети
        /// </summary>
        public bool HasChildren
        {
            get { return this.Children.Count > 0 && this.Children[0] != DummyChild; }
        }

        #region IsExpanded

        /// <summary>
        /// Получает / задает, является ли TreeViewItem
        /// связанный с этим объектом.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {

                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.RaisePropertyChanged(nameof(IsExpanded));
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        #endregion // IsExpanded

      
        #region IsSelected

        /// <summary>
        /// Является ли TreeViewItem
        /// выбранным объектом.
        /// </summary>
        public bool IsSelected
        {
            get {   return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                
                    this.RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }

        #endregion // IsSelected


        //#region SelectedItem

        ///// <summary>
        ///// Выбранный объект
        ///// </summary>
        //public TreeViewItemViewModel SelectedItem
        //{
        //    get
        //    {
        //        if (_selectedItem != null)
        //            Console.WriteLine(((TreeViewModel)(_selectedItem)).Name); return _selectedItem; }
        //    set
        //    {
        //        if (value != _selectedItem)
        //        {
        //            _selectedItem = value;
        //            if (value != null)
        //                Console.WriteLine(((TreeViewModel)(value)).Name);
        //            RaisePropertyChanged(nameof(SelectedItem));
        //        }
        //    }
        //}

        //#endregion // SelectedItem


        #region LoadChildren

        /// <summary>
        /// Вызывается, когда дочерние элементы необходимо загружать по требованию.
        /// Подклассы могут переопределить это, чтобы заполнить коллекцию Children.
        /// </summary>
        protected virtual void LoadChildren()
        {

        }

        #endregion // LoadChildren

        #region Parent

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Parent

     
        #endregion // Presentation Members
    }
}
