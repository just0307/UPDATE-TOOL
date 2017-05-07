/********************************************************************/
/*  file name    : OptionFormModel.cs                               */
/*  function     : OptionForm的viewmodel                            */
/*  date/version : 2015/11/24/v1.0                                  */
/*  author       : WN                                               */
/********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using InToolSet.Sys;
using InToolSet.Sys.Option;
using SharpPcap;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using InToolSet.Sys.Option;

namespace InToolSet.ViewModel.NetworkConfiguration
{
    public class OptionFormModel : INotifyPropertyChanged
    {
        private ISysSetting m_sysSetting;

    
        public static string m_LocalAreaConnection;
        public string LocalAreaConnection
        {
            get { return m_LocalAreaConnection; }
            set { m_LocalAreaConnection = value; OnPropertyChanged("LocalAreaConnection"); }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OptionFormModel()
        {
        }



        /// <summary>
        /// 阈值
        /// </summary>
        public double ThresholdNum
        {
            get { return m_sysSetting.MVBSplitThresholdValue; }
            set { m_sysSetting.MVBSplitThresholdValue = value; Notify("ThresholdNum"); }
        }

        /// <summary>
        /// 最近打开工程路径表
        /// </summary>
        public List<string> RecentProjectPathList
        {
            get { return m_sysSetting.RecentProjectPathList; }
        }

        public ISysSetting SysSetting
        {
            get { return m_sysSetting; }
            set { m_sysSetting = value; }
        }

        public List<string> LocalAreaConnectionList
        {
            get
            {
                CaptureDeviceList devices = null;
                try
                {
                    devices = CaptureDeviceList.Instance;
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("获取网络连接失败，请确认本地连接是否已经启用，并检查wpcap");
                    return null;
                }
                List<string> localAreaConnectionList = new List<string>();
                for (int i = 0; i < devices.Count; i++)
                { 
                    string devDescribe = devices[i].ToString();
                    Regex reg = new Regex("FriendlyName:.(.+)");
                    Match match = reg.Match(devDescribe);
                    string connectionName = match.Groups[1].Value;
                    localAreaConnectionList.Add(connectionName);
                }

                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                        && adapter.GetPhysicalAddress().ToString().Length != 0)
                    {
                        if(adapter.OperationalStatus != OperationalStatus.Up)
                            localAreaConnectionList.Remove(adapter.Name);
                    }
                    if (adapter.Name.Contains("无线") || adapter.Name.Contains("VMware") || adapter.Name.Contains("virtual"))
                        localAreaConnectionList.Remove(adapter.Name);
                }


                //设置默认连接,若没有名为“本地连接”的interface，则默认localAreaConnectionList中的第一个
                LocalAreaConnection = localAreaConnectionList[0];
                foreach (string connect in localAreaConnectionList)
                {
                    if (connect == "本地连接")
                        LocalAreaConnection = connect;
                }

                



                return localAreaConnectionList;

            }
        }
        public string m_CurrentIPAddr = "169.254.147.106";
        public string CurrentIPStr
        {
            get
            {
                return m_CurrentIPAddr;
            }
            set
            {
                if (value != m_CurrentIPAddr)
                {
                    m_CurrentIPAddr = value;
                    m_CurrentIPAddr = m_CurrentIPAddr.Replace(" ", "");
                    OnPropertyChanged("CurrentIPStr");
                }
            }
        }

        public string m_VersionResult = "";
        public string VersionResult
        {
            get
            {
                return m_VersionResult;
            }
            set
            {
                if (value != m_VersionResult)
                {
                    m_VersionResult = value;
                    OnPropertyChanged("VersionResult");
                }
            }
        }
        
    }

}
