using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    class ImageDetail
    {
        private int height;
        private int width;
        private double pX;
        private double pY;

        public ImageDetail()
        {
        }

        public ImageDetail(int height, int width, double pX, double pY)
        {
            this.height = height;
            this.width = width;
            this.pX = pX;
            this.pY = pY;
        }

        public int Height { get => height; set => height = value; }
        public int Width { get => width; set => width = value; }
        public double PX { get => pX; set => pX = value; }
        public double PY { get => pY; set => pY = value; }

        
    }
}
