using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using FileTransfer;
using System.Text.RegularExpressions;


namespace FileTransfer
{
    public class CommandsFromTools
    {
        public string RequestCommand;
        public string ACKCommand;
    }

    public class TCPCommand
    {
        
        private static TCPCommand instance;
        private TCPCommand()
        {

        }
        public static TCPCommand GetInstance()
        {
            if(instance == null)
            {
                instance = new TCPCommand();
            }
            return instance;
        }
        Socket mSocketClient = null;
        
        static string Command_Header = "*";
        static string Command_Tail = "#";
        public static string ReceiveStrForProgress = "";
        public static int PORT = 5009;
        public static int PORT_PCI = 5008;

        public static string GET_PKG_VER         = "GET_PKG_VER";//获取底层软件版本
        public static string GET_BOOTROM_VER     = "GET_BOOTROM_VER";//获取BOOTROM版本
        public static string GET_PKG_BOOTROM_VER = "GET_PKG_BOOTROM_VER";//获取PKG文件中的BOOTROM版本
        public static string REQ_REBOOT          = "REQ_REBOOT";//复位模块
        public static string UPDATE_BOOTROM      = "UPDATE_BOOTROM";//更新BOOTROM
        public static string UPDATE_PKG          = "UPDATE_PKG";//更新底层软件
        public static string UPDATE_CFG          = "UPDATE_CFG";//更新配置文件
        public static string UPDATE_APP          = "UPDATE_APP";//更新应用程序
        public static string BACKUP_PKG          = "BACKUP_PKG";//备份底层软件
        public static string BACKUP_CFG          = "BACKUP_CFG";//备份配置文件
        public static string BACKUP_APP          = "BACKUP_APP";//备份应用程序
        public static string GET_DEV_TYPE        = "GET_DEV_TYPE";//获取底层软件版本
        public static string UPDATE_PKG_VIAPCI   = "UPDATE_PKG_VIAPCI";//通过PCI更新底层软件
        public static string UPDATE_FPGA_VIAPCI = "UPDATE_FPGA_VIAPCI";//通过PCI更新FPGA软件
        public static string GENERATE_TOPOLOGY_FILE = "GENERATE_TOPOLOGY_FILE";//请求生成拓扑文件
        public static string CRC_RECEIVE         = "CRC_RECEIVE";//请求请求获取拓扑文件CRC码
        public static string CRC_SEND            = "CRC_SEND";//发送拓扑文件(只包含拓扑关系)的CRC码
        public static string REQ_FORMAT          = "REQ_FORMAT";//格式化
        public static string GET_TFFS_VER        = "GET_TFFS_VER";//获取TFFS版本


        public static int ACTION_UPDATE = 0;
        public static int ACTION_BACKUP = 1;

        public const int FILETYPE_FIRMWARE = 0; // 固件文件
        public const int FILETYPE_APPLICATION = 1; // 应用程序文件
        public const int FILETYPE_FPGA = 2; // FPGA文件
        public const int FILETYPE_CPLD = 3; // CPLD文件
        public const int FILETYPE_CONFIG_TOPOLOGY = 4; // 拓扑配置文件(在线拓扑扫描时，使用此接口)
        public const int FILETYPE_DIAGNOSIS = 5; // 诊断信息文件
        public const int FILETYPE_CONFIG_DEVICE = 6; // 设备配置信息文件
        public const int FILETYPE_CONFIG_MVB = 7; // MVB配置文件
        public const int FILETYPE_CONFIG_TRDP = 8; // TRDP配置文件
        public const int FILETYPE_OTHER = 9; // 其它文件
        public const int FILETYPE_BSP = 20; // BSP文件

        public const int TRANS_OK = 0;
        public const int TRANS_ERROR = 1;
        public const int TRANS_BUSY = 2;


        public const int END_OK = 0;
        public const int END_ERROR = 1;
        public const int END_BUSY = 2;
        public const int CONTINUE = 3;

        public static bool IsDeviceBusy = false;

        public static string pre_lastline = "";
        public static int count_receive_same_info = 0;

        public delegate void UpdateProgressBarDelegate(string ProgressInfo, long Bytes);
        public  UpdateProgressBarDelegate ProgressBarCallBack;
        public  string UpdateDeviceName = "";

        public string m_strIPAddr = "";
        public bool isOnLine = false;
        public void CallBack(string UpdateProgressBarInfo,long ReceiveBytes)
        {
            ProgressBarCallBack(UpdateProgressBarInfo, ReceiveBytes);
        }
        public void registerCallBack(UpdateProgressBarDelegate DelegateProgressBar)
        {
            ProgressBarCallBack = DelegateProgressBar;
        }
        public static Dictionary<int, string> UPDATE_COMMAND_DIC = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,UPDATE_PKG},
            {FILETYPE_APPLICATION,UPDATE_APP},
            {FILETYPE_CONFIG_MVB,UPDATE_CFG},
            {FILETYPE_CONFIG_TRDP,UPDATE_CFG},
            {FILETYPE_CONFIG_DEVICE,UPDATE_CFG},
            {FILETYPE_CONFIG_TOPOLOGY,UPDATE_CFG},
            {FILETYPE_OTHER,UPDATE_CFG},
            {FILETYPE_FPGA,UPDATE_FPGA_VIAPCI},
            {FILETYPE_BSP,UPDATE_BOOTROM}
        };

        public static Dictionary<int, string> UPDATE_COMMAND_DIC_PCI = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,UPDATE_PKG_VIAPCI},
            {FILETYPE_FPGA,UPDATE_FPGA_VIAPCI}
        };

        public static Dictionary<int, string> BACKUP_COMMAND_DIC = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,BACKUP_PKG},
            {FILETYPE_APPLICATION,BACKUP_APP},
            {FILETYPE_CONFIG_MVB,BACKUP_CFG},
            {FILETYPE_CONFIG_TRDP,BACKUP_CFG},
            {FILETYPE_CONFIG_DEVICE,BACKUP_CFG},
            {FILETYPE_CONFIG_TOPOLOGY,BACKUP_CFG},
            {FILETYPE_OTHER,BACKUP_CFG}
        };

        public static Dictionary<int, string> FILENAME_DIC = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,"PKG.bin"},
            {FILETYPE_APPLICATION,"image.bin"},
            {FILETYPE_CONFIG_MVB,"mvb_confm.dat"},
            {FILETYPE_CONFIG_TRDP,"TRDPCFG.xml"},
            {FILETYPE_CONFIG_DEVICE,"G300CFG.xml"},
        };

        public bool Connect(IPAddress ipaddress, int port)
        {
            mSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
            try
            {
                mSocketClient.Connect(endpoint);
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        public bool Connect(IPAddress ipaddress, int port, int timeout)
        {
            mSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
            int count = timeout;
            while (true)
            {
                try
                {
                    mSocketClient.Connect(endpoint);
                    break;
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10061 || e.ErrorCode == 10060)
                    {
                        return false;
                    }
                    Thread.Sleep(100);
                    count--;
                    if (count < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Disconnect()
        {
            if(mSocketClient!=null && mSocketClient.Connected)
            {
                mSocketClient.Disconnect(false);
                mSocketClient.Close();
                Thread.Sleep(1000);
            }
        }

        public CommandsFromTools FormatCommand(int actiontype, string filename, int filetype, int slot)
        {
            string reqCmd = null;
            string ackCmd = null;
            ulong crc32 = 0;

            CommandsFromTools commands = new CommandsFromTools();
            try
            {
                switch (filetype)
                {
                    case FILETYPE_FIRMWARE:
                    case FILETYPE_APPLICATION:
                    case FILETYPE_CONFIG_MVB:
                    case FILETYPE_CONFIG_TRDP:
                    case FILETYPE_CONFIG_TOPOLOGY:
                    case FILETYPE_CONFIG_DEVICE:
                    case FILETYPE_OTHER:
                    case FILETYPE_FPGA:
                    case FILETYPE_BSP:
                        {
                            if (actiontype == ACTION_UPDATE)
                            {
                                if (slot > 0)
                                {
                                    reqCmd = string.Format("{0}\t{1}\n{2}\n{3}\n{4}\t{5}\t", UPDATE_COMMAND_DIC_PCI[filetype], FTPServerHelper.FTP_USR, FTPServerHelper.FTP_PASS, FTPServerHelper.Instance.getPort(), filename, slot.ToString());
                                }
                                else
                                {
                                    reqCmd = string.Format("{0}\t{1}\n{2}\n{3}\n{4}\t{5}\t", UPDATE_COMMAND_DIC[filetype], FTPServerHelper.FTP_USR, FTPServerHelper.FTP_PASS, FTPServerHelper.Instance.getPort(), filename, "1");
                                }
                                ackCmd = string.Format("{0}\t{1}\t{2}\t", UPDATE_COMMAND_DIC[filetype], "ACK", "1");
                            }
                            else if (actiontype == ACTION_BACKUP)
                            {
                                reqCmd = string.Format("{0}\t{1}\n{2}\n{3}\n{4}\t{5}\t", BACKUP_COMMAND_DIC[filetype], FTPServerHelper.FTP_USR, FTPServerHelper.FTP_PASS, FTPServerHelper.Instance.getPort(), filename, "1");
                                ackCmd = string.Format("{0}\t{1}\t{2}\t", BACKUP_COMMAND_DIC[filetype], "ACK", "1");
                            }
                            break;
                        }

                }
            }
            catch
            {
                TCPCommand.GetInstance().CallBack("文件类型格式报错，请确认文件类型/更新方式/槽位号已配置正确！", -1);
            }
            
            crc32 = CRC32.GetCRC32Str(reqCmd);//CRC32计算
            reqCmd = reqCmd + crc32.ToString() + "\t";
            commands.RequestCommand = Command_Header + reqCmd + reqCmd.Length + Command_Tail;//格式化请求命令
            crc32 = CRC32.GetCRC32Str(ackCmd);
            ackCmd = ackCmd + crc32.ToString() + "\t";
            commands.ACKCommand = Command_Header + ackCmd + ackCmd.Length + Command_Tail;//格式化ACK命令
            return commands;
        }

        public CommandsFromTools FormatCommand(string command)
        {
            string reqCmd = null;
            string ackCmd = null;
            ulong crc32 = 0;
            CommandsFromTools commands = new CommandsFromTools();

            reqCmd = string.Format("{0}\t{1}\t{2}\t", command, "NULL", "1");
            ackCmd = string.Format("{0}\t{1}\t{2}\t", command, "ACK", "1");

            crc32 = CRC32.GetCRC32Str(reqCmd);//CRC32计算
            reqCmd = reqCmd + crc32.ToString() + "\t";
            commands.RequestCommand = Command_Header + reqCmd + reqCmd.Length + Command_Tail;//格式化请求命令
            crc32 = CRC32.GetCRC32Str(ackCmd);
            ackCmd = ackCmd + crc32.ToString() + "\t";
            commands.ACKCommand = Command_Header + ackCmd + ackCmd.Length + Command_Tail;//格式化ACK命令
            return commands;
        }
        public CommandsFromTools FormatCommand(string command,string topologyfilecrc)
        {
            string reqCmd = null;
            string ackCmd = null;
            ulong crc32 = 0;
            CommandsFromTools commands = new CommandsFromTools();

            reqCmd = string.Format("{0}\t{1}\t{2}\t", command, topologyfilecrc, "1");
            ackCmd = string.Format("{0}\t{1}\t{2}\t", command, "ACK", "1");

            crc32 = CRC32.GetCRC32Str(reqCmd);//CRC32计算
            reqCmd = reqCmd + crc32.ToString() + "\t";
            commands.RequestCommand = Command_Header + reqCmd + reqCmd.Length + Command_Tail;//格式化请求命令
            crc32 = CRC32.GetCRC32Str(ackCmd);
            ackCmd = ackCmd + crc32.ToString() + "\t";
            commands.ACKCommand = Command_Header + ackCmd + ackCmd.Length + Command_Tail;//格式化ACK命令
            return commands;
        }
        public Socket getSocket()
        {
            return mSocketClient;
        }

        public delegate int RecMsgHandler(Socket socketClient);

        public int RecMsg_FileTrans(string commandHead)
        {
            int ret = TRANS_OK;
            byte[] lastMsg = null;
            TCPCommand.IsDeviceBusy = false;
            do
            {
                byte[] arrRecMsg = new byte[1000];
                mSocketClient.Receive(arrRecMsg);
                List<string> MessageBuffer = MessageAssembly(arrRecMsg, ref lastMsg);

                string lastLine = MessageBuffer[MessageBuffer.Count-1];
                //if (lastLine == pre_lastline)
                //    count_receive_same_info++;
                //if(count_receive_same_info>200)
                //{
                //    CallBack("传输异常：不断重复接收相同信息！", -1);
                //    count_receive_same_info = 0;
                //    return END_ERROR;
                //}
                string ProgressBarInfo = "";
                string BytesDownloaded = "0";
                if (lastLine.Contains("已下载"))
                {
                    string str =lastLine.Trim();
                    string[] list1 = System.Text.RegularExpressions.Regex.Split(str, "已下载");
                    string[] list2 = list1[1].Split(' ');
                    BytesDownloaded = list2[0];
                    ProgressBarInfo = "已下载" + BytesDownloaded + "字节";
                    CallBack(ProgressBarInfo, Int64.Parse(BytesDownloaded));
                }
                else if (lastLine.Contains("%") || lastLine.Contains("percent"))
                {
                    if(lastLine.Contains("%"))
                    {
                        string[] list = lastLine.Split('\t', '\n', ' ');
                        if (list.Length > 2)
                            ProgressBarInfo = list[2];
                        else
                            ProgressBarInfo = "设备反馈数据包解析错误";
                    }
                    else if (lastLine.Contains("percent"))
                    {
                        string[] list = lastLine.Split('\t', '\n', ' ');
                        if (list.Length > 5)
                            ProgressBarInfo = list[2] + " "+ list[3] + " "+ list[4]+" "+ list[5];
                        else
                            ProgressBarInfo = "设备反馈数据包解析错误";
                    }
                    
                    CallBack(ProgressBarInfo, Int64.Parse(BytesDownloaded));
                }
                else
                {
                    string[] list = lastLine.Split('\t', '\n', ' ');
                    if (list.Length > 2)
                        ProgressBarInfo = list[1] + " " + list[2];
                    else
                        ProgressBarInfo = "设备反馈数据包解析错误";
                    CallBack(ProgressBarInfo, -1);//该情况与进度显示无关，传-1
                }
                //Console.WriteLine(lastLine);
                pre_lastline = lastLine;
                for (int i = 0; i < MessageBuffer.Count; i++)
                {
                    ret = MessegeCheck(MessageBuffer[i], commandHead);
                    if (ret == END_ERROR)
                    {
                        return TRANS_ERROR;
                    }
                    if (ret == END_OK)
                    {
                        return TRANS_OK;
                    }
                    if(ret == END_BUSY)
                    {
                        TCPCommand.IsDeviceBusy = true;
                        return TRANS_BUSY;
                    }
                }
            } while (true);
            return ret;
        }

        private List<string> MessageAssembly(byte[] NewMsg, ref byte[] lastMsg)
        {
            List<string> BufferList = new List<string>();
            byte[] TotalMsg;
            int lastTear;

            //如果上一个buffer里面有尾包，在这个buffer里面添加进来
            if (lastMsg != null && lastMsg.Length > 0)
            {
                TotalMsg = lastMsg.Concat(NewMsg).ToArray();
                lastMsg = null;
            }
            else
            {
                TotalMsg = NewMsg;
            }

            int length = TotalMsg.Length;

            for (lastTear = length - 1; lastTear >= 0; lastTear--)
            {
                byte b = TotalMsg[lastTear];
                if (b == 35)//获取最后一个#
                {
                    if (lastTear != (length - 1))
                    {
                        if (TotalMsg[lastTear + 1] == 42)//判断是否紧接着*
                        {
                            lastMsg = new byte[length - 1 - lastTear];
                            Array.Copy(TotalMsg, lastTear + 1, lastMsg, 0, length - 1 - lastTear);   
                        }   
                    }
                    break;
                }
            }
            //若没有遍历到#号，则将TotalMsg赋给lastMsg
            if (lastTear == -1)
            {
                lastMsg = new byte[TotalMsg.Length];
                Array.Copy(TotalMsg, 0, lastMsg, 0, TotalMsg.Length); 
            }
            string str = Encoding.Default.GetString(TotalMsg, 0, lastTear + 1);
            Console.WriteLine("==========START==========\n" + str + "\n==========END==========\n");
            string[] strList = str.Split('*', '#');

            foreach (string s in strList)
            {
                if (s.Length > 0)
                {
                    BufferList.Add(s);
                }
            }
            return BufferList;
        }

        public int RecMsg_FileTrans(string commandHead, ref string strRev)
        {
            int ret = TRANS_OK;
            byte[] lastMsg = null;
            TCPCommand.IsDeviceBusy = false;
            do
            {
                byte[] arrRecMsg = new byte[1000];
                mSocketClient.Receive(arrRecMsg);
                List<string> MessageBuffer = MessageAssembly(arrRecMsg, ref lastMsg);
                for(int i = 0;i < MessageBuffer.Count;i++)
                {
                    ret = MessegeCheck(MessageBuffer[i], commandHead, ref strRev);
                    if (ret == END_ERROR)
                    {
                        return TRANS_ERROR;
                    }
                    if(ret == END_OK)
                    {
                        return TRANS_OK;
                    }
                    if (ret == END_BUSY)
                    {
                        TCPCommand.IsDeviceBusy = true;
                        return TRANS_BUSY;
                    }
                }
            } while (true);     
        }

        private List<string> MessageAssembly(string Msg, ref string preMsg,ref string lastMsg)
        {
            List<string> RecMsgBuffer = new List<string>();
            int recMsgLen = Msg.Length;
            string[] splitMsg = Msg.Split('*', '#');
            int splitMsgLen = splitMsg.Length;
            if (splitMsgLen > 1)
            {
                if (!splitMsg[0].Equals(""))
                {
                    lastMsg = splitMsg[0];
                }
                if (!preMsg.Equals("") && !lastMsg.Equals(""))
                {
                    RecMsgBuffer.Add(preMsg + lastMsg);
                    preMsg = "";
                    lastMsg = "";
                }
                if (!splitMsg[splitMsgLen - 1].Equals(""))
                {
                    preMsg = splitMsg[splitMsgLen - 1];
                }
                for (int i = 1; i < splitMsgLen - 1; i++)
                {
                    if (!splitMsg[i].Equals(""))
                    {
                        RecMsgBuffer.Add(splitMsg[i]);
                    }
                }
            }
            else if (splitMsgLen == 1)
            {
                preMsg += splitMsg[0];
            }
            return RecMsgBuffer;
        }

        private int MessegeCheck(string strRecMsg, string commandHead)
        {
            int strLength = strRecMsg.Length;
            int ret = CONTINUE;
            string[] splitRecMsg = strRecMsg.Split('\t', '\n', '*');
            if (IsCrc(strRecMsg))
            {
                if (splitRecMsg[0] == commandHead)
                {
                    if (splitRecMsg[1] == "OK")
                    {
                        ret = END_OK;
                    }
                    else if (splitRecMsg[1] == "ERROR")
                    {
                        ret = END_ERROR;
                    }
                    else if (splitRecMsg[1] == "BUSY")
                    {
                        ret = END_BUSY;
                    }
                }
                else
                {
                    ret = END_ERROR;
                }
            }
            else
            {
                ret = END_ERROR;
            }
            return ret;
        }

        private int MessegeCheck(string strRecMsg, string commandHead, ref string strRev)
        {
            int strLength = strRecMsg.Length;
            int ret = CONTINUE;
            if (strRecMsg.Contains("上传文件"))
            {
                Regex reg = new Regex("\n上传文件([A-Za-z0-9-._]+)完成\t");
                Match match = reg.Match(strRecMsg);
                strRev = match.Groups[1].Value;
            }
            string[] splitRecMsg = strRecMsg.Split('\t', '\n', '*');
            if (IsCrc(strRecMsg))
            {
                if (splitRecMsg[0] == commandHead)
                {
                    if (splitRecMsg[1] == "OK")
                    {
                        ret = END_OK;
                    }
                    else if (splitRecMsg[1] == "ERROR")
                    {
                        ret = END_ERROR;
                    }
                    else if(splitRecMsg[1] == "BUSY")
                    {
                        ret = END_BUSY;
                    }
                }
                else
                {
                    ret = END_ERROR;
                }
            }
            else
            {
                ret = END_ERROR;
            }
            return ret;
        }

        private int MessegeCheck_GetInfo(string strRecMsg, string commandHead, ref string strRev)
        {
            int strLength = strRecMsg.Length;
            int ret = CONTINUE;     
            string[] splitRecMsg = strRecMsg.Split('\t', '\n', '*');
            if (IsCrc(strRecMsg))
            {
                if (splitRecMsg[0] == commandHead)
                {
                    if (splitRecMsg[1] == "OK" || splitRecMsg[1] == "YES")
                    {
                        ret = END_OK;
                        Regex reg = new Regex("OK\n([A-Za-z0-9-_]+)\t");
                        Match match = reg.Match(strRecMsg);
                        strRev = match.Groups[1].Value;
                    }
                    else if (splitRecMsg[1] == "ERROR" || splitRecMsg[1] == "NO")
                    {
                        ret = END_ERROR;
                    }
                    else if (splitRecMsg[1] == "BUSY")
                    {
                        ret = END_BUSY;
                    }
                    else
                    {
                        strRev = splitRecMsg[1];
                    }
                }
                else
                {
                    ret = END_ERROR;
                }
            }
            else
            {
                ret = END_ERROR;
            }
            return ret;
        }

        public int RecMsg_GetInfo(string commandHead, ref string strRev)
        {
            int ret = TRANS_OK;
            byte[] lastMsg = null;
            TCPCommand.IsDeviceBusy = false;
            do
            {
                byte[] arrRecMsg = new byte[50];
                mSocketClient.Receive(arrRecMsg);
                List<string> MessageBuffer = MessageAssembly(arrRecMsg, ref lastMsg);
                for (int i = 0; i < MessageBuffer.Count; i++)
                {
                    ret = MessegeCheck_GetInfo(MessageBuffer[i], commandHead, ref strRev);
                    if (ret == END_ERROR)
                    {
                        return TRANS_ERROR;
                    }
                    if (ret == END_OK)
                    {
                        return TRANS_OK;
                    }
                    if (ret == END_BUSY)
                    {
                        TCPCommand.IsDeviceBusy = true;
                        return TRANS_ERROR;
                    }
                }
            } while (true);
        }

        public void SendMsg(string sendMsg)
        {
            byte[] arrClientSendMsg = Encoding.Default.GetBytes(sendMsg);
            mSocketClient.Send(arrClientSendMsg);
        }


        public int Formatting(string IPAddr)
        {
            
            if (!TCPCommand.instance.Connect(IPAddress.Parse(IPAddr), TCPCommand.PORT, 20))
            {
                return 1;
            }
            m_strIPAddr = IPAddr;
            CommandsFromTools commandMsgs = TCPCommand.instance.FormatCommand(FileTransfer.TCPCommand.REQ_FORMAT);
            TCPCommand.instance.SendMsg(commandMsgs.RequestCommand);
            string FormattingInfo = null;
            int ret = TCPCommand.instance.RecMsg_GetInfo(TCPCommand.REQ_FORMAT, ref FormattingInfo);
            TCPCommand.instance.SendMsg(commandMsgs.ACKCommand);
            TCPCommand.instance.Disconnect();
            return ret;
        }

        public string GetTFFSVersion(string IPAddr)
        {
            if (!TCPCommand.instance.Connect(IPAddress.Parse(IPAddr), TCPCommand.PORT, 20))
            {
                return null;
            }
            m_strIPAddr = IPAddr;
            CommandsFromTools commandMsgs = TCPCommand.instance.FormatCommand(FileTransfer.TCPCommand.GET_TFFS_VER);
            TCPCommand.instance.SendMsg(commandMsgs.RequestCommand);
            string VersionInfo = null;
            int ret = TCPCommand.instance.RecMsg_GetInfo(TCPCommand.GET_TFFS_VER, ref VersionInfo);
            TCPCommand.instance.SendMsg(commandMsgs.ACKCommand);
            TCPCommand.instance.Disconnect();
            if(ret == TCPCommand.TRANS_OK)
                return VersionInfo;
            return null;
        }
        public bool UpdateBSP(string IPAddr)
        {
            CallBack("正在更新BSP...", -1);
            m_strIPAddr = IPAddr;
            if (!TCPCommand.instance.Connect(IPAddress.Parse(IPAddr), TCPCommand.PORT, 20))
            {
                return false;
            }
            CommandsFromTools commandMsgs = TCPCommand.instance.FormatCommand(FileTransfer.TCPCommand.UPDATE_BOOTROM);//.instance.FormatCommand(FileTransfer.TCPCommand.FILETYPE_BSP);
            TCPCommand.instance.SendMsg(commandMsgs.RequestCommand);
            string result = null;
            int ret = TCPCommand.instance.RecMsg_GetInfo(TCPCommand.UPDATE_BOOTROM, ref result);
            TCPCommand.instance.SendMsg(commandMsgs.ACKCommand);
            TCPCommand.instance.Disconnect();
            if (ret == TCPCommand.TRANS_OK)
            {
                CallBack("更新BSP成功", -1);
                return true; 
            }
            else
            {
                CallBack("更新BSP失败", -1);
                return false; 
            }
        }
        /// <summary>
        /// 重启设备
        /// </summary>
        /// <returns></returns>
        public int Restart(string IPAddr)
        {
            if (!TCPCommand.instance.Connect(IPAddress.Parse(IPAddr), TCPCommand.PORT, 20))
            {
                return END_ERROR;
            }
            CommandsFromTools commandMsgs = TCPCommand.instance.FormatCommand(TCPCommand.REQ_REBOOT);
            TCPCommand.instance.SendMsg(commandMsgs.RequestCommand);
            string RestartInfo = null;
            int ret = TCPCommand.instance.RecMsg_GetInfo(TCPCommand.REQ_REBOOT, ref RestartInfo);
            TCPCommand.instance.SendMsg(commandMsgs.ACKCommand);
            TCPCommand.instance.Disconnect();
            int count = 0;
            if (ret == FileTransfer.TCPCommand.TRANS_OK)
            {
                Thread.Sleep(6000);//发送重启命令后5秒重启，该处先等待6秒
                while (true)
                {
                    if (count++ > 100)
                        return END_ERROR;
                    SetIPAddr(IPAddr);
                    //string strValue = null;
                    //if (GetValueByOid(sysUpTimeOid, out strValue) == DLLConstInfo.RET_OK)
                    //{
                    //    //获取设备的启动时间，如果>0则认为设备已完成重启
                    //    if(int.Parse(strValue) > 0)
                    //    {
                    //        return DLLConstInfo.RET_OK;
                    //    }
                    //}
                    if (isOnLine)
                    {
                        return END_OK;
                    }
                    Thread.Sleep(500);
                }
            }
            else
            {
                return END_ERROR;
            }
        }

        public bool PingConnect(string ip)
        {
            if (ip.Equals("0.0.0.0"))
            {
                return false;
            }
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            int timeout = 10; // Timeout 时间，单位：毫秒
            System.Net.NetworkInformation.PingReply reply;
            try
            {
                reply = p.Send(ip, timeout);
            }
            catch
            {
                return false;
            }
            if (reply != null && reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 操作设备的IP地址
        /// </summary>
        /// <param name="strIPAddr">IP地址</param>
        public void SetIPAddr(string strIPAddr)
        {
            m_strIPAddr = strIPAddr;
            if (strIPAddr != null)
            {
                isOnLine = PingConnect(m_strIPAddr);
            }
        }

        public bool IsCrc(string strRecMsg)
        {
            int strLength = strRecMsg.Length;
            string[] splitRecMsg = strRecMsg.Split('\t');
            if(splitRecMsg.Length < 2)
            {
                return false;
            }
            ulong crc32Rec = Convert.ToUInt64(splitRecMsg[splitRecMsg.Length - 2]);//获取接收到的CRC码

            string strForCrc = "";
            for (int i = 0; i < splitRecMsg.Length - 2; i++)
            {
                strForCrc += splitRecMsg[i] + '\t';
            }
            ulong crc32 = CRC32.GetCRC32Str(strForCrc);//计算当前CRC码
            if (crc32 == crc32Rec)//判断CRC码是否匹配
                return true;
            else
                return false;
        }

    }
}
