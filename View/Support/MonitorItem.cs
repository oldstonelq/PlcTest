using PlcTest.Models;

namespace PlcTest.View.Support
{
    /// <summary>
    /// 一条监视条目：地址文本、位/字方式与长度。
    /// <para>
    /// 详情页靠它决定「这一块怎么排」——是不是按位、排几行几列、行头从哪个地址起。
    /// 之所以不直接解析表格单元格：单元格随时可编辑，而条目是「这一轮要读什么」的快照，
    /// 拿它排版才不会出现「标题还是旧地址、值已经按新地址读回来」的错位。
    /// </para>
    /// </summary>
    public sealed class MonitorItem
    {
        /// <summary>用户填的地址文本，如 <c>D100</c>、<c>DB1.DBW0</c></summary>
        public string AddressText { get; private set; }

        /// <summary>
        /// 解析后的地址；文本非法时为 null。
        /// 为 null 不影响条目存在——那一行只是读不出来，详情页照样给它留一块并显示原因。
        /// </summary>
        public PlcAddress Address { get; private set; }

        /// <summary>按位还是按字</summary>
        public AddressValueKind Kind { get; private set; }

        /// <summary>点数（按位）或字数（按字）</summary>
        public int Length { get; private set; }

        /// <summary>构造</summary>
        /// <param name="addressText">地址文本</param>
        /// <param name="kind">位/字</param>
        /// <param name="length">长度，小于 1 时按 1 处理</param>
        /// <param name="address">解析结果，可为 null</param>
        public MonitorItem(string addressText, AddressValueKind kind, int length, PlcAddress address = null)
        {
            AddressText = addressText ?? string.Empty;
            Kind = kind;
            Length = length < 1 ? 1 : length;
            Address = address;
        }

        /// <summary>是否按位监视</summary>
        public bool IsBit
        {
            get { return Kind == AddressValueKind.Bit; }
        }
    }
}
