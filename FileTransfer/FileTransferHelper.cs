using System;
using System.Windows ;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using InToolSet.Sys.Device;
using InToolSet.Sys;
using Microsoft.Win32;
namespace FileTransfer
{
    /// <summary>
    /// 该模块的对外接口类

    /// </summary>
    /// 
    /// 
  
    public class FileTransferHelper
    {
        /// <summary>
        /// 文件类型定义
        /// </summary>
        public const int FILETYPE_HMI_FILE_SYSTEM = 10; // 显示器文件系统
        public const int FILETYPE_HMI_APP = 11; // 显示器APP文件
        public const int FILETYPE_HMI_DATA = 12; // 显示器DATA文件
        public const int FILETYPE_HMI_KERNEL = 13; // 显示器内核文件
        public const int FILETYPE_HMI_UBOOT = 14; // 显示器固件文件（UBOOT）
        public const int FILETYPE_HMI_DIAGFILE = 15; //显示器log文件diagFile.txt
        public const int FILETYPE_HMI_DTB = 16; //显示器设备树文件
        public const int FILETYPE_HMI_RAMDISK = 17; //显示器拯救系统文件
        public const int FILETYPE_HMI_FREE_FILE1 = 18;//显示器自由选择单个文件类型1
        public const int FILETYPE_HMI_FREE_FILE2 = 19;//显示器自由选择单个文件类型2

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

        private string m_strIPAddr;
        public static Object copyLock = 1;
        private bool isOnLine = false;
        public const bool DOWNLOAD = true;
        public const bool UPLOAD = false;
        //public delegate void UpdateProgressBarDelegate(string ProgressInfo);
        //public UpdateProgressBarDelegate ProgressBarCallBack;
        //public string UpdateDeviceName = "";
        
 
        
        //public void registerCallBack(UpdateProgressBarDelegate DelegateProgressBar)
        //{
        //    ProgressBarCallBack = DelegateProgressBar;
        //}
        //public void CallBack(string UpdateProgressBarInfo)
        //{
        //    ProgressBarCallBack(UpdateProgressBarInfo);
        //}
        /// <summary>
        /// 设定备份/更新文件的设备的IP地址
        /// </summary>
        /// <param name="strIP">设备IP地址</param>
        public void SetIPAddr(string strIP)
        {
            m_strIPAddr = strIP;
            isOnLine = Ping(m_strIPAddr);
        }

        /// <summary>
        /// 从设备获取文件

        /// </summary>
        /// <param name="strPath">文件保存路径</param>
        /// <param name="iFileType">文件类型</param>
        /// <returns>0：成功，0以外：失败</returns>
        public int UploadFromDevice(string strPath, int iFileType)
        {
            if (!isOnLine)
                return FileTransferInfo.RET_NG;
            string srcPath;
            string desPath;
            string srcFileName = null;
            if(TCPCommand.FILENAME_DIC.ContainsKey(iFileType))
            {
                srcFileName = TCPCommand.FILENAME_DIC[iFileType];
            }

            if (System.IO.Path.GetExtension(strPath) != "")
            {
                srcFileName = System.IO.Path.GetFileName(strPath);
                strPath = System.IO.Path.GetDirectoryName(strPath);
            }

            TCPCommand tcpCommand = TCPCommand.GetInstance();
            if(!tcpCommand.Connect(IPAddress.Parse(m_strIPAddr), TCPCommand.PORT,20))
            {
                return FileTransferInfo.RET_NG;
            }
            CommandsFromTools commandMsgs = tcpCommand.FormatCommand(TCPCommand.ACTION_BACKUP, srcFileName, iFileType, -1);
            tcpCommand.SendMsg(commandMsgs.RequestCommand);
            //tcpCommand.RegisterRecMsgHandler(TCPCommand.MESSAGEHANDLER_DIC[iFileType]);
            string transFileName = null;
            int ret = tcpCommand.RecMsg_FileTrans(TCPCommand.BACKUP_COMMAND_DIC[iFileType],ref transFileName);
            tcpCommand.SendMsg(commandMsgs.ACKCommand);
            tcpCommand.Disconnect();
            if (ret == TCPCommand.TRANS_ERROR || ret == TCPCommand.TRANS_BUSY)
            {
                return FileTransferInfo.RET_NG;
            }
            if (transFileName != null)
            {
                srcFileName = transFileName;
            }
            srcPath = FTPServerHelper.getTempPath() + srcFileName;
            desPath = strPath + "\\" + srcFileName;
            
            System.IO.File.Copy(srcPath, desPath, true);
            return FileTransferInfo.RET_OK;
        }



        /// <summary>
        /// 更新文件到设备

        /// </summary>
        /// <param name="strPath">文件所在路径</param>
        /// <param name="iFileType">文件类型</param>
        /// <returns></returns>
        public int DownloadToDevice(string strPath, int FileType)
        {
            //若更新类型为BSP则更新固件文件
            int iFileType;
            if (FILETYPE_BSP == FileType)
                iFileType = FILETYPE_FIRMWARE;
            else
                iFileType = FileType;

            if (!isOnLine)
                return FileTransferInfo.RET_NG;
            //iFileType = TCPCommand.FILETYPE_CONFIG_TRDP;
            string fileName = System.IO.Path.GetFileName(strPath);
            //FileInfo fi = new FileInfo(strPath);
            //if(TCPCommand.FILENAME_DIC.ContainsKey(iFileType))
            //{
            //    fileName = TCPCommand.FILENAME_DIC[iFileType];
            //}

            string desPath = FTPServerHelper.getTempPath() + fileName;
            this.CopyToFTPDirectory(strPath, desPath, true);
            TCPCommand tcpCommand = TCPCommand.GetInstance();
            if (!tcpCommand.Connect(IPAddress.Parse(m_strIPAddr), TCPCommand.PORT, 20))
            {
                return FileTransferInfo.RET_NG;
            }
            CommandsFromTools commandMsgs = tcpCommand.FormatCommand(TCPCommand.ACTION_UPDATE, fileName, iFileType, -1);
            tcpCommand.SendMsg(commandMsgs.RequestCommand);
            int ret = tcpCommand.RecMsg_FileTrans(TCPCommand.UPDATE_COMMAND_DIC[iFileType]);
            tcpCommand.SendMsg(commandMsgs.ACKCommand);
            tcpCommand.Disconnect();
            if (ret == TCPCommand.TRANS_ERROR || ret == TCPCommand.TRANS_BUSY)
            {
                return FileTransferInfo.RET_NG;
            }

            if (FILETYPE_BSP == FileType)
            {
                if (TCPCommand.GetInstance().UpdateBSP(m_strIPAddr))
                    System.Windows.Forms.MessageBox.Show("BSP更新成功！");
                else
                    System.Windows.Forms.MessageBox.Show("BSP更新失败！");
            }
            return FileTransferInfo.RET_OK;
        }

        public int DownloadToDevice(string strPath, int iFileType, int iSlotIndex, string updateMode)
        {
            int ret = -1;

            try
            {
                switch (iFileType)
                {
                    case TCPCommand.FILETYPE_FPGA:
                        ret = DownloadToDeviceViaPCI(strPath, iFileType, iSlotIndex);
                        break;
                    default:
                        if ("PCI更新" == updateMode)
                            ret = DownloadToDeviceViaPCI(strPath, iFileType, iSlotIndex);
                        else if ("以太网更新" == updateMode)
                            ret = DownloadToDevice(strPath, iFileType);
                        break;
                }
            }
            catch
            {
                TCPCommand.GetInstance().CallBack("文件类型格式报错，请确认文件类型/更新方式/槽位号已配置正确！", -1);
            }
 
            return ret;
        }

        private int DownloadToDeviceViaPCI(string strPath, int iFileType, int iSlotIndex)
        {
            if (!isOnLine)
                return FileTransferInfo.RET_NG;
            string fileName = System.IO.Path.GetFileName(strPath);
            //if (TCPCommand.FILENAME_DIC.ContainsKey(iFileType))
            //{
            //    fileName = TCPCommand.FILENAME_DIC[iFileType];
            //}

            string desPath = FTPServerHelper.getTempPath() + fileName;
            this.CopyToFTPDirectory(strPath, desPath, true);
            TCPCommand tcpCommand = TCPCommand.GetInstance();
            if (!tcpCommand.Connect(IPAddress.Parse(m_strIPAddr), TCPCommand.PORT_PCI, 20))
            {
                return FileTransferInfo.RET_NG;
            }
            CommandsFromTools commandMsgs = tcpCommand.FormatCommand(TCPCommand.ACTION_UPDATE, fileName, iFileType, iSlotIndex);
            tcpCommand.SendMsg(commandMsgs.RequestCommand);
            int ret = tcpCommand.RecMsg_FileTrans(TCPCommand.UPDATE_COMMAND_DIC_PCI[iFileType]);
            tcpCommand.SendMsg(commandMsgs.ACKCommand);
            tcpCommand.Disconnect();
            if (ret == TCPCommand.TRANS_ERROR || ret == TCPCommand.TRANS_BUSY)
            {
                return FileTransferInfo.RET_NG;
            }
            return FileTransferInfo.RET_OK;
        }

        public int UploadFromDevice(string strPath, int iFileType, int iSlotIndex)
        {
            Thread.Sleep(2000);
            return FileTransferInfo.RET_OK;
        }

        private void CopyToFTPDirectory(string srcPath, string desPath, bool toRewrite)
        {
            lock (copyLock)
            {
                string strPath = Path.GetDirectoryName(desPath);
                if(!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
                System.IO.File.Copy(srcPath, desPath, toRewrite);
            }
        }

        public delegate void TransferEvent();

        public bool Ping(string ip)
        {
            if (ip.Equals("0.0.0.0"))
            {
                return false;
            }
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            int timeout = 20; // Timeout 时间，单位：毫秒
            System.Net.NetworkInformation.PingReply reply = p.Send(ip);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                return true;
            else
                return false;
        }
       
    }
}
