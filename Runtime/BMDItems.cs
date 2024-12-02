using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDItems : MonoBehaviour
    {
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ITEM_ATTRIBUTE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public char[] Name; //30char
            public short TwoHand;
            public short Level;
            public short m_byItemSlot;
#if PBG_ADD_NEWCHAR_MONK_SKILL
        public short m_wSkillIndex;
#else //PBG_ADD_NEWCHAR_MONK_SKILL
            public short m_bySkillIndex;
#endif //PBG_ADD_NEWCHAR_MONK_SKILL
            public byte Width;
            public byte Height;
            public byte DamageMin;
            public byte DamageMax;
            public byte SuccessfulBlocking;
            public byte Defense;
            public byte MagicDefense;
            public byte WeaponSpeed;
            public byte WalkSpeed;
            public byte Durability;
            public byte MagicDur;
            public byte MagicPower;
            public short RequireStrength;
            public short RequireDexterity;
            public short RequireEnergy;
            public short RequireVitality;
            public short RequireCharisma;
            public short RequireLevel;
            public short Value;
            public int iZen;
            public byte AttType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CLASS + 1)]
            public byte[] RequireClass;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_RESISTANCE + 1)]
            public byte[] Resistance;
        };


        public ITEM_ATTRIBUTE[] Items;


    }
}