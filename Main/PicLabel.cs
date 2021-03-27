using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor
{
    class PicLabel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string pic_label;

        public PicLabel()
        {
        }

        public PicLabel(string pic_label)
        {
            this.pic_label = pic_label;
        }

        public string Pic_label
        {
            get
            {
                return pic_label;
            }
            set
            {
                if (pic_label != value)
                {
                    pic_label = value;
                    OnPropertyChanged("Pic_label");
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
