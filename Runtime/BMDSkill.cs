using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDSkill : MonoBehaviour
    {
        public SKILL_ATTRIBUTE[] Skills;


        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SKILL_ATTRIBUTE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] Name;
            public byte Level;
            public short Damage;
            public short Mana;
            public short AbilityGuage;
            public byte Distance;
            public int Delay;
            public int Energy;
            public short Charisma;
            public byte MasteryType;
            public byte SkillUseType;
            public byte SkillBrand;
            public byte KillCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DUTY_CLASS)]
            public byte[] RequireDutyClass;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CLASS)]
            public byte[] RequireClass;
            public short Magic_Icon;
            public byte TypeSkill;
            public int Strength;
            public int Dexterity;

        }

    }
}