using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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

using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;

using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;
using log4net;
using System.Reflection;
using LightConductor.Main;

namespace LightConductor
{
    class VideoHandle : IDisposable
    {
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private static bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private bool m_bTalk = false;
        private Int32 m_lRealHandle = -1;
        private int lVoiceComHandle = -1;
        private string str;

        CHCNetSDK.REALDATACALLBACK RealData = null;
        CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;
        public CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;

        public static int MAX_LINK = 6;

        private System.Windows.Forms.PictureBox pictureBox = null;

        private static Dictionary<String, List<VideoHandle>> HANDLE_DIC = new Dictionary<string, List<VideoHandle>>();

        private string ip;

        public VideoHandle(string ip)
        {
            this.ip = ip;
        }

        private VideoHandle()
        {

        }

        public static VideoHandle GetVideoHandle(string Ip)
        {
            if (string.IsNullOrWhiteSpace(Ip))
            {
                return new VideoHandle();
            }
            return new VideoHandle(Ip);

        }

        public static void Init()
        {
            Log.Info("*** VIDEO video init");
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                Log.Error("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\LC\\HK-SdkLog\\", true);
            }
        }

        public void Login(string ip, string port, string userName, string password)
        {
            if (m_lUserID < 0)
            {
                if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port)
                    || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    Log.Error("参数为空！Login Failed!");
                    return;
                }

                struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

                //设备IP地址或者域名
                byte[] byIP = System.Text.Encoding.Default.GetBytes(ip);
                struLogInfo.sDeviceAddress = new byte[129];
                byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

                //设备用户名
                byte[] byUserName = System.Text.Encoding.Default.GetBytes(userName);
                struLogInfo.sUserName = new byte[64];
                byUserName.CopyTo(struLogInfo.sUserName, 0);

                //设备密码
                byte[] byPassword = System.Text.Encoding.Default.GetBytes(password);
                struLogInfo.sPassword = new byte[64];
                byPassword.CopyTo(struLogInfo.sPassword, 0);

                struLogInfo.wPort = ushort.Parse(port);//设备服务端口号

                if (LoginCallBack == null)
                {
                    LoginCallBack = new CHCNetSDK.LOGINRESULTCALLBACK(cbLoginCallBack);//注册回调函数                    
                }
                struLogInfo.cbLoginResult = LoginCallBack;
                struLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 

                DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V40 failed, error code= " + iLastErr; //登录失败，输出错误号
                    Log.ErrorFormat(str + ", {0},{1},{2},{3}", ip, port, userName, password);
                    return;
                }
                else
                {
                    //登录成功
                    Log.InfoFormat("*** VIDEO Login Success! {0},{1},{2},{3}", ip, port, userName, password);

                    //addHandle(ip);

                }

            }
            else
            {
                //注销登录 Logout the device
                if (m_lRealHandle >= 0)
                {
                    Log.Error("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    Log.Error(str);
                    return;
                }
                m_lUserID = -1;

            }
            return;
        }



        private void addHandle(string ip)
        {
            if (HANDLE_DIC.ContainsKey(ip))
            {
                List<VideoHandle> ip_handles = HANDLE_DIC[ip];
                if (ip_handles != null)
                {
                    ip_handles.Add(this);
                }
            }
            else
            {
                List<VideoHandle> ip_handles = new List<VideoHandle>();
                ip_handles.Add(this);
                HANDLE_DIC.Add(ip, ip_handles);
            }
        }


        public void btn_Exit_Click()
        {
            //停止预览 Stop live view 
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //注销登录 Logout the device
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

        }

        public void btnPreview_Click(WindowsFormsHost windowsFormsHost)
        {
            if (m_lUserID < 0)
            {
                Log.Error("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();

                pictureBox = windowsFormsHost.Child as System.Windows.Forms.PictureBox;
                lpPreviewInfo.hPlayWnd = pictureBox.Handle;//预览窗口
                //lpPreviewInfo.lChannel = Int16.Parse(textBoxChannel.Text);//预te览的设备通道
                lpPreviewInfo.lChannel = 1;
                lpPreviewInfo.dwStreamType = (uint)Properties.Settings.Default.StreamType;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = false; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 1; //播放库播放缓冲区最大缓冲帧数
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;

                //if (textBoxID.Text != "")
                //{
                //    lpPreviewInfo.lChannel = -1;
                //    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                //    lpPreviewInfo.byStreamID = new byte[32];
                //    byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
                //}


                if (RealData == null)
                {
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数
                }

                IntPtr pUser = new IntPtr();//用户数据

                //打开预览 Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                    Log.Error(str);
                    return;
                }
                else
                {
                    //预览成功
                    Log.InfoFormat("*** VIDEO Live View, {0}", ip);
                }
            }
            else
            {
                //停止预览 Stop live view 
                StopRealPlay();

            }
            return;
        }

        //停止预览 Stop live view 
        public void StopRealPlay()
        {
            if (m_lRealHandle > -1)
            {
                //停止预览 Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    Log.Error(str);
                    return;
                }
                m_lRealHandle = -1;
            }
            Log.InfoFormat("*** VIDEO Stop Live View, {0}", ip);
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                string str = "实时流数据.ps";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();
            }
        }

        public void cbLoginCallBack(int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {
            string strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

            if (dwResult == 0)
            {
                uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                strLoginCallBack = strLoginCallBack + "，错误号:" + iErrCode;
            }

        }


        public void btnBMP_Click()
        {
            string sBmpPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sBmpPicFileName = "BMP_test_2.bmp";

            //BMP抓图 Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandle, sBmpPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CapturePicture failed, error code= " + iLastErr;
                Log.Error(str);
                return;
            }
            else
            {
                str = "Successful to capture the BMP file and the saved file is " + sBmpPicFileName;
                Log.Info(str);
            }
            return;
        }

        public Boolean btnJPEG_File(string sJpegPicFileName)
        {
            //string sJpegPicFileName;
            //图片保存路径和文件名 the path and file name to save
            //sJpegPicFileName = "JPEG_test_2.jpg";
            Boolean flag = false;
            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                Log.Error(str);
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                Log.Info(str);
                flag = true;
            }
            return flag;
        }

        public byte[] btnJPEG_Byte()
        {
            //byte[] bytes = new byte[0];
            if (m_lUserID > -1)
            {
                CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
                lpJpegPara.wPicQuality = 0;
                lpJpegPara.wPicSize = 0xff;
                uint dwPicSize = 1 * 1024 * 1024;
                uint lpSizeReturned = 0;
                int lChannel = 1;
                byte[] p = new byte[1 * 1024 * 1024];


                if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(m_lUserID, lChannel, ref lpJpegPara, p, dwPicSize, ref lpSizeReturned))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr;
                    Log.Error(str);
                }
                else
                {
                    str = "Successful to capture the JPEG to Memory";
                    Log.Info(str);
                }

                if (lpSizeReturned > 0)
                {
                    byte[] bytes = new byte[lpSizeReturned];
                    Array.Copy(p, bytes, lpSizeReturned);
                    return bytes;
                }
                //bytes = p;
            }
            return new byte[0];
        }

        public void Stop()
        {
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }
           
            if (pictureBox != null)
            {
                pictureBox.Refresh();
            }
            if (HANDLE_DIC.Count > 0)
            {
                HANDLE_DIC = new Dictionary<string, List<VideoHandle>>();
            }

        }

        /// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		public void Dispose()
        {
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }
            if (m_bInitSDK == true)
            {
                CHCNetSDK.NET_DVR_Cleanup();
                m_bInitSDK = false;
            }
            

        }

        public void RefreshPicture()
        {
            if (pictureBox != null)
            {
                pictureBox.Refresh();
            }
        }


    }
}
