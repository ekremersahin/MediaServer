
namespace MediaServer.ICE.Constants
{
    public static class StunConstants
    {
        public static class MessageTypes
        {
            public const ushort BINDING_REQUEST = 0x0001;
            public const ushort BINDING_RESPONSE_SUCCESS = 0x0101;
            public const ushort BINDING_RESPONSE_ERROR = 0x0111;
        }

        public static class Attributes
        {
            public const ushort MAPPED_ADDRESS = 0x0001;
            public const ushort XOR_MAPPED_ADDRESS = 0x0020;
            public const ushort USERNAME = 0x0006;
            public const ushort MESSAGE_INTEGRITY = 0x0008;
            public const ushort FINGERPRINT = 0x8028;
        }

        public static class RFC5780
        {
            public const ushort CHANGE_REQUEST = 0x0003;
            public const ushort RESPONSE_ORIGIN = 0x802b;
            public const ushort OTHER_ADDRESS = 0x802c;
        }

        public static class ChangeRequestFlags
        {
            public const uint CHANGE_IP = 0x04;
            public const uint CHANGE_PORT = 0x02;
        }

        public static class Protocol
        {
            public const int HEADER_LENGTH = 20;
            public const uint MAGIC_COOKIE = 0x2112A442;
        }

        public static class Timeouts
        {
            public const int DEFAULT_CONNECT_TIMEOUT = 5000;
            public const int DEFAULT_RECEIVE_TIMEOUT = 5000;
            public const int DEFAULT_SEND_TIMEOUT = 5000;
        }
    }
}