using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;

using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;

namespace LightConductor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();

            //Rect rc = SystemParameters.WorkArea; //获取工作区大小
            //this.Left = 0; //设置位置
            //this.Top = 0;
            //this.Width = rc.Width;
            //this.Height = rc.Height;

            //this.WindowState = System.Windows.WindowState.Maximized;
            //this.WindowStyle = System.Windows.WindowStyle.None;

        }

        private Pages.Setting_D Setting_D;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Header as string;
            if (tabItem.Contains("设置")) {

                //this.SettingTab.Navigate(new Uri("Pages/Setting_D.xaml", UriKind.Relative));
                if (Setting_D == null) { 
                    Setting_D = new Pages.Setting_D();
                }
                this.SettingTab.Content = Setting_D;
                Setting_D.ParentWindow = this;

                Setting_D.UpdateLoginTb();
            }
         
        }
    }
}
