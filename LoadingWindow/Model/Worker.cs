using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LoadingWindow.Model
{
    public class WorkerEventArgs
    {
        public String TextProcess { get { return Worker.TextProcess; } set { Worker.TextProcess = value; } }
    }

    public static class Worker
    {

        #region create event
        public delegate void WorkerEventHandler(object source, EventArgs args);

        public static event WorkerEventHandler WorkerEvent = delegate { };

        public static void OnWorkerEvent()
        {
            if (WorkerEvent != null)
                WorkerEvent(null, EventArgs.Empty);
        }
        static Worker()
        {
            OnWorkerEvent();
        }
        #endregion

        private static string _textProcess = string.Empty;

        /// <summary>
        /// Sets and gets the  property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public static string TextProcess
        {
            get
            {
                return _textProcess;
            }

            set
            {
                if (_textProcess == value)
                {
                    return;
                }

                _textProcess = value;
            }
        }

        public static Action Work { get; set; }

        static Action _work { get; set; }

        static public void AddWork(Action work, string nameProcess)
        {
            if (work == null)
                throw new ArgumentNullException();
            else
                Work = work;

            TextProcess = nameProcess;

            IsWorkComplet = true;

            //   Task.Factory.StartNew(Work).ContinueWith(t => { IsWorkComplet = false; }, TaskScheduler.FromCurrentSynchronizationContext());
        }



        private static bool _isWorkComplet = true;

        public static bool IsWorkComplet
        {
            get { return _isWorkComplet; }
            set { _isWorkComplet = value; }
        }

    }
}
