using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wpf_SyncCompositionPE.ViewModel
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

        readonly TreeViewModel _treeViewModel;


        bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren = false)
        {
            _parent = parent;

            _children = new ObservableCollection<TreeViewItemViewModel>();

            _treeViewModel = this as TreeViewModel;

            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }

        // Это используется для создания экземпляра DummyChild.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members

        public string Name
        {
            get { return _treeViewModel.Name; }
        }

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

        /// <summary>
        /// Возвращает true, если имеются дети
        /// </summary>
        public bool HasChildren
        {
            get { return this.Children.Count > 0 && this.Children[0] != DummyChild; }
        }

        #endregion // HasLoadedChildren

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
                    this.RaisePropertyChanged("IsExpanded");
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

        #region Visibility

        //private string visibility = "Collapsed";

        ///// <summary>
        ///// Флаг отвечающий за отображение чекбокса в дереве
        ///// </summary>
        //public string Visibility
        //{
        //    get
        //    {
        //        if ((bool)IsObjectToSync)
        //            visibility = "Visible";
        //        else
        //            visibility = "Collapsed";

        //        return visibility;
        //    }
        //}



        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Получает / задает, является ли TreeViewItem
        /// , связанный с этим объектом.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region LoadChildren

        /// <summary>
        /// Вызывается, когда дочерние элементы необходимо загружать по требованию.
        /// Подклассы могут переопределить это, чтобы заполнить коллекцию Children.
        /// </summary>
        protected virtual void LoadChildren()
        {

        }

        public void clearAllCheckboxes(TreeViewModel data)
        {
            if (data == null) return;

            if ((bool)data.IsSelectObjToSynch == false) return;

            if ((bool)data.IsObjectToSync == true)
                data.IsObjectToSync = false;

            foreach (var child in data.Children.OfType<TreeViewModel>())
                clearAllCheckboxes(child);
        }

        public void MarkAllParents(TreeViewModel data)
        {
            if (data == null) { return; }

            if ((bool)data.IsSelectObjToSynch == false) { return; }

            if ((bool)data.IsObjectToSync == false)
                data.IsObjectToSync = true;

            MarkAllParents(data.Parent as TreeViewModel);
        }

        #endregion // LoadChildren

        #region Parent

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Parent

        //#region NameContainsText

        //public bool NameContainsText(string text)
        //{
        //    if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
        //        return false;

        //    return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        //}

        //#endregion // NameContainsText

        #endregion // Presentation Members
    }
}
