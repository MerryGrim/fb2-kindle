; FB2 Kindle offline installer. Compile through build-installer.ps1.
; Mira Mint controls and local language bootstrap are part of this MIT source.
#ifndef AppSource
  #define AppSource AddBackslash(SourcePath) + "..\.."
#endif
#ifndef ReleaseDir
  #define ReleaseDir AppSource + "\.."
#endif
#define AppName "FB2 Kindle"
#ifndef AppVersion
  #define AppVersion "1.3.0"
#endif
#ifndef OutputName
  #define OutputName "FB2Kindle.Inner"
#endif
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
OutputBaseFilename={#OutputName}
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
SetupArchitecture=x64
WizardStyle=modern dynamic windows11 hidebevels
WizardSizePercent=110
WizardImageFile=
WizardSmallImageFile=
ShowLanguageDialog=no
LanguageDetectionMethod=uilanguage
UsePreviousLanguage=yes
CloseApplications=yes
RestartApplications=no
SetupMutex=Local\FB2Kindle-Installer
DisableStartupPrompt=yes
ChangesAssociations=no
ChangesEnvironment=no
RestartIfNeededByRun=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "licenses\FB2Kindle-ru.txt"
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "licenses\FB2Kindle-en.txt"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"; LicenseFile: "licenses\FB2Kindle-zh-CN.txt"

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

russian.CloseWindow=Закрыть
english.CloseWindow=Close
chinesesimplified.CloseWindow=关闭
russian.MoveWindow=Перетащите свободную верхнюю область, чтобы переместить окно
english.MoveWindow=Drag the empty top area to move this window
chinesesimplified.MoveWindow=拖动顶部空白区域可移动窗口
russian.FolderTitle=Выбор папки
english.FolderTitle=Choose a folder
chinesesimplified.FolderTitle=选择文件夹
russian.FolderHint=Выберите папку или введите новый локальный путь.
english.FolderHint=Choose a folder or enter a new local path.
chinesesimplified.FolderHint=请选择文件夹或输入新的本地路径。
russian.FolderChoose=Выбрать папку
english.FolderChoose=Select folder
chinesesimplified.FolderChoose=选择文件夹
russian.FolderInvalid=Укажите локальный путь с буквой диска, например D:\Programs\FB2Kindle, не корень диска.
english.FolderInvalid=Enter a local path with a drive letter, such as D:\Programs\FB2Kindle, excluding the drive root.
chinesesimplified.FolderInvalid=请输入包含盘符的本地路径，例如 D:\Programs\FB2Kindle，不要选择磁盘根目录。
russian.CancelTitle=Отменить установку?
english.CancelTitle=Cancel setup?
chinesesimplified.CancelTitle=取消安装？
russian.CancelBody=Мастер закроется. Если копирование уже началось, Inno отменит установку штатным способом.
english.CancelBody=Setup will close. If copying has started, Inno will perform its normal cancellation.
chinesesimplified.CancelBody=安装向导将关闭。如果已开始复制，Inno 将执行正常的取消流程。
russian.CancelExit=Выйти из мастера
english.CancelExit=Exit setup
chinesesimplified.CancelExit=退出安装
russian.CancelKeep=Продолжить установку
english.CancelKeep=Keep setup open
chinesesimplified.CancelKeep=保留安装向导

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopShortcut}"; GroupDescription: "{cm:AdditionalShortcuts}"; Flags: unchecked

[Files]
; Engines, their original notices and matching source archives travel together.
; No wildcard delete is used by the uninstaller, so user-created books survive.
Source: "{#AppSource}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppSource}\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion
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
const
  GWL_STYLE = -16;
  WS_CAPTION = $00C00000;
  SWP_NOSIZE = $0001;
  SWP_NOMOVE = $0002;
  SWP_NOZORDER = $0004;
  SWP_NOACTIVATE = $0010;
  SWP_FRAMECHANGED = $0020;
  WM_NCHITTEST = $0084;
  WM_NCDESTROY = $0082;
  HTCAPTION = 2;
  WizardSubclassId = 41721;

type
  TInstallerPoint = record X, Y: Integer; end;
  TInstallerBounds = record Left, Top, Width, Height: Integer; end;

function GetWindowLongPtr(Window: HWND; Index: Integer): NativeInt;
  external 'GetWindowLongPtrW@user32.dll stdcall';
function SetWindowLongPtr(Window: HWND; Index: Integer; Value: NativeInt): NativeInt;
  external 'SetWindowLongPtrW@user32.dll stdcall';
function SetWindowPos(Window, InsertAfter: HWND; X, Y, Width, Height: Integer; Flags: Cardinal): Boolean;
  external 'SetWindowPos@user32.dll stdcall';
function ScreenToClient(Window: HWND; var Point: TInstallerPoint): Boolean;
  external 'ScreenToClient@user32.dll stdcall';
function SetWindowSubclass(Window: HWND; Callback: NativeInt; Id, RefData: NativeUInt): Boolean;
  external 'SetWindowSubclass@comctl32.dll stdcall';
function RemoveWindowSubclass(Window: HWND; Callback: NativeInt; Id: NativeUInt): Boolean;
  external 'RemoveWindowSubclass@comctl32.dll stdcall';
function DefSubclassProc(Window: HWND; Msg: Cardinal; WParam: NativeUInt; LParam: NativeInt): NativeInt;
  external 'DefSubclassProc@comctl32.dll stdcall';

var
  WindowCloseButton: TNewButton;
  WizardCallback: NativeInt;
  WizardWindow: HWND;

#include "MiraMintButtons.iss"
#include "MiraMintDialogs.iss"

function WizardWindowProc(Window: HWND; Msg: Cardinal; WParam: NativeUInt;
  LParam: NativeInt; Id, RefData: NativeUInt): NativeInt;
var
  Point: TInstallerPoint;
  InCloseButton: Boolean;
begin
  if Msg = WM_NCDESTROY then
    RemoveWindowSubclass(Window, WizardCallback, WizardSubclassId);
  if (Msg = WM_NCHITTEST) and (WindowCloseButton <> nil) then begin
    { LPARAM packs signed screen coordinates, including monitors left/above primary. }
    Point.X := LParam and $FFFF;
    Point.Y := (LParam shr 16) and $FFFF;
    if Point.X >= $8000 then Point.X := Point.X - $10000;
    if Point.Y >= $8000 then Point.Y := Point.Y - $10000;
    if ScreenToClient(Window, Point) then begin
      InCloseButton := (Point.X >= WindowCloseButton.Left) and
        (Point.X < WindowCloseButton.Left + WindowCloseButton.Width) and
        (Point.Y >= WindowCloseButton.Top) and
        (Point.Y < WindowCloseButton.Top + WindowCloseButton.Height);
      if (Point.X >= 0) and (Point.X < WizardForm.ClientWidth) and (Point.Y >= 0) and
        (Point.Y < WindowCloseButton.Top + WindowCloseButton.Height + ScaleY(4)) and not InCloseButton then begin
        Result := HTCAPTION;
        Exit;
      end;
    end;
  end;
  Result := DefSubclassProc(Window, Msg, WParam, LParam);
end;

procedure RemoveNativeCaption;
var
  Style: NativeInt;
begin
  { SetupArchitecture=x64 makes pointer-sized declarations correct for this process. }
  Style := GetWindowLongPtr(WizardForm.Handle, GWL_STYLE);
  if (Style and WS_CAPTION) <> 0 then begin
    SetWindowLongPtr(WizardForm.Handle, GWL_STYLE, Style and not WS_CAPTION);
    SetWindowPos(WizardForm.Handle, 0, 0, 0, 0, 0,
      SWP_NOSIZE or SWP_NOMOVE or SWP_NOZORDER or SWP_NOACTIVATE or SWP_FRAMECHANGED);
  end;
  MintApplyDwmCorners(WizardForm.Handle);
end;

procedure CloseWizardWindow(Sender: TObject);
begin
  { Use Inno's normal close query/cancel/rollback path, never terminate the process. }
  WizardForm.Close;
end;

procedure SyncWindowCloseButton(FinishedPage: Boolean);
var AllowClose: Boolean;
begin
  if WindowCloseButton = nil then Exit;
  AllowClose := WizardForm.CancelButton.Enabled or FinishedPage;
  if WindowCloseButton.Enabled <> AllowClose then
    WindowCloseButton.Enabled := AllowClose;
end;

procedure CurInstallProgressChanged(CurProgress, MaxProgress: Integer);
begin
  { File progress does not change captions, fonts or styles. Keep the native
    cancel/rollback gate, without repainting every button on every file. }
  SyncWindowCloseButton(False);
end;

#ifdef MintProgressProofInclude
  #if Pos("MOCK", OutputName) == 0
    #error MintProgressProofInclude is only allowed in explicitly named MOCK builds
  #endif
  #include MintProgressProofInclude
#endif

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not IsDotNetInstalled(net48, 0) then
    Result := CustomMessage('DotNetRequired');
end;

procedure InitializeWizard;
var
  I, Padding, Count, OriginalWidth, OriginalHeight: Integer;
  OriginalBounds: array of TInstallerBounds;
begin
  MintRunCommandProof;
  { Keep native page content and Back/Next/Cancel behavior, with an empty drag strip. }
  Padding := ScaleY(32);
  Count := WizardForm.ControlCount;
  OriginalWidth := WizardForm.ClientWidth;
  OriginalHeight := WizardForm.ClientHeight;
  SetArrayLength(OriginalBounds, Count);
  for I := 0 to Count - 1 do begin
    OriginalBounds[I].Left := WizardForm.Controls[I].Left;
    OriginalBounds[I].Top := WizardForm.Controls[I].Top;
    OriginalBounds[I].Width := WizardForm.Controls[I].Width;
    OriginalBounds[I].Height := WizardForm.Controls[I].Height;
  end;
  RemoveNativeCaption;
  WizardForm.ClientWidth := OriginalWidth;
  WizardForm.ClientHeight := OriginalHeight + Padding;
  { Client resize updates anchored heights; restore complete original bounds. }
  for I := 0 to Count - 1 do
    WizardForm.Controls[I].SetBounds(OriginalBounds[I].Left,
      OriginalBounds[I].Top + Padding, OriginalBounds[I].Width, OriginalBounds[I].Height);

  WizardForm.WizardBitmapImage.Visible := False;
  WizardForm.WizardBitmapImage2.Visible := False;
  WizardForm.WizardSmallBitmapImage.Visible := False;
  WizardForm.SelectDirBitmapImage.Visible := False;
  WizardForm.SelectGroupBitmapImage.Visible := False;
  { Reclaim the text gutter left by the removed directory/group pictograms. }
  WizardForm.SelectDirLabel.Left := 0;
  WizardForm.SelectDirLabel.Width := WizardForm.InnerPage.ClientWidth;
  WizardForm.SelectDirBrowseLabel.Left := 0;
  WizardForm.SelectDirBrowseLabel.Width := WizardForm.InnerPage.ClientWidth;
  WizardForm.DirEdit.Left := 0;
  WizardForm.DirBrowseButton.Left := WizardForm.InnerPage.ClientWidth - WizardForm.DirBrowseButton.Width;
  WizardForm.DirEdit.Width := WizardForm.DirBrowseButton.Left - ScaleX(8);
  WizardForm.SelectStartMenuFolderLabel.Left := 0;
  WizardForm.SelectStartMenuFolderLabel.Width := WizardForm.InnerPage.ClientWidth;
  WizardForm.SelectStartMenuFolderBrowseLabel.Left := 0;
  WizardForm.SelectStartMenuFolderBrowseLabel.Width := WizardForm.InnerPage.ClientWidth;
  WizardForm.GroupEdit.Left := 0;
  WizardForm.GroupBrowseButton.Left := WizardForm.InnerPage.ClientWidth - WizardForm.GroupBrowseButton.Width;
  WizardForm.GroupEdit.Width := WizardForm.GroupBrowseButton.Left - ScaleX(8);
  WizardForm.WelcomeLabel1.Left := ScaleX(24);
  WizardForm.WelcomeLabel1.Width := WizardForm.WelcomePage.ClientWidth - ScaleX(48);
  WizardForm.WelcomeLabel2.Left := ScaleX(24);
  WizardForm.WelcomeLabel2.Width := WizardForm.WelcomePage.ClientWidth - ScaleX(48);
  WizardForm.WelcomeLabel2.Caption := CustomMessage('WelcomeLabel2');
  WizardForm.FinishedHeadingLabel.Left := ScaleX(24);
  WizardForm.FinishedHeadingLabel.Width := WizardForm.FinishedPage.ClientWidth - ScaleX(48);
  WizardForm.FinishedLabel.Left := ScaleX(24);
  WizardForm.FinishedLabel.Width := WizardForm.FinishedPage.ClientWidth - ScaleX(48);
  WizardForm.FinishedLabel.Caption := CustomMessage('FinishedLabel');
  WizardForm.PageNameLabel.Width := WizardForm.MainPanel.ClientWidth - WizardForm.PageNameLabel.Left - ScaleX(24);
  WizardForm.PageDescriptionLabel.Width := WizardForm.PageNameLabel.Width;
  WizardForm.Hint := CustomMessage('MoveWindow');
  WizardForm.ShowHint := True;

  WindowCloseButton := TNewButton.Create(WizardForm);
  WindowCloseButton.Parent := WizardForm;
  WindowCloseButton.SetBounds(WizardForm.ClientWidth - ScaleX(40), ScaleY(2), ScaleX(32), ScaleY(28));
  WindowCloseButton.Anchors := [akTop, akRight];
  WindowCloseButton.Caption := '×';
  WindowCloseButton.OnClick := @CloseWizardWindow;
  WindowCloseButton.TabStop := True;
  WindowCloseButton.Hint := CustomMessage('CloseWindow');
  WindowCloseButton.ShowHint := True;
  MintInitializeButtons(WindowCloseButton);
  WizardForm.DirBrowseButton.OnClick := @MintBrowseFolder;
  MintRunPopupProof;
  MintWriteOptionalPainterProof;
  { Diagnostic proof-only mode exits silently here, before any installation step. }
  if ExpandConstant('{param:MINTBUTTONPROOFONLY|0}') = '1' then begin
    MintShutdownButtons;
    Abort;
  end;
  WizardWindow := WizardForm.Handle;
  WizardCallback := CreateCallback(@WizardWindowProc);
  if not SetWindowSubclass(WizardForm.Handle, WizardCallback, WizardSubclassId, 0) then
    Log('Custom drag area could not be enabled; Alt+Space, Move remains available.');
  RemoveNativeCaption;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  RemoveNativeCaption;
  SyncWindowCloseButton(CurPageID = wpFinished);
  MintRefreshButtons;
#ifdef MintProgressProofInclude
  if CurPageID = wpWelcome then MintRunProgressProof;
#endif
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  PreferencesDirectory, LanguageFile, LanguageCode: String;
begin
  MintGuardInstallStep(CurStep);
  if CurStep <> ssPostInstall then Exit;
  { Match the selected setup language on first launch. An upgrade preserves
    the language the user has already chosen inside the application. }
  PreferencesDirectory := ExpandConstant('{userappdata}\FB2Kindle');
  LanguageFile := PreferencesDirectory + '\language.txt';
  if FileExists(LanguageFile) then Exit;
  LanguageCode := 'en';
  if ActiveLanguage = 'russian' then LanguageCode := 'ru'
  else if ActiveLanguage = 'chinesesimplified' then LanguageCode := 'zh';
  if ForceDirectories(PreferencesDirectory) then
    if not SaveStringToFile(LanguageFile, LanguageCode, False) then
      Log('Could not initialize application language; the in-app picker remains available.');
end;

procedure DeinitializeSetup;
begin
  MintShutdownButtons;
  if (WizardWindow <> 0) and (WizardCallback <> 0) then
    RemoveWindowSubclass(WizardWindow, WizardCallback, WizardSubclassId);
end;
