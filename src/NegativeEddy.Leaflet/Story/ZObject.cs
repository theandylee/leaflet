using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NegativeEddy.Leaflet.Memory;

namespace NegativeEddy.Leaflet.Story
{
    public class ZObject
    {
        private const int ZOBJECT_ARRAY_SIZE_V1_3 = 31;
        private const int ZOBJECT_ARRAY_SIZE_V4 = 63;

        private readonly byte _version;
        private int ParentIDOffset => _version <= 3 ? 4 : 6;
        private int SiblingIDOffset => _version <= 3 ? 5 : 8;
        private int ChildIDOffset => _version <= 3 ? 6 : 10;
        private int PropertyAddressOffset => _version <= 3 ? 7 : 12;

        public const string UNNAMED_OBJECT_NAME = "<unnamed>";
        public const int INVALID_ID = 0;

        private readonly byte[] _bytes;

        public static ZObject InvalidObject = new ZObject(null, 0, ZObject.INVALID_ID, 3);

        public static ushort[] DefaultProperties { get; set; }

        public int BaseAddress { get; }
        public int ID { get; }

        private ulong Attributes
        {
            get
            {
                if (_version <= 3)
                {
                    return _bytes.GetDWord(BaseAddress);
                }
                else
                {
                    return ((ulong)_bytes.GetWord(BaseAddress) << 32) | ((ulong)_bytes.GetWord(BaseAddress + 2) << 16) | (ulong)_bytes.GetWord(BaseAddress + 4);
                }
            }
            set
            {
                if (_version <= 3)
                {
                    _bytes.SetDWord((uint)value, BaseAddress);
                }
                else
                {
                    _bytes.SetWord((ushort)(value >> 32), BaseAddress);
                    _bytes.SetWord((ushort)(value >> 16), BaseAddress + 2);
                    _bytes.SetWord((ushort)value, BaseAddress + 4);
                }
            }
        }

        public int ParentID
        {
            get
            {
                if (_version <= 3)
                {
                    return _bytes[BaseAddress + ParentIDOffset];
                }
                else
                {
                    return _bytes.GetWord(BaseAddress + ParentIDOffset);
                }
            }
            set
            {
                if (_version <= 3)
                {
                    _bytes[BaseAddress + ParentIDOffset] = (byte)value;
                }
                else
                {
                    _bytes.SetWord((ushort)value, BaseAddress + ParentIDOffset);
                }
            }
        }

        public int SiblingID
        {
            get
            {
                if (_version <= 3)
                {
                    return _bytes[BaseAddress + SiblingIDOffset];
                }
                else
                {
                    return _bytes.GetWord(BaseAddress + SiblingIDOffset);
                }
            }
            set
            {
                if (_version <= 3)
                {
                    _bytes[BaseAddress + SiblingIDOffset] = (byte)value;
                }
                else
                {
                    _bytes.SetWord((ushort)value, BaseAddress + SiblingIDOffset);
                }
            }
        }

        public int ChildID
        {
            get
            {
                if (_version <= 3)
                {
                    return _bytes[BaseAddress + ChildIDOffset];
                }
                else
                {
                    return _bytes.GetWord(BaseAddress + ChildIDOffset);
                }
            }
            set
            {
                if (_version <= 3)
                {
                    _bytes[BaseAddress + ChildIDOffset] = (byte)value;
                }
                else
                {
                    _bytes.SetWord((ushort)value, BaseAddress + ChildIDOffset);
                }
            }
        }

        public int PropertyTableAddress
        {
            get { return (int)_bytes.GetWord(BaseAddress + PropertyAddressOffset); }
        }

        public override string ToString()
        {
            return $"[{ID:D3}] {ShortName}";
        }

        public string ToLongString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"ID:{ID} Name:{this.ShortName.Replace(' ', '_')} Attributes:");
            int maxBit = _version <= 3 ? 31 : 47;
            foreach (var bit in Attributes.GetBits())
            {
                sb.Append((maxBit - (int)bit).ToString());
                sb.Append(',');
            }

            // remove the last comma if attributes were added
            if (sb[sb.Length - 1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append(" ");

            sb.Append($"Parent:{ParentID} Sibling:{SiblingID} Child:{ChildID} ");
            sb.Append($"PropertyAddr:{PropertyTableAddress:x4} ");

            sb.Append("Properties:");
            foreach (var prop in CustomProperties.OrderBy(x => x.ID))
            {
                sb.Append($"[{prop.ID}],");
                foreach (byte b in prop.Data)
                {
                    sb.Append($"{b:x2},");
                }
            }

            // remove the last comma if custom properties were added
            if (sb[sb.Length - 1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public string ToFullString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ID}. Attributes: ");
            int maxBit = _version <= 3 ? 31 : 47;
            foreach (var bit in Attributes.GetBits())
            {
                sb.Append((maxBit - (int)bit).ToString());
                sb.Append(',');
            }

            sb.AppendLine();

            sb.AppendLine($"   Parent object:  {ParentID}  Sibling object: {SiblingID}  Child object:  {ChildID}");
            sb.AppendLine($"   Property address: {PropertyTableAddress:x4}");

            sb.AppendLine($"       Description: \"{this.ShortName}\"");
            sb.AppendLine("        Properties:");
            foreach (var prop in CustomProperties)
            {
                sb.Append($"        [{prop.ID}] ");
                foreach (byte b in prop.Data)
                {
                    sb.Append($"{b:x2}  ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public int ShortNameLengthInWords
        {
            get { return (int)_bytes[PropertyTableAddress]; }
        }

        public string ShortName
        {
            get { return NameBuilder?.ToString() ?? UNNAMED_OBJECT_NAME; }
        }

        private ZStringBuilder NameBuilder
        {
            get
            {
                int length = ShortNameLengthInWords;
                int currentIndex = PropertyTableAddress + 1;
                if (length > 0)
                {
                    var zb = new ZStringBuilder(_bytes, currentIndex, length * 2);
                    return zb;
                }
                else
                {
                    return null;
                }
            }
        }

        private int NameLengthInBytes
        {
            get { return ShortNameLengthInWords * 2; }
        }

        public uint GetPropertyValue(int propertyID)
        {
            var props = CustomProperties;

            var prop = props.FirstOrDefault(p => p.ID == propertyID);
            if (prop != null)
            {
                var data = prop.Data;

                if (data.Count == 1)
                {
                    return data[0];
                }
                else if (data.Count == 2)
                {
                    return data.GetWord(0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot get property value. It has {data.Count} bytes");
                }
            }
            else
            {
                // When the game attempts to read the value of property n for an object which 
                // does not provide property n, the n-th entry in this table is the resulting 
                // value. spec 12.2
                ushort value = DefaultProperties[propertyID - 1];
                Debug.WriteLine($"  Default property used for prop {propertyID}. Value = {value}");
                return value;
            }
        }

        public void SetPropertyValue(int propertyID, int value)
        {
            var prop = CustomProperties.FirstOrDefault(p => p.ID == propertyID);
            if (prop != null)
            {
                var data = prop.Data;

                if (data.Count == 1)
                {
                    data[0] = (byte)value;
                }
                else if (data.Count == 2)
                {
                    data.SetWord((ushort)value, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot set property value. It has {data.Count} bytes");
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot set property value. Object {ID} does not have property {propertyID} bytes");
            }
        }

        public ZObjectProperty[] CustomProperties
        {
            get
            {
                int propertyAddress = PropertyTableAddress + 1 + NameLengthInBytes;
                var properties = new List<ZObjectProperty>();
                var prop = new ZObjectProperty(_bytes, propertyAddress, _version);
                while (prop.ID != 0)
                {
                    properties.Add(prop);
                    propertyAddress += prop.LengthInBytes;
                    prop = new ZObjectProperty(_bytes, propertyAddress, _version);
                }

                return properties.ToArray();
            }
        }

        public ZObject(byte[] bytes, int baseAddress, int ID, byte version)
        {
            this.ID = ID;
            _bytes = bytes;
            BaseAddress = baseAddress;
            _version = version;
        }

        /// <summary>
        /// Checks if the object has a specific attribute enabled
        /// </summary>
        /// <param name="attributeNumber">the bit of the attribute to check (0-31)</param>
        /// <returns>true if the object has the atttribute set</returns>
        public bool HasAttribute(BitNumber attributeNumber)
        {
            int maxBit = _version <= 3 ? 31 : 47;
            return Attributes.FetchBits((BitNumber)(maxBit - (int)attributeNumber), 1) == 1;
        }

        public void SetAttribute(BitNumber attributeNumber, bool set)
        {
            int maxBit = _version <= 3 ? 31 : 47;
            Attributes = Attributes.SetBit((BitNumber)(maxBit - (int)attributeNumber), set);
        }
    }
}