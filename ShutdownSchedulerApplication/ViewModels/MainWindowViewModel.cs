using AppConfigurationManager;
using Prism.Commands;
using Prism.Events;
using PrismMvvmBase.Bindable;
using PrismMvvmBase.Events;
using ShutdownSchedulerApplication.Configuration;
using ShutdownSchedulerApplication.Models;
using System.Windows;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields and properties
        private ViewModelBase mSelectedViewModel;
        private DelegateCommand mCloseApplicationCommand;

        public ViewModelBase SelectedViewModel
        {
            get => mSelectedViewModel;
            set => SetProperty(ref mSelectedViewModel, value);
        }
        public DelegateCommand CloseApplicationCommand
        {
            get => mCloseApplicationCommand ?? (mCloseApplicationCommand = new DelegateCommand(CloseApplicationExecute));
            set => mCloseApplicationCommand = value;
        }
        #endregion

        #region Constructors
        public MainWindowViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            Title = "Shutdown Scheduler";
            EventAggregator.GetEvent<ViewChangeRequestEvent>().Subscribe(OnViewChangeRequest);
            
            AppConfigSection settings = AppConfigManager<AppConfigSection>.GetSection();
            if (settings.IsShutdownScheduled == false)
            {
                SelectedViewModel = new ScheduleShutdownViewModel(eventAggregator, shutdownInfo);
            }
            else
            {
                SelectedViewModel = new ShutdownScheduledViewModel(eventAggregator, shutdownInfo);
            }
        }
        #endregion

        #region Events handlers
        private void OnViewChangeRequest(ViewModelBase vm)
        {
            if (vm is ScheduleShutdownViewModel scheduleShutdownVm)
            {
                SelectedViewModel = new ShutdownScheduledViewModel(EventAggregator, scheduleShutdownVm.ShutdownInfo);
            }
            else if (vm is ShutdownScheduledViewModel shutdownScheduledVm)
            {
                SelectedViewModel = new ScheduleShutdownViewModel(EventAggregator, shutdownScheduledVm.ShutdownInfo);
            }
        }
        #endregion

        #region Command methods
        private void CloseApplicationExecute()
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
