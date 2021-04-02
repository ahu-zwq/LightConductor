using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;

namespace LightConductor.Main
{
    public class MatlabCVUtils
    {

        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static ImageDetail getHighPoint(string imgPath)
        {
            ImageDetail detail = new ImageDetail();
            try
            {
                if (!File.Exists(imgPath))
                {
                    return detail;
                }
                MWArray mw = imgPath;
                imageproNative.Class1 class11 = new imageproNative.Class1();
                object[] vs = class11.imagepro(2, mw);

                double x = ((Double[,])vs[0])[0, 0];
                double y = ((Double[,])vs[1])[0, 0];
                detail.PX = Convert.ToDouble(x.ToString("0.00"));
                detail.PY = Convert.ToDouble(y.ToString("0.00"));

                Image image = Image.FromFile(imgPath);
                detail.Width = image.Width;
                detail.Height = image.Height;
                Log.InfoFormat("*** MATLAB getHighPoint, height:{0}, weight:{1}, PX:{2}, PY:{3}, cv result: x={4}, y={5}", detail.Height, detail.Width, detail.PX, detail.PY, x, y);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return detail;
        }

    }
}
