using System;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileTransfer;
using InToolSet.Sys.Device;
using InToolSet.ViewModel.NetworkConfiguration;
using Scanner;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
namespace ConfigUpdate
{
    public struct filetype
    {
        public int id{get;set;}
        public string text{get;set;}
    }
    //生成事件脚本
//    set LIBZ=$(SolutionDir)libz-1.1.0.2-tool\tool\libz.exe
//%LIBZ% inject-dll --assembly ConfigUpdate.exe --include *.dll --move

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public class NetConnection : INotifyPropertyChanged
    {
        /// <summary>
        /// 单例化
        /// </summary>
        private static NetConnection m_instance = new NetConnection();


        /// <summary>
        /// 传输/连接状态声明
        /// </summary>
        public int ConnectStatus = 0;
        public int TransferStatus = 0;
        public int IsWorkSuccess = TCPCommand.CONTINUE;
        public bool IsWorkRunning = false;

        


        /// <summary>
        /// 离线配置一致性检查管理实例
        /// </summary>
        public static NetConnection Instance
        {
            get
            {
                return m_instance;
            }
        }

        /// <summary>
        /// 传输获取结果内容
        /// </summary>


        public string m_CurrentIPAddr = "0.0.0.0";
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
        public string GetVersionResult;



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public partial class MainWindow : Window
    {
        public List<filetype> m_typeList = new List<filetype>();
        public List<string> name_list = new List<string> { "固件程序", "FPGA程序", "配置文件", "应用程序" };
        
        /// <summary>
        /// 单例对象
        /// </summary>
        private static MainWindow s_instance;
        public static int count = 0;
        public static int cnt = 0;
        public static int countFileTransfer = 0;
        /// <summary>
        /// 状态返回值定义
        /// </summary>
        public const int FINISH_NO = 0;
        public const int FINISH_OK = 1;
        public const int FINISH_ERROR = 2;

        /// <summary>
        /// 委托声明
        /// </summary>
        public delegate void MyInvokeProgress(string str,System.Windows.Visibility m_visible,double progressValue);
        public delegate void MyInvoke(string str, System.Windows.Visibility m_visible);
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

        public static string GET_DEV_TYPE = "GET_DEV_TYPE";//获取底层软件版本

        public static bool IsUpdateNow;
        public static string fileTypeStr;
        private OptionFormModel m_OptionFormModel;
        public TransferFileInfo file_info;
        public Thread threadProgressBar;
        public double percentValue = 0;
        public static string CfgFile;
        public static FileStream CfgFs;
        public MainWindow()
        {
            s_instance = this; 
            InitializeComponent();
            //待更新设备类型选择的相关控件初始化
            TextBoxSlotNum.Visibility = Visibility.Collapsed;
            LabelSlotNum.Visibility = Visibility.Collapsed;
            UpdateModeLabel.Visibility = Visibility.Hidden;
            UpdateMode.Visibility = Visibility.Hidden;
            UpdateMode.SelectedIndex = 0;
            PluginType.Visibility = Visibility.Collapsed;
            PluginType.SelectedIndex = 0;
            PluginTypeLabel.Visibility = Visibility.Collapsed;
            TextBoxSlotNum.IsEnabled = false;
            PluginType.IsEnabled = false;
            RadioButtonModule.IsChecked = true;
            IsUpdateNow = false;
            this.m_ProgressBar.Visibility = System.Windows.Visibility.Hidden;
            m_OptionFormModel = new OptionFormModel();
            //this.addressText.DataContext = NetConnection.Instance;
            this.DataContext = m_OptionFormModel;
            this.combo.Items.Add("固件程序");
            //this.combo.Items.Add("FPGA程序");
            this.combo.Items.Add("配置文件");
            //this.combo.Items.Add("应用程序");
            this.combo.Items.Add("固件+BSP一起更新");
            //添加更新模式COMBOBOX选项
            this.UpdateMode.Items.Add("以太网更新");
            this.UpdateMode.Items.Add("PCI更新");
            //添加插件设备COMBOBOX选项
            this.PluginType.Items.Add("PU300");
            this.PluginType.Items.Add("MB300");
            this.PluginType.Items.Add("CN300");
            this.PluginType.Items.Add("BN300");
            this.PluginType.Items.Add("IO300");
            this.PluginType.Items.Add("IO302");
            this.PluginType.Items.Add("IO310");
            this.PluginType.Items.Add("IO312");
            this.PluginType.Items.Add("IO330");
            this.PluginType.Items.Add("DR300");
            this.PluginType.Items.Add("WL310");
            this.PluginType.Items.Add("WL320");
            this.PluginType.Items.Add("SC310");
            this.PluginType.Items.Add("SC320");

            //添加模块设备COMBOBOX选项
            this.ModuleType.Items.Add("EDRM");
            this.ModuleType.Items.Add("ECNN");
            this.ModuleType.Items.Add("EGWM");
            this.ModuleType.Items.Add("EVVM");
            this.ModuleType.Items.Add("EVVM-C");
            this.ModuleType.Items.Add("EVVM-H");
            this.ModuleType.Items.Add("EVVM-M");
            this.ModuleType.Items.Add("EVVM-R");
            this.ModuleType.Items.Add("EWLM");
            //获取配置文件信息（存有历史访问路径）
            
            string SysDirectory = System.Environment.SystemDirectory;
            CfgFile = SysDirectory.Substring(0, 3);
            CfgFile = CfgFile + "ConfigUpdateSetting.txt";
            if (!File.Exists(CfgFile))
            {
                CfgFs = new FileStream(CfgFile, FileMode.Create);
                CfgFs.Close();
            }
            //获取历史IP信息
            string HistoryIPFile = SysDirectory.Substring(0, 3);
            HistoryIPFile = HistoryIPFile + "ConfigUpdateIP.txt";
            if (System.IO.File.Exists(HistoryIPFile))
            {

                StreamReader sr = new StreamReader(HistoryIPFile, Encoding.Default);
                String line;
                line = sr.ReadLine();
                m_OptionFormModel.CurrentIPStr = line;
                //关闭流
                sr.Close();
            }

            //判断WpCap的库是否存在
            WpCapExist();
            //判断Winpcap驱动是否安装
            WinPcapDriverExist();
            
            //获取版本号
            string VersionStr = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Title = "EDRM专版   版本" + VersionStr.Substring(0,5);
            //combo.ItemsSource = m_typeList;
            combo.SelectedIndex = 0;

            fileText.Text = GetPathRecord(CfgFile);
        }


        /// <summary>
        /// 取得主界面的单例
        /// </summary>
        public static MainWindow Instance
        {
            get { return s_instance; }
            private set { s_instance = value; }
        }
        /// <summary>
        /// WPCAP环境判断
        /// </summary>
        public void WpCapExist()
        {
            //获取系统盘盘符，防止C盘不是系统盘的情况
            string SysDirectory = System.Environment.SystemDirectory;
            string WpcapPath = SysDirectory.Substring(0, 3);
            WpcapPath = WpcapPath +"Windows\\System32\\wpcap_crrc.dll";
            if (File.Exists(WpcapPath))
                ;
            else
            {
                //释放资源文件wpcap_crrc.dll
                byte[] wpcap_crrc = (byte[])Properties.Resources.ResourceManager.GetObject("wpcap_crrc");
                File.WriteAllBytes(WpcapPath, wpcap_crrc);
                //MessageBox.Show("检测到路径C:\\Windows\\System32下找不到wpcap_crrc.dll，已为您添加！");

            }
        }
        /// <summary>
        /// 判断wpcap_crrc.dll 是否存在
        /// </summary>
        public void WinPcapDriverExist()
        {
            //判断Winpcap驱动是否安装
            string SysDirectory = System.Environment.SystemDirectory;
            string InstallDateFile = SysDirectory.Substring(0, 3);
            string InstallDateFileX86 = SysDirectory.Substring(0, 3);
            InstallDateFile = InstallDateFile + "Program Files";
            InstallDateFileX86 = InstallDateFileX86 + "Program Files (x86)";
            if ((!System.IO.File.Exists(InstallDateFile + "\\WinPcap\\rpcapd.exe")) && (!System.IO.File.Exists(InstallDateFileX86 + "\\WinPcap\\rpcapd.exe")))
            {
                //释放资源文件WinPcap_4_1_3.exe
                byte[] WinPcapFile = (byte[])Properties.Resources.ResourceManager.GetObject("WinPcap_4_1_3");
                File.WriteAllBytes("WinPcap_4_1_3.exe", WinPcapFile);

                MessageBox.Show("检测到您未安装WinPcap驱动，马上开始为您安装!");
                string path = @"WinPcap_4_1_3.exe";//这个path就是你要调用的exe程序的绝对路径
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "WinPcap_4_1_3.exe";
                process.StartInfo.WorkingDirectory = path;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                //MessageBox.Show("当前目录下未找到WinPcap_4_1_3.exe，无法安装WinPcap");

            }
        }
        /// <summary>
        /// 更新按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpDate_Click(object sender, RoutedEventArgs e)
        {
            //记录当前打开的文件路径到配置文件
            string[] path = fileText.Text.Split('\\');
            StreamWriter sw = new StreamWriter(CfgFile);
            string wr = fileText.Text.Substring(0, fileText.Text.Length - path[path.Length - 1].Length);
            sw.Write(wr);
            sw.Close();

            //清空ProgressBar的title
            this.AutoGetStatus.Content = "";

            //registerCallBack
            TCPCommand.GetInstance().registerCallBack(UpdateProgressBarInfomation);

            //如果选择更新固件或者FPGA，需进行文件内容检查，确定该文件对应目标设备类型
            if (0 == combo.SelectedIndex || 2 == combo.SelectedIndex)
            {
                if (!BinaryFileCheck(this.fileText.Text,"EDRM"))
                {
                    return;
                }
            }
            System.IO.FileInfo UpdateFileInfo = new System.IO.FileInfo(fileText.Text);
            if(false == IsUpdateNow)
            {
                file_info = new TransferFileInfo();
                try
                {
                    file_info.m_fileLength = UpdateFileInfo.Length;
                    file_info.m_ipaddr = addressText.Text;
                    file_info.UpdateModeStr = this.UpdateMode.Text;
                }
                catch
                {
                    MessageBox.Show("请选择正确的目标文件路径");
                    return;
                }
                
                IsUpdateNow = true;
                if (RadioButtonSlot.IsChecked == true)
                {
                    file_info.m_iSlotIndex = Int32.Parse (TextBoxSlotNum.Text);
                }
                else
                {
                    file_info.m_iSlotIndex = -1;
                }

                file_info.m_bDownloadToDevice = true;
                bool ret;
                switch (combo.SelectedIndex)
                {
                    case 0:
                        file_info.m_iFileType = FILETYPE_FIRMWARE;
                        break;
                    case 1:
                        file_info.m_iFileType = FILETYPE_OTHER;
                        break;
                    case 2:
                        file_info.m_iFileType = FILETYPE_BSP;
                        break;
                    default:
                        Console.WriteLine("Error:You didn't select update fileType!!!");
                        break;
                }

                //if (FILETYPE_BSP == file_info.m_iFileType)
                //{
                //    string strIP = addressText.Text.Replace(" ", "");
                //    if (TCPCommand.GetInstance().UpdateBSP(strIP))
                //        MessageBox.Show("BSP更新成功！");
                //    else
                //        MessageBox.Show("BSP更新失败！");
                //    IsUpdateNow = false;
                //    return;
                //}
                if (File.Exists(fileText.Text))
                    file_info.m_strFilePath = fileText.Text;
                else
                {
                    MessageBox.Show("请选择正确的文件路径！");
                    IsUpdateNow = false;
                    return;
                }
                string FileName = file_info.m_strFilePath.Substring(file_info.m_strFilePath.LastIndexOf("\\") + 1);

                for (int i = 0; i < FileName.Length; i++)
                {
                    if ((short)FileName[i] > 127)
                    {
                        MessageBox.Show("文件名包含有中文字符！");
                        IsUpdateNow = false;
                        return;
                    }
                }
                ret = FileTransferMgr.Instance.DownloadToDevice(file_info);

            }
        }
        /// <summary>
        /// 更新前对固件程序/FPGA程序的bin文件进行检测
        /// </summary>
        public bool BinaryFileCheck(string path,string deviceName)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                file.Seek(8, SeekOrigin.Begin);
                BinaryReader read = new BinaryReader(file);
                byte[] buffer = new byte[64];
                read.Read(buffer, 0, 32);
                string msg = Encoding.Default.GetString(buffer);
                msg = msg.TrimEnd('\0');
                ////判断文件头截取的设备信息里是否包含插件名
                msg = msg.ToUpper();
                deviceName = deviceName.ToUpper();
                if (msg.Contains(deviceName))
                    return true;
                else
                {
                    MessageBox.Show("文件内容与目标设备类型不匹配，请确认选择文件正确！");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("请选择正确的目标文件路径");
                return false;
            }

        }
        public void UpdateWaiting(string str, System.Windows.Visibility m_visible)
        {
            this.m_ProgressBar.Visibility = m_visible;
            this.AutoGetStatus.Content = str;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            // 更新时弹出文件选择框
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.InitialDirectory = GetPathRecord(CfgFile);
            if (fileDialog.ShowDialog() == true) // 弹出文件选择框
            {
                fileText.Text = fileDialog.FileName;
            }
        }
        /// <summary>
        /// 获取上次记录的路径
        /// </summary>
        /// <param name="CfgFile"></param>
        public string GetPathRecord(string CfgFile)
        {
            StreamReader sr = new StreamReader(CfgFile);
            string ret = sr.ReadLine();
            sr.Close();
            return ret;
        }
        private void btnAutoGet_Click(object sender, RoutedEventArgs e)
        {
            //清空ProgressBar的title
            this.AutoGetStatus.Content = "";

            if (false == IsUpdateNow)
            {
                NetConnection.Instance.m_CurrentIPAddr = null;
                NetConnection.Instance.ConnectStatus = FINISH_NO;
                threadProgressBar = new Thread(ProgressBarWork);
                threadProgressBar.Start("扫描IP");
                DeviceScanner scanner = new DeviceScanner();
                scanner.DeviceScan();//m_deviceParameterViewModel
            }
            //m_OptionFormModel.CurrentIPStr = "100.100.100.100";

        }

        public void ProgressBarWork(object ProgressType)
        {
            double percent = 0; ;
            string ProgressBarStatus = "";
            string type = (string)ProgressType;
            count = 0;

            MyInvokeProgress m_Invoke = new MyInvokeProgress(UpdateProgress);
            if (type == "扫描IP")
            {
                this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "正在扫描", System.Windows.Visibility.Visible, 0 });
                while (FINISH_NO == NetConnection.Instance.ConnectStatus || (FINISH_OK == NetConnection.Instance.ConnectStatus) && (NetConnection.Instance.m_CurrentIPAddr == null))
                {
                    Thread.Sleep(50);
                    percent = (count % 200) / 2;
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "正在扫描", System.Windows.Visibility.Visible, percent });

                    if (count++ > 1000)
                        break;
                }
                if (count > 1000)
                {
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "扫描超时", System.Windows.Visibility.Visible, percent });
                    ProgressBarStatus = "扫描超时";
                }
                else
                {
                    if (FINISH_OK == NetConnection.Instance.ConnectStatus)
                    {
                        this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "扫描完成", System.Windows.Visibility.Visible, percent });
                        ProgressBarStatus = "扫描完成";
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "扫描失败", System.Windows.Visibility.Visible, percent });
                        ProgressBarStatus = "扫描失败";
                    }
                }
                Thread.Sleep(1500);
                this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "", System.Windows.Visibility.Hidden, percent });
            }
            else
            {
                //格式化进度条 
                double percentage=0.0;
                cnt = 0;
                while (TCPCommand.CONTINUE == NetConnection.Instance.IsWorkSuccess)
                {
                    Thread.Sleep(50);
                    percentage = (cnt % 200) / 2;
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "正在" + type + "...", System.Windows.Visibility.Visible, percentage });
                    if (cnt++ > 1000)
                        break; 
                }
                if (TCPCommand.TRANS_OK == NetConnection.Instance.IsWorkSuccess)
                {
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { type + "成功", System.Windows.Visibility.Visible, 100.0 });
                    Thread.Sleep(2000);
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "", System.Windows.Visibility.Collapsed, 100.0 });
                }
                else
                {
                    if(TCPCommand.IsDeviceBusy)
                        this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "设备繁忙！"+ type + "失败" + " 建议重启设备", System.Windows.Visibility.Collapsed, 100.0 });
                    else
                        this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { type + "失败", System.Windows.Visibility.Collapsed, 100.0 });
                    Thread.Sleep(2000);
                    this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { "", System.Windows.Visibility.Collapsed, 100.0 });
                }

                m_OptionFormModel.VersionResult = NetConnection.Instance.GetVersionResult;    
                NetConnection.Instance.GetVersionResult = null;
                NetConnection.Instance.IsWorkSuccess = TCPCommand.CONTINUE;
                NetConnection.Instance.IsWorkRunning = false;
                    
            }
        }

        public void UpdateProgress(string str, System.Windows.Visibility m_visible,double value)
        {
            this.m_ProgressBar.Visibility = m_visible;
            this.AutoGetStatus.Content = str;
            this.m_ProgressBar.Value = value;

            if ("扫描完成" == str)
            {
                addressText.Text = NetConnection.Instance.m_CurrentIPAddr;
                this.m_ProgressBar.Value = 100;
            }

        }

        private void SlotDeviceSelected(object sender, RoutedEventArgs e)
        {
            ModuleType.Visibility = Visibility.Collapsed;
            ModuleTypeLabel.Visibility = Visibility.Collapsed;

            TextBoxSlotNum.Visibility = Visibility.Visible;
            LabelSlotNum.Visibility = Visibility.Visible;
            UpdateModeLabel.Visibility = Visibility.Visible;
            UpdateMode.Visibility = Visibility.Visible;
            PluginType.Visibility = Visibility.Visible;
            PluginTypeLabel.Visibility = Visibility.Visible;

            if (this.combo != null && this.TextBoxSlotNum.Text.Length > 0 && this.UpdateMode.Text == "PCI更新" && this.combo.Items.Count == 4)
            {
                this.combo.Items.Remove(this.combo.Items[3]);
                this.combo.Items.Remove(this.combo.Items[2]);
            }
        }

        private void ModuleDeviceSelected(object sender, RoutedEventArgs e)
        {
            
            TextBoxSlotNum.Visibility = Visibility.Collapsed;
            LabelSlotNum.Visibility = Visibility.Collapsed;
            UpdateModeLabel.Visibility = Visibility.Collapsed;
            UpdateMode.Visibility = Visibility.Collapsed;
            PluginType.Visibility = Visibility.Collapsed;
            PluginTypeLabel.Visibility = Visibility.Collapsed;

            //ModuleType.Visibility = Visibility.Visible;
            //ModuleTypeLabel.Visibility = Visibility.Visible;
            ModuleType.Visibility = Visibility.Collapsed;
            ModuleTypeLabel.Visibility = Visibility.Collapsed;
            if (this.combo.Items.Count == 2)
            {
                this.combo.Items.Add("配置文件");
                this.combo.Items.Add("应用程序");
            }
        }

        private void TextBoxSlotNumTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.UpdateMode != null && this.TextBoxSlotNum.Text == "1")
                this.UpdateMode.SelectedIndex = 0;
            else if (this.UpdateMode != null && this.TextBoxSlotNum.Text.Length > 0)
                this.UpdateMode.SelectedIndex = 1;
        }

        private void UpdateModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCombobox();
            if(null != UpdateMode)
            {
                if ("PCI更新" == UpdateMode.Text)
                {
                    TextBoxSlotNum.IsEnabled = false;
                    PluginType.IsEnabled = false;
                    PluginType.SelectedIndex = 0;
                }
                else if ("以太网更新" == UpdateMode.Text)
                { 
                    TextBoxSlotNum.IsEnabled = true;
                    PluginType.IsEnabled = true;
                }
            }
        }
        public bool isNumberStr(string message)
        {
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
            if (rex.IsMatch(message))
                return true;
            else
                return false;
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            //threadProgressBar.Abort();
            SetHistoryIP();
            System.Environment.Exit(0);
        }
        public void UpdateProgressBarInfomation(string ProgressBarInfomation,long ReceiveBytes)
        {
            MyInvokeProgress m_Invoke = new MyInvokeProgress(UpdateProgress);
            if (ReceiveBytes>0)
                percentValue = ((100*ReceiveBytes) / (file_info.m_fileLength));
            if (ProgressBarInfomation.Contains("%"))
            {
                if (ProgressBarInfomation.Contains("擦除文件"))
                {
                    string[] str = (ProgressBarInfomation.Substring(4)).Split('%');
                    percentValue = Convert.ToDouble(Int32.Parse(str[0]));
                }
                else if (ProgressBarInfomation.Contains("完成"))
                {
                    string[] str = (ProgressBarInfomation.Substring(6)).Split('%');
                    percentValue = Convert.ToDouble(Int32.Parse(str[0]));
                }
            }
            else if (ProgressBarInfomation.Contains("percent"))
            {
                string[] str = (ProgressBarInfomation.Split(' '));
                if (Regex.IsMatch(str[2], "^[0-100]*$"))
                    percentValue = Convert.ToDouble(Int32.Parse(str[2]));
            }
            this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { ProgressBarInfomation, System.Windows.Visibility.Visible, percentValue});
            if (FINISH_NO != NetConnection.Instance.TransferStatus)
            {
                NetConnection.Instance.TransferStatus = 0;
                Thread.Sleep(1000);
                this.Dispatcher.BeginInvoke(m_Invoke, new Object[] { ProgressBarInfomation, System.Windows.Visibility.Hidden, percentValue });
            }
        }

        public void UpdateProgressBarInfomation(bool IsFinish)
        {
            if(!IsFinish)
            {
                Thread.Sleep(100);
                percentValue = percentValue + 1;
            }
            percentValue = 100.0;
        }
        public void UpdateCombobox()
        {
            if (this.combo != null && this.UpdateMode.Text == "PCI更新")
            {
                if (this.combo.Items.Count == 2)
                {
                    this.combo.Items.Add("配置文件");
                    this.combo.Items.Add("应用程序");
                }
            }
            else if (this.combo != null && this.TextBoxSlotNum.Text.Length > 0 && this.UpdateMode.Text == "以太网更新")
            {
                this.combo.Items.Remove(this.combo.Items[3]);
                this.combo.Items.Remove(this.combo.Items[2]);
            }
        }

        private void FormattingClick(object sender, RoutedEventArgs e)
        {
            if(!NetConnection.Instance.IsWorkRunning)
            {
                NetConnection.Instance.IsWorkRunning = true;
                string strIP = addressText.Text.Replace(" ", "");
                Thread FormdattingThread = new Thread(ThreadFormatting);
                FormdattingThread.Start(strIP);
                threadProgressBar = new Thread(ProgressBarWork);
                threadProgressBar.Start("格式化");
            }
            
            //if (ret == TCPCommand.TRANS_OK)
            //    FormattingText.Text = "格式化成功";
            //else
            //    FormattingText.Text = "格式化失败";
            //UpdateProgressBarInfomation
        }
        public void ThreadFormatting(object IP)
        {
            string strIP = (string)IP;
            
            int ret = TCPCommand.GetInstance().Formatting(strIP);
            if (ret == TCPCommand.TRANS_OK)
            {
                NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_OK;
                MessageBox.Show("格式化成功！");
            }
            else
            {
                NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_ERROR;
                MessageBox.Show("格式化失败！");
            }
        }

        private void GetTFFSVersionClick(object sender, RoutedEventArgs e)
        {
            if (!NetConnection.Instance.IsWorkRunning)
            {
                NetConnection.Instance.IsWorkRunning = true;
                string strIP = addressText.Text.Replace(" ", "");

                Thread GetVersionThread = new Thread(ThreadGetVersion);
                GetVersionThread.Start(strIP);
                threadProgressBar = new Thread(ProgressBarWork);
                threadProgressBar.Start("获取版本");
            }
        }

        public void ThreadGetVersion(object IP)
        {
            string strIP = (string)IP;
            string result;
            result = TCPCommand.GetInstance().GetTFFSVersion(strIP);
            if (null != result)
            {
                int retDec = Convert.ToInt32(result);
                result = "0X" + retDec.ToString("x4");
                NetConnection.Instance.GetVersionResult = result;
                NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_OK;
            }
            else
            {
                NetConnection.Instance.GetVersionResult = "未获取";
                NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_ERROR;
            }
        }
        //public void ThreadUpdateBSP(object IP)
        //{
        //    string strIP = (string)IP;
        //    if(TCPCommand.GetInstance().UpdateBSP(strIP))
        //    {
        //        NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_OK;
        //    }
        //    else
        //    {
        //        NetConnection.Instance.IsWorkSuccess = TCPCommand.TRANS_ERROR;
        //    }

        //}
        private void ModuleSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ModuleTypeLostFocus(object sender, RoutedEventArgs e)
        {
            //if (ModuleType.Text == "EDRM")
            //{
            //    FormattingButton.IsEnabled = true;
            //    TFFSVersionButton.IsEnabled = true;
            //}
            //else
            //{
            //    FormattingButton.IsEnabled = false;
            //    TFFSVersionButton.IsEnabled = false;
            //}
        }

        private void FileTypeComboLostFocus(object sender, RoutedEventArgs e)
        {
            //if ("固件+BSP一起更新" == this.combo.Text)
            //{ 
            //    fileText.IsEnabled = false;
            //    fileSelect.IsEnabled = false;
            //}
            //else
            //{
            //    fileText.IsEnabled = true;
            //    fileSelect.IsEnabled = true;
            //}
        }
        //private void open_exe(object sender, RoutedEventArgs e)
        //{
        //    string path = @"WinPcap_4_1_3.exe";//这个path就是你要调用的exe程序的绝对路径
        //    System.Diagnostics.Process process = new System.Diagnostics.Process();
        //    process.StartInfo.FileName = "WinPcap_4_1_3.exe";
        //    process.StartInfo.WorkingDirectory = path;
        //    process.StartInfo.CreateNoWindow = true;
        //    process.Start();
        //}

        public void SetHistoryIP()
        {
            string SysDirectory = System.Environment.SystemDirectory;
            string HistoryIPFile = SysDirectory.Substring(0, 3);
            HistoryIPFile = HistoryIPFile + "ConfigUpdateIP.txt";
            string IPString;
            IPAddress IPaddr;
            FileStream fs;
            IPString = this.addressText.Text;
            try
            {
                    fs = new FileStream(HistoryIPFile,
                    FileMode.Create);

                    StreamWriter sw = new StreamWriter(fs);
                    //开始写入
                    IPString = IPString.Replace(" ", "");
                    Regex rx = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
                   if (rx.IsMatch(IPString))
                        sw.Write(IPString);
                    else
                        sw.Write("169.254.147.106");
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message + "\n Cannot create file.");
            }

        }
    }





}
