using System;
using System.Windows;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE.ViewModel;

namespace Wpf_SyncCompositionPE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
      public MainWindowViewModel viewModel { get; set; }

        public MainWindow(/*ReferenceObject startObjectRef*/)
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel();

            this.DataContext = viewModel;

            //if (viewModel.CloseAction == null)
            //    viewModel.CloseAction = new Action(() => this.Close());
        }


    }
}
