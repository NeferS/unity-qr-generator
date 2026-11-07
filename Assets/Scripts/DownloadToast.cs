using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Toast that slides in from the top-left corner after a download.
/// On desktop, clicking it once opens the folder holding the file.
/// </summary>
public class DownloadToast : MonoBehaviour
{
    [SerializeField]
    RectTransform _panel;
    [SerializeField]
    CanvasGroup _group;
    [SerializeField]
    Button _button;
    [SerializeField]
    TMP_Text _message;

    [Header("Animation")]
    [SerializeField]
    float _margin = 40f;
    [SerializeField]
    float _slideDuration = 0.35f;
    [SerializeField]
    float _visibleDuration = 3f;

    string _filePath;
    Sequence _sequence;

    void Awake()
    {
        _button.onClick.AddListener(OpenFolder);
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    void OnDestroy() => _sequence?.Kill();

    /// <summary>
    /// Shows the toast. With a file path the toast can be clicked to open its folder.
    /// </summary>
    public void Show(string message, string filePath = null)
    {
        _filePath = filePath;
        _message.text = message;
        _button.interactable = filePath != null;
        _group.blocksRaycasts = true;

        // The width depends on the message, so the off-screen position is measured after the layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
        Vector2 shown = new(_margin, -_margin);
        Vector2 hidden = new(-_panel.rect.width - _margin, -_margin);

        _sequence?.Kill();
        _panel.anchoredPosition = hidden;
        _group.alpha = 0f;
        _sequence = DOTween.Sequence()
            .Append(_panel.DOAnchorPos(shown, _slideDuration).SetEase(Ease.OutCubic))
            .Join(_group.DOFade(1f, _slideDuration))
            .AppendInterval(_visibleDuration)
            .Append(Hide(hidden))
            .SetLink(gameObject);
    }

    Sequence Hide(Vector2 hidden)
    {
        return DOTween.Sequence()
            .AppendCallback(() => _group.blocksRaycasts = false)
            .Append(_panel.DOAnchorPos(hidden, _slideDuration).SetEase(Ease.InCubic))
            .Join(_group.DOFade(0f, _slideDuration));
    }

    void OpenFolder()
    {
        if (_filePath == null)
            return;

        // Only once per toast, so repeated clicks don't open several windows
        _button.interactable = false;
        _sequence?.Kill();
        _sequence = Hide(new Vector2(-_panel.rect.width - _margin, -_margin)).SetLink(gameObject);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Opens Explorer with the file selected
        Process.Start("explorer.exe", $"/select,\"{_filePath}\"");
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // Escapes spaces and other characters in the path
        Application.OpenURL(new System.Uri(Path.GetDirectoryName(_filePath)).AbsoluteUri);
#endif
    }
}
