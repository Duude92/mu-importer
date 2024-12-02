using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    [Serializable]
    public struct BMDObject
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct BMDHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] BMD;
            public byte TypeCode;               // 0x0a = normal
            public bool IsValid()
            {
                if (new string(BMD).ToLower() != "bmd")
                {
                    return false;
                }
                if (TypeCode != 0x0A && TypeCode != 0x0C)
                {
                    return false;
                }
                return true;
            }
        }
        [Serializable]
        public struct BMDFileInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] sName;//[BMD_FILE_FILENAME_SIZE];
            public ushort nMeshCnt;
            public ushort nBoneCnt;
            public ushort nAnimCnt;

        }
        [Serializable]
        public struct BMD_FILE_MESH
        {

            public ushort nVertPos;
            public ushort nVertNorm;
            public ushort nVertTex;
            public ushort nTrngl;
            public ushort id;
        }
        [Serializable]
        public struct BMD_FILE_VERTEX_POS
        {
            public short iParent;
            public short trash;
            public Vector3 vPos;
            public Vector3 posRight => new Vector3(vPos.x, vPos.z, vPos.y);
            public BMD_FILE_VERTEX_POS(short parent, Vector3 position)
            {
                iParent = parent;
                vPos = position;
                trash = 0;
            }
        }
        [Serializable]

        public struct BMD_FILE_VERTEX_NORM
        {
            public int iParent;
            public Vector3 vNorm;
            public int id;
            public BMD_FILE_VERTEX_NORM FromBytes(byte[] data, ref int position)
            {
                iParent = data[position];
                vNorm = VectorExtension.FromBytesStatic(data, position + 4);
                id = data[position + 16];
                position += 20;
                return this;
            }
        }
        [Serializable]

        // BMD Texture coordinates for vertex
        public struct BMD_FILE_VERTEX_TEX
        {
            public Vector2 vTex;
            public static BMD_FILE_VERTEX_TEX FromBytes(byte[] data, ref int position)
            {
                return new BMD_FILE_VERTEX_TEX(VectorExtension.FromBytes(data, ref position));
            }
            public BMD_FILE_VERTEX_TEX(Vector2 Tex)
            {
                vTex = Tex;
            }
        }
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct BMD_FILE_VERTEX_ID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public short[] v; //4

            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]

            // short[] v2; //3

            // short trash; // alignment shit =), or not 
            public static BMD_FILE_VERTEX_ID FromBytes(byte[] data, ref int position)
            {
                BMD_FILE_VERTEX_ID bmdFileVertex = new BMD_FILE_VERTEX_ID();
                bmdFileVertex.v = new short[4];
                //bmdFileVertex.v2 = new short[3];
                for (int i = 0; i < bmdFileVertex.v.Length; i++)
                {
                    bmdFileVertex.v[i] = BitConverter.ToInt16(data, position);
                    position += 2;
                }
                return bmdFileVertex;
            }

        }


        [Serializable]
        public struct BMD_FILE_TRIANGLE
        {

            byte count; // ???
            byte trash; // not used	
            [SerializeField] public BMD_FILE_VERTEX_ID viPos;
            [SerializeField] public BMD_FILE_VERTEX_ID viNorm;
            [SerializeField] public BMD_FILE_VERTEX_ID viTex;
            [SerializeField] public BMD_FILE_VERTEX_ID viMem; // Used in main after model is loaded
            short trash2; // alignment shit =)
            // memory below is not used by main

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            byte[] trash3; //28
            public static BMD_FILE_TRIANGLE FromBytes(byte[] data, ref int position)
            {
                var bmdFileTriangle = new BMD_FILE_TRIANGLE();
                bmdFileTriangle.count = data[position];
                bmdFileTriangle.trash = data[position + 1];
                position += 2;
                bmdFileTriangle.viPos = BMD_FILE_VERTEX_ID.FromBytes(data, ref position);
                bmdFileTriangle.viNorm = BMD_FILE_VERTEX_ID.FromBytes(data, ref position);
                bmdFileTriangle.viTex = BMD_FILE_VERTEX_ID.FromBytes(data, ref position);
                bmdFileTriangle.viMem = BMD_FILE_VERTEX_ID.FromBytes(data, ref position);
                bmdFileTriangle.trash2 = BitConverter.ToInt16(data, position);
                position += 2;
                bmdFileTriangle.trash3 = new byte[28];
                Array.Copy(data, position, bmdFileTriangle.trash3, 0, 28);
                position += 28;
                return bmdFileTriangle;
            }

        }


        // BMD animation info
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BMD_FILE_ANIM
        {
            public short nFrameCnt;
            public char bFlag;
            public static BMD_FILE_ANIM FromBytes(byte[] data, ref int position)
            {
                BMD_FILE_ANIM fileAnim = new BMD_FILE_ANIM();
                fileAnim.nFrameCnt = BitConverter.ToInt16(data, position);
                position += 2;
                fileAnim.bFlag = (char)data[position];
                position++;
                return fileAnim;
            }
        }

        // BMD bone info
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BMD_FILE_BONE
        {
            public byte IsNull;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] sName; //32
            // public string sName;
            public short Parent;
            public BMD_FILE_BONE FromPtr(ref IntPtr ptr)
            {
                IsNull = Marshal.ReadByte(ptr);
                ptr += 1;
                if (IsNull == 0)
                {
                    sName = new char[32];
                    for (int i = 0; i < 32; i++)
                    {
                        sName[i] = (char)Marshal.ReadByte(ptr);
                        ptr += 1;
                    }
                    // byte[] name = new byte[32];
                    // Marshal.Copy(ptr,name,0,32);
                    // sName = System.Text.Encoding.ASCII.GetString(name);
                    // ptr+=32;
                    Parent = Marshal.ReadInt16(ptr);
                    ptr += 2;
                }
                else
                {
                    sName = new char[] { '\0' };
                    Parent = 0;
                }
                return this;
            }
        }
        [Serializable]
        public struct BMD_MESH
        {
            [SerializeField] public BMD_FILE_MESH MeshInfo;
            [SerializeField] public BMD_FILE_VERTEX_POS[] lpVertPos;
            [SerializeField] public BMD_FILE_VERTEX_NORM[] lpVertNorm;
            [SerializeField] public BMD_FILE_VERTEX_TEX[] lpVertTex;
            [SerializeField] public BMD_FILE_TRIANGLE[] lpTrngl;
            [SerializeField] public char[] szTextureName;//[BMD_FILE_TEXTURENAME_SIZE + 1]; 33

            public BMD_MESH FromPtr(ref IntPtr ptr)
            {
                szTextureName = new char[32];
                MeshInfo = Marshal.PtrToStructure<BMD_FILE_MESH>(ptr);
                ptr += Marshal.SizeOf(MeshInfo);

                lpVertPos = new BMD_FILE_VERTEX_POS[MeshInfo.nVertPos];
                lpVertNorm = new BMD_FILE_VERTEX_NORM[MeshInfo.nVertNorm];
                lpVertTex = new BMD_FILE_VERTEX_TEX[MeshInfo.nVertTex];
                lpTrngl = new BMD_FILE_TRIANGLE[MeshInfo.nTrngl];

                int size = Marshal.SizeOf(new BMD_FILE_VERTEX_POS());
                for (int j = 0; j < MeshInfo.nVertPos; j++)
                {
                    lpVertPos[j] = Marshal.PtrToStructure<BMD_FILE_VERTEX_POS>(ptr);
                    ptr += size;
                }

                size = Marshal.SizeOf(new BMD_FILE_VERTEX_NORM());
                for (int j = 0; j < MeshInfo.nVertNorm; j++)
                {
                    lpVertNorm[j] = Marshal.PtrToStructure<BMD_FILE_VERTEX_NORM>(ptr);
                    ptr += size;
                }

                size = Marshal.SizeOf(new BMD_FILE_VERTEX_TEX());
                for (int j = 0; j < MeshInfo.nVertTex; j++)
                {
                    lpVertTex[j] = Marshal.PtrToStructure<BMD_FILE_VERTEX_TEX>(ptr);
                    ptr += size;

                }

                size = Marshal.SizeOf(new BMD_FILE_TRIANGLE());
                for (int j = 0; j < MeshInfo.nTrngl; j++)
                {
                    lpTrngl[j] = Marshal.PtrToStructure<BMD_FILE_TRIANGLE>(ptr);
                    ptr += size;

                }

                for (int j = 0; j < 32; j++)
                {
                    szTextureName[j] = (char)Marshal.ReadByte(ptr);
                    ptr += 1;
                }

                //szTextureName = Marshal.PtrToStructure<char[]>(ptr);

                return this;

            }
            public struct BMDVertices
            {
                public Vector3[] Vertices;
                public int[] parents;
                public BMDVertices(int length)
                {
                    this.Vertices = new Vector3[length];
                    this.parents = new int[length];
                }
            }

            public BMDVertices GetVertices()
            {
                int arraySize = lpTrngl.Length * 3;
                //UnityEngine.Vector3[] vertices = new UnityEngine.Vector3[arraySize];
                BMDVertices vertices1 = new BMDVertices(arraySize);
                for (int i = 0; i < lpTrngl.Length; i++)
                {
                    // vertices[i * 3 + 0] = lpVertPos[lpTrngl[i].viPos.v[0]].vPos;
                    // vertices[i * 3 + 1] = lpVertPos[lpTrngl[i].viPos.v[1]].vPos;
                    // vertices[i * 3 + 2] = lpVertPos[lpTrngl[i].viPos.v[2]].vPos;

                    vertices1.Vertices[i * 3 + 0] = lpVertPos[lpTrngl[i].viPos.v[0]].vPos;
                    vertices1.Vertices[i * 3 + 1] = lpVertPos[lpTrngl[i].viPos.v[1]].vPos;
                    vertices1.Vertices[i * 3 + 2] = lpVertPos[lpTrngl[i].viPos.v[2]].vPos;

                    vertices1.parents[i * 3 + 0] = lpVertPos[lpTrngl[i].viPos.v[0]].iParent;
                    vertices1.parents[i * 3 + 1] = lpVertPos[lpTrngl[i].viPos.v[1]].iParent;
                    vertices1.parents[i * 3 + 2] = lpVertPos[lpTrngl[i].viPos.v[2]].iParent;

                }
                return vertices1;
            }
            public int[] GetTriangles()
            {
                List<int> triangles = new List<int>();
                int i = 0;
                foreach (var tri in lpTrngl)
                {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                    i += 3;
                }
                return triangles.ToArray();
            }
            public Vector2[] GetUV()
            {
                int arraySize = lpTrngl.Length * 3;
                UnityEngine.Vector2[] vertices = new UnityEngine.Vector2[arraySize];
                for (int i = 0; i < lpTrngl.Length; i++)
                {
                    vertices[i * 3 + 0] = lpVertTex[lpTrngl[i].viTex.v[0]].vTex.MultiplyMember(1, -1);
                    vertices[i * 3 + 1] = lpVertTex[lpTrngl[i].viTex.v[1]].vTex.MultiplyMember(1, -1);
                    vertices[i * 3 + 2] = lpVertTex[lpTrngl[i].viTex.v[2]].vTex.MultiplyMember(1, -1);
                }
                return vertices;
            }
            public Vector3[] GetNomals()
            {
                int arraySize = lpTrngl.Length * 3;
                UnityEngine.Vector3[] vertices = new UnityEngine.Vector3[arraySize];
                for (int i = 0; i < lpTrngl.Length; i++)
                {
                    vertices[i * 3 + 0] = lpVertNorm[lpTrngl[i].viNorm.v[0]].vNorm;
                    vertices[i * 3 + 1] = lpVertNorm[lpTrngl[i].viNorm.v[1]].vNorm;
                    vertices[i * 3 + 2] = lpVertNorm[lpTrngl[i].viNorm.v[2]].vNorm;
                }
                return vertices;
            }
        }

        [Serializable]
        public struct BMD_ANIM
        {
            public BMD_FILE_ANIM AnimInfo;
            public Vector3[] lpVector3;
            public BMD_ANIM FromPtr(ref IntPtr ptr)
            {
                AnimInfo = Marshal.PtrToStructure<BMD_FILE_ANIM>(ptr);
                ptr += Marshal.SizeOf(AnimInfo);
                if (AnimInfo.bFlag != '\0')
                {
                    lpVector3 = new Vector3[AnimInfo.nFrameCnt];
                    for (int i = 0; i < AnimInfo.nFrameCnt; i++)
                    {
                        lpVector3[i] = Marshal.PtrToStructure<Vector3>(ptr);
                        ptr += 12;
                    }
                }
                return this;
            }
            public static BMD_ANIM FromBytes(byte[] data, ref int position)
            {
                BMD_ANIM bmdAnim = new BMD_ANIM();
                bmdAnim.AnimInfo = BMD_FILE_ANIM.FromBytes(data, ref position);
                if (bmdAnim.AnimInfo.bFlag != '\0')
                {
                    bmdAnim.lpVector3 = new Vector3[bmdAnim.AnimInfo.nFrameCnt];
                    for (int i = 0; i < bmdAnim.AnimInfo.nFrameCnt; i++)
                    {
                        bmdAnim.lpVector3[i] = VectorExtension.FromBytesStatic(data, position);
                        position += 12;
                    }
                }
                return bmdAnim;
            }
        }
        [Serializable]
        public struct Vector3Array
        {
            public Vector3[] vector;
        }
        [Serializable]
        public struct Quaternion3Array
        {
            public Quaternion[] vector;
            public Vector3[] originalVector;
            public Vector3[] vector3s;
        }

        [Serializable]
        public struct BMD_BONE
        {
            public BMD_FILE_BONE BoneInfo;
            public Vector3Array[] lpVertPos;
            public Quaternion3Array[] lpVertRot;
            public BMD_BONE FromPtr(ref IntPtr ptr, ref BMD_ANIM[] anims)
            {
                // BoneInfo = Marshal.PtrToStructure<BMD_FILE_BONE>(ptr);
                // ptr += Marshal.SizeOf(BoneInfo);
                BoneInfo = BoneInfo.FromPtr(ref ptr);
                if (BoneInfo.IsNull == 0)
                {
                    lpVertPos = new Vector3Array[anims.Length];
                    lpVertRot = new Quaternion3Array[anims.Length];
                    for (int i = 0; i < anims.Length; i++)
                    {
                        int nFrm = anims[i].AnimInfo.nFrameCnt;
                        lpVertPos[i].vector = new Vector3[nFrm];
                        lpVertRot[i].vector = new Quaternion[nFrm];
                        lpVertRot[i].vector3s = new Vector3[nFrm];
                        lpVertRot[i].originalVector = new Vector3[nFrm];
                        for (int j = 0; j < nFrm; j++)
                        {
                            lpVertPos[i].vector[j] = Marshal.PtrToStructure<Vector3>(ptr);
                            ptr += 12;
                        }
                        for (int j = 0; j < nFrm; j++)
                        {
                            //lpVertRot[i].vector[j] = Marshal.PtrToStructure<Vector3>(ptr);
                            Vector3 rotRad = new Vector3();

                            rotRad = Marshal.PtrToStructure<Vector3>(ptr);
                            lpVertRot[i].originalVector[j] = rotRad;
                            ptr += 12;
                            lpVertRot[i].vector[j] = AngleQuaternion(rotRad);
                            rotRad *= Mathf.Rad2Deg;
                            // lpVertRot[i].vector3s[j] = j == 0 ? rotRad : rotRad.NearestAngleToPrevious(lpVertRot[i].vector3s[j - 1]);//.PositiveAngle()  ;
                            lpVertRot[i].vector3s[j] = j == 0 ? rotRad : rotRad.CountAndReverseAngle(lpVertRot[i].vector3s[j - 1]);
                            // lpVertRot[i].vector3s[j] = rotRad;
                        }

                    }
                }
                return this;
            }
            private Quaternion AngleQuaternion(Vector3 angles)
            {
                Quaternion quaternion = Quaternion.identity;
                float angle;
                float sr, sp, sy, cr, cp, cy;
                angles = angles * Mathf.Rad2Deg;

                // FIXME: rescale the inputs to 1/2 angle
                angle = angles.z * 0.5f;
                sy = Mathf.Sin(angle);
                cy = Mathf.Cos(angle);
                angle = angles.y * 0.5f;
                sp = Mathf.Sin(angle);
                cp = Mathf.Cos(angle);
                angle = angles.x * 0.5f;
                sr = Mathf.Sin(angle);
                cr = Mathf.Cos(angle);

                quaternion[0] = sr * cp * cy - cr * sp * sy; // X
                quaternion[1] = cr * sp * cy + sr * cp * sy; // Y
                quaternion[2] = cr * cp * sy - sr * sp * cy; // Z
                quaternion[3] = cr * cp * cy + sr * sp * sy; // W
                return Quaternion.Euler(angles);
            }
            public static BMD_BONE FromBytes(byte[] data, ref int position, ref BMD_ANIM[] animation)
            {
                return new BMD_BONE();
                // BMD_BONE bone = new BMD_BONE();
                // bone.BoneInfo = BMD_FILE_BONE.FromBytes(data, ref position);

                // if (bone.BoneInfo.IsNull != '\0')
                // {
                //     bone.lpVertPos = new Vector3[animation.Length][];
                //     bone.lpVertRot = new Vector3[animation.Length][];

                //     for (int k = 0; k < animation.Length; k++)
                //     {
                //         int nFrm = animation[k].AnimInfo.nFrameCnt;
                //         bone.lpVertPos[k] = new Vector3[nFrm];
                //         for (int j = 0; j < animation[k].AnimInfo.nFrameCnt; j++)
                //         {
                //             bone.lpVertPos[k][j] = VectorExtension.FromBytesStatic(data, position);
                //             position += 12;
                //         }

                //         bone.lpVertRot[k] = new Vector3[nFrm];
                //         for (int j = 0; j < animation[k].AnimInfo.nFrameCnt; j++)
                //         {
                //             bone.lpVertRot[k][j] = VectorExtension.FromBytesStatic(data, position);
                //             position += 12;

                //         }
                //     }
                // }
                // else
                // {
                //     Debug.Log($"Null bone {new string(bone.BoneInfo.sName)}");
                // }
                // return bone;
            }
        }


        public BMDHeader bmdHeader;
        public BMDFileInfo bmdFileInfo;
        public BMD_MESH[] meshes;
        public BMD_BONE[] bones;
        public BMD_ANIM[] anims;
        public void FromBytesM(byte[] data)
        {
            //IntPtr ptr = Marshal.AllocCoTaskMem(data.Length);
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            //Marshal.Copy(data, 0, ptr, data.Length);

            //  = Marshal.AllocCoTaskMem(Marshal.SizeOf(bmdHeader));
            try
            {
                bmdHeader = Marshal.PtrToStructure<BMDHeader>(ptr);
                ptr = ptr + Marshal.SizeOf(bmdHeader);





                if (bmdHeader.IsValid())
                {
                    if (bmdHeader.TypeCode == 0x0C)
                    {
                        int Size = BitConverter.ToInt32(data, 8);
                        ptr += 4;


                        byte[] decrypted = new byte[data.Length - 8];
                        Array.Copy(data, 8, decrypted, 0, decrypted.Length);
                        FileStream writerx = File.Create("./newObjectEncr.bmd");
                        writerx.Write(decrypted, 0, decrypted.Length);
                        writerx.Close();
                        data = Decryptor.Decrypt(decrypted, decrypted.Length);
                        FileStream writer = File.Create("./newObjectDecr.bmd");
                        writer.Write(data, 0, data.Length);
                        writer.Close();
                        handle.Free();



                        handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                        ptr = handle.AddrOfPinnedObject();

                    }
                    bmdFileInfo = Marshal.PtrToStructure<BMDFileInfo>(ptr);
                    ptr = ptr + Marshal.SizeOf(bmdFileInfo);

                    meshes = new BMD_MESH[bmdFileInfo.nMeshCnt];
                    bones = new BMD_BONE[bmdFileInfo.nBoneCnt];
                    anims = new BMD_ANIM[bmdFileInfo.nAnimCnt];
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        meshes[i].FromPtr(ref ptr);
                    }

                    for (int i = 0; i < anims.Length; i++)
                    {
                        anims[i].FromPtr(ref ptr);
                    }
                    for (int i = 0; i < bones.Length; i++)
                    {
                        bones[i].FromPtr(ref ptr, ref anims);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                handle.Free();
            }

        }



        // private byte[] Decrypt(byte[] data, int size)
        // {
        //     byte[] returnData = new byte[data.Length];
        //     char kk = (char)0x5E;
        //     byte[] key = new byte[]
        //     {
        // 0xD1, 0x73, 0x52, 0xF6,
        // 0xD2, 0x9A, 0xCB, 0x27,
        // 0x3E, 0xAF, 0x59, 0x31,
        // 0x37, 0xB3, 0xE7, 0xA2
        //     };
        //     int keys = key.Length;


        //     if (data == null || size < 1)
        //         return null;

        //     byte wMapKey = (byte)kk;
        //     for (int i = 0; i < size; i++)
        //     {
        //         returnData[i] = (byte)((data[i] ^ key[i % 16]) - (byte)wMapKey);
        //         //data[i] = (byte)((data[i] +(byte)wMapKey) ^ key[i % 16]) ;

        //         wMapKey = (byte)(data[i] + 0x3D);
        //         // byte a = (byte)(data[i] ^ key[i % keys]);
        //         // byte b = (byte)(a - k);
        //         //data[i] = (byte)(a-b);
        //         wMapKey = (byte)(wMapKey & 0xff);
        //     }
        //     return returnData;

        // }

    }

    public static class VectorExtension
    {
        public static Vector3 FromBytesStatic(byte[] data, int position)
        {
            Vector3 vector = new Vector3();
            vector.x = BitConverter.ToSingle(data, position);
            vector.y = BitConverter.ToSingle(data, position + 4);
            vector.z = BitConverter.ToSingle(data, position + 8);

            return vector;
        }
        public static Vector3 NearestAngleToPrevious(this Vector3 vector, Vector3 previousAngle)
        {
            Vector3 ret = new Vector3(
                IsRightDirection(vector.x, previousAngle.x) ? vector.x : 360 - vector.x,
                IsRightDirection(vector.y, previousAngle.y) ? vector.y : 360 - vector.y,
                IsRightDirection(vector.z, previousAngle.z) ? vector.z : 360 - vector.z
            );
            return ret;

        }
        private static bool IsRightDirection(float neededAngle, float previousAngle)
        {
            return Mathf.Abs(neededAngle - previousAngle) <= Mathf.Abs(360 + neededAngle - previousAngle);
        }
        public static Vector3 PositiveAngle(this Vector3 vector)
        {
            return new Vector3(vector.x.PositiveAngle(), vector.y.PositiveAngle(), vector.z.PositiveAngle());
        }
        public static Vector3 CountAndReverseAngle(this Vector3 value, Vector3 previousAngle)
        {

            // // var a = Vector3.SignedAngle(value, previousAngle,Vector3.right);
            // float angleX, angleY, angleZ;
            // angleX = Vector3.Angle(previousAngle, value);
            // // angleY = Vector3.SignedAngle(previousAngle, value, Vector3.up);
            // // angleZ = Vector3.SignedAngle(previousAngle, value, Vector3.forward);
            // return angleX < 180 ? value : new Vector3(value.x.ReverseAngle(), value.y.ReverseAngle(), value.z.ReverseAngle());
            // return new Vector3(
            //     value.x + angleX < 180 ? angleX : -angleX,
            //     value.y + angleY < 180 ? angleY : -angleY,
            //     value.z + angleZ < 180 ? angleZ : -angleZ);
            return new Vector3(value.x.AngleDifference(previousAngle.x), value.y.AngleDifference(previousAngle.y), value.z.AngleDifference(previousAngle.z));
            //FIXME: необходимо следующий угол реверснуть, если путь от предыдущего больше 180 градусов
        }
        public static float AngleDifference(this float angle1, float angle2)
        {
            float diff = angle2 - angle1;
            if(diff<180) diff+=360;
            if(diff>360) diff-=360;

            if (angle2 >= angle1)
                diff = angle2 - diff;
            else
                diff = angle2 + diff;
            return diff;
        }
        public static float PositiveAngle(this float value)
        {
            return value < 0 ? 360 + value : value;
        }
        public static float ReverseAngle(this float value)
        {
            return (value + 360) % 360;
        }
        public static float CountAndReverseAngle(this float value, float previousAngle)
        {
            return Mathf.Abs(previousAngle - value) <= Mathf.Abs(previousAngle.ReverseAngle() - value) ? value : value.ReverseAngle();
        }
        public static Vector2 FromBytes(byte[] data, ref int position)
        {
            var vector = new Vector2() { x = BitConverter.ToSingle(data, position), y = BitConverter.ToSingle(data, position + 4) };
            position += 8;
            return vector;
        }
        public static Vector2 MultiplyMember(this Vector2 vector2, float x, float y)
        {
            return new Vector2(vector2.x * x, vector2.y * y);
        }
    }
}