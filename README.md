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

This project assumes ZXing.Net is available through [UnityNuGet](https://github.com/bdovaz/UnityNuGet) and demonstrates its usage **as if it were already added to the NuGet registry**.

You can use either:
- `org.nuget.zxing.net` from the official registry
- `org.custom.zxing.net` from a local Docker-based UnityNuGet instance

> ℹ️ If you're using a custom registry, make sure to choose the right port for the url of the scoped registry (default is 5000).

### 2. **Manual DLL Import**

Alternatively, download the [prebuilt DLL](https://github.com/micjahn/ZXing.Net/blob/master/Clients/UnityDemo/Assets/zxing.unity.dll) from ZXing.Net 

## License

This project is open source under the **Apache License 2.0** license and it's intended for demo purposes.
