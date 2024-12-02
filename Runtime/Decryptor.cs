namespace MUImporter
{
    public static class Decryptor
    {
        public static byte[] Decrypt(byte[] data, int size)
        {
            if (data[0] == 0x89)
            {
                data = DecryptBless(data, size);
            }

            byte[] returnData = new byte[data.Length];
            char kk = (char)0x5E;
            byte[] key = new byte[]
            {
                0xD1, 0x73, 0x52, 0xF6,
                0xD2, 0x9A, 0xCB, 0x27,
                0x3E, 0xAF, 0x59, 0x31,
                0x37, 0xB3, 0xE7, 0xA2
            };
            int keys = key.Length;


            if (data == null || size < 1)
                return null;

            byte wMapKey = (byte)kk;
            for (int i = 0; i < size; i++)
            {
                returnData[i] = (byte)((data[i] ^ key[i % 16]) - (byte)wMapKey);
                //data[i] = (byte)((data[i] +(byte)wMapKey) ^ key[i % 16]) ;

                wMapKey = (byte)(data[i] + 0x3D);
                // byte a = (byte)(data[i] ^ key[i % keys]);
                // byte b = (byte)(a - k);
                //data[i] = (byte)(a-b);
                wMapKey = (byte)(wMapKey & 0xff);
            }

            return returnData;
        }

        public static byte[] DecryptBless(byte[] buffer, int size)
        {
            byte[] result = new byte[size];

            byte[] key = new byte[]
            {
                0x7B, 0xAD,
                0x2B, 0x81,
                0x39, 0x89,
                0x93, 0xBD
            };
            byte wMapKey = 0x63;

            for (int i = 0; i < size; i++)
            {
                result[i] = (byte)((buffer[i] ^ key[i & 7]) - wMapKey);
                wMapKey = (byte)(buffer[i] + 0x3F);
                wMapKey &= 0xff;
            }

            return result;
        }

        static byte[] bBuxCode = new byte[3] { 0xfc, 0xcf, 0xab };

        public static byte[] BuxConvert(byte[] buffer, int length, int offset = 0)
        {
            byte[] returnValue = new byte[length];
            for (int i = 0; i < length; i++)
                returnValue[i] = (byte)(buffer[i + offset] ^ bBuxCode[i % 3]);
            return returnValue;
        }

        public static void BuxConvert(ref byte[] buffer, int length, int offset = 0)
        {
            buffer = BuxConvert(buffer, length, offset);
        }
    }
}