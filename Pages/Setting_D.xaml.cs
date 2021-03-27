using LightConductor.Main;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightConductor.Pages
{

    /// <summary>
    /// Setting_D.xaml 的交互逻辑
    /// </summary>
    public partial class Setting_D : Page
    {

        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static List<DeviceModule> DEVICE_LIST = new List<DeviceModule>();
        public readonly static string SettingPath = "D:\\LC\\settings_db.json";
        public readonly static int DeviceNum = 7;

        public Setting_D()
        {
            InitializeComponent();

            Setting_dg.DataContext = InitDevices();
        }

        public static List<DeviceModule> GetDeviceList()
        {
            if (DEVICE_LIST.Count == DeviceNum)
            {
                return DEVICE_LIST;
            }
            return InitDevices();
        }

        private static List<DeviceModule> InitDevices()
        {
            Log.Info("InitDevices");
            List<DeviceModule> lists = JsonNewtonsoft.ReadingJson<DeviceModule>(SettingPath);
            int v = DeviceNum - lists.Count;
            if (v > 0)
            {
                DEVICE_LIST = lists;
                for (int i = 0; i < v; i++)
                {
                    DEVICE_LIST.Add(DeviceModule.getEmptyDevice());
                }
            }
            else
            {
                DEVICE_LIST = lists.GetRange(0, DeviceNum);
            }
            for (int i = 0; i < DeviceNum; i++)
            {
                DEVICE_LIST[i].Id = i + 1 + "";
            }
            return DEVICE_LIST;

        }

        private void UpdateSetting()
        {
            JsonNewtonsoft.WritingJson(SettingPath, DEVICE_LIST);
        }


    }

    



}
