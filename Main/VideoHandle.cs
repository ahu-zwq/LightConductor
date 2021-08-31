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

        public string errorMsg = "";

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
                    Log.Error("Login Failed！参数为空！");
                    //throw new Exception("参数为空！Login Failed!");
                    errorMsg = "注册失败！参数为空！";
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
                struLogInfo.bUseAsynLogin = false; //是否异步注册：0- 否，1- 是 

                DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

                //注册设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V40 failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr]; //注册失败，输出错误号
                    Log.ErrorFormat(str + ", {0},{1},{2},{3}", ip, port, userName, password);
                    //throw new Exception("注册失败，" + iLastErr);
                    errorMsg = "注册失败，" + ERROR_DIC[(int)iLastErr];
                    return;
                }
                else
                {
                    //注册成功
                    Log.InfoFormat("*** VIDEO Login Success! {0},{1},{2},{3}", ip, port, userName, password);

                    //addHandle(ip);

                }

            }
            else
            {
                //注销注册 Logout the device
                if (m_lRealHandle >= 0)
                {
                    Log.Error("Please stop live view firstly");
                    //throw new Exception("Please stop live view firstly");
                    errorMsg = "Please stop live view firstly";
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr];
                    Log.Error(str);
                    //throw new Exception("注销失败，" + iLastErr);
                    errorMsg = "注销失败，" + iLastErr + ERROR_DIC[(int)iLastErr];
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

            //注销注册 Logout the device
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

        }

        public static IntPtr GetPictureHandle(WindowsFormsHost windowsFormsHost) 
        {
            System.Windows.Forms.PictureBox pictureBox = windowsFormsHost.Child as System.Windows.Forms.PictureBox;
            return pictureBox.Handle;
        }


        public void btnPreview_Click(WindowsFormsHost windowsFormsHost)
        {
            pictureBox = windowsFormsHost.Child as System.Windows.Forms.PictureBox;
            btnPreview_Click(pictureBox.Handle);
        }

        public void btnPreview_Click(IntPtr handle)
        {
            if (m_lUserID < 0)
            {
                Log.Error("Please login the device firstly");
                //throw new Exception("预览失败，" + "Please login the device firstly");
                //errorMsg = "Please login the device firstly";
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();

                //pictureBox = windowsFormsHost.Child as System.Windows.Forms.PictureBox;
                lpPreviewInfo.hPlayWnd = handle;//预览窗口
                //lpPreviewInfo.lChannel = Int16.Parse(textBoxChannel.Text);//预te览的设备通道
                lpPreviewInfo.lChannel = 1;
                lpPreviewInfo.dwStreamType = (uint)Properties.Settings.Default.VideoStreamType;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
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
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr]; //预览失败，输出错误号
                    Log.Error(str);
                    //throw new Exception("预览失败，" + iLastErr);
                    errorMsg = "预览失败，" + ERROR_DIC[(int)iLastErr];
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
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr];
                    Log.Error(str);
                    //throw new Exception("停止预览失败，" + iLastErr);
                    //errorMsg = "停止预览失败，" + iLastErr;
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
            string strLoginCallBack = "注册设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

            if (dwResult == 0)
            {
                uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                strLoginCallBack = strLoginCallBack + "，错误号:" + iErrCode + ERROR_DIC[(int)iErrCode];
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
                str = "NET_DVR_CapturePicture failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr];
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
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr];
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
                    str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr + ERROR_DIC[(int)iLastErr];
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
            DateTime t1 = DateTime.Now;
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }

            DateTime t2 = DateTime.Now;
            Log.Info("cam1 : " + t2.Subtract(t1).TotalMilliseconds);

            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }

            DateTime t3 = DateTime.Now;
            Log.Info("cam2 : " + t3.Subtract(t1).TotalMilliseconds);

            if (pictureBox != null)
            {
                pictureBox.Refresh();
            }

            DateTime t4 = DateTime.Now;
            Log.Info("cam3 : " + t4.Subtract(t1).TotalMilliseconds);

            if (HANDLE_DIC.Count > 0)
            {
                HANDLE_DIC = new Dictionary<string, List<VideoHandle>>();
            }

            Dispose();

            DateTime t5 = DateTime.Now;
            Log.Info("cam4 : " + t5.Subtract(t1).TotalMilliseconds);
        }

        /// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		public void Dispose()
        {
            //DateTime t1 = DateTime.Now;

            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }

            //DateTime t2 = DateTime.Now;
            //Log.Info("cam01 : " + t2.Subtract(t1).TotalMilliseconds);

            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }

            //DateTime t3 = DateTime.Now;
            //Log.Info("cam02 : " + t3.Subtract(t1).TotalMilliseconds);

            if (m_bInitSDK == true)
            {
                CHCNetSDK.NET_DVR_Cleanup();
                m_bInitSDK = false;
            }

            //DateTime t4 = DateTime.Now;
            //Log.Info("cam03 : " + t4.Subtract(t1).TotalMilliseconds);

        }

        public void RefreshPicture()
        {
            if (pictureBox != null)
            {
                pictureBox.Refresh();
            }
        }

        public void RefreshPicture(System.Windows.Forms.PictureBox pictureBoxItem)
        {
            if (pictureBoxItem != null)
            {
                pictureBoxItem.Refresh();
            }
        }



        private static Dictionary<int, string> ERROR_DIC = new Dictionary<int, string>();

        static VideoHandle() 
        {
            ERROR_DIC.Add(0, "没有错误。");
            ERROR_DIC.Add(1, "用户名密码错误。注册时输入的用户名或者密码错误。");
            ERROR_DIC.Add(2, "权限不足。该注册用户没有权限执行当前对设备的操作，可以与远程用户参数配置做对比。");
            ERROR_DIC.Add(3, "SDK	未初始化。");
            ERROR_DIC.Add(4, "通道号错误。设备没有对应的通道号。");
            ERROR_DIC.Add(5, "连接到设备的用户个数超过最大。");
            ERROR_DIC.Add(6, "版本不匹配。SDK	和设备的版本不匹配。");
            ERROR_DIC.Add(7, "连接设备失败。设备不在线或网络原因引起的连接超时等。");
            ERROR_DIC.Add(8, "向设备发送失败。");
            ERROR_DIC.Add(9, "从设备接收数据失败。");
            ERROR_DIC.Add(10, "从设备接收数据超时。");
            ERROR_DIC.Add(11, "传送的数据有误。发送给设备或者从设备接收到的数据错误，如远程参数配置时输入设备不支持的值。");
            ERROR_DIC.Add(12, "调用次序错误。");
            ERROR_DIC.Add(13, "无此权限。");
            ERROR_DIC.Add(14, "设备命令执行超时。");
            ERROR_DIC.Add(15, "串口号错误。指定的设备串口号不存在。");
            ERROR_DIC.Add(16, "报警端口错误。指定的设备报警输出端口不存在。");
            ERROR_DIC.Add(17, "参数错误。SDK接口中给入的输入或输出参数为空。");
            ERROR_DIC.Add(18, "设备通道处于错误状态");
            ERROR_DIC.Add(19, "设备无硬盘。当设备无硬盘时，对设备的录像文件、硬盘配置等操作失败。");
            ERROR_DIC.Add(20, "硬盘号错误。当对设备进行硬盘管理操作时，指定的硬盘号不存在时返回该错误。");
            ERROR_DIC.Add(21, "设备硬盘满。");
            ERROR_DIC.Add(22, "设备硬盘出错");
            ERROR_DIC.Add(23, "设备不支持。");
            ERROR_DIC.Add(24, "设备忙。");
            ERROR_DIC.Add(25, "设备修改不成功。");
            ERROR_DIC.Add(26, "密码输入格式不正确");
            ERROR_DIC.Add(27, "硬盘正在格式化，不能启动操作。");
            ERROR_DIC.Add(28, "设备资源不足。");
            ERROR_DIC.Add(29, "设备操作失败。");
            ERROR_DIC.Add(30, "语音对讲、语音广播操作中采集本地音频或打开音频输出失败。");
            ERROR_DIC.Add(31, "设备语音对讲被占用。");
            ERROR_DIC.Add(32, "时间输入不正确。");
            ERROR_DIC.Add(33, "回放时设备没有指定的文件。");
            ERROR_DIC.Add(34, "创建文件出错。本地录像、保存图片、获取配置文件和远程下载录像时创建文件失败。");
            ERROR_DIC.Add(35, "打开文件出错。设置配置文件、设备升级、上传审讯文件时打开文件失败。");
            ERROR_DIC.Add(36, "上次的操作还没有完成");
            ERROR_DIC.Add(37, "获取当前播放的时间出错。");
            ERROR_DIC.Add(38, "播放出错。");
            ERROR_DIC.Add(39, "文件格式不正确。");
            ERROR_DIC.Add(40, "路径错误");
            ERROR_DIC.Add(41, "SDK	资源分配错误。");
            ERROR_DIC.Add(42, "声卡模式错误。当前打开声音播放模式与实际设置的模式不符出错。");
            ERROR_DIC.Add(43, "缓冲区太小。接收设备数据的缓冲区或存放图片缓冲区不足。");
            ERROR_DIC.Add(44, "创建SOCKET	出错。");
            ERROR_DIC.Add(45, "设置SOCKET	出错。");
            ERROR_DIC.Add(46, "个数达到最大。分配的注册连接数、预览连接数超过SDK	支持的最大数。");
            ERROR_DIC.Add(47, "用户不存在。注册的用户ID	已注销或不可用。");
            ERROR_DIC.Add(48, "写FLASH	出错。设备升级时写FLASH	失败。");
            ERROR_DIC.Add(49, "设备升级失败。网络或升级文件语言不匹配等原因升级失败。");
            ERROR_DIC.Add(50, "解码卡已经初始化过。");
            ERROR_DIC.Add(51, "调用播放库中某个函数失败。");
            ERROR_DIC.Add(52, "登录设备的用户数达到最大。");
            ERROR_DIC.Add(53, "获得本地PC	的IP	地址或物理地址失败。");
            ERROR_DIC.Add(54, "设备该通道没有启动编码。");
            ERROR_DIC.Add(55, "IP	地址不匹配。");
            ERROR_DIC.Add(56, "MAC	地址不匹配。");
            ERROR_DIC.Add(57, "升级文件语言不匹配。");
            ERROR_DIC.Add(58, "播放器路数达到最大。");
            ERROR_DIC.Add(59, "备份设备中没有足够空间进行备份。");
            ERROR_DIC.Add(60, "没有找到指定的备份设备。");
            ERROR_DIC.Add(61, "图像素位数不符，限24	色。");
            ERROR_DIC.Add(62, "图片高*宽超限，限128*256。");
            ERROR_DIC.Add(63, "图片大小超限，限100K。");
            ERROR_DIC.Add(64, "载入当前目录下Player	Sdk	出错。");
            ERROR_DIC.Add(65, "找不到Player	Sdk	中某个函数入口。");
            ERROR_DIC.Add(66, "载入当前目录下DSsdk	出错。");
            ERROR_DIC.Add(67, "找不到DsSdk	中某个函数入口。");
            ERROR_DIC.Add(68, "调用硬解码库DsSdk	中某个函数失败。");
            ERROR_DIC.Add(69, "声卡被独占。");
            ERROR_DIC.Add(70, "加入多播组失败。");
            ERROR_DIC.Add(71, "建立日志文件目录失败。");
            ERROR_DIC.Add(72, "绑定套接字失败。");
            ERROR_DIC.Add(73, "socket	连接中断，此错误通常是由于连接中断或目的地不可达。");
            ERROR_DIC.Add(74, "注销时用户ID	正在进行某操作。");
            ERROR_DIC.Add(75, "监听失败。");
            ERROR_DIC.Add(76, "程序异常。");
            ERROR_DIC.Add(77, "写文件失败。本地录像、远程下载录像、下载图片等操作时写文件失败。");
            ERROR_DIC.Add(78, "禁止格式化只读硬盘。");
            ERROR_DIC.Add(79, "远程用户配置结构中存在相同的用户名。");
            ERROR_DIC.Add(80, "导入参数时设备型号不匹配。");
            ERROR_DIC.Add(81, "导入参数时语言不匹配。");
            ERROR_DIC.Add(82, "导入参数时软件版本不匹配。");
            ERROR_DIC.Add(83, "预览时外接IP	通道不在线。");
            ERROR_DIC.Add(84, "加载标准协议通讯库StreamTransClient	失败。");
            ERROR_DIC.Add(85, "加载转封装库失败。");
            ERROR_DIC.Add(86, "超出最大的IP	接入通道数。");
            ERROR_DIC.Add(87, "添加录像标签或者其他操作超出最多支持的个数。");
            ERROR_DIC.Add(88, "图像增强仪，参数模式错误（用于硬件设置时，客户端进行软件设置时错误值）。");
            ERROR_DIC.Add(89, "码分器不在线。");
            ERROR_DIC.Add(90, "设备正在备份。");
            ERROR_DIC.Add(91, "通道不支持该操作。");
            ERROR_DIC.Add(92, "高度线位置太集中或长度线不够倾斜。");
            ERROR_DIC.Add(93, "取消标定冲突，如果设置了规则及全局的实际大小尺寸过滤。");
            ERROR_DIC.Add(94, "标定点超出范围。");
            ERROR_DIC.Add(95, "尺寸过滤器不符合要求。");
            ERROR_DIC.Add(96, "设备没有注册到ddns	上。");
            ERROR_DIC.Add(97, "DDNS	服务器内部错误。");
            ERROR_DIC.Add(100, "加载当前目录下的语音对讲库失败。");
            ERROR_DIC.Add(101, "没有正确的升级包。");
            ERROR_DIC.Add(150, "别名重复（EasyDDNS	的配置）");
            ERROR_DIC.Add(153, "用户名被锁定。");
            ERROR_DIC.Add(165, "连接测试服务器失败。");
            ERROR_DIC.Add(166, "NAS	服务器挂载目录失败，目录无效或者用户名密码错误。");
            ERROR_DIC.Add(167, "NAS	服务器挂载目录失败，没有权限。");
            ERROR_DIC.Add(168, "服务器使用域名，但是没有配置DNS，可能造成域名无效。");
            ERROR_DIC.Add(169, "没有配置网关，可能造成发送邮件失败。");
            ERROR_DIC.Add(170, "用户名密码不正确，测试服务器的用户名或密码错误。");
            ERROR_DIC.Add(171, "设备和smtp	服务器交互异常。");
            ERROR_DIC.Add(172, "FTP	服务器创建目录失败。");
            ERROR_DIC.Add(173, "FTP	服务器没有写入权限。");
            ERROR_DIC.Add(174, "IP	冲突。");
            ERROR_DIC.Add(178, "断网续传布防连接已经存在（私有SDK	协议布防连接已经建立的情况下，重复布防且选择断网续传功能时返回该错误）。");
            ERROR_DIC.Add(179, "断网续传上传连接已经存在（EHOME	协议和私有SDK协议不能同时支持断网续传，其中一种协议已经建议连接，另外一个连接建立时返回该错误）。");
            ERROR_DIC.Add(182, "HRUDP	连接数超过设备限制。");
            ERROR_DIC.Add(791, "设备其它功能占用资源，导致该功能无法开启。");
            ERROR_DIC.Add(800, "网络流量超过设备能力上限");
            ERROR_DIC.Add(801, "录像文件在录像，无法被锁定");
            ERROR_DIC.Add(802, "由于硬盘太小无法格式化");
            ERROR_DIC.Add(834, "校验密码错误。");
            ERROR_DIC.Add(1101, "透明通道已打开，当前操作无法完成。");
            ERROR_DIC.Add(1102, "设备正在升级");
            ERROR_DIC.Add(1103, "升级包类型不匹配");
            ERROR_DIC.Add(1104, "设备正在格式化");
            ERROR_DIC.Add(1105, "升级包版本不匹配");
            ERROR_DIC.Add(1111, "验证码不合法，请修改验证码");
            ERROR_DIC.Add(1112, "缺少验证码，请输入验证码能力集错误码");
            ERROR_DIC.Add(1000, "不支持能力节点获取。");
            ERROR_DIC.Add(1001, "输出内存不足。");
            ERROR_DIC.Add(1002, "无法找到对应的本地xml。");
            ERROR_DIC.Add(1003, "加载本地xml	出错。");
            ERROR_DIC.Add(1004, "设备能力数据格式错误。");
            ERROR_DIC.Add(1005, "能力集类型错误。");
            ERROR_DIC.Add(1006, "XML	能力节点格式错误。");
            ERROR_DIC.Add(1007, "输入的能力XML	节点值错误。");
            ERROR_DIC.Add(1008, "XML	版本不匹配。");
            ERROR_DIC.Add(401, "无权限：服务器返回401	时，转成这个错误码");
            ERROR_DIC.Add(402, "分配资源失败");
            ERROR_DIC.Add(403, "参数错误");
            ERROR_DIC.Add(404, "指定的URL	地址不存在：服务器返回404	时，转成这个");
            ERROR_DIC.Add(406, "用户中途强行退出");
            ERROR_DIC.Add(407, "获取RTSP	端口错误");
            ERROR_DIC.Add(410, "RTSP	DECRIBE	交互错误");
            ERROR_DIC.Add(411, "RTSP	DECRIBE	发送超时");
            ERROR_DIC.Add(412, "RTSP	DECRIBE	发送失败");
            ERROR_DIC.Add(413, "RTSP	DECRIBE	接收超时");
            ERROR_DIC.Add(414, "RTSP	DECRIBE	接收数据错误");
            ERROR_DIC.Add(415, "RTSP	DECRIBE	接收失败");
            ERROR_DIC.Add(416, "RTSP	DECRIBE	服务器返回错误状态。例如服务器返回400，可能是不支持子码流");
            ERROR_DIC.Add(420, "RTSP	SETUP	交互错误，一般是服务器返回的码流地址无法连接上，或者被服务器拒绝。（老版本的SDK	可能返回错误号419，为同样的错误原因）");
            ERROR_DIC.Add(421, "RTSP	SETUP	发送超时");
            ERROR_DIC.Add(422, "RTSP	SETUP	发送错误");
            ERROR_DIC.Add(423, "RTSP	SETUP	接收超时");
            ERROR_DIC.Add(424, "RTSP	SETUP	接收数据错误");
            ERROR_DIC.Add(425, "RTSP	SETUP	接收失败");
            ERROR_DIC.Add(426, "超过服务器最大连接数，或者服务器资源不足，服务器返回453	时，转成这个错误码");
            ERROR_DIC.Add(427, "RTSP	SETUP	服务器返回错误状态");
            ERROR_DIC.Add(430, "RTSP	PLAY	交互错误");
            ERROR_DIC.Add(431, "RTSP	PLAY	发送超时");
            ERROR_DIC.Add(432, "RTSP	PLAY	发送错误");
            ERROR_DIC.Add(433, "RTSP	PLAT	接收超时");
            ERROR_DIC.Add(434, "RTSP	PLAY	接收数据错误");
            ERROR_DIC.Add(435, "RTSP	PLAY	接收失败");
            ERROR_DIC.Add(436, "RTSP	PLAY	服务器返回错误状态");
            ERROR_DIC.Add(440, "RTSP	TEARDOWN	交互错误");
            ERROR_DIC.Add(441, "RTSP	TEARDOWN	发送超时");
            ERROR_DIC.Add(442, "RTSP	TEARDOWN	发送错误");
            ERROR_DIC.Add(443, "RTSP	TEARDOWN	接收超时");
            ERROR_DIC.Add(444, "RTSP	TEARDOWN	接收数据错误");
            ERROR_DIC.Add(445, "RTSP	TEARDOWN	接收失败");
            ERROR_DIC.Add(446, "RTSP	TEARDOWN	服务器返回错误状态");
            ERROR_DIC.Add(500, "没有错误");
            ERROR_DIC.Add(501, "输入参数非法");
            ERROR_DIC.Add(502, "调用顺序不对");
            ERROR_DIC.Add(503, "多媒体时钟设置失败");
            ERROR_DIC.Add(504, "视频解码失败");
            ERROR_DIC.Add(505, "音频解码失败");
            ERROR_DIC.Add(506, "分配内存失败");
            ERROR_DIC.Add(507, "文件操作失败");
            ERROR_DIC.Add(508, "创建线程事件等失败");
            ERROR_DIC.Add(509, "创建directDraw	失败");
            ERROR_DIC.Add(510, "创建后端缓存失败");
            ERROR_DIC.Add(511, "缓冲区满，输入流失败");
            ERROR_DIC.Add(512, "创建音频设备失败");
            ERROR_DIC.Add(513, "设置音量失败");
            ERROR_DIC.Add(514, "只能在播放文件时才能使用此接口");
            ERROR_DIC.Add(515, "只能在播放流时才能使用此接口");
            ERROR_DIC.Add(516, "系统不支持，解码器只能工作在Pentium	3	以上");
            ERROR_DIC.Add(517, "没有文件头");
            ERROR_DIC.Add(518, "解码器和编码器版本不对应");
            ERROR_DIC.Add(519, "初始化解码器失败");
            ERROR_DIC.Add(520, "文件太短或码流无法识别");
            ERROR_DIC.Add(521, "初始化多媒体时钟失败");
            ERROR_DIC.Add(522, "位拷贝失败");
            ERROR_DIC.Add(523, "显示overlay	失败");
            ERROR_DIC.Add(524, "打开混合流文件失败");
            ERROR_DIC.Add(525, "打开视频流文件失败");
            ERROR_DIC.Add(526, "JPEG	压缩错误");
            ERROR_DIC.Add(527, "不支持该文件版本.");
            ERROR_DIC.Add(528, "提取文件数据失败");
            ERROR_DIC.Add(600, "没有错误");
            ERROR_DIC.Add(601, "不支持");
            ERROR_DIC.Add(602, "内存申请错误");
            ERROR_DIC.Add(603, "参数错误");
            ERROR_DIC.Add(604, "调用次序错误");
            ERROR_DIC.Add(605, "未发现设备");
            ERROR_DIC.Add(606, "不能打开设备");
            ERROR_DIC.Add(607, "设备上下文出错");
            ERROR_DIC.Add(608, "WAV	文件出错");
            ERROR_DIC.Add(609, "无效的WAV	参数类型");
            ERROR_DIC.Add(610, "编码失败");
            ERROR_DIC.Add(611, "解码失败");
            ERROR_DIC.Add(612, "播放失败");
            ERROR_DIC.Add(613, "降噪失败");
            ERROR_DIC.Add(619, "未知错误");

        }

    }
}
