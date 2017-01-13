using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CleverChair.model;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using CleverChair.utils;
using System.Windows.Forms;
using System.Drawing;

namespace CleverChair
{


    public partial class MainWindow : Window
    {


        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;
        private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
        private const int APPCOMMAND_MEDIA_PLAY = 0x2E0000;
        private const int APPCOMMAND_MEDIA_PAUSE = 0x2F0000;
        private const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 0x180000;
        private const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 0x2C0000;

        private const string uri = "<firebase url>";

        private volatile bool soundChangingEnabled = false;

        private volatile bool micChangingEnabled = false;

        private volatile bool lockedChangingEnabled = false;

        private volatile bool playStopChangingEnabled = false;

        private volatile bool soundMuted = false;

        private volatile bool micMuted = false;

        private volatile bool locked = false;

        private volatile bool stopped = false;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("user32")]
        public static extern void LockWorkStation();

        private WindowInteropHelper helper;

        private NotifyIcon trayIcon;

        private System.Windows.Forms.ContextMenu trayMenu;

        public MainWindow()
        {
            InitializeComponent();
           
            
            // Create a simple tray menu with only one item.
            trayMenu = new System.Windows.Forms.ContextMenu();
            trayMenu.MenuItems.Add("Открыть", OnOpen);
            trayMenu.MenuItems.Add("Выйти", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.DoubleClick += new System.EventHandler(OnOpen);
            trayIcon.Text = "MyTrayApp";
            trayIcon.Icon = new Icon(Properties.Resources.barbers_chair_32, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        
        helper = new WindowInteropHelper(this);
            init();
            runTask();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Close();
            System.Windows.Application.Current.Shutdown();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            this.Show();
            this.Focus();
            this.WindowState = WindowState.Normal;
        }


        private void init()
        {
            soundMuted = SoundUtils.isMuted(true);
            micMuted = SoundUtils.isMuted(false);
        }

        private void runTask()
        {
          Thread t =   new Thread(() =>
            {
                while (true)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var serializer = new DataContractJsonSerializer(typeof(FirebaseResponseData));
                    FirebaseResponseData data = (FirebaseResponseData)serializer.ReadObject(response.GetResponseStream());
                    tryLock(data);
                    tryToggleSoundPhones(data);
                    tryToggleSoundMic(data);
                    tryTogglePlayStop(data);
                    Thread.Sleep(100);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void tryTogglePlayStop(FirebaseResponseData data)
        {
            if (!playStopChangingEnabled)
            {
                return;
            }
            if (data != null)
            {
                if (!stopped && data.FreeTime > 0)
                {
                    stopped = true;
                    this.Dispatcher.BeginInvoke((Action)(() => SendMessageW(helper.Handle, WM_APPCOMMAND, helper.Handle, (IntPtr)APPCOMMAND_MEDIA_PAUSE)));
                }
                else if (stopped && data.BusyTime > 0)
                {
                    stopped = false;
                    this.Dispatcher.BeginInvoke((Action)(() => SendMessageW(helper.Handle, WM_APPCOMMAND, helper.Handle, (IntPtr)APPCOMMAND_MEDIA_PLAY)));
                }
            }
        }

        private void tryToggleSoundMic(FirebaseResponseData data)
        {
            if (!micChangingEnabled)
            {
                return;
            }
            if (data != null)
            {
                if (micMuted && data.BusyTime > 0)
                {
                    micMuted = false;
                    SoundUtils.Mute(false, false);
                }
                else if (!micMuted && data.FreeTime >= data.MicMuteTimeout)
                {
                    micMuted = true;
                    SoundUtils.Mute(false, true);
                }
            }
        }

        private void tryToggleSoundPhones(FirebaseResponseData data)
        {
            if (!soundChangingEnabled)
            {
                return;
            }

            if (data != null)
            {
                if (soundMuted && data.BusyTime > 0)
                {
                    soundMuted = false;
                    SoundUtils.Mute(true, false);
                } else if (!soundMuted && data.FreeTime >= data.SoundMuteTimeout)
                {
                    soundMuted = true;
                    SoundUtils.Mute(true, true);
                }
            }
        }

        private void tryLock(FirebaseResponseData data)
        {
            if (!lockedChangingEnabled)
            {
                return;
            }
            if (data != null && data.FreeTime >= data.LockTimeout)
            {
                if (!locked)
                {
                    LockWorkStation();
                    locked = true;
                }
            }
            else
            {
                locked = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Receive_Data(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            soundChangingEnabled = true;
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            micChangingEnabled = true;
        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            playStopChangingEnabled = true;
        }

        private void CheckBox_Checked_3(object sender, RoutedEventArgs e)
        {

            lockedChangingEnabled = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                
                case WindowState.Minimized:
                    Hide();
                    
                    break;
                default:
                  
                    Focus();

                    break;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            lockedChangingEnabled = false;
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            playStopChangingEnabled = false;
        }

        private void CheckBox_Unchecked_2(object sender, RoutedEventArgs e)
        {
            micChangingEnabled = false;
        }

        private void CheckBox_Unchecked_3(object sender, RoutedEventArgs e)
        {
            soundChangingEnabled = false;
        }
    }
}
