using System.Windows;
using LoadingWindow.ViewModel;
using System;

namespace LoadingWindow
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingMainWindow : Window, IDisposable
    {
        public MainViewModel LoadingViewModel { get; set; }
        /// <summary>
        /// Initializes a new instance of the LoadingWindow class.
        /// </summary>
        public LoadingMainWindow(Action worker, string nameProcess)
        {
            InitializeComponent();

            LoadingViewModel = new MainViewModel(worker, nameProcess);

            this.DataContext = LoadingViewModel;

            if (LoadingViewModel.CloseAction == null)
                LoadingViewModel.CloseAction = new Action(() => this.Close());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
          
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                LoadingViewModel = null;
            }
            // free native resources if there are any.
        }
    }
}