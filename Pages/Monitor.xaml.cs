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
        public static string IMAGE_V1_PATH = "D:\\LC\\image_v1.jpg";
        public static string IMAGE_V2_PATH = "D:\\LC\\image_v2.jpg";
        public static string IMAGE_TEMP_PATH = "D:\\LC\\image_v2.jpg";
        public static int TIME_MILLISECONDS = Convert.ToInt32(ConfigurationManager.AppSettings["catch_position_interval"]);
        public static int TDC_MAX_VELOCITY = Convert.ToInt32(ConfigurationManager.AppSettings["tdc_max_velocity"]);

        //光斑中心坐标
        private SpotPosition SpotPosition_v1_mark;
        private SpotPosition SpotPosition_v1_real;
        private SpotPosition SpotPosition_v2_mark;
        private SpotPosition SpotPosition_v2_real;

        private PicLabel picLabel_v1;
        private PicLabel picLabel_v2;

        private PicLabel picLabel_t1;
        private PicLabel picLabel_t2;
        private PicLabel picLabel_t3;
        private PicLabel picLabel_t4;
        private PicLabel picLabel_t5;
        private PicLabel picLabel_t6;
        private PicLabel picLabel_t7;


        public Monitor()
        {
            InitializeComponent();

            ThreadPool.SetMaxThreads(20, 10);

            InitBinding();

            OpenAllCameraAndTDC();

            startTimer();

        }

        private void InitBinding()
        {
            Log.Info("InitPoint");
            SpotPosition_v1_mark = new SpotPosition(0, 0);
            SpotPosition_v1_real = new SpotPosition(0, 0);
            pointBinding(SpotPosition_v1_mark, null, rect_v1_s, line_h_v1_s, line_v_v1_s, text_v1_s);
            pointBinding(SpotPosition_v1_real, point_v1_n, null, line_h_v1_n, line_v_v1_n, text_v1_n);

            SpotPosition_v2_mark = new SpotPosition(0, 0);
            SpotPosition_v2_real = new SpotPosition(0, 0);
            pointBinding(SpotPosition_v2_mark, null, rect_v2_s, line_h_v2_s, line_v_v2_s, text_v2_s);
            pointBinding(SpotPosition_v2_real, point_v2_n, null, line_h_v2_n, line_v_v2_n, text_v2_n);

            picLabel_v1 = new PicLabel("");
            picLabel_v2 = new PicLabel("");
            labelBinding(picLabel_v1, label_v1);
            labelBinding(picLabel_v2, label_v2);


            picLabel_t1 = new PicLabel("");
            picLabel_t2 = new PicLabel("");
            picLabel_t3 = new PicLabel("");
            picLabel_t4 = new PicLabel("");
            picLabel_t5 = new PicLabel("");
            picLabel_t6 = new PicLabel("");
            picLabel_t7 = new PicLabel("");
            labelBinding(picLabel_t1, top_label_1);
            labelBinding(picLabel_t2, top_label_2);
            labelBinding(picLabel_t3, top_label_3);
            labelBinding(picLabel_t4, top_label_4);
            labelBinding(picLabel_t5, top_label_5);
            labelBinding(picLabel_t6, top_label_6);
            labelBinding(picLabel_t7, top_label_7);


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
            Log.Info("开始定时任务，" + TIME_MILLISECONDS);
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = TIME_MILLISECONDS;
            aTimer.Enabled = true;

        }
        private delegate void TimerDispatcherDelegate();
        private void OnTimedEvent(object sender, EventArgs e)
        {
            TimeAction();
        }


        private void TimeAction(object sender, EventArgs e)
        {
            TimeAction();
        }

        private void TimeAction()
        {
            Log.Info("-------TIMER bigin");
            DateTime n1 = DateTime.Now;
            startCatchJpeg(cameraPair_v1, IMAGE_V1_PATH, SpotPosition_v1_mark, SpotPosition_v1_real);
            startCatchJpeg(cameraPair_v2, IMAGE_V2_PATH, SpotPosition_v2_mark, SpotPosition_v2_real);
            DateTime n2 = DateTime.Now;
            Log.Info("-------TIMER end ：" + DateUtils.DateDiff(n2, n1) + " ms");
        }

        private void startCatchJpeg(CameraPair cameraPair, string imagePath, SpotPosition SpotPosition_mark, SpotPosition SpotPosition_real)
        {

            if (cameraPair != null)
            {
                DateTime t1 = DateTime.Now;
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
                DateTime t2 = DateTime.Now;
                byte[] bytes = cameraPair.MainVideoHandle.btnJPEG_Byte();

                DateTime t3 = DateTime.Now;
                if (bytes.Length > 0)
                {

                    ImageDetail imageDetail = OpenCVUtils.getHighPoint(bytes);
                    //ImageDetail imageDetail = OpenCVUtils.getHighPoint(imagePath);
                    SpotPosition_real.A_x = imageDetail.PX;
                    SpotPosition_real.A_y = imageDetail.PY;

                    cameraPair.ImageDetail = imageDetail;
                }
                else
                {
                    SpotPosition_real.A_x = 0;
                    SpotPosition_real.A_y = 0;
                }
                DateTime t4 = DateTime.Now;
                Log.InfoFormat("TIMER catchPic:{0}, getHighPoint:{1}", DateUtils.DateDiff(t3, t2), DateUtils.DateDiff(t4, t3));
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
            }
        }

        private void cleanPictureBox(CameraPair cameraPair, WindowsFormsHost pictureBoxHost)
        {
            if (cameraPair != null)
            {
                cameraPair.MainVideoHandle.StopRealPlay();
                cameraPair.MainVideoHandle.RefreshPicture();

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //cameraPair_v1.MainVideoHandle.btnJPEG_File(IMAGE_V1_PATH);
        }



        private void Up_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            logAllPoint(getBeforeMoveLogName(t, "up"));

            Move(cameraPair_v1.VerticalTDC, false);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "up"));
        }



        private void Down_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            logAllPoint(getBeforeMoveLogName(t, "dwon"));

            Move(cameraPair_v1.VerticalTDC, true);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "dwon"));
        }

        private void Left_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            logAllPoint(getBeforeMoveLogName(t, "left"));

            Move(cameraPair_v1.HorizontalTDC, false);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "left"));
        }

        private void Right_click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Now;
            logAllPoint(getBeforeMoveLogName(t, "right"));

            Move(cameraPair_v1.HorizontalTDC, true);

            ThreadPool.QueueUserWorkItem(logAllPointThread, getAfterMoveLogName(t, "right"));
        }


        private string getBeforeMoveLogName(DateTime t, string direction)
        {
            return string.Format("----MOVE {0}, deviceId:{1}, {2}, 移动前", t.ToString(), cameraPair_v1.Id, direction, velocity_tb.Text);
        }

        private string getAfterMoveLogName(DateTime t, string direction)
        {
            return string.Format("----MOVE {0}, deviceId:{1}, {2}, 移动后", t.ToString(), cameraPair_v1.Id, direction, velocity_tb.Text);
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
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

        }

        private static void logAllPointThread(object o)
        {
            Thread.Sleep(TIME_MILLISECONDS);
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
                byte[] bytes = cameraPair.MainVideoHandle.btnJPEG_Byte();
                //bool b = cameraPair.MainVideoHandle.btnJPEG_File(IMAGE_TEMP_PATH);
                DeviceModule deviceModule = cameraPair.DeviceModule;
                double Datum_x = Double.NaN;
                double Datum_y = Double.NaN;
                //if (File.Exists(IMAGE_TEMP_PATH) && b) 
                if (bytes.Length > 0)
                {
                    ImageDetail imageDetail = OpenCVUtils.getHighPoint(bytes);
                    //ImageDetail imageDetail = OpenCVUtils.getHighPoint(IMAGE_TEMP_PATH);

                    if (imageDetail.PX > 0 || imageDetail.PY > 0)
                    {
                        Datum_x = imageDetail.PX;
                        Datum_y = SpotPosition.Max_Height - imageDetail.PY;
                    }
                }
                deviceModule.Datum_x = Datum_x;
                deviceModule.Datum_y = Datum_y;
                deviceModule.updateConfig();

                Log.InfoFormat("datum id:{0}, x:{1}, y:{2}", deviceModule.Id, deviceModule.Datum_x, deviceModule.Datum_y);

            }
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");

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
    }




}
