using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Ini;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace AutoUpdate
{
    /// <summary>
    /// Interaction logic for GameConfig.xaml
    /// </summary>
    public partial class GameConfig : Window
    {
        private bool m_IsPressed = false;
        private readonly DoubleAnimation _oa;

        public GameConfig()
        {
            InitializeComponent();
            this.MouseDown += new MouseButtonEventHandler(MainBgr_MouseDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(MainBgr_PreviewMouseDown);
            this.PreviewMouseUp += new MouseButtonEventHandler(MainBgr_PreviewMouseUp);

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.IsEnabled = false; // Vô hiệu hoá MainWindow
                mainWindow.Focus(); // Tập trung vào MainWindow (nếu cần)
            }
            _oa = new DoubleAnimation();
            _oa.From = 0;
            _oa.To = 1;
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(0d));

            BeginAnimation(OpacityProperty, _oa);
        }


        private void MainBgr_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            m_IsPressed = false;
        }

        private void MainBgr_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                m_IsPressed = true;
            }
            else
            {
                m_IsPressed = false;
            }
        }

        private void MainBgr_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_IsPressed)
            {
                this.DragMove();
            }
        }


   
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            {
                IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
                string a = iniFile.IniReadValue("Client", "DynamicLight");
                bool flag = a == "0";
                if (flag)
                {
                    this.DynamicLight_cb.IsChecked = false;
                }
                else
                {
                    bool flag2 = a == "1";
                    if (flag2)
                    {
                        this.DynamicLight_cb.IsChecked = true;
                    }
                }

                string a2 = iniFile.IniReadValue("Client", "FullScreen");
                bool flag3 = a2 == "0";
                if (flag3)
                {
                    this.WindowToggle.IsChecked = true;
                }
                else
                {
                    bool flag4 = a2 == "1";
                    if (flag4)
                    {
                        this.FullScreenToggle.IsChecked = true;
                    }
                }
                this.txtCapPath.Text = iniFile.IniReadValue("Client", "CapPath");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _oa.From = 1;
            _oa.To = 0;
            _oa.Completed += new EventHandler((send, ea) =>
            {
                this.Close();
            });
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(0d));

            BeginAnimation(OpacityProperty, _oa);
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            {
                IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
                bool @checked = this.DynamicLight_cb.IsChecked == true;
                if (@checked)
                {
                    iniFile.IniWriteValue("Client", "DynamicLight", "1");

                }
                else
                {
                    bool checked2 = this.DynamicLight_cb.IsChecked == false;
                    if (checked2)
                    {
                        iniFile.IniWriteValue("Client", "DynamicLight", "0");


                    }
                }
                bool checked3 = this.FullScreenToggle.IsChecked == true;
                if (checked3)
                {
                    iniFile.IniWriteValue("Client", "FullScreen", "1");
                }
                else
                {
                    bool checked4 = this.WindowToggle.IsChecked == true;
                    if (checked4)
                    {
                        iniFile.IniWriteValue("Client", "FullScreen", "0");
                    }
                }
                this.Close();
            }
        }

        private void BtnDefault_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnLocate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                txtCapPath.Text = dialog.SelectedPath;
        }

        private void TxtCapPath_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                txtCapPath.Text = dialog.SelectedPath;
        }

        private void Display_mode_cb_Unchecked(object sender, RoutedEventArgs e)
        {
            // Khi CheckBox bị tắt, chạy storyboard giả tạo việc uncheck rồi check lại ngay lập tức
            Storyboard fakeUncheckStoryboard = FindResource("FakeCheckBoxUncheck") as Storyboard;
            fakeUncheckStoryboard.Begin();
        }

    }
}
