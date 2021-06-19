using System;
using System.Configuration;

namespace ShutdownSchedulerApplication.Configuration
{
    public class AppConfigSection : ConfigurationSection
    {
        #region Properties
        [ConfigurationProperty("ShutdownTime", DefaultValue = null)]
        public DateTime ShutdownTime
        {
            get => (DateTime)this["ShutdownTime"];
            set => this["ShutdownTime"] = value;
        }
        [ConfigurationProperty("IsShutdownScheduled", DefaultValue = false)]
        public bool IsShutdownScheduled
        {
            get => (bool)this["IsShutdownScheduled"];
            set => this["IsShutdownScheduled"] = value;
        }
        [ConfigurationProperty("IsPlannedServiceStop", DefaultValue = false)]
        public bool IsPlannedServiceStop
        {
            get => (bool)this["IsPlannedServiceStop"];
            set => this["IsPlannedServiceStop"] = value;
        }
        #endregion
    }
}
