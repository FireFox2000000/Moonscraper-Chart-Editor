// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

/// <summary>
/// Mute and Solo buttons beside each song audio volume slider in Settings > Audio, like Audacity's.
/// Clicking Solo plays only that audio; Shift-clicking it adds to the soloed audio.
/// The rows of audio the song doesn't have are dimmed.
/// </summary>
public class StemMuteButtons
{
    const float ButtonWidth = 22;
    const float ButtonHeight = 20;
    const float ButtonGap = 4;

    // Dimmed rows are darkened rather than made see-through, so a slider's handle still covers its bar
    const float DimmedBrightness = 0.4f;

    static readonly Color c_offColour = new Color(0.45f, 0.45f, 0.45f);
    static readonly Color c_mutedColour = new Color(0.85f, 0.25f, 0.25f);
    static readonly Color c_soloedColour = new Color(0.95f, 0.75f, 0.2f);
    static readonly Color c_offTextColour = Color.white;
    static readonly Color c_mutedTextColour = Color.white;
    static readonly Color c_soloedTextColour = Color.black;

    // Windows' icon font, for a muted speaker and headphones. Without it, the buttons show M and S.
    static readonly string[] c_iconFontNames = { "Segoe Fluent Icons", "Segoe MDL2 Assets" };
    const string MuteIcon = "";
    const string SoloIcon = "";

    const string MuteHint = "Mute this stem";
    const string SoloHint = "Play only this stem. Shift+click to play more stems.";
    static readonly Vector2 c_hintOffset = new Vector2(14, -12);     // Below and right of the button

    class Row
    {
        public Song.AudioInstrument audio;
        public Image muteImage;
        public Text muteText;
        public Image soloImage;
        public Text soloText;
        public List<KeyValuePair<Graphic, Color>> rowGraphics = new List<KeyValuePair<Graphic, Color>>();     // The slider and its label, with their own colours
    }

    readonly List<Row> m_rows = new List<Row>();
    readonly Font m_iconFont;

    public StemMuteButtons(IDictionary<Song.AudioInstrument, Slider> sliders)
    {
        m_iconFont = LoadIconFont();

        foreach (var slider in sliders)
        {
            m_rows.Add(CreateRow(slider.Key, slider.Value));
        }
    }

    /// <summary>
    /// Shows each audio's mute and solo state, and dims the audio the current song doesn't have.
    /// </summary>
    public void Refresh()
    {
        SongAudioManager songAudio = ChartEditor.Instance.currentSongAudio;

        foreach (Row row in m_rows)
        {
            bool muted = StemMute.IsMuted(row.audio);
            bool soloed = StemMute.IsSoloed(row.audio);
            float brightness = songAudio.GetAudioIsLoaded(row.audio) ? 1 : DimmedBrightness;

            row.muteImage.color = Darken(muted ? c_mutedColour : c_offColour, brightness);
            row.muteText.color = Darken(muted ? c_mutedTextColour : c_offTextColour, brightness);
            row.soloImage.color = Darken(soloed ? c_soloedColour : c_offColour, brightness);
            row.soloText.color = Darken(soloed ? c_soloedTextColour : c_offTextColour, brightness);

            foreach (var graphic in row.rowGraphics)
            {
                graphic.Key.color = Darken(graphic.Value, brightness);
            }
        }
    }

    static Color Darken(Color colour, float brightness)
    {
        return new Color(colour.r * brightness, colour.g * brightness, colour.b * brightness, colour.a);
    }

    static Font LoadIconFont()
    {
        var installed = new HashSet<string>(Font.GetOSInstalledFontNames());
        foreach (string name in c_iconFontNames)
        {
            if (installed.Contains(name))
                return Font.CreateDynamicFontFromOSFont(name, 16);
        }

        return null;
    }

    Row CreateRow(Song.AudioInstrument audio, Slider slider)
    {
        Row row = new Row() { audio = audio };
        RectTransform sliderRect = (RectTransform)slider.transform;
        Text label = FindLabel(sliderRect);
        Image sliderBackground = slider.transform.Find("Background").GetComponent<Image>();

        float x = sliderRect.anchoredPosition.x + sliderRect.sizeDelta.x * (1 - sliderRect.pivot.x) + ButtonGap + ButtonWidth / 2;
        CreateButton(sliderRect, "M", MuteIcon, MuteHint, x, sliderBackground, label, () => { StemMute.ToggleMute(audio); }, out row.muteImage, out row.muteText);
        CreateButton(sliderRect, "S", SoloIcon, SoloHint, x + ButtonWidth + ButtonGap, sliderBackground, label, () => { StemMute.ToggleSolo(audio, Globals.secondaryInputActive); }, out row.soloImage, out row.soloText);

        foreach (Graphic graphic in slider.GetComponentsInChildren<Graphic>(true))
        {
            row.rowGraphics.Add(new KeyValuePair<Graphic, Color>(graphic, graphic.color));
        }

        if (label != null)
            row.rowGraphics.Add(new KeyValuePair<Graphic, Color>(label, label.color));

        return row;
    }

    void CreateButton(RectTransform sliderRect, string letter, string icon, string hint, float x, Image style, Text labelFont, UnityEngine.Events.UnityAction onClick, out Image image, out Text text)
    {
        GameObject button = new GameObject(sliderRect.name.Trim() + " " + letter, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = (RectTransform)button.transform;
        rect.SetParent(sliderRect.parent, false);
        rect.anchorMin = sliderRect.anchorMin;
        rect.anchorMax = sliderRect.anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
        rect.anchoredPosition = new Vector2(x, sliderRect.anchoredPosition.y);

        image = button.GetComponent<Image>();
        image.sprite = style.sprite;
        image.type = style.type;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        text = textObject.GetComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        if (m_iconFont != null)
        {
            text.text = icon;
            text.font = m_iconFont;
            text.fontSize = 14;
        }
        else
        {
            text.text = letter;
            text.font = labelFont != null ? labelFont.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
        }

        HintMouseOver hintMouseOver = button.AddComponent<HintMouseOver>();
        hintMouseOver.message = hint;
        hintMouseOver.offset = c_hintOffset;

        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.targetGraphic = image;
        buttonComponent.onClick.AddListener(() =>
        {
            onClick();
            Refresh();
        });
    }

    // The slider's label is the text beside it on the same row
    static Text FindLabel(RectTransform sliderRect)
    {
        foreach (Transform sibling in sliderRect.parent)
        {
            Text text = sibling.GetComponent<Text>();
            if (text != null && Mathf.Abs(((RectTransform)sibling).anchoredPosition.y - sliderRect.anchoredPosition.y) < 1)
                return text;
        }

        return null;
    }
}
