using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace MUImporter
{
    public class ObjCustomImport
    {
        public static TerrainObject[] OpenObj(string path)
        {
            string rootName = path;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            FileStream file = File.OpenRead(path);
            long fileSize = file.Length;
            byte[] buffer = new byte[fileSize];
            file.Read(buffer, 0, (int)fileSize);
            file.Close();
            byte[] data;
            // if (buffer[0] == 137)
            // {
            //     data = Decryptor.BuxConvert(buffer, (int)fileSize);
            // }
            // else
            {

                data = Decryptor.Decrypt(buffer, (int)fileSize);    //
            }
            // if (data[0] == 0xfa)
            //     data = Decryptor.BuxConvert(data,data.Length);

            FileStream writer = File.Create("./objDecrypted.obj");
            writer.Write(data, 0, data.Length);
            writer.Close();

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            int DataPtr = 0;
            DataPtr += 1;
            int iMapNumber = (int)((byte)data[DataPtr]);
            DataPtr++;//(int)*((BYTE*)(Data + DataPtr)); DataPtr += 1;     //
            short Count = BitConverter.ToInt16(data, DataPtr);
            DataPtr += 2;//(int)*((BYTE*)(Data + DataPtr)); DataPtr += 1;     //
            TerrainObject[] objects = new TerrainObject[Count];
            ptr += DataPtr;

            for (int i = 0; i < Count; i++)
            {
                objects[i] = Marshal.PtrToStructure<TerrainObject>(ptr);
                ptr += Marshal.SizeOf(typeof(TerrainObject));


            }
            return objects;

        }
    }
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerrainObject
    {
        public short Type;
        public Vector3 Position;
        public Vector3 Angle;
        public float Scale;
        public void CreateObject(string path, GameObject parent)
        {
            int i = (int)(Position.x / (16 * 256));
            int j = (int)(Position.y / (16 * 256));
            if (i < 0 || j < 0 || i >= 16 || j >= 16) return;
            var objectNum = Type < 9 ? "0" + (Type + 1) : "" + (Type + 1);
            string path0 = $"{path}/Object{objectNum}.bmd";
            var currObject = AssetDatabase.LoadAssetAtPath<GameObject>(path0);

            if (!currObject)
            {
                Debug.LogWarning($"Object{objectNum} not found");
                return;
            }
            Vector3 pos = new Vector3(Position.x, Position.z, Position.y);
            Vector3 rot = new Vector3(-(Angle.x + 90), -Angle.z, Angle.y);
            GameObject obj = (GameObject)GameObject.Instantiate(currObject, pos, Quaternion.Euler(rot), parent.transform);
            var bmd = obj.GetComponent<BMD>();
            bmd.startPos = Position;
            bmd.startRot = Angle;
            obj.transform.localScale = new Vector3(Scale, -Scale, Scale);
        }
    }
}