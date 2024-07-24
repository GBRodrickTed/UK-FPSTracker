using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace FPSTracker
{
    public class FPSDisplay : MonoBehaviour
    {
        public GameObject stuff;
        Text fpsText;
        public int sampleSize = 30;
        public float sampleTimeLimit = 1f;
        float sampleTimer = 0;
        int sampleCounter = 0;
        float sampleTotal = 0;
        float sampleAverage = 0;
        float xPos = 0;
        float yPos = 0;
        public void Start()
        {
            stuff = transform.Find("Stuff").gameObject;
            fpsText = transform.Find("Stuff").Find("Text").GetComponent<Text>();
            stuff.SetActive(ConfigManager.fpsDisplay.value);
            sampleSize = ConfigManager.fpsDisplaySampleSize.value;
            sampleTimeLimit = ConfigManager.fpsDisplayTimeLimit.value;
            xPos = gameObject.GetComponent<RectTransform>().anchoredPosition.x;
            yPos = gameObject.GetComponent<RectTransform>().anchoredPosition.y;
            Offset(ConfigManager.fpsDisplayOffsetX.value, ConfigManager.fpsDisplayOffsetY.value);
        }
        public void Update()
        {
            float currentFps = 1f / Time.unscaledDeltaTime;
            sampleTotal += currentFps;
            sampleCounter += 1;
            sampleTimer += Time.unscaledDeltaTime;
            if (sampleCounter >= sampleSize || sampleTimer >= sampleTimeLimit)
            {
                sampleAverage = sampleTotal / sampleCounter;
                if (stuff)
                {
                    UpdateText();
                }
                //Debug.Log(Math.Round(sampleAverage));
                Refresh();
            }
        }
        public void Offset(float xOff, float yOff)
        {
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos + xOff, yPos + yOff);
        }
        public void Refresh()
        {
            sampleCounter = 0;
            sampleAverage = 0;
            sampleTotal = 0;
            sampleTimer = 0;
        }
        public void UpdateText()
        {
            fpsText.text = Math.Round(sampleAverage).ToString();
        }
        public void AddSampleToTotal(float fps)
        {
            sampleTotal += fps;
            sampleCounter += 1;
        }
    }
}
