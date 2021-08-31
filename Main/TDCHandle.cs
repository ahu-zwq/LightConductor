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
using System.Runtime.CompilerServices;

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
                //DateTime t1 = DateTime.Now;

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

                //DateTime t2 = DateTime.Now;
                //Log.Info("tdc : " + t2.Subtract(t1).TotalMilliseconds);

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


        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static Dictionary<String, TDCHandle> getTDCHandle(HashSet<string> serialSet)
        {
            Dictionary<String, TDCHandle> handleMap = new Dictionary<string, TDCHandle>();
            Parallel.ForEach(serialSet, item =>
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    TDCHandle tDCHandle = new TDCHandle(item);
                    handleMap.Add(item, tDCHandle);
                }
            });

            List<string> list = handleMap.Keys.ToList();
            list.ForEach(f =>
            {
                if (!HANDLE_DIC.ContainsKey(f))
                {
                    HANDLE_DIC.Add(f, handleMap[f]);
                }
            });

            return handleMap;
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
        //private decimal minPosition = Properties.Settings.Default.MinTDCPositon;
        //private decimal maxPosition = Properties.Settings.Default.MaxTDCPositon;

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
                        if (nowPos > device.MotorDeviceSettings.Physical.MinPosUnit)
                        {
                            _taskID = device.MoveTo(device.MotorDeviceSettings.Physical.MinPosUnit, CommandCompleteFunction);
                        }
                        else
                        {
                            throw new Exception("已移动到最小！");
                        }
                        break;
                    case MotorDirection.Forward:
                        if (nowPos < device.MotorDeviceSettings.Physical.MaxPosUnit)
                        {
                            _taskID = device.MoveTo(device.MotorDeviceSettings.Physical.MaxPosUnit, CommandCompleteFunction);
                        }
                        else
                        {
                            throw new Exception("已移动到最大！");
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

        public Decimal getMaxVel()
        {
            try
            {
                return device.MotorDeviceSettings.Physical.MaxVelUnit;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Failed getPosition, {0}", e.Message);
            }
            return 2.6M;
        }


        /// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		public void Dispose()
        {
            if (device != null)
            {
                DateTime t1 = DateTime.Now;

                device.StopPolling();

                //DateTime t2 = DateTime.Now;
                //Log.Info("tdc0 : " + t2.Subtract(t1).TotalMilliseconds);

                device.Disconnect(true);

                DateTime t3 = DateTime.Now;
                Log.Info("tdc1 : " + t3.Subtract(t1).TotalMilliseconds);

                HANDLE_DIC.Remove(SerialNo);
            }
        }



    }
}
