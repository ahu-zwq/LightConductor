using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    public interface IUserSettings : INotifyPropertyChanged
    {
        void Register<T>(string name, T defaultValue);
        bool Contains(string name);
        //object Get(string name, object defaultValue);
        T Get<T>(string name, T defaultValue);
        void Set<T>(string name, T value);

        void Reload();
        void Save();
        void Upgrade();

    }
}
