using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    class CameraPair
    {
        private string id;
        private DeviceModule deviceModule;
        private string name;
        private VideoHandle topVideoHandle;
        private VideoHandle mainVideoHandle;
        private TDCHandle verticalTDC;
        private TDCHandle horizontalTDC;

        public CameraPair(string id, DeviceModule deviceModule, string name, VideoHandle topVideoHandle, VideoHandle mainVideoHandle, TDCHandle verticalTDC, TDCHandle horizontalTDC)
        {
            this.id = id;
            this.deviceModule = deviceModule;
            this.name = name;
            this.topVideoHandle = topVideoHandle;
            this.mainVideoHandle = mainVideoHandle;
            this.verticalTDC = verticalTDC;
            this.horizontalTDC = horizontalTDC;
        }

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public DeviceModule DeviceModule { get => deviceModule; set => deviceModule = value; }
        internal VideoHandle TopVideoHandle { get => topVideoHandle; set => topVideoHandle = value; }
        internal VideoHandle MainVideoHandle { get => mainVideoHandle; set => mainVideoHandle = value; }
        internal TDCHandle VerticalTDC { get => verticalTDC; set => verticalTDC = value; }
        internal TDCHandle HorizontalTDC { get => horizontalTDC; set => horizontalTDC = value; }


    }
}
