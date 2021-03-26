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

namespace LightConductor.Pages
{
    /// <summary>
    /// Monitor.xaml 的交互逻辑
    /// </summary>
    public partial class Monitor : Page
    {

        DispatcherTimer dispatcherTimer = null;

        private static List<CameraPair> CAMERA_PAIR_LIST = new List<CameraPair>();

        //VideoHandle videoHandle_v1;
        //VideoHandle videoHandle_v2;
        CameraPair cameraPair_v1;
        CameraPair cameraPair_v2;
        TDCHandle tdcHandle_v;
        TDCHandle tdcHandle_h;
        public static string IMAGE_V1_PATH = "D:\\LC\\image_v1.jpg";
        public static string IMAGE_V2_PATH = "D:\\LC\\image_v2.jpg";
        public static string IMAGE_TEMP_PATH = "D:\\LC\\image_v2.jpg";
        public static int TIME_MILLISECONDS = 1000;

        //光斑中心坐标
        private SpotPosition SpotPosition_v1_s;
        private SpotPosition SpotPosition_v1_n;
        private SpotPosition SpotPosition_v2_s;
        private SpotPosition SpotPosition_v2_n;

        ButtonColors buttonColors;
        Color colorOn = Color.FromRgb(30, 30, 30);
        Color colorOff = Color.FromRgb(207, 207, 207);


        public Monitor()
        {
            InitializeComponent();


            OpenAllCameraAndTDC();

            InitPoint();

            startTimer();

        }

        private void InitPoint()
        {
            LogUtils.Log.Info("InitPoint");
            SpotPosition_v1_s = new SpotPosition(0, 0);
            SpotPosition_v1_n = new SpotPosition(0, 0);
            setPoint(SpotPosition_v1_s, null, rect_v1_s, line_h_v1_s, line_v_v1_s, text_v1_s);
            setPoint(SpotPosition_v1_n, point_v1_n, null, line_h_v1_n, line_v_v1_n, text_v1_n);

            SpotPosition_v2_s = new SpotPosition(0, 0);
            SpotPosition_v2_n = new SpotPosition(0, 0);
            setPoint(SpotPosition_v2_s, null, rect_v2_s, line_h_v2_s, line_v_v2_s, text_v2_s);
            setPoint(SpotPosition_v2_n, point_v2_n, null, line_h_v2_n, line_v_v2_n, text_v2_n);
        }

        private void setPoint(SpotPosition spot, Ellipse point, Rectangle rect, Line line_h, Line line_v, TextBlock text)
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
            LogUtils.Log.Info("开始定时任务，" + TIME_MILLISECONDS);
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
            DateTime n1 = DateTime.Now;
            startCatchJpeg(cameraPair_v1, IMAGE_V1_PATH, SpotPosition_v1_s, SpotPosition_v1_n);
            startCatchJpeg(cameraPair_v2, IMAGE_V2_PATH, SpotPosition_v2_s, SpotPosition_v2_n);
            DateTime n2 = DateTime.Now;
            Console.WriteLine(DateUtils.DateDiff(n2, n1));
        }

        private void startCatchJpeg(CameraPair cameraPair, string imagePath, SpotPosition SpotPosition_s, SpotPosition SpotPosition_n)
        {

            if (cameraPair != null)
            {
                DeviceModule deviceModule = cameraPair.DeviceModule;
                if (deviceModule.Datum_x > 0 || deviceModule.Datum_y > 0)
                {
                    SpotPosition_s.A_x = deviceModule.Datum_x;
                    SpotPosition_s.A_y = SpotPosition.Max_Height - deviceModule.Datum_y;
                }
                else {
                    SpotPosition_s.A_x = 0;
                    SpotPosition_s.A_y = 0;
                }

                byte[] bytes = cameraPair.MainVideoHandle.btnJPEG_Byte();
                if (bytes.Length > 0)
                {

                    ImageDetail imageDetail = OpenCVUtils.getHighPoint(bytes);
                    //ImageDetail imageDetail = OpenCVUtils.getHighPoint(imagePath);
                    SpotPosition_n.A_x = imageDetail.PX;
                    SpotPosition_n.A_y = imageDetail.PY;
                }
                else {
                    SpotPosition_n.A_x = 0;
                    SpotPosition_n.A_y = 0;
                }

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
            LogUtils.Log.Info("OpenAllCameraAndTDC");

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
            cleanDetail();
            string PictureBoxName = (sender as System.Windows.Forms.PictureBox).Name;
            string v = PictureBoxName.Split('_')[1];
            int PictureBoxNum = int.Parse(v);

            startNumPictureBox(PictureBoxNum);

        }

        private void startNumPictureBox(int PictureBoxNum)
        {
            if (!label_v1.Text.Equals(CAMERA_PAIR_LIST[PictureBoxNum - 1].Name))
            {

                cleanPictureBox(cameraPair_v1, pictureBoxHost_v1);
                cleanPictureBox(cameraPair_v2, pictureBoxHost_v2);

                cameraPair_v1 = CAMERA_PAIR_LIST[PictureBoxNum - 1];
                cameraPair_v2 = CAMERA_PAIR_LIST[PictureBoxNum];
                tdcHandle_v = CAMERA_PAIR_LIST[PictureBoxNum - 1].VerticalTDC;
                tdcHandle_h = CAMERA_PAIR_LIST[PictureBoxNum - 1].HorizontalTDC;

                cameraPair_v1.MainVideoHandle.btnPreview_Click(pictureBoxHost_v1);
                cameraPair_v2.MainVideoHandle.btnPreview_Click(pictureBoxHost_v2);

                label_v1.Text = CAMERA_PAIR_LIST[PictureBoxNum - 1].Name;
                label_v2.Text = CAMERA_PAIR_LIST[PictureBoxNum].Name;
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
            Move(tdcHandle_v, false);
        }

        private void Down_click(object sender, RoutedEventArgs e)
        {
            Move(tdcHandle_v, true);
        }

        private void Left_click(object sender, RoutedEventArgs e)
        {
            Move(tdcHandle_h, false);
        }

        private void Right_click(object sender, RoutedEventArgs e)
        {
            Move(tdcHandle_h, true);
        }

        private void Move(TDCHandle tDCHandle, Boolean plus)
        {
            if (tdcHandle_h == null)
            {
                return;
            }
            string text = velocity_tb.Text;
            decimal v = 100;
            try
            {
                v = decimal.Parse(text);
            }
            catch (Exception) { 
            
            }
            if (plus)
            {
                tdcHandle_v.Move(v, 1000);
            }
            else
            {
                tdcHandle_v.Move(-v, 1000);
            }
        }

        private void Save_Datum_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认重置基准点？", "重置基准点？", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                LogUtils.Log.Info("重置基准点开始");
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

                    LogUtils.Log.Info(deviceModule.Id + "," + deviceModule.Datum_x + "," + deviceModule.Datum_y);

                }
                MessageBox.Show("重置成功");
                LogUtils.Log.Info("重置基准点成功");

            }
            else {
                WaitingBox.Show(this, () =>
                {
                    System.Threading.Thread.Sleep(3000);
                }, "正在玩命的加载，请稍后...");
                

            }
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");

            e.Handled = re.IsMatch(e.Text);

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            LogUtils.Log.Info("配置刷新.....");
            CloseAllCameraAndTDC();

            OpenAllCameraAndTDC();

            //InitPoint();

        }



        private void CloseAllCameraAndTDC()
        {
            LogUtils.Log.Info("CloseAllCameraAndTDC");

            for (int i = 0; i < CAMERA_PAIR_LIST.Count; i++)
            {
                CameraPair cameraPair = CAMERA_PAIR_LIST[i];
                cameraPair.TopVideoHandle.Dispose();
                cameraPair.MainVideoHandle.Dispose();
                cameraPair.VerticalTDC.Dispose();
                cameraPair.HorizontalTDC.Dispose();

            }
            cleanDetail();
            CAMERA_PAIR_LIST = new List<CameraPair>();

        }


        public void cleanDetail()
        {
            label_v1.Text = "";
            label_v2.Text = "";

            SpotPosition_v1_s.A_x = 0;
            SpotPosition_v1_s.A_y = 0;

            SpotPosition_v1_n.A_x = 0;
            SpotPosition_v1_n.A_y = 0;

            SpotPosition_v2_s.A_x = 0;
            SpotPosition_v2_s.A_y = 0;

            SpotPosition_v2_n.A_x = 0;
            SpotPosition_v2_n.A_y = 0;

        }


    }




}
