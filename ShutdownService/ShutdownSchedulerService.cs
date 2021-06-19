using AppConfigurationManager;
using ShutdownSchedulerApplication.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace ShutdownService
{
    /// <summary>
    /// Service that is used by the Shutdown Scheduler application to shutdown the local machine.
    /// </summary>
    public partial class ShutdownSchedulerService : ServiceBase
    {
        #region Fields
        private Timer mShutdownTimer;
        #endregion

        #region Properties
        public Timer ShutdownTimer
        {
            get => mShutdownTimer;
            set => mShutdownTimer = value;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for the service. It initializes all properties and configures the service.
        /// The current working directory is also changed to the applications base directory (this must be
        /// done in the constructor, changing the working directory in <see cref="OnStart(string[])"/> will
        /// throw an exception).
        /// </summary>
        public ShutdownSchedulerService()
        {
            try
            {
                InitializeComponent();

                ShutdownTimer = null;

                // Setup the service
                CanShutdown = true;
                CanHandleSessionChangeEvent = true;
                EventLog.Source = ServiceName;

                // Change the current working directory to the applications base directory. This must be done
                // because the default working directory for services is %SYSTEMROOT%\System32.
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Exception occurred while instantiating {ServiceName}.\n\n" +
                                         $"Message: {e.Message}\n" +
                                         $"Stacktrace: {e.StackTrace}");
            }
        }
        #endregion

        #region ServiceBase methods
        /// <summary>
        /// This method is called when the service is started.
        /// <para/>
        /// The configuration file stored in the <see cref="ShutdownSchedulerConfig"/> project is opened so that the
        /// service can access resources that are shared between this service and the Shutdown Scheduler application.
        /// <para/>
        /// The <see cref="ShutdownTimer"/> is also set to the shutdown time that's saved to the shared configuration file 
        /// (i.e. the configuration stored in the <see cref="ShutdownSchedulerConfig"/> project).
        /// </summary>
        /// <param name="args">Arguments passed to the service when the service was started.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                // Initializes and open the configuration file
                AppConfigManager<AppConfigSection>.Init(Environment.SpecialFolder.CommonApplicationData, "ShutdownSchedulerApplication");
                AppConfigSection section = AppConfigManager<AppConfigSection>.GetSection();
                section.IsPlannedServiceStop = false; // Initializing to false makes all stops unplanned unless otherwise specified
                                                      // Set IsPlannedServiceStop to true to specify a planned stop

                // Set the shutdown timer
                DateTime shutdownTime = section.ShutdownTime;
                DateTime currentTime = DateTime.Now;
                if (shutdownTime >= currentTime)
                {
                    ShutdownTimer = new Timer()
                    {
                        AutoReset = false,
                        Interval = (shutdownTime - DateTime.Now).TotalMilliseconds
                    };
                    ShutdownTimer.Elapsed += ShutdownTimer_Elapsed;
                    ShutdownTimer.Start();
                    EventLog.WriteEntry($"Shutdown has been scheduled for {shutdownTime}.");
                }
                else
                {
                    // Shutdown time was in the past (the GUI application should prevent this), so the timer can't be started.
                    // Log a message and then stop the service.
                    EventLog.WriteEntry("Shutdown timer was not started because the specified shutdown time was in the past.\n" +
                                            $"Current time: {currentTime}\n" +
                                            $"Shutdown time: {shutdownTime}");
                    section.IsPlannedServiceStop = true;
                    Stop();
                }
                AppConfigManager<AppConfigSection>.Save();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Exception occurred while starting {ServiceName}.\n\n" +
                                         $"Message: {e.Message}\n" +
                                         $"Stacktrace: {e.StackTrace}");
            }
        }

        protected override void OnStop()
        {
            CancelTimerProcedure();
        }

        /// <summary>
        /// This method is called when a system shutdown is detected. The service is stopped and
        /// the <see cref="IsPlannedStop"/> boolean is set to false to indicate that stopping
        /// the service was not planned.
        /// </summary>
        protected override void OnShutdown()
        {
            Stop();
        }

        /// <summary>
        /// This method is called when a session change is detected. The service is stopped if the
        /// reason for session change is a <see cref="SessionChangeReason.SessionLogoff"/>.
        /// <para/>
        /// <see cref="SessionChangeReason"/> provides information about session changes. This method
        /// will be called after any session change occurs.
        /// </summary>
        /// <param name="changeDescription"></param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);

            if (changeDescription.Reason == SessionChangeReason.SessionLogoff)
            {
                Stop();
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Event handler for the <see cref="ShutdownTimer"/>'s <see cref="Timer.Elapsed"/> event.
        /// </summary>
        /// <param name="sender">Object that invoked the event.</param>
        /// <param name="e">Data for the event.</param>
        private void ShutdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventLog.WriteEntry($"Shutdown timer elapsed at {e.SignalTime:MMM dd, yyyy hh:mm:ss tt}.");
            ShutdownMachine();

            // Stop the service
            AppConfigSection section = AppConfigManager<AppConfigSection>.GetSection();
            section.IsPlannedServiceStop = true;
            AppConfigManager<AppConfigSection>.Save();
            Stop();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Runs a command that shuts down the system.
        /// </summary>
        private void ShutdownMachine()
        {
            // Test creating a process to call a command
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                    Arguments = "/C shutdown /s",
                    WindowStyle = ProcessWindowStyle.Hidden
                },
            };
            EventLog.WriteEntry($"{ServiceName} is shutting down the system.");
            process.Start();
            process.Dispose();
        }

        /// <summary>
        /// Runs the cancel procedure. This includes changing settings back to their defaults
        /// and logs a message to the Event Viewer if the service is stopped before the shutdown timer
        /// (<see cref="ShutdownTimer"/>) elapses.
        /// </summary>
        private void CancelTimerProcedure()
        {
            try
            {
                ShutdownTimer.Stop();

                AppConfigSection section = AppConfigManager<AppConfigSection>.GetSection();
                // If the service is stopped before the shutdown timer elapses, log a message saying the shutdown was cancelled
                if (!section.IsPlannedServiceStop)
                {
                    EventLog.WriteEntry($"Scheduled shutdown was cancelled due to the user logging off, the system shutting down, or stopping the {ServiceName} service.",
                                    EventLogEntryType.Information);
                }

                // Reset all settings to default
                section.IsShutdownScheduled = false;
                section.ShutdownTime = DateTime.MinValue;
                AppConfigManager<AppConfigSection>.Save();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("An exception was thrown while cancelling the shutdown timer.\n\n" +
                                    $"Message: {e.Message}\n\n" +
                                    $"Stacktrace: {e.StackTrace}");
            }
        }
        #endregion
    }
}