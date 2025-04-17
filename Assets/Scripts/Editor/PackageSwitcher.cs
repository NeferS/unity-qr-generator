using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class PackageSwitcher
{
    const string ItemsLocation = "Tools/QrGenerator";

    const string ProgressTitle = "Switching package source";
    const string ProgressSwitch = "Switching to {0} source";

    const string PackageName = "zxing.net";
    const string RemoteSource = "org.nuget";
    const string LocalSource = "org.custom";

    enum SwitchSource { Remote, Local }

    static AddRequest _addRequest;
    static string _lastAdded;

    [MenuItem(ItemsLocation + "/Select remote source (org.nuget)")]
    public static void SwitchToRemote()
    {
        SwitchTo(SwitchSource.Remote);
    }

    [MenuItem(ItemsLocation + "/Select local source (org.custom)")]
    public static void SwitchToLocal()
    {
        SwitchTo(SwitchSource.Local);
    }

    static void SwitchTo(SwitchSource source)
    {
        EditorUtility.DisplayProgressBar(
            ProgressTitle,
            string.Format(ProgressSwitch, source.ToString().ToLower()),
            0.95f
        );

        string sourceName = source switch
        {
            SwitchSource.Remote => RemoteSource,
            SwitchSource.Local => LocalSource,
            _ => RemoteSource
        };
        _addRequest = Client.Add(_lastAdded = $"{sourceName}.{PackageName}");
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        if (!_addRequest.IsCompleted) return;

        EditorApplication.update -= Progress;

        if (_addRequest.Status == StatusCode.Success)
            Debug.Log($"Added package {_lastAdded}");
        else
            Debug.LogWarning(_addRequest.Error.message);

        EditorUtility.ClearProgressBar();
    }
}
