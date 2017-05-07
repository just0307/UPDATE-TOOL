using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shell32;
using SharpPcap;
using PacketDotNet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using InToolSet.Sys.Option;
using InToolSet.ViewModel.NetworkConfiguration;
namespace InToolSet.Util
{
    class NetWorkInterface
    {
        public static ICaptureDevice mDevice;

        private static Process NetshProcess = new Process();
        private static bool SetNetwork(bool status)
        {

            return ChangeNetworkConnectionStatus(status, OptionFormModel.m_LocalAreaConnection);
        }

        private static bool ChangeNetworkConnectionStatus(bool enable, string networkConnectionName)
        {
            string netshCmd = "interface set interface name=\"{0}\" admin={1}";
            string arguments = String.Format(netshCmd, networkConnectionName, enable ? "ENABLED" : "DISABLED");
            return NetshCommand(arguments);
        }

        private static bool NetshCommand(string comand)
        {
            NetshProcess.EnableRaisingEvents = false;
            NetshProcess.StartInfo.Arguments = comand;
            NetshProcess.StartInfo.FileName = "netsh.exe";
            NetshProcess.StartInfo.CreateNoWindow = true;
            NetshProcess.StartInfo.ErrorDialog = false;
            NetshProcess.StartInfo.RedirectStandardError = false;
            NetshProcess.StartInfo.RedirectStandardInput = false;
            NetshProcess.StartInfo.RedirectStandardOutput = true;
            NetshProcess.StartInfo.UseShellExecute = false;
            NetshProcess.Start();
            string rtn = NetshProcess.StandardOutput.ReadToEnd();
            if (rtn.Trim().Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Open()
        {
            int count = 30;
            while(!SetNetwork(true))
            {
                Thread.Sleep(1000);
                count--;
                if (count < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Close()
        {
            int count = 30;
            while (!SetNetwork(false))
            {
                Thread.Sleep(1000);
                count--;
                if (count < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static void Init()
        {
            CaptureDeviceList devices = null;
            int count = 30;
            while (devices == null)
            {
                try
                {
                    devices = CaptureDeviceList.Instance;
                }
                catch
                {
                    Thread.Sleep(1000);
                    count--;
                    if (count < 0)
                    {
                        System.Windows.Forms.MessageBox.Show("发现设备失败,请重新扫描");
                        return;
                    }
                }
            }
            if (devices != null)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    string devDescribe = devices[i].ToString();
                    Regex reg = new Regex("FriendlyName:.(.+)");
                    Match match = reg.Match(devDescribe);
                    if (match.Groups[1].Value.Equals(OptionFormModel.m_LocalAreaConnection))
                    {
                        mDevice = devices[i];
                        return;
                    }
                }
            }
        }

        public static void RegisterPacketArrivalHandler(PacketArrivalEventHandler handler)
        {
            mDevice.OnPacketArrival += new PacketArrivalEventHandler(handler);
        }

        public static void StartCapture()
        {
            mDevice.Open();
            mDevice.StartCapture();
        }

        public static void StopCapture()
        {
            mDevice.StopCapture();
            mDevice.Close();         
        }

        public static void SetStaticIP(string ip, string mask, string gateway)
        {
            while(!ChangeNetWorkIP(ip,mask,gateway))
            {
                Thread.Sleep(1000);
            }
        }

        private static bool ChangeNetWorkIP(string ip, string mask, string gateway)
        {
            string netshCmd = "interface ip set address \"{0}\" static {1} {2} {3}";
            string rguments = String.Format(netshCmd, OptionFormModel.m_LocalAreaConnection, ip, mask, gateway);
            return NetshCommand(rguments);
        }

        public static string getNewLocalIPAddress(string deviceIP)
        {
            Regex reg = new Regex("\\d+\\.\\d+\\.\\d+\\.(\\d+)");
            Regex regRev = new Regex("(\\d+\\.\\d+\\.\\d+\\.)\\d+");
            Match matchLast = reg.Match(deviceIP);
            Match matchFirst = regRev.Match(deviceIP);
            string last = matchLast.Groups[1].Value;
            string first = matchFirst.Groups[1].Value;
            string ip = null;
            if (!last.Equals("249"))
            {
                ip = first + "249";
            }
            else
            {
                ip = first + "248";
            }
            return ip;
        }

        public static bool Ping(string ip)
        {
            if (ip.Equals("0.0.0.0"))
            {
                return false;
            }
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            int timeout = 1000; // Timeout 时间，单位：毫秒
            System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                return true;
            else
                return false;
        }
    }
}
