using PlcTest.View.Support;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PlcTest.View.Controls
{
    /// <summary>
    /// 详情页里的一个区域区块：标题栏 + 「行头 = 起始地址、列头 = 偏移、格子 = 值」的矩阵。
    /// <para>
    /// 列数按位/字区分：字 12 列、位 16 列（一个字的 16 位正好一行，和位软元件的编号节奏一致）。
    /// </para>
    /// <para>
    /// 行头写成 <c>D100</c> / <c>D100+12</c> 而不是算好的完整地址，是因为
    /// <c>PlcAddress.Address</c> 的单位在不同协议里不一样——三菱是字编号、S7 是字节偏移。
    /// 在这里替它们推算地址，必然有一家算错；不如直接给出「相对起始地址第几个点」。
    /// </para>
    /// </summary>
    public partial class MonitorRegionBlock : UserControl
    {
        /// <summary>按字监视时每行的格子数</summary>
        public const int WordColumns = 12;

        /// <summary>按位监视时每行的格子数（对应一个字的 16 位）</summary>
        public const int BitColumns = 16;

        private const int CellHeight = 24;
        private const int BarHeight = 26;

        private static readonly Color AlternateRow = Color.FromArgb(247, 248, 250);
        private static readonly Color BitOn = Color.FromArgb(15, 110, 86);
        private static readonly Color ErrorText = Color.FromArgb(163, 45, 45);

        private MonitorItem _item;
        private int _columns = WordColumns;
        private int _rowCount;
        private Label[] _cells = new Label[0];

        /// <summary>构造：绑折叠按钮，设置标题加粗</summary>
        public MonitorRegionBlock()
        {
            InitializeComponent();

            _labTitle.Font = new Font(_labTitle.Font, FontStyle.Bold);
            _btnToggle.Click += (s, e) => SetExpanded(!IsExpanded);
            SetExpanded(true);
        }

        /// <summary>这一块对应的监视条目</summary>
        public MonitorItem Item
        {
            get { return _item; }
        }

        /// <summary>矩阵是否展开</summary>
        public bool IsExpanded
        {
            get { return _matrix.Visible; }
        }

        /// <summary>矩阵的行数（不含列头行）</summary>
        public int RowCount
        {
            get { return _rowCount; }
        }

        /// <summary>矩阵的列数（不含行头列）</summary>
        public int ColumnCount
        {
            get { return _columns; }
        }

        /// <summary>
        /// 绑定条目并重建矩阵。地址、长度或位/字发生变化时调用。
        /// </summary>
        /// <param name="item">监视条目，为 null 时显示「未填地址」</param>
        public void Bind(MonitorItem item)
        {
            _item = item;
            _columns = item != null && item.IsBit ? BitColumns : WordColumns;
            _rowCount = item == null ? 0 : (item.Length + _columns - 1) / _columns;

            _labTitle.ForeColor = SystemColors.ControlText;
            _labTitle.Text = BuildTitle();
            RebuildMatrix();
            AdjustHeight();
        }

        /// <summary>
        /// 把一个读取结果刷到格子上。只改动文本有变化的格子，避免整块重绘闪烁。
        /// </summary>
        /// <param name="result">读取结果；null 或失败时清空格子并显示原因</param>
        public void Apply(MonitorReadResult result)
        {
            if (_cells.Length == 0)
            {
                return;
            }

            if (result == null || !result.Success)
            {
                _matrix.SuspendLayout();
                try
                {
                    for (int i = 0; i < _cells.Length; i++)
                    {
                        _cells[i].Text = string.Empty;
                        _cells[i].ForeColor = SystemColors.ControlText;
                    }
                }
                finally
                {
                    _matrix.ResumeLayout();
                }

                _labTitle.ForeColor = ErrorText;
                _labTitle.Text = BuildTitle() + "  ·  " + (result == null ? "无结果" : result.Error);
                return;
            }

            if (_labTitle.ForeColor != SystemColors.ControlText)
            {
                _labTitle.ForeColor = SystemColors.ControlText;
                _labTitle.Text = BuildTitle();
            }

            _matrix.SuspendLayout();
            try
            {
                for (int i = 0; i < _cells.Length; i++)
                {
                    Label cell = _cells[i];
                    bool beyond = _item == null || i >= _item.Length;

                    if (beyond)
                    {
                        // 末行不满的格子留空：填 0 会让人以为真的读到 0
                        if (cell.Text.Length != 0)
                        {
                            cell.Text = string.Empty;
                        }
                        continue;
                    }

                    if (result.Bits != null)
                    {
                        bool on = i < result.Bits.Length && result.Bits[i];
                        string text = on ? "1" : "0";
                        if (cell.Text != text)
                        {
                            cell.Text = text;
                        }
                        cell.ForeColor = on ? BitOn : SystemColors.ControlText;
                    }
                    else if (result.Words != null)
                    {
                        string text = i < result.Words.Length ? result.Words[i].ToString() : string.Empty;
                        if (cell.Text != text)
                        {
                            cell.Text = text;
                        }
                    }
                }
            }
            finally
            {
                _matrix.ResumeLayout();
            }
        }

        /// <summary>
        /// 读某个格子的文本（测试用）。index 按行优先展开，越界返回 null。
        /// </summary>
        /// <param name="index">格子序号</param>
        public string GetCellText(int index)
        {
            if (index < 0 || index >= _cells.Length)
            {
                return null;
            }
            return _cells[index].Text;
        }

        /// <summary>展开或折叠矩阵</summary>
        /// <param name="expanded">true 展开</param>
        public void SetExpanded(bool expanded)
        {
            _matrix.Visible = expanded;
            _btnToggle.Text = expanded ? "收起" : "展开";
            AdjustHeight();
        }

        #region 内部

        private string BuildTitle()
        {
            if (_item == null || _item.AddressText.Length == 0)
            {
                return "未填地址";
            }

            string unit = _item.IsBit ? "个点" : "个字";
            return string.Format("{0} 起 {1} {2}", _item.AddressText, _item.Length, unit);
        }

        private void AdjustHeight()
        {
            if (!IsExpanded)
            {
                Height = BarHeight + Padding.Bottom;
                return;
            }

            // 问控件本身要多高，而不是自己乘行数：
            // CellBorderStyle 的边框也要占位置，手算差几个像素就会把最后一行裁掉。
            int width = Width > 0 ? Width : 600;
            Size preferred = _matrix.GetPreferredSize(new Size(width, 0));
            int body = preferred.Height > 0 ? preferred.Height : (_rowCount + 1) * CellHeight;
            Height = BarHeight + body + Padding.Bottom;
        }

        private void RebuildMatrix()
        {
            _matrix.SuspendLayout();
            try
            {
                // 不能 foreach 边遍历边 Dispose：Dispose 会把自己从集合里摘掉，
                // 枚举器随即失效。逐个摘到空为止。
                while (_matrix.Controls.Count > 0)
                {
                    _matrix.Controls[0].Dispose();
                }
                _matrix.ColumnStyles.Clear();
                _matrix.RowStyles.Clear();

                _matrix.ColumnCount = _columns + 1;
                _matrix.RowCount = _rowCount + 1;

                _matrix.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                for (int c = 0; c < _columns; c++)
                {
                    _matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / _columns));
                }

                for (int r = 0; r <= _rowCount; r++)
                {
                    _matrix.RowStyles.Add(new RowStyle(SizeType.Absolute, CellHeight));
                }

                _matrix.Controls.Add(MakeCaption("偏移", true), 0, 0);
                for (int c = 0; c < _columns; c++)
                {
                    _matrix.Controls.Add(MakeCaption(c.ToString(), true), c + 1, 0);
                }

                _cells = new Label[_rowCount * _columns];
                for (int r = 0; r < _rowCount; r++)
                {
                    _matrix.Controls.Add(MakeCaption(RowHeader(r), false), 0, r + 1);

                    for (int c = 0; c < _columns; c++)
                    {
                        Label cell = MakeCell(r % 2 == 1);
                        _cells[r * _columns + c] = cell;
                        _matrix.Controls.Add(cell, c + 1, r + 1);
                    }
                }
            }
            finally
            {
                _matrix.ResumeLayout();
            }
        }

        private string RowHeader(int rowIndex)
        {
            if (_item == null || _item.AddressText.Length == 0)
            {
                return string.Empty;
            }

            int offset = rowIndex * _columns;
            return offset == 0 ? _item.AddressText : _item.AddressText + "+" + offset;
        }

        private static Label MakeCaption(string text, bool header)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                TextAlign = header ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleRight,
                ForeColor = header ? SystemColors.GrayText : SystemColors.ControlText,
                Padding = header ? Padding.Empty : new Padding(4, 0, 4, 0),
            };
        }

        private static Label MakeCell(bool alternate)
        {
            return new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = alternate ? AlternateRow : SystemColors.Window,
            };
        }

        #endregion
    }
}
