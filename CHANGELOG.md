# Changelog

## 1.3.1 — 2026-10-26

### English

- The first launch now opens in the center of the active screen, with a smaller default width and height of 960 × 820 logical pixels. The compact layout still fits seven books before the queue needs its own scrollbar.
- The app remembers the window position, size, and maximized state between launches. Closing a minimized window preserves its previous normal or maximized state.
- Saved window bounds are fitted to the available screen if a monitor is disconnected or the display working area changes.

### Русский

- При первом запуске окно открывается по центру активного экрана; начальные ширина и высота уменьшены до 960 × 820 логических пикселей. Компактный макет по-прежнему вмещает семь книг до появления внутренней прокрутки очереди.
- Позиция, размер и развёрнутое состояние окна сохраняются между запусками. Закрытие свёрнутого окна сохраняет его предыдущее обычное или развёрнутое состояние.
- После отключения монитора или изменения рабочей области экрана сохранённое окно размещается в доступной области.

### 简体中文

- 首次启动时，窗口在当前屏幕中央打开，默认宽度和高度缩小为 960 × 820 个逻辑像素。紧凑布局仍可容纳七本书，超过后才在队列内显示滚动条。
- 程序会记住窗口的位置、大小和最大化状态。关闭已最小化的窗口时，会保留其之前的普通或最大化状态。
- 显示器断开连接或屏幕工作区发生变化后，会调整已保存的窗口位置和大小，使其位于可用区域内。

## 1.3.0 — 2026-10-24

### English

- Replaced the application interface with the approved compact MiraUI layout: mint accents, light and dark themes, integrated window controls, and a themed About dialog.
- Added an individual progress bar for every book alongside the overall queue progress.
- Added automatic scrolling to the book being processed. The queue gains its own scrollbar only after seven books.
- Grouped destination controls into two clear modes: beside the original books or in a chosen folder, with folder selection and result opening together.
- Retained FB2, ZIP, RAR, and 7Z input; EPUB/AZW3 output; add, remove, clear, and stop actions; and English, Russian, and Simplified Chinese.
- Applied MiraUI to the Windows installation flow. The offline package retains unchanged Calibre 9.15.0 and 7-Zip 26.00, their licenses and notices, and corresponding source archives.
- Added a static three-language GitHub Pages website with light/dark themes, native application screenshots, format guidance, and direct release downloads.

### Русский

- Интерфейс переведён на согласованный компактный макет MiraUI: мятные акценты, светлая и тёмная темы, встроенные кнопки окна и тематическое окно «О приложении».
- К общему прогрессу очереди добавлен отдельный мини-прогресс каждой книги.
- Добавлена автоматическая прокрутка к обрабатываемой книге. Внутренняя прокрутка очереди появляется только после семи книг.
- Сохранение собрано в два понятных режима: рядом с исходными книгами или в выбранную папку; выбор папки и открытие результата находятся вместе.
- Сохранены входные FB2, ZIP, RAR и 7Z, выходные EPUB/AZW3, добавление, удаление, очистка и остановка, а также Русский, English и 简体中文.
- MiraUI применён к установке Windows. Автономный пакет сохраняет неизменённые Calibre 9.15.0 и 7-Zip 26.00, их лицензии, уведомления и соответствующие исходные архивы.
- Добавлена статическая страница GitHub Pages на трёх языках со светлой и тёмной темами, снимками настоящего интерфейса, пояснениями форматов и прямым скачиванием релиза.

### 简体中文

- 程序界面改用已确认的紧凑 MiraUI 布局：薄荷色点缀、浅色和深色主题、集成窗口按钮，以及采用相同主题的“关于”窗口。
- 在整体队列进度之外，为每本书加入独立的迷你进度条。
- 加入自动滚动到当前电子书的功能。超过七本书时，队列内部才会出现滚动条。
- 保存设置明确分为原文件旁和指定文件夹两种模式，并将选择文件夹与打开结果的操作放在一起。
- 保留 FB2、ZIP、RAR 和 7Z 输入、EPUB/AZW3 输出、添加、移除、清空和停止操作，以及英语、俄语和简体中文。
- Windows 安装流程采用 MiraUI。离线安装包保留未经修改的 Calibre 9.15.0 和 7-Zip 26.00，以及对应的许可证、声明和源码压缩包。
- 加入三语静态 GitHub Pages 网站，提供浅色和深色主题、真实程序截图、格式说明，以及版本直接下载链接。

## 1.2.1 — 2026-10-02

- Added English, Russian, and Simplified Chinese to the application and installer.
- Added installer language selection and a welcome page.
- Improved button contrast and centered the drop area.
- Added a separate About window with versions, licenses, and the author's GitHub profile.
- Published the application's own code, documentation, and assets under MIT.
