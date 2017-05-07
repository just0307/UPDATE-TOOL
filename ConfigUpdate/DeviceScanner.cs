using System.ComponentModel;
using InToolSet.Misc;

using InToolSet.Logging;
using InToolSet.Sys.Device;
using InToolSet.ViewModel.NetworkConfiguration;

using InToolSet.Util;
using InToolSet.Sys;
using System.Threading;
using SharpPcap;
using PacketDotNet;
using DeviceStatus;
using SnmpSharpNet;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using FileTransfer;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using InToolSet.Sys.Option;
using ConfigUpdate;

namespace Scanner
{
    public class MessageEventArgs : EventArgs
    {
        public const int STEP_DEVICE_FOUNDING = 0;
        public const int STEP_IP_FOUNDING = 1;
        public const int STEP_IP_FOUNDED = 2;
        public const int STEP_LOCAL_IP_MODIFIING = 3;
        public const int STEP_LOCAL_IP_MODIFIED = 4;
        public const int STEP_DEVICE_CREATING = 5;
        public const int STEP_DEVICE_CREATED = 6;
        public const int STEP_SCAN_END = 99;


        public int Message;
        public MessageEventArgs(int message)
        {
            this.Message = message;
        }
    }

    class DeviceScanner
    {
        /// <summary>
        /// 状态返回值定义
        /// </summary>
        public const int FINISH_NO = 0;
        public const int FINISH_OK = 1;
        public const int FINISH_ERROR = 2;

        
        private static bool isScanning = false;
        private bool isCapturing = false;
        //private DeviceParameterViewModel m_DeviceParameterViewModel = null;
        public delegate void MessageHandler(MessageEventArgs e);
        private LoadFileProgressModel m_progressModel = LoadFileProgressModel.Instance;
        private System.Timers.Timer m_ARPtimer = new System.Timers.Timer(10000);
        private System.Timers.Timer m_IPtimer = new System.Timers.Timer(3000);
        private bool isARPTimerElapsed = false;
        private bool isIPTimerElapsed = false;

        public static Dictionary<int, string> STEP_DIC = new Dictionary<int, string>
        {
            {MessageEventArgs.STEP_DEVICE_FOUNDING,"寻找设备"},
            {MessageEventArgs.STEP_IP_FOUNDING,"获取设备IP"},
            {MessageEventArgs.STEP_IP_FOUNDED,"设备IP已获取"},
            {MessageEventArgs.STEP_LOCAL_IP_MODIFIING,"修改本机IP"},
            {MessageEventArgs.STEP_LOCAL_IP_MODIFIED,"本机IP已修改"},
            {MessageEventArgs.STEP_DEVICE_CREATING,"设备正在创建"},
            {MessageEventArgs.STEP_DEVICE_CREATED,"设备已创建"},
        };

        private void DeviceScanMessageHandler(MessageEventArgs e)
        {
            switch (e.Message)
            {
                case MessageEventArgs.STEP_DEVICE_FOUNDING:
                case MessageEventArgs.STEP_IP_FOUNDING:
                case MessageEventArgs.STEP_IP_FOUNDED:
                case MessageEventArgs.STEP_LOCAL_IP_MODIFIING:
                case MessageEventArgs.STEP_LOCAL_IP_MODIFIED:
                case MessageEventArgs.STEP_DEVICE_CREATING:
                case MessageEventArgs.STEP_DEVICE_CREATED:
                    UpdateProgressBarInfomation(STEP_DIC[e.Message]);
                    break;
                case MessageEventArgs.STEP_SCAN_END:
                    quitScan();
                    break;
                default:
                    break;
            }

        }

        public void UpdateProgressBarInfomation(string information)
        {
            lock (m_progressModel)
            {
                m_progressModel.Information = information;
            }
        }

        public void DeviceScan()//DeviceParameterViewModel DeviceViewModel
        {
            if (!isScanning)//如果正在扫描，不能进行新一次的扫描
            {
                //m_DeviceParameterViewModel = DeviceViewModel;
                isScanning = true;
                Thread thread = new Thread(DeviceScanHandle);
                thread.Start();
            }
        }

        private void DeviceScanHandle()
        {
            DeviceScanMessageHandler(new MessageEventArgs(MessageEventArgs.STEP_DEVICE_FOUNDING));
            ToCapture();
            if (0 == NetConnection.Instance.ConnectStatus)
                NetConnection.Instance.ConnectStatus = FINISH_OK;
        }

        private void ToCapture()
        {
            m_progressModel.ItemsTotal = 1;
            m_progressModel.ItemsCompleted = 1;
            NetWorkInterface.Init();
            if (NetWorkInterface.mDevice == null)
            {
                System.Windows.Forms.MessageBox.Show("请检查界面中的网络连接选择");
                isScanning = false;
                m_progressModel.ItemsCompleted++;
                NetConnection.Instance.ConnectStatus = FINISH_ERROR;
                return;
            }
            NetWorkInterface.mDevice.Open();
            NetWorkInterface.mDevice.OnPacketArrival += OnPacketArrival;
            NetWorkInterface.mDevice.StartCapture();
            isCapturing = true;//开始抓包
            //首先确认是否能够抓取设备直接发出来的包
            m_IPtimer.Elapsed += new System.Timers.ElapsedEventHandler(IPTimerElapsed);
            m_IPtimer.Start();
            isIPTimerElapsed = false;
            while (isCapturing)
            {
                Thread.Sleep(50);
            }
            if (isIPTimerElapsed)
            {
                NetWorkInterface.mDevice.OnPacketArrival -= OnPacketArrival;
                NetWorkInterface.mDevice.StopCapture();
                NetWorkInterface.mDevice.Close();
                SwtichOnOffNetWorkInterfaceToCapture();//通过关闭网口再开启，触发模块发送arp的包
            }
            else
            {
                DeviceScanMessageHandler(new MessageEventArgs(MessageEventArgs.STEP_IP_FOUNDED));
            }
        }

        private void SwtichOnOffNetWorkInterfaceToCapture()
        {
            if (NetWorkInterface.Close())//关闭网口
            {
                Thread.Sleep(2000);
                if (!NetWorkInterface.Open())//开启网口
                {
                    System.Windows.Forms.MessageBox.Show("发现设备失败,请重新扫描");
                    quitScan();
                    NetConnection.Instance.ConnectStatus = FINISH_ERROR;
                    return;
                }
                NetWorkInterface.Init();
                if (NetWorkInterface.mDevice == null)
                {
                    System.Windows.Forms.MessageBox.Show("请检查界面中的网络连接选择");
                    isScanning = false;
                    m_progressModel.ItemsCompleted++;
                    NetConnection.Instance.ConnectStatus = FINISH_ERROR;
                    return;
                }
                NetWorkInterface.mDevice.Open();
                NetWorkInterface.mDevice.OnPacketArrival += OnPacketArrival;
                NetWorkInterface.mDevice.StartCapture();
                isCapturing = true;//开始抓包
                m_ARPtimer.Elapsed += new System.Timers.ElapsedEventHandler(ARPTimerElapsed);
                m_ARPtimer.Start();
                isARPTimerElapsed = false;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("发现设备失败,建议在确认连接正常后，再重新扫描");
                quitScan();
                NetConnection.Instance.ConnectStatus = FINISH_ERROR;
                return;
            }
            while (isCapturing)
            {
                Thread.Sleep(50);
            }
            if (isARPTimerElapsed)
            {
                System.Windows.Forms.MessageBox.Show("发现设备失败,建议在确认连接正常后，再重新扫描");
                quitScan();
                NetConnection.Instance.ConnectStatus = FINISH_ERROR;
                return;
            }
            else
            {
                DeviceScanMessageHandler(new MessageEventArgs(MessageEventArgs.STEP_IP_FOUNDED));
            }
        }

        void ARPTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isCapturing == true)
            {
                isCapturing = false;
                isARPTimerElapsed = true;
                m_ARPtimer.Stop();
            }          
        }

        void IPTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isCapturing == true)
            {
                isCapturing = false;
                isIPTimerElapsed = true;
                m_IPtimer.Stop();
            }   
        }
        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (!isCapturing)
            {
                return;
            }
            try
            {
                System.Net.IPHostEntry myEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                string myIp = getLocalIP();
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                string srcIp = "0.0.0.0";
                if (packet is PacketDotNet.EthernetPacket)//抓取以太网帧
                {
                    var udp = PacketDotNet.UdpPacket.GetEncapsulated(packet);
                    if (udp != null)
                    {
                        IpPacket ippacket = PacketDotNet.IpPacket.GetEncapsulated(packet);
                        string tempip = ippacket.SourceAddress.ToString();
                        if (isIPCheck(tempip))
                        {
                            srcIp = tempip;
                        }

                    }
                    else
                    {
                        var arp = PacketDotNet.ARPPacket.GetEncapsulated(packet);
                        if (arp != null)//抓取ARP包
                        {
                            ARPPacket arppacket = (ARPPacket)arp;
                            srcIp = arppacket.SenderProtocolAddress.ToString();//获取ARP包中的sender的IP                
                        }
                    }
                    if (!srcIp.Equals("0.0.0.0") && !srcIp.Equals("127.0.0.1") && !myIp.Equals(srcIp))//过滤干扰ARP包
                    {
                        Thread thread = new Thread(AfterFindDeviceIP);//以获取设备IP，另起线程进行设备类型的获取
                        thread.Start(srcIp);
                        isCapturing = false;//结束抓包
                    }
                }
            }
            catch(Exception ee)
            {
                System.Windows.Forms.MessageBox.Show("连接异常！请检查选择的网络连接");
                Console.WriteLine("Error: {0}", ee);
            }

        }

        private bool isIPCheck(string str)
        {
            return Regex.IsMatch(str, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        private String getLocalIP()
        {
            NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            int len = interfaces.Length;
            String localip = "";
            for (int i = 0; i < len; i++)
            {
                NetworkInterface ni = interfaces[i];
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (ni.Name == OptionFormModel.m_LocalAreaConnection)
                    {
                        IPInterfaceProperties property = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in
                            property.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                localip = ip.Address.ToString();
                            }
                        }
                    }
                }

            }
            return localip;
        }
        private void AfterFindDeviceIP(object ip)
        {
             string srcIp = ip as string;
            string newLocalIP = NetWorkInterface.getNewLocalIPAddress(srcIp);//按照设备IP设定本机新IP
            NetWorkInterface.SetStaticIP(newLocalIP, "255.0.0.0", "");//修改本机ip和掩码
            Thread.Sleep(500);
            DeviceScanMessageHandler(new MessageEventArgs(MessageEventArgs.STEP_LOCAL_IP_MODIFIED));
            FileTransfer.TCPCommand tcpCommand = TCPCommand.GetInstance();
            if (tcpCommand.Connect(IPAddress.Parse(srcIp), TCPCommand.PORT, 50))
            {
                CommandsFromTools commandMsgs = tcpCommand.FormatCommand(FileTransfer.TCPCommand.GET_DEV_TYPE);
                tcpCommand.SendMsg(commandMsgs.RequestCommand);
                string deviceType = null;
                int ret = tcpCommand.RecMsg_GetInfo(TCPCommand.GET_DEV_TYPE, ref deviceType);
                tcpCommand.SendMsg(commandMsgs.ACKCommand);
                tcpCommand.Disconnect();

                if (ret == FileTransfer.TCPCommand.TRANS_OK)
                {
                    if (deviceType == null || deviceType == "")
                    {
                        quitScan();
                        return;
                    }
                    //else
                    //{
                    //    modifyDeviceCurrentIP(deviceType, srcIp);
                    //}
                    Action action = () =>
                    {
                        modifyDeviceCurrentIP(deviceType, srcIp);
                        //if (m_DeviceParameterViewModel != null)
                        //{
                        //    //修改设备当前IP
                        //    modifyDeviceCurrentIP(deviceType, srcIp);
                        //}
                        //else
                        //{
                        //    //生成设备
                        //    ////generateDevice(deviceType, srcIp);
                        //}
                    };
                    if (System.Threading.Thread.CurrentThread != MainWindow.Instance.Dispatcher.Thread)
                    {
                        MainWindow.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, action);
                    }
                    else
                    {
                        action();
                    }
                }
            }
            else
            {
            }
            DeviceScanMessageHandler(new MessageEventArgs(MessageEventArgs.STEP_SCAN_END));
        }

        private void modifyDeviceCurrentIP(string type, string ip)
        {
            string deviceType = type;
            if (GlobalDictionary.DEVICE_NAME_DIC.ContainsKey(type))
            {
                deviceType = GlobalDictionary.DEVICE_NAME_DIC[type];
            }
  
            NetConnection.Instance.m_CurrentIPAddr = ip;
        }

        private List<SlotInfo> getSlotInfo(string ip)
        {
            return getSlotInfoFromSNMP(ip);
        }

        private string getChassisType(string ip)
        {
            string type = null;
            try
            {
                OctetString community = new OctetString("public");
                AgentParameters param = new AgentParameters(community);
                param.Version = SnmpVersion.Ver2;
                IpAddress agent = new IpAddress(ip);
                UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
                Pdu pdu = new Pdu(PduType.Get);
                pdu.VbList.Add("1.3.6.1.4.1.37720.8.2.0");
                pdu.VbList.Add("1.3.6.1.4.1.37720.8.3.0");
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                string chassisName = result.Pdu.VbList[0].Value.ToString();
                string num = result.Pdu.VbList[1].Value.ToString();
                int length = int.Parse(num) * 4;
                type = string.Format("{0} {1}R", chassisName, length);
            }
            catch
            {

            }
            return type;
        }

        private List<SlotInfo> getSlotInfoFromSNMP(string ip)
        {
            List<int> slots = new List<int>();
            List<string> plugin = new List<string>();
            try
            {
                snmpWalk(ip, "1.3.6.1.4.1.37720.8.4.1.1", ref slots);
                snmpWalk(ip, "1.3.6.1.4.1.37720.8.4.1.3", ref plugin);
            }
            catch
            {

            }
            List<SlotInfo> slotInfoList = new List<SlotInfo>();
            for(int i = 0; i < slots.Count; i++)
            {
                slotInfoList.Add(new SlotInfo(slots[i],plugin[i]));
            }
            return slotInfoList;
        }

        private void snmpWalk(string ip, string oid, ref List<int> list)
        {
            List<String> listString = new List<string>();
            snmpWalk(ip, oid, ref listString);
            foreach(string str in listString)
            {
                list.Add(int.Parse(str) - 1);
            }
        }

        private void snmpWalk(string ip, string oid, ref List<string> list)
        {
            OctetString community = new OctetString("public");
            AgentParameters param = new AgentParameters(community);
            param.Version = SnmpVersion.Ver2;
            IpAddress agent = new IpAddress(ip);
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            Oid rootOid = new Oid(oid);
            Oid lastOid = (Oid)rootOid.Clone();
            Pdu pdu = new Pdu(PduType.GetBulk);
            pdu.NonRepeaters = 0;
            pdu.MaxRepetitions = 30;
            while (lastOid != null)
            {
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                pdu.VbList.Clear();
                pdu.VbList.Add(lastOid);
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                if (result != null)
                {
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        break;
                    }
                    else
                    {
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                list.Add(v.Value.ToString());
                                lastOid = v.Oid;
                            }
                            else
                            {
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }
            target.Dispose();
        }

        private class SlotInfo
        {
            public int slot;
            public string pluginType;
            public SlotInfo(int pSlot, string pPluginType)
            {
                slot = pSlot;
                pluginType = pPluginType;
            }
        }

        private void quitScan()
        {
            NetWorkInterface.mDevice.StopCapture();
            NetWorkInterface.mDevice.OnPacketArrival -= OnPacketArrival;
            NetWorkInterface.mDevice.Close();
            isScanning = false;
            m_progressModel.ItemsCompleted++;
        }
    }
}
