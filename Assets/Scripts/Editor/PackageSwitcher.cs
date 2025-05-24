using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class PackageSwitcher
{
    const string ItemsLocation = "Tools/QrGenerator";

    const string ProgressTitle = "Switching package source";
    const string ProgressSwitch = "Switching to {0} source";

    const string ManifestPath = "../Packages/manifest.json";
    const string PackageName = "zxing.net";
    const string RemoteSource = "org.nuget";
    const string LocalSource = "org.custom";

    enum SwitchSource { Remote, Local }

    static Request _request;
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

     static Manifest GetManifest()
    {
        string manifestPath = Path.Combine(Application.dataPath, ManifestPath);
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("manifest.json not found.");
            return null;
        }
        string manifestContent = File.ReadAllText(manifestPath);
        return JsonConvert.DeserializeObject<Manifest>(manifestContent);
    }

    static void SwitchTo(SwitchSource source)
    {
        EditorUtility.DisplayProgressBar(
            ProgressTitle,
            string.Format(ProgressSwitch, source.ToString().ToLower()),
            0.95f
        );

        Manifest manifest = GetManifest();
        if (manifest == null || manifest.Dependencies == null)
        {
            Debug.LogError("Manifest could not be deserialized.");
            EditorUtility.ClearProgressBar();
            return;
        }

        (string sourceName, string deleteName) = source switch
        {
            SwitchSource.Local => (_lastAdded = $"{LocalSource}.{PackageName}", $"{RemoteSource}.{PackageName}"),
            _ => (_lastAdded = $"{RemoteSource}.{PackageName}", $"{LocalSource}.{PackageName}")
        };

        if (manifest.Dependencies.ContainsKey(sourceName))
        {
            Debug.Log($"{_lastAdded} is already installed");
            EditorUtility.ClearProgressBar();
            return;
        }

        _request = manifest.Dependencies.ContainsKey(deleteName) ?
            Client.AddAndRemove(new[] { sourceName }, new[] { deleteName }) :
            Client.Add(sourceName);
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        if (!_request.IsCompleted) return;

        EditorApplication.update -= Progress;

        if (_request.Status == StatusCode.Success)
            Debug.Log($"Added package {_lastAdded}");
        else
            Debug.LogWarning(_request.Error.message);

        EditorUtility.ClearProgressBar();
    }
}

[Serializable]
class Manifest
{
    public Dictionary<string, string> Dependencies { get; set; }
}
