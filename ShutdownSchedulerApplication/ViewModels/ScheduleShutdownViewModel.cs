using AppConfigurationManager;
using MvvmBase.Bindable;
using Prism.Commands;
using Prism.Events;
using ShutdownSchedulerApplication.Configuration;
using ShutdownSchedulerApplication.Enums;
using ShutdownSchedulerApplication.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class ScheduleShutdownViewModel : ViewModelBase
    {
        #region Fields and properties
        private DelegateCommand mScheduleShutdownCommand;
        private ShutdownInformation mShutdownInfo;
        private ViewModelBase mSelectedShutdownTypeViewModel;
        private ShutdownType mSelectedShutdownType;

        public ShutdownInformation ShutdownInfo
        {
            get => mShutdownInfo;
            private set => mShutdownInfo = value;
        }
        public DelegateCommand ScheduleShutdownCommand
        {
            get => mScheduleShutdownCommand ?? (mScheduleShutdownCommand = new DelegateCommand(ScheduleShutdownExecute, ScheduleShutdownCanExecute));
        }
        public ViewModelBase SelectedShutdownTypeViewModel
        {
            get => mSelectedShutdownTypeViewModel;
            set => SetProperty(ref mSelectedShutdownTypeViewModel, value);
        }
        public ShutdownType SelectedShutdownType
        {
            get => mSelectedShutdownType;
            set
            {
                switch (value)
                {
                    case ShutdownType.@in:
                        SelectedShutdownTypeViewModel = new ShutdownInViewModel(EventAggregator, ShutdownInfo);
                        break;
                    case ShutdownType.at:
                        SelectedShutdownTypeViewModel = new ShutdownAtViewModel(EventAggregator, ShutdownInfo);
                        break;
                    default:
                        break;
                }
                mSelectedShutdownType = value;
            }
        }
        public IEnumerable<ShutdownType> ShutdownTypes
        {
            get => Enum.GetValues(typeof(ShutdownType)).Cast<ShutdownType>();
        }
        #endregion

        #region Constructors
        public ScheduleShutdownViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            ShutdownInfo = shutdownInfo;
            ShutdownInfo.PropertyChanged += ShutdownInfo_PropertyChanged;
            ShutdownInfo.ShutdownTime = "";

            SelectedShutdownType = ShutdownType.@in;            
        }
        #endregion

        #region Events Handlers
        private void ShutdownInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ScheduleShutdownCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Command methods
        private void ScheduleShutdownExecute()
        {
            try
            {
                AppConfigSection settings = AppConfigManager<AppConfigSection>.GetSection();
                settings.IsShutdownScheduled = true;
                settings.ShutdownTime = DateTime.Parse(ShutdownInfo.ShutdownTime);
                AppConfigManager<AppConfigSection>.Save();

                // Start the service
                ServiceController service = new ServiceController("ShutdownSchedulerService");
                service.Start();
                service.Dispose();

                InvokeViewChangeRequest();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Current working directory: {Directory.GetCurrentDirectory()}\n\n" +
                                $"Message: {e.Message}\n\n" +
                                $"Stacktrace: {e.StackTrace}");
            }
        }

        private bool ScheduleShutdownCanExecute()
        {
            if (SelectedShutdownTypeViewModel is ShutdownAtViewModel)
            {
                return ShutdownInfo.ValidateShutdownTime(ShutdownInfo.ShutdownTime, out string _);
            }
            else if (SelectedShutdownTypeViewModel is ShutdownInViewModel vm)
            {
                return vm.ValidateUserInput(vm.UserInput, out string _);
            }
            else
            {
                throw new Exception($"'{nameof(SelectedShutdownTypeViewModel)}' is not a valid view model. It must be a {nameof(ShutdownAtViewModel)} or {nameof(ShutdownInViewModel)}.");
            }
        }
        #endregion
    }
}