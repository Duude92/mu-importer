using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "ozt")]
    public class OZTCustomImport : ScriptedImporter
    {
        [SerializeField] private bool _asSprite = false;
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            if (_asSprite)
            {
                var sprite = LoadSprite(ctx.assetPath);
                ctx.AddObjectToAsset(sprite.texture.name, sprite.texture);
                ctx.AddObjectToAsset(sprite.name, sprite);
                ctx.SetMainObject(sprite);

            }
            else
            {
                var material = LoadMaterial(ctx.assetPath);

                ctx.AddObjectToAsset(material.mainTexture.name, material.mainTexture);
                ctx.SetMainObject(material.mainTexture);
                ctx.AddObjectToAsset(material.name, material);
            }
            var a = ctx.assetPath;
            var b = Selection.activeObject;
            var c = AssetDatabase.GetAssetPath(b);
            AssetDatabase.MakeEditable(a);

        }

        public static Sprite LoadSprite(string path)
        {
            string rootName = path;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            Stream file = File.OpenRead(path);


            var texture2D = LoadTGA(file);
            texture2D.name = rootName + ".tga";


            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
            sprite.name = rootName + ".sprite";

            return sprite;

        }
        public static Material LoadMaterial(string path)
        {
            string rootName = path;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            Stream file = File.OpenRead(path);


            var texture2D = LoadTGA(file);
            texture2D.name = rootName + ".tga";


            Material material = new Material(Shader.Find("Unlit/Transparent"));
            material.mainTexture = texture2D;

            material.name = rootName + ".tga_mat";
            return material;
        }
        public static Texture2D LoadTGA(string path)
        {
            string rootName = path;
            var split = rootName.Split('/');
            rootName = split[split.Length - 1];
            rootName = rootName.Split('.')[0];
            Stream file = File.OpenRead(path);


            var texture2D = LoadTGA(file);
            texture2D.name = rootName + ".tga";
            return texture2D;
        }
        private static Texture2D LoadTGA(Stream TGAStream)
        {

            using (BinaryReader r = new BinaryReader(TGAStream))
            {
                // Skip some header info we don't care about.
                // Even if we did care, we have to move the stream seek point to the beginning,
                // as the previous method in the workflow left it at the end.
                r.BaseStream.Seek(16, SeekOrigin.Begin);

                short width = r.ReadInt16();
                short height = r.ReadInt16();
                int bitDepth = r.ReadByte();

                // Skip a byte of header information we don't care about.
                r.BaseStream.Seek(1, SeekOrigin.Current);

                Texture2D tex = new Texture2D(width, height);
                Color32[] pulledColors = new Color32[width * height];

                if (bitDepth == 32)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        byte red = r.ReadByte();
                        byte green = r.ReadByte();
                        byte blue = r.ReadByte();
                        byte alpha = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, alpha);
                    }
                }
                else if (bitDepth == 24)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        byte red = r.ReadByte();
                        byte green = r.ReadByte();
                        byte blue = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, 1);
                    }
                }
                else
                {
                    throw new Exception("TGA texture had non 32/24 bit depth.");
                }

                tex.SetPixels32(pulledColors);
                tex.Apply();
                return tex;

            }
        }
    }

    public static class BitArrayExtension
    {
        public static void Reverse(this BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }
        }
    }
}