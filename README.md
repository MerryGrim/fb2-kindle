# FB2 Kindle 1.3.0

[English](#english) · [Русский](#русский) · [简体中文](#简体中文)

A local Windows desktop converter from FB2 to EPUB and AZW3, with light and dark MiraUI themes, drag and drop, batch conversion, and ZIP, RAR, and 7Z support.

![FB2 Kindle 1.3.0 — English, light MiraUI theme](docs/screenshots/1.3.0-en-light.png)

**[Download for Windows](https://github.com/MerryGrim/fb2-kindle/releases/download/v1.3.0/FB2Kindle-1.3.0-Setup.exe)** · [Website](https://MerryGrim.github.io/fb2-kindle/) · [Source archive](https://github.com/MerryGrim/fb2-kindle/releases/download/v1.3.0/FB2Kindle-Source-1.3.0.zip) · [All releases](https://github.com/MerryGrim/fb2-kindle/releases) · [Author](https://github.com/MerryGrim)

## English

FB2 Kindle turns individual FB2 books and book archives into separate EPUB or AZW3 files. Add files with the button or drag them into the window, choose an output format and folder, and start conversion. Books inside an archive appear as individual entries in the queue. Existing files are preserved; duplicate names receive a number.

- **Inputs:** FB2, ZIP, RAR, and 7Z, including archives containing multiple books.
- **Outputs:** EPUB for Amazon Send to Kindle and compatible readers; AZW3 for direct USB transfer to a compatible Kindle.
- **Languages:** English, Russian, and Simplified Chinese. Switch the application language from the main window.
- **Local processing:** conversion runs on your computer. The app saves files; it does not upload books or manage a connected device.
- **Queue:** overall progress and a mini progress bar for each book, automatic scrolling to the current book, and a stop action. The queue has its own scrollbar only when it contains more than seven books.
- **Destination:** save beside each original book or in a chosen folder; folder selection and result opening are grouped together.
- **Interface:** compact MiraUI with mint accents, light and dark themes, integrated window controls, and a themed About dialog for version information, credits, licenses, and the author's GitHub profile.

### Installation

Download [FB2Kindle-1.3.0-Setup.exe](https://github.com/MerryGrim/fb2-kindle/releases/download/v1.3.0/FB2Kindle-1.3.0-Setup.exe). The offline installer includes unchanged Calibre 9.15.0 and 7-Zip 26.00 runtimes, their licenses and notices, and corresponding source archives. Select English, Русский, or 简体中文, then follow the MiraUI installation flow.

**Requirements:** Windows 10 or 11, x64, and .NET Framework 4.8. Password-protected archives must be unpacked first. A single FB2 book is limited to 96 MiB. Some named FB2 styles are simplified; rendering depends on the reader and its firmware.

EPUB is intended for Send to Kindle or another EPUB-compatible reader. For ordinary USB transfer to Kindle, choose AZW3. Simply copying an EPUB file to a Kindle does not convert it.

### License

The application's own source code, documentation, and assets are licensed under [MIT](LICENSE), © 2026 MerryGrim. Calibre, 7-Zip, and Inno Setup retain their own licenses; MIT does not cover them. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

![FB2 Kindle 1.3.0 — English, dark MiraUI theme](docs/screenshots/1.3.0-en-dark.png)

## Русский

FB2 Kindle превращает отдельные FB2-книги и архивы с книгами в самостоятельные файлы EPUB или AZW3. Добавьте файлы кнопкой или перетащите их в окно, выберите формат и папку сохранения, затем запустите конвертацию. Каждая книга из архива появляется отдельной строкой в очереди. Существующие файлы сохраняются; одинаковые названия получают номера.

- **Входные форматы:** FB2, ZIP, RAR и 7Z, включая архивы с несколькими книгами.
- **Выходные форматы:** EPUB для Amazon Send to Kindle и совместимых читалок; AZW3 для прямого переноса по USB на совместимый Kindle.
- **Языки:** Русский, English и 简体中文. Переключатель языка находится в главном окне.
- **Локальная работа:** конвертация выполняется на вашем компьютере. Программа сохраняет файлы, не отправляет книги на сервер и не управляет подключённой читалкой.
- **Очередь:** общий прогресс и мини-прогресс каждой книги, автоматическая прокрутка к текущей книге и остановка обработки. Внутренняя прокрутка появляется только после семи книг.
- **Сохранение:** рядом с каждой исходной книгой или в выбранную папку; выбор папки и открытие результата собраны в одном блоке.
- **Интерфейс:** компактный MiraUI с мятными акцентами, светлой и тёмной темами, встроенными кнопками окна и тематическим окном «О приложении» с версиями, авторством, лицензиями и профилем автора на GitHub.

### Установка

Скачайте [FB2Kindle-1.3.0-Setup.exe](https://github.com/MerryGrim/fb2-kindle/releases/download/v1.3.0/FB2Kindle-1.3.0-Setup.exe). Автономный установщик включает неизменённые движки Calibre 9.15.0 и 7-Zip 26.00, их лицензии, уведомления и соответствующие исходные архивы. Выберите Русский, English или 简体中文 и пройдите установку в MiraUI. Для конвертации интернет не нужен.

**Требования:** Windows 10 или 11, x64, и .NET Framework 4.8. Парольные архивы предварительно распакуйте. Предел одной FB2-книги — 96 МиБ. Некоторые именованные стили FB2 упрощаются; отображение зависит от читалки и её прошивки.

EPUB предназначен для Send to Kindle и других совместимых читалок. Для обычного переноса на Kindle по USB выбирайте AZW3: простое копирование EPUB не преобразует книгу.

### Лицензия

Собственный код, документация и ресурсы программы распространяются по [MIT](LICENSE), © 2026 MerryGrim. Calibre, 7-Zip и Inno Setup сохраняют свои лицензии; MIT на них не распространяется. Подробности: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## 简体中文

FB2 Kindle 可将单本 FB2 电子书或包含多本书的压缩包转换为独立的 EPUB 或 AZW3 文件。点击按钮添加文件，或将文件拖入窗口；选择输出格式和保存文件夹，然后开始转换。压缩包中的每本书都会单独列入队列。现有文件不会被覆盖，重名文件会自动添加编号。

- **输入格式：** FB2、ZIP、RAR 和 7Z，支持包含多本书的压缩包。
- **输出格式：** EPUB 可用于 Amazon Send to Kindle 和兼容阅读器；AZW3 可通过 USB 直接传输至兼容的 Kindle。
- **界面语言：** 英语、俄语和简体中文，可在主窗口切换。
- **本地处理：** 转换在您的电脑上完成。程序只生成文件，不会上传电子书，也不会管理连接的阅读设备。
- **队列：** 整体进度和每本书的迷你进度条、自动滚动到当前电子书，以及停止处理。超过七本书时，队列内部才会出现滚动条。
- **保存位置：** 原文件旁或指定文件夹；选择文件夹和打开结果的操作集中在同一区域。
- **界面风格：** 紧凑的 MiraUI 布局、薄荷色点缀、浅色和深色主题，以及集成窗口按钮。采用相同主题的“关于”窗口提供版本、作者、许可证及作者的 GitHub 主页链接。

### 安装

下载 [FB2Kindle-1.3.0-Setup.exe](https://github.com/MerryGrim/fb2-kindle/releases/download/v1.3.0/FB2Kindle-1.3.0-Setup.exe)。离线安装包包含未经修改的 Calibre 9.15.0 和 7-Zip 26.00 引擎，以及对应的许可证、声明和源码压缩包。选择 English、Русский 或 简体中文，然后按照 MiraUI 安装界面完成安装。转换时无需联网。

**系统要求：** Windows 10 或 11（x64），以及 .NET Framework 4.8。加密压缩包需要先自行解压。单本 FB2 文件上限为 96 MiB。部分 FB2 命名样式会被简化，实际显示效果取决于阅读器及其固件。

EPUB 适用于 Send to Kindle 或其他支持 EPUB 的阅读器。通过 USB 直接传输至 Kindle 时，请选择 AZW3；仅复制 EPUB 文件不会将其转换为 Kindle 格式。

### 许可证

本程序自行编写的源代码、文档和资源采用 [MIT 许可证](LICENSE)，© 2026 MerryGrim。Calibre、7-Zip 和 Inno Setup 保留各自的许可证，MIT 许可证不适用于这些第三方组件。详情请参阅 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)。

## Building from source

The application remains C# WinForms on .NET Framework 4.8. The source repository contains no bundled engines or books. To compile on Windows, run PowerShell from the repository root:

```powershell
.\source\build.ps1
.\FB2Kindle.exe
```

No external packages are required to compile the app. Keep `FB2Kindle.exe.config` next to the executable.

Ordinary FB2 → EPUB conversion uses the app's own code. For AZW3, install [Calibre](https://calibre-ebook.com/download_windows); the app finds a standard installation or `ebook-convert.exe` on `PATH`. For RAR and 7Z, install [7-Zip x64](https://www.7-zip.org/). Some unusual ZIP entry names also require 7-Zip.

Alternatively, supply the unchanged portable engines alongside the executable:

```text
engine/
  Calibre/       ebook-convert.exe, with its complete portable runtime
  Archive7z/     7z.exe and 7z.dll
```

Calibre needs its full portable directory, including libraries and resources.

### Command line

```text
FB2Kindle.exe --convert "C:\Books\book.fb2" "C:\Books\book.epub"
FB2Kindle.exe --convert "C:\Books\book.fb2" "C:\Books\book.azw3"
```

The destination file must not already exist. In command-line mode, an archive must contain exactly one FB2 book. Exit codes: `0` for success, `1` for an error.

### Building the installer

Install [Inno Setup 7.1 or newer](https://jrsoftware.org/isdl.php). The script finds `ISCC.exe` on `PATH` or in its standard installation folder, or accepts `-InnoCompiler`. The native installer bootstrap also requires Visual Studio Build Tools with Desktop C++ and a Windows SDK; pass `-VisualStudioPath` if automatic discovery is unavailable.

For the complete offline installer, first prepare `engine/` with the full, unchanged Calibre and 7-Zip runtimes, their licenses and copyright notices, and the corresponding source archives. Include the Inno Setup license at `engine/InnoSetup/LICENSE.txt`. These components are not included in this repository. Follow their distribution requirements in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

```powershell
.\source\installer\build-installer.ps1 -Version 1.3.0
```

The final installer is `FB2Kindle-1.3.0-Setup.exe`. By default it is written to the parent directory; use `-ReleaseDir` to choose a clean output directory and `-AppSource` to choose the prepared application directory. Existing installer output is not overwritten. When invoking `ISCC.exe` directly, `/DAppSource` and `/DReleaseDir` override the source and output directories; use the build script to include the native bootstrap.

### Website

The static GitHub Pages website is in `docs/index.html`; no build tools or network dependencies are needed. Its three languages and two themes use six native application screenshots in `docs/screenshots/`. GitHub Pages can serve `main` → `/docs`. The website does not perform conversion or receive books.

See [CHANGELOG.md](CHANGELOG.md) for release changes. Source and software checks do not establish physical Kindle or live Send to Kindle compatibility. Transfer to a physical Kindle and the Send to Kindle service have not been tested.
