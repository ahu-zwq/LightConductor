using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LightConductor.Main
{
    class SpotPosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double a_x;
        private double a_y;
        private double x;
        private double y;
        private double p_x;
        private double p_y;
        private string info;
        private Thickness p_margin;
        private Thickness t_margin;
        private double p_width;
        private double p_height;
        private double l_thick;

        static double P_Wight = 4.0;
        static double L_Wight = 1.0;
        public static int Max_Height = 1080;

        public SpotPosition(double a_x, double a_y)
        {
            this.a_x = a_x;
            this.a_y = a_y;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public double X
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    x = a_x / 4;
                }
                else {
                    x = Double.NaN;
                }
                return x;
            }
            set
            {
                if (x != value)
                {
                    x = value;
                    OnPropertyChanged("X");
                }
            }
        }


        public double Y
        {
            get
            {
                if (a_y > 0 || a_x > 0)
                {
                    y = a_y / 4;
                }
                else
                {
                    y = Double.NaN;
                }
                return y;
            }
            set
            {
                if (y != value)
                {
                    y = value;
                    OnPropertyChanged("Y");
                }
            }
        }


        public double P_x
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    p_x = a_x / 4 - P_Wight;
                }
                else
                {
                    p_x = Double.NaN;
                }
                return p_x;
            }
            set
            {
                if (p_x != value)
                {
                    p_x = value;
                    OnPropertyChanged("P_x");
                }
            }
        }


        public double P_y
        {
            get
            {
                if (a_y > 0 || a_x > 0)
                {
                    p_y = a_y / 4 - P_Wight;
                }
                else
                {
                    p_y = Double.NaN;
                }
                return p_y;
            }
            set
            {
                if (p_y != value)
                {
                    p_y = value;
                    OnPropertyChanged("P_y");
                }
            }
        }


        public string Info
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    info = "[" + a_x + ", " + (Max_Height - a_y) + "]";
                }
                else
                {
                    info = "";
                }
                return info;
            }
            set
            {
                if (info != value)
                {
                    info = value;
                    OnPropertyChanged("Info");
                }
            }
        }
        
        public Thickness P_margin
        {
            get
            {
                if (a_x > 0)
                {
                    p_margin = new Thickness((a_x / 4 - P_Wight), (a_y / 4 - P_Wight), 0, 0);
                }
                else
                {
                    p_margin = new Thickness();
                }
                return p_margin;
            }
            set
            {
                if (p_margin != value)
                {
                    p_margin = value;
                    OnPropertyChanged("P_margin");
                }
            }
        }

        public double P_width
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    p_width = P_Wight * 2;
                }
                else
                {
                    p_width = 0;
                }
                return p_width;
            }
            set
            {
                if (p_width != value)
                {
                    p_width = value;
                    OnPropertyChanged("P_width");
                }
            }
        }
        public double P_height
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    p_height = P_Wight * 2;
                }
                else
                {
                    p_height = 0;
                }
                return p_height;
            }
            set
            {
                if (p_height != value)
                {
                    p_height = value;
                    OnPropertyChanged("P_height");
                }
            }
        }

        public double L_thick
        {
            get
            {
                if (a_x > 0 || a_y > 0)
                {
                    l_thick = L_Wight;
                }
                else
                {
                    l_thick = 0;
                }
                return l_thick;
            }
            set
            {
                if (l_thick != value)
                {
                    l_thick = value;
                    OnPropertyChanged("L_thick");
                }
            }
        }

        public Thickness T_margin
        {
            get
            {
                if (a_x > 0)
                {
                    t_margin = new Thickness((a_x / 4), (a_y / 4), 0, 0);
                }
                else
                {
                    t_margin = new Thickness();
                }
                return t_margin;
            }
            set
            {
                if (t_margin != value)
                {
                    t_margin = value;
                    OnPropertyChanged("T_margin");
                }
            }
        }

        //private double a_x;
        //private double a_y;
        //private double x;
        //private double y;
        //private double p_x;
        //private double p_y;
        //private string info;
        //private Thickness p_margin;
        //private Thickness t_margin;
        //private double p_width;
        //private double p_height;
        //private double l_thick;

        public double A_x
        {
            get
            {
                return a_x;
            }
            set
            {
                if (a_x != value)
                {
                    a_x = value;
                    OnPropertyChanged("A_x");
                    OnPropertyChanged("A_y");
                    OnPropertyChanged("X");
                    OnPropertyChanged("Y");
                    OnPropertyChanged("P_x");
                    OnPropertyChanged("P_y");
                    OnPropertyChanged("Info");
                    OnPropertyChanged("P_margin");
                    OnPropertyChanged("T_margin");
                    OnPropertyChanged("P_width");
                    OnPropertyChanged("P_height");
                    OnPropertyChanged("L_thick");
                }
            }
        }

        public double A_y
        {
            get
            {
                return a_y;
            }
            set
            {
                if (a_y != value)
                {
                    a_y = value;
                    OnPropertyChanged("A_x");
                    OnPropertyChanged("A_y");
                    OnPropertyChanged("X");
                    OnPropertyChanged("Y");
                    OnPropertyChanged("P_x");
                    OnPropertyChanged("P_y");
                    OnPropertyChanged("Info");
                    OnPropertyChanged("P_margin");
                    OnPropertyChanged("T_margin");
                    OnPropertyChanged("P_width");
                    OnPropertyChanged("P_height");
                    OnPropertyChanged("L_thick");
                }
            }
        }

    }
}
