using System;
using System.IO;
using UnityEngine;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "ozj")]
    public class OZJCustomImport : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            Texture2D texture2D = LoadJPG(ctx.assetPath);

            ctx.AddObjectToAsset(texture2D.name, texture2D);
            Shader shader = null;// Shader.Find("Custom/NewShader");
            if (!shader)
                shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.mainTexture = texture2D;
            material.name = texture2D.name + "_mat";
            ctx.AddObjectToAsset(texture2D.name + "_mat", material);
            // if (rootName.ToLower() == "terrainlight")
            // {

            //     LightmapData lmd = new LightmapData();
            //     lmd.lightmapColor = texture2D;
            //     ctx.AddObjectToAsset(rootName + ".lightmap", lmd);

            // }
        }
        public static Texture2D LoadJPG(string path)
        {
            string rootName = path;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            FileStream file = File.OpenRead(path);
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, (int)file.Length);
            file.Close();
            // ushort identifier = BitConverter.ToUInt16(buffer, 0);
            byte[] imageBuffer;
            // if (identifier == 55551)
            // {
            imageBuffer = new byte[buffer.Length - 0x18];
            Array.Copy(buffer, 0x18, imageBuffer, 0, imageBuffer.Length);
            // }
            // else
            // {
            //     imageBuffer = new byte[buffer.Length - 24];
            //     Array.Copy(buffer, 24, imageBuffer, 0, imageBuffer.Length);

            // }

            int height = imageBuffer[0x202];
            int width = imageBuffer[0x204];

            Texture2D texture2D = new Texture2D(0, 0, TextureFormat.ARGB32, -1, false);
            texture2D.LoadImage(imageBuffer);
            texture2D.name = rootName + ".jpg";
            texture2D.Apply(true, false); 
            return texture2D;
        }
    }
}
