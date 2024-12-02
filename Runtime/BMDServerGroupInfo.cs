using System.Runtime.InteropServices;
using static MUImporter.Enums;

namespace MUImporter
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BMDServerGroupInfo
    {
        public short m_wIndex;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SLM_MAX_SERVER_NAME_LENGTH)]
        public string m_szName;


        public byte m_byPos;
        public byte m_bySequence;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SLM_MAX_SERVER_COUNT)]
        public byte[] m_abyNonPVP;

        public short m_nDescriptLen;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ServerGroupInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SLM_MAX_SERVER_NAME_LENGTH)]
        public string m_szName;

        public byte m_byPos;
        public byte m_bySequence;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SLM_MAX_SERVER_COUNT)]
        public byte[] m_abyNonPVP;

        public string m_strDescript;

        public ServerGroupInfo(BMDServerGroupInfo info, string description)
        {
            m_szName = info.m_szName;
            m_byPos = info.m_byPos;
            m_bySequence = info.m_bySequence;
            m_abyNonPVP = info.m_abyNonPVP;
            m_strDescript = description;
        }
    };
}