using System;
using UnityEngine;
using BepInEx;
using TMPro;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PluginConfig;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;
using PluginConfig.API;
using System.ComponentModel;

namespace FPSTracker
{
    public static class ConfigManager
    {
        private static PluginConfigurator config;
        public static BoolField fpsDisplay;
        public static IntField fpsDisplaySampleSize;
        public static FloatField fpsDisplayTimeLimit;
        public static FloatField fpsDisplayOffsetX;
        public static FloatField fpsDisplayOffsetY;
        public static KeyCodeField fpsDisplayKeyBind;
        

        public static BoolField fpsGraph;
        public static FloatField fpsGraphTimeLimit;
        public static IntField fpsGraphTargetFps;
        public static IntField fpsGraphDataPoints;
        public static IntField fpsGraphQuality;
        public static BoolField fpsGraphMarker;
        public static FloatSliderField fpsGraphMarkerVisibility;
        public static FloatField fpsGraphOffsetX;
        public static FloatField fpsGraphOffsetY;
        public static KeyCodeField fpsGraphKeyBind;

        public static ButtonField fpsRefresh;
        static string workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string iconFilePath = Path.Combine(Path.Combine(workingDirectory, "Data"), "icon.png");
        public static void Setup()
        {
            config = PluginConfigurator.Create(PluginInfo.Name, PluginInfo.GUID);
            // FPS Display
            fpsDisplay = new BoolField(config.rootPanel, "FPS Display", "bool.fps_display", true);
            fpsDisplaySampleSize = new IntField(config.rootPanel, "FPS Display Sample Size", "int.fps_display_sample_size", 30, 1, 5318008);
            fpsDisplayTimeLimit = new FloatField(config.rootPanel, "FPS Display Time Limit", "float.fps_display_time_limit", 1f, 0, 42069);
            fpsDisplayOffsetX = new FloatField(config.rootPanel, "X Offset", "float.fps_display_offset_x", 0);
            fpsDisplayOffsetY = new FloatField(config.rootPanel, "Y Offset", "float.fps_display_offset_y", 0);
            fpsDisplayKeyBind = new KeyCodeField(config.rootPanel, "FPS Display Bind", "keycode.fps_display_key_bind", KeyCode.None);

            // FPS Graph
            fpsGraph = new BoolField(config.rootPanel, "FPS Graph", "bool.fps_graph", true);
            fpsGraphTimeLimit = new FloatField(config.rootPanel, "FPS Graph Time Limit", "float.fps_graph_time_limit", 0.5f, 0, 42069);
            fpsGraphTargetFps = new IntField(config.rootPanel, "FPS Graph Target FPS", "int.fps_graph_target_fps", 60, 1, 42069);
            fpsGraphDataPoints = new IntField(config.rootPanel, "FPS Graph Data Points", "int.fps_graph_data_points", 25, 2, 100000);
            fpsGraphQuality = new IntField(config.rootPanel, "FPS Graph Quality", "int.fps_graph_quality", 185, 1, 100000);
            fpsGraphOffsetX = new FloatField(config.rootPanel, "X Offset", "float.fps_graph_offset_x", 0);
            fpsGraphOffsetY = new FloatField(config.rootPanel, "Y Offset", "float.fps_graph_offset_y", 0);
            fpsGraphKeyBind = new KeyCodeField(config.rootPanel, "FPS Graph Bind", "keycode.fps_graph_key_bind", KeyCode.None);
            fpsGraphMarker = new BoolField(config.rootPanel, "FPS Graph Marker", "bool.fps_graph_marker", true);
            fpsGraphMarkerVisibility = new FloatSliderField(config.rootPanel, "FPS Graph Marker Visibility", "float_slider.fps_graph_marker_visiblility", new Tuple<float, float>(0, 1f), 0.5f, 2);//

            fpsRefresh = new ButtonField(config.rootPanel, "Refresh Displays", "button.refresh");
            fpsRefresh.onClick += new ButtonField.OnClick(Plugin.Refresh);

            fpsDisplay.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().stuff.SetActive(e.value);
                    fpsDisplaySampleSize.hidden = !e.value;
                    fpsDisplayTimeLimit.hidden = !e.value;
                    fpsDisplayOffsetX.hidden = !e.value;
                    fpsDisplayOffsetY.hidden = !e.value;
                }
            };

            fpsDisplaySampleSize.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().sampleSize = e.value;
                }
            };

            fpsDisplayTimeLimit.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().sampleTimeLimit = e.value;
                }
            };

            fpsDisplayOffsetX.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().Offset(e.value, fpsDisplayOffsetY.value);
                }
            };

            fpsDisplayOffsetY.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().Offset(fpsDisplayOffsetX.value, e.value);
                }
            };


            fpsGraph.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().stuff.SetActive(e.value);
                    if (e.value)
                    {
                        Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                    }
                }
            };

            fpsGraphTimeLimit.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().updateTime = e.value;
                }
            };

            fpsGraphTargetFps.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().targetFps = e.value;
                    Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                }
            };

            fpsGraphDataPoints.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().ChangeDataPointTotal(e.value);
                }
            };

            fpsGraphQuality.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().ResizeGraphTexture(e.value);
                    Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                }
            };

            fpsGraphMarker.onValueChange += (e) =>
            {
                fpsGraphMarker.value = e.value;
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                }
            };

            fpsGraphMarkerVisibility.onValueChange += (e) =>
            {
                fpsGraphMarkerVisibility.value = e.newValue;
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                }
            };

            fpsGraphOffsetX.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().Offset(e.value, fpsGraphOffsetY.value);
                }
            };

            fpsGraphOffsetY.onValueChange += (e) =>
            {
                if (Plugin.startup)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().Offset(fpsGraphOffsetX.value, e.value);
                }
            };


            ConfigManager.config.SetIconWithURL("file://" + iconFilePath);
        }
        public static void UpdateSettings()
        {
            if (Plugin.startup)
            {
                Plugin.fpsDisplay.GetComponent<FPSDisplay>().stuff.SetActive(fpsDisplay.value);
                if (fpsDisplay.value)
                {
                    Plugin.fpsDisplay.GetComponent<FPSDisplay>().UpdateText();
                }
                Plugin.fpsGraph.GetComponent<FPSGraph>().stuff.SetActive(fpsGraph.value);
                if (fpsGraph.value)
                {
                    Plugin.fpsGraph.GetComponent<FPSGraph>().PlotData();
                }
            }
        }
    }
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency(PluginConfiguratorController.PLUGIN_GUID)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle bundle;
        public static GameObject fpsDisplay;
        public static GameObject fpsCanvas;
        public static GameObject fpsGraph;
        Text fpsText;
        string scene;
        public static bool startup = false;
        public void Start()
        {
            Debug.Log("Im trippin");
            ConfigManager.Setup();
            scene = SceneHelper.CurrentScene;
            SceneManager.sceneLoaded += SceneChange;
            //DontDestroyOnLoad(fpsCanvas);
            //Instantiate(bundle.LoadAsset<GameObject>("FPS Display"));//
        }

        public void dob()
        {
            
        }

        public void SceneChange(Scene scene, LoadSceneMode mode)
        {
            if (startup == false)
            {
                startup = true;
                Debug.Log("Loading FPS Tracker");
                bundle = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(ModDir(), "Data"), "fps_assets"));
                fpsCanvas = Instantiate(bundle.LoadAsset<GameObject>("FPS Canvas"));
                DontDestroyOnLoad(fpsCanvas);
                fpsDisplay = Instantiate(bundle.LoadAsset<GameObject>("FPS Display"), fpsCanvas.transform);
                fpsDisplay.AddComponent<FPSDisplay>();
                fpsGraph = Instantiate(bundle.LoadAsset<GameObject>("FPS Graph"), fpsCanvas.transform);
                fpsGraph.AddComponent<FPSGraph>();
            }
        }

        public static void Refresh()
        {
            if (startup)
            {
                fpsDisplay.GetComponent<FPSDisplay>().Refresh();
                fpsGraph.GetComponent<FPSGraph>().Refresh();
                fpsGraph.GetComponent<FPSGraph>().PlotData();
            }
        }

        public void Update()
        {
            if (Input.GetKeyUp(ConfigManager.fpsDisplayKeyBind.value))
            {
                ConfigManager.fpsDisplay.value = !ConfigManager.fpsDisplay.value;
                ConfigManager.fpsDisplay.TriggerValueChangeEvent();
            }

            if (Input.GetKeyUp(ConfigManager.fpsGraphKeyBind.value))
            {
                ConfigManager.fpsGraph.value = !ConfigManager.fpsGraph.value;
                ConfigManager.fpsGraph.TriggerValueChangeEvent();
            }

            /*//samples[sampleCounter] = Time.unscaledDeltaTime;
            float currentFps = 1f / Time.unscaledDeltaTime;
            sampleTotal += currentFps;
            if (fpsGraph)
            {
                fpsGraph.GetComponent<FPSGraph>().AddToTotal(currentFps);
            }
            
            sampleCounter++;
            if (sampleCounter >= sampleSize)
            {
                sampleAverage = sampleTotal / sampleSize;
                if (fpsText)
                {
                    fpsText.text = Math.Round(sampleAverage).ToString();
                }
                //Debug.Log(Math.Round(sampleAverage));
                sampleCounter = 0;
                sampleAverage = 0;
                sampleTotal = 0;
            }*/
        }
        public static string ModDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
