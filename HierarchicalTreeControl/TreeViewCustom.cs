using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace HierarchicalTreeControl
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HierarchicalTreeControl"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HierarchicalTreeControl;assembly=HierarchicalTreeControl"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    /// Представляет элемент управления, который отображает иерархические данные в древовидной структуре, 
    /// в которой есть элементы, которые могут расширяться и сворачиваться.
    /// </summary>
    public class TreeViewCustom : TreeView
    {

        static TreeViewCustom()
        {
            // Переопределить стиль по умолчанию и шаблон управления по умолчанию
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewCustom), new FrameworkPropertyMetadata(typeof(TreeViewCustom)));
        }

        /// <summary>
        /// Инициализировать новый экземпляр TreeViewCustom.
        /// </ summary>
        public TreeViewCustom()
        {
            Columns = new GridViewColumnCollection();

        }

        #region Свойства


        /// <summary>
        /// Получает или задает коллекцию System.Windows.Controls.GridViewColumn
        /// объектов, которые определены для этого TreeViewCustom.
        /// </ summary>
        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Получает или задает, могут ли столбцы в TreeViewCustom
        /// переупорядочивается с помощью операции перетаскивания. Это свойство зависимостей.
        /// </ summary>
        public bool AllowsColumnReorder
        {
            get { return (bool)GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }


        #endregion

        #region Свойства статической зависимости


        // Использование DependencyProperty в качестве хранилища резервных копий для AllowsColumnReorder. 
        //Это позволяет создавать анимацию, стиль, привязку и т. Д. 
        public static readonly DependencyProperty AllowsColumnReorderProperty =
            DependencyProperty.Register("AllowsColumnReorder", typeof(bool), typeof(TreeViewCustom), new UIPropertyMetadata(null));

        //Использование DependencyProperty в качестве хранилища резервных копий для столбцов. 
        //Это позволяет создавать анимацию, стиль, привязку и т. Д. ...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(GridViewColumnCollection),
            typeof(TreeViewCustom),
            new UIPropertyMetadata(null));
        #endregion
    }

    /// <summary>
    /// Представляет элемент управления, который может переключать состояния, чтобы развернуть узел TreeViewCustom.
    /// </summary>
    public class TreeViewCustomExpander : ToggleButton
    {

    }

    /// <summary>
    /// Представляет конвертер, который может вычислять отступ любого элемента в классе, производном от TreeView.
    /// </summary>
    public class TreeViewCustomConverter : IValueConverter
    {
        public const double Indentation = 10;
        
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Если значение равно null, ничего не возвращайте
            if (value == null) return null;

            //Преобразуйте элемент в двойное
            if (targetType == typeof(double) && typeof(DependencyObject).IsAssignableFrom(value.GetType()))
            {
                //Передать элемент как объект DependencyObject
                DependencyObject Element = value as DependencyObject;
                //Создайте счетчик уровня со значением, установленным на -1
                int Level = -1;

                //Переместите визуальное дерево и подсчитайте количество TreeViewItem.
                for (; Element != null; Element = VisualTreeHelper.GetParent(Element))
                    //Проверьте, является ли текущий элемент TreeViewItem
                    if (typeof(TreeViewItem).IsAssignableFrom(Element.GetType()))
                    {
                        //Увеличить счетчик уровня
                        Level++;

                    }

                //Вернуть отступ как двойной
                return Indentation * Level;
            }
            //Преобразование типов не поддерживается
            throw new NotSupportedException(
                string.Format("Cannot convert from <{0}> to <{1}> using <TreeViewCustomConverter>.",
                value.GetType(), targetType));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Этот метод не поддерживается.");
        }

        #endregion
    }
}
