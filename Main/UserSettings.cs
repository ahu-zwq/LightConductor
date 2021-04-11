using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    public sealed class UserSettings : System.Configuration.ApplicationSettingsBase, IUserSettings
    {

        private static readonly bool ThrowOnErrorDeserializing = false, ThrowOnErrorSerializing = false;
        private static IUserSettings defaultInstance = ((UserSettings)System.Configuration.ApplicationSettingsBase.Synchronized(new UserSettings()));
        private static readonly System.Configuration.SettingsAttributeDictionary SettingsAttributes = new System.Configuration.SettingsAttributeDictionary() {
            {typeof(System.Configuration.UserScopedSettingAttribute), new System.Configuration.UserScopedSettingAttribute()}
        };

        private System.Configuration.SettingsProvider provider;

        private UserSettings()
        {
        }

        public static IUserSettings Instance
        {
            get
            {
                return defaultInstance;
            }
        }

        public void Register<T>(string name, T defaultValue)
        {
            if (name == null || name.Trim().Length == 0)
                throw new ArgumentNullException("name");
            var property = this.Properties[name];
            if (property == null)
                this.CreateSettingsProperty(name, typeof(T), defaultValue);
        }

        public bool Contains(string name)
        {
            if (name == null || name.Trim().Length == 0)
                throw new ArgumentNullException("name");
            var property = this.Properties[name];
            return property != null;
        }

        public void Set<T>(string name, T value)
        {
            if (this.Contains(name) == false)
                this.Register<T>(name, value);
            this[name] = value;
        }

        public T Get<T>(string name, T defaultValue)
        {
            if (name == null || name.Trim().Length == 0)
                throw new ArgumentNullException("name");
            if (this.Contains(name))
            {
                return (T)(this[name] ?? defaultValue);
            }
            else
            {
                this.CreateSettingsProperty(name, typeof(T), defaultValue);
                var val = this[name];
                //if(val == null) this.Remove(name);                
                return (T)(val ?? defaultValue);
            }
        }

        public void Remove(string name)
        {
            if (name == null || name.Trim().Length == 0)
                throw new ArgumentNullException("name");
            //var property = this.Properties[key];
            //if (property != null)
            this.PropertyValues.Remove(name);
            this.Properties.Remove(name);
        }

        private void CreateSettingsProperty(string name, Type propertyType, object defaultValue)
        {
            var property = new System.Configuration.SettingsProperty(name, propertyType, this.Provider, false, defaultValue,
                this.GetSerializeAs(propertyType), SettingsAttributes, ThrowOnErrorDeserializing, ThrowOnErrorSerializing);
            this.Properties.Add(property);
        }

        private System.Configuration.SettingsSerializeAs GetSerializeAs(Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            bool flag = converter.CanConvertTo(typeof(string));
            bool flag2 = converter.CanConvertFrom(typeof(string));
            if (flag && flag2)
            {
                return System.Configuration.SettingsSerializeAs.String;
            }
            return System.Configuration.SettingsSerializeAs.Xml;
        }

        private System.Configuration.SettingsProvider Provider
        {
            get
            {
                if (this.provider == null && (this.provider = this.Providers["LocalFileSettingsProvider"]) == null)
                {
                    this.provider = new System.Configuration.LocalFileSettingsProvider();
                    this.provider.Initialize(null, null);
                    this.Providers.Add(this.provider);
                }
                return this.provider;
            }
        }

    }

}
