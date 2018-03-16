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
        public LoadWindowViewModel LoadingViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the LoadingWindow class.
        /// </summary>
        public LoadingMainWindow()
        {
            InitializeComponent();

            LoadingViewModel = new LoadWindowViewModel(); // this creates an instance of the ViewModel
            this.DataContext = LoadingViewModel; // this sets the newly created ViewModel as the DataContext for the View

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