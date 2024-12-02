using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDGate : MonoBehaviour
    {
        public GATE_ATTRIBUTE[] Gates;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GATE_ATTRIBUTE
        {
            public byte Flag;
            public byte Map;
            public byte x1;
            public byte y1;
            public byte x2;
            public byte y2;
            public short Target;
            public byte Angle;
            public short Level;
            public short m_wMaxLevel;
        }
    }
}