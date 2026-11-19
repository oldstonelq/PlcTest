using System;
using System.Drawing;
using System.Windows.Forms;

namespace PlcTest.View.Support
{
    /// <summary>
    /// 窗体尺寸适配：设计器里按 1280×820 画，运行时若屏幕工作区装不下就等比缩小。
    /// <para>
    /// 两个工作台是固定尺寸窗体（<c>FixedSingle</c>、不可拖拽），不会自己适应屏幕，
    /// 而 1366×768 的笔记本可用高度只有 700 出头，820 会直接顶出屏幕。
    /// 这里只缩不放：屏幕够大就原样用，避免同一份界面在 4K 屏上被拉散。
    /// </para>
    /// </summary>
    public static class ScreenFit
    {
        /// <summary>装不下时的缩放比例</summary>
        private const float FallbackScale = 0.85f;

        /// <summary>
        /// 按主屏工作区修正尺寸。
        /// </summary>
        /// <param name="preferred">设计尺寸</param>
        /// <returns>放得下就原样返回；放不下则等比缩小并夹到工作区内</returns>
        public static Size Fit(Size preferred)
        {
            Screen screen = Screen.PrimaryScreen;
            if (screen == null)
            {
                return preferred;
            }

            Rectangle area = screen.WorkingArea;
            if (preferred.Width <= area.Width && preferred.Height <= area.Height)
            {
                return preferred;
            }

            int width = Math.Min((int)(preferred.Width * FallbackScale), area.Width);
            int height = Math.Min((int)(preferred.Height * FallbackScale), area.Height);
            return new Size(width, height);
        }
    }
}
