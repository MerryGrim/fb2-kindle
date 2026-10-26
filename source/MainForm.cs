using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fb2Kindle
{
    public sealed class MainForm : MiraWindow
    {
        readonly MiraQueue queue = new MiraQueue();
        readonly Label brand = new Label(), subtitle = new Label(), heading = new Label(), count = new Label(), formats = new Label(), formatTitle = new Label(), saveTitle = new Label(), overallTitle = new Label(), overallPercent = new Label(), status = new Label();
        readonly MiraButton logo = new MiraButton(), theme = new MiraButton(), about = new MiraButton(), minimize = new MiraButton(), maximize = new MiraButton(), close = new MiraButton(), add = new MiraButton(), remove = new MiraButton(), clear = new MiraButton(), drop = new MiraButton(), together = new MiraButton(), beside = new MiraButton(), browse = new MiraButton(), open = new MiraButton(), convert = new MiraButton(), cancel = new MiraButton();
        readonly MiraFormatButton azw3 = new MiraFormatButton(), epub = new MiraFormatButton();
        readonly MiraButton language = new MiraButton();
        readonly MiraContextMenu languageMenu=MiraMenus.Create();
        readonly MiraPathPanel pathPanel = new MiraPathPanel();
        readonly TextBox outputFolder = new TextBox();
        readonly MiraSegmentPanel destinationModes = new MiraSegmentPanel();
        readonly MiraProgress overall = new MiraProgress();
        readonly ToolTip tips = new ToolTip {AutoPopDelay=20000};
        readonly Dictionary<Control,string> captions = new Dictionary<Control,string>();
        CancellationTokenSource cancellation;
        bool busy,preview,initializing=true;
        string lastFolder,lastResultDirectory,statusKey="Добавьте книги для конвертации";
        object[] statusArgs = new object[0];
        int completedCount;
        bool placementReady;
        Rectangle normalWindowBounds;
        FormWindowState lastVisibleWindowState=FormWindowState.Normal;
        public MainForm()
        {
            UiSettings.Load();MiraTheme.Dark=UiSettings.Theme=="dark"||(UiSettings.Theme=="system"&&MiraTheme.SystemDark());
            using(Graphics g=CreateGraphics())UiScale=g.DpiX/96f;
            Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.Manual;Text="FB2 Kindle";KeyPreview=true;
            try{Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);}catch{}
            Rectangle work=Screen.FromControl(this).WorkingArea;
            Size=new Size(Math.Min(S(960),work.Width-S(24)),Math.Min(S(820),work.Height-S(24)));MinimumSize=new Size(Math.Min(S(900),work.Width-S(24)),Math.Min(S(740),work.Height-S(24)));
            SetupLabel(brand,"FB2 Kindle",15,true);SetupLabel(subtitle,"Книги для вашей читалки",9,false);
            SetupLabel(heading,"Очередь книг",16,true);SetupLabel(count,"",9,false);SetupLabel(formats,"FB2 · ZIP · RAR · 7Z",9,false);
            SetupLabel(formatTitle,"Формат результата",10,true);SetupLabel(saveTitle,"Сохранение",10,true);
            SetupLabel(overallTitle,"",9,true);SetupLabel(overallPercent,"0%",9,false);SetupLabel(status,"",9,false);
            SetupButton(logo,"","book",false,true);logo.Selected=true;logo.TabStop=false;
            SetupButton(theme,"","theme",false,true);SetupButton(about,"","info",false,true);
            SetupButton(minimize,"","minus",false,true);SetupButton(maximize,"","square",false,true);SetupButton(close,"","x",false,true);
            SetupButton(add,"Добавить книги","plus",false,false);SetupButton(remove,"","trash",false,true);SetupButton(clear,"Очистить очередь",null,false,true);
            SetupButton(drop,"Перетащите сюда книги или архивы","upload",false,false);drop.Dashed=true;
            SetupButton(together,"В одну папку",null,false,false);SetupButton(beside,"Рядом с книгами",null,false,false);
            together.Segment=beside.Segment=true;
            SetupButton(browse,"","ellipsis",false,true);SetupButton(open,"Открыть","folder-open",false,false);
            SetupButton(convert,"Конвертировать","play",true,false);SetupButton(cancel,"Остановить","stop",false,false);
            azw3.Text="AZW3";epub.Text="EPUB";azw3.Font=epub.Font=new Font("Segoe UI Semibold",11);azw3.UiScale=epub.UiScale=UiScale;Controls.Add(azw3);Controls.Add(epub);
            Controls.Add(queue);queue.UiScale=UiScale;queue.SelectionChanged+=delegate{RefreshActions();};queue.DeleteRequested+=RemoveBooks;queue.DetailsRequested+=ShowBookDetails;
            outputFolder.BorderStyle=BorderStyle.None;outputFolder.Font=new Font("Segoe UI",10);outputFolder.Text=UiSettings.Folder;pathPanel.Controls.Add(outputFolder);pathPanel.Controls.Add(browse);Controls.Add(pathPanel);
            Controls.Add(destinationModes);destinationModes.Controls.Add(together);destinationModes.Controls.Add(beside);Controls.Add(overall);
            language.Font=new Font("Segoe UI",10);language.UiScale=UiScale;language.Quiet=true;Controls.Add(language);language.Click+=delegate{ShowLanguageMenu();};
            lastFolder=UiSettings.Folder;azw3.Selected=UiSettings.Format=="AZW3";epub.Selected=!azw3.Selected;beside.Selected=UiSettings.Beside;together.Selected=!beside.Selected;
            brand.MouseDown+=DragHeader;subtitle.MouseDown+=DragHeader;MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Y<S(CompactLayout?54:70))DragHeader(sender,e);};
            theme.Click+=delegate{SetTheme(!MiraTheme.Dark,true);};about.Click+=delegate{using(AboutForm f=new AboutForm())f.ShowDialog(this);};minimize.Click+=delegate{WindowState=FormWindowState.Minimized;};maximize.Click+=delegate{ToggleMaximize();};close.Click+=delegate{Close();};
            add.Click+=async delegate{await ChooseBooks();};drop.Click+=async delegate{if(!busy)await ChooseBooks();};remove.Click+=delegate{RemoveBooks();};clear.Click+=delegate{if(!busy){queue.Books.Clear();queue.Selected.Clear();queue.RefreshBooks();completedCount=0;SetStatus("Добавьте книги для конвертации");RefreshActions();RefreshProgress();}};
            azw3.Click+=delegate{SetFormat("AZW3");};epub.Click+=delegate{SetFormat("EPUB");};together.Click+=delegate{SetDestination(false);};beside.Click+=delegate{SetDestination(true);};browse.Click+=delegate{ChooseFolder();};open.Click+=delegate{OpenResultFolder();};
            outputFolder.TextChanged+=delegate{if(!initializing&&!beside.Selected){lastFolder=outputFolder.Text;UiSettings.Folder=lastFolder;if(!preview)UiSettings.Save();RefreshActions();}};
            convert.Click+=async delegate{await ConvertBooks();};cancel.Click+=delegate{RequestStop();};EnableDrop(this);
            KeyDown+=async delegate(object sender,KeyEventArgs e){if(e.Control&&e.KeyCode==Keys.O&&!busy){await ChooseBooks();e.Handled=true;}else if(e.KeyCode==Keys.F1){using(AboutForm f=new AboutForm())f.ShowDialog(this);e.Handled=true;}else if(e.KeyCode==Keys.Escape&&busy){RequestStop();e.Handled=true;}};
            FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;RequestStop();SetStatus("Остановка… Затем окно можно закрыть.");}};
            MiraTheme.Changed+=ApplyTheme;initializing=false;RefreshLanguage();ApplyTheme();UpdateDestinationText();RefreshActions();
        }
        int S(int value){return (int)Math.Round(value*UiScale);}
        bool CompactLayout{get{return ClientSize.Height<S(904);}}
        protected override void OnLoad(EventArgs e)
        {
            if(!preview)
            {
                WindowPlacement saved=UiSettings.Placement;
                Screen screen=saved==null?Screen.FromPoint(Cursor.Position):Screen.FromRectangle(saved.Bounds);
                Rectangle work=screen.WorkingArea;
                MinimumSize=new Size(Math.Min(S(900),work.Width-S(24)),Math.Min(S(740),work.Height-S(24)));
                Bounds=saved==null?WindowPlacement.Center(new Size(Math.Min(S(960),work.Width-S(24)),Math.Min(S(820),work.Height-S(24))),work):WindowPlacement.Resolve(saved.Bounds,saved.WorkingArea,work,MinimumSize);
                normalWindowBounds=Bounds;
                if(saved!=null&&saved.Maximized)WindowState=FormWindowState.Maximized;
                lastVisibleWindowState=WindowState;
                placementReady=true;
                UpdateWindowState();
            }
            base.OnLoad(e);
        }
        protected override void OnLocationChanged(EventArgs e){base.OnLocationChanged(e);RememberWindowBounds();}
        protected override void OnResize(EventArgs e){base.OnResize(e);RememberWindowBounds();UpdateWindowState();}
        void RememberWindowBounds()
        {
            if(!placementReady||preview||WindowState==FormWindowState.Minimized)return;
            lastVisibleWindowState=WindowState;
            if(WindowState==FormWindowState.Normal)normalWindowBounds=Bounds;
        }
        void UpdateWindowState()
        {
            if(maximize!=null){maximize.IconName=WindowState==FormWindowState.Maximized?"restore":"square";maximize.Invalidate();}
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if(e.Cancel||preview||!placementReady)return;
            RememberWindowBounds();
            UiSettings.Placement=new WindowPlacement{Bounds=normalWindowBounds,WorkingArea=Screen.FromRectangle(normalWindowBounds).WorkingArea,Maximized=lastVisibleWindowState==FormWindowState.Maximized};
            UiSettings.Folder=lastFolder??"";UiSettings.Save();
        }
        void SetupLabel(Label c,string text,float size,bool strong){c.Font=new Font(strong?"Segoe UI Semibold":"Segoe UI",size);c.TextAlign=ContentAlignment.MiddleLeft;c.AutoEllipsis=true;c.UseMnemonic=false;Controls.Add(c);if(text!="FB2 Kindle"&&!String.IsNullOrEmpty(text)&&text!="FB2 · ZIP · RAR · 7Z")captions[c]=text;c.Text=L10n.T(text);}
        void SetupButton(MiraButton c,string text,string icon,bool primary,bool quiet){c.Font=new Font("Segoe UI",10);c.UiScale=UiScale;c.IconName=icon;c.Primary=primary;c.Quiet=quiet;c.ForeColor=MiraTheme.Text;Controls.Add(c);if(!String.IsNullOrEmpty(text))captions[c]=text;c.Text=L10n.T(text);}
        void DragHeader(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left){if(e.Clicks==2)ToggleMaximize();else DragWindow();}}
        void ToggleMaximize(){WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;maximize.IconName=WindowState==FormWindowState.Maximized?"restore":"square";PerformLayout();Invalidate(true);}
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);if(brand==null||initializing)return;int w=ClientSize.Width,h=ClientSize.Height,p=S(24),inner=w-p*2;bool shortHeight=CompactLayout;
            logo.SetBounds(p,S(shortHeight?13:20),S(shortHeight?28:34),S(shortHeight?28:34));brand.SetBounds(p+S(47),S(shortHeight?7:15),S(350),S(28));subtitle.SetBounds(p+S(47),S(shortHeight?31:40),S(350),S(20));
            int chromeY=S(shortHeight?10:19),x=w-S(18);foreach(MiraButton b in new[]{close,maximize,minimize}){x-=S(36);b.SetBounds(x,chromeY,S(36),S(34));}x-=S(12);language.SetBounds(x-S(67),chromeY+S(3),S(62),S(28));x-=S(76);theme.SetBounds(x-S(34),chromeY,S(34),S(34));about.SetBounds(x-S(72),chromeY,S(34),S(34));
            int headingY=S(shortHeight?64:88),toolbarY=S(shortHeight?96:124),toolbarH=S(shortHeight?34:38);
            heading.SetBounds(p,headingY,S(245),S(29));int headingWidth=TextRenderer.MeasureText(heading.Text,heading.Font).Width;count.SetBounds(p+Math.Min(S(245),headingWidth)+S(10),headingY+S(3),S(120),S(24));
            add.SetBounds(p,toolbarY,S(166),toolbarH);remove.SetBounds(p+S(174),toolbarY,S(38),toolbarH);clear.SetBounds(p+S(220),toolbarY,S(170),toolbarH);formats.SetBounds(w-p-S(180),toolbarY,S(180),toolbarH);formats.TextAlign=ContentAlignment.MiddleRight;
            int settingsY=h-S(shortHeight?184:222),dropY=settingsY-S(shortHeight?40:48),queueY=S(shortHeight?142:174),queueH=Math.Min(S(672),Math.Max(1,dropY-S(shortHeight?6:12)-queueY));
            queue.SetBounds(p,queueY,inner,queueH);drop.SetBounds(p,dropY,inner,S(shortHeight?28:34));
            int colGap=S(22),col=(inner-colGap)/2,right=p+col+colGap;formatTitle.SetBounds(p,settingsY,col,S(24));saveTitle.SetBounds(right,settingsY,col,S(24));
            int controlsY=settingsY+S(shortHeight?27:31),cardGap=S(8);azw3.SetBounds(p,controlsY,(col-cardGap)/2,S(shortHeight?64:88));epub.SetBounds(p+(col-cardGap)/2+cardGap,controlsY,(col-cardGap)/2,S(shortHeight?64:88));
            destinationModes.SetBounds(right,controlsY,col,S(shortHeight?30:38));int modeWidth=(col-S(9))/2;together.SetBounds(S(3),S(3),modeWidth,S(shortHeight?24:32));beside.SetBounds(S(6)+modeWidth,S(3),modeWidth,S(shortHeight?24:32));
            int openW=S(105),pathW=col-openW-S(8),pathY=controlsY+S(shortHeight?36:48),pathH=S(shortHeight?28:40);pathPanel.SetBounds(right,pathY,pathW,pathH);outputFolder.SetBounds(S(37),S(shortHeight?5:11),Math.Max(S(30),pathW-S(76)),S(shortHeight?20:22));browse.SetBounds(pathW-S(34),S(shortHeight?2:5),S(29),S(shortHeight?24:30));open.SetBounds(right+pathW+S(8),pathY,openW,pathH);
            int foot=h-S(shortHeight?66:80),progressW=inner-S(342);overallTitle.SetBounds(p,foot+S(shortHeight?8:14),Math.Max(S(90),progressW-S(55)),S(21));overallPercent.SetBounds(p+progressW-S(50),foot+S(shortHeight?8:14),S(50),S(21));overallPercent.TextAlign=ContentAlignment.MiddleRight;overall.SetBounds(p,foot+S(shortHeight?32:40),progressW,S(shortHeight?5:6));status.SetBounds(p,foot+S(shortHeight?40:49),progressW,S(22));
            cancel.SetBounds(w-p-S(320),foot+S(shortHeight?13:20),S(128),S(shortHeight?38:40));convert.SetBounds(w-p-S(182),foot+S(shortHeight?13:20),S(182),S(shortHeight?38:40));queue.RefreshBooks();
        }
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);bool shortHeight=CompactLayout;int header=S(shortHeight?54:70),footer=S(shortHeight?66:80);using(SolidBrush b=new SolidBrush(MiraTheme.Surface)){e.Graphics.FillRectangle(b,0,0,Width,header);e.Graphics.FillRectangle(b,0,Height-footer,Width,footer);}using(Pen p=new Pen(MiraTheme.Border)){e.Graphics.DrawLine(p,0,header,Width,header);e.Graphics.DrawLine(p,0,Height-footer,Width,Height-footer);e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);}}
        void SetTheme(bool dark,bool save){MiraTheme.Set(dark);if(save&&!preview){UiSettings.Theme=dark?"dark":"light";UiSettings.Save();}}
        void ApplyTheme(){BackColor=MiraTheme.Background;ForeColor=MiraTheme.Text;foreach(Control c in Controls){c.BackColor=MiraTheme.Background;c.ForeColor=c==subtitle||c==count||c==formats||c==overallPercent||c==status?MiraTheme.Muted:MiraTheme.Text;}outputFolder.BackColor=MiraTheme.Surface;outputFolder.ForeColor=beside.Selected?MiraTheme.Muted:MiraTheme.Text;pathPanel.BackColor=MiraTheme.Background;destinationModes.BackColor=MiraTheme.Soft;language.BackColor=MiraTheme.Surface;language.ForeColor=MiraTheme.Text;overall.BackColor=MiraTheme.Surface;overallTitle.BackColor=overallPercent.BackColor=status.BackColor=MiraTheme.Surface;brand.BackColor=subtitle.BackColor=MiraTheme.Surface;foreach(MiraButton b in new[]{theme,about,minimize,maximize,close,logo,language})b.BackColor=MiraTheme.Surface;browse.BackColor=cancel.BackColor=convert.BackColor=MiraTheme.Surface;open.BackColor=MiraTheme.Background;together.BackColor=beside.BackColor=MiraTheme.Soft;Invalidate(true);}
        void SetFormat(string format){if(busy)return;azw3.Selected=format=="AZW3";epub.Selected=!azw3.Selected;azw3.Invalidate();epub.Invalidate();if(!preview){UiSettings.Format=format;UiSettings.Save();}}
        void SetDestination(bool value){if(busy)return;beside.Selected=value;together.Selected=!value;UpdateDestinationText();if(!preview){UiSettings.Beside=value;UiSettings.Save();}}
        void UpdateDestinationText(){initializing=true;outputFolder.ReadOnly=beside.Selected;outputFolder.Text=beside.Selected?L10n.T("Папка каждой исходной книги"):lastFolder??"";outputFolder.ForeColor=beside.Selected?MiraTheme.Muted:MiraTheme.Text;browse.Enabled=!busy&&!beside.Selected;beside.Invalidate();together.Invalidate();initializing=false;RefreshActions();}
        void RefreshLanguage(){language.Text=L10n.LanguageCode=="zh"?"简中":L10n.LanguageCode.ToUpperInvariant();foreach(KeyValuePair<Control,string> pair in captions)pair.Key.Text=L10n.T(pair.Value);azw3.Subtitle=L10n.T("Для переноса по USB");epub.Subtitle=L10n.T("Для Send to Kindle");queue.AccessibleName=L10n.T("Очередь книг");language.AccessibleName=L10n.T("Язык интерфейса");SetTip(about,"О приложении");SetTip(theme,"Сменить тему");SetTip(minimize,"Свернуть");SetTip(maximize,"Развернуть");SetTip(close,"Закрыть");SetTip(remove,"Убрать выбранные");SetTip(browse,"Выбрать папку…");SetTip(open,"Папка результата");outputFolder.AccessibleName=L10n.T("Папка результата");together.AccessibleName=L10n.T("В одну папку");beside.AccessibleName=L10n.T("Рядом с книгами");SetStatus(statusKey,statusArgs);UpdateDestinationText();RefreshProgress();RefreshActions();queue.RefreshBooks();PerformLayout();Invalidate(true);}
        void SetTip(Control c,string key){c.AccessibleName=L10n.T(key);tips.SetToolTip(c,L10n.T(key));}
        void SetStatus(string key,params object[] args){statusKey=key;statusArgs=args;status.Text=String.Format(L10n.T(key),args);tips.SetToolTip(status,status.Text);}
        void RefreshActions(){count.Text=L10n.BookCount(queue.Books.Count);add.Enabled=drop.Enabled=!busy;remove.Enabled=!busy&&queue.Selected.Count>0;clear.Enabled=!busy&&queue.Books.Count>0;convert.Enabled=!busy&&queue.Books.Count>0;cancel.Enabled=busy&&cancellation!=null&&!cancellation.IsCancellationRequested;language.Enabled=!busy;azw3.Enabled=epub.Enabled=!busy;together.Enabled=beside.Enabled=!busy;outputFolder.Enabled=!busy;browse.Enabled=!busy&&!beside.Selected;open.Enabled=!busy&&(!String.IsNullOrEmpty(lastResultDirectory)||(!beside.Selected&&Directory.Exists(lastFolder)));}
        void RefreshProgress(){int n=queue.Books.Count,percent=n==0?0:queue.Books.Sum(delegate(BookItem b){return b.Processed?100:b.Percent;})/n;int done=queue.Books.Count(delegate(BookItem b){return b.State==BookState.Done||b.State==BookState.Warning;});overallTitle.Text=n==0?L10n.T("Общий прогресс"):String.Format(L10n.T("Готово {0} из {1} книг"),done,n);overall.Percent=percent;overallPercent.Text=percent+"%";overall.AccessibleName=L10n.T("Общий прогресс")+" "+percent+"%";overall.Invalidate();}
        void EnableDrop(Control c){c.AllowDrop=true;c.DragEnter+=delegate(object s,DragEventArgs e){e.Effect=!busy&&e.Data.GetDataPresent(DataFormats.FileDrop)?DragDropEffects.Copy:DragDropEffects.None;};c.DragDrop+=async delegate(object s,DragEventArgs e){if(!busy&&e.Data.GetDataPresent(DataFormats.FileDrop))await AddBooksAsync((string[])e.Data.GetData(DataFormats.FileDrop));};foreach(Control child in c.Controls)EnableDrop(child);}
        async Task ChooseBooks(){if(busy)return;using(OpenFileDialog d=new OpenFileDialog()){d.Title=L10n.T("Добавить книги");d.Filter=L10n.T("Книги и архивы|*.fb2;*.zip;*.rar;*.7z|Книги FB2|*.fb2|Архивы ZIP|*.zip|Архивы RAR|*.rar|Архивы 7Z|*.7z");d.Multiselect=true;if(d.ShowDialog(this)==DialogResult.OK)await AddBooksAsync(d.FileNames);}}
        sealed class ScanResult {public List<BookItem> Books=new List<BookItem>();public List<string> Errors=new List<string>();}
        static ScanResult ScanBooks(string[] files,CancellationToken token){ScanResult result=new ScanResult();foreach(string raw in files){token.ThrowIfCancellationRequested();try{string path=Path.GetFullPath(raw),ext=Path.GetExtension(path).ToLowerInvariant();if(!File.Exists(path))throw new FileNotFoundException(L10n.T("Файл не найден."));if(ext==".fb2")result.Books.Add(new BookItem{Source=path,Size=new FileInfo(path).Length});else if(ext==".zip"||ext==".rar"||ext==".7z"){foreach(ArchiveBook entry in ArchiveBooks.List(path,token))result.Books.Add(new BookItem{Source=path,Entry=entry,Size=entry.Size});}else throw new InvalidDataException(L10n.T("Поддерживаются FB2, ZIP, RAR и 7Z."));}catch(OperationCanceledException){throw;}catch(Exception error){result.Errors.Add(raw+"\r\n"+error.Message);}}return result;}
        public void AddBooks(string[] files){if(!busy)AddScannedBooks(ScanBooks(files,CancellationToken.None));}
        public async Task AddBooksAsync(string[] files){if(busy)return;cancellation=new CancellationTokenSource();busy=true;RefreshActions();SetStatus("Поиск книг в архивах…");try{ScanResult result=await Task.Run(delegate{return ScanBooks(files,cancellation.Token);});cancellation.Token.ThrowIfCancellationRequested();AddScannedBooks(result);}catch(OperationCanceledException){SetStatus("Добавление книг остановлено.");}catch(Exception error){SetStatus("Не удалось добавить книги.");ShowMessage(L10n.T("Не удалось добавить книги."),error.Message);}finally{busy=false;cancellation.Dispose();cancellation=null;RefreshActions();}}
        void AddScannedBooks(ScanResult result){int added=0;foreach(BookItem b in result.Books){if(queue.Books.Exists(delegate(BookItem item){return String.Equals(item.Source,b.Source,StringComparison.OrdinalIgnoreCase)&&((item.Entry==null&&b.Entry==null)||(item.Entry!=null&&b.Entry!=null&&item.Entry.Index==b.Entry.Index));}))continue;if(queue.Books.Count>=10000){result.Errors.Add(L10n.T("В очередь можно добавить до 10000 книг за раз. Обработайте текущую очередь, затем очистите её."));break;}queue.Books.Add(b);added++;}queue.RefreshBooks();RefreshProgress();RefreshActions();SetStatus(result.Errors.Count>0?"Добавлено книг: {0}. Ошибок: {1}":"Добавлено книг: {0}",added,result.Errors.Count);if(result.Errors.Count>0)ShowMessage(L10n.T("Не удалось добавить книги."),String.Join("\r\n\r\n",result.Errors));}
        void RemoveBooks(){if(busy)return;List<int> indices=queue.Selected.ToList();indices.Sort();indices.Reverse();foreach(int index in indices)if(index<queue.Books.Count)queue.Books.RemoveAt(index);queue.Selected.Clear();queue.RefreshBooks();RefreshActions();RefreshProgress();}
        void ChooseFolder(){if(busy)return;using(FolderBrowserDialog d=new FolderBrowserDialog()){d.Description=L10n.T("Папка для готовых EPUB / AZW3");d.ShowNewFolderButton=true;if(Directory.Exists(lastFolder))d.SelectedPath=lastFolder;if(d.ShowDialog(this)==DialogResult.OK){lastFolder=d.SelectedPath;SetDestination(false);UiSettings.Folder=lastFolder;if(!preview)UiSettings.Save();}}}
        void RequestStop(){if(cancellation==null)return;cancellation.Cancel();SetStatus("Остановка после текущей операции…");RefreshActions();}
        string StageKey(ConversionStage stage){switch(stage){case ConversionStage.Extracting:return "Распаковка книги…";case ConversionStage.BuildingEpub:return "Создание EPUB…";case ConversionStage.ConvertingAzw3:return "Создание AZW3…";case ConversionStage.Saving:return "Сохранение файла…";default:return "Чтение FB2…";}}
        public async Task ConvertBooks()
        {
            if(busy||queue.Books.Count==0)return;string directory=beside.Selected?null:lastFolder;
            if(!beside.Selected&&String.IsNullOrWhiteSpace(directory)){ChooseFolder();if(beside.Selected||String.IsNullOrWhiteSpace(lastFolder))return;directory=lastFolder;}
            if(directory!=null&&!Directory.Exists(directory)){SetStatus("Папка сохранения больше не существует.");return;}
            string extension=azw3.Selected?".azw3":".epub";if(extension==".azw3"&&KindleConverter.FindCalibre()==null){SetStatus("Для AZW3 не найден необходимый компонент. Переустановите программу.");return;}
            cancellation=new CancellationTokenSource();busy=true;completedCount=0;RefreshActions();int good=0,failed=0;bool stopped=false;
            foreach(BookItem b in queue.Books){b.Percent=0;b.Processed=false;b.State=BookState.Waiting;b.StageKey="Ожидает своей очереди";}RefreshProgress();queue.Invalidate();
            try
            {
                foreach(BookItem book in queue.Books)
                {
                    if(cancellation.IsCancellationRequested){stopped=true;break;}
                    book.Output=null;book.Detail=book.SourceDescription;book.State=BookState.Working;book.StageKey="Чтение FB2…";queue.EnsureBookVisible(queue.Books.IndexOf(book));queue.Invalidate();SetStatus("Книга {0} из {1}",completedCount+1,queue.Books.Count);
                    string target=directory??Path.GetDirectoryName(book.Source);
                    try
                    {
                        string output=KindleConverter.AvailableOutput(book.Entry==null?book.Source:book.FileName,target,extension);
                        Action<ConversionProgress> report=delegate(ConversionProgress p){if(IsDisposed||!IsHandleCreated)return;try{BeginInvoke((Action)delegate{if(!busy||book.State!=BookState.Working)return;book.Percent=Math.Max(book.Percent,Math.Min(99,p.Percent));book.StageKey=StageKey(p.Stage);queue.Invalidate();RefreshProgress();});}catch(InvalidOperationException){} };
                        ConversionResult result=await Task.Run(delegate{return KindleConverter.Convert(book.Source,book.Entry,output,cancellation.Token,report);});
                        book.Output=output;book.Title=result.Title;book.Author=result.Author;book.ResultFormat=extension.Substring(1).ToUpperInvariant();book.Percent=100;book.Processed=true;book.State=result.Warnings.Count>0?BookState.Warning:BookState.Done;book.Detail=result.Title+(String.IsNullOrEmpty(result.Author)?"":" · "+result.Author)+"\r\n"+output+(result.Warnings.Count==0?"":"\r\n"+String.Join("\r\n",result.Warnings));lastResultDirectory=target;good++;
                    }
                    catch(OperationCanceledException){book.State=BookState.Stopped;book.StageKey="Остановлено";book.Detail=L10n.T("Конвертация остановлена.")+"\r\n"+book.SourceDescription;stopped=true;break;}
                    catch(Exception error){book.State=BookState.Error;book.StageKey="Ошибка";book.Processed=true;book.Detail=error.Message+"\r\n"+book.SourceDescription;failed++;}
                    completedCount++;RefreshProgress();queue.Invalidate();
                }
                if(failed>0)SetStatus(stopped?"Остановлено. Готово: {0}. Ошибок: {1}":"Готово: {0}. Ошибок: {1}",good,failed);else SetStatus(stopped?"Остановлено. Готово: {0}":"Готово: {0}",good);
            }
            finally{busy=false;cancellation.Dispose();cancellation=null;RefreshProgress();RefreshActions();queue.Invalidate();}
        }
        void OpenResultFolder(){string folder=!beside.Selected&&Directory.Exists(lastFolder)?lastFolder:lastResultDirectory;BookItem selected=queue.FirstSelected;if(selected!=null&&selected.Output!=null)folder=Path.GetDirectoryName(selected.Output);if(folder==null)return;try{Process.Start(new ProcessStartInfo(folder){UseShellExecute=true});}catch(Exception e){SetStatus("Не удалось открыть папку: {0}",e.Message);}}
        void ShowBookDetails(){BookItem b=queue.FirstSelected;if(b!=null)ShowMessage(b.FileName,b.AllDetails);}
        void ShowMessage(string title,string text){using(MiraMessage f=new MiraMessage(title,text))f.ShowDialog(this);}
        public void Preview(){Preview("ru",MiraTheme.Dark?"dark":"light",4);}
        public void Preview(string lang,string themeName,int number)
        {
            preview=true;L10n.SetLanguage(lang);MiraTheme.Dark=themeName=="dark";lastFolder=@"D:\Books\Kindle";beside.Selected=false;together.Selected=true;azw3.Selected=true;epub.Selected=false;
            queue.Books.Clear();string[] names=lang=="ru"?new[]{"Путешествие к северному маяку.fb2","Сборник историй.fb2","Двадцать тысяч лье под водой.fb2","Затерянный мир.fb2"}:lang=="zh"?new[]{"北方灯塔之旅.fb2","短篇故事集.fb2","海底两万里.fb2","失落的世界.fb2"}:new[]{"Journey to the northern lighthouse.fb2","Collected stories.fb2","Twenty Thousand Leagues Under the Seas.fb2","The Lost World.fb2"};
            for(int i=0;i<number;i++){BookItem b=new BookItem{Source=@"D:\Books\"+(i<names.Length?names[i]:"Book "+(i+1)+".fb2"),Size=862208+i*234567,Author=i==0?(lang=="ru"?"Алексей Северный":lang=="zh"?"阿列克谢·谢韦尔尼":"Alexey Severny"):"",State=i==0?BookState.Done:i==1?BookState.Working:BookState.Waiting,Percent=i==0?100:i==1?64:0,Processed=i==0,ResultFormat="AZW3",StageKey=i==1?"Создание AZW3…":"Ожидает своей очереди"};queue.Books.Add(b);}
            if(number<7)ClientSize=new Size(ClientSize.Width,Math.Min(ClientSize.Height,S(454+Math.Max(1,number)*96)));
            RefreshLanguage();SetStatus("Книга {0} из {1}",2,number);ApplyTheme();RefreshProgress();queue.RefreshBooks();RefreshActions();
        }
        public string VerifyUi()
        {
            PerformLayout();List<BookItem> old=new List<BookItem>(queue.Books);List<string> checks=new List<string>();
            if(MiraWindow.DecodeHitPoint(new IntPtr(0x00000000FFECFFF0L))!=new Point(-16,-20))throw new InvalidOperationException("Negative monitor coordinates did not decode");checks.Add("negativeMonitorCoordinates=PASS");
            queue.Books.Clear();queue.Books.Add(new BookItem{Source="Done.fb2",Percent=100,Processed=true,State=BookState.Done});queue.Books.Add(new BookItem{Source="Working.fb2",Percent=99,State=BookState.Working});RefreshProgress();if(overall.Percent!=99)throw new InvalidOperationException("Overall progress reached 100 before completion");checks.Add("pending99Overall=99");queue.Books[1].Processed=true;queue.Books[1].State=BookState.Done;queue.Books[1].Percent=100;RefreshProgress();if(overall.Percent!=100)throw new InvalidOperationException("Completed overall progress is not 100");checks.Add("completeOverall=100");
            foreach(int n in new[]{7,8,40}){queue.Books.Clear();for(int i=0;i<n;i++)queue.Books.Add(new BookItem{Source="Book "+i+".fb2"});queue.AutoScrollPosition=Point.Empty;queue.RefreshBooks();bool needs=queue.VerticalScroll.Visible;checks.Add("queue"+n+".scroll="+needs);if(n==7&&queue.Height>=S(448)&&needs)throw new InvalidOperationException("Seven rows unexpectedly scroll");if(n>7&&!needs)throw new InvalidOperationException("Large queue must scroll");if(n==40){queue.EnsureBookVisible(8);if(queue.AutoScrollPosition.Y>=0)throw new InvalidOperationException("Active book did not scroll");checks.Add("autoScroll="+queue.AutoScrollPosition.Y);}}
            Rectangle headerBounds=brand.Bounds,settingsBounds=azw3.Bounds,footerBounds=convert.Bounds;checks.Add(queue.VerifyNavigation());if(headerBounds!=brand.Bounds||settingsBounds!=azw3.Bounds||footerBounds!=convert.Bounds)throw new InvalidOperationException("Scrolling moved fixed UI");checks.Add("fixedHeaderSettingsFooter=PASS");
            queue.Books.Clear();queue.Books.AddRange(old);queue.Selected.Clear();queue.AutoScrollPosition=Point.Empty;queue.RefreshBooks();if(azw3.Bottom!=pathPanel.Bottom||azw3.Top!=destinationModes.Top)throw new InvalidOperationException("Settings groups are not aligned");
            foreach(Control c in new Control[]{queue,drop,azw3,epub,pathPanel,open,convert,cancel,language})if(c.Left<0||c.Right>ClientSize.Width||c.Bottom>ClientSize.Height)throw new InvalidOperationException("Clipped control: "+c.GetType().Name);
            VerifyNoOverlap();checks.Add("pairwiseNoOverlap=PASS");checks.Add(VerifyShortHeights());checks.Add(VerifyMenus());RefreshProgress();RefreshActions();checks.Add("alignment=PASS");checks.Add("bounds=PASS");checks.Add("language="+L10n.LanguageCode);checks.Add("theme="+(MiraTheme.Dark?"dark":"light"));return String.Join("\r\n",checks);
        }
        void VerifyNoOverlap()
        {
            Control[] controls={heading,add,queue,drop,formatTitle,saveTitle,azw3,epub,destinationModes,pathPanel,open,overallTitle,overallPercent,overall,status,cancel,convert};
            for(int i=0;i<controls.Length;i++)for(int j=i+1;j<controls.Length;j++)if(controls[i].Bounds.IntersectsWith(controls[j].Bounds))throw new InvalidOperationException("Overlapping controls: "+controls[i].GetType().Name+" / "+controls[j].GetType().Name);
            if(outputFolder.Bottom>pathPanel.ClientSize.Height||browse.Bottom>pathPanel.ClientSize.Height)throw new InvalidOperationException("Output controls are clipped");
        }
        string VerifyShortHeights()
        {
            float previousScale=UiScale;Size previousSize=ClientSize,previousMinimum=MinimumSize;List<string> checks=new List<string>();
            try
            {
                MinimumSize=Size.Empty;
                foreach(float scale in new[]{1.25f,1.5f})
                {
                    UiScale=scale;queue.UiScale=scale;foreach(Control control in Controls){MiraButton button=control as MiraButton;if(button!=null)button.UiScale=scale;}together.UiScale=beside.UiScale=browse.UiScale=scale;
                    ClientSize=new Size(S(1040),698);PerformLayout();VerifyNoOverlap();if(queue.Height<S(64))throw new InvalidOperationException("Short layout cannot show a complete book row");
                    foreach(Control c in new Control[]{heading,formatTitle,saveTitle,overallTitle,status,together,beside,convert,cancel})if(c.Height<(int)Math.Ceiling(c.Font.Height*scale/previousScale))throw new InvalidOperationException("Short layout clips a caption: "+c.Text);
                    checks.Add("shortHeight698.scale"+scale.ToString(System.Globalization.CultureInfo.InvariantCulture)+"=PASS;queueHeight="+queue.Height);
                }
            }
            finally{UiScale=previousScale;queue.UiScale=previousScale;foreach(Control control in Controls){MiraButton button=control as MiraButton;if(button!=null)button.UiScale=previousScale;}together.UiScale=beside.UiScale=browse.UiScale=previousScale;ClientSize=previousSize;MinimumSize=previousMinimum;PerformLayout();}
            return String.Join("\r\n",checks);
        }
        string VerifyMenus()
        {
            string previousLanguage=L10n.LanguageCode;Exception threadError=null;ThreadExceptionEventHandler handler=delegate(object sender,ThreadExceptionEventArgs args){threadError=args.Exception;};Application.ThreadException+=handler;
            try
            {
                ShowLanguageMenu();Application.DoEvents();((ToolStripMenuItem)languageMenu.Items[1]).PerformClick();languageMenu.Close();Application.DoEvents();if(L10n.LanguageCode!="en"||languageMenu.IsDisposed)throw new InvalidOperationException("English menu selection failed");
                ShowLanguageMenu();Application.DoEvents();languageMenu.Items[2].Select();languageMenu.VerifyEnter();Application.DoEvents();if(L10n.LanguageCode!="zh"||languageMenu.IsDisposed)throw new InvalidOperationException("Language menu Enter selection failed");
                using(AboutForm dialog=new AboutForm()){dialog.StartPosition=FormStartPosition.Manual;dialog.Location=new Point(-30000,-30000);dialog.Show(this);Application.DoEvents();dialog.VerifyDocumentMenu();dialog.Close();}
                if(threadError!=null)throw new InvalidOperationException("Menu interaction raised an application-thread exception",threadError);
            }
            finally{languageMenu.Close();L10n.SetLanguage(previousLanguage);RefreshLanguage();Application.ThreadException-=handler;}
            return "languageMenuPerformClick=PASS\r\nlanguageMenuEnter=PASS\r\nlicenseMenuPerformClickEnter=PASS";
        }
        protected override void Dispose(bool disposing){if(disposing){MiraTheme.Changed-=ApplyTheme;tips.Dispose();languageMenu.Dispose();if(cancellation!=null)cancellation.Cancel();}base.Dispose(disposing);}
        void ShowLanguageMenu(){MiraContextMenu menu=languageMenu;menu.Items.Clear();menu.BackColor=MiraTheme.Surface;menu.ForeColor=MiraTheme.Text;string[] codes={"ru","en","zh"},names={"Русский","English","简体中文"};for(int i=0;i<codes.Length;i++){string code=codes[i];ToolStripMenuItem item=new ToolStripMenuItem(names[i]){Checked=code==L10n.LanguageCode,ForeColor=MiraTheme.Text};item.Click+=delegate{L10n.SetLanguage(code);RefreshLanguage();};menu.Items.Add(item);}menu.Show(language,new Point(0,language.Height));}
    }
}
