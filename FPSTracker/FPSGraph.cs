using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Audio;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static UnityEngine.GraphicsBuffer;

namespace FPSTracker
{
    public class FPSGraph : MonoBehaviour
    {
        public GameObject stuff;
        RectTransform graphContainer;
        Texture2D graphTexture;
        Sprite graphSprite;
        Image graphImage;
        Color[] clearStuff;
        float updateTimer = 0;
        public float updateTime = 0.1f;
        int fpsDataTotal = 75;
        int textureSize = 75;
        public int targetFps = 240;
        bool cheapGraph;
        float[] fpsData;
        int fpsDataCount = 0;
        List<GameObject> plottedPoints = new List<GameObject>();
        float sampleTotal = 0;
        int sampleCount = 0;
        int tableMax = 90;
        float xPos = 0;
        float yPos = 0;
        public void Start()
        {
            stuff = transform.Find("Stuff").gameObject;
            graphContainer = transform.Find("Stuff").Find("GraphContainer").gameObject.GetComponent<RectTransform>();
            fpsDataTotal = ConfigManager.fpsGraphDataPoints.value;
            textureSize = ConfigManager.fpsGraphQuality.value;
            xPos = gameObject.GetComponent<RectTransform>().anchoredPosition.x;
            yPos = gameObject.GetComponent<RectTransform>().anchoredPosition.y;
            Offset(ConfigManager.fpsGraphOffsetX.value, ConfigManager.fpsGraphOffsetY.value);
            fpsData = new float[fpsDataTotal];
            graphImage = transform.Find("Stuff").Find("GraphContainer").gameObject.GetComponent<Image>();//
            graphTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            graphSprite = Sprite.Create(graphTexture, new Rect(0, 0, graphTexture.width, graphTexture.height), new Vector2(0.5f, 0.5f));
            graphTexture.filterMode = FilterMode.Point;
            graphImage.sprite = graphSprite;
            clearStuff = new Color[graphTexture.width * graphTexture.height];
            cheapGraph = true;
            stuff.SetActive(ConfigManager.fpsGraph.value);

            updateTime = ConfigManager.fpsGraphTimeLimit.value;
            targetFps = ConfigManager.fpsGraphTargetFps.value;
            /*clearStuff = new Color[128*128];//
            for (int i = 0; i < clearStuff.Length; i++)
            {
                clearStuff[i] = Color.clear;
            }*/
            //DrawPog(new Vector2(10, 10));
            PlotData();
        }
        public void Update()
        {
            updateTimer += Time.unscaledDeltaTime;
            float currentfps = 1f / Time.unscaledDeltaTime;
            sampleTotal += currentfps;
            sampleCount++;
            if (updateTimer >= updateTime)
            {
                if (sampleCount > 0) // will probably never be zero but eyedeegaph
                {
                    PushSample(sampleTotal / sampleCount);
                } else
                {
                    PushSample(currentfps);
                }
                if (stuff.activeSelf)
                {
                    PlotData();
                }
                
                updateTimer = 0;
                sampleTotal = 0;
                sampleCount = 0;
            }
        }
        public void PlotPoint(Vector2 pos)
        {
            GameObject point = Instantiate(Plugin.bundle.LoadAsset<GameObject>("Point"), graphContainer.transform);
            RectTransform pointRect = point.GetComponent<RectTransform>();
            pointRect.sizeDelta = new Vector2(2, 2);
            pointRect.anchoredPosition = pos;
            plottedPoints.Add(point);
        }

        public void PushSample(float value)
        {
            for(int i = fpsDataCount; i >= 0; --i)
            {
                if ((i + 1) >= fpsData.Length)
                {
                    continue;
                }
                fpsData[i + 1] = fpsData[i];
            }
            fpsData[0] = value;
            if (fpsData.Length > fpsDataCount)
            {
                fpsDataCount++;
            }
        }

        public void PlotData()
        {
            //TODO: bruh
            graphTexture.SetPixels(clearStuff);
            float thing = (graphTexture.width - 1f) / (fpsDataTotal - 1f);
            float thing2 = (graphTexture.height - 1f) / (targetFps * 2f);
            //graphTexture.SetPixel((int)(i * thing), (int)Mathf.Clamp(fpsData[i], 0, graphTexture.height), Color.red);
            if (ConfigManager.fpsGraphMarker.value)
            {
                float segment = (graphTexture.height - 1) / 4f;
                for (int i = 0; i < 5; i++)
                {
                    Vector2 startPos = new Vector2(0, Mathf.Round(segment * i));
                    Vector2 endPos = new Vector2(graphTexture.width - 1, Mathf.Round(segment * i));
                    if (i == 2)
                    {
                        DrawLineInTexture(startPos, endPos, new Color(1, 0, 0, ConfigManager.fpsGraphMarkerVisibility.value));
                    }
                    else
                    {
                        DrawLineInTexture(startPos, endPos, new Color(0.5f, 0.5f, 0.5f, ConfigManager.fpsGraphMarkerVisibility.value));
                    }

                }
            }
            
            for (int i = 0; i < fpsDataCount; i++)
            {
                if (i + 1 >= fpsDataCount)
                {
                    continue;
                }
                Vector2 startPos = new Vector2(Mathf.Round(i * thing), Mathf.Round(Mathf.Clamp(fpsData[i] * thing2, 0, (graphTexture.height - 1))));
                Vector2 endPos = new Vector2(Mathf.Round((i + 1) * thing), Mathf.Round(Mathf.Clamp(fpsData[(i + 1)] * thing2, 0, (graphTexture.height - 1))));
                DrawLineInTexture(startPos, endPos, Color.white);
            }
            
            graphTexture.Apply();

        }

        public void Refresh()
        {
            updateTimer = 0;
            sampleTotal = 0;
            sampleCount = 0;
            fpsData = new float[fpsDataTotal];
            fpsDataCount = 0;
        }

        public void ChangeDataPointTotal(int newTotal)
        {
            float[] newFpsData = new float[newTotal];
            for(int i = 0; i < fpsDataCount; i++)
            {
                if (i >= newTotal)
                {
                    fpsDataCount = newTotal;
                    break;
                }
                newFpsData[i] = fpsData[i];
            }
            fpsData = newFpsData;
            fpsDataTotal = newTotal;
        }

        public void ResizeGraphTexture(int size)
        {
            graphTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            graphSprite = Sprite.Create(graphTexture, new Rect(0, 0, graphTexture.width, graphTexture.height), new Vector2(0.5f, 0.5f));
            graphTexture.filterMode = FilterMode.Point;
            graphImage.sprite = graphSprite;
            clearStuff = new Color[graphTexture.width * graphTexture.height];
        }

        public void Offset(float xOff, float yOff)
        {
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos + xOff, yPos + yOff);
        }

        public void ClearPlottedData()
        {
            //todo, figure out SetPixels
            
            /*for (int i = 0; i < graphTexture.height; i++)
            {
                for (int j = 0; j < graphTexture.width; j++)
                {
                    graphTexture.SetPixel(j, i, Color.clear);
                }
            }*/

            /*for (int i = 0; i < plottedPoints.Count; i++)
            {
                Destroy(plottedPoints[i]);
            }
            plottedPoints.Clear();*/
        }
        //Bresenham's Algorithm, solution from https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        public void DrawLineInTexture(Vector2 startPos, Vector2 endPos, Color color)
        {
            int x0 = (int)startPos.x;
            int y0 = (int)startPos.y;
            int x1 = (int)endPos.x;
            int y1 = (int)endPos.y;

            int dx = Math.Abs(x1 - x0);
            int dy = -Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                graphTexture.SetPixel(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * error;
                if (e2 >= dy)
                {
                    if (x0 == x1) break;
                    error += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    if (y0 == y1) break;
                    error += dx;
                    y0 += sy;
                }
            }
        }

        public void AddToTotal(float fps)
        {
            sampleTotal += fps;
            sampleCount += 1;
        }
    }
}
