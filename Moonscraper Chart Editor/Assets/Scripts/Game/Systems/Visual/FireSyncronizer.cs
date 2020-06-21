// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

[ExecuteInEditMode]
public class FireSyncronizer : MonoBehaviour {
    public float m_Brightness = 8.0f;
    public float m_Speed = 1.0f;
    static MaterialPropertyBlock m_MatProps;
    public float m_StartTime = 0.0f;

    public Material flameMat;
    public static Material[] flameMaterials = new Material[7];  // Max used
    public Color orangeColor;

    Dictionary<Chart.GameMode, Dictionary<int, Color>> gameModeColourDict;
    Dictionary<Chart.GameMode, Dictionary<int, Dictionary<int, Color>>> gameModeColourDictLaneOverride;

    void Awake()
    {
        if (Application.isPlaying)
        {
            gameModeColourDict = new Dictionary<Chart.GameMode, Dictionary<int, Color>>()
            {
                {
                    Chart.GameMode.Guitar, new Dictionary<int, Color>()
                    {
                        { (int)Note.GuitarFret.Green, Color.green },
                        { (int)Note.GuitarFret.Red, Color.red },
                        { (int)Note.GuitarFret.Yellow, Color.yellow },
                        { (int)Note.GuitarFret.Blue, Color.blue },
                        { (int)Note.GuitarFret.Orange, orangeColor },
                        { (int)Note.GuitarFret.Open, Color.magenta },
                    }
                },
                {
                    Chart.GameMode.Drums, new Dictionary<int, Color>()
                    {  
                        { (int)Note.DrumPad.Red, Color.red },
                        { (int)Note.DrumPad.Yellow, Color.yellow },
                        { (int)Note.DrumPad.Blue, Color.blue },
                        { (int)Note.DrumPad.Orange, orangeColor },
                        { (int)Note.DrumPad.Green, Color.green },
                        { (int)Note.DrumPad.Kick, Color.magenta },
                    }
                },
                {
                    Chart.GameMode.GHLGuitar, new Dictionary<int, Color>()
                    {
                        { (int)Note.GHLiveGuitarFret.Black1, Color.gray },
                        { (int)Note.GHLiveGuitarFret.Black2, Color.gray },
                        { (int)Note.GHLiveGuitarFret.Black3, Color.gray },
                        { (int)Note.GHLiveGuitarFret.White1, Color.white },
                        { (int)Note.GHLiveGuitarFret.White2, Color.white  },
                        { (int)Note.GHLiveGuitarFret.White3, Color.white },
                        { (int)Note.GHLiveGuitarFret.Open, Color.magenta },
                    }
                },
            };

            gameModeColourDictLaneOverride = new Dictionary<Chart.GameMode, Dictionary<int, Dictionary<int, Color>>>()
            {
                {
                    Chart.GameMode.Drums, new Dictionary<int, Dictionary<int, Color>>()
                    {
                        {
                            4, new Dictionary<int, Color>()
                            {
                                { (int)Note.DrumPad.Red, Color.red },
                                { (int)Note.DrumPad.Yellow, Color.yellow },
                                { (int)Note.DrumPad.Blue, Color.blue },
                                { (int)Note.DrumPad.Orange, Color.green },
                                { (int)Note.DrumPad.Green, Color.green },
                                { (int)Note.DrumPad.Kick, Color.magenta },
                            }
                        }
                    }
                },
            };


            flameMaterials = new Material[7];
            for (int i = 0; i < flameMaterials.Length; ++i)
            {
                flameMaterials[i] = new Material(flameMat);

                Skin customSkin = SkinManager.Instance.currentSkin;

                if (i < customSkin.sustain_mats.Length && customSkin.sustain_mats[i])
                    flameMaterials[i].color = customSkin.sustain_mats[i].color;
                else
                {
                    switch (i)
                    {
                        case (0):
                            flameMaterials[i].color = Color.green;
                            break;
                        case (1):
                            flameMaterials[i].color = Color.red;
                            break;
                        case (2):
                            flameMaterials[i].color = Color.yellow;
                            break;
                        case (3):
                            flameMaterials[i].color = Color.blue;
                            break;
                        case (4):
                            flameMaterials[i].color = orangeColor;
                            break;
                        case (5):
                            flameMaterials[i].color = Color.magenta;
                            break;
                        case (6):
                            flameMaterials[i].color = Color.white;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        ChartEditor.Instance.events.lanesChangedEvent.Register(OnLanesChanged);
    }

    void SetMaterialColours(Chart.GameMode gameMode, int laneCount)
    {
        Dictionary<int, Color> colorDict = GetColorDict(gameMode, laneCount);

        foreach(var keyValue in colorDict)
        {
            flameMaterials[keyValue.Key].color = keyValue.Value;
        }
    }

    void OnLanesChanged(in int laneCount)
    {
        ChartEditor editor = ChartEditor.Instance;
        SetMaterialColours(editor.currentGameMode, laneCount);
    }

    Dictionary<int, Color> GetColorDict(Chart.GameMode gameMode, int laneCount)
    {
        Dictionary<int, Color> colorDict;
        Dictionary<int, Dictionary<int, Color>> laneOverrideDict;
        if (gameModeColourDictLaneOverride.TryGetValue(gameMode, out laneOverrideDict))
        {
            
            if (laneOverrideDict.TryGetValue(laneCount, out colorDict))
            {
                return colorDict;
            }
        }

        if (gameModeColourDict.TryGetValue(gameMode, out colorDict))
            return colorDict;

        return null;        // Shouldn't ever be here
    }
	
	// Update is called once per frame
	void Update () {
        Fire.m_Brightness = m_Brightness;
        Fire.m_Speed = m_Speed;

        if (!Fire.cam)
        {
            Fire.cam = Camera.main;         
            Fire.cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        Fire.camPosition = Fire.cam.transform.position;
        Fire.camlocalToWorldMatrix = Fire.cam.transform.localToWorldMatrix;

        if (Application.isPlaying)
        {
            float time = Time.time;
            Fire.m_TimeElapsed += m_Speed * (time - Fire.m_LastFrameTime);

            Fire.m_LastFrameTime = time;
            Shader.SetGlobalFloat("_FireTime", m_StartTime + Fire.m_TimeElapsed);
        }
        else
        {
            Shader.SetGlobalFloat("_FireTime", m_StartTime);
        }
    }
}
