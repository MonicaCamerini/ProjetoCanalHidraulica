using System;

namespace ProjetoCanaldeHidraulica
{
    class Word
    {
        /// <summary>
        /// Convert UInt16 to byte array
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>Byte Array</returns>
        public static byte[] UInt16ToByte(UInt16 value)
        {
            byte[] array = BitConverter.GetBytes(value);
            Array.Reverse(array);
            return array;
        }

        /// <summary>
        /// Convert UInt16 array to byte array
        /// </summary>
        /// <param name="value">Values</param>
        /// <returns>Byte Array</returns>
        public static byte[] UInt16ToByte(UInt16[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt16 val in value)
                arr.Add(UInt16ToByte(val));
            return arr.array;
        }

        public static UInt16 FromByteArray(byte[] bytes)
        {
            // bytes[0] -> HighByte
            // bytes[1] -> LowByte
            return FromBytes(bytes[1], bytes[0]);
        }

        public static UInt16 FromBytes(byte LoVal, byte HiVal)
        {
            return (UInt16)(HiVal * 256 + LoVal);
        }

        public static UInt16[] ByteToUInt16(byte[] bytes)
        {
            UInt16[] values = new UInt16[bytes.Length / 2];
            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++] });
            return values;
        }
    }
}
