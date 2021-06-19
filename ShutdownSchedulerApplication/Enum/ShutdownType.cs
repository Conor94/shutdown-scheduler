using System.ComponentModel;

namespace ShutdownSchedulerApplication.Enums
{
    public enum ShutdownType
    {
        [Description("Shutdown in minutes/hours")]
        @in = 1,
        [Description("Shutdown at a specific time")]
        at = 2
    }
}