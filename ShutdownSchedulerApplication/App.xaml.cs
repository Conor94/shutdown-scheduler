using AppConfigurationManager;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using ShutdownSchedulerApplication.Configuration;
using ShutdownSchedulerApplication.Models;
using ShutdownSchedulerApplication.Views;
using System;
using System.Reflection;
using System.Windows;

namespace ShutdownSchedulerApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<ShutdownInformation>();
        }

        private void PrismApplication_OnStartup(object sender, StartupEventArgs e)
        {
            // Initializes the configuration file
            AppConfigManager<AppConfigSection>.Init(Environment.SpecialFolder.CommonApplicationData, Assembly.GetExecutingAssembly().GetName().Name);
        }
    }
}
