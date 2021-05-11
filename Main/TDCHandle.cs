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
using Thorlabs.MotionControl.GenericMotorCLI;

namespace LightConductor.Main
{
    class TDCHandle : IDisposable
    {

        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string serialNo;
        private TCubeDCServo device;
        private MotorConfiguration motorSettings;
        private static Dictionary<String, TDCHandle> HANDLE_DIC = new Dictionary<string, TDCHandle>();

        public string SerialNo { get => serialNo; set => serialNo = value; }

        private TDCHandle(string serialNo)
        {
            try
            {
                Log.Info("*** TDC start tdc," + serialNo);
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
            catch (Exception e)
            {
                // Connection failed
                Log.ErrorFormat("{0}, {1}", serialNo, e.StackTrace);
            }
        }

        public TDCHandle()
        {
        }

        public static TDCHandle getTDCHandle(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo))
            {
                return new TDCHandle();
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
                    Log.InfoFormat("*** TDC >>>>>>>> {0} start move, from:{1}, distance:{2}, to:{3}", serialNo, nowPos, distance, toPos);
                    device.MoveTo(toPos, 60000);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Failed to move to position, {0}", e.Message);
                    MessageBox.Show("无法继续移动");
                    return;
                }

                Decimal newPos = device.Position;
                Log.InfoFormat("*** TDC <<<<<<<<< {0} move success, now:{1}", serialNo, newPos);
            }
        }

        private static bool _taskComplete;
        private static ulong _taskID;
        private decimal minPosition = Properties.Settings.Default.MinTDCPositon;
        private decimal maxPosition = Properties.Settings.Default.MaxTDCPositon;

        public static void CommandCompleteFunction(ulong taskID)
        {
            if ((_taskID > 0) && (_taskID == taskID))
            {
                _taskComplete = true;
            }
        }

        public void StopContinuousMove()
        {
            _taskComplete = false;
        }

        public TCubeDCServo MoveAsync(MotorDirection direction, decimal velocity)
        {
            if (velocity > 0)
            {
                //try
                //{
                VelocityParameters velPars = device.GetVelocityParams();
                velPars.MaxVelocity = velocity;
                device.SetVelocityParams(velPars);

                Decimal nowPos = device.Position;

                //if (nowPos == minPosition || nowPos == maxPosition)
                //{
                //    minPosition += 1;
                //    maxPosition -= 1;
                //}
                //Decimal toPos = nowPos + distance;
                //Log.InfoFormat("*** TDC >>>>>>>> {0} start move, from:{1}, distance:{2}, to:{3}", serialNo, nowPos, distance, toPos); -65  2208
                _taskComplete = false;
                switch (direction)
                {
                    case MotorDirection.Backward:
                        if (nowPos > minPosition)
                        {
                            _taskID = device.MoveTo(minPosition, CommandCompleteFunction);
                        }
                        else 
                        {
                            throw new Exception("is min");
                        }
                        break;
                    case MotorDirection.Forward:
                        if (nowPos < maxPosition)
                        {
                            _taskID = device.MoveTo(maxPosition, CommandCompleteFunction);
                        }
                        else
                        {
                            throw new Exception("is max");
                        }
                        break;
                }
                //while (!_taskComplete)
                //{
                //    Thread.Sleep(500);
                //    StatusBase status = device.Status;
                //    Console.WriteLine("Device Moving {0}", status.Position);

                //    // will need some timeout functionality;
                //}
                //}
                //catch (Exception e)
                //{
                //Log.ErrorFormat("Failed to move to position, {0}", e.Message);
                //MessageBox.Show("无法继续移动");
                //}

                //Decimal newPos = device.Position;
                //Log.InfoFormat("*** TDC <<<<<<<<< {0} move success, now:{1}", serialNo, newPos);

            }
            return device;
        }

        public Decimal getPosition()
        {
            try
            {
                return device.Position;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Failed getPosition, {0}", e.Message);
            }
            return -100000;
        }


        /// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		public void Dispose()
        {
            if (device != null)
            {
                device.StopPolling();
                device.Disconnect(true);
                HANDLE_DIC.Remove(SerialNo);
            }
        }



    }
}
