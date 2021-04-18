using LightConductor.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using log4net;
using System.Reflection;
using System.Configuration;
using System.Runtime.CompilerServices;
using LightConductor.Properties;
using Thorlabs.MotionControl.GenericMotorCLI;

namespace LightConductor.Pages
{
    /// <summary>
    /// Monitor.xaml 的交互逻辑
    /// </summary>
    public partial class Monitor : Page
    {

        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static List<CameraPair> CAMERA_PAIR_LIST = new List<CameraPair>();

        //VideoHandle videoHandle_v1;
        //VideoHandle videoHandle_v2;
        CameraPair cameraPair_v1;
        CameraPair cameraPair_v2;
        //TDCHandle tdcHandle_v;
        //TDCHandle tdcHandle_h;
        //public static string IMAGE_V1_PATH = "D:\\LC\\image_v1.jpg";
        //public static string IMAGE_V2_PATH = "D:\\LC\\image_v2.jpg";
        //public static string IMAGE_TEMP_PATH = "D:\\LC\\image_v2.jpg";
        //public static int TIME_MILLISECONDS = Convert.ToInt32(ConfigurationManager.AppSettings["catch_position_interval"]);
        public static int TDC_MAX_VELOCITY = Convert.ToInt32(ConfigurationManager.AppSettings["tdc_max_velocity"]);
        public static string SPOT_LOCATIOIN_METHOD = ConfigurationManager.AppSettings["spot_location_method"];
        public static string TMP = System.Environment.GetEnvironmentVariable("TMP") + System.IO.Path.DirectorySeparatorChar + "LC" + System.IO.Path.DirectorySeparatorChar;
        public static string REFRESH_RATE_NAME = "refresh_rate";
        public static int REFRESH_RATE_DEFAULT = 60;

        //光斑中心坐标
        private SpotPosition SpotPosition_v1_mark = new SpotPosition(0, 0);
        private SpotPosition SpotPosition_v1_real = new SpotPosition(0, 0);
        private SpotPosition SpotPosition_v2_mark = new SpotPosition(0, 0);
        private SpotPosition SpotPosition_v2_real = new SpotPosition(0, 0);

        private PicLabel picLabel_v1 = new PicLabel("");
        private PicLabel picLabel_v2 = new PicLabel("");

        private PicLabel velocityLabel = new PicLabel("1");

        private PicLabel picLabel_t1 = new PicLabel("");
        private PicLabel picLabel_t2 = new PicLabel("");
        private PicLabel picLabel_t3 = new PicLabel("");
        private PicLabel picLabel_t4 = new PicLabel("");
        private PicLabel picLabel_t5 = new PicLabel("");
        private PicLabel picLabel_t6 = new PicLabel("");
        private PicLabel picLabel_t7 = new PicLabel("");

        private TDCDetail tdc_detail = new TDCDetail();

        private System.Timers.Timer mainTimer = new System.Timers.Timer();


        public Monitor()
        {
            InitializeComponent();

            ThreadPool.SetMaxThreads(20, 10);

            InitBinding();

            AddButtonHandler();

            OpenAllCameraAndTDC();

            startTimer();

        }

        private void AddButtonHandler()
        {

            //btn.AddHandler(Button.MouseDownEvent, new MouseButtonEventHandler(Btn_MouseDown));
            //btn.AddHandler(Button.MouseUpEvent, new MouseButtonEventHandler(Btn_MouseUp));

            Up.AddHandler(Button.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Btn_MouseDown));
            Up.AddHandler(Button.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(Btn_MouseUp));
            Down.AddHandler(Button.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Btn_MouseDown));
            Down.AddHandler(Button.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(Btn_MouseUp));
            Left.AddHandler(Button.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Btn_MouseDown));
            Left.AddHandler(Button.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(Btn_MouseUp));
            Right.AddHandler(Button.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Btn_MouseDown));
            Right.AddHandler(Button.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(Btn_MouseUp));

        }

        private void InitBinding()
        {
            Log.Info("InitPoint");
            //SpotPosition_v1_mark = new SpotPosition(0, 0);
            //SpotPosition_v1_real = new SpotPosition(0, 0);
            pointBinding(SpotPosition_v1_mark, null, rect_v1_s, line_h_v1_s, line_v_v1_s, text_v1_s);
            pointBinding(SpotPosition_v1_real, point_v1_n, null, line_h_v1_n, line_v_v1_n, text_v1_n);

            //SpotPosition_v2_mark = new SpotPosition(0, 0);
            //SpotPosition_v2_real = new SpotPosition(0, 0);
            pointBinding(SpotPosition_v2_mark, null, rect_v2_s, line_h_v2_s, line_v_v2_s, text_v2_s);
            pointBinding(SpotPosition_v2_real, point_v2_n, null, line_h_v2_n, line_v_v2_n, text_v2_n);

            //picLabel_v1 = new PicLabel("");
            //picLabel_v2 = new PicLabel("");
            labelBinding(picLabel_v1, label_v1);
            labelBinding(picLabel_v2, label_v2);

            velocityBinding(velocityLabel, velocity_tb);

            //picLabel_t1 = new PicLabel("");
            //picLabel_t2 = new PicLabel("");
            //picLabel_t3 = new PicLabel("");
            //picLabel_t4 = new PicLabel("");
            //picLabel_t5 = new PicLabel("");
            //picLabel_t6 = new PicLabel("");
            //picLabel_t7 = new PicLabel("");
            labelBinding(picLabel_t1, top_label_1);
            labelBinding(picLabel_t2, top_label_2);
            labelBinding(picLabel_t3, top_label_3);
            labelBinding(picLabel_t4, top_label_4);
            labelBinding(picLabel_t5, top_label_5);
            labelBinding(picLabel_t6, top_label_6);
            labelBinding(picLabel_t7, top_label_7);


            t_tdc_detail.SetBinding(TextBlock.TextProperty, new Binding("TdcDetail") { Source = tdc_detail, Mode = BindingMode.OneWay });

            tb_refresh_rate.Text = UserSettings.Instance.Get(REFRESH_RATE_NAME, "" + REFRESH_RATE_DEFAULT);
            //UIElementCollection children = top_devices.Children;
            //int tb = 0;
            //int wf = 0;
            //for (int i = 0; i < children.Count; i++)
            //{
            //    UIElement uIElement = children[i];
            //    if (uIElement is TextBlock)
            //    {
            //        TextBlock block = uIElement as TextBlock;
            //        block.Text = CAMERA_PAIR_LIST[tb].DeviceModule.Name;
            //        tb++;
            //    }
            //    if (uIElement is WindowsFormsHost)
            //    {
            //        WindowsFormsHost host = uIElement as WindowsFormsHost;
            //        CAMERA_PAIR_LIST[wf].TopVideoHandle.btnPreview_Click(host);
            //        wf++;
            //    }

            //}
        }

        private void labelBinding(PicLabel label, TextBlock text)
        {
            text.SetBinding(TextBlock.TextProperty, new Binding("Pic_label") { Source = label, Mode = BindingMode.OneWay });

        }
        private void velocityBinding(PicLabel label, TextBox text)
        {
            text.SetBinding(TextBox.TextProperty, new Binding("Pic_label") { Source = label, Mode = BindingMode.TwoWay });

        }


        private void pointBinding(SpotPosition spot, Ellipse point, Rectangle rect, Line line_h, Line line_v, TextBlock text)
        {
            if (point != null)
            {
                point.SetBinding(Ellipse.MarginProperty, new Binding("P_margin") { Source = spot, Mode = BindingMode.OneWay });
                point.SetBinding(Ellipse.WidthProperty, new Binding("P_width") { Source = spot, Mode = BindingMode.OneWay });
                point.SetBinding(Ellipse.HeightProperty, new Binding("P_height") { Source = spot, Mode = BindingMode.OneWay });
            }

            if (rect != null)
            {
                rect.SetBinding(Rectangle.MarginProperty, new Binding("P_margin") { Source = spot, Mode = BindingMode.OneWay });
                rect.SetBinding(Rectangle.WidthProperty, new Binding("P_width") { Source = spot, Mode = BindingMode.OneWay });
                rect.SetBinding(Rectangle.HeightProperty, new Binding("P_height") { Source = spot, Mode = BindingMode.OneWay });
            }
            line_h.SetBinding(Line.StrokeThicknessProperty, new Binding("L_thick") { Source = spot, Mode = BindingMode.OneWay });
            line_h.SetBinding(Line.Y1Property, new Binding("Y") { Source = spot, Mode = BindingMode.OneWay });
            line_h.SetBinding(Line.Y2Property, new Binding("Y") { Source = spot, Mode = BindingMode.OneWay });

            line_v.SetBinding(Line.StrokeThicknessProperty, new Binding("L_thick") { Source = spot, Mode = BindingMode.OneWay });
            line_v.SetBinding(Line.X1Property, new Binding("X") { Source = spot, Mode = BindingMode.OneWay });
            line_v.SetBinding(Line.X2Property, new Binding("X") { Source = spot, Mode = BindingMode.OneWay });

            text.SetBinding(TextBlock.TextProperty, new Binding("Info") { Source = spot, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.MarginProperty, new Binding("T_margin") { Source = spot, Mode = BindingMode.OneWay });

        }

        private void startTimer()
        {
            //main
            int timeInterval = getTimetInterval();
            Log.Info("开始定时任务，" + timeInterval);
            mainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mainTimer.Interval = timeInterval;
            mainTimer.Enabled = true;


            //clearTMP 每5分钟清理临时目录
            System.Timers.Timer clearTimer = new System.Timers.Timer();
            clearTimer.Elapsed += new ElapsedEventHandler(OnTimedEventClear);
            clearTimer.Interval = 1000 * 60 * 5;
            clearTimer.Enabled = true;

        }
        private delegate void TimerDispatcherDelegate();
        private void OnTimedEvent(object sender, EventArgs e)
        {
            TimeAction();
        }

        private void OnTimedEventClear(object sender, EventArgs e)
        {
            ClearTMP();
        }

        private void TimeAction(object sender, EventArgs e)
        {
            TimeAction();
        }

        private void TimeAction()
        {
            Log.Info("-------TIMER bigin");
            DateTime n1 = DateTime.Now;
            startCatchJpeg(cameraPair_v1, SpotPosition_v1_mark, SpotPosition_v1_real);
            startCatchJpeg(cameraPair_v2, SpotPosition_v2_mark, SpotPosition_v2_real);
            DateTime n2 = DateTime.Now;
            Log.Info("-------TIMER end ：" + DateUtils.DateDiff(n2, n1) + " ms");
        }

        private void startCatchJpeg(CameraPair cameraPair, SpotPosition SpotPosition_mark, SpotPosition SpotPosition_real)
        {

            if (cameraPair != null)
            {
                DeviceModule deviceModule = cameraPair.DeviceModule;
                if (deviceModule.Datum_x > 0 || deviceModule.Datum_y > 0)
                {
                    SpotPosition_mark.A_x = deviceModule.Datum_x;
                    SpotPosition_mark.A_y = SpotPosition.Max_Height - deviceModule.Datum_y;
                }
                else
                {
                    SpotPosition_mark.A_x = 0;
                    SpotPosition_mark.A_y = 0;
                }

                ImageDetail imageDetail = runSpotLocation(cameraPair.MainVideoHandle);
                SpotPosition_real.A_x = imageDetail.PX;
                SpotPosition_real.A_y = imageDetail.PY;
                cameraPair.ImageDetail = imageDetail;

            }
        }

        private static ImageDetail runSpotLocation(VideoHandle videoHandle)
        {
            ImageDetail imageDetail;
            switch (SPOT_LOCATIOIN_METHOD)
            {
                case "1":
                    imageDetail = fromMatlab(videoHandle);
                    break;
                default:
                    imageDetail = fromOpenCV(videoHandle);
                    break;
            }
            return imageDetail;
        }

        private static ImageDetail fromOpenCV(VideoHandle videoHandle)
        {
            DateTime t1 = DateTime.Now;
            ImageDetail imageDetail = new ImageDetail();
            byte[] bytes = videoHandle.btnJPEG_Byte();

            DateTime t2 = DateTime.Now;
            if (bytes.Length > 0)
            {
                imageDetail = OpenCVUtils.getHighPoint(bytes);
                //SpotPosition_real.A_x = imageDetailO.PX;
                //SpotPosition_real.A_y = imageDetailO.PY;
                //cameraPair.ImageDetail = imageDetailO;
            }
            else
            {
                //SpotPosition_real.A_x = 0;
                //SpotPosition_real.A_y = 0;
            }
            DateTime t3 = DateTime.Now;
            Log.InfoFormat("TIMER -opencv catchPic:{0}, getHighPoint:{1}", DateUtils.DateDiff(t2, t1), DateUtils.DateDiff(t3, t2));
            return imageDetail;
        }

        private static ImageDetail fromMatlab(VideoHandle videoHandle)
        {
            DateTime t1 = DateTime.Now;
            if (Directory.Exists(TMP) == false)
            {
                Directory.CreateDirectory(TMP);
            }
            //ClearTMP();

            string picPath = TMP + System.Guid.NewGuid().ToString() + ".jpg";
            videoHandle.btnJPEG_File(picPath);

            DateTime t2 = DateTime.Now;
            ImageDetail imageDetailM = MatlabCVUtils.getHighPoint(picPath);
            //SpotPosition_real.A_x = imageDetailM.PX;
            //SpotPosition_real.A_y = imageDetailM.PY;
            //cameraPair.ImageDetail = imageDetailM;

            DateTime t3 = DateTime.Now;
            Log.InfoFormat("TIMER -matlab catchPic:{0}, getHighPoint:{1}", DateUtils.DateDiff(t2, t1), DateUtils.DateDiff(t3, t2));

            return imageDetailM;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void ClearTMP()
        {
            DirectoryInfo folder = new DirectoryInfo(TMP);
            if (!folder.Exists)
            {
                return;
            }
            FileInfo[] fileInfos = folder.GetFiles();
            if (fileInfos.Length > 500)
            {
                Log.InfoFormat("正在清理文件夹，{0}，{1}", fileInfos.Length, TMP);
                int k = 0;
                for (int i = 0; i < fileInfos.Length; i++)
                {
                    FileInfo fileInfo = fileInfos[i];
                    DateTime creationTime = fileInfo.CreationTime;
                    double v = DateUtils.DateDiff(DateTime.Now, creationTime);
                    if (v > 1000 * 60 * 5)
                    {
                        try
                        {
                            fileInfo.Delete();
                            k++;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.Message);
                        }
                    }
                }
                Log.InfoFormat("清理完毕，已清理 {0} 个文件", k);
            }
        }


        //private void startCatchJpeg(VideoHandle videoHandle, string imagePath, Canvas canvas)
        //{
        //    if (videoHandle != null)
        //    {
        //        videoHandle.btnJPEG_Click(imagePath);

        //        ImageDetail imageDetail = OpenCVUtils.getHighPoint(imagePath);

        //        CanvasHandle CanvasUtils = new CanvasHandle(imageDetail);
        //        //网格
        //        CanvasUtils.drawGrid(canvas, imageDetail.Width, imageDetail.Height);
        //        //光斑中心点
        //        CanvasUtils.drawPoint_N(canvas, imageDetail.PX, imageDetail.PY);
        //        //基准点
        //        CanvasUtils.drawPoint_S(canvas, imageDetail.PX + 190, imageDetail.PY + 120);
        //    }
        //}

        public static BitmapImage GetImage(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            if (File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            return bitmap;
        }


        private void OpenAllCameraAndTDC()
        {
            Log.Info("OpenAllCameraAndTDC");

            VideoHandle.Init();

            List<DeviceModule> DeviceList = Setting_D.GetDeviceList();
            for (int i = 0; i < DeviceList.Count; i++)
            {
                DeviceModule deviceModule = DeviceList[i];
                VideoHandle topVideoHandle = OpenCamera(deviceModule);
                VideoHandle mainVideoHandle = OpenCamera(deviceModule);

                TDCHandle verticalTDC = TDCHandle.getTDCHandle(deviceModule.VerticalMotorSerialNo);
                TDCHandle horizontalTDC = TDCHandle.getTDCHandle(deviceModule.HorizontalMotorSerialNo);

                CameraPair cameraPair = new CameraPair(deviceModule.Id, deviceModule, deviceModule.Name, topVideoHandle, mainVideoHandle, verticalTDC, horizontalTDC);
                CAMERA_PAIR_LIST.Add(cameraPair);
            }

            CAMERA_PAIR_LIST[0].TopVideoHandle.btnPreview_Click(pictureBoxHost1);
            CAMERA_PAIR_LIST[1].TopVideoHandle.btnPreview_Click(pictureBoxHost2);
            CAMERA_PAIR_LIST[2].TopVideoHandle.btnPreview_Click(pictureBoxHost3);
            CAMERA_PAIR_LIST[3].TopVideoHandle.btnPreview_Click(pictureBoxHost4);
            CAMERA_PAIR_LIST[4].TopVideoHandle.btnPreview_Click(pictureBoxHost5);
            CAMERA_PAIR_LIST[5].TopVideoHandle.btnPreview_Click(pictureBoxHost6);
            CAMERA_PAIR_LIST[6].TopVideoHandle.btnPreview_Click(pictureBoxHost7);

            picLabel_t1.Pic_label = CAMERA_PAIR_LIST[0].DeviceModule.Name;
            picLabel_t2.Pic_label = CAMERA_PAIR_LIST[1].DeviceModule.Name;
            picLabel_t3.Pic_label = CAMERA_PAIR_LIST[2].DeviceModule.Name;
            picLabel_t4.Pic_label = CAMERA_PAIR_LIST[3].DeviceModule.Name;
            picLabel_t5.Pic_label = CAMERA_PAIR_LIST[4].DeviceModule.Name;
            picLabel_t6.Pic_label = CAMERA_PAIR_LIST[5].DeviceModule.Name;
            picLabel_t7.Pic_label = CAMERA_PAIR_LIST[6].DeviceModule.Name;

            velocityLabel.Pic_label = CAMERA_PAIR_LIST[0].DeviceModule.Velocity + "";

            startNumPictureBox(1);

        }




        private VideoHandle OpenCamera(DeviceModule device)
        {
            VideoHandle videoHandle = VideoHandle.GetVideoHandle(device.CameraIp);
            videoHandle.Login(device.CameraIp, device.CameraPort, device.CameraUserName, device.CameraPassword);
            return videoHandle;
        }

        //private TDCHandle OpenTDC(string serialNo)
        //{

        //    TDCHandle tDCHandle = TDCHandle.getTDCHandle(serialNo);


        //    return tDCHandle;
        //}


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            WaitingBox.Show(this, () =>
            {
                cleanDetail();
                string PictureBoxName = (sender as System.Windows.Forms.PictureBox).Name;
                string v = PictureBoxName.Split('_')[1];
                int PictureBoxNum = int.Parse(v);

                startNumPictureBox(PictureBoxNum);
            });

        }

        private void startNumPictureBox(int PictureBoxNum)
        {
            if (!picLabel_v1.Pic_label.Equals(CAMERA_PAIR_LIST[PictureBoxNum - 1].Name) || string.IsNullOrWhiteSpace(CAMERA_PAIR_LIST[PictureBoxNum - 1].Name))
            {

                cleanPictureBox(cameraPair_v1, pictureBoxHost_v1);
                cleanPictureBox(cameraPair_v2, pictureBoxHost_v2);

                cameraPair_v1 = CAMERA_PAIR_LIST[PictureBoxNum - 1];
                cameraPair_v2 = CAMERA_PAIR_LIST[PictureBoxNum];
                //tdcHandle_v = CAMERA_PAIR_LIST[PictureBoxNum - 1].VerticalTDC;
                //tdcHandle_h = CAMERA_PAIR_LIST[PictureBoxNum - 1].HorizontalTDC;

                cameraPair_v1.MainVideoHandle.btnPreview_Click(pictureBoxHost_v1);
                cameraPair_v2.MainVideoHandle.btnPreview_Click(pictureBoxHost_v2);

                picLabel_v1.Pic_label = CAMERA_PAIR_LIST[PictureBoxNum - 1].Name;
                picLabel_v2.Pic_label = CAMERA_PAIR_LIST[PictureBoxNum].Name;

                velocityLabel.Pic_label = CAMERA_PAIR_LIST[PictureBoxNum - 1].DeviceModule.Velocity + "";
            }
        }

        private void cleanPictureBox(CameraPair cameraPair, WindowsFormsHost pictureBoxHost)
        {
            if (cameraPair != null)
            {
                //DateTime t1 = DateTime.Now;
                cameraPair.MainVideoHandle.StopRealPlay();
                //DateTime t2 = DateTime.Now;
                cameraPair.MainVideoHandle.RefreshPicture();
                //DateTime t3 = DateTime.Now;
                //Log.InfoFormat(">>>>> StopRealPlay:{0}, RefreshPicture:{1}", DateUtils.DateDiff(t2, t1), DateUtils.DateDiff(t3, t2));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //cameraPair_v1.MainVideoHandle.btnJPEG_File(IMAGE_V1_PATH);
        }

        private Thorlabs.MotionControl.TCube.DCServoCLI.TCubeDCServo cubeDCServo;
        private Thread th;
        private Boolean thread_complate_flag = false;
        private DateTime log_x_time = DateTime.Now;

        private void Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string btn_name = (sender as Button).Name;

            TDCHandle tDCHandle = cameraPair_v1.VerticalTDC;
            switch (btn_name)
            {
                case "Up":
                case "Down":
                    tDCHandle = cameraPair_v1.VerticalTDC;
                    break;
                case "Left":
                case "Right":
                    tDCHandle = cameraPair_v1.HorizontalTDC;
                    break;
            }
            if (tDCHandle == null)
            {
                return;
            }
            string text = velocity_tb.Text;
            cameraPair_v1.DeviceModule.Velocity = Convert.ToDecimal(text);
            cameraPair_v1.DeviceModule.updateConfig();

            decimal v = 100;
            try
            {
                v = decimal.Parse(text);
                int direction_num = 1;
                switch (btn_name)
                {
                    case "Up":
                        direction_num = Settings.Default.Up_D;
                        break;
                    case "Down":
                        direction_num = Settings.Default.Down_D;
                        break;
                    case "Left":
                        direction_num = Settings.Default.Left_D;
                        break;
                    case "Right":
                        direction_num = Settings.Default.Right_D;
                        break;
                }
                MotorDirection direction = MotorDirection.Backward;
                if (direction_num > 0)
                {
                    direction = MotorDirection.Forward;
                }
                else
                {
                    direction = MotorDirection.Backward;
                }

                try
                {
                    cubeDCServo = tDCHandle.MoveAsync(direction, v);

                    log_x_time = DateTime.Now;
                    logAllPoint(getBeforeMoveLogName(log_x_time, "up", tDCHandle.getPosition()));

                    thread_complate_flag = false;
                    th = new Thread(new ThreadStart(showPosition));
                    th.Start();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    MessageBox.Show("无法继续移动！");
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }


        private void showPosition()
        {
            Thread.Sleep(200);
            while (!thread_complate_flag && cubeDCServo.Status.IsMoving)
            {
                if (cubeDCServo.Position != 0)
                {
                    tdc_detail.Position = cubeDCServo.Position;
                    tdc_detail.SerialNo = cubeDCServo.SerialNo;
                    Thread.Sleep(200);
                }
            }
        }


        private void Btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string name = (sender as Button).Name;
            thread_complate_flag = true;

            if (cubeDCServo != null)
            {
                cubeDCServo.StopImmediate();
                if (cubeDCServo.Position != 0)
                {
                    tdc_detail.Position = cubeDCServo.Position;
                }

                logAllPoint(getAfterMoveLogName(log_x_time, "up", cubeDCServo.Position));
            }

        }



        private void Up_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            TDCHandle tdc = cameraPair_v1.VerticalTDC;
            logAllPoint(getBeforeMoveLogName(t, "up", tdc.getPosition()));

            cameraPair_v1.DeviceModule.Velocity = Convert.ToDecimal(velocityLabel.Pic_label);
            Move(tdc, false);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "up", tdc.getPosition()));
        }



        private void Down_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            TDCHandle tdc = cameraPair_v1.VerticalTDC;
            logAllPoint(getBeforeMoveLogName(t, "dwon", tdc.getPosition()));

            cameraPair_v1.DeviceModule.Velocity = Convert.ToDecimal(velocityLabel.Pic_label);
            Move(tdc, true);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "dwon", tdc.getPosition()));
        }

        private void Left_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            TDCHandle tdc = cameraPair_v1.HorizontalTDC;
            logAllPoint(getBeforeMoveLogName(t, "left", tdc.getPosition()));

            cameraPair_v1.DeviceModule.Velocity = Convert.ToDecimal(velocityLabel.Pic_label);
            Move(tdc, true);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "left", tdc.getPosition()));
        }

        private void Right_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            TDCHandle tdc = cameraPair_v1.HorizontalTDC;
            logAllPoint(getBeforeMoveLogName(t, "right", tdc.getPosition()));

            cameraPair_v1.DeviceModule.Velocity = Convert.ToDecimal(velocityLabel.Pic_label);
            Move(tdc, false);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "right", tdc.getPosition()));
        }


        private string getBeforeMoveLogName(DateTime t, string direction, Decimal position)
        {
            return string.Format("----MOVE {0}, deviceId:{1}, {2}, {3}, 移动前: pos-{4}", t.ToString(), cameraPair_v1.Id, direction, velocity_tb.Text, position);
        }

        private string getAfterMoveLogName(DateTime t, string direction, Decimal position)
        {
            return string.Format("----MOVE {0}, deviceId:{1}, {2}, {3}, 移动后: pos-{4}", t.ToString(), cameraPair_v1.Id, direction, velocity_tb.Text, position);
        }




        private void Move(TDCHandle tDCHandle, Boolean plus)
        {
            if (tDCHandle == null)
            {
                return;
            }
            string text = velocity_tb.Text;
            decimal v = 100;
            try
            {
                v = decimal.Parse(text);
                if (!plus)
                {
                    v = -v;
                }
                tDCHandle.Move(v, TDC_MAX_VELOCITY);

                tdc_detail.SerialNo = tDCHandle.SerialNo;
                tdc_detail.Position = tDCHandle.getPosition();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

        }

        public int getTimetInterval()
        {
            string tbRefreshRate = UserSettings.Instance.Get(REFRESH_RATE_NAME, "" + REFRESH_RATE_DEFAULT);
            Regex re = new Regex("^[1-9]\\d*$");
            if (re.IsMatch(tbRefreshRate))
            {
                int refreshRate = Convert.ToInt32(tbRefreshRate);
                return 60 * 1000 / refreshRate;
            }
            else
            {
                MessageBox.Show("请输入大于0的数");
                tb_refresh_rate.Text = "" + REFRESH_RATE_DEFAULT;
                return REFRESH_RATE_DEFAULT;
            }

        }

        private void logAllPointThread(object o)
        {
            Thread.Sleep(getTimetInterval());
            logAllPoint((string)o);
        }

        private static void logAllPoint(string info)
        {
            for (int i = 0; i < CAMERA_PAIR_LIST.Count; i++)
            {
                ImageDetail imageDetail = CAMERA_PAIR_LIST[i].ImageDetail;
                if (imageDetail != null)
                {
                    Log.InfoFormat("{0},id:{1},{2}", info, CAMERA_PAIR_LIST[i].Id, imageDetail.ToJSON());
                }
            }
        }

        private void Save_Datum_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认重置基准点？", "重置基准点？", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Log.Info("重置基准点开始");

                WaitingBox.Show(this, () =>
                {
                    setDatum();
                });

                MessageBox.Show("重置成功");
                Log.Info("重置基准点成功");

            }

        }

        private static void setDatum()
        {
            int count = CAMERA_PAIR_LIST.Count;
            //List<DeviceModule> devices = Setting_D.GetDeviceList();
            for (int i = 0; i < count; i++)
            {
                CameraPair cameraPair = CAMERA_PAIR_LIST[i];

                ImageDetail imageDetail = runSpotLocation(cameraPair.MainVideoHandle);

                //byte[] bytes = cameraPair.MainVideoHandle.btnJPEG_Byte();
                //bool b = cameraPair.MainVideoHandle.btnJPEG_File(IMAGE_TEMP_PATH);
                DeviceModule deviceModule = cameraPair.DeviceModule;
                double Datum_x = Double.NaN;
                double Datum_y = Double.NaN;
                //if (File.Exists(IMAGE_TEMP_PATH) && b) 
                //if (bytes.Length > 0)
                //{
                //ImageDetail imageDetail = OpenCVUtils.getHighPoint(bytes);
                //ImageDetail imageDetail = OpenCVUtils.getHighPoint(IMAGE_TEMP_PATH);

                if (imageDetail.PX > 0 || imageDetail.PY > 0)
                {
                    Datum_x = imageDetail.PX;
                    Datum_y = SpotPosition.Max_Height - imageDetail.PY;
                }
                //}
                deviceModule.Datum_x = Datum_x;
                deviceModule.Datum_y = Datum_y;
                deviceModule.updateConfig();

                Log.InfoFormat("datum id:{0}, x:{1}, y:{2}", deviceModule.Id, deviceModule.Datum_x, deviceModule.Datum_y);

            }
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.]+");

            e.Handled = re.IsMatch(e.Text);

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Log.Info("配置刷新.....");
            WaitingBox.Show(this, () =>
            {
                CloseAllCameraAndTDC();
                OpenAllCameraAndTDC();
            });

        }



        private void CloseAllCameraAndTDC()
        {
            Log.Info("CloseAllCameraAndTDC");

            for (int i = 0; i < CAMERA_PAIR_LIST.Count; i++)
            {
                try
                {
                    CameraPair cameraPair = CAMERA_PAIR_LIST[i];
                    cameraPair.TopVideoHandle.Stop();
                    cameraPair.MainVideoHandle.Stop();
                    cameraPair.VerticalTDC.Dispose();
                    cameraPair.HorizontalTDC.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            cleanDetail();
            CAMERA_PAIR_LIST = new List<CameraPair>();

        }


        public void cleanDetail()
        {
            picLabel_v1.Pic_label = "";
            picLabel_v2.Pic_label = "";

            SpotPosition_v1_mark.A_x = 0;
            SpotPosition_v1_mark.A_y = 0;

            SpotPosition_v1_real.A_x = 0;
            SpotPosition_v1_real.A_y = 0;

            SpotPosition_v2_mark.A_x = 0;
            SpotPosition_v2_mark.A_y = 0;

            SpotPosition_v2_real.A_x = 0;
            SpotPosition_v2_real.A_y = 0;

        }


        private void Open_Auto(object sender, RoutedEventArgs e)
        {

        }

        private void Close_Auto(object sender, RoutedEventArgs e)
        {

        }

        private void tb_refresh_rate_TextChanged(object sender, TextChangedEventArgs e)
        {
            UserSettings.Instance.Set(REFRESH_RATE_NAME, tb_refresh_rate.Text);
            UserSettings.Instance.Save();
            int timeInterval = getTimetInterval();
            Log.Info("更新定时任务间隔，" + timeInterval);
            mainTimer.Interval = timeInterval;
            //MessageBox.Show("刷新频率修改成功");
        }

        private void tb_rate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("^[0-9]\\d*$");
            //Boolean b = Convert.ToInt32(e.Text) <= 0;
            e.Handled = !re.IsMatch(e.Text);
        }



        private Boolean is_auto = false;

        private void Change_Auto_Click(object sender, RoutedEventArgs e)
        {
            Button btn_auto = sender as Button;
            if (is_auto)
            {
                if (MessageBox.Show("确认切换手动模式？", "切换手动模式？", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    is_auto = false;
                    btn_auto.Background = new SolidColorBrush(Color.FromRgb(171, 171, 171));
                    btn_auto.Content = "手动";
                    btn_auto.Tag = "" + is_auto;

                }
                else
                {
                    return;
                }


            }
            else
            {
                if (MessageBox.Show("确认切换自动模式？", "切换自动模式？", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    is_auto = true;
                    btn_auto.Background = new SolidColorBrush(Color.FromRgb(141, 210, 138));
                    btn_auto.Content = "自动";
                    btn_auto.Tag = "" + is_auto;

                }
                else
                {
                    return;
                }
            }




        }




    }




}
