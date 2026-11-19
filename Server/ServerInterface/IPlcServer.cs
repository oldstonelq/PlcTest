using PlcTest.Models;
using System;
using System.Collections.Generic;

namespace PlcTest.Server.ServerInterface
{
    /// <summary>
    /// PLC 服务端契约：描述客户端可读写的软元件，以及界面直接操作模拟内存的入口。
    /// </summary>
    public interface IPlcServer
    {
        /// <summary>服务端是否正在监听。</summary>
        bool ServerIsOpen { get; }

        /// <summary>
        /// 本服务端支持的软元件区域，供界面「软元件视图」列行（与客户端的同名方法对称）。
        /// </summary>
        IReadOnlyList<MemoryArea> GetSupportedAreas();
        /// <summary>当前已连接的客户端数量。</summary>
        int ClientCount { get; }

        /// <summary>启动监听。重复调用是空操作。</summary>
        void OpenServer();

        /// <summary>停止监听并释放监听资源。</summary>
        void CloseServer();

        /// <summary>
        /// 以下 8 个读写方法是界面直接操作模拟内存的入口，不经协议报文解析，
        /// 与客户端 <see cref="PlcTest.Client.ClientInterface.IPlcClient"/> 的同名方法对称。
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        Result<bool> ReadBit(PlcAddress address);
        /// <summary>读连续位</summary>
        /// <param name="address">起始地址，<c>Length</c> 决定点数</param>
        /// <returns>读取结果</returns>
        Result<bool[]> ReadBitArray(PlcAddress address);
        /// <summary>写单个位</summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">位值</param>
        /// <returns>写入结果</returns>
        Result<bool> WriteBit(PlcAddress address, bool value);
        /// <summary>写连续位</summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">位值数组</param>
        /// <returns>写入结果</returns>
        Result<bool> WriteBitArray(PlcAddress address, bool[] values);
        /// <summary>读单个字</summary>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        Result<short> ReadWord(PlcAddress address);
        /// <summary>读连续字</summary>
        /// <param name="address">起始地址，<c>Length</c> 决定字数</param>
        /// <returns>读取结果</returns>
        Result<short[]> ReadWordArray(PlcAddress address);
        /// <summary>写单个字</summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">字值</param>
        /// <returns>写入结果</returns>
        Result<bool> WriteWord(PlcAddress address, short value);
        /// <summary>写连续字</summary>
        /// <param name="address">起始地址</param>
        /// <param name="values">字值数组</param>
        /// <returns>写入结果</returns>
        Result<bool> WriteWordArray(PlcAddress address, short[] values);

        /// <summary>
        /// 纯文本通讯日志：收到 / 发出的原始报文、以及丢弃、异常等提示。
        /// 由协议线程触发，订阅方（界面）需要自己切回 UI 线程再更新控件。
        /// </summary>
        event Action<string> MessageLogged;

        /// <summary>
        /// 结构化通讯帧：带方向、时间与原始字节，供报文视图着色、复制与筛选。
        /// 与 <see cref="MessageLogged"/> 同时触发、内容等价。
        /// </summary>
        event Action<CommFrame> FrameLogged;
    }
}
