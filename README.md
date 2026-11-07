# Unity ZXing.Net QR Code Generator Example

This Unity project demonstrates a **basic usage of the [ZXing.Net](https://github.com/micjahn/ZXing.Net)** library to generate QR codes. ZXing.Net is a .NET port of the open-source ZXing ("Zebra Crossing") barcode image processing library, maintained by Michael Jahn.

This project demonstrates the integration of ZXing.Net in Unity 6 using **[UnityNuGet](https://github.com/bdovaz/UnityNuGet) as a package source**.


## Features

- Generate QR codes from text input
- Display the generated QR code as a `Texture2D`
- Minimal example, easy to extend

## Supported Platforms

This project requires Unity 6000.0 or newer and has been tested and confirmed to work on the following build targets:

- ✅ **Windows (Standalone)**
- ✅ **Android**
- ✅ **WebGL**

The UI and code are cross-platform compatible, no platform-specific APIs are used.

## Dependency Options

ZXing.Net can be integrated into the project using one of the following methods:

### 1. **UnityNuGet (Recommended for demonstration)**

This project assumes ZXing.Net is available through [UnityNuGet](https://github.com/bdovaz/UnityNuGet) and demonstrates its usage **as if it was already added to the NuGet registry**.

You can use either:
- `org.nuget.zxing.net` from the official registry
- `org.custom.zxing.net` from a local Docker-based UnityNuGet instance

> ℹ️ If you're using a custom registry, make sure to choose the right port for the url of the scoped registry (default is 5000).

> 🎉 The [merge request](https://github.com/bdovaz/UnityNuGet/pull/516) has been closed on 2025/04/17 and the package has been included in the [release 0.62.0](https://github.com/bdovaz/UnityNuGet/releases/tag/0.62.0) on 2025/05/22!

### 2. **Manual DLL Import**

Alternatively, download the [prebuilt DLL](https://github.com/micjahn/ZXing.Net/blob/master/Clients/UnityDemo/Assets/zxing.unity.dll) from ZXing.Net 

## Releases

Releases are built by the [Release workflow](.github/workflows/release.yml) with [GameCI](https://game.ci). Pushing a version tag builds every platform and publishes a GitHub Release:

```bash
git tag v1.1.0
git push origin v1.1.0
```

| Platform | Build profile | Release files |
|----------|---------------|---------------|
| Windows | `Windows Profile` | `QrGenerator-<version>-Windows-Setup.exe` (installer), `QrGenerator-<version>-Windows.zip` (portable) |
| Linux | `Linux Profile` | `QrGenerator-<version>-Linux.tar.gz` |
| Android | `Android Profile` | `QrGenerator-<version>-Android.apk` (ARM64 only) |

The workflow can also be run by hand from the **Actions** tab: it builds everything and uploads the files as workflow artifacts, without publishing a release. The WebGL build is not part of the workflow.

The Windows installer is made with [Inno Setup](https://jrsoftware.org/isinfo.php) from [`installer/QrGenerator.iss`](installer/QrGenerator.iss). It's not code signed, so Windows SmartScreen shows a warning the first time it runs.

### Required secrets

Add these under **Settings → Secrets and variables → Actions → New repository secret**:

| Secret | Value |
|--------|-------|
| `UNITY_LICENSE` | Contents of your Unity license file, see below |
| `UNITY_EMAIL` | Email of your Unity account |
| `UNITY_PASSWORD` | Password of your Unity account |
| `ANDROID_KEYSTORE_BASE64` | The keystore file encoded in base64, see below |
| `ANDROID_KEYSTORE_PASS` | Keystore password |
| `ANDROID_KEYALIAS_NAME` | Key alias, e.g. `qr-generator` |
| `ANDROID_KEYALIAS_PASS` | Key password |

**Unity license (Personal):** in Unity Hub, sign in with the account used for CI and go to **Preferences → Licenses → Add → Get a free personal license**. Then copy the whole content of `C:\ProgramData\Unity\Unity_lic.ulf` into `UNITY_LICENSE`. See the [GameCI activation guide](https://game.ci/docs/github/activation) for other systems and license types.

**Android keystore:** encode the keystore and copy it to the clipboard with PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("$env:USERPROFILE\keystores\qr-generator-upload.jks")) | Set-Clipboard
```

Never commit the keystore to the repository, and keep a backup of it and its passwords: an app signed with a lost key can't be updated anymore.

## License

This project is open source under the **Apache License 2.0** license and it's intended for demo purposes.
