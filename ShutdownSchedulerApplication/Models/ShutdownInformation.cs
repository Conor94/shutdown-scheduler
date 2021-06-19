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
            AddValidator(nameof(ShutdownTime), ValidateShutdownTime);
        }
        #endregion

        public string ValidateShutdownTime(object parameter)
        {
            if (parameter is string userInput)
            {
                if (DateTime.TryParse(userInput, out DateTime shutdownTime) == false)
                {
                    Error = "Invalid time format.";
                }
                else if (shutdownTime.RemoveSeconds() < DateTime.Now.RemoveSeconds())
                {
                    Error = "Shutdown time cannot be in the past.";
                }
                else
                {
                    Error = "";
                }
                return Error;
            }
            else
            {
                throw new ArgumentException($"Argument 'object {nameof(parameter)}' must be of type 'string'.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}
