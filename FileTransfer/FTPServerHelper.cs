using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Midapex.Net.Ftp;
using Midapex.Net;
using System.IO;
using InToolSet.Logging;

namespace FileTransfer
{
    public class FTPServerHelper
    {
        public static string FTP_USR = "admin";
        public static string FTP_PASS = "admin";
        public static int FTP_PORT = 2221;
        public static string TEMP_PATH = Path.GetTempPath() + "InToolSetTemp" + Path.DirectorySeparatorChar;

        private FtpServer mFtpServer;
        private static FTPServerHelper m_instance = new FTPServerHelper();
     
        public static FTPServerHelper Instance
        {
            get
            {
                return m_instance;
            }
        }

        private FTPServerHelper()
        {
            mFtpServer = new FtpServer();

            /*
            * 服务器的最大连接数
            */
            mFtpServer.Capacity = 1000;

            /*
             * 连接超时时间
             */
            mFtpServer.HeartBeatPeriod = 120000;  //120秒

            /*
             * 创建一个使用FTP的用户，
             */
            FtpUser user = new FtpUser("admin");
            user.Password = "admin";
            user.AllowWrite = true;
            user.HomeDir = FTPServerHelper.getTempPath();

            /*
             * 限制该帐号的用户的连接服务器的最大连接数
             * 也就是限制该使用该帐号的FTP同时连接服务器的数量。
             */
            user.MaxConnectionCount = 2;

            /*
             * 限制用户的最大上传文件为20M，超过这个值上传文件会失败。
             * 默认不限制该值，可以传输大文件。
             */
            user.MaxUploadFileLength = 1024 * 1024 * 200;
            mFtpServer.AddUser(user);
            mFtpServer.Port = FTP_PORT;

            //把当前目录作为匿名用户的目录，测试目的(必须指定)
            mFtpServer.AnonymousUser.HomeDir = FTPServerHelper.getTempPath();
        }

        public void startServer()
        {
            if (!isStarted())
            {
                mFtpServer.Start();
            }
        }

        public void stopServer()
        {
            mFtpServer.Stop();
            mFtpServer.Dispose();
        }
        public bool isStarted()
        {
            return mFtpServer.IsRun;
        }

        public int getPort()
        {
            return mFtpServer.Port;
        }

        public static string getTempPath()
        {
            string strPath = TEMP_PATH;
            CreatePath(TEMP_PATH);
            return strPath;
        }

        public static void CreatePath(string strPath)
        {
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
            }
            catch (Exception ex)
            {
                //创建或删除临时路径失败
                Log.Error(string.Format("Path create error. path=[{0}]", strPath), ex);
            }
        }
    }
}
