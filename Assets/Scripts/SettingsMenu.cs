using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Modal menu to pick the app color and font. Options are built from the <see cref="ThemeManager"/> lists.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    ThemeManager _theme;

    [Header("UI")]
    [SerializeField]
    Button _openButton;
    [SerializeField]
    Button _closeButton;
    [SerializeField]
    [Tooltip("Full screen button behind the card, closes the menu when tapped")]
    Button _backdropButton;
    [SerializeField]
    CanvasGroup _menu;
    [SerializeField]
    RectTransform _card;

    [Header("Options")]
    [SerializeField]
    Button _colorTemplate;
    [SerializeField]
    Button _fontTemplate;

    [Header("Color option style")]
    [SerializeField]
    Color _selectedRing = Color.white;
    [SerializeField]
    Color _unselectedRing = new(1f, 1f, 1f, 0.2f);

    [Header("Font option style")]
    [SerializeField]
    Color _selectedBackground = Color.white;
    [SerializeField]
    Color _selectedText = new(0.196f, 0.196f, 0.196f);
    [SerializeField]
    Color _unselectedBackground = new(0f, 0f, 0f, 0.3f);
    [SerializeField]
    Color _unselectedText = Color.white;

    const float FadeDuration = 0.15f;
    const float ClosedCardScale = 0.95f;
    const float ScreenMargin = 40f;

    readonly List<Image> _colorRings = new();
    readonly List<(Image background, TMP_Text label)> _fontOptions = new();
    Coroutine _transition;
    float _progress;

    void Start()
    {
        _openButton.onClick.AddListener(Open);
        _closeButton.onClick.AddListener(Close);
        _backdropButton.onClick.AddListener(Close);

        BuildColorOptions();
        BuildFontOptions();
        _theme.Changed += RefreshSelection;
        RefreshSelection();

        _menu.alpha = 0f;
        _menu.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_theme != null)
            _theme.Changed -= RefreshSelection;
    }

    void Update()
    {
        if (!_menu.gameObject.activeSelf)
            return;

        // Escape on desktop, back button on Android
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();

        // Follows window resizes and rotations while open
        UpdateCardScale();
    }

    void BuildColorOptions()
    {
        for (int i = 0; i < _theme.Colors.Count; i++)
        {
            int index = i;
            Button option = Instantiate(_colorTemplate, _colorTemplate.transform.parent);
            option.name = _theme.Colors[i].Name;
            // The button graphic is the inner fill, the root image draws the selection ring
            option.targetGraphic.color = _theme.Colors[i].Color;
            option.onClick.AddListener(() => _theme.SetColor(index));
            option.gameObject.SetActive(true);
            _colorRings.Add(option.GetComponent<Image>());
        }
        _colorTemplate.gameObject.SetActive(false);
    }

    void BuildFontOptions()
    {
        for (int i = 0; i < _theme.Fonts.Count; i++)
        {
            int index = i;
            Button option = Instantiate(_fontTemplate, _fontTemplate.transform.parent);
            option.name = _theme.Fonts[i].Name;
            var label = option.GetComponentInChildren<TMP_Text>();
            // Each option previews its own font
            label.text = _theme.Fonts[i].Name;
            label.font = _theme.Fonts[i].Font;
            label.fontSize *= _theme.Fonts[i].SizeScale;
            option.onClick.AddListener(() => _theme.SetFont(index));
            option.gameObject.SetActive(true);
            _fontOptions.Add(((Image)option.targetGraphic, label));
        }
        _fontTemplate.gameObject.SetActive(false);
    }

    void RefreshSelection()
    {
        for (int i = 0; i < _colorRings.Count; i++)
            _colorRings[i].color = i == _theme.ColorIndex ? _selectedRing : _unselectedRing;

        for (int i = 0; i < _fontOptions.Count; i++)
        {
            bool selected = i == _theme.FontIndex;
            _fontOptions[i].background.color = selected ? _selectedBackground : _unselectedBackground;
            _fontOptions[i].label.color = selected ? _selectedText : _unselectedText;
        }
    }

    public void Open() => Transition(true);

    public void Close() => Transition(false);

    void Transition(bool open)
    {
        if (_transition != null)
            StopCoroutine(_transition);
        _menu.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_card);
        _transition = StartCoroutine(Fade(open));
    }

    IEnumerator Fade(bool open)
    {
        _menu.interactable = open;
        _menu.blocksRaycasts = open;

        float from = _menu.alpha;
        float to = open ? 1f : 0f;
        for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
        {
            SetProgress(Mathf.Lerp(from, to, t / FadeDuration));
            yield return null;
        }
        SetProgress(to);

        if (!open)
            _menu.gameObject.SetActive(false);
        _transition = null;
    }

    void SetProgress(float progress)
    {
        _progress = progress;
        _menu.alpha = progress;
        UpdateCardScale();
    }

    // Shrinks the card when the screen is too small for it, e.g. short desktop windows or landscape phones
    void UpdateCardScale()
    {
        var area = ((RectTransform)_card.parent).rect;
        Vector2 size = _card.rect.size;
        float fit = Mathf.Min(1f,
            (area.width - 2 * ScreenMargin) / size.x,
            (area.height - 2 * ScreenMargin) / size.y);
        _card.localScale = Vector3.one * (fit * Mathf.Lerp(ClosedCardScale, 1f, _progress));
    }
}
