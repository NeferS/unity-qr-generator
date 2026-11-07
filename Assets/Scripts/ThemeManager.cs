using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Applies the selected background color and font to the UI and remembers the choice between sessions.
/// </summary>
public class ThemeManager : MonoBehaviour
{
    [Serializable]
    public struct ColorOption
    {
        public string Name;
        public Color Color;
    }

    [Serializable]
    public struct FontOption
    {
        public string Name;
        public TMP_FontAsset Font;
        [Tooltip("Multiplier applied to the designed font sizes, to compensate for wider fonts")]
        public float SizeScale;
        [Tooltip("Added to the designed line spacing, to compensate for fonts with a tighter line height")]
        public float LineSpacing;
    }

    [SerializeField]
    ColorOption[] _colors;
    [SerializeField]
    FontOption[] _fonts;

    [Header("Targets")]
    [SerializeField]
    [Tooltip("Graphics that take the background color, besides the main camera")]
    Graphic[] _backgroundGraphics;
    [SerializeField]
    [Tooltip("Texts under these roots keep their own font")]
    Transform[] _fontExcludedRoots;

    const string ColorKey = "settings.color";
    const string FontKey = "settings.font";

    readonly List<(TMP_Text text, float baseSize, float baseLineSpacing)> _texts = new();

    public IReadOnlyList<ColorOption> Colors => _colors;
    public IReadOnlyList<FontOption> Fonts => _fonts;
    public int ColorIndex { get; private set; }
    public int FontIndex { get; private set; }

    public event Action Changed;

    void Awake()
    {
        // Font sizes are designed for the first font, so they are captured before any change
        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            if (!_fontExcludedRoots.Any(root => root != null && text.transform.IsChildOf(root)))
                _texts.Add((text, text.fontSize, text.lineSpacing));
        }

        ColorIndex = Mathf.Clamp(PlayerPrefs.GetInt(ColorKey, 0), 0, _colors.Length - 1);
        FontIndex = Mathf.Clamp(PlayerPrefs.GetInt(FontKey, 0), 0, _fonts.Length - 1);
        ApplyColor();
        ApplyFont();
    }

    public void SetColor(int index)
    {
        if (index == ColorIndex || index < 0 || index >= _colors.Length)
            return;

        ColorIndex = index;
        PlayerPrefs.SetInt(ColorKey, index);
        PlayerPrefs.Save();
        ApplyColor();
        Changed?.Invoke();
    }

    public void SetFont(int index)
    {
        if (index == FontIndex || index < 0 || index >= _fonts.Length)
            return;

        FontIndex = index;
        PlayerPrefs.SetInt(FontKey, index);
        PlayerPrefs.Save();
        ApplyFont();
        Changed?.Invoke();
    }

    void ApplyColor()
    {
        Color color = _colors[ColorIndex].Color;
        if (Camera.main != null)
            Camera.main.backgroundColor = color;

        foreach (var graphic in _backgroundGraphics)
        {
            if (graphic != null)
                graphic.color = new Color(color.r, color.g, color.b, graphic.color.a);
        }
    }

    void ApplyFont()
    {
        FontOption option = _fonts[FontIndex];
        foreach (var (text, baseSize, baseLineSpacing) in _texts)
        {
            if (text == null)
                continue;
            text.font = option.Font;
            text.fontSize = baseSize * option.SizeScale;
            text.lineSpacing = baseLineSpacing + option.LineSpacing;
        }
    }
}
