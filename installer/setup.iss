; Inno Setup 6+ script for VLESS VPN Client.
; Builds an installer that bundles the published app and downloads
; the latest xray-core release on first run (or right after install).

#define AppName "TYU HUB"
#define AppShortName "TyuHub"
#define AppVersion "1.0.0"
#define AppPublisher "Timaski"
#define AppExeName "VlessVpnClient.exe"
#define PublishedAppDir "..\publish\VlessVpnClient"

[Setup]
AppId={{C8E5F6F0-2A3B-4C9D-9E5A-1B6F1F7B5D11}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/Timaski/vless-vpn-client
AppSupportURL=https://github.com/Timaski/vless-vpn-client/issues
AppUpdatesURL=https://github.com/Timaski/vless-vpn-client/releases
DefaultDirName={autopf}\{#AppShortName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog commandline
OutputBaseFilename=TyuHub-Setup-{#AppVersion}
Compression=lzma2/ultra
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\VlessVpnClient.App\Resources\app.ico
UninstallDisplayIcon={app}\{#AppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Запускать вместе с Windows"; GroupDescription: "Дополнительно:"; Flags: unchecked
Name: "downloadxray"; Description: "Скачать актуальный xray-core при установке (рекомендуется)"; GroupDescription: "Дополнительно:";

[Files]
Source: "{#PublishedAppDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "download-xray.ps1"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "VlessVpnClient"; ValueData: """{app}\{#AppExeName}"" --minimized"; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "powershell.exe"; \
  Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\download-xray.ps1"" -InstallDir ""{app}\xray"""; \
  StatusMsg: "Скачивание xray-core…"; \
  Tasks: downloadxray; \
  Flags: runhidden waituntilterminated
Filename: "{app}\{#AppExeName}"; \
  Description: "{cm:LaunchProgram,{#AppName}}"; \
  Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\xray"
Type: filesandordirs; Name: "{localappdata}\VlessVpnClient"
