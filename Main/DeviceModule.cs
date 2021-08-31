using LightConductor.Main;
using LightConductor.Pages;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor
{
    public class DeviceModule : INotifyPropertyChanged, IEditableObject
    {
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string id;
        private string name;
        //垂直方向电机序列号
        private string verticalMotorSerialNo;
        //水平方向电机序列号
        private string horizontalMotorSerialNo;
        private string cameraIp;
        private string cameraPort;
        private string cameraUserName;
        private string cameraPassword;
        private double datum_x;
        private double datum_y;
        private string datum_pos;
        private decimal velocity;
        //在setting_db.json中修改
        private int verticalSpotRel = 1;
        private int horizontalSpotRel = 1;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string VerticalMotorSerialNo { get => verticalMotorSerialNo; set => verticalMotorSerialNo = value; }
        public string HorizontalMotorSerialNo { get => horizontalMotorSerialNo; set => horizontalMotorSerialNo = value; }
        public string CameraIp { get => cameraIp; set => cameraIp = value; }
        public string CameraPort { get => cameraPort; set => cameraPort = value; }
        public string CameraUserName { get => cameraUserName; set => cameraUserName = value; }
        public string CameraPassword { get => cameraPassword; set => cameraPassword = value; }
        public double Datum_x { get => datum_x; set {
                if (datum_x != value)
                {
                    datum_x = value;
                    OnPropertyChanged("Datum_x");
                    OnPropertyChanged("Datum_pos");
                }
            } }
        public double Datum_y { get => datum_y; set {
                if (datum_y != value)
                {
                    datum_y = value;
                    OnPropertyChanged("Datum_y");
                    OnPropertyChanged("Datum_pos");
                }
            } }
        public string Datum_pos
        {
            get
            {
                if (datum_x > 0 || datum_y > 0)
                {
                    datum_pos = datum_x + ", " + datum_y;
                }
                else
                {
                    datum_pos = "";
                }
                return datum_pos;
            }
            set
            {
                if (datum_pos != value)
                {
                    datum_pos = value;
                    OnPropertyChanged("Datum_pos");
                }
            }
        }

        public decimal Velocity
        {
            get
            {
                if (velocity <= 0)
                {
                    return 1;
                }
                return velocity;
            }
            set
            {
                if (velocity != value && value > 0)
                {
                    velocity = value;
                    OnPropertyChanged("Velocity");
                }
            }
        }

        public int VerticalSpotRel { get => verticalSpotRel; set => verticalSpotRel = value; }
        public int HorizontalSpotRel { get => horizontalSpotRel; set => horizontalSpotRel = value; }

        public static DeviceModule getEmptyDevice()
        {
            return new DeviceModule("", "", "", "", "", "", "", "", 0, 0, "");
        }

        public DeviceModule()
        {
        }

        public DeviceModule(string id, string name, string verticalMotorSerialNo, string horizontalMotorSerialNo, string cameraIp, string cameraPort, string cameraUserName, string cameraPassword, double datum_x, double datum_y, string datum_pos)
        {
            this.id = id;
            this.name = name;
            this.verticalMotorSerialNo = verticalMotorSerialNo;
            this.horizontalMotorSerialNo = horizontalMotorSerialNo;
            this.cameraIp = cameraIp;
            this.cameraPort = cameraPort;
            this.cameraUserName = cameraUserName;
            this.cameraPassword = cameraPassword;
            this.datum_x = datum_x;
            this.datum_y = datum_y;
            this.datum_pos = datum_pos;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void BeginEdit()
        {

        }

        public void CancelEdit()
        {

        }

        public void EndEdit()
        {
            updateConfig();

        }

        public void updateConfig()
        {
            List<DeviceModule> lists = Setting_D.GetDeviceList();
            for (int i = 0; i < lists.Count; i++)
            {
                DeviceModule old = lists[i];
                if (old.Id.Equals(this.Id))
                {
                    old.Name = this.Name;
                    old.VerticalMotorSerialNo = this.VerticalMotorSerialNo;
                    old.HorizontalMotorSerialNo = this.HorizontalMotorSerialNo;
                    old.CameraIp = this.CameraIp;
                    old.CameraPort = this.CameraPort;
                    old.CameraUserName = this.CameraUserName;
                    old.CameraPassword = this.CameraPassword;
                    old.Datum_x = this.Datum_x;
                    old.Datum_y = this.Datum_y;
                    old.Datum_pos = this.Datum_pos;
                    old.velocity = this.velocity;

                    JsonNewtonsoft.WritingJson(Setting_D.SettingPath, lists);

                    Log.Info("更新配置 > " + Setting_D.SettingPath);
                    break;
                }
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
