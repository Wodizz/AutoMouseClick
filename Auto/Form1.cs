using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMouseClick
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        const int MOUSEEVENTF_MOVE = 0x0001;      //移动鼠标 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTUP = 0x0004; //模拟鼠标左键抬起 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020; //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040; //模拟鼠标中键抬起 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; //标示是否采用绝对坐标

        private RegisterHotKeyClass recordHotKey = new RegisterHotKeyClass();
        private RegisterHotKeyClass stopHotKey = new RegisterHotKeyClass();
        float clickTime;
        float loopTime;
        int loopCount;
        int mywidth;
        int myheight;

      
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 绑定与开启热键监听
            recordHotKey.Keys = Keys.F10;
            recordHotKey.WindowHandle = this.Handle;
            recordHotKey.HotKey += F2KeyClicked;
            recordHotKey.StarHotKey();

            stopHotKey.Keys = Keys.F11;
            stopHotKey.WindowHandle = this.Handle;
            stopHotKey.HotKey += F3KeyClicked;
            stopHotKey.StarHotKey();

            mywidth = Screen.PrimaryScreen.Bounds.Width;
            myheight = Screen.PrimaryScreen.Bounds.Height;
        }

        private void ShowWindow()
        {
            WodiManager.ProgramState = E_State.Normal;
            this.Visible = true;
            this.notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void HideWindow()
        {
            this.notifyIcon1.Visible = true;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // 获取设置的时间
            try
            {
                clickTime = float.Parse(this.textBox1.Text);
                loopTime = float.Parse(this.textBox2.Text);
                loopCount = int.Parse(this.textBox4.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入大于0的合法数字");
                return;
            }
            if (WodiManager.ClickDataList.Count == 0)
            {
                MessageBox.Show("请先进行记录操作");
                return;
            }
            this.notifyIcon1.ShowBalloonTip(1000, "开始运行", "按F11停止，", ToolTipIcon.Info);
            // 开启点击定时器
            this.clickTimer.Enabled = true;
            // 关闭循环定时器
            this.loopTimer.Enabled = false;
            // 设置点击定时器时间
            this.clickTimer.Interval = (int)clickTime * 1000;
            // 设置循环定时器时间
            this.loopTimer.Interval = (int)loopTime * 1000;
            // 数据索引设置为0
            dataIndex = 0;
            // 循环次数设置为0
            loopIndex = 0;
            WodiManager.ProgramState = E_State.Running;
            HideWindow();
        }

        private void buttonRecord_Click(object sender, EventArgs e)
        {
            WodiManager.ProgramState = E_State.Recording;
            this.notifyIcon1.ShowBalloonTip(1000, "开始记录", "请将鼠标移至记录点后按F10", ToolTipIcon.Info);
            HideWindow();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            WodiManager.ClickDataList.Clear();
            this.listBox1.Items.Clear();
        }


        private void F2KeyClicked()
        {
            if (WodiManager.ProgramState == E_State.Recording)
            {
                // 添加鼠标位置与点击模式
                if (this.radioButtonOnce.Checked)
                    WodiManager.ClickDataList.Add(new OnceClickData(E_ClickMode.Once, Cursor.Position));
                else if (this.radioButtonDouble.Checked)
                    WodiManager.ClickDataList.Add(new OnceClickData(E_ClickMode.Double, Cursor.Position));
                // 最大化窗口
                ShowWindow();
                // 添加位置进listbox
                this.listBox1.Items.Add("记录位置 x:" + Cursor.Position.X + " y:" + Cursor.Position.Y + 
                   (WodiManager.ClickDataList.Last().ClickMode == E_ClickMode.Once ? " 单击" : " 双击"));
            }
        }

        private void F3KeyClicked()
        {
            if (WodiManager.ProgramState == E_State.Running)
            {
                ShowWindow();
            }
        }

        private int dataIndex;
        private Point dataPoint;
        private void clickTimer_Tick(object sender, EventArgs e)
        {
            if (WodiManager.ProgramState != E_State.Running)
            {
                this.clickTimer.Enabled = false;
                return;
            }
            // 遍历到最后 重置进入循环计时器
            if (dataIndex > WodiManager.ClickDataList.Count - 1)
            {
                dataIndex = 0;
                this.clickTimer.Enabled = false;
                this.loopTimer.Enabled = true;
                return;
            }
            dataPoint = WodiManager.ClickDataList[dataIndex].CursorPos;
            /// 注意：在鼠标坐标系统中，屏幕在水平和垂直方向上均匀分割成65535×65535个单元。
            /// 目标点在屏幕上横坐标和纵坐标需要转化为鼠标坐标系统中的横坐标和纵坐标。
            switch (WodiManager.ClickDataList[dataIndex].ClickMode)
            {
                case E_ClickMode.Once:
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                        dataPoint.X * 65536 / mywidth, dataPoint.Y * 65536 / myheight, 0, 0);
                    break;
                case E_ClickMode.Double:
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                         dataPoint.X * 65536 / mywidth, dataPoint.Y * 65536 / myheight, 0, 0);
                    break;
                default:
                    break;
            }
            dataIndex++;
        }

        private int loopIndex;
        private void loopTimer_Tick(object sender, EventArgs e)
        {
            loopIndex++;
            this.loopTimer.Enabled = false;
            if (WodiManager.ProgramState != E_State.Running)
                return;
            if (loopIndex > loopCount)
            {
                this.notifyIcon1.ShowBalloonTip(1000, "本次脚本完成", "关注嘉然然，顿顿解馋馋", ToolTipIcon.Info);
                ShowWindow();
            }
            else
                clickTimer.Enabled = true;
        }

        /// <summary>
        /// 注册系统热键
        /// </summary>
        public class RegisterHotKeyClass
        {

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterHotKey(IntPtr wnd, int id, MODKEY mode, Keys vk);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UnregisterHotKey(IntPtr wnd, int id);

            [DllImport("Kernel32.dll")]
            public extern static int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buf, int size, ref IntPtr args);

            private IntPtr m_WindowHandle = IntPtr.Zero;
            private MODKEY m_ModKey = MODKEY.MOD_CONTROL;
            private Keys m_Keys = Keys.A;
            private int m_WParam = 10000;
            private bool Star = false;
            private HotKeyWndProc m_HotKeyWnd = new HotKeyWndProc();

            public IntPtr WindowHandle
            {
                get { return m_WindowHandle; }
                set { if (Star) return; m_WindowHandle = value; }
            }
            public MODKEY ModKey
            {
                get { return m_ModKey; }
                set { if (Star) return; m_ModKey = value; }
            }
            public Keys Keys
            {
                get { return m_Keys; }
                set { if (Star) return; m_Keys = value; }
            }
            public int WParam
            {
                get { return m_WParam; }
                set { if (Star) return; m_WParam = value; }
            }

            /// <summary>
            /// 错误调试方法 根据win32错误码获得对应错误信息
            /// </summary>
            public static string GetSysErrMsg(int errCode)
            {
                IntPtr tempptr = IntPtr.Zero;
                string msg = null;
                FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
                return msg;
            }

            public void StarHotKey()
            {
                if (m_WindowHandle != IntPtr.Zero)
                {
                    bool tb = RegisterHotKey(m_WindowHandle, m_WParam, 0, m_Keys);
                    //捕获错误信息
                    int error = Marshal.GetLastWin32Error();
                    string errorMsg = GetSysErrMsg(error);
                    if (!tb)
                    {
                        throw new Exception(errorMsg);
                    }
                    try
                    {
                        m_HotKeyWnd.m_HotKeyPass = new HotKeyPass(KeyPass);
                        m_HotKeyWnd.m_WParam = m_WParam;
                        m_HotKeyWnd.AssignHandle(m_WindowHandle);
                        Star = true;
                    }
                    catch
                    {
                        StopHotKey();
                    }
                }
            }
            private void KeyPass()
            {
                if (HotKey != null) HotKey();
            }
            public void StopHotKey()
            {
                if (Star)
                {
                    if (!UnregisterHotKey(m_WindowHandle, m_WParam))
                    {
                        throw new Exception("卸载失败！");
                    }
                    Star = false;
                    m_HotKeyWnd.ReleaseHandle();
                }
            }


            public delegate void HotKeyPass();
            public event HotKeyPass HotKey;


            private class HotKeyWndProc : NativeWindow
            {
                public int m_WParam = 10000;
                public HotKeyPass m_HotKeyPass;
                protected override void WndProc(ref Message m)
                {
                    if (m.Msg == 0x0312 && m.WParam.ToInt32() == m_WParam)
                    {
                        if (m_HotKeyPass != null) m_HotKeyPass.Invoke();
                    }

                    base.WndProc(ref m);
                }
            }

            public enum MODKEY
            {
                MOD_ALT = 0x0001,
                MOD_CONTROL = 0x0002,
                MOD_SHIFT = 0x0004,
                MOD_WIN = 0x0008,
            }


        }

        
    }
}
