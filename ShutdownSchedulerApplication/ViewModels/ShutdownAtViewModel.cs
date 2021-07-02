using MvvmBase.Bindable;
using Prism.Events;
using ShutdownSchedulerApplication.Models;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class ShutdownAtViewModel : ViewModelBase
    {
        public ShutdownInformation ShutdownInfo { get; }

        #region Constructors
        public ShutdownAtViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            ShutdownInfo = shutdownInfo;
        }
        #endregion
    }
}
