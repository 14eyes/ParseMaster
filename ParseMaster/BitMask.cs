namespace ParseMaster;

public class BitMask
{
    private readonly ulong _len;
    private readonly ulong _mask;
    private readonly uint[]? _excelMask;

    public BitMask(DeReader reader, bool small) //BinOut
    {
        _mask = small ? reader.ReadU8() : reader.ReadVarUInt();
        _len = sizeof(ulong);
    }

    public BitMask(DeReader reader) //Excel
    {
        _len = reader.ReadVarUInt();
        _excelMask = new uint[_len / sizeof(uint) + 1];

        for (uint i = 0; i < _len / sizeof(uint); i++)
        {
            _excelMask[i] = reader.ReadU32();
        }

        // Read leftovers
        for (uint i = 0; i < _len % sizeof(uint); i++)
        {
            _excelMask[_len / sizeof(uint)] |= (uint)reader.ReadU8() << (int)(i * 8);
        }
    }

    public bool TestBit(int index)
    {
        if ((uint)index >= _len * 8) return false;

        return _excelMask is not null
            ? (_excelMask[(uint)index >> 5] & (1u << (index & 0x1F))) != 0
            : (_mask & (1ul << index)) != 0;
    }
}