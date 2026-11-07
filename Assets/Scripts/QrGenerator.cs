using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing.Common;
using ZXing;
using TMPro;
using System.IO;
using System.Runtime.InteropServices;

public class QrGenerator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    TMP_InputField _textToEncode;
    [SerializeField]
    RawImage _qrCodeImage;
    [SerializeField]
    Button _generateButton;
    [SerializeField]
    Button _resetButton;
    [SerializeField]
    Button _downloadButton;
    [SerializeField]
    [Tooltip("Optional, confirms downloads on platforms without a system notification")]
    DownloadToast _downloadToast;

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
            var text = string.IsNullOrWhiteSpace(_textToEncode?.text) ? DefaultText : _textToEncode.text;
            GenerateQrCode(text);
        });

        // Reset and download buttons are optional, a layout may leave them out
        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(() =>
            {
                if (_downloadButton != null)
                    _downloadButton.interactable = false;
                _qrCodeImage.enabled = false;
            });
        }

        if (_downloadButton != null)
        {
            _downloadButton.interactable = false;
            _downloadButton.onClick.AddListener(DownloadQrCode);
        }
    }

    void GenerateQrCode(string text)
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

        if (_downloadButton != null)
            _downloadButton.interactable = true;
    }
    
    void DownloadQrCode()
    {
        Texture2D texture = _qrCodeImage.texture as Texture2D;
        if (texture == null)
        {
            Debug.LogWarning("QR code texture is not a Texture2D!");
            return;
        }

        // Convert texture to PNG bytes
        byte[] pngData = texture.EncodeToPNG();
        
        // Generate filename with timestamp
        string fileName = $"QRCode_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        
#if UNITY_ANDROID && !UNITY_EDITOR
        SaveToGalleryAndroid(pngData, fileName);
#elif UNITY_WEBGL && !UNITY_EDITOR
        // Browsers can't write files, so the page starts a download instead (Plugins/WebGL/DownloadFile.jslib)
        DownloadFile(fileName, pngData, pngData.Length);
        if (_downloadToast != null)
            _downloadToast.Show("QR code downloaded");
#else
        // Desktop and editor: save to the user's Downloads folder
        string downloadsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloadsPath);
        string filePath = Path.Combine(downloadsPath, fileName);
        File.WriteAllBytes(filePath, pngData);
        Debug.Log($"QR Code saved to: {filePath}");
        if (_downloadToast != null)
            _downloadToast.Show("QR code saved to Downloads\nClick to open the folder", filePath);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void DownloadFile(string fileName, byte[] data, int length);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    void SaveToGalleryAndroid(byte[] pngData, string fileName)
    {
        try
        {
            using AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
            string downloadsPath = GetDownloadFolder(environment);
            string filePath = Path.Combine(downloadsPath, fileName);

            File.WriteAllBytes(filePath, pngData);
            Debug.Log("QR code saved to: " + filePath);

            // Notify the media scanner so the file appears in the Downloads app.
            // The scan returns a content:// URI, used by the notification to open the file
            using AndroidJavaClass mediaScanner =
                new AndroidJavaClass("android.media.MediaScannerConnection");
            using AndroidJavaObject context =
                GetUnityActivity().Call<AndroidJavaObject>("getApplicationContext");
            long byteCount = pngData.LongLength;
            mediaScanner.CallStatic("scanFile", context, new[] { filePath }, new[] { "image/png" },
                new MediaScanListener(uri =>
                    NotificationService.Instance.SendDownloadNotification(fileName, byteCount, uri)));
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving QR code: " + e.Message);
        }
    }

    class MediaScanListener : AndroidJavaProxy
    {
        readonly Action<AndroidJavaObject> _onScanCompleted;

        public MediaScanListener(Action<AndroidJavaObject> onScanCompleted)
            : base("android.media.MediaScannerConnection$OnScanCompletedListener")
        {
            _onScanCompleted = onScanCompleted;
        }

        // Called by Java on a background thread; uri is null if the scan failed
        void onScanCompleted(string path, AndroidJavaObject uri) => _onScanCompleted(uri);
    }
        
    static AndroidJavaObject GetUnityActivity()
    {
        using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    static string GetDownloadFolder(AndroidJavaClass environment)
    {
        return environment.CallStatic<AndroidJavaObject>(
            "getExternalStoragePublicDirectory",
            environment.GetStatic<string>("DIRECTORY_DOWNLOADS")).Call<string>("getAbsolutePath");
    }
#endif
}