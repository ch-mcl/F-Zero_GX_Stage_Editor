﻿// Created by Raphael "Stark" Tetreault /2017
// Copyright (c) 2017 Raphael Tetreault
// Last updated 

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using GameCube.LibGxTexture;
using System.Text.RegularExpressions;

namespace GameCube.Games.FZeroGX.FileStructures
{
    public class TexturePaletteLibrary_ScriptableObject : ScriptableObject, IBinarySerializable
    {
        [SerializeField]
        private uint numDescriptors;
        [SerializeField]
        private TEXDescriptor[] descriptorArray;
        [SerializeField]
        private Texture2D[] textures;

        public void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
        public void Deserialize(BinaryReader reader)
        {
            numDescriptors = reader.GetUInt32();
            descriptorArray = new TEXDescriptor[numDescriptors];
            for (uint i = 0; i < descriptorArray.Length; i++)
            {
                descriptorArray[i] = new TEXDescriptor();
                descriptorArray[i].Deserialize(reader);
            }

            // TEMP - SHOULD REGEX EXTENSION(S), ',LZ' PSEUDO EXTENSION(S)
            string filePath = AssetDatabase.GetAssetPath(this.GetInstanceID());
            //string filePathWithoutExtensions = Regex.Match(filePath, "/.+?(?=,)/").Value;
            //filePathWithoutExtensions = Regex.Match(filePath, "/.+?(?=/.)/").Value;
            string directory = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)).PathToUnityPath();

            if (!Directory.Exists(directory))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));

            textures = new Texture2D[numDescriptors];
            for (int i = 0; i < numDescriptors; i++)
                textures[i] = ReadTexture(reader, descriptorArray[i], directory, i.ToString());
        }

        public Texture2D ReadTexture(BinaryReader reader, TEXDescriptor desc, string directory, string number)
        {
            // Verify if index is valid (some entries can be nulled out)
            // We check for 0 specifically because garbage can be store in the
            // first few entries of the file.
            if (desc.isNullEntry != 0)
                return null;

            // We use Try as GxTextureFormatCodec.GetCodec() can return an error if the type is invalid
            try
            {
                GxTextureFormatCodec codec = GxTextureFormatCodec.GetCodec((GxTextureFormat)desc.format);
                reader.BaseStream.Position = desc.dataPtr;
                byte[] texRaw = reader.GetBytes(codec.CalcTextureSize(desc.width, desc.height));
                byte[] texRGBA = new byte[4 * desc.width * desc.height]; // RGBA (4 bytes) * w * h
                codec.DecodeTexture(texRGBA, 0, desc.width, desc.height, desc.width * 4, texRaw, 0, null, 0);

                // Reconstruct Texture using Unity's format
                Texture2D texture = new Texture2D(desc.width, desc.height);
                for (int y = 0; y < desc.height; y++)
                {
                    for (int x = 0; x < desc.width; x++)
                    {
                        // Invert Y because LibGXTexture returns array upside-down?
                        // ei 'x, (desc.width - y)' instead of 'x, y'
                        texture.SetPixel(x, (desc.width - y), new Color32(
                            texRGBA[(y * desc.width + x) * 4 + 0],
                            texRGBA[(y * desc.width + x) * 4 + 1],
                            texRGBA[(y * desc.width + x) * 4 + 2],
                            texRGBA[(y * desc.width + x) * 4 + 3]));
                    }
                }

                // TEMP - SHOULD REGEX ,lz
                string fileName = Path.GetFileNameWithoutExtension(directory).PathToUnityPath();

                string assetPath = string.Format("{0}/tex_tpl_{3}_{1}_{2}.png", directory, number, (GxTextureFormat)desc.format, fileName).PathToUnityPath();
                //string assetPath = string.Format("{0}/{1}.png", directory, fileName).PathToUnityPath();
                byte[] imageBytes = texture.EncodeToPNG();
                using (BinaryWriter writer = new BinaryWriter(File.Create(assetPath, imageBytes.Length)))
                {
                    writer.Write(imageBytes);
                }

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh(ImportAssetOptions.Default);
                Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
                return tex;
            }
            catch
            {
                Debug.LogErrorFormat("GxTextureFormatCodec.GetCodec() failed to find format [{0}]", desc.format.ToString("X"));
                return null;
            }
        }
        public bool WriteTexture(BinaryWriter writer, int index, Texture2D texture)
        {
            throw new System.NotImplementedException();
        }

        [Serializable]
        public class TEXDescriptor : IBinarySerializable
        {
            public ushort pad16;
            public byte isNullEntry;
            public byte format;
            public uint dataPtr; // Reader Address
            public ushort width;
            public ushort height;
            public ushort powerOf;
            public ushort endianness; // 1234 instead of 3412

            public void Serialize(BinaryWriter writer)
            {
                throw new System.NotImplementedException();
            }
            public void Deserialize(BinaryReader reader)
            {
                pad16 = reader.GetUInt16();
                isNullEntry = reader.GetByte();
                format = reader.GetByte();
                dataPtr = reader.GetUInt32();
                width = reader.GetUInt16();
                height = reader.GetUInt16();
                powerOf = reader.GetUInt16();
                endianness = reader.GetUInt16();
            }
        }
    }
}