using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "ozb")]
    public class OZBCustomImport : UnityEditor.AssetImporters.ScriptedImporter
    {
        const int TERRAIN_SIZE = 256;
        const float g_fMinHeight = -500f;
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {

            string rootName = ctx.assetPath;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];

            var height = CreateTerrain(ctx.assetPath);

            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 256;
            terrainData.size = new Vector3(256, 3.8f, 256);
            terrainData.SetHeights(0, 0, height);
            terrainData.name = rootName + ".data";
            ctx.AddObjectToAsset(rootName + ".data", terrainData);
        }
        public static float[,] CreateTerrain(string path)
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
            bool newTerrrain = false;
            var height = newTerrrain ? OpenTerrainNew(buffer) : OpenTerrain(buffer);

            return height;
        }
        private static float[,] OpenTerrain(byte[] buffer)
        {
            int Index = 1080;
            int Size = 256 * 256 + Index;
            byte[] terrainRaw = new byte[Size];
            Array.Copy(buffer, 4, terrainRaw, 0, Size - 4);
            byte[] BMPHeader = new byte[Index];
            Array.Copy(terrainRaw, BMPHeader, Index);

            // float[] heights = new float[256 * 256];

            float[,] height = new float[TERRAIN_SIZE, TERRAIN_SIZE];
            float maxHeight = 256 * 1.5f;

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    // heights[i * 256 + j] = terrainRaw[Index + i * 256 + j];
                    height[i, j] = Normalize(terrainRaw[Index + (i * 256 + j)] * 1.5f, maxHeight);

                }
            }
            // FileStream writer = File.OpenWrite("./map.ozb");
            // byte[] raw = new byte[heights.Length * 40];
            // for (int i = 0; i < heights.Length; i++)
            // {
            //     Array.Copy(BitConverter.GetBytes(heights[i]), 0, raw, i * 4, 4);
            // }
            // writer.Write(raw, 0, raw.Length);
            // writer.Close();


            return height;

        }
        private static float Normalize(float inFloat, float maxHeight)
        {
            return inFloat / maxHeight;
        }
        private static float[,] OpenTerrainNew(byte[] buffer)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            int baseptr = ((int)ptr);

            int baseAddress = 4;
            ptr += 4;
            BITMAPINFOHEADER bmiHeader = Marshal.PtrToStructure<BITMAPINFOHEADER>(ptr);
            ptr += Marshal.SizeOf(bmiHeader);
            baseAddress += Marshal.SizeOf(bmiHeader);
            BITMAPFILEHEADER header = Marshal.PtrToStructure<BITMAPFILEHEADER>(ptr);
            ptr += Marshal.SizeOf(header);

            baseAddress += Marshal.SizeOf(header);

            float[,] height = new float[TERRAIN_SIZE, TERRAIN_SIZE];
            float[] heights = new float[256 * 256];

            for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; ++i)
            {
                byte[] src = new byte[4];
                Array.Copy(buffer, baseAddress + i * 3, src, 1, 3);
                Array.Reverse(src);
                int x, y;
                x = (int)i / 256;
                y = i - x * 256;
                heights[i] = BitConverter.ToSingle(src, 0) + g_fMinHeight;
                height[x, y] = BitConverter.ToSingle(src, 0) + g_fMinHeight;
            }
            FileStream writer = File.OpenWrite("./map.ozb");
            byte[] raw = new byte[heights.Length * 40];
            for (int i = 0; i < heights.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(heights[i]), 0, raw, i + 4, 4);
            }
            writer.Write(raw, 0, raw.Length);
            writer.Close();
            return height;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BITMAPINFOHEADER
        {
            int biSize;
            long biWidth;
            long biHeight;
            short biPlanes;
            short biBitCount;
            int biCompression;
            int biSizeImage;
            long biXPelsPerMeter;
            long biYPelsPerMeter;
            int biClrUsed;
            int biClrImportant;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BITMAPFILEHEADER
        {
            short bfType;
            int bfSize;
            short bfReserved1;
            short bfReserved2;
            int bfOffBits;
        }
    }
}