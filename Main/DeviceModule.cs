﻿using LightConductor.Main;
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

        private string id;
        private string name;
        private string verticalMotorSerialNo;
        private string horizontalMotorSerialNo;
        private string cameraIp;
        private string cameraPort;
        private string cameraUserName;
        private string cameraPassword;
        private double datum_x;
        private double datum_y;
        private string datum_pos;

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
                }
            } }
        public double Datum_y { get => datum_y; set {
                if (datum_y != value)
                {
                    datum_y = value;
                    OnPropertyChanged("Datum_y");
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

                    JsonNewtonsoft.WritingJson(Setting_D.SettingPath, lists);

                    LogUtils.Log.Info("更新配置");
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
