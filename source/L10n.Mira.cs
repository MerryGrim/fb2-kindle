namespace Fb2Kindle
{
    static partial class L10n
    {
        public static string BookCount(int count){if(LanguageCode!="ru")return System.String.Format(T("{0} книг"),count);int unit=count%10,ten=count%100;return count+" "+(unit==1&&ten!=11?"книга":unit>=2&&unit<=4&&(ten<12||ten>14)?"книги":"книг");}
        static void Mira(string ru,string en,string zh){if(!Translations.ContainsKey(ru))Add(ru,en,zh);}
        static void InitializeMiraTranslations()
        {
            Mira("Книги для вашей читалки","Books for your reader","为您的阅读器准备图书");
            Mira("Очередь книг","Book queue","图书队列");
            Mira("Очистить очередь","Clear queue","清空队列");
            Mira("Перетащите сюда книги или архивы","Drop books or archives here","将图书或压缩包拖到此处");
            Mira("Формат результата","Output format","输出格式");
            Mira("Для переноса по USB","For USB transfer","通过 USB 传输");
            Mira("Для Send to Kindle","For Send to Kindle","用于 Send to Kindle");
            Mira("Сохранение","Save location","保存位置");
            Mira("В одну папку","One folder","统一文件夹");
            Mira("Рядом с книгами","Beside the books","与图书同目录");
            Mira("Открыть","Open","打开");
            Mira("Сменить тему","Switch theme","切换主题");
            Mira("Свернуть","Minimize","最小化");
            Mira("Развернуть","Maximize","最大化");
            Mira("{0} книг","{0} books","{0} 本图书");
            Mira("Общий прогресс","Overall progress","总体进度");
            Mira("Готово {0} из {1} книг","Completed {0} of {1} books","已完成 {0} 本，共 {1} 本");
            Mira("Отдельный файл","Individual file","独立文件");
            Mira("Ожидает своей очереди","Waiting in the queue","等待处理");
            Mira("Добавьте книги или архивы","Add books or archives","添加图书或压缩包");
            Mira("Распаковка книги…","Extracting the book…","正在解压图书…");
            Mira("Создание EPUB…","Creating EPUB…","正在创建 EPUB…");
            Mira("Создание AZW3…","Creating AZW3…","正在创建 AZW3…");
            Mira("Сохранение файла…","Saving the file…","正在保存文件…");
            Mira("Чтение FB2…","Reading FB2…","正在读取 FB2…");
            Mira("Версия 1.3.0 · MiraUI","Version 1.3.0 · MiraUI","版本 1.3.0 · MiraUI");
            Mira("Лицензии и компоненты","Licenses and components","许可证与组件");
            Mira("Calibre и 7-Zip работают отдельными процессами и сохраняют свои лицензии. Версии и соответствующие исходные архивы указаны в THIRD-PARTY-NOTICES.md и папке engine.","Calibre and 7-Zip run as separate processes and retain their own licenses. Versions and matching source archives are listed in THIRD-PARTY-NOTICES.md and the engine folder.","Calibre 和 7-Zip 作为独立进程运行，保留各自的许可证。版本及对应源代码压缩包见 THIRD-PARTY-NOTICES.md 和 engine 文件夹。");
            Mira("Продолжить","Continue","继续");
            Mira("Выбранные книги","Selected books","所选图书");
        }
    }
}
