using MvvmBase.Bindable;
using Prism.Events;
using ShutdownSchedulerApplication.Enums;
using ShutdownSchedulerApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class ShutdownInViewModel : ViewModelBase
    {
        #region Fields and properties
        private ShutdownInformation mShutdownInfo;
        private string mUserInput;
        private TimeFormat mSelectedTimeFormat;

        public ShutdownInformation ShutdownInfo
        {
            get => mShutdownInfo;
            private set => mShutdownInfo = value;
        }
        public string UserInput
        {
            get => mUserInput;
            set
            {
                SetProperty(ref mUserInput, value);
            }
        }
        public TimeFormat SelectedTimeFormat
        {
            get => mSelectedTimeFormat;
            set
            {
                SetProperty(ref mSelectedTimeFormat, value);
                ShutdownInfo.ShutdownTime = CalculateShutdownTime(mUserInput); // Recalculate the shutdown time because the format has changed
            }
        }
        public IEnumerable<TimeFormat> TimeFormats
        {
            get => Enum.GetValues(typeof(TimeFormat)).Cast<TimeFormat>();
        }
        #endregion

        #region Constructors
        public ShutdownInViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            ShutdownInfo = shutdownInfo;

            UserInput = "";

            SelectedTimeFormat = TimeFormat.minutes;

            AddValidator(nameof(UserInput), new DataErrorValidator<string>(ValidateUserInput));
        }
        #endregion

        #region Validator methods
        public bool ValidateUserInput(string userInput, out string errorMessage)
        {
            bool isValid = false;

            if (string.IsNullOrWhiteSpace(userInput))
            {
                errorMessage = "Must enter input.";
            }
            else if (!double.TryParse(userInput, out double timeInput))
            {
                errorMessage = "Invalid shutdown time.";
            }
            else if ((SelectedTimeFormat == TimeFormat.hours && timeInput * 60 < ShutdownInformation.MinimumShutdownTimeInMinutes) ||
                     (SelectedTimeFormat == TimeFormat.minutes && timeInput < ShutdownInformation.MinimumShutdownTimeInMinutes))
            {
                errorMessage = $"Shutdown time cannot be less than {ShutdownInformation.MinimumShutdownTimeInMinutes} {(ShutdownInformation.MinimumShutdownTimeInMinutes < 10 ? "minute" : "minutes")}.";
            }
            else
            {
                errorMessage = "";
                isValid = true;
            }

            ShutdownInfo.ShutdownTime = CalculateShutdownTime(mUserInput);

            return isValid;
        }
        #endregion

        #region Helper methods
        public string CalculateShutdownTime(string userInput)
        {
            DateTime? shutdownTime = null;
            if (string.IsNullOrEmpty(Error) && !string.IsNullOrEmpty(userInput) && userInput != ".")
            {
                double timeInput = double.Parse(userInput);
                DateTime currentTime = DateTime.Now;
                if (SelectedTimeFormat == TimeFormat.minutes)
                {
                    shutdownTime = currentTime.AddMinutes(timeInput);
                }
                else if (SelectedTimeFormat == TimeFormat.hours)
                {
                    shutdownTime = currentTime.AddHours(timeInput);
                }
            }
            return shutdownTime.ToString();
        }
        #endregion
    }
}
