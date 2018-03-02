using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE.ViewModel;

namespace Wpf_SyncCompositionPE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel;
        public MainWindow(ReferenceObject startObjectRef)
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel(startObjectRef);

            this.DataContext = viewModel;

            if (viewModel.CloseAction == null)
                viewModel.CloseAction = new Action(() => this.Close()); 
        }
        
    }
}
