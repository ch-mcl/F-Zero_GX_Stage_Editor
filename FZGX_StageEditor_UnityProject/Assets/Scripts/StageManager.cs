﻿// Resources
//http://answers.unity3d.com/questions/8187/how-can-i-read-binary-files-from-resources.html

using UnityEngine;
using System.IO;
using GameCube.Games.FZeroGX;

namespace GameCube.Games.FZeroGX
{
    /// <summary>
    /// Singleton script for controlling stage loading and stream management
    /// </summary>
    [ExecuteInEditMode]
    public class StageManager : MonoBehaviour
    {
        public static FZeroGXStage currentStage = FZeroGXStage.MUTE_CITY_Twist_Road;
        public static FZeroGXStage lastStage = FZeroGXStage.EX_Victory_Lap;
        public static string resourcePath = "FZGX_EN/stage";

        // Used to modify how Vector3s are read from file to compensate for difference in winding
        public static readonly bool doInverseWindingPositionX = true;
        public static readonly bool doInverseWindingRotationX = true;
        public static readonly bool doInverseWindingScaleX = false;
        public static readonly bool doInverseWindingNormalX = true;
        public static readonly float alpha = 0.5f;

        // Stage file
        private static TextAsset stageFile;
        private static Stream fileStream;
        public static BinaryReader Reader { get; private set; }

        private static StageManager current;
        public static StageManager Current
        {
            get
            {
                // Get instance to Singleton if missing
                if (current == null)
                    current = FindObjectOfType<StageManager>();

                return current;
            }
        }

        // EXECUTE IN EDIT MODE
        private static void Start()
        {
            LoadStageFileAndSetStream();
        }


        private static void PrintCurrentStage()
        {
            Debug.LogFormat("Loaded: ({0}) {1}", ((int)currentStage).ToString("D2"), currentStage.ToString().Replace('_', ' '));
        }

        // Execute in edit mode
        public void Update()
        {
            if (currentStage != lastStage)
            {
                LoadStageFileAndSetStream();
                lastStage = currentStage;
            }
        }

        public static void ChangeStage(FZeroGXStage stage)
        {
            currentStage = stage;
            current.Update();
        }

        [UnityEditor.Callbacks.DidReloadScripts(int.MinValue)]
        private static void LoadStageFileAndSetStream()
        {
            string filename = string.Format("{1}/COLI_COURSE{0},lz", ((int)currentStage).ToString("D2"), resourcePath);

            try
            {
                foreach (MonoBehaviour listener in FindObjectsOfType<MonoBehaviour>())
                    if (listener is IFZGXEditorStageEventReceiver)
                        ((IFZGXEditorStageEventReceiver)listener).StageUnloaded(Reader);

                // Load file based on name. GX stores it's files as COLI_COURSE##,lz. If the number exceeds 99, the number
                // section grows with it. ie: COLI_COURSE###,lz
                stageFile = Resources.Load(filename) as TextAsset;

                // Load file as bytes
                fileStream = new MemoryStream(stageFile.bytes);
                // Set the reader's stream to the loaded file
                Reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8);

                foreach (var listener in FindObjectsOfType<MonoBehaviour>())
                    if (listener is IFZGXEditorStageEventReceiver)
                        ((IFZGXEditorStageEventReceiver)listener).StageLoaded(Reader);

                // Check to see if necessary
                if (current != null)
                    UnityEditor.EditorUtility.SetDirty(Current);
                PrintCurrentStage();
            }
            catch
            {
                Debug.Log($"Failed to load scene file at path '{filename}'");
            }
        }
    }
}