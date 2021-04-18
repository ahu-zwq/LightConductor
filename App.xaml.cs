using LightConductor.Main;
using LightConductor.Pages;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

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

            if (Directory.Exists(Monitor.TMP))
            {
                Directory.Delete(Monitor.TMP, true);
            }
            Log.InfoFormat("清空文件夹，{0}", Monitor.TMP);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("<<<========================End==");
            base.OnExit(e);
        }


        public App()
        {
            //AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", "D:\\software\\matlab runtime\\v95\\bin\\win64;");
            //AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", "D:\\software\\matlab runtime\\v95\\bin\\win64;");
            //var method = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            //var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            //method.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", "Libs;" });

        }

    }


    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            bool v = System.Convert.ToBoolean(value);

            return !v;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class IsShowConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            
            return "Green";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
