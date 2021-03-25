using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OpenCvSharp;

namespace LightConductor.Main
{
    class OpenCVUtils
    {

        public static Mat bytesToMat(byte[] bytes)
        {
            return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }

        public static ImageDetail getHighPoint(byte[] bytes)
        {
            ImageDetail detail = new ImageDetail();
            if (bytes.Length == 0)
            {
                return detail;
            }
            try
            {
                return getHighPoint(bytesToMat(bytes));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return detail;
            }
        }

        public static ImageDetail getHighPoint(string path)
        {
            ImageDetail detail = new ImageDetail();
            try
            {
                return getHighPoint(Cv2.ImRead(path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return detail;
            }
        }


        public static ImageDetail getHighPoint(Mat srcImg)
        {
            ImageDetail detail = new ImageDetail();
            try
            {
                LogUtils.Log.Info("getHighPoint, image size:" + srcImg.Size());
                Mat dstImg = srcImg.Clone();
                Mat tempImg = srcImg.Clone();
                Cv2.CvtColor(tempImg, tempImg, ColorConversionCodes.BGR2GRAY);//色域转换
                Cv2.GaussianBlur(tempImg, tempImg, new Size(3, 3), 0, 0);//高斯滤波
                Cv2.Threshold(tempImg, tempImg, 230, 255, ThresholdTypes.Binary);//二值化

                Point[][] contours = new Point[][] { };

                HierarchyIndex[] hierarcy = new HierarchyIndex[] { };

                Cv2.FindContours(tempImg, out contours, out hierarcy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);//取轮廓
                RotatedRect[] box = new RotatedRect[contours.Length];

                int max_index = 0;
                float max_area = 0;
                for (int i = 0; i < contours.Length; i++)//求拟合椭圆
                {
                    if (contours[i].Length >= 50)
                    {
                        box[i] = Cv2.FitEllipse(contours[i]);
                        Size2f size = box[i].Size;
                        if (size.Height > 15 && size.Width > 15 && size.Height * size.Width > max_area)
                        {
                            max_area = size.Height * size.Width;
                            max_index = i;
                        }
                    }
                }


                detail.Width = dstImg.Cols;
                detail.Height = dstImg.Rows;
                if (max_area != 0)//绘制中心十字
                {
                    Point2f center = box[max_index].Center;

                    detail.PX = Convert.ToInt16(center.X);
                    detail.PY = Convert.ToInt16(center.Y);
                    //Cv2.Line(dstImg, new Point(center.X, center.Y - 6), new Point(center.X, center.Y + 6), new Scalar(0, 0, 255), 2);
                    //Cv2.Line(dstImg, new Point(center.X - 6, center.Y), new Point(center.X + 6, center.Y), new Scalar(0, 0, 255), 2);
                    //Cv2.Ellipse(dstImg, box[max_index], new Scalar(0, 255, 0), 1);

                    //Console.WriteLine("中心点\t" + center.X + "\t" + center.Y);
                    //Console.WriteLine("图片大小\t" + dstImg.Rows + "\t" + dstImg.Cols);

                }
                LogUtils.Log.InfoFormat("getHighPoint, height:{0},weight:{1}, result:{2},{3}", dstImg.Cols, dstImg.Rows, detail.PX, detail.PY);
                return detail;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return detail;
            }
        }

    }
}
