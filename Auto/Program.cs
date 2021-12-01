using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMouseClick
{
    /// <summary>
    /// 鼠标点击模式
    /// </summary>
    public enum E_ClickMode
    {
        Once,
        Double,
    }

    /// <summary>
    /// 程序运行状态
    /// </summary>
    public enum E_State
    {
        Normal,
        Recording,
        Running,
    }

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    /// <summary>
    /// 单次点击数据
    /// </summary>
    public class OnceClickData
    {
        private E_ClickMode clickMode;
        private Point cursorPos;

        public E_ClickMode ClickMode => clickMode;
        public Point CursorPos => cursorPos;

        public OnceClickData(E_ClickMode clickMode, Point cursorPos)
        {
            this.clickMode = clickMode;
            this.cursorPos = cursorPos;
        }
    }

    public static class WodiManager
    {
        public static E_State ProgramState = E_State.Normal;
        public static List<OnceClickData> ClickDataList = new List<OnceClickData>();

        
    }
}
