using System.Text;
using System.Text.RegularExpressions;

namespace ParseMaster;

public partial class DeReader : IDisposable
{
    // Composition over inheritance
    private readonly BinaryReader _reader;

    public DeReader(string filename)
    {
        _reader = new(File.Open(filename, FileMode.Open));
    }

    public long LenToEof()
    {
        return _reader.BaseStream.Length - _reader.BaseStream.Position;
    }

    public bool ReadBool()
    {
        var b = _reader.ReadByte();

        return b switch
        {
            0 => false,
            1 => true,
            _ => throw new InvalidDataException($"Invalid boolean value: {b}")
        };
    }

    public byte ReadU8()
    {
        return _reader.ReadByte();
    }

    public sbyte ReadS8()
    {
        return _reader.ReadSByte();
    }

    public ushort ReadU16()
    {
        return _reader.ReadUInt16();
    }

    public short ReadS16()
    {
        return _reader.ReadInt16();
    }

    public ushort ReadU16Be()
    {
        var a = _reader.ReadByte();
        var b = _reader.ReadByte();
        return (ushort)(a << 8 | b);
    }

    public int ReadS32()
    {
        return _reader.ReadInt32();
    }

    public uint ReadU32()
    {
        return _reader.ReadUInt32();
    }

    public ulong ReadU64()
    {
        return _reader.ReadUInt64();
    }

    public float ReadF32()
    {
        return _reader.ReadSingle();
    }

    public double ReadF64()
    {
        return _reader.ReadDouble();
    }

    public long ReadVarInt()
    {
        // From ZigZag-encoded data
        var encoded = ReadVarUInt();
        var sign = encoded & 1;
        var absValue = encoded >> 1;
        return (long)(sign == 0 ? absValue : ~(absValue - 1));
    }

    public ulong ReadVarUInt()
    {
        // Stolen from some repo on Github

        var shift = 0;
        ulong result = 0;

        // VarInt is max 128 bits => 19 bytes (+1 extra just for cute round number)
        for (var i = 0; i < 20; i++)
        {
            ulong byteValue = _reader.ReadByte();

            var tmp = byteValue & 0x7f;
            result |= tmp << shift;

            if ((byteValue & 0x80) != 0x80)
                return result;

            shift += 7;
        }

        throw new IOException("Invalid VarUInt encoding.");
    }

    public string ReadString()
    {
        var len = ReadVarUInt();
        Span<byte> bytes = stackalloc byte[(int)len];
        _ = _reader.Read(bytes);
        var str = Encoding.UTF8.GetString(bytes);
        return EscapeRegex().Replace(str, m => "\\" + m.Value);
    }

    public byte PeekByte()
    {
        return (byte)(_reader.PeekChar() & 0xFF);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }

    [GeneratedRegex("[\\\\\"]")]
    private static partial Regex EscapeRegex();
}