/********************************************************************/
/*  file name    : DeviceUpDownloadAdapter.cs                       */
/*  function     : 文件上传下载类                                   */
/*  date/version : 2015/11/23/v1.0                                  */
/*  author       : liukui                                           */
/********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FileTransfer;
using InToolSet.Logging;
using InToolSet.ViewModel.NetworkConfiguration;
using ConfigUpdate;


namespace InToolSet.Sys.Device
{

    /// <summary>
    /// 使用FileUpDownloadDLL类，事项设备相关文件备份、更新相关机能的操作类。
    /// </summary>
    public class DeviceUpDownloadAdapter
    {
        public delegate void UpdateProgressBarDelegate(string ProgressInfo);
        /// <summary>
        /// 设定备份/更新文件的设备的IP地址
        /// </summary>
        public string m_strIPAddr;
        /// <summary>
        /// 文件类型定义
        /// </summary>
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

        public const int FILETYPE_HMI_FILE_SYSTEM = 10; // 显示器文件系统
        public const int FILETYPE_HMI_APP = 11; // 显示器APP文件
        public const int FILETYPE_HMI_DATA = 12; // 显示器DATA文件
        public const int FILETYPE_HMI_KERNEL = 13; // 显示器内核文件
        public const int FILETYPE_HMI_UBOOT = 14; // 显示器固件文件（UBOOT）
        public const int FILETYPE_HMI_DIAGFILE = 15; // 显示器log文件diagFile.txt
        public const int FILETYPE_HMI_DTB = 16; // 显示器设备树文件
        public const int FILETYPE_HMI_RAMDISK = 17; // 显示器拯救系统文件
        public const int FILETYPE_HMI_FREE_FILE1 = 18; // 显示器自由选择单个文件类型1
        public const int FILETYPE_HMI_FREE_FILE2 = 19; // 显示器自由选择单个文件类型2
       



        //public static TransferFileInfo global_file_info =  new TransferFileInfo();
        public static Dictionary<int, string> FILETYPE_DOWNLOAD_DIC = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,"更新固件"},
            {FILETYPE_APPLICATION,"更新应用程序"},
            {FILETYPE_FPGA,"更新FPGA"},
            {FILETYPE_CPLD,"更新CPLD"},
            {FILETYPE_CONFIG_TOPOLOGY,"更新拓扑配置"},
            {FILETYPE_DIAGNOSIS,"更新诊断信息"},
            {FILETYPE_CONFIG_DEVICE,"更新设备配置"},
            {FILETYPE_CONFIG_MVB,"更新MVB通信配置"},
            {FILETYPE_CONFIG_TRDP,"更新TRDP通信配置"},
            {FILETYPE_OTHER,"更新其他文件"},
            {FILETYPE_HMI_FILE_SYSTEM,"更新显示器固件"},
            {FILETYPE_HMI_APP,"更新显示器APP"},
            {FILETYPE_HMI_DATA,"更新显示器DATA"},
            {FILETYPE_HMI_KERNEL,"更新显示器内核"},
            {FILETYPE_HMI_UBOOT,"更新显示器文件系统"},
            {FILETYPE_HMI_DTB,"更新显示器设备树文件"},
            {FILETYPE_HMI_RAMDISK,"更新显示器拯救系统文件"},
            {FILETYPE_HMI_FREE_FILE1,"更新自选单个文件1"},
            {FILETYPE_HMI_FREE_FILE2,"更新自选单个文件2"},
            {FILETYPE_BSP,"更新BSP文件"}
        };

        public static Dictionary<int, string> FILETYPE_UPDATE_DIC = new Dictionary<int, string>
        {
            {FILETYPE_FIRMWARE,"获取固件"},
            {FILETYPE_APPLICATION,"获取应用程序"},
            {FILETYPE_FPGA,"获取FPGA"},
            {FILETYPE_CPLD,"获取CPLD"},
            {FILETYPE_CONFIG_TOPOLOGY,"获取拓扑配置"},
            {FILETYPE_DIAGNOSIS,"获取诊断信息"},
            {FILETYPE_CONFIG_DEVICE,"获取设备配置"},
            {FILETYPE_CONFIG_MVB,"获取MVB通信配置"},
            {FILETYPE_CONFIG_TRDP,"获取TRDP通信配置"},
            {FILETYPE_OTHER,"获取其他文件"},
            {FILETYPE_HMI_FILE_SYSTEM,"获取显示器文件系统"},
            {FILETYPE_HMI_APP,"获取显示器APP"},
            {FILETYPE_HMI_DATA,"获取显示器DATA"},
            {FILETYPE_HMI_KERNEL,"获取显示器内核"},
            {FILETYPE_HMI_UBOOT,"获取显示器固件"},
            {FILETYPE_HMI_DIAGFILE,"获取显示器log文件diagFile.txt"},
            {FILETYPE_HMI_DTB,"获取显示器设备树文件"},
            {FILETYPE_HMI_RAMDISK,"获取显示器拯救系统文件"},
            
        
        };
        
        /// <summary>
        /// 设定备份/更新文件的设备的IP地址
        /// </summary>
        /// <param name="strIpAdress"></param>
        public DeviceUpDownloadAdapter(string strIpAdress)
        {
            this.m_strIPAddr = strIpAdress;
        }

        /// <summary>
        /// 从设备获取文件
        /// </summary>
        /// <returns>是否成功</returns>
        public bool UploadFromDevice(string strPath, int fileType, int iSlotIndex)
        {
            FileTransferHelper fileHelper = new FileTransferHelper();
            //if (NetworkConfigurationModel.GetNetworkConfigurationModel().SelectedDeviceModel != null)
            //{
            //    fileHelper.registerCallBack(UpdateProgressBarInfomation, NetworkConfigurationModel.GetNetworkConfigurationModel().SelectedDeviceModel.DeviceName);
            //    SelectedDeviceName = NetworkConfigurationModel.GetNetworkConfigurationModel().SelectedDeviceModel.DeviceName;
            //}
            fileHelper.SetIPAddr(m_strIPAddr);
            int iRet = FileTransferInfo.RET_OK;


            if (iSlotIndex == -1)
            {
                iRet = fileHelper.UploadFromDevice(strPath, fileType);
            }
            else
            {
                iRet = fileHelper.UploadFromDevice(strPath, fileType, iSlotIndex);
            }

            
            if (FileTransferInfo.RET_OK == iRet)
            {
                return true;
            }
            else
            {
                Log.Error(string.Format("fileHelper.UploadFromDevice error. filepath={0}", strPath));
                return false;
            }
        }


        /// <summary>
        /// 更新文件到设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool DownloadToDevice(string strPath, int fileType, int iSlotIndex,string updateMode)
        {
            FileTransferHelper fileHelper = new FileTransferHelper();
            fileHelper.SetIPAddr(m_strIPAddr);

            int iRet = FileTransferInfo.RET_OK;

            if (iSlotIndex == -1)
            {
                iRet = fileHelper.DownloadToDevice(strPath, fileType);
            }
            else
            {
                iRet = fileHelper.DownloadToDevice(strPath, fileType, iSlotIndex, updateMode);
            }
            
            if (FileTransferInfo.RET_OK == iRet)
            {
                return true;
            }
            else
            {
                Log.Error(string.Format("fileHelper.DownloadToDevice error. filepath={0}", strPath));
                return false;
            }
        }

        public void UpdateProgressBarInfomation(string ProgressBarInfomation)
        {
            LoadFileProgressModel.Instance.Information = ProgressBarInfomation;

        }


    }

    /// <summary>
    /// 传输文件信息定义
    /// </summary>
    public class TransferFileInfo
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string  m_ipaddr;

        /// <summary>
        /// 文件路径列表
        /// </summary>
        public string m_strFilePath;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long m_fileLength = 0 ;

        /// <summary>
        /// 文件类型
        /// </summary>
        public int m_iFileType;
       
        /// <summary>
        /// 上传下载标志
        /// </summary>
        public bool m_bDownloadToDevice;

        /// <summary>
        /// 文件传输完了的回调函数类型
        /// </summary>
        public delegate void FileTranserFinishCallBackFunc(object param);

        /// <summary>
        /// 文件传输完了的回调函数定义
        /// </summary>
        public FileTranserFinishCallBackFunc FinishCallBack;

        /// <summary>
        /// 回调函数的参数
        /// </summary>
        public object FinishCallBackParam;

        /// <summary>
        /// 插槽序号
        /// </summary>
        public int m_iSlotIndex = -1;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public TransferFileInfo()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deviceModel">设备模型</param>
        /// <param name="strFilePath">文件路径</param>
        /// <param name="iFileType">文件类型</param>
        public TransferFileInfo(string IP, string strFilePath, int iFileType)
        {
            m_strFilePath = strFilePath;
            m_ipaddr = IP;
            m_iFileType = iFileType;
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get { return m_strFilePath; }
            set { m_strFilePath = value; }
        }


        /// <summary>
        /// 文件类型
        /// </summary>
        public int FileType
        {
            get { return m_iFileType; }
            set { m_iFileType = value; }
        }


        /// <summary>
        /// 插槽序号
        /// </summary>
        public int SlotIndex
        {
            get { return m_iSlotIndex; }
            set { m_iSlotIndex = value; }
        }

        /// <summary>
        /// 更新模式
        /// </summary>
        public string UpdateModeStr;
    }

    /// <summary>
    /// 用于线程的文件传输信息
    /// </summary>
    public class FileTransferInfoForThread
    {
        /// <summary>
        /// 上传下载标志
        /// </summary>
        private bool m_bDownloadToDevice;

        /// <summary>
        /// 线程操作的设备
        /// </summary>
        private IDeviceModel m_deviceModel;

        /// <summary>
        /// 文件传输信息列表
        /// </summary>
        private List<TransferFileInfo> m_infoList = new List<TransferFileInfo>();

        /// <summary>
        /// 线程操作的设备
        /// </summary>
        public IDeviceModel DeviceModel
        {
            get { return m_deviceModel; }
            set { m_deviceModel = value; }
        }

        /// <summary>
        /// 文件传输信息列表
        /// </summary>
        public List<TransferFileInfo> FileInfoList
        {
            get { return m_infoList; }
        }

        /// <summary>
        /// 上传下载标志
        /// </summary>
        public bool DownLoadToDevice
        {
            get { return m_bDownloadToDevice; }
            set { m_bDownloadToDevice = value; }
        }
    }

    /// <summary>
    /// 文件传输管理类
    /// </summary>
    public class FileTransferMgr
    {
        /// <summary>
        /// 传输文件列表
        /// </summary>
        public Thread m_transferThreadMgr = null;
        private Dictionary<IDeviceModel, FileTransferInfoForThread> m_transferFileList = new Dictionary<IDeviceModel, FileTransferInfoForThread>();
        private Dictionary<IDeviceModel, Thread> m_transferFileThreadList = new Dictionary<IDeviceModel, Thread>();
        public static FileTransferMgr m_instance = new FileTransferMgr();
        public LoadFileProgressModel m_progressModel = LoadFileProgressModel.Instance; // 已完成进度为0
        public readonly string DefaultDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);



        /// <summary>
        /// 状态返回值定义
        /// </summary>
        public const int FINISH_NO = 0;
        public const int FINISH_OK = 1;
        public const int FINISH_ERROR = 2;
        public const int END_OK = 0;
        public const int END_ERROR = 1;
        public const int END_BUSY = 2;
        public const int CONTINUE = 3;

        public static bool TransResult = true;
        /// <summary>
        /// 同时启动的传输线程最大数
        /// </summary>
        public const int THREAD_MAXCNT = 10;

        /// <summary>
        /// 获取唯一实例
        /// </summary>
        public static FileTransferMgr Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// 文件传输处理
        /// </summary>
        /// <param name="fileList">文件列表</param>
        /// <param name="bDownloadToDevice">备份/更新标志</param>
        /// <returns>是否成功</returns>
        public bool FileTransferHandle(TransferFileInfo fileInfo, bool bDownloadToDevice)
        {
            TransResult = true;
            if (IsTransfering)
            {
                //正在传输中
                return false;
            }
            //文件列表转换成上传线程用的文件列表
            //m_transferFileList.Clear();
            int iFileCnt = 0;
            // 进度条显示
            m_progressModel.ItemsTotal = iFileCnt;
            m_progressModel.ItemsCompleted = 1;
            m_progressModel.IsIndeterminate = (1 == iFileCnt);
            UpdateProgressBarInfomation();
            //m_transferFileThreadList.Clear();
            // 使用线程开始上传下载
            m_transferThreadMgr = new Thread(FileTransferThreadMgr);
            m_transferThreadMgr.Start(fileInfo);
            return true;
        }

        /// <summary>
        /// 下载到指定设备
        /// </summary>
        /// <param name="fileList">文件列表</param>
        /// <returns>是否成功</returns>
        public bool DownloadToDevice(TransferFileInfo fileInfo)
        {
            return FileTransferHandle(fileInfo, true);
        }

        /// <summary>
        /// 从指定设备获取文件
        /// </summary>
        /// <param name="fileList">文件列表</param>
        /// <returns>是否成功</returns>
        public bool UploadFromDevice(TransferFileInfo fileInfo)
        {
            return FileTransferHandle(fileInfo, false);
        }

        /// <summary>
        /// 文件传输线程管理线程
        /// </summary>
        public void FileTransferThreadMgr(object fileInfo)
        {
            TransferFileInfo m_fileInfo = fileInfo as TransferFileInfo;
            FileTransfer.FTPServerHelper.Instance.startServer();
            Console.WriteLine("transfer start");
            List<IDeviceModel> transferedDeviceList = new List<IDeviceModel>();
            List<IDeviceModel> needTransferDeviceList = m_transferFileList.Keys.ToList();

            Thread transferThread = new Thread(FileTransferThread);
            transferThread.Start(m_fileInfo);

            // 传输Flag
            bool bLoopFlag = false;
            do
            {
                bLoopFlag = false;

                if (transferThread.IsAlive)
                {
                    //当存在活动线程时，循环继续
                    Thread.Sleep(1000);
                    bLoopFlag = true;
                }
            } while (bLoopFlag);


            //所有传输线程都传输完了
            Console.WriteLine("transfer end");
            FileTransfer.FTPServerHelper.Instance.stopServer();
            if (FileTransferMgr.TransResult)
            {
                NetConnection.Instance.TransferStatus = FINISH_OK;
                ConfigUpdate.MainWindow.IsUpdateNow = false;
                TCPCommand.GetInstance().CallBack("", -1);
                string strIP = m_fileInfo.m_ipaddr.Replace(" ", "");
                IsRestartNow(strIP);
            }

            else
            {
                NetConnection.Instance.TransferStatus = FINISH_ERROR;
                if (TCPCommand.IsDeviceBusy)
                    System.Windows.Forms.MessageBox.Show("设备繁忙！ 请尝试重启设备！", "结果", System.Windows.Forms.MessageBoxButtons.OK);
                else
                    System.Windows.Forms.MessageBox.Show("更新失败！ 若网络配置和连接正常，请尝试格式化后更新！", "结果", System.Windows.Forms.MessageBoxButtons.OK);
                FileTransferMgr.TransResult = true;
                ConfigUpdate.MainWindow.IsUpdateNow = false;
                TCPCommand.GetInstance().CallBack("", -1);
            }
            
        }
        /// <summary>
        /// 更新完成时询问是否现在重启
        /// </summary>
        public void IsRestartNow(string ipaddr) 
        {
            DialogResult dr;
            dr = MessageBox.Show("已更新成功，请问是否现在重启设备？", "已完成", MessageBoxButtons.YesNoCancel,
                     MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.Yes)
            {
                //TCPCommand.GetInstance().CallBack("正在重启中...", -1);
                Thread restartThread = new Thread(Thread_Restart);
                restartThread.Start(ipaddr);
            }
            else if (dr == DialogResult.No)
                ;
            else if (dr == DialogResult.Cancel)
                ;
        }
        public void Thread_Restart(Object obj)
        {
            string ip = obj as string;
            if (END_OK == TCPCommand.GetInstance().Restart(ip))
            {
                MessageBox.Show("重启成功！");
            }
            else
            {
                MessageBox.Show("重启失败！");
            }
            //TCPCommand.GetInstance().CallBack("", -1);
        }
        /// <summary>
        /// 文件传输线程
        /// </summary>
        /// <param name="fileInfo">用于线程的文件传输信息</param>
        public void FileTransferThread(object fileInfo)
        {
            TransferFileInfo transInfo = fileInfo as TransferFileInfo;
            TransferFileInfo.FileTranserFinishCallBackFunc callBackFunc = null;
            object objParam = null;

            //Dictionary<IDeviceModel, DeviceUpdateStatus> transferResult = new Dictionary<IDeviceModel, DeviceUpdateStatus>();
            string strIP = transInfo.m_ipaddr;
            strIP = strIP.Replace(" ", "");
           
            if(string.IsNullOrEmpty(strIP))
            {
                
            }


            if (!string.IsNullOrEmpty(strIP))
            {
                //transInfo.DeviceModel.UpdateStatus = DeviceUpdateStatus.Updating;
                // IP地址存在时，开始文件传输
                DeviceUpDownloadAdapter adapter = new DeviceUpDownloadAdapter(strIP);
                //DeviceUpdateStatus updateSts = DeviceUpdateStatus.UpdateOK;

                //UpdateProgressBarInfomation(transInfo.m_bDownloadToDevice, info);
                if (transInfo.m_bDownloadToDevice) // 更新
                {
                    if (!adapter.DownloadToDevice(transInfo.FilePath, transInfo.FileType, transInfo.SlotIndex,transInfo.UpdateModeStr))
                    {
                        TransResult = false;
                        //存在传输失败的文件时，状态设置成NG
                        //updateSts = DeviceUpdateStatus.UpdateNG;
                    }
                }
                else // 备份
                {
                    if (!adapter.UploadFromDevice(transInfo.FilePath, transInfo.FileType, transInfo.SlotIndex))
                    {
                        //存在传输失败的文件时，状态设置成NG
                        //updateSts = DeviceUpdateStatus.UpdateNG;
                    }
                }
                lock (m_progressModel)
                {
                    // 更新设备配置，备份后开启更新
                    m_progressModel.ItemsCompleted++;
                }
                //UpdateProgressBarInfomation(transInfo.m_bDownloadToDevice, info);

                //回调函数保存
                if (transInfo.FinishCallBack != null)
                {
                    callBackFunc = transInfo.FinishCallBack;
                    objParam = transInfo.FinishCallBackParam;
                }

                //transInfo.DeviceModel.UpdateStatus = updateSts;
            }
            else
            {
                // 备份更新失败
                TransResult = false;
                Log.Error(string.Format("FileTransferThread error (ip empty). device={0}", "更新备份失败"));
                UpdateProgressBarInfomation();
                //transInfo.DeviceModel.UpdateStatus = DeviceUpdateStatus.UpdateNG;
            }

        }

        /// <summary>
        /// 是否传输中
        /// </summary>
        public bool IsTransfering
        {
            get
            {
                if (null != m_transferThreadMgr && m_transferThreadMgr.IsAlive)
                {
                    //正在传输中
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 获取进度条信息
        /// </summary>
        public void UpdateProgressBarInfomation()
        {
            string strTmpInfo = "变量strTmpInfo";
            lock (m_progressModel)
            {
                int iFileCnt = Convert.ToInt32(m_progressModel.ItemsTotal);
                int iCompFileCnt = Convert.ToInt32(m_progressModel.ItemsCompleted);
                m_progressModel.Information = string.Format("{0}({1}/{2})", strTmpInfo, iCompFileCnt, iFileCnt);
            }
        }
        /// <summary>
        /// 处理结束
        /// </summary>
        public void Stop()
        {
            if (m_transferThreadMgr != null)
            {
                m_transferThreadMgr.Abort();
                m_transferThreadMgr.Join();
            }
        }
    }
}
