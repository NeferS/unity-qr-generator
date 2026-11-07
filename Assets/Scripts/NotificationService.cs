// The notifications package only exists for Android (and the Editor), so desktop players leave this out
#if UNITY_ANDROID || UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using Unity.Notifications.Android;

public class NotificationService : MonoBehaviour
{
    public static NotificationService Instance { get; private set; }

    // Channels persist in the system settings, so their ids must be stable across launches
    const string GroupId = "main_notifications";
    const string ChannelId = "downloads";
    const string SmallIcon = "check_line";

    // Android constants (Intent / PendingIntent)
    const int FlagGrantReadUriPermission = 0x00000001;
    const int FlagActivityNewTask = 0x10000000;
    const int FlagImmutable = 0x04000000;
    const int FlagUpdateCurrent = 0x08000000;

    int _lastNotificationId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        var group = new AndroidNotificationChannelGroup()
        {
            Id = GroupId,
            Name = "Main notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannelGroup(group);
        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "Downloads",
            Importance = Importance.Default,
            Description = "Downloaded QR codes",
            Group = GroupId
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        StartCoroutine(RequestNotificationPermission());
    }

    IEnumerator RequestNotificationPermission()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;
    }

    /// <summary>
    /// Posts the notification natively instead of through AndroidNotificationCenter,
    /// because the Unity package always reopens the app when the notification is tapped.
    /// Tapping this one opens the file in a viewer app. May be called from any thread.
    /// </summary>
    public void SendDownloadNotification(string fileName, long byteCount, AndroidJavaObject fileUri)
    {
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var context = activity.Call<AndroidJavaObject>("getApplicationContext");

            int id = ++_lastNotificationId;

            using var builder = new AndroidJavaObject("android.app.Notification$Builder", context, ChannelId);
            Chain(builder, "setSmallIcon", GetSmallIconId(context));
            Chain(builder, "setContentTitle", fileName);
            Chain(builder, "setContentText", $"Download completed · {GetReadableFileSize(byteCount)}");
            Chain(builder, "setAutoCancel", true);

            if (fileUri != null)
            {
                using var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                Chain(intent, "setDataAndType", fileUri, "image/png");
                Chain(intent, "addFlags", FlagGrantReadUriPermission | FlagActivityNewTask);

                using var pendingIntentClass = new AndroidJavaClass("android.app.PendingIntent");
                using var pendingIntent = pendingIntentClass.CallStatic<AndroidJavaObject>(
                    "getActivity", context, id, intent, FlagImmutable | FlagUpdateCurrent);
                Chain(builder, "setContentIntent", pendingIntent);
            }
            else
            {
                Debug.LogWarning("Media scan returned no URI, notification won't open the file");
            }

            using var notification = builder.Call<AndroidJavaObject>("build");
            using var notificationManager = context.Call<AndroidJavaObject>("getSystemService", "notification");
            notificationManager.Call("notify", id, notification);
            Debug.Log($"Notification sent: id: {id}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending notification: " + e.Message);
        }
    }

    // Calls a builder-style Java method and releases the returned reference
    static void Chain(AndroidJavaObject target, string method, params object[] args)
    {
        target.Call<AndroidJavaObject>(method, args)?.Dispose();
    }

    // Icon registered in Project Settings > Mobile Notifications, falling back to the app icon
    static int GetSmallIconId(AndroidJavaObject context)
    {
        using var resources = context.Call<AndroidJavaObject>("getResources");
        string packageName = context.Call<string>("getPackageName");
        int iconId = resources.Call<int>("getIdentifier", SmallIcon, "drawable", packageName);
        if (iconId != 0)
            return iconId;

        using var appInfo = context.Call<AndroidJavaObject>("getApplicationInfo");
        return appInfo.Get<int>("icon");
    }

    string GetReadableFileSize(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB" };
        if (byteCount == 0)
            return "0 B";
        int place = (int)Math.Floor(Math.Log(byteCount, 1024));
        double num = Math.Round(byteCount / Math.Pow(1024, place), 2);
        return $"{num:N2} {suf[place]}";
    }
}
#endif
