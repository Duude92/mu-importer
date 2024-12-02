using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDMonsterSkill : MonoBehaviour
    {
        public Script_Skill[] Skills;
        [Serializable]
        public struct Script_Skill
        {
            public int dummy;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_MONSTERSKILL_NUM)]
            public int[] Skill_Num;
            public int Slot;
        }
    }
}