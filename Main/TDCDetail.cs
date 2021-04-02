using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    class TDCDetail : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string tdcDetail;

        private string serialNo;
        private Decimal position;

        public TDCDetail()
        {

        }

        public TDCDetail(string serialNo, Decimal position)
        {
            this.serialNo = serialNo;
            this.position = position;
        }

        public string SerialNo
        {
            get
            {
                return serialNo;
            }
            set
            {
                if (serialNo != value)
                {
                    serialNo = value;
                    OnPropertyChanged("TdcDetail");
                    OnPropertyChanged("SerialNo");
                }
            }
        }

        public Decimal Position
        {
            get
            {
                return position;
            }
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged("TdcDetail");
                    OnPropertyChanged("Position");
                }
            }
        }

        public string TdcDetail
        {
            get
            {
                if (serialNo != null && position > -100000)
                {
                    tdcDetail = "[" + serialNo + "]  :  " + position;
                }
                else {
                    tdcDetail = "";
                }
                return tdcDetail;
            }
            set
            {
                if (tdcDetail != value)
                {
                    tdcDetail = value;
                    OnPropertyChanged("TdcDetail");
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
