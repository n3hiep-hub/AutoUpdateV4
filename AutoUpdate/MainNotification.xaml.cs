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

namespace AutoUpdate
{
    /// <summary>
    /// Interaction logic for MainNotification.xaml
    /// </summary>
    public partial class MainNotification : Window
    {
        private bool m_IsPressed = false;
        private readonly DoubleAnimation _oa;
        public event EventHandler OkButtonClicked;
        public MainNotification(string data)
        {
            InitializeComponent();
            this.Topmost = true;
            this.MouseDown += new MouseButtonEventHandler(MainBgr_MouseDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(MainBgr_PreviewMouseDown);
            this.PreviewMouseUp += new MouseButtonEventHandler(MainBgr_PreviewMouseUp);
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.IsEnabled = false; // Vô hiệu hoá MainWindow
                mainWindow.Focus(); // Tập trung vào MainWindow (nếu cần)
            }
            Notification_TextBlock.Text = data;
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _oa.From = 1;
            _oa.To = 0;
            _oa.Completed += new EventHandler((send, ea) =>
            {
                this.Close();
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.IsEnabled = true; // Kích hoạt lại MainWindow
                }
            });
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(0d));

            BeginAnimation(OpacityProperty, _oa);

           
        }

        private void btn_ok_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            OkButtonClicked?.Invoke(this, EventArgs.Empty);
            this.Close();
            // Khi MainNotification được đóng
            
        }
    }
}
