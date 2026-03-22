[Setup]
AppName=TextCleaner
AppVersion=1.0
AppPublisher=TextCleaner
DefaultDirName={autopf}\TextCleaner
DefaultGroupName=TextCleaner
OutputDir=Z:\home\geekom\project\TextCleaner\installer
OutputBaseFilename=TextCleanerSetup
SetupIconFile=Z:\home\geekom\project\TextCleaner\Assets\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "Z:\home\geekom\project\TextCleaner\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "Z:\home\geekom\project\TextCleaner\redist\WindowsAppRuntimeInstall-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\TextCleaner"; Filename: "{app}\TextCleaner.exe"; IconFilename: "{app}\Assets\app.ico"
Name: "{autodesktop}\TextCleaner"; Filename: "{app}\TextCleaner.exe"; IconFilename: "{app}\Assets\app.ico"; Tasks: desktopicon

[Run]
; Install Windows App Runtime silently first (skips if already installed)
Filename: "{tmp}\WindowsAppRuntimeInstall-x64.exe"; Parameters: "--quiet"; StatusMsg: "Installing Windows App Runtime..."; Flags: waituntilterminated
; Then launch the app
Filename: "{app}\TextCleaner.exe"; Description: "{cm:LaunchProgram,TextCleaner}"; Flags: nowait postinstall skipifsilent
