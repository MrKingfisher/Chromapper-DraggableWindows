using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;

namespace DraggableWindows
{

    [Plugin("Draggable Windows")]
    public class Plugin
    {
        public bool shiftEnabled = false;
        protected const string DraggableWindowsSettingsFile = "DraggableWindowsSettings.json";
        [Init]
        [Obsolete]
        private void Init()
        {
            // HUH? THIS LOOKS KINDA UGLY BUT IS USEFULL
            CheckChromapperVersion();
            EnsureJsonfileExists();

            try
            {
                Sprite buttonSprite;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DraggableWindows.ShiftToggleButton.png"))
                {
                    var len = (int)stream.Length;
                    var bytes = new byte[len];
                    stream.Read(bytes, 0, len);

                    Texture2D texture2D = new Texture2D(512, 512);
                    texture2D.LoadImage(bytes);

                    buttonSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);
                }
                SceneManager.sceneLoaded += SceneLoaded;
                ExtensionButton shiftToggleButton = ExtensionButtons.AddButton(buttonSprite, "Toggle Shift+Drag when dragging windows", ToggleShiftDrag);
            }catch (Exception ex) { Debug.LogError(ex.Message + ex.StackTrace); }
        }

        [Obsolete]
        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex == 3)
            {
                GameObject mapEditorUI = GameObject.Find("MapEditorUI");

                List<Transform> children = new List<Transform>(mapEditorUI.GetComponentsInChildren<Transform>());
                foreach (var child in children)
                {
                    if (child.GetComponent<CanvasScaler>() != null)
                    {
                        switch (child.name)
                        {
                            case "Node Editor Canvas":
                                AddDraggable(child.Find("Node Editor").gameObject, child.gameObject);
                                break;
                            case "Strobe Generator Canvas":
                                AddDraggable(child.Find("Strobe Generator").gameObject, child.gameObject);
                                break;
                            case "Chroma Colour Selector":
                                AddDraggable(child.Find("Chroma Colour Selector").Find("Picker 2.0").gameObject, child.gameObject);
                                break;
                            case "BPM Tapper Canvas":
                                AddDraggable(child.Find("BPM Tapper").Find("Background Panel").gameObject, child.gameObject);
                                break;
                        }
                    }
                }
            }
        }
        public void ToggleShiftDrag()
        {
            shiftEnabled = !shiftEnabled;
            if (shiftEnabled == true)
            {
                PersistentUI.Instance.DisplayMessage("Shift + Drag has been enabled", PersistentUI.DisplayMessageType.Bottom);
            }
            else
            {
                PersistentUI.Instance.DisplayMessage("Shift + Drag has been disabled", PersistentUI.DisplayMessageType.Bottom);
            }

            JSONObject saveJson = new JSONObject();
            saveJson.Add("ShiftEnabled", shiftEnabled);
            var path = Application.persistentDataPath + "/DraggableWindowsSettings.json";
            File.WriteAllText(path, saveJson.ToString());
        }
        public void AddDraggable(GameObject ui, GameObject parentUi)
        {
            Debug.Log("creating draggable");
            ui.AddComponent<DragWindow>();
            ui.GetComponent<DragWindow>().canvas = parentUi.GetComponent<Canvas>();
        }
        // if build version < current version of chromapper we warn user of potential bugs n such
        private void CheckChromapperVersion()
        {
            var appVersion = Application.version;
            var assemblyBuildReleaseVersion = GetAssemblyBuildReleaseVersion();
            var assembleTitle = GetAssembleTitle();
            if (assemblyBuildReleaseVersion != null)
            {
                // Compare the versions
                int comparisonResult = CompareVersions(appVersion, assemblyBuildReleaseVersion);

                if (comparisonResult != 0)
                {
                    Debug.LogError("Warning! Plugin: "+ assembleTitle +" was built on Chromapper Version: " + assemblyBuildReleaseVersion + "\n" +
                        "Your version of Chromapper is: " + appVersion + " and may not work properly or even at all.");
                }
            }
            else
            {
                Debug.LogError("AssemblyBuildReleaseVersion attribute not found. something bad happend Please send log if possible");
            }

        }

        // a bit overkill but if some users believe they get banned from mapping due to plugin mismatch
        // then this is for a good reason
        private string GetAssemblyBuildReleaseVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attribute = assembly.GetCustomAttribute<AssemblyBuildReleaseVersionAttribute>();

            return attribute?.Version;
        }
        private string GetAssembleTitle()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();

            return attribute?.Title;
        }

        // compare versions
        private int CompareVersions(string appVersion, string pluginVersion)
        {
            Version appVer = new Version(appVersion);
            Version pluginVer = new Version(pluginVersion);

            return appVer.CompareTo(pluginVer);
        }

        // Locate Appdata folder for chromapper
        public static string GetAppdataFolder()
        {
            return Application.persistentDataPath.ToString();
        }
        private void EnsureJsonfileExists()
        {
            string AppdataPath = GetAppdataFolder();
            if (!File.Exists(AppdataPath + "/" + DraggableWindowsSettingsFile))
            {
                // Create Json file under app data if not found
                File.Create(AppdataPath + "/" + DraggableWindowsSettingsFile);
                Debug.Log(DraggableWindowsSettingsFile + " has been created in: " + AppdataPath);
            }
        }

    }

}
