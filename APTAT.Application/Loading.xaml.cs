using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace APTAT.Application
{

    public partial class Loading
    {
        static readonly object syncRoot = new object();
        int nb;
        bool isRunning;

        #region IsBusy (DependencyProperty)

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(Loading),
            new PropertyMetadata(OnIsBusyChanged));

        private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Loading)d).OnIsBusyChanged(e);
        }

        protected virtual void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.BeginInvoke((Action)Start, null);
            }
            else
            {
                Dispatcher.BeginInvoke((Action)Stop, null);
            }
        }
        #endregion

        readonly Storyboard startLoad;
        readonly Storyboard loadCycle;
        readonly Storyboard stopLoad;

        public Loading()
        {
            InitializeComponent();

            startLoad = Resources["StartLoad"] as Storyboard;
            loadCycle = Resources["LoadCycle"] as Storyboard;
            stopLoad = Resources["StopLoad"] as Storyboard;
        }

        private void Start()
        {
            lock (syncRoot)
            {
                nb++;

                if (!isRunning)
                {
                    isRunning = true;
                    startLoad.Begin();
                    loadCycle.Begin();
                }
            }
        }

        private void Stop()
        {
            lock (syncRoot)
            {
                nb--;

                if (nb <= 0)
                {
                    isRunning = false;
                    stopLoad.Begin();
                    nb = 0;
                }
            }
        }
    }
}
