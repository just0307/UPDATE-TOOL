using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileTransfer
{
    
    public class FileTransferInfo
    {
        /// <summary>
        /// 接口返回值定义
        /// </summary>
        public const int RET_OK = 0;
        public const int RET_NG = 1;
        public const int RET_CONNECT_ERROR = 2;


        /// <summary>
        /// 文件类型定义
        /// </summary>
        public const int FILETYPE_FIRMWARE        = 0; //固件文件
        public const int FILETYPE_APPLICATION     = 1; //应用程序文件
        public const int FILETYPE_FPGA            = 2; //FPGA文件
        public const int FILETYPE_CPLD            = 3; //CPLD文件
        public const int FILETYPE_CONFIG_TOPOLOGY = 4; //拓扑配置文件(在线拓扑扫描时，使用此接口)
        public const int FILETYPE_DIAGNOSIS       = 5; //诊断信息文件
        public const int FILETYPE_CONFIG_DEVICE   = 6; //设备配置信息文件
        public const int FILETYPE_CONFIG_MVB      = 7; //MVB配置文件
        public const int FILETYPE_CONFIG_TRDP     = 8; //TRDP配置文件
        public const int FILETYPE_BSP = 20; //BSP文件
    }
}
