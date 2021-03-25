using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; //enables use of Thread.Sleep() “wait” method
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.Settings; //this will specifically target only the commands contained within the .Settings sub-class library in *.GenericMotorCLI.dll.
using Thorlabs.MotionControl.TCube.DCServoCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using System.Windows;
using log4net;
using System.Reflection;

namespace LightConductor.Main
{
    class TDCHandle : IDisposable
    {

        private string serialNo;
        private TCubeDCServo device;
        private MotorConfiguration motorSettings;
        private static Dictionary<String, TDCHandle> HANDLE_DIC = new Dictionary<string, TDCHandle>();

        private TDCHandle(string serialNo)
        {
            try
            {
                LogUtils.Log.Info("start tdc," + serialNo);
                this.serialNo = serialNo;
                DeviceManagerCLI.BuildDeviceList();
                device = TCubeDCServo.CreateTCubeDCServo(serialNo);
                device.Connect(serialNo);
                device.WaitForSettingsInitialized(5000);
                motorSettings = device.LoadMotorConfiguration(serialNo, DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);
                device.StartPolling(250);
                device.EnableDevice();
                Thread.Sleep(500);
            }
            catch (Exception)
            {
                // Connection failed
                LogUtils.Log.ErrorFormat("Failed to open device {0}", serialNo);
            }


        }

        public static TDCHandle getTDCHandle(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo))
            {
                return null;
            }
            if (HANDLE_DIC.ContainsKey(serialNo))
            {
                return HANDLE_DIC[serialNo];
            }
            else
            {
                TDCHandle tDCHandle = new TDCHandle(serialNo);
                HANDLE_DIC.Add(serialNo, tDCHandle);
                return tDCHandle;
            }
        }


        public void Move(decimal distance, decimal velocity)
        {
            if (distance != 0)
            {
                try
                {
                    if (velocity != 0)
                    {
                        VelocityParameters velPars = device.GetVelocityParams();
                        velPars.MaxVelocity = velocity;
                        device.SetVelocityParams(velPars);
                    }
                    Decimal nowPos = device.Position;
                    Decimal toPos = nowPos + distance;
                    LogUtils.Log.InfoFormat("Moving Device to {0}", toPos);
                    device.MoveTo(toPos, 60000);
                }
                catch (Exception e)
                {
                    LogUtils.Log.ErrorFormat("Failed to move to position, {0}", e.Message);
                    MessageBox.Show("无法继续移动");
                    return;
                }
                LogUtils.Log.Info("Device Moved");

                Decimal newPos = device.Position;
                LogUtils.Log.InfoFormat("Device Moved to {0}", newPos);
            }

        }


        /// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		public void Dispose()
        {
            device.StopPolling();
            device.Disconnect(true);
        }



    }
}
