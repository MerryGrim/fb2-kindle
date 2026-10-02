namespace Fb2Kindle
{
    static partial class L10n
    {
        static void InitializeTranslations()
        {
            Add("Собственный код, документация и ресурсы FB2 Kindle распространяются по MIT. Сторонние компоненты в папке engine сохраняют свои лицензии.", "FB2 Kindle's own code, documentation and assets are licensed under MIT. Third-party components in the engine folder retain their own licenses.", "FB2 Kindle 自有的代码、文档和资源采用 MIT 许可证。engine 文件夹中的第三方组件保留各自的许可证。");
            // Window captions and actions.
            Add("Конвертер электронных книг", "E-book converter", "电子书转换器");
            Add("Добавить книги", "Add books", "添加图书");
            Add("Убрать выбранные", "Remove selected", "移除所选");
            Add("Очистить", "Clear", "清空");
            Add("Выберите книги FB2 или архивы ZIP, RAR, 7Z.", "Choose FB2 books or ZIP, RAR, 7Z archives.", "请选择 FB2 图书或 ZIP、RAR、7Z 压缩包。");
            Add("ФОРМАТ ФАЙЛА", "FILE FORMAT", "文件格式");
            Add("Для переноса на Kindle\nпо USB.", "For copying to Kindle\nby USB.", "通过 USB\n复制到 Kindle。");
            Add("Для отправки через\nAmazon Send to Kindle.", "For sending with\nAmazon Send to Kindle.", "通过 Amazon\nSend to Kindle 发送。");
            Add("Обложка, оглавление,\nиллюстрации и сноски.", "Cover, contents,\nillustrations and notes.", "保留封面、目录、\n插图和注释。");
            Add("Рядом с исходной книгой", "Beside the original book", "与原始图书保存在同一文件夹");
            Add("Папка каждой исходной книги", "Each original book's folder", "每本原始图书所在的文件夹");
            Add("Выбрать папку…", "Choose folder…", "选择文件夹…");
            Add("Выберите папку сохранения", "Choose an output folder", "请选择保存位置");
            Add("Конвертировать", "Convert", "转换");
            Add("Остановить", "Stop", "停止");
            Add("Папка результата", "Output folder", "打开输出文件夹");
            Add("Книги обрабатываются на вашем компьютере", "Books are processed on your computer", "图书在您的电脑上处理");
            Add("Язык интерфейса", "Interface language", "界面语言");
            Add("КНИГА", "BOOK", "图书");
            Add("РАЗМЕР", "SIZE", "大小");
            Add("СТАТУС", "STATUS", "状态");
            Add("{0} В ОЧЕРЕДИ", "{0} QUEUED", "队列中有 {0} 本");
            Add("МБ", "MB", "MB");
            Add("КБ", "KB", "KB");
            Add("Книги и архивы|*.fb2;*.zip;*.rar;*.7z|Книги FB2|*.fb2|Архивы ZIP|*.zip|Архивы RAR|*.rar|Архивы 7Z|*.7z", "Books and archives|*.fb2;*.zip;*.rar;*.7z|FB2 books|*.fb2|ZIP archives|*.zip|RAR archives|*.rar|7Z archives|*.7z", "图书和压缩包|*.fb2;*.zip;*.rar;*.7z|FB2 图书|*.fb2|ZIP 压缩包|*.zip|RAR 压缩包|*.rar|7Z 压缩包|*.7z");
            Add("Папка для готовых EPUB / AZW3", "Folder for converted EPUB / AZW3 books", "转换后的 EPUB / AZW3 图书保存位置");
            Add("Выберите книгу, чтобы увидеть путь и результат.", "Select a book to see its path and result.", "选择图书以查看路径和转换结果。");
            Add("Перетащите книги сюда или нажмите для выбора", "Drop books here or click to browse", "将图书拖到此处，或点击选择");
            Add("FB2 / ZIP / RAR / 7Z  ·  все книги из архива", "FB2 / ZIP / RAR / 7Z  ·  all books in each archive", "FB2 / ZIP / RAR / 7Z  ·  压缩包内的全部图书");
            Add("\r\nКнига в архиве: ", "\r\nBook in archive: ", "\r\n压缩包内的图书：");

            // Progress, results and queue errors.
            Add("Добавьте книги для конвертации", "Add books to convert", "请添加需要转换的图书");
            Add("Остановка после текущей операции…", "Stopping after the current operation…", "当前操作完成后停止…");
            Add("Остановка… Затем окно можно закрыть.", "Stopping… You can then close the window.", "正在停止…完成后即可关闭窗口。");
            Add("Поиск книг в архивах…", "Finding books in archives…", "正在查找压缩包中的图书…");
            Add("Добавление книг остановлено.", "Adding books was stopped.", "已停止添加图书。");
            Add("Не удалось добавить книги.", "Could not add books.", "无法添加图书。");
            Add("Файл не найден.", "File not found.", "找不到文件。");
            Add("Поддерживаются FB2, ZIP, RAR и 7Z.", "FB2, ZIP, RAR and 7Z are supported.", "支持 FB2、ZIP、RAR 和 7Z。");
            Add("В очередь можно добавить до 10000 книг за раз. Обработайте текущую очередь, затем очистите её.", "You can queue up to 10,000 books at a time. Process the current queue, then clear it.", "每次最多可添加 10,000 本图书。请先处理当前队列，再清空队列。");
            Add("Ожидает", "Waiting", "等待中");
            Add("Добавлено книг: {0}. Ошибок: {1}", "Books added: {0}. Errors: {1}", "已添加 {0} 本图书。错误：{1}");
            Add("Добавлено книг: {0}", "Books added: {0}", "已添加 {0} 本图书");
            Add("Папка сохранения больше не существует.", "The output folder no longer exists.", "保存文件夹已不存在。");
            Add("Для AZW3 нужна папка engine из полного архива.", "AZW3 requires the engine folder from the complete package.", "转换为 AZW3 需要完整程序包中的 engine 文件夹。");
            Add("Для AZW3 не найден необходимый компонент. Переустановите программу.", "A required AZW3 component was not found. Reinstall the application.", "找不到 AZW3 所需的组件。请重新安装程序。");
            Add("Обработка…", "Converting…", "正在转换…");
            Add("Книга {0} из {1}", "Book {0} of {1}", "正在处理第 {0} 本，共 {1} 本");
            Add("Готово · замечания", "Done · warnings", "完成 · 有提示");
            Add("Готово", "Done", "完成");
            Add("Остановлено", "Stopped", "已停止");
            Add("Конвертация остановлена.", "Conversion was stopped.", "转换已停止。");
            Add("Ошибка", "Error", "错误");
            Add("Остановлено. Готово: {0}. Ошибок: {1}", "Stopped. Done: {0}. Errors: {1}", "已停止。完成：{0}。错误：{1}");
            Add("Готово: {0}. Ошибок: {1}", "Done: {0}. Errors: {1}", "完成：{0}。错误：{1}");
            Add("Остановлено. Готово: {0}", "Stopped. Done: {0}", "已停止。完成：{0}");
            Add("Готово: {0}", "Done: {0}", "完成：{0}");
            Add("Не удалось открыть папку: {0}", "Could not open the folder: {0}", "无法打开文件夹：{0}");
            Add("Книги готовы к конвертации", "Books are ready to convert", "图书已准备好，可以开始转换");
            Add("Готовый файл: ", "Output file: ", "输出文件：");
            Add("Готовый файл: {0}", "Output file: {0}", "输出文件：{0}");
            Add("Книга в архиве: {0}", "Book in archive: {0}", "压缩包内的图书：{0}");

            // About window. Third-party legal documents stay in their original language.
            Add("О приложении", "About", "关于");
            Add("Конвертер FB2 и архивов ZIP, RAR, 7Z в EPUB и AZW3.\r\nКниги обрабатываются на вашем компьютере.", "Convert FB2 books and ZIP, RAR, 7Z archives to EPUB and AZW3.\r\nBooks are processed on your computer.", "将 FB2 图书和 ZIP、RAR、7Z 压缩包转换为 EPUB 或 AZW3。\r\n图书在您的电脑上处理。");
            Add("Профиль автора на GitHub", "Author's GitHub profile", "作者的 GitHub 主页");
            Add("Компоненты", "Components", "组件");
            Add("Calibre 9.15.0 — создание AZW3.\r\n7-Zip 26.00 — чтение архивов.\r\nInno Setup 7.1.0 — установочный пакет.\r\n\r\nДвижки Calibre и 7-Zip не изменены и запускаются отдельными процессами. Лицензии приведены на языке оригинала. Уведомления об авторстве и исходные архивы движков находятся в папке engine рядом с программой.", "Calibre 9.15.0 — AZW3 conversion.\r\n7-Zip 26.00 — archive reading.\r\nInno Setup 7.1.0 — installer.\r\n\r\nCalibre and 7-Zip are unmodified and run as separate processes. Licenses are shown in their original language. Copyright notices and source archives for the engines are in the engine folder next to the application.", "Calibre 9.15.0 — AZW3 转换。\r\n7-Zip 26.00 — 读取压缩包。\r\nInno Setup 7.1.0 — 安装程序。\r\n\r\nCalibre 和 7-Zip 均未经过修改，以独立进程运行。许可证以原文显示。引擎的版权声明和源代码压缩包位于程序旁的 engine 文件夹中。");
            Add("Calibre · авторство", "Calibre · copyright", "Calibre · 版权");
            Add("Закрыть", "Close", "关闭");
            Add("Файл лицензии не найден: ", "License file not found: ", "找不到许可证文件：");
            Add("Не удалось прочитать файл лицензии: ", "Could not read the license file: ", "无法读取许可证文件：");
            Add("Не удалось открыть ссылку: ", "Could not open the link: ", "无法打开链接：");

            // Archive readers and integrity checks.
            Add("ZIP-архив повреждён: каталог обрезан.", "The ZIP archive is damaged: its directory is truncated.", "ZIP 压缩包已损坏：目录不完整。");
            Add("ZIP-архив повреждён: неверная запись каталога.", "The ZIP archive is damaged: invalid directory entry.", "ZIP 压缩包已损坏：目录项无效。");
            Add("ZIP-архив повреждён: неверная структура каталога.", "The ZIP archive is damaged: invalid directory structure.", "ZIP 压缩包已损坏：目录结构无效。");
            Add("ZIP-архив повреждён: неверный список файлов.", "The ZIP archive is damaged: invalid file list.", "ZIP 压缩包已损坏：文件列表无效。");
            Add("ZIP-архив слишком большой: предел — 256 МБ.", "The ZIP archive is too large: the limit is 256 MB.", "ZIP 压缩包过大：上限为 256 MB。");
            Add("ZIP64-архив повреждён или состоит из нескольких томов.", "The ZIP64 archive is damaged or split into multiple volumes.", "ZIP64 压缩包已损坏，或由多个分卷组成。");
            Add("ZIP64-архив повреждён.", "The ZIP64 archive is damaged.", "ZIP64 压缩包已损坏。");
            Add("Архив защищён паролем. Распакуйте его с паролем и добавьте файлы FB2.", "The archive is password-protected. Extract it using the password, then add the FB2 files.", "压缩包受密码保护。请使用密码解压后，再添加 FB2 文件。");
            Add("Архив и временный файл должны иметь разные имена.", "The archive and temporary file must have different names.", "压缩包与临时文件的名称必须不同。");
            Add("Архив изменился после добавления. Удалите его из списка и добавьте заново.", "The archive changed after it was added. Remove it from the list and add it again.", "压缩包在添加后已发生变化。请从列表中移除后重新添加。");
            Add("Архив повреждён: неожиданный конец файла.", "The archive is damaged: unexpected end of file.", "压缩包已损坏：文件意外结束。");
            Add("Архив с книгами не найден.", "The book archive was not found.", "找不到图书压缩包。");
            Add("В ZIP-архиве не найден файл FB2.", "No FB2 file was found in the ZIP archive.", "ZIP 压缩包中没有 FB2 文件。");
            Add("В ZIP-архиве несколько книг FB2. Распакуйте архив и выберите одну книгу.", "The ZIP archive contains several FB2 books. Extract it and choose one book.", "ZIP 压缩包中有多本 FB2 图书。请解压后选择一本图书。");
            Add("В ZIP-архиве слишком много файлов.", "The ZIP archive contains too many files.", "ZIP 压缩包中的文件过多。");
            Add("В архиве есть имя файла, которое невозможно прочитать. Распакуйте архив вручную.", "The archive contains a filename that cannot be read. Extract the archive manually.", "压缩包中有无法读取的文件名。请手动解压。");
            Add("В архиве есть неоднозначное имя файла. Распакуйте архив вручную.", "The archive contains an ambiguous filename. Extract the archive manually.", "压缩包中有无法唯一识别的文件名。请手动解压。");
            Add("В архиве есть неоднозначное имя файла. Распакуйте эту книгу вручную.", "The archive contains an ambiguous filename. Extract this book manually.", "压缩包中有无法唯一识别的文件名。请手动解压这本图书。");
            Add("В архиве не найдено книг FB2.", "No FB2 books were found in the archive.", "压缩包中没有 FB2 图书。");
            Add("В архиве несколько книг с одинаковым путём. Распакуйте их и добавьте нужную книгу отдельно.", "Several books in the archive have the same path. Extract them and add the required book separately.", "压缩包中有多本图书使用相同路径。请先解压，再单独添加所需图书。");
            Add("В архиве несколько книг. Добавьте архив в окно программы: каждая книга будет сохранена отдельным файлом.", "The archive contains several books. Add it to the application window: each book will be saved as a separate file.", "压缩包中有多本图书。请将压缩包添加到程序窗口，每本图书都会保存为单独的文件。");
            Add("В архиве слишком много файлов: предел — 10 000.", "The archive contains too many files: the limit is 10,000.", "压缩包中的文件过多：上限为 10,000 个。");
            Add("В архиве указан неверный размер книги.", "The archive specifies an invalid book size.", "压缩包中的图书大小信息无效。");
            Add("Выберите архив ZIP, RAR или 7Z с книгами FB2.", "Choose a ZIP, RAR or 7Z archive containing FB2 books.", "请选择包含 FB2 图书的 ZIP、RAR 或 7Z 压缩包。");
            Add("Каталог архива слишком большой.", "The archive directory is too large.", "压缩包目录过大。");
            Add("Книга в ZIP-архиве повреждена: проверка целостности не пройдена.", "The book in the ZIP archive is damaged: the integrity check failed.", "ZIP 压缩包中的图书已损坏：完整性检查未通过。");
            Add("Книга в ZIP-архиве слишком большая: предел — 96 МБ.", "The book in the ZIP archive is too large: the limit is 96 MB.", "ZIP 压缩包中的图书过大：上限为 96 MB。");
            Add("Книга в архиве защищена паролем. Распакуйте её с паролем и добавьте файл FB2.", "The book in the archive is password-protected. Extract it using the password, then add the FB2 file.", "压缩包中的图书受密码保护。请使用密码解压后，再添加 FB2 文件。");
            Add("Книга в архиве повреждена: распакован неверный объём данных.", "The book in the archive is damaged: the extracted data size is incorrect.", "压缩包中的图书已损坏：解压后的数据大小不正确。");
            Add("Книга в архиве слишком большая: предел для FB2 — 96 МБ.", "The book in the archive is too large: the FB2 limit is 96 MB.", "压缩包中的图书过大：FB2 上限为 96 MB。");
            Add("Многотомный ZIP-архив не поддерживается. Соберите его в один архив.", "Split ZIP archives are not supported. Combine the volumes into a single archive.", "不支持分卷 ZIP 压缩包。请先合并为一个压缩包。");
            Add("Не выбран архив с книгами.", "No book archive was selected.", "未选择图书压缩包。");
            Add("Не выбран временный файл для книги.", "No temporary file was selected for the book.", "未指定图书的临时文件。");
            Add("Не найден модуль чтения RAR и 7Z. Распакуйте программу целиком вместе с папкой engine.", "The RAR and 7Z reader was not found. Extract the complete application, including its engine folder.", "找不到 RAR 和 7Z 读取组件。请完整解压程序，包括 engine 文件夹。");
            Add("Не удалось запустить модуль чтения RAR и 7Z. Распакуйте программу целиком вместе с папкой engine.", "Could not start the RAR and 7Z reader. Extract the complete application, including its engine folder.", "无法启动 RAR 和 7Z 读取组件。请完整解压程序，包括 engine 文件夹。");
            Add("Не удалось запустить модуль чтения архива.", "Could not start the archive reader.", "无法启动压缩包读取组件。");
            Add("Не удалось прочитать ZIP-архив. Проверьте, что он не повреждён и не защищён паролем.", "Could not read the ZIP archive. Check that it is not damaged or password-protected.", "无法读取 ZIP 压缩包。请确认它未损坏且未受密码保护。");
            Add("Не удалось прочитать архив. Он повреждён, имеет неподдерживаемый формат или состоит из нескольких томов.", "Could not read the archive. It is damaged, uses an unsupported format or is split into multiple volumes.", "无法读取压缩包。它可能已损坏、格式不受支持，或由多个分卷组成。");
            Add("Не удалось прочитать архив. Проверьте, что он не повреждён и не защищён паролем.", "Could not read the archive. Check that it is not damaged or password-protected.", "无法读取压缩包。请确认它未损坏且未受密码保护。");
            Add("Не удалось прочитать список файлов архива.", "Could not read the archive's file list.", "无法读取压缩包的文件列表。");
            Add("Содержимое архива изменилось. Добавьте архив заново.", "The archive contents changed. Add the archive again.", "压缩包内容已发生变化。请重新添加压缩包。");
            Add("Содержимое архива изменилось. Удалите его из списка и добавьте заново.", "The archive contents changed. Remove it from the list and add it again.", "压缩包内容已发生变化。请从列表中移除后重新添加。");
            Add("Чтение архива заняло слишком много времени. Распакуйте архив вручную.", "Reading the archive took too long. Extract it manually.", "读取压缩包耗时过长。请手动解压。");
            Add("Это не ZIP-архив или архив повреждён.", "This is not a ZIP archive, or the archive is damaged.", "这不是 ZIP 压缩包，或压缩包已损坏。");

            // FB2 parser, conversion and size limits.
            Add("В EPUB слишком много файлов.", "The EPUB contains too many files.", "EPUB 中的文件过多。");
            Add("В FB2 нет названия книги. Использовано имя файла.", "The FB2 has no book title. The filename was used.", "FB2 中没有图书标题，已使用文件名。");
            Add("В FB2 нет содержимого книги: элемент body отсутствует.", "The FB2 has no book content: the body element is missing.", "FB2 中没有图书内容：缺少 body 元素。");
            Add("В FB2 нет текста или изображений для конвертации.", "The FB2 has no text or images to convert.", "FB2 中没有可转换的文字或图像。");
            Add("В FB2 указана неподдерживаемая кодировка текста.", "The FB2 specifies an unsupported text encoding.", "FB2 指定了不支持的文本编码。");
            Add("В книге слишком много разделов: предел — 10 000.", "The book contains too many sections: the limit is 10,000.", "图书中的章节过多：上限为 10,000 个。");
            Add("В файле нет корневого элемента FictionBook. Это не книга FB2.", "The file has no FictionBook root element. It is not an FB2 book.", "文件缺少 FictionBook 根元素。这不是 FB2 图书。");
            Add("Выберите файл .fb2 или ZIP-архив с одной книгой FB2.", "Choose an .fb2 file or a ZIP archive containing one FB2 book.", "请选择 .fb2 文件或包含一本 FB2 图书的 ZIP 压缩包。");
            Add("Выберите формат EPUB или AZW3.", "Choose EPUB or AZW3 format.", "请选择 EPUB 或 AZW3 格式。");
            Add("Готовая книга превышает допустимый размер EPUB.", "The converted book exceeds the EPUB size limit.", "转换后的图书超过 EPUB 大小上限。");
            Add("Движок AZW3 не найден. Распакуйте полный архив программы вместе с папкой engine или установите Calibre.", "The AZW3 engine was not found. Extract the complete application, including its engine folder, or install Calibre.", "找不到 AZW3 转换引擎。请完整解压程序，包括 engine 文件夹，或安装 Calibre。");
            Add("Исходный файл и готовая книга должны иметь разные имена.", "The original file and converted book must have different names.", "原始文件与转换后的图书名称必须不同。");
            Add("Исходный файл не найден.", "The original file was not found.", "找不到原始文件。");
            Add("Книга слишком большая: предел для FB2 — 96 МБ.", "The book is too large: the FB2 limit is 96 MB.", "图书过大：FB2 上限为 96 MB。");
            Add("Не выбран исходный файл FB2.", "No original FB2 file was selected.", "未选择原始 FB2 文件。");
            Add("Не выбран путь для сохранения EPUB.", "No output path was selected for the EPUB.", "未选择 EPUB 保存路径。");
            Add("Не удалось проверить внутренние ссылки EPUB.", "Could not verify the EPUB's internal links.", "无法验证 EPUB 的内部链接。");
            Add("Не удалось прочитать FB2: некорректный XML или запрещённый DTD. Строка ", "Could not read the FB2: invalid XML or a disallowed DTD. Line ", "无法读取 FB2：XML 无效或包含不允许的 DTD。行号：");
            Add(", позиция ", ", position ", "，位置：");
            Add("Не удалось создать AZW3.\r\n", "Could not create the AZW3.\r\n", "无法创建 AZW3。\r\n");
            Add("Не удалось создать уникальные идентификаторы EPUB.", "Could not create unique EPUB identifiers.", "无法创建唯一的 EPUB 标识符。");
            Add("Некорректный код языка в FB2 заменён на «und».", "The invalid language code in the FB2 was replaced with “und”.", "FB2 中无效的语言代码已替换为“und”。");
            Add("Папка для сохранения не найдена.", "The output folder was not found.", "找不到保存文件夹。");
            Add("Папка сохранения не найдена.", "The output folder was not found.", "找不到保存文件夹。");
            Add("Слишком сложная структура FB2: превышен предел вложенности или числа элементов.", "The FB2 structure is too complex: the nesting or element count limit was exceeded.", "FB2 结构过于复杂：嵌套深度或元素数量超过上限。");
            Add("Создание AZW3 заняло больше пяти минут. Попробуйте EPUB или книгу меньшего размера.", "Creating the AZW3 took more than five minutes. Try EPUB or a smaller book.", "创建 AZW3 耗时超过五分钟。请尝试 EPUB 格式或更小的图书。");
            Add("Файл с таким именем уже существует. Выберите другое имя для EPUB.", "A file with this name already exists. Choose another name for the EPUB.", "同名文件已存在。请为 EPUB 选择其他名称。");
            Add("Файл уже существует: ", "The file already exists: ", "文件已存在：");

            // Image and markup warnings use fragments around an original FB2 identifier.
            Add("BMP-изображение «", "BMP image “", "BMP 图像“");
            Add("Внутренняя ссылка «", "Internal link “", "内部链接“");
            Add("Изображение «", "Image “", "图像“");
            Add("Не удалось преобразовать BMP-изображение «", "Could not convert BMP image “", "无法转换 BMP 图像“");
            Add("Неподдерживаемая ссылка «", "Unsupported link “", "不支持的链接“");
            Add("Повреждённое BMP-изображение «", "Damaged BMP image “", "已损坏的 BMP 图像“");
            Add("Повреждённое изображение «", "Damaged image “", "已损坏的图像“");
            Add("Повторяющееся изображение «", "Duplicate image “", "重复的图像“");
            Add("Повторяющийся идентификатор «", "Duplicate identifier “", "重复的标识符“");
            Add("Слишком большое встроенное изображение в FB2: «", "The embedded FB2 image is too large: “", "FB2 中的嵌入图像过大：“");
            Add("Формат изображения «", "Image format “", "图像格式“");
            Add("Элемент FB2 «", "FB2 element “", "FB2 元素“");
            Add("».", "”.", "”。");
            Add("» не найдена; её текст сохранён.", "” was not found; its text was preserved.", "”不存在，已保留其文本。");
            Add("» не найдено или не поддерживается.", "” was not found or is unsupported.", "”不存在或不受支持。");
            Add("» не поддерживается. Поддерживаются JPEG, PNG, GIF и BMP.", "” is unsupported. JPEG, PNG, GIF and BMP are supported.", "”不受支持。支持 JPEG、PNG、GIF 和 BMP。");
            Add("» преобразовано в PNG.", "” was converted to PNG.", "”已转换为 PNG。");
            Add("» пропущено.", "” was skipped.", "”已跳过。");
            Add("» слишком большое по разрешению и пропущено.", "” has too high a resolution and was skipped.", "”分辨率过高，已跳过。");
            Add("» слишком большое после преобразования и пропущено.", "” is too large after conversion and was skipped.", "”转换后过大，已跳过。");
            Add("» сохранена как текст.", "” was preserved as text.", "”已保留为文本。");
            Add("» сохранён как обычный текстовый блок.", "” was preserved as a plain text block.", "”已保留为普通文本块。");
            Add("»: ссылки ведут к первому элементу.", "”: links point to the first element.", "”：链接指向第一个元素。");
            Add("Встроенное изображение без идентификатора пропущено.", "An embedded image without an identifier was skipped.", "已跳过没有标识符的嵌入图像。");
            Add("Встроенное изображение превышает предел 24 МБ.", "An embedded image exceeds the 24 MB limit.", "嵌入图像超过 24 MB 上限。");
            Add("Изображение обложки не найдено или не поддерживается.", "The cover image was not found or is unsupported.", "封面图像不存在或不受支持。");
            Add("Общий размер изображений превышает предел 80 МБ.", "The total image size exceeds the 80 MB limit.", "图像总大小超过 80 MB 上限。");
            Add("[Изображение недоступно]", "[Image unavailable]", "[图像不可用]");

            // Generated book navigation and fallback labels.
            Add("Аннотация", "Description", "简介");
            Add("Обложка", "Cover", "封面");
            Add("Обложка: ", "Cover: ", "封面：");
            Add("Иллюстрация", "Illustration", "插图");
            Add("Навигация", "Navigation", "导航");
            Add("Начало книги", "Start of book", "正文开始");
            Add("Примечания", "Notes", "注释");
            Add("Содержание", "Contents", "目录");
            Add("Часть ", "Part ", "部分 ");
            Add("↩ К тексту", "↩ Back to text", "↩ 返回正文");
        }
    }
}
