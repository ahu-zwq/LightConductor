using LightConductor.Main;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LightConductor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Info("==Startup=====================>>>");
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("<<<========================End==");
            base.OnExit(e);
        }

    }
}
