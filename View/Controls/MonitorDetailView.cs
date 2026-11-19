using PlcTest.View.Support;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PlcTest.View.Controls
{
    /// <summary>
    /// 详情页：把监视表里的每个条目展开成一块矩阵，纵向排列、可滚动。
    /// <para>
    /// 它**不发起任何读取**——数据来自 <see cref="MonitorGrid.RowUpdated"/>。
    /// 如果让详情页自己再读一遍，同一批地址就会被打两次，轮询周期直接翻倍。
    /// </para>
    /// <para>
    /// 默认显示全部区域（联调时经常要同时盯两组），勾「仅显示选中区域」才收成一个。
    /// </para>
    /// </summary>
    public partial class MonitorDetailView : UserControl
    {
        /// <summary>被选中区块的底色</summary>
        private static readonly Color SelectedBack = Color.FromArgb(232, 242, 252);

        /// <summary>区块列表，与监视条目一一对应</summary>
        private readonly List<MonitorRegionBlock> _blocks = new List<MonitorRegionBlock>();
        /// <summary>当前条目快照，用来判断结构是否变化、要不要重建</summary>
        private readonly List<MonitorItem> _items = new List<MonitorItem>();

        /// <summary>选中的区块序号，-1 表示未选中</summary>
        private int _selectedIndex = -1;

        /// <summary>构造：绑选项与尺寸变化</summary>
        public MonitorDetailView()
        {
            InitializeComponent();

            EnableDoubleBuffer(this);
            EnableDoubleBuffer(_flow);

            _chkOnlySelected.CheckedChanged += (s, e) => ApplyFilter();
            _flow.SizeChanged += (s, e) => ResizeBlocks();
        }

        /// <summary>区块数量（测试用）</summary>
        public int RegionCount
        {
            get { return _blocks.Count; }
        }

        /// <summary>当前选中的区块序号，-1 表示未选中</summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        /// <summary>是否只显示选中区域</summary>
        public bool OnlySelected
        {
            get { return _chkOnlySelected.Checked; }
            set { _chkOnlySelected.Checked = value; }
        }

        /// <summary>取某一块（测试用）</summary>
        /// <param name="index">序号</param>
        public MonitorRegionBlock GetRegion(int index)
        {
            if (index < 0 || index >= _blocks.Count)
            {
                return null;
            }
            return _blocks[index];
        }

        /// <summary>
        /// 按条目重建全部区块。条目内容没变时直接返回，避免每轮刷新都重建控件。
        /// </summary>
        /// <param name="items">当前监视条目，可为 null</param>
        public void Rebuild(IReadOnlyList<MonitorItem> items)
        {
            var next = new List<MonitorItem>();
            if (items != null)
            {
                foreach (MonitorItem item in items)
                {
                    next.Add(item);
                }
            }

            if (IsSameAsCurrent(next))
            {
                // 结构没变也要跟着容器调一次宽：
                // 页签刚切过来时 _flow 可能还没布局完，那时算出的宽度是错的。
                ResizeBlocks();
                return;
            }

            _flow.SuspendLayout();
            _flow.Visible = false;
            try
            {
                foreach (MonitorRegionBlock block in _blocks)
                {
                    _flow.Controls.Remove(block);
                    block.Dispose();
                }
                _blocks.Clear();
                _items.Clear();

                foreach (MonitorItem item in next)
                {
                    var block = new MonitorRegionBlock();
                    block.Bind(item);
                    _flow.Controls.Add(block);
                    _blocks.Add(block);
                    _items.Add(item);
                }
            }
            finally
            {
                _flow.ResumeLayout();
                _flow.Visible = true;
            }

            if (_selectedIndex >= _blocks.Count)
            {
                _selectedIndex = -1;
            }

            ResizeBlocks();
            ApplyFilter();
            HighlightSelection();
        }

        /// <summary>
        /// 把某一条目的读取结果刷到对应区块。行号越界（条目刚被删）时静默忽略。
        /// </summary>
        /// <param name="index">条目序号</param>
        /// <param name="result">读取结果</param>
        public void Apply(int index, MonitorReadResult result)
        {
            if (index < 0 || index >= _blocks.Count)
            {
                return;
            }
            _blocks[index].Apply(result);
        }

        /// <summary>
        /// 清空所有区块的读数。停止自动刷新时调用：
        /// 不再变化的数字留在屏幕上，比空着更容易被当成实时值。
        /// </summary>
        public void ClearValues()
        {
            foreach (MonitorRegionBlock block in _blocks)
            {
                block.ClearValues();
            }
        }

        /// <summary>
        /// 定位到某个区块：选中、展开并滚动到可见位置。双击监视表的行时调用。
        /// </summary>
        /// <param name="index">条目序号</param>
        public void FocusRegion(int index)
        {
            if (index < 0 || index >= _blocks.Count)
            {
                return;
            }

            _selectedIndex = index;
            ApplyFilter();

            MonitorRegionBlock block = _blocks[index];
            block.SetExpanded(true);
            _flow.ScrollControlIntoView(block);
            HighlightSelection();
        }

        #region 内部

        /// <summary>打开控件及其容器的双缓冲。</summary>
        private static void EnableDoubleBuffer(Control control)
        {
            if (control == null)
            {
                return;
            }

            PropertyInfo property = typeof(Control).GetProperty(
                "DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                property.SetValue(control, true, null);
            }
        }

        /// <summary>条目内容是否与当前一致（一致就不用重建控件）。</summary>
        private bool IsSameAsCurrent(List<MonitorItem> next)
        {
            if (next.Count != _items.Count)
            {
                return false;
            }

            for (int i = 0; i < next.Count; i++)
            {
                MonitorItem oldItem = _items[i];
                MonitorItem newItem = next[i];
                if (oldItem.AddressText != newItem.AddressText
                    || oldItem.Kind != newItem.Kind
                    || oldItem.Length != newItem.Length)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>按「仅显示选中区域」决定各区块是否可见。</summary>
        private void ApplyFilter()
        {
            bool only = _chkOnlySelected.Checked;
            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].Visible = !only || i == _selectedIndex;
            }

            // 勾了「只看选中」但还没选中时，至少留一块——全空会让人以为坏了
            if (only && _selectedIndex < 0 && _blocks.Count > 0)
            {
                _blocks[0].Visible = true;
            }

            ResizeBlocks();
        }

        /// <summary>给选中的区块换底色，其余恢复。</summary>
        private void HighlightSelection()
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                _blocks[i].BackColor = i == _selectedIndex ? SelectedBack : _flow.BackColor;
            }
        }

        /// <summary>让每个区块的宽度跟随容器。</summary>
        private void ResizeBlocks()
        {
            // 容器还没布局出尺寸时不要硬算：否则会按 200 的下限把块压扁，
            // 12 列每列只剩十几像素，数字全被裁掉，而 SizeChanged 之后未必再来一次。
            if (_flow.ClientSize.Width <= 0)
            {
                return;
            }

            int width = _flow.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8;
            if (width < 200)
            {
                width = 200;
            }

            foreach (MonitorRegionBlock block in _blocks)
            {
                if (block.Width != width)
                {
                    block.Width = width;
                }
            }
        }

        #endregion
    }
}
