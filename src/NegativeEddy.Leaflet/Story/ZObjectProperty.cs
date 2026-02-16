using System;
using System.Collections.Generic;
using System.Diagnostics;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Story
{
    public class ZObjectProperty
    {
        private IList<byte> _bytes;
        private readonly byte _version;

        public int BaseAddress { get; }

        public ZObjectProperty(IList<byte> bytes, int baseAddress, byte version = 3)
        {
            _bytes = bytes;
            BaseAddress = baseAddress;
            _version = version;
        }

        private byte FirstSizeByte { get { return _bytes[BaseAddress]; } }

        public int ID
        {
            get
            {
                if (_version <= 3)
                {
                    return FirstSizeByte.FetchBits(BitNumber.Bit_4, 5);
                }
                else
                {
                    return FirstSizeByte & 0x3F;
                }
            }
        }

        public int DataLength
        {
            get
            {
                if (_version <= 3)
                {
                    return FirstSizeByte.FetchBits(BitNumber.Bit_7, 3) + 1;
                }
                else
                {
                    if ((FirstSizeByte & 0x80) != 0)
                    {
                        int len = _bytes[BaseAddress + 1] & 0x3F;
                        return len == 0 ? 64 : len;
                    }
                    else
                    {
                        return (FirstSizeByte >> 6) + 1;
                    }
                }
            }
        }

        public int SizeBytesCount
        {
            get
            {
                if (_version <= 3)
                {
                    return 1;
                }
                else
                {
                    return (FirstSizeByte & 0x80) != 0 ? 2 : 1;
                }
            }
        }

        public int DataAddress { get { return BaseAddress + SizeBytesCount; } }

        public int LengthInBytes { get { return SizeBytesCount + DataLength; } }

        public IList<byte> Data
        {
            get
            {
                Debug.Assert(DataLength >= 0);
                Debug.Assert(DataLength <= 8);
                return new ArraySegment<byte>((byte[])_bytes, DataAddress, DataLength);
            }
        }

        public byte[] DataArray
        {
            get
            {
                byte[] result = new byte[DataLength];
                for (int i = 0; i < DataLength; i++)
                {
                    result[i] = _bytes[DataAddress + i];
                }
                return result;
            }
        }

        public int ValueAsWord
        {
            get
            {
                if (DataLength == 1)
                {
                    return _bytes[DataAddress];
                }
                else if (DataLength >= 2)
                {
                    return _bytes.GetWord(DataAddress);
                }
                return 0;
            }
        }
    }
}