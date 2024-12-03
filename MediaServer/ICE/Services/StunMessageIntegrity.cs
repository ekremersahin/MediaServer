using MediaServer.ICE.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class StunMessageIntegrity
    {
        private const ushort MESSAGE_INTEGRITY_ATTR = 0x0008;
        private const ushort FINGERPRINT_ATTR = 0x8028;
        private const uint FINGERPRINT_XOR = 0x5354554e;

        public static bool ValidateMessageIntegrity(byte[] message, string key)
        {
            try
            {
                // Message-Integrity attribute'unu bul
                int position = StunConstants.Protocol.HEADER_LENGTH;
                while (position + 4 <= message.Length)
                {
                    var attrType = (ushort)((message[position] << 8) | message[position + 1]);
                    var attrLength = (ushort)((message[position + 2] << 8) | message[position + 3]);

                    if (attrType == MESSAGE_INTEGRITY_ATTR)
                    {
                        // Gelen HMAC-SHA1 değerini al
                        var receivedHmac = new byte[20];
                        Buffer.BlockCopy(message, position + 4, receivedHmac, 0, 20);

                        // Message-Integrity öncesi mesajı al
                        var messageForHmac = new byte[position];
                        Buffer.BlockCopy(message, 0, messageForHmac, 0, position);

                        // Mesaj uzunluğunu güncelle
                        var messageLength = (ushort)position;
                        messageForHmac[2] = (byte)(messageLength >> 8);
                        messageForHmac[3] = (byte)(messageLength & 0xFF);

                        // HMAC-SHA1 hesapla
                        var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
                        var computedHmac = hmac.ComputeHash(messageForHmac);

                        // HMAC değerlerini karşılaştır
                        return computedHmac.SequenceEqual(receivedHmac);
                    }

                    position += 4 + attrLength;
                    if (attrLength % 4 != 0)
                    {
                        position += 4 - (attrLength % 4);
                    }
                }

                return false; // MESSAGE-INTEGRITY bulunamadı
            }
            catch
            {
                return false; // Herhangi bir hata durumunda doğrulama başarısız
            }
        }


        public static bool ValidateFingerprint(byte[] message)
        {
            try
            {
                if (message.Length < 8) return false;

                var position = message.Length - 8;
                var attrType = (ushort)((message[position] << 8) | message[position + 1]);

                if (attrType != FINGERPRINT_ATTR) return false;

                // CRC32 hesaplama
                uint computedCrc = CalculateCrc32(message, 0, position);
                uint fingerprintValue = computedCrc ^ FINGERPRINT_XOR;

                // Gelen değeri al
                uint receivedValue = (uint)(
                    (message[position + 4] << 24) |
                    (message[position + 5] << 16) |
                    (message[position + 6] << 8) |
                    message[position + 7]);

                return fingerprintValue == receivedValue;
            }
            catch
            {
                return false;
            }
        }

        private static uint CalculateCrc32(byte[] data, int offset, int length)
        {
            uint crc = 0xFFFFFFFF;
            var polynomial = 0xEDB88320;

            for (var i = offset; i < offset + length; i++)
            {
                crc ^= data[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ polynomial;
                    else
                        crc >>= 1;
                }
            }

            return ~crc; // Final XOR
        }
    }
}
