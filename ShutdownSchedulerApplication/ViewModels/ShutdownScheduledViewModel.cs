using AppConfigurationManager;
using Prism.Commands;
using Prism.Events;
using MvvmBase.Bindable;
using ShutdownSchedulerApplication.Models;
using ShutdownSchedulerApplication.Configuration;
using System;
using System.ServiceProcess;
using System.Windows;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class ShutdownScheduledViewModel : ViewModelBase
    {
        #region Fields and properties
        private DelegateCommand mAbortShutdownCommand;
        private ShutdownInformation mShutdownInfo;

        public DelegateCommand AbortShutdownCommand
        {
            get => mAbortShutdownCommand ?? (mAbortShutdownCommand = new DelegateCommand(AbortShutdownExecute));
        }
        public ShutdownInformation ShutdownInfo
        {
            get => mShutdownInfo;
            private set => mShutdownInfo = value;
        }
        #endregion

        #region Constructors
        public ShutdownScheduledViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            ShutdownInfo = shutdownInfo;
            
            AppConfigSection section = AppConfigManager<AppConfigSection>.GetSection();
            ShutdownInfo.ShutdownTime = section.ShutdownTime.ToString();
        }
        #endregion

        #region Command methods
        public void AbortShutdownExecute()
        {
            // Ask if the user is sure they want to abort the scheduled shutdown
            MessageBoxResult userResponse = MessageBox.Show("Are you sure you want to abort the scheduled shutdown?", "Abort scheduled shutdown", MessageBoxButton.YesNo);
            if (userResponse == MessageBoxResult.Yes)
            {
                try
                {
                    // Change settings
                    AppConfigSection section = AppConfigManager<AppConfigSection>.GetSection();
                    section.IsShutdownScheduled = false;
                    section.ShutdownTime = DateTime.MinValue;

                    // Stop the service
                    section.IsPlannedServiceStop = true;
                    AppConfigManager<AppConfigSection>.Save();
                    ServiceController service = new ServiceController("ShutdownSchedulerService");
                    service.Stop();
                    service.Dispose();

                    InvokeViewChangeRequest();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error occurred when attempting to abort the scheduled shutdown. This has likely occurred because the " +
                                    "service used to shutdown the system was stopped while this program was running.\n\n" +
                                    "Restart the program to fix the error. If this does not work, the program must be reinstalled.\n\n" +
                                    "Exception information:" +
                                    $"Message: {e.Message}\n" +
                                    $"Stacktrace: {e.StackTrace}");
                }
            }
        }
        #endregion
    }
}