using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "att")]
    public class AttCustomImport : UnityEditor.AssetImporters.ScriptedImporter
    {
        const int TERRAIN_SIZE = 256;
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {

            string rootName = ctx.assetPath;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            FileStream file = File.OpenRead(ctx.assetPath);
            long fileSize = file.Length;
            byte[] buffer = new byte[fileSize];
            file.Read(buffer, 0, (int)fileSize);
            file.Close();

            byte[] decrypted = Decryptor.Decrypt(buffer, (int)fileSize);    //

            decrypted = Decryptor.BuxConvert(decrypted, (int)fileSize);

            // FileStream writer = File.Create("./attDecrypted.att");
            // writer.Write(decrypted, 0, decrypted.Length);
            // writer.Close();

            bool extAtt = false;

            if (fileSize != 131076 && fileSize != 65540)
            {
                return;
            }

            if (fileSize == 131076)
            {
                extAtt = true;
            }
            GCHandle handle = GCHandle.Alloc(decrypted, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            byte Version = decrypted[0];
            int iMap = decrypted[1];
            byte Width = decrypted[2];
            byte Height = decrypted[3];
            ptr += 4;

            // if (extAtt == false)
            // {
            byte[] TWall = new byte[TERRAIN_SIZE * TERRAIN_SIZE];
            Marshal.Copy(ptr, TWall, 0, TERRAIN_SIZE * TERRAIN_SIZE);
            ptr += TERRAIN_SIZE * TERRAIN_SIZE;


            //     for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; ++i)
            //     {
            //         TerrainWall[i] = TWall[i];
            //     }
            // }
            // else
            // {
            //     memcpy(TerrainWall, &byBuffer[4], TERRAIN_SIZE * TERRAIN_SIZE * sizeof(WORD));
            // }
            bool Error = false;
            if (Version != 0 || Width != 255 || Height != 255)
            {
                Error = true;
            }
            float[,] height = new float[TERRAIN_SIZE, TERRAIN_SIZE];
            for (int i = 0; i < TERRAIN_SIZE; i++)
            {
                for (int j = 0; j < TERRAIN_SIZE; j++)
                {
                    height[i, j] = (float)TWall[(i * 256) + j];
                }
            }
            // for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; i++)
            // {
            //     WORD wWall = TerrainWall[i];
            //     //Wall = ( Wall ^ ( Wall & 8)) | ( (TerrainWall[i]&4) << 1);
            //     //Wall = ( Wall ^ ( Wall & 4)) | ( (TerrainWall[i]&8) >> 1);
            //     TerrainWall[i] = wWall;
            //     TerrainWall[i] = TerrainWall[i] & 0xFF;

            //     if ((BYTE)TerrainWall[i] >= 128)
            //         Error = true;
            // }
            if (Error)
            {
                return;
            }
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 256;
            terrainData.SetHeights(0, 0, height);
            terrainData.name = rootName+".data";
            ctx.AddObjectToAsset(rootName+".data",terrainData);

        }
    }
}