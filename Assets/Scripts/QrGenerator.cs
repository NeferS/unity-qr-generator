using UnityEngine;
using UnityEngine.UI;
using ZXing.Common;
using ZXing;
using TMPro;

public class QrGenerator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    TMP_InputField _textToEncode;
    [SerializeField]
    RawImage _qrCodeImage;
    [SerializeField]
    Button _generateButton;

    [Header("QR Code Settings")]
    [SerializeField]
    int _qrCodeWidth = 512;
    [SerializeField]
    int _qrCodeHeight = 512;

    const string DefaultText = "Insert a text or link to encode in the QR!";

    void Start()
    {
        _generateButton.onClick.AddListener(() =>
        {
            string text;
            if(string.IsNullOrWhiteSpace(_textToEncode?.text))
                text = DefaultText;
            else
                text = _textToEncode.text;
            GenerateQRCode(text);
        });
    }

    public void GenerateQRCode(string text)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = _qrCodeWidth,
                Height = _qrCodeHeight,
                Margin = 1
            }
        };

        var pixelData = writer.Write(text);

        Texture2D texture = new Texture2D(pixelData.Width, pixelData.Height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(pixelData.Pixels);
        texture.Apply();

        _qrCodeImage.enabled = true;
        _qrCodeImage.texture = texture;
        _qrCodeImage.rectTransform.sizeDelta = new Vector2(_qrCodeWidth, _qrCodeHeight);
    }
}
