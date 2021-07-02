using MvvmBase.Bindable;
using ShutdownSchedulerApplication.Extensions;
using System;
using System.Windows;

namespace ShutdownSchedulerApplication.Models
{
    public class ShutdownInformation : ModelBase
    {
        #region Fields
        private string mShutdownTime;
        #endregion

        #region Properties
        public string ShutdownTime
        {
            get => mShutdownTime;
            set
            {
                if (DateTime.TryParse(value, out DateTime dateTime))
                {
                    value = dateTime.ToString((string)Application.Current.Resources["DateTimeFormat"]);
                }
                SetProperty(ref mShutdownTime, value);
            }
        }
        #endregion

        #region Constructors
        public ShutdownInformation() : base()
        {
            ShutdownTime = DateTime.Now.AddMinutes(30).ToString();

            AddValidator(nameof(ShutdownTime), new DataErrorValidator<string>(ValidateShutdownTime));
        }
        #endregion

        public bool ValidateShutdownTime(string userInput, out string errorMessage)
        {
            bool isValid = false;

            if (DateTime.TryParse(userInput, out DateTime shutdownTime) == false)
            {
                errorMessage = "Invalid time format.";
            }
            else if (shutdownTime.RemoveSeconds() < DateTime.Now.RemoveSeconds())
            {
                errorMessage = "Shutdown time cannot be in the past.";
            }
            else
            {
                errorMessage = "";
                isValid = true;
            }

            return isValid;
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}
