; FB2 Kindle offline installer. Compile with Inno Setup 7.1 or newer.
; Override AppSource and ReleaseDir with /D... for another checkout.
#ifndef AppSource
  #define AppSource AddBackslash(SourcePath) + "..\.."
#endif
#ifndef ReleaseDir
  #define ReleaseDir AppSource + "\.."
#endif
#define AppName "FB2 Kindle"
#define AppVersion "1.2.1"
#define AppExe "FB2Kindle.exe"

[Setup]
AppId={{DA28D6AF-554A-4826-BBE9-20A87FE8F59F}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=MerryGrim
AppPublisherURL=https://github.com/MerryGrim
VersionInfoVersion={#AppVersion}
VersionInfoDescription=FB2 Kindle offline installer
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\FB2Kindle
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
DisableDirPage=no
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible and not arm64
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
AllowRootDirectory=no
AllowUNCPath=no
UsePreviousAppDir=yes
UsePreviousTasks=yes
OutputDir={#ReleaseDir}
OutputBaseFilename=FB2Kindle-{#AppVersion}-Setup
Compression=lzma2/ultra64
SolidCompression=yes
CompressionThreads=auto
LZMANumBlockThreads=2
DiskSpanning=no
SetupIconFile={#AppSource}\source\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName} {#AppVersion}
Uninstallable=yes
CreateUninstallRegKey=yes
WizardStyle=modern
WizardSizePercent=110
WizardImageBackColor=$00FAF8F7
WizardSmallImageBackColor=$00FFFFFF
#ifdef WizardImage
WizardImageFile={#WizardImage}
#else
WizardImageFile={#AppSource}\source\installer-assets\wizard.bmp
#endif
#ifdef WizardSmallImage
WizardSmallImageFile={#WizardSmallImage}
#else
WizardSmallImageFile={#AppSource}\source\installer-assets\wizard-small.bmp
#endif
ShowLanguageDialog=yes
CloseApplications=yes
RestartApplications=no
SetupMutex=Local\FB2Kindle-Installer
DisableStartupPrompt=yes
ChangesAssociations=no
ChangesEnvironment=no
RestartIfNeededByRun=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[CustomMessages]
russian.DesktopShortcut=Создать ярлык на рабочем столе
russian.AdditionalShortcuts=Ярлыки:
russian.ReadInstructions=Инструкция
russian.RunProgram=Запустить FB2 Kindle
russian.DotNetRequired=Для FB2 Kindle нужен Microsoft .NET Framework 4.8 или более новая совместимая версия.%n%nУстановите его с официального сайта Microsoft и снова запустите этот установщик:%nhttps://dotnet.microsoft.com/download/dotnet-framework/net48%n%nНа современных версиях Windows 10/11 этот компонент обычно уже установлен.
russian.WelcomeLabel2=Конвертер книг FB2 и архивов ZIP, RAR и 7Z в EPUB и AZW3.%n%nВсе движки включены. Интернет для конвертации не нужен.%n%nНажмите «Далее», чтобы продолжить.
russian.FinishedLabel=FB2 Kindle установлен.%n%nДобавляйте книги кнопкой или перетаскивайте их в окно. Готовые EPUB и AZW3 сохраняются отдельными файлами.
english.DesktopShortcut=Create a desktop shortcut
english.AdditionalShortcuts=Shortcuts:
english.ReadInstructions=Instructions
english.RunProgram=Launch FB2 Kindle
english.DotNetRequired=FB2 Kindle requires Microsoft .NET Framework 4.8 or a later compatible version.%n%nInstall it from Microsoft's official website and run this installer again:%nhttps://dotnet.microsoft.com/download/dotnet-framework/net48%n%nRecent versions of Windows 10/11 usually include this component.
english.WelcomeLabel2=Convert FB2 books and ZIP, RAR and 7Z archives to EPUB and AZW3.%n%nAll conversion engines are included. Conversion works offline.%n%nClick Next to continue.
english.FinishedLabel=FB2 Kindle is installed.%n%nUse the Add books button or drop files into the window. Converted EPUB and AZW3 books are saved as separate files.
chinesesimplified.DesktopShortcut=创建桌面快捷方式
chinesesimplified.AdditionalShortcuts=快捷方式：
chinesesimplified.ReadInstructions=使用说明
chinesesimplified.RunProgram=启动 FB2 Kindle
chinesesimplified.DotNetRequired=FB2 Kindle 需要 Microsoft .NET Framework 4.8 或更高的兼容版本。%n%n请从 Microsoft 官方网站安装，然后重新运行此安装程序：%nhttps://dotnet.microsoft.com/download/dotnet-framework/net48%n%n较新的 Windows 10/11 通常已包含此组件。
chinesesimplified.WelcomeLabel2=将 FB2 图书和 ZIP、RAR、7Z 压缩包转换为 EPUB 或 AZW3。%n%n所有转换引擎均已包含，无需联网即可转换。%n%n点击“下一步”继续。
chinesesimplified.FinishedLabel=FB2 Kindle 安装完成。%n%n点击“添加图书”或将文件拖入窗口。转换后的 EPUB 和 AZW3 图书将保存为单独的文件。

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopShortcut}"; GroupDescription: "{cm:AdditionalShortcuts}"; Flags: unchecked

[Files]
; Engines, their original notices and matching source archives travel together.
; No wildcard delete is used by the uninstaller, so user-created books survive.
Source: "{#AppSource}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\engine\*"; DestDir: "{app}\engine"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#AppSource}\source\*"; DestDir: "{app}\source"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SourcePath}INSTALLATION.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; AppUserModelID: "FB2Kindle.Converter"
Name: "{group}\{cm:ReadInstructions}"; Filename: "{sys}\notepad.exe"; Parameters: """{app}\INSTALLATION.txt"""; WorkingDir: "{app}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; AppUserModelID: "FB2Kindle.Converter"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:RunProgram}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not IsDotNetInstalled(net48, 0) then
    Result := CustomMessage('DotNetRequired');
end;

procedure InitializeWizard;
begin
  WizardForm.Color := $00FAF8F7;
  WizardForm.MainPanel.Color := $00FFFFFF;
  WizardForm.PageNameLabel.Font.Color := $00EF5E15;
  WizardForm.WelcomeLabel1.Font.Color := $00EF5E15;
  WizardForm.FinishedHeadingLabel.Font.Color := $00EF5E15;
  WizardForm.WelcomeLabel2.Caption := CustomMessage('WelcomeLabel2');
  WizardForm.FinishedLabel.Caption := CustomMessage('FinishedLabel');
end;
