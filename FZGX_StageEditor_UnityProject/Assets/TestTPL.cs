﻿// Created by Raphael "Stark" Tetreault /2017
// Copyright (c) 2017 Raphael Tetreault
// Last updated 

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCube.Games.FZeroGX.ImportExport;
using GameCube.Games.FZeroGX.FileStructures;
using UnityEditor;

public class TestTPL : FZGX_ImporterExporter
{
    [SerializeField]
    private string resourcePath = "TPL";
    [SerializeField]
    private int stageIndex;
    [SerializeField]
    private TPL tpl;

    public string filename
    {
        get
        {
            return string.Format("{1}/st{0},lz", ((int)stageIndex).ToString("D2"), resourcePath);
        }
    }

    public override void Import()
    {
        tpl = new TPL(GetStreamFromFile(filename));
        Export();
    }
    public override void Export()
    {
        if (tpl != null)
        {
            Texture2D tex;
            for (int i = 0; i < tpl.NumDescriptors; i++)
                tpl.ReadTextureFromTPL(GetStreamFromFile(filename), i, out tex, (i).ToString("X"));
        }
    }
}

//[CustomEditor(typeof(TestTPL))]
//public class TestTPL_Editor : Editor
//{

//}