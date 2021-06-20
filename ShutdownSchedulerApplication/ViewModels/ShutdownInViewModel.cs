using Prism.Events;
using MvvmBase.Bindable;
using ShutdownSchedulerApplication.Models;
using System;

namespace ShutdownSchedulerApplication.ViewModels
{
    public class ShutdownInViewModel : ViewModelBase
    {
        #region Fields and properties
        private ShutdownInformation mShutdownInfo;
        private string mUserInput;
        private string mSelectedTimeFormat;
        private string[] mTimeFormats;

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
        public string SelectedTimeFormat
        {
            get => mSelectedTimeFormat;
            set
            {
                SetProperty(ref mSelectedTimeFormat, value);
                ShutdownInfo.ShutdownTime = CalculateShutdownTime(mUserInput);
            }
        }
        public string[] TimeFormats
        {
            get
            {
                if (mTimeFormats == null)
                {
                    mTimeFormats = new string[] { "minutes", "hours" };
                    SelectedTimeFormat = mTimeFormats[0];
                }
                return mTimeFormats;
            }
            private set => mTimeFormats = value;
        }
        #endregion

        #region Constructors
        public ShutdownInViewModel(IEventAggregator eventAggregator, ShutdownInformation shutdownInfo) : base(eventAggregator, null)
        {
            ShutdownInfo = shutdownInfo;

            UserInput = "";
            
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
                errorMessage = "Invalid number.";
            }
            else if (timeInput <= 0)
            {
                errorMessage = "Number must be greater than 0.";
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
            if (string.IsNullOrEmpty(Error) && !string.IsNullOrEmpty(userInput) && SelectedTimeFormat != null)
            {
                double timeInput = double.Parse(userInput);
                DateTime currentTime = DateTime.Now;
                if (SelectedTimeFormat == "minutes")
                {
                    shutdownTime = currentTime.AddMinutes(timeInput);
                }
                else if (SelectedTimeFormat == "hours")
                {
                    shutdownTime = currentTime.AddHours(timeInput);
                }
            }
            return shutdownTime.ToString();
        }
        #endregion
    }
}
