using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Forms;

namespace AutoUpdate
{
    internal class Config
    {
        #region Properties

        private static Config n_Configs;

        private string _FileGame;
        private string _AutoTrain;
        private string _AutoPK;
        private string _WebHome;
        private string _WebRegister;
        private string _WebSupport;
        private string _WebCom;

        private string _StartFile;
        public string StartFile
        {
            get { return _StartFile; }
            set { _StartFile = value; }
        }

        private string _Slides;
        public string Slides
        {
            get { return _Slides; }
            set { _Slides = value; }
        }

        private string _Version;
        public string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        private string _ServerUpdate;
        public string ServerUpdate
        {
            get { return _ServerUpdate; }
            set { _ServerUpdate = value; }
        }

        private string _News;
        public string News
        {
            get { return _News; }
            set { _News = value; }
        }

        private string _GSIP;
        public string GSIP
        {
            get { return _GSIP; }
            set { _GSIP = value; }
        }

        private string _GSPort;
        public string GSPort
        {
            get { return _GSPort; }
            set { _GSPort = value; }
        }
        public string FileGame
        {
            get { return _FileGame; }
            set { _FileGame = value; }
        }
        public string AutoTrain
        {
            get { return _AutoTrain; }
            set { _AutoTrain = value; }
        }
        public string AutoPK
        {
            get { return _AutoPK; }
            set { _AutoPK = value; }
        }
        public string WebHome
        {
            get { return _WebHome; }
            set { _WebHome = value; }
        }
        public string WebRegister
        {
            get { return _WebRegister; }
            set { _WebRegister = value; }
        }
        public string WebSupport
        {
            get { return _WebSupport; }
            set { _WebSupport = value; }
        }
        public string WebCom
        {
            get { return _WebCom; }
            set { _WebCom = value; }
        }

        #endregion

        public static Config GetConfigs()
        {
            if (n_Configs == null)
            {
                n_Configs = new Config();
            }
            return n_Configs;
        }

        protected Config()
        {

        }

        public void LoadLocalConfig(string FileName, string EncryptKey)
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    MessageBox.Show("Không tìm thấy file cấu hình \nXin hãy liên hệ với ban quản trị!", "Thông Báo");
                    Environment.Exit(0);
                }
                string[] ConfigsList_ = Regex.Split(SecureStringManager.Decrypt(File.ReadAllText(FileName), EncryptKey), "\r\n");
                n_Configs.Version = ConfigsList_[0];
                n_Configs.ServerUpdate = ConfigsList_[1];
                n_Configs.Slides = ConfigsList_[2];
                n_Configs.News = ConfigsList_[3];
                n_Configs.GSIP = ConfigsList_[4];
                n_Configs.GSPort = ConfigsList_[5];
                n_Configs.FileGame = ConfigsList_[6];
                n_Configs.AutoTrain = ConfigsList_[7];
                n_Configs.AutoPK = ConfigsList_[8];
                n_Configs.WebHome = ConfigsList_[9];
                n_Configs.WebRegister = ConfigsList_[10];
                n_Configs.WebSupport = ConfigsList_[11];
                n_Configs.WebCom = ConfigsList_[12];


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
