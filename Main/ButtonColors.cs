using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LightConductor.Main
{
    class ButtonColors : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Brush up_color;
        private Brush right_color;
        private Brush left_color;
        private Brush down_color;

        public ButtonColors(Brush up_color, Brush right_color, Brush left_color, Brush down_color)
        {
            this.up_color = up_color;
            this.right_color = right_color;
            this.left_color = left_color;
            this.down_color = down_color;
        }

        public Brush Up_color
        {
            get
            {
                
                return up_color;
            }
            set
            {
                if (up_color != value)
                {
                    up_color = value;
                    OnPropertyChanged("Up_color");
                }
            }
        }

        public Brush Right_color
        {
            get
            {

                return right_color;
            }
            set
            {
                if (right_color != value)
                {
                    right_color = value;
                    OnPropertyChanged("Right_color");
                }
            }
        }

        public Brush Left_color
        {
            get
            {

                return left_color;
            }
            set
            {
                if (left_color != value)
                {
                    left_color = value;
                    OnPropertyChanged("Left_color");
                }
            }
        }

        public Brush Down_color
        {
            get
            {

                return down_color;
            }
            set
            {
                if (down_color != value)
                {
                    down_color = value;
                    OnPropertyChanged("Down_color");
                }
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
