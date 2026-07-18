#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

#define MyAppName "TinyTable"
#define MyAppExeName "TinyTable.exe"

[Setup]
AppId={{9284796A-AB3B-4CD0-B887-2EAE53469C5F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=TinyTable
DefaultDirName={localappdata}\Programs\TinyTable
DefaultGroupName=TinyTable
DisableProgramGroupPage=yes
AllowNoIcons=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..\dist
OutputBaseFilename=TinyTable-Setup-{#MyAppVersion}
SetupIconFile=..\assets\TinyTable.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes
UsePreviousTasks=yes
LanguageDetectionMethod=uilanguage
UsePreviousLanguage=no
ShowLanguageDialog=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "chinesetraditional"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "vietnamese"; MessagesFile: "compiler:Default.isl"
Name: "indonesian"; MessagesFile: "compiler:Default.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"

[LangOptions]
chinesetraditional.LanguageName=繁體中文
chinesetraditional.LanguageID=$0404
chinesetraditional.LanguageCodePage=0
vietnamese.LanguageName=Tiếng Việt
vietnamese.LanguageID=$042A
vietnamese.LanguageCodePage=0
indonesian.LanguageName=Bahasa Indonesia
indonesian.LanguageID=$0421
indonesian.LanguageCodePage=0

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\TinyTable.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "TinyTable.local-data"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; DestName: "README.md"; Flags: ignoreversion

[Dirs]
Name: "{app}\data"; Permissions: users-modify

[Icons]
Name: "{group}\TinyTable"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\TinyTable"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,TinyTable}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--remove-context-menu"; RunOnceId: "RemoveTinyTableContextMenu"; Flags: runhidden waituntilterminated skipifdoesntexist

[UninstallDelete]
Type: files; Name: "{app}\TinyTable.initial-language"

[Code]
function AppLanguageCode: String;
var
  L: String;
begin
  L := ActiveLanguage;
  if L = 'chinesesimplified' then Result := 'zh-CN'
  else if L = 'chinesetraditional' then Result := 'zh-TW'
  else if L = 'german' then Result := 'de-DE'
  else if L = 'french' then Result := 'fr-FR'
  else if L = 'spanish' then Result := 'es-ES'
  else if L = 'italian' then Result := 'it-IT'
  else if L = 'brazilianportuguese' then Result := 'pt-BR'
  else if L = 'dutch' then Result := 'nl-NL'
  else if L = 'polish' then Result := 'pl-PL'
  else if L = 'russian' then Result := 'ru-RU'
  else if L = 'ukrainian' then Result := 'uk-UA'
  else if L = 'turkish' then Result := 'tr-TR'
  else if L = 'japanese' then Result := 'ja-JP'
  else if L = 'korean' then Result := 'ko-KR'
  else if L = 'vietnamese' then Result := 'vi-VN'
  else if L = 'indonesian' then Result := 'id-ID'
  else if L = 'czech' then Result := 'cs-CZ'
  else Result := 'en-US';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    SaveStringToFile(
      ExpandConstant('{app}\TinyTable.initial-language'),
      AppLanguageCode, False);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RegDeleteValue(HKEY_CURRENT_USER,
      'Software\Microsoft\Windows\CurrentVersion\Run', 'TinyTable');
end;
