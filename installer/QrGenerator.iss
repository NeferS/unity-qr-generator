; Inno Setup script for the Windows build.
; The release workflow passes the defines below; they can also be given by hand:
;   ISCC.exe /DAppVersion=1.0.0 /DSourceDir=C:\path\to\build installer\QrGenerator.iss

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\build\StandaloneWindows64"
#endif
#ifndef OutputDir
  #define OutputDir "..\build\installer"
#endif
#ifndef OutputBaseFilename
  #define OutputBaseFilename "QrGenerator-" + AppVersion + "-Windows-Setup"
#endif

#define AppName "QrCode Generator"
#define AppPublisher "NeferCrafts"
#define AppExeName "QrGenerator.exe"

[Setup]
; Identifies the app for upgrades and uninstall, never change it
AppId={{02F7FC61-A90B-45B4-B4D9-44617D35A2C3}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/NeferS/unity-qr-generator
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Installs for the current user without admin rights, unless the user picks all users
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*_DoNotShip,*_ButDontShipItWithYourGame"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
