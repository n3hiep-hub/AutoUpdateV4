using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Net.Sockets;
using System.Timers;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.Win32;
using Ini;
using Ionic.Zip;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
// Không dùng "using IWshRuntimeLibrary" để tránh conflict File với System.IO.File
// Dùng fully-qualified IWshRuntimeLibrary.WshShell và IWshRuntimeLibrary.IWshShortcut thay thế
// Alias ngắn gọn để code dễ đọc hơn
using SysFile = System.IO.File;
using SysDirectory = System.IO.Directory;
using SysPath = System.IO.Path;
using SysStream = System.IO.FileStream;
using SysStreamReader = System.IO.StreamReader;

namespace AutoUpdate
{
    public partial class MainWindow : Window
    {
        private bool isMainWindowEnabled = true;
        private const string EncryptKey = "zxczxczxc";
        private String[,] rssData = null;
        private String[,] ImgData = null;
        public static MainWindow _FormInstance;
        public static String DirApp { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        System.Windows.Forms.Timer timerTime = new System.Windows.Forms.Timer();
        private System.Timers.Timer myTimer = new System.Timers.Timer();
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private readonly DoubleAnimation _oa;
        private static Mutex mutex;
        private bool m_IsPressed = false;
        Storyboard fadeIn, fadeOut;
        string slide1 = null;
        string slide2 = null;
        string slide3 = null;
        string slide4 = null;
        string slide5 = null;
        string slidelink = null;
        private double percentwidth;
        private string md5onlineautoupdate = "";
        private string md5autoupdate = "";
        private Config n_Configs = null;
        private XmlTextReader reader;
        private long P;
        private long H;
        private long num2;
        private int percentvalue;

        // =====================================================================
        // FIX #5: Bỏ qua lỗi SSL certificate (hỗ trợ HTTPS)
        // =====================================================================
        private static bool ValidateServerCertificate(
            object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Accept all certificates
        }

        public void DownloadComplete(bool cancelled)
        {
            bool flagCancelled = !cancelled;
            if (flagCancelled)
            {
                SetUpdateCompletedState();
                GetDataConfig();
                this.progressbar.Width = 536;
                this.progresspercent_total.Content = "100";
                this.progresspercent_now_file.Content = "100";
                this.progresspercent_total.Width = 55;
                this.progresspercent_now_file.Width = 55;
                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.lblTienTrinh.Content = "Cập nhật thành công phiên bản " + this.n_Configs.Version;
                    this.lblThongTin.Content = "Vui lòng bấm vào nút [ĐĂNG NHẬP] để bắt đầu trò chơi.";
                    this.txtCurVer.Content = "Phiên bản hiện tại: " + this.n_Configs.Version;
                }));
                SysFile.Delete(AppDomain.CurrentDomain.BaseDirectory + "update.xml");
                Thread.Sleep(500);
                this.btnConfig.IsEnabled = true;
                if (!cancelled)
                {
                    this.Stop_Animation_ProgressBar();
                    this.StartButtonRotations();
                }
            }
        }

        private bool isMouseOverButton = false;
        private void StartButtonRotations()
        {
            if (!isMouseOverButton)
            {
                var insideAnimation = new DoubleAnimation
                {
                    By = 360,
                    RepeatBehavior = RepeatBehavior.Forever,
                    Duration = TimeSpan.FromSeconds(1.5)
                };
                Inside.BeginAnimation(RotateTransform.AngleProperty, insideAnimation);
            }
        }

        private void StopButtonRotations()
        {
            Inside.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        private void DelFolder()
        {
            if (SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "settings"))
            {
                string settings = (AppDomain.CurrentDomain.BaseDirectory + "settings");
                SysDirectory.Delete(settings, true);
            }
            if (SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "ui"))
            {
                string ui = (AppDomain.CurrentDomain.BaseDirectory + "ui");
                SysDirectory.Delete(ui, true);
            }
            if (SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "UImage"))
            {
                string script = (AppDomain.CurrentDomain.BaseDirectory + "UImage");
                SysDirectory.Delete(script, true);
            }
            if (SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Image"))
            {
                string script = (AppDomain.CurrentDomain.BaseDirectory + "Image");
                SysDirectory.Delete(script, true);
            }
        }

        private void BtnLogin_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            try
            {
                new Thread(new ThreadStart(this.RunGame)).Start();
            }
            catch { }
        }

        private bool checkAuto()
        {
            return Process.GetProcessesByName("AutoVLTK").Length <= 0;
        }

        private bool checkRunningGame()
        {
            return Process.GetProcessesByName("Game").Length == 0;
        }

        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        private void RunGame()
        {
            string pathGame = SysPath.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                + "\\" + this.n_Configs.FileGame;
            try
            {
                Process runGame = Process.Start(pathGame);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainNotification mainNotification = new MainNotification(ex.Message + " Path: " + pathGame);
                    mainNotification.OkButtonClicked += (senderObj, args) =>
                    {
                        mainNotification.Close();
                        this.IsEnabled = true;
                    };
                    mainNotification.Show();
                }));
                Thread.Sleep(2000);
            }
        }

        private void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bool flag = e.Error != null;
            if (flag)
            {
                Console.WriteLine(e.Error.Message);
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainNotification mainNotification = new MainNotification(e.Error.Message);
                    mainNotification.OkButtonClicked += (senderObj, args) =>
                    {
                        mainNotification.Close();
                        this.IsEnabled = true;
                    };
                    mainNotification.Show();
                }));
                Thread.Sleep(2000);
            }
            else
            {
                new Thread(new ThreadStart(this.RunGame)).Start();
            }
        }

        public void ChangeTexts(double length, double percent)
        {
            percentvalue = (int)Math.Round(percent);
            this.lblThongTin.Content = "Tiến độ tải tập tin: " + (int)H + "/" + (int)P + " [" + percentvalue + "%]";
            short value = (short)Math.Round((double)(this.H * 100L) / (double)this.P);
            this.progressbar.Width = percentvalue * 536 / 100;
            this.progresspercent_total.Content = value;
            this.progresspercent_now_file.Content = percentvalue;
            percentwidth = 55;
            this.progresspercent_total.Width = percentwidth;
            this.progresspercent_now_file.Width = percentwidth;
        }

        public MainWindow()
        {
            InitializeComponent();
            checkAdmin();

            // =====================================================================
            // FIX #5: Cấu hình HTTPS/TLS trước khi làm bất cứ điều gì
            // Dùng giá trị số vì .NET 4.0 chưa có enum Tls12/Tls11
            // Tls   = 192, Tls11 = 768, Tls12 = 3072
            // =====================================================================
            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType)3072   // TLS 1.2
                | (SecurityProtocolType)768  // TLS 1.1
                | (SecurityProtocolType)192; // TLS 1.0
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(ValidateServerCertificate);

            // =====================================================================
            // FIX #1: Thêm thư mục vào whitelist Windows Defender
            // =====================================================================
            AddDefenderExclusion();

            ni.Icon = AutoUpdate.Properties.Resources.vltk2;
            ni.DoubleClick += delegate (object sender, EventArgs args)
            {
                Show();
                WindowState = WindowState.Normal;
                ni.Visible = false;
            };

            this.MouseDown += new MouseButtonEventHandler(MainBgr_MouseDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(MainBgr_PreviewMouseDown);
            this.PreviewMouseUp += new MouseButtonEventHandler(MainBgr_PreviewMouseUp);

            bool firstInstance;
            mutex = new Mutex(false, "Thông Báo AutoUpdate", out firstInstance);
            if (!firstInstance)
            {
                MainNotification mainNotification = new MainNotification("AutoUpdate đang chạy!");
                mainNotification.OkButtonClicked += (senderObj, args) =>
                {
                    mainNotification.Close();
                    Thread.Sleep(100);
                    this.Close();
                };
                mainNotification.Show();
                this.Close();
            }
            else
            {
                Process[] processesByName = Process.GetProcessesByName("Game");
                for (int gameproc = 0; gameproc < processesByName.Length; gameproc++)
                    if (MessageBox.Show("Trò chơi đang chạy, bạn có muốn thoát game để cập nhật bản mới không?",
                        "Thông Báo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        this.Close();
                    }
                    else
                    {
                        processesByName[gameproc].Kill();
                    }
            }

            this.SetUpdatingState();
            this.progressbar.Width = 0;
            this.txtCurVer.Content = "Phiên bản hiện tại: ----";
            this.progresspercent_total.Content = "";
            this.progresspercent_now_file.Content = "";
            this.lblThongTin.Content = "";
            this.lblTienTrinh.Content = "";
            this.txtStatus.Content = "----";
            this.txtStatus.Foreground = Brushes.Gray;
            this.btnConfig.IsEnabled = false;
            this.GetDataConfig();
            this.CheckUpdate();

            this.SetRSS();
            this.SetImgData();
            this.slideshow.Source = new ImageSourceConverter().ConvertFromString(slide5) as ImageSource;
            timerTime.Tick += (timer1_Tick);
            timerTime.Interval = 4000;
            timerTime.Start();

            fadeIn = (Storyboard)this.Resources["FadeIn"];
            fadeOut = (Storyboard)this.Resources["FadeOut"];
            this.BeginStoryboard(fadeOut);
            CheckPort(n_Configs.GSIP, int.Parse(n_Configs.GSPort), txtStatus);
            _oa = new DoubleAnimation();
            _oa.From = 0;
            _oa.To = 1;
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(900d));
            BeginAnimation(OpacityProperty, _oa);
            slideshow.Source = new ImageSourceConverter().ConvertFromString(slide1) as ImageSource;
            slidelink = ImgData[0, 2];
            this.DelFolder();
        }

        // =====================================================================
        // FIX #1: Thêm thư mục vào whitelist Windows Defender
        // =====================================================================
        private void AddDefenderExclusion()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                // Chạy ngầm, không hiện cửa sổ
                this.RunCMD(
                    "powershell -Command \"Add-MpPreference -ExclusionPath '" + baseDir + "'\"",
                    false, false, false
                );
            }
            catch
            {
                // Bỏ qua nếu lỗi (ví dụ Defender bị tắt)
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void checkAdmin()
        {
            if (!IsAdministrator())
            {
                MainNotification mainNotification = new MainNotification("Hãy khởi chạy bằng quyền Admin!");
                mainNotification.OkButtonClicked += (senderObj, args) =>
                {
                    mainNotification.Close();
                    Thread.Sleep(100);
                    this.Close();
                };
                mainNotification.Show();
            }
        }

        public void RunCMD(string command, bool ShowWindow, bool WaitForProcessComplete, bool permanent)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.Arguments = " " + ((ShowWindow && permanent) ? "/K" : "/C") + " " + command;
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.CreateNoWindow = !ShowWindow;
            if (ShowWindow)
                processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            else
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            new Process { StartInfo = processStartInfo }.Start();
        }

        private void GetDataConfig()
        {
            n_Configs = Config.GetConfigs();
            n_Configs.LoadLocalConfig(AppDomain.CurrentDomain.BaseDirectory + "autoupdate.dat", EncryptKey);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetUpdatingState();
            this.txtCurVer.Content = "Phiên bản hiện tại: " + this.n_Configs.Version;
        }

        private void CheckUpdate()
        {
            string folderPathz = AppDomain.CurrentDomain.BaseDirectory + "\\temp";

            if (SysDirectory.Exists(folderPathz))
            {
                try
                {
                    SysDirectory.Delete(folderPathz, true);
                }
                catch (Exception ex)
                {
                    MainNotification mainNotification = new MainNotification("Lỗi khi xoá thư mục: " + ex.Message);
                    mainNotification.OkButtonClicked += (senderObj, args) =>
                    {
                        mainNotification.Close();
                        Thread.Sleep(100);
                        this.Close();
                    };
                    mainNotification.Show();
                }
            }

            try
            {
                WebClient webClient = new WebClient();
                SysStreamReader streamReader = new SysStreamReader(
                    webClient.OpenRead(n_Configs.ServerUpdate + "checkautoupdate.php?vlph=" + Guid.NewGuid().ToString())
                );
                this.md5onlineautoupdate = streamReader.ReadToEnd();
                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.lblTienTrinh.Content = "Đang kiểm tra tập tin [AutoUpdate.exe]...";
                }));
                this.md5autoupdate = GetMD5HashFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdate.exe");
                bool flag2 = this.md5autoupdate != this.md5onlineautoupdate && this.md5onlineautoupdate.Length > 0;
                if (flag2)
                {
                    string folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\temp";

                    if (!SysDirectory.Exists(folderPath))
                    {
                        try
                        {
                            SysDirectory.CreateDirectory(folderPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi tạo thư mục: " + ex.Message);
                        }
                    }

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.lblTienTrinh.Content = "Đang tải về tập tin [AutoUpdate.exe]...";
                    }));

                    // =====================================================================
                    // FIX #3: Dùng file trung gian thay vì ghi thẳng vào AutoUpdate.exe
                    // =====================================================================
                    string tempUpdateFile = folderPath + "\\AutoUpdate_new.exe";
                    webClient.DownloadFile(n_Configs.ServerUpdate + "files/AutoUpdate.exe", tempUpdateFile);

                    ni.Visible = true;
                    ni.ShowBalloonTip(1000, "Thông Báo",
                        "Đã tải về phiên bản mới AutoUpdate.Exe. Đang áp dụng...",
                        System.Windows.Forms.ToolTipIcon.Info);

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.lblTienTrinh.Content = "Đang áp dụng cập nhật [AutoUpdate.exe]...";
                    }));

                    // Tạo batch script để thay thế file sau khi app đóng
                    CreateUpdateBatch(tempUpdateFile);
                    Thread.Sleep(1000);

                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        // Chạy batch thay thế file
                        Process.Start(
                            new ProcessStartInfo(
                                AppDomain.CurrentDomain.BaseDirectory + "\\au_updater.bat"
                            )
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true
                            }
                        );
                    }));
                    Environment.Exit(0);
                }
                else
                {
                    new Thread(new ThreadStart(this.BatDauHoatDong)).Start();
                    new Thread(new ThreadStart(this.CreateShortcut)).Start();
                }
            }
            catch (WebException ex2)
            {
                bool flag3 = ex2.Status == WebExceptionStatus.NameResolutionFailure;
                if (flag3)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.lblThongTin.Content = "Phân giải tên miền thất bại, vui lòng kiểm tra kết nối mạng! code: 691";
                    }));
                }
                new Thread(new ThreadStart(this.BatDauHoatDong)).Start();
                new Thread(new ThreadStart(this.CreateShortcut)).Start();
            }
        }

        // =====================================================================
        // FIX #3: Tạo batch script thay thế AutoUpdate.exe
        // =====================================================================
        private void CreateUpdateBatch(string newExePath)
        {
            string currentExe = AppDomain.CurrentDomain.BaseDirectory + "\\AutoUpdate.exe";
            string batchPath = AppDomain.CurrentDomain.BaseDirectory + "\\au_updater.bat";

            string batchContent =
                "@echo off\r\n" +
                "timeout /t 2 /nobreak > nul\r\n" +
                "move /y \"" + newExePath + "\" \"" + currentExe + "\"\r\n" +
                "start \"\" \"" + currentExe + "\"\r\n" +
                "del \"%~f0\"\r\n"; // Tự xóa batch sau khi chạy xong

            SysFile.WriteAllText(batchPath, batchContent, Encoding.Default);
        }

        private void BatDauHoatDong()
        {
            new Thread(new ThreadStart(this.KiemTraUpdateXml)).Start();
        }

        private void Set16Bit()
        {
            if (SysFile.Exists(AppDomain.CurrentDomain.BaseDirectory + "Game.exe"))
            {
                var valueLC = Registry.CurrentUser.CreateSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                var valueCU = Registry.LocalMachine.CreateSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                if (valueCU != null)
                {
                    valueCU.SetValue(AppDomain.CurrentDomain.BaseDirectory + "Game.exe",
                        "~ RUNASADMIN 16BITCOLOR WIN8RTM");
                    valueLC.SetValue(AppDomain.CurrentDomain.BaseDirectory + "Game.exe",
                        "~ RUNASADMIN 16BITCOLOR WIN8RTM");
                }
                else
                {
                    var resCU = Registry.CurrentUser.CreateSubKey(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                    var resLC = Registry.LocalMachine.CreateSubKey(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                    resCU.SetValue(AppDomain.CurrentDomain.BaseDirectory + "Game.exe",
                        "~ RUNASADMIN 16BITCOLOR WIN8RTM");
                    resLC.SetValue(AppDomain.CurrentDomain.BaseDirectory + "Game.exe",
                        "~ RUNASADMIN 16BITCOLOR WIN8RTM");
                }
            }
        }

        private bool isMouseOverButton_Progresbar = false;
        private void Start_Animation_ProgressBar()
        {
            if (!isMouseOverButton_Progresbar)
            {
                var outsideAnimation = new DoubleAnimation
                {
                    By = 360,
                    RepeatBehavior = RepeatBehavior.Forever,
                    Duration = TimeSpan.FromSeconds(4)
                };
                Outside_progressbar.BeginAnimation(RotateTransform.AngleProperty, outsideAnimation);
            }
        }

        private void Stop_Animation_ProgressBar()
        {
            Outside_progressbar.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        private void KiemTraUpdateXml()
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                SetUpdatingState();
            }));
            bool flagXML = SysFile.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\update.xml");
            if (flagXML)
            {
                try
                {
                    SysFile.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\update.xml");
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        MainNotification mainNotification = new MainNotification("Lỗi khi xóa tệp" + ex.Message);
                        mainNotification.OkButtonClicked += (senderObj, args) =>
                        {
                            mainNotification.Close();
                            Thread.Sleep(100);
                            this.Close();
                        };
                        mainNotification.Show();
                    }));
                }
            }
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(
                    n_Configs.ServerUpdate + "update.php?vlph=" + Guid.NewGuid().ToString(),
                    AppDomain.CurrentDomain.BaseDirectory + "\\update.xml"
                );
                Thread.Sleep(1000);
                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.lblTienTrinh.Content = "Đang kiểm tra tập tin [update.xml]";
                    this.lblThongTin.Content = "Bắt đầu tiến trình cập nhật...";
                }));
                this.DemSoLuongTapTin();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainNotification mainNotification = new MainNotification("Lỗi kiểm tra tập tin [update.xml]");
                    mainNotification.OkButtonClicked += (senderObj, args) =>
                    {
                        mainNotification.Close();
                        Thread.Sleep(100);
                        this.Close();
                    };
                    mainNotification.Show();
                }));
                Thread.Sleep(2000);
            }
            try
            {
                SysFile.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\update.xml");
            }
            catch (Exception ex3)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainNotification mainNotification = new MainNotification("Lỗi khi xóa tệp" + ex3.Message);
                    mainNotification.OkButtonClicked += (senderObj, args) =>
                    {
                        mainNotification.Close();
                        Thread.Sleep(100);
                        this.Close();
                    };
                    mainNotification.Show();
                }));
            }
        }

        private void DemSoLuongTapTin()
        {
            string text = "";
            this.reader = new XmlTextReader(AppDomain.CurrentDomain.BaseDirectory + "\\update.xml");
            while (this.reader.Read())
            {
                bool flag = this.reader.NodeType == XmlNodeType.Element;
                if (flag)
                {
                    bool flag2 = this.reader.Name == "Folder" && this.reader.AttributeCount > 0;
                    if (flag2)
                    {
                        while (this.reader.MoveToNextAttribute())
                        {
                            bool flag3 = this.reader.Name == "Name"
                                && !SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + this.reader.Value);
                            if (flag3)
                                SysDirectory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\" + this.reader.Value);
                        }
                    }
                    bool flag4 = this.reader.Name == "File" && this.reader.AttributeCount > 0;
                    if (flag4)
                    {
                        while (this.reader.MoveToNextAttribute())
                        {
                            bool flag5 = this.reader.Name == "Name";
                            if (flag5) text = this.reader.Value;
                            bool flag6 = this.reader.Name == "MD5" && text != "";
                            if (flag6)
                            {
                                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    this.lblTienTrinh.Content = "Kiểm tra tập tin [" + text + "]";
                                }));
                                bool flag7 = SysFile.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + text);
                                if (flag7)
                                {
                                    bool flag8 = GetMD5HashFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\" + text)
                                        != this.reader.Value;
                                    if (flag8) this.P += 1L;
                                }
                                else
                                {
                                    this.P += 1L;
                                }
                            }
                        }
                    }
                }
            }
            this.reader.Close();
            this.KiemTraTapTin();
        }

        private void KiemTraTapTin()
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.lblThongTin.Content = "Đang nâng cấp phiên bản vui lòng chờ...";
            }));
            string text = "";
            this.reader = new XmlTextReader(AppDomain.CurrentDomain.BaseDirectory + "\\update.xml");
            while (this.reader.Read())
            {
                bool flag = this.reader.NodeType == XmlNodeType.Element;
                if (flag)
                {
                    bool flag2 = this.reader.Name == "Folder" && this.reader.AttributeCount > 0;
                    if (flag2)
                    {
                        while (this.reader.MoveToNextAttribute())
                        {
                            bool flag3 = this.reader.Name == "Name"
                                && !SysDirectory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + this.reader.Value);
                            if (flag3)
                                SysDirectory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\" + this.reader.Value);
                        }
                    }
                    bool flag4 = this.reader.Name == "File" && this.reader.AttributeCount > 0;
                    if (flag4)
                    {
                        while (this.reader.MoveToNextAttribute())
                        {
                            bool flag5 = this.reader.Name == "Name";
                            if (flag5) text = this.reader.Value;
                            bool flag6 = this.reader.Name == "MD5" && text != "";
                            if (flag6)
                            {
                                bool flag7 = SysFile.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + text);
                                if (flag7)
                                {
                                    bool flag8 = GetMD5HashFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\" + text)
                                        != this.reader.Value;
                                    if (flag8)
                                    {
                                        if (SysFile.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + text))
                                            SysFile.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\" + text);
                                        try
                                        {
                                            DownloadFileWithProgress(text);
                                        }
                                        catch (Exception ex)
                                        {
                                            App.Current.Dispatcher.Invoke((Action)(() =>
                                            {
                                                MainNotification mn = new MainNotification("Lỗi Kiểm Tra Tệp Tin\n" + ex.Message);
                                                mn.OkButtonClicked += (s, a) => { mn.Close(); Thread.Sleep(100); this.Close(); };
                                                mn.Show();
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        DownloadFileWithProgress(text);
                                    }
                                    catch (Exception ex2)
                                    {
                                        App.Current.Dispatcher.Invoke((Action)(() =>
                                        {
                                            MainNotification mn = new MainNotification("Lỗi tải về tập tin [" + text + "]\n" + ex2.Message);
                                            mn.OkButtonClicked += (s, a) => { mn.Close(); Thread.Sleep(100); this.Close(); };
                                            mn.Show();
                                        }));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.reader.Close();
            DownloadCompleteSafe method3 = new DownloadCompleteSafe(this.DownloadComplete);
            Dispatcher.Invoke(method3, new object[] { false });
        }

        // =====================================================================
        // FIX #5: Hàm download dùng HTTPS với progress
        // =====================================================================
        private void DownloadFileWithProgress(string fileName)
        {
            // Thử HTTPS trước, fallback về HTTP nếu lỗi
            string[] urlPrefixes = {
                n_Configs.ServerUpdate.Replace("http://", "https://"),
                n_Configs.ServerUpdate
            };

            Exception lastException = null;
            foreach (string prefix in urlPrefixes)
            {
                try
                {
                    string url = prefix + "files/" + fileName;
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.Timeout = 30000;
                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    long contentLength = httpWebResponse.ContentLength;
                    ChangeTextsSafe method = new ChangeTextsSafe(this.ChangeTexts);
                    Dispatcher.Invoke(method, new object[] { contentLength, 0 });

                    SysStream fileStream = new SysStream(
                        AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName,
                        System.IO.FileMode.Create
                    );

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.lblTienTrinh.Content = "Đang tải về tập tin [" + fileName + "]";
                    }));

                    this.num2 = 0;
                    for (;;)
                    {
                        byte[] buffer = new byte[4096];
                        int num = httpWebResponse.GetResponseStream().Read(buffer, 0, 4096);
                        this.num2 += (long)num;
                        short num2 = (short)Math.Round((double)(this.num2 * 100L) / (double)contentLength);
                        Dispatcher.Invoke(method, new object[] { contentLength, num2 });
                        if (num == 0) break;
                        fileStream.Write(buffer, 0, num);
                    }
                    this.H += 1L;
                    this.num2 = 0L;
                    httpWebResponse.GetResponseStream().Close();
                    fileStream.Close();
                    return; // Thành công, thoát khỏi vòng lặp
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    // Thử URL tiếp theo
                }
            }
            // Nếu cả 2 URL đều lỗi
            if (lastException != null) throw lastException;
        }

        public static string GetMD5HashFromFile(string filename)
        {
            string result;
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] array = md5.ComputeHash(SysFile.ReadAllBytes(filename));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                    sb.Append(array[i].ToString("x2"));
                result = sb.ToString();
            }
            return result;
        }

        private void CreateShortcut()
        {
            string shortcutLocation = SysPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Properties.Settings.Default.ShortCut_Name + ".lnk"
            );
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcutDesktop = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcutDesktop.Description = Properties.Settings.Default.ShortCut_Name;
            shortcutDesktop.WorkingDirectory = DirApp;
            shortcutDesktop.TargetPath = AppDomain.CurrentDomain.BaseDirectory + "AutoUpdate.exe";
            shortcutDesktop.Save();

            string shortcutLocationStartMenu = SysPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Properties.Settings.Default.ShortCut_Name + ".lnk"
            );
            IWshRuntimeLibrary.IWshShortcut shortcutStartMenu = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutLocationStartMenu);
            shortcutStartMenu.Description = Properties.Settings.Default.ShortCut_Name;
            shortcutStartMenu.WorkingDirectory = DirApp;
            shortcutStartMenu.TargetPath = AppDomain.CurrentDomain.BaseDirectory + "AutoUpdate.exe";
            shortcutStartMenu.Save();
        }

        private void CheckPort(string Ip, int Port, Label txtStatus)
        {
            TcpClient TcpScan = new TcpClient();
            var result = TcpScan.BeginConnect(Ip, Port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            if (!success)
            {
                txtStatus.Foreground = Brushes.Red;
                txtStatus.Content = "Đang bảo trì";
            }
            else
            {
                txtStatus.Foreground = Brushes.Lime;
                txtStatus.Content = "Đang hoạt động";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (new Random().Next(1, 5))
            {
                case 1:
                    fadeIn.Begin();
                    slideshow.Source = new ImageSourceConverter().ConvertFromString(slide1) as ImageSource;
                    slidelink = ImgData[0, 2];
                    fadeOut.Begin();
                    break;
                case 2:
                    fadeIn.Begin();
                    slideshow.Source = new ImageSourceConverter().ConvertFromString(slide2) as ImageSource;
                    slidelink = ImgData[1, 2];
                    fadeOut.Begin();
                    break;
                case 3:
                    fadeIn.Begin();
                    slideshow.Source = new ImageSourceConverter().ConvertFromString(slide3) as ImageSource;
                    slidelink = ImgData[2, 2];
                    fadeOut.Begin();
                    break;
                case 4:
                    fadeIn.Begin();
                    slideshow.Source = new ImageSourceConverter().ConvertFromString(slide4) as ImageSource;
                    slidelink = ImgData[3, 2];
                    fadeOut.Begin();
                    break;
                case 5:
                    fadeIn.Begin();
                    slideshow.Source = new ImageSourceConverter().ConvertFromString(slide5) as ImageSource;
                    slidelink = ImgData[4, 2];
                    fadeOut.Begin();
                    break;
            }
        }

        private void SetImgData()
        {
            string SlidesLink = (n_Configs.Slides) + "?vlph=" + Guid.NewGuid().ToString();
            ImgData = getImgData(SlidesLink);
            for (int i = 0; i < 5; i++)
            {
                if (ImgData[i, 0] != null)
                {
                    if (i == 0) slide1 = ImgData[i, 0];
                    else if (i == 1) slide2 = ImgData[i, 0];
                    else if (i == 2) slide3 = ImgData[i, 0];
                    else if (i == 3) slide4 = ImgData[i, 0];
                    else if (i == 4) slide5 = ImgData[i, 0];
                }
            }
        }

        private String[,] getImgData(String channel)
        {
            try
            {
                System.Net.WebRequest myRequestImg = System.Net.WebRequest.Create(channel);
                System.Net.WebResponse myResponseImg = myRequestImg.GetResponse();
                System.IO.Stream rssStreamImg = myResponseImg.GetResponseStream();
                System.Xml.XmlDocument rssDocImg = new System.Xml.XmlDocument();
                rssDocImg.Load(rssStreamImg);
                System.Xml.XmlNodeList rssItemsImg = rssDocImg.SelectNodes("rss/channel/item");
                String[,] tempRssImgData = new String[100, 3];
                for (int i = 0; i < rssItemsImg.Count; i++)
                {
                    System.Xml.XmlNode rssNodeImg;
                    rssNodeImg = rssItemsImg.Item(i).SelectSingleNode("image");
                    tempRssImgData[i, 0] = rssNodeImg != null ? rssNodeImg.InnerText : "";
                    rssNodeImg = rssItemsImg.Item(i).SelectSingleNode("pubDate");
                    tempRssImgData[i, 1] = rssNodeImg != null ? rssNodeImg.InnerText : "";
                    rssNodeImg = rssItemsImg.Item(i).SelectSingleNode("link");
                    tempRssImgData[i, 2] = rssNodeImg != null ? rssNodeImg.InnerText : "";
                }
                return tempRssImgData;
            }
            catch (Exception ex4)
            {
                MessageBox.Show($"Xảy ra lỗi: {ex4.Message}", "Lỗi");
                return new String[100, 3];
            }
        }

        private void SetRSS()
        {
            News1.Content = ""; News2.Content = ""; News3.Content = ""; News4.Content = "";
            Event1.Content = ""; Event2.Content = ""; Event3.Content = "";
            News1.Visibility = Visibility.Hidden; News2.Visibility = Visibility.Hidden;
            News3.Visibility = Visibility.Hidden; News4.Visibility = Visibility.Hidden;
            Event1.Visibility = Visibility.Hidden; Event2.Visibility = Visibility.Hidden;
            Event3.Visibility = Visibility.Hidden;

            string NewsLink = ((n_Configs.News) + "?vlph=" + Guid.NewGuid().ToString());
            rssData = getRssData(NewsLink);
            for (int i = 0; i < 7; i++)
            {
                if (rssData[i, 0] != null)
                {
                    if (i == 0) { News1.Visibility = Visibility.Visible; News1.Content = "      " + rssData[i, 0]; }
                    else if (i == 1) { News2.Visibility = Visibility.Visible; News2.Content = "      " + rssData[i, 0]; }
                    else if (i == 2) { News3.Visibility = Visibility.Visible; News3.Content = "      " + rssData[i, 0]; }
                    else if (i == 3) { News4.Visibility = Visibility.Visible; News4.Content = "      " + rssData[i, 0]; }
                    else if (i == 4) { Event1.Visibility = Visibility.Visible; Event1.Content = "      " + rssData[i, 0]; }
                    else if (i == 5) { Event2.Visibility = Visibility.Visible; Event2.Content = "      " + rssData[i, 0]; }
                    else if (i == 6) { Event3.Visibility = Visibility.Visible; Event3.Content = "      " + rssData[i, 0]; }
                }
            }
        }

        private String[,] getRssData(String channel)
        {
            try
            {
                System.Net.WebRequest myRequest = System.Net.WebRequest.Create(channel);
                System.Net.WebResponse myResponse = myRequest.GetResponse();
                System.IO.Stream rssStream = myResponse.GetResponseStream();
                System.Xml.XmlDocument rssDoc = new System.Xml.XmlDocument();
                rssDoc.Load(rssStream);
                System.Xml.XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/item");
                String[,] tempRssData = new String[100, 3];
                for (int i = 0; i < rssItems.Count; i++)
                {
                    System.Xml.XmlNode rssNode;
                    rssNode = rssItems.Item(i).SelectSingleNode("content");
                    tempRssData[i, 0] = rssNode != null ? rssNode.InnerText : "";
                    rssNode = rssItems.Item(i).SelectSingleNode("pubDate");
                    tempRssData[i, 1] = rssNode != null ? rssNode.InnerText : "";
                    rssNode = rssItems.Item(i).SelectSingleNode("link");
                    tempRssData[i, 2] = rssNode != null ? rssNode.InnerText : "";
                }
                return tempRssData;
            }
            catch (Exception ex5)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainNotification mn = new MainNotification($"Xảy ra lỗi: {ex5.Message}");
                    mn.OkButtonClicked += (s, a) => { mn.Close(); Thread.Sleep(100); this.Close(); };
                    mn.Show();
                }));
                return new String[100, 3];
            }
        }

        public void SetUpdatingState()
        {
            Start_Animation_ProgressBar();
            this.InSide_login.Visibility = Visibility.Collapsed;
            this.btnLogin.IsEnabled = false;
            this.btnLogin.Visibility = Visibility.Collapsed;
        }

        public void SetTryAgainState()
        {
            this.btnLogin.IsEnabled = false;
            this.btnLogin.Visibility = Visibility.Collapsed;
        }

        public void SetUpdateCompletedState()
        {
            this.InSide_login.Visibility = Visibility.Visible;
            MyImage.Visibility = Visibility.Collapsed;
            this.btnThulai.IsEnabled = true;
            this.btnThulai.Visibility = Visibility.Visible;
            this.btnLogin.IsEnabled = true;
            this.btnLogin.Visibility = Visibility.Visible;
        }

        private void MainBgr_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_IsPressed = e.ChangedButton == MouseButton.Left;
        }

        private void MainBgr_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            m_IsPressed = false;
        }

        private void MainBgr_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_IsPressed) this.DragMove();
        }

        public delegate void ChangeTextsSafe(double length, double percent);

        private void LblStatus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckPort(n_Configs.GSIP, int.Parse(n_Configs.GSPort), txtStatus);
        }

        private void TxtStatus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckPort(n_Configs.GSIP, int.Parse(n_Configs.GSPort), txtStatus);
        }

        private void BtnAutotrain_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (SysFile.Exists(this.n_Configs.AutoTrain))
            {
                var autotrain = new ProcessStartInfo(this.n_Configs.AutoTrain);
                autotrain.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(autotrain);
            }
            else
            {
                MainNotification mn = new MainNotification(
                    "Không tìm thấy file " + this.n_Configs.AutoTrain + ". Lỗi sai đường dẫn trong thư mục game");
                mn.OkButtonClicked += (s, a) => { mn.Close(); this.IsEnabled = true; };
                mn.Show();
            }
        }

        private void BtnAutoPK_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (SysFile.Exists("AutoPk\\" + this.n_Configs.AutoPK))
            {
                var autopk = new ProcessStartInfo(this.n_Configs.AutoPK);
                autopk.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "AutoPk\\";
                Process.Start(autopk);
            }
            else
            {
                MainNotification mn = new MainNotification(
                    "Không tìm thấy file " + this.n_Configs.AutoPK + ". Lỗi sai đường dẫn trong thư mục game");
                mn.OkButtonClicked += (s, a) => { mn.Close(); this.IsEnabled = true; };
                mn.Show();
            }
        }

        private void Slideshow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (slidelink == null)
            {
                MessageBox.Show("Chưa tìm thấy đường dẫn. Vui lòng thử lại", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            Process.Start(slidelink);
        }

        private void Minimize()
        {
            WindowState = WindowState.Minimized;
            if (WindowState.Minimized == this.WindowState)
            {
                ni.Visible = true;
                Hide();
                ni.ShowBalloonTip(1000, "Thông Báo",
                    "Đang chạy chế độ ẩn!\n\nBấm vào biểu tượng ở thanh tác vụ để mở.",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void News1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[0, 2] != null) Process.Start(rssData[0, 2]); }
        private void News2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[1, 2] != null) Process.Start(rssData[1, 2]); }
        private void News3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[2, 2] != null) Process.Start(rssData[2, 2]); }
        private void News4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[3, 2] != null) Process.Start(rssData[3, 2]); }
        private void Event1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[4, 2] != null) Process.Start(rssData[4, 2]); }
        private void Event2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[5, 2] != null) Process.Start(rssData[5, 2]); }
        private void Event3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (rssData[6, 2] != null) Process.Start(rssData[6, 2]); }

        private void BtnHome_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        { Process.Start(this.n_Configs.WebHome); }
        private void BtnReg_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        { Process.Start(this.n_Configs.WebRegister); }
        private void BtnSupport_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        { Process.Start(this.n_Configs.WebSupport); }
        private void BtnCom_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        { Process.Start(this.n_Configs.WebCom); }

        private void BtnConfig_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            isMainWindowEnabled = false;
            GameConfig OptionForm = new GameConfig();
            OptionForm.Owner = this;
            OptionForm.ShowInTaskbar = false;
            OptionForm.Show();
            OptionForm.Closed += (senderObj, args) =>
            {
                isMainWindowEnabled = true;
                if (isMainWindowEnabled) this.IsEnabled = true;
            };
        }

        private void BtnThulai_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(this.KiemTraUpdateXml)).Start();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            MainNotification mn = new MainNotification("Bạn có chắc chắn muốn thoát ?");
            mn.OkButtonClicked += (senderObj, args) =>
            {
                mn.Close();
                this.IsEnabled = true;
                Thread.Sleep(100);
                this.Close();
            };
            mn.Show();
            this.IsEnabled = false;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Minimize();
        }

        public delegate void DownloadCompleteSafe(bool cancelled);
    }
}
