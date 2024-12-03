using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class STUNMessage
    {
        // STUN Message Type
        public const ushort BindingRequest = 0x0001;
        public const ushort BindingResponse = 0x0101;
        public const ushort BindingErrorResponse = 0x0111;

        // STUN Attribute Types
        public const ushort MappedAddress = 0x0001;
        public const ushort XorMappedAddress = 0x0020;
        public const ushort Username = 0x0006;
        public const ushort MessageIntegrity = 0x0008;
        public const ushort Fingerprint = 0x8028;

        public ushort MessageType { get; set; }
        public ushort MessageLength { get; set; }
        public byte[] TransactionId { get; set; } = new byte[12];
        public byte[] Attributes { get; set; }

        public byte[] Serialize()
        {
            var message = new byte[20 + (Attributes?.Length ?? 0)];

            // Message Type
            message[0] = (byte)(MessageType >> 8);
            message[1] = (byte)(MessageType & 0xFF);

            // Message Length
            message[2] = (byte)(MessageLength >> 8);
            message[3] = (byte)(MessageLength & 0xFF);

            // Transaction ID
            Buffer.BlockCopy(TransactionId, 0, message, 4, 12);

            // Attributes
            if (Attributes != null)
            {
                Buffer.BlockCopy(Attributes, 0, message, 20, Attributes.Length);
            }

            return message;
        }
    }
}
