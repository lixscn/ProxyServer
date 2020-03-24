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
using ProxyLibrary;
using static ProxyLibrary.StateObject;

namespace ProxyServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SocketService sks = null;
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 窗口加载完成后运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentRensered_init(object sender, EventArgs e)
        {
            //SocketService sks = new SocketService();
            //sks.OnStart();

            proxyRadioIsChecked(true);

        }

        private void proxyRadioIsChecked(bool bo)
        {
            this.proxyRadio.IsChecked = bo;
            if (this.proxyRadio.IsChecked == true)
            {
                this.remoteLabel.IsEnabled = false;
                this.remoteTextBox.IsEnabled = false;
                this.remotePortLabel.IsEnabled = false;
                this.remotePortTextBox.IsEnabled = false;
            }
            else
            {
                this.remoteLabel.IsEnabled = true;
                this.remoteTextBox.IsEnabled = true;
                this.remotePortLabel.IsEnabled = true;
                this.remotePortTextBox.IsEnabled = true;
            }
        }

        private void transmitRadioChecked(object sender, RoutedEventArgs e)
        {
            proxyRadioIsChecked(false);
        }

        private void proxyRadioChecked(object sender, RoutedEventArgs e)
        {
            proxyRadioIsChecked(true);
        }

        private void ServerStartButtonClick(object sender, RoutedEventArgs e)
        {
            
            if (this.ServerStartButton.Content.Equals("服务启动")) { 
                EnabledServerSteupPanel(false);
                this.statLabel.Content = "服务器启动中...";
                string host = this.ServerIPTextBox.Text;
                Int32 port = 5556;
                try
                {
                    port = Int32.Parse(this.ServerPortTextBox.Text);
                }
                catch { }
                string remoteHost = this.remoteTextBox.Text;
                Int32 remotePort = 1080;
                try
                {
                    remotePort = Int32.Parse(this.remotePortTextBox.Text);
                }
                catch { }


                CONNMODEL ConnModel;
                if (this.proxyRadio.IsChecked == true)   //代理模式
                {
                    ConnModel = CONNMODEL.PROXY;
                    sks = new SocketService(host, port, ConnModel);
                }
                if (this.proxyRadio.IsChecked == false) //转发模式
                {
                    ConnModel = CONNMODEL.TRANSMIT;
                    sks = new SocketService(host, port, ConnModel, remoteHost, remotePort);
                }

                
                bool bo = sks.OnStart();
                if (bo == true)
                {
                    this.statLabel.Content = "服务器运行中";
                    this.ServerStartButton.Content = "服务停止";
                }
                else
                {
                    this.statLabel.Content = "服务器启动失败";
                    
                }
            }
            else //服务停止
            {
                EnabledServerSteupPanel(true);
                sks.OnStop();
                this.ServerStartButton.Content = "服务启动";
            }

        }
        private void EnabledServerSteupPanel(bool bo)
        {
            
            this.ServerIPLabel.IsEnabled = bo;
            this.ServerIPTextBox.IsEnabled = bo;
            this.ServerPortLabel.IsEnabled = bo;
            this.ServerPortTextBox.IsEnabled = bo;
            if (this.proxyRadio.IsChecked == false) {
                this.remoteLabel.IsEnabled = bo;
                this.remoteTextBox.IsEnabled = bo;
                this.remotePortLabel.IsEnabled = bo;
                this.remotePortTextBox.IsEnabled = bo;
            }
            

        }
    }
}
