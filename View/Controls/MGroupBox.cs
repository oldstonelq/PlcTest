using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlcTest.View.Controls
{
    /// <summary>
    /// 可自定边框颜色的分组框。GroupBox 的边框颜色由系统主题决定、
    /// 没有公开属性可改，所以这里在 OnPaint 里整块自绘。
    /// </summary>
    public partial class MGroupBox : GroupBox
    {
        /// <summary>边框颜色</summary>
        private Color _borderColor = Color.Black;

        /// <summary>边框颜色，赋值后立即重绘。</summary>
        [Browsable(true), Description("边框颜色"), Category("自定义分组")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>构造</summary>
        public MGroupBox()
        {
            //SetStyle中的AllPaintingInWmPaint可以减少控件重绘次数,不会导致控件闪烁
            //但是必须是UserPaint为true时才有用,UserPaint为true时需要用户重绘控件
            this.SetStyle(ControlStyles.UserPaint
                        | ControlStyles.ResizeRedraw
                        | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.DoubleBuffer, true);
            InitializeComponent();
        }

        /// <summary>构造，并把自身注册到设计器容器里</summary>
        /// <param name="container">组件容器，设计器传入</param>
        public MGroupBox(IContainer container)
        {
            //SetStyle中的AllPaintingInWmPaint可以减少控件重绘次数,不会导致控件闪烁
            //但是必须是UserPaint为true时才有用,UserPaint为true时需要用户重绘控件
            this.SetStyle(ControlStyles.UserPaint
                        | ControlStyles.ResizeRedraw
                        | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.DoubleBuffer, true);
            container.Add(this);

            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            // 1. 无需调用 base.OnPaint，完全自己画
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            // 防锯齿（让边框和文字边缘更平滑，工业软件视觉更细腻）
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 测量标题尺寸
            SizeF FontSize = g.MeasureString(this.Text, this.Font);

            // 2. 画标题文字（Y坐标自适应字体高度，避免大字体被裁剪）
            float textY = (this.Font.Height - FontSize.Height) / 2;
            if (textY < 0) textY = 0;
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, 10, textY);
            }

            // 3. 画边框（用 using 确保 Pen 释放）
            using (Pen pen = new Pen(this._borderColor))
            {
                float lineY = FontSize.Height / 2 + textY; // 让边框线对齐文字中间

                // 字左侧的实线
                g.DrawLine(pen, 1, lineY, 8, lineY);
                // 字右侧的实线
                g.DrawLine(pen, FontSize.Width + 8, lineY, this.Width - 2, lineY);
                // 左侧竖线
                g.DrawLine(pen, 1, lineY, 1, this.Height - 2);
                // 底部横线
                g.DrawLine(pen, 1, this.Height - 2, this.Width - 2, this.Height - 2);
                // 右侧竖线
                g.DrawLine(pen, this.Width - 2, lineY, this.Width - 2, this.Height - 2);
            }
        }
    }
}
