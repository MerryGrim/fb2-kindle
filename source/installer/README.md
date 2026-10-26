# FB2 Kindle Mira Mint installer

Build the complete offline installer after the application and unmodified
Calibre 9.15.0 / 7-Zip 26.00 engines have been prepared:

```powershell
.\source\installer\build-installer.ps1 `
  -AppSource 'D:\AI\Projects\FB2Kindle' `
  -ReleaseDir 'D:\AI\Projects\FB2Kindle\outputs\1.3.1' `
  -InnoCompiler 'D:\AI\Tools\InnoSetup7\ISCC.exe' `
  -VisualStudioPath 'D:\AI\Tools\BuildTools'
```

Use `-SkipApplicationBuild` only after separately validating the current EXE.
The final distributable is `FB2Kindle-1.3.1-Setup.exe`; `installer-build/`
contains intermediate files, including the embedded Inno executable. Do not
distribute that inner executable as the public installer. Existing final output
is never replaced; select a new release folder for a repeated build.

The native x64 language selector embeds the Inno setup, keeps user command-line
arguments intact, and forwards English, Russian or Simplified Chinese. It
supports silent installation without showing the selector. Both the selector
and wizard use Mira Mint, integrated close controls, and Windows 11 DWM corner
hints. Ordinary folder and cancellation dialogs use the same treatment. Windows
10 may keep rectangular compositor corners. No security settings are changed.

The flow is language, welcome, license acceptance, destination, optional desktop
shortcut, confirmation, installation, completion. Native Inno Back/Next/Cancel
and rollback behavior remain in use. The MIT license text in each localized
agreement is unchanged. Third-party notices and matching engine source archives
are required by the build preflight and travel with the payload.

The per-user application ID and destination stay compatible with 1.2.1 upgrades.
No wildcard uninstall deletion is used. The uninstaller only removes installed
application files; books and user-created output are retained.

Tool requirements: Inno Setup 7.1 or newer, Visual Studio C++ Build Tools with a
Windows SDK, and the existing .NET Framework compiler. No build-time downloads
are performed. The bootstrap uses the static Microsoft C++ runtime; see
`licenses/bootstrap-NOTICE.md` and `licenses/Microsoft-STL-LICENSE.txt`.

Guarded diagnostics inherited from the Mira Mint installer are opt-in. For
example, `/VERYSILENT /LANG=english /MINTCOMMANDCHECKONLY="existing-folder"`
writes a command receipt and aborts before installation. A guarded Abort can
return exit code 1; inspect its `installExecuted:false` receipt. Diagnostics are
not evidence of a completed installation, upgrade or removal.
