namespace NegativeEddy.Leaflet.Memory
{
    public class PackedAddress
    {
        public ushort Bits { get; }
        public byte Version { get; }

        public PackedAddress(ushort bits, byte version = 3)
        {
            Bits = bits;
            Version = version;
        }

        public int Address
        {
            get { return (int)AddressHelper.UnpackAddress(Bits, Version); }
        }
    }
}
