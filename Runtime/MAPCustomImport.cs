using System;
using System.IO;
using UnityEngine;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "map")]
    public class MAPCustomImport : UnityEditor.AssetImporters.ScriptedImporter
    {
        //Terrain painting 
        //TODO: class not completed
        const int TERRAIN_SIZE = 256;
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            (byte[] map1, byte[] map2, float[] alpha) = OpenMap(ctx.assetPath);
            Texture2D t1 = new Texture2D(256, 256, TextureFormat.R8, 1, false);
            t1.LoadRawTextureData(map1);
            t1.Apply();
            Texture2D t2 = new Texture2D(256, 256, TextureFormat.R8, 1, false);
            t2.LoadRawTextureData(map2);
            t2.Apply();
            ctx.AddObjectToAsset("aboba1" + ".data", t1);
            ctx.AddObjectToAsset("aboba2" + ".data", t2);

            // TerrainData terrainData = new TerrainData();
            // terrainData.heightmapResolution = 256;
            // terrainData.SetHeights(0, 0, heights);
            // terrainData.name = rootName + ".data";
            // ctx.AddObjectToAsset(rootName + ".data", terrainData);

        }
        public static (byte[], byte[], float[]) OpenMap(string path)
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

            byte[] decrypted = Decryptor.Decrypt(buffer, (int)fileSize);    //

            // decrypted = Decryptor.BuxConvert(decrypted, (int)fileSize);

            FileStream writer = File.Create("./mapDecrypted.map");
            writer.Write(decrypted, 0, decrypted.Length);
            writer.Close();


            int DataPtr = 0;
            DataPtr += 1;

            int iMapNumber = (int)((byte)decrypted[DataPtr]);
            DataPtr++;
            byte[] TerrainMappingLayer1, TerrainMappingLayer2;
            TerrainMappingLayer1 = new byte[256 * 256];
            TerrainMappingLayer2 = new byte[256 * 256];
            float[] TerrainMappingAlpha = new float[256 * 256];
            Array.Copy(decrypted, DataPtr, TerrainMappingLayer1, 0, 256 * 256);
            DataPtr += 256 * 256;
            Array.Copy(decrypted, DataPtr, TerrainMappingLayer2, 0, 256 * 256);
            DataPtr += 256 * 256;

            for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; i++)
            {
                byte Alpha;
                Alpha = decrypted[DataPtr]; DataPtr += 1;
                TerrainMappingAlpha[i] = (float)(Alpha / 255f);
            }
            return (TerrainMappingLayer1, TerrainMappingLayer2, TerrainMappingAlpha);
        }
    }
}