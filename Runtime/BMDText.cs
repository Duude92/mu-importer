using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDText : MonoBehaviour
    {
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GLOBALTEXT_HEADER
        {
            public short wSignature;   //. 0x5447
            public int dwNumberOfText;
        };
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GLOBALTEXT_STRING_HEADER
        {
            public int dwKey;
            public int dwSizeOfString;
        };
        public string[] stringList = new string[] { };
    }
}