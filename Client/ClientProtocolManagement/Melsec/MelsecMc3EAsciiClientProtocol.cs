using PlcTest.Models;
using PlcTest.Client.ClientInterface;
using System;
using System.Linq;
using System.Text;
using PlcTest.Protocols;
using PlcTest.Tool;

namespace PlcTest.Client.ClientProtocolManagement.Melsec
{
    /// <summary>
    /// 三菱3EASCII通讯协议
    /// </summary>
    public class MelsecMc3EAsciiClientProtocol : IPlcClientProtocol, IProtocolOptionHost
    {
        /// <summary>
        ///获取寄存器地址的真实编码
        /// </summary>
        /// <param name="memoryArea">寄存器区域</param>
        /// <returns>寄存器地址的真实编码</returns>
        public int GetMemoryCode(MemoryArea memoryArea)
        {
            // ASCII 软元件代码由 2 个 ASCII 字符打包：高字节第 1 字符，低字节第 2 字符，
            // 单字符软元件（X/Y/M/L/F/V/B/D/W/S/Z/R）补 '*'，多字符软元件（SM/TS/DX/ZR…）用双字母。
            return Melsec3E4EDeviceCatalog.Get(memoryArea).AsciiCodeWord;
        }
        /// <summary>
        /// 该软元件区域允许的访问方式。
        /// 位软元件（M/X/Y 等）既可位访问也可字访问（字访问时每字 16 点）；字软元件（D/W/R 等）只能字访问。
        /// </summary>
        /// <param name="memoryArea">寄存器区域</param>
        /// <returns>允许的访问方式</returns>
        public AreaAccess GetAreaAccess(MemoryArea memoryArea)
        {
            // 位/字能力由软元件表统一声明，协议不再自己维护第二套规则
            return Melsec3E4EDeviceCatalog.Get(memoryArea).Access;
        }

        /// <summary>
        /// 软元件编号上限取自 <see cref="Melsec3E4EDeviceCatalog"/>（基本型 QCPU 默认范围），
        /// 仅用于发出报文前的越界提示，不影响报文结构。
        /// </summary>
        /// <param name="memoryArea">寄存器区域</param>
        /// <returns>最大编号；-1 表示协议不做限制</returns>
        public int GetMaxIndex(MemoryArea memoryArea)
        {
            return Melsec3E4EDeviceCatalog.Get(memoryArea).DefaultMaxIndex;
        }

        /// <summary>
        /// 根据响应前缀推断整帧长度：3E ASCII 响应为 18 字符头 + [14..17] 四位十六进制"数据长度"（从结束码起算）。
        /// </summary>
        public int GetResponseFrameLength(byte[] request, byte[] buffer, int available)
        {
            if (buffer == null || available < 18) return -1;
            if (buffer[0] != (byte)'D' || buffer[1] != (byte)'0') return -1;
            string lengthText = Encoding.ASCII.GetString(buffer, 14, 4);
            ushort bodyLength;
            if (!ushort.TryParse(lengthText, System.Globalization.NumberStyles.HexNumber, null, out bodyLength))
            {
                return -1;
            }
            return 18 + bodyLength;
        }
        /// <summary>
        /// 写入位数据转Byte数组
        /// </summary>
        /// <param name="bools">位数据</param>
        /// <returns>打包后的字节数组</returns>
        public byte[] GetWriteBitData(bool[] bools)
        {
            byte[] buffer = new byte[bools.Length];
            for (int i = 0; i < bools.Length; i++)
            {
                buffer[i] = bools[i] ? (byte)0x31 : (byte)0x30;
            }
            return buffer;
        }
        /// <summary>
        /// 写入字数据转Byte数组
        /// </summary>
        /// <param name="shorts">字数据</param>
        /// <returns>打包后的字节数组</returns>
        public byte[] GetWriteWordData(short[] shorts)
        {
            byte[] buffer = new byte[shorts.Length * 4];
            for (int i = 0; i < shorts.Length; i++)
            {
                byte[] ascii = Encoding.ASCII.GetBytes(Convert.ToString(shorts[i], 16).ToUpper().PadLeft(4, '0'));
                ascii.CopyTo(buffer, i * 4);
            }
            return buffer;
        }
        /// <summary>
        /// 目标网络号（0–255）。直连本机 CPU 时为 0；经 MELSECNET / CC-Link 路由时才需要改。
        /// 写进请求帧头的网络号字节。
        /// </summary>
        public int NetworkNumber { get; set; } = 0;

        /// <summary>
        /// 目标模块站号（0–255）。直连本机 CPU 时为 0。
        /// </summary>
        public int StationNumber { get; set; } = 0;

        /// <summary>
        /// 应用界面传来的协议选项（网络号 / 站号）。越界值夹到字节范围，避免拼帧时写坏。
        /// </summary>
        /// <param name="options">选项集，可能为 null</param>
        public void ApplyOptions(ProtocolOptions options)
        {
            NetworkNumber = ProtocolOptionValues.ClampInt(options, "NetworkNumber", NetworkNumber, 0, 255);
            StationNumber = ProtocolOptionValues.ClampInt(options, "StationNumber", StationNumber, 0, 255);
        }

        /// <summary>
        /// 创建读取数据包
        /// </summary>
        /// <param name="address">PLC地址模型</param>
        /// <returns>读取命令报文</returns>
        public byte[] BuildReadData(PlcAddress address)
        {
            int memory = GetMemoryCode(address.Area);
            int startAddress = address.Address;
            bool isBit = address.IsBit;
            int length = address.Length;
            byte[] frame = new byte[20];
            //读:0401
            frame[0] = 0x30;
            frame[1] = 0x34;
            frame[2] = 0x30;
            frame[3] = 0x31;
            //以点为单位:0001,字为单位:0000
            frame[4] = 0x30;
            frame[5] = 0x30;
            frame[6] = 0x30;
            frame[7] = isBit ? (byte)0x31 : (byte)0x30;
            //软元件类型（2 个 ASCII 字符，高字节为第 1 字符）
            frame[8] = (byte)((memory >> 8) & 0xFF);
            frame[9] = (byte)(memory & 0xFF);
            //起始地址
            string addressText = Melsec3E4EDeviceCatalog.Get(address.Area).FormatIndex(startAddress, 6);
            byte[] ascii = Encoding.ASCII.GetBytes(addressText);
            Array.Copy(ascii, 0, frame, 10, 6);
            //软元件点数
            string lengthText = Convert.ToString(length, 16).PadLeft(4, '0').ToUpper();
            ascii = Encoding.ASCII.GetBytes(lengthText);
            Array.Copy(ascii, 0, frame, 16, 4);
            return frame;
        }
        /// <summary>
        /// 创建写入数据包
        /// </summary>
        /// <param name="address">PLC地址模型</param>
        /// <param name="data">写入数据</param>
        /// <returns>写命令报文</returns>
        public byte[] BuildWriteData(PlcAddress address, byte[] data)
        {
            int memory = GetMemoryCode(address.Area);
            int startAddress = address.Address;
            bool isBit = address.IsBit;
            int length = address.Length;
            byte[] frame = new byte[20 + data.Length];
            //写:1401
            frame[0] = 0x31;
            frame[1] = 0x34;
            frame[2] = 0x30;
            frame[3] = 0x31;
            //以点为单位:0001,字为单位:0000
            frame[4] = 0x30;
            frame[5] = 0x30;
            frame[6] = 0x30;
            frame[7] = isBit ? (byte)0x31 : (byte)0x30;
            //软元件类型（2 个 ASCII 字符，高字节为第 1 字符）
            frame[8] = (byte)((memory >> 8) & 0xFF);
            frame[9] = (byte)(memory & 0xFF);
            //起始地址
            string addressText = Melsec3E4EDeviceCatalog.Get(address.Area).FormatIndex(startAddress, 6);
            byte[] ascii = Encoding.ASCII.GetBytes(addressText);
            Array.Copy(ascii, 0, frame, 10, 6);
            //软元件点数
            string lengthText = Convert.ToString(length, 16).PadLeft(4, '0').ToUpper();
            ascii = Encoding.ASCII.GetBytes(lengthText);
            Array.Copy(ascii, 0, frame, 16, 4);
            //状态
            Array.Copy(data, 0, frame, 20, data.Length);
            return frame;
        }
        /// <summary>
        /// 拼接完整数据包
        /// </summary>
        /// <param name="data">命令报文</param>
        /// <returns>完整命令报文</returns>
        public byte[] BuildCompletePack(byte[] data)
        {
            byte[] packet = new byte[22 + data.Length];
            // 副标题
            packet[0] = 0x35;
            packet[1] = 0x30;
            packet[2] = 0x30;
            packet[3] = 0x30;
            //网络号（两位大写十六进制字符）
            byte[] networkBytes = Encoding.ASCII.GetBytes(NetworkNumber.ToString("X2"));
            packet[4] = networkBytes[0];
            packet[5] = networkBytes[1];
            // PLC编号
            packet[6] = 0x46;
            packet[7] = 0x46;
            // 目标模块IO编号
            packet[8] = 0x30;
            packet[9] = 0x33;
            packet[10] = 0x46;
            packet[11] = 0x46;
            //目标模块站号（两位大写十六进制字符）
            byte[] stationBytes = Encoding.ASCII.GetBytes(StationNumber.ToString("X2"));
            packet[12] = stationBytes[0];
            packet[13] = stationBytes[1];
            //数据长度
            byte[] lengthAscii = Encoding.ASCII.GetBytes(Convert.ToString(packet.Length - 18, 16).PadLeft(4, '0').ToUpper());
            packet[14] = lengthAscii[0];
            packet[15] = lengthAscii[1];
            packet[16] = lengthAscii[2];
            packet[17] = lengthAscii[3];
            //CPU监视定时器
            packet[18] = 0x30;
            packet[19] = 0x30;
            packet[20] = 0x31;
            packet[21] = 0x30;
            data.CopyTo(packet, 22);
            return packet;
        }
        /// <summary>
        /// 解析写返回
        /// </summary>
        /// <param name="data">返回数据</param>
        /// <returns>返回写入结果</returns>
        public Result<bool> ParseWriteResponse(byte[] data)
        {
            Result<bool> result = new Result<bool>();
            if (data == null)
            {
                result.ErrorMsg = "返回数据为空";
                return result;
            }
            string text = Encoding.ASCII.GetString(data);
            if (text.Length < 22)
            {
                result.ErrorMsg = "返回数据长度错误";
                return result;
            }
            //if (!CheckResponseHeader(data))
            //{
            //    result.ErrorMsg = "响应副标题错误";
            //    return result;
            //}
            ushort endCode = Convert.ToUInt16(text.Substring(18, 4), 16);
            if (endCode != 0)
            {
                result.ErrorMsg = $"结束代码错误: 0x{endCode:X4}";
                result.Kind = ErrorKind.DeviceEndCode;
                result.EndCode = endCode;
                return result;
            }

            result.IsSuccess = true;
            result.Data = true;
            return result;
        }
        /// <summary>
        /// 解析读位数据返回
        /// </summary>
        /// <param name="data">返回数据</param>
        /// <param name="size">要读取的点数</param>
        /// <returns>位数据数组</returns>
        public Result<bool[]> ParseReadBitResponse(byte[] data, int size)
        {
            Result<bool[]> result = new Result<bool[]>();
            if (data == null)
            {
                result.ErrorMsg = "返回数据为空";
                return result;
            }
            string text = Encoding.ASCII.GetString(data);
            if (text.Length < 22 + size)
            {
                result.ErrorMsg = "返回数据长度错误";
                return result;
            }
            //if (!CheckResponseHeader(data))
            //{
            //    result.ErrorMsg = "响应副标题错误";
            //    return result;
            //}
            ushort endCode = Convert.ToUInt16(text.Substring(18, 4), 16);
            if (endCode != 0)
            {
                result.ErrorMsg = $"结束代码错误: 0x{endCode:X4}";
                result.Kind = ErrorKind.DeviceEndCode;
                result.EndCode = endCode;
                return result;
            }

            // 解析返回数据
            bool[] values = new bool[size];
            values = text.Substring(22, size).Select(o => o == '1').Take(size).ToArray();

            result.IsSuccess = true;
            result.Data = values;
            return result;
        }
        /// <summary>
        /// 解析读字数据返回
        /// </summary>
        /// <param name="data">返回数据</param>
        /// <param name="size">要读取的字数</param>
        /// <returns>字数据数组</returns>
        public Result<short[]> ParseReadWordResponse(byte[] data, int size)
        {
            Result<short[]> result = new Result<short[]>();
            if (data == null)
            {
                result.ErrorMsg = "返回数据为空";
                return result;
            }
            string text = Encoding.ASCII.GetString(data);
            if (text.Length < 22 + 4 * size)
            {
                result.ErrorMsg = "返回数据长度错误";
                return result;
            }
            //if (!CheckResponseHeader(data))
            //{
            //    result.ErrorMsg = "响应副标题错误";
            //    return result;
            //}
            ushort endCode = Convert.ToUInt16(text.Substring(18, 4), 16);
            if (endCode != 0)
            {
                result.ErrorMsg = $"结束代码错误: 0x{endCode:X4}";
                result.Kind = ErrorKind.DeviceEndCode;
                result.EndCode = endCode;
                return result;
            }
            //解析返回数据
            short[] values = new short[size];
            for (int i = 0; i < size; i++)
            {
                values[i] = Convert.ToInt16(text.Substring(22 + 4 * i, 4), 16);
            }

            result.IsSuccess = true;
            result.Data = values;
            return result;
        }
        /// <summary>
        /// 校验 3E 响应副标题（0xD0 0x00）
        /// </summary>
        private static bool CheckResponseHeader(byte[] data)
        {
            return data[0] == 0xD0 && data[1] == 0x00;
        }
    }
}
