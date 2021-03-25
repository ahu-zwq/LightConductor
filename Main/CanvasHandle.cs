using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LightConductor.Main
{
    class CanvasHandle
    {

       

        public void drawPoint_S(Canvas canvas, double a_x, double a_y)
        {

            if (a_x <= 0 || a_y <= 0)
            {
                return;
            }
            int x = (int)(a_x * ((W * 1.0) / (e_width * 1.0)));
            int y = (int)(a_y * ((H * 1.0) / (e_height * 1.0)));

            Color color = Color.FromRgb(226, 242, 196);

            var myPoint = new Ellipse();
            myPoint.Height = POINT_W * 2;
            myPoint.Width = POINT_W * 2;
            myPoint.Fill = new SolidColorBrush(color);

            myPoint.Margin = new Thickness(x - POINT_W + X_CUT, y - POINT_W, 0, 0);
            myPoint.Stroke = new SolidColorBrush(color);
            canvas.Children.Add(myPoint);

            //横线
            Line h_line = new Line();
            h_line.Stroke = new SolidColorBrush(color);
            h_line.StrokeThickness = 1;//线宽度
            h_line.X1 = X_CUT;
            h_line.Y1 = y;
            h_line.X2 = W + X_CUT;
            h_line.Y2 = y;
            h_line.StrokeDashArray = new DoubleCollection() { 2, 3 };
            canvas.Children.Add(h_line);

            //竖线
            Line v_line = new Line();
            v_line.Stroke = new SolidColorBrush(color);
            v_line.StrokeThickness = 1;//线宽度
            v_line.X1 = x + X_CUT;
            v_line.Y1 = 0;
            v_line.X2 = x + X_CUT;
            v_line.Y2 = H;
            v_line.StrokeDashArray = new DoubleCollection() { 2, 3 };
            canvas.Children.Add(v_line);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "[" + a_x + ", " + (H - a_y) + "]";
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.FontSize = 15;
            Canvas.SetLeft(textBlock, x + X_CUT);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);

        }

        public void drawPoint_N(Canvas canvas, double a_x, double a_y)
        {

            if (a_x <= 0 || a_y <= 0)
            {
                return;
            }
            int x = (int)(a_x * ((W * 1.0) / (e_width * 1.0)));
            int y = (int)(a_y * ((H * 1.0) / (e_height * 1.0)));

            Color color = Colors.Red;

            var myPoint = new Ellipse();
            myPoint.Height = POINT_W * 2;
            myPoint.Width = POINT_W * 2;
            myPoint.Fill = new SolidColorBrush(color);

            myPoint.Margin = new Thickness(x - POINT_W + X_CUT, y - POINT_W, 0, 0);
            myPoint.Stroke = new SolidColorBrush(color);
            canvas.Children.Add(myPoint);

            //横线
            Line h_line = new Line();
            h_line.Stroke = new SolidColorBrush(color);
            h_line.StrokeThickness = 1;//线宽度
            h_line.X1 = X_CUT;
            h_line.Y1 = y;
            h_line.X2 = W + X_CUT;
            h_line.Y2 = y;
            h_line.StrokeDashArray = new DoubleCollection() { 2, 3 };
            canvas.Children.Add(h_line);

            //竖线
            Line v_line = new Line();
            v_line.Stroke = new SolidColorBrush(color);
            v_line.StrokeThickness = 1;//线宽度
            v_line.X1 = x + X_CUT;
            v_line.Y1 = 0;
            v_line.X2 = x + X_CUT;
            v_line.Y2 = H;
            v_line.StrokeDashArray = new DoubleCollection() { 2, 3 };
            canvas.Children.Add(v_line);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "[" + a_x + ", " + (H - a_y) + "]";
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.FontSize = 15;
            Canvas.SetLeft(textBlock, x + X_CUT);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);

        }

        static double VP = 1920.0 / 1080.0;
        static int MAX_W = 480;
        static int MAX_H = 270;
        static int SPACE = 50;
        static int POINT_W = 3;
        static int X_CUT = 50;


        public CanvasHandle(int e_width, int e_height, int e_x, int e_y)
        {
            this.e_width = e_width;
            this.e_height = e_height;
            this.e_x = e_x;
            this.e_y = e_y;
        }

        public CanvasHandle(ImageDetail detail)
        {
            this.e_width = detail.Width;
            this.e_height = detail.Height;
            this.e_x = detail.PX;
            this.e_y = detail.PY;
        }




        private int e_width;
        private int e_height;
        private double e_x;
        private double e_y;
        int H;
        int W;
        int X;
        int Y;
        double n_vp;

        public void drawGrid(Canvas canvas, int width, int height)
        {
            canvas.Children.Clear();
            if (width <= 0 || height <= 0)
            {
                return;
            }
            H = MAX_H;
            W = MAX_W;
            n_vp = (width * 1.0) / (height * 1.0);
            if (n_vp < VP)
            {
                W = (int)(H * n_vp);
            }
            else
            {
                H = (int)(W / n_vp);
            }


            canvas.Children.Clear();


            //矩形边框
            var myPolygon = new Polygon();
            var lefttop = new Point(X_CUT, 0);
            var righttop = new Point(X_CUT + MAX_W, 0);
            var rightbottom = new Point(X_CUT + MAX_W, MAX_H);
            var leftbottom = new Point(X_CUT, MAX_H);
            var points = new Point[] { lefttop, righttop, rightbottom, leftbottom };
            myPolygon.Points = new PointCollection(points);
            myPolygon.Stroke = new SolidColorBrush(Colors.Black);
            myPolygon.StrokeThickness = 1;
            myPolygon.Fill = new SolidColorBrush(Color.FromRgb(117, 117, 117));
            canvas.Children.Add(myPolygon);

            //横线
            for (double i = H; i > 0; i -= SPACE)
            {
                Line mydrawline = new Line();
                mydrawline.Stroke = Brushes.Black;
                mydrawline.StrokeThickness = 1;//线宽度
                mydrawline.X1 = X_CUT;
                mydrawline.Y1 = i;
                mydrawline.X2 = W + X_CUT;
                mydrawline.Y2 = i;
                canvas.Children.Add(mydrawline);

                TextBlock textBlock = new TextBlock();
                string v = (H - i) * 4 + "";
                textBlock.Text =  v;
                textBlock.Foreground = new SolidColorBrush { Color = Colors.Black };
                Canvas.SetLeft(textBlock, (6 - v.Length) * 7);
                Canvas.SetTop(textBlock, i - 15);
                canvas.Children.Add(textBlock);

            }

            //竖线
            for (double i = X_CUT; i < W + X_CUT; i += SPACE)
            {
                Line mydrawline = new Line();
                mydrawline.Stroke = Brushes.Black;
                mydrawline.StrokeThickness = 1;
                mydrawline.X1 = i;
                mydrawline.Y1 = 0;
                mydrawline.X2 = i;
                mydrawline.Y2 = H;
                canvas.Children.Add(mydrawline);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = (i - X_CUT) * 4 + "";
                textBlock.Foreground = new SolidColorBrush { Color = Colors.Black };
                Canvas.SetLeft(textBlock, i);
                Canvas.SetTop(textBlock, H);
                canvas.Children.Add(textBlock);

            }


            
        }
    }
}
