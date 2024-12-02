using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDMasterSkillTree : MonoBehaviour
    {
        public MASTER_SKILL_DATA[] MasterSkillData;
        public MASTER_LEVEL_DATA[] Data;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MASTER_LEVEL_DATA //TODO: 2-dim array
        {
            public byte Width;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 4)]
            public int[] Ability;//[8][4];
        }
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MASTER_SKILL_DATA
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] SkillData;
        }
    }
}