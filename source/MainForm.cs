using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fb2Kindle
{
    public class MainForm : Form
    {
        static readonly Color Blue = ColorTranslator.FromHtml("#155EEF");
        static readonly Color Ink = ColorTranslator.FromHtml("#182230");
        static readonly Color Muted = ColorTranslator.FromHtml("#667085");
        static readonly Color Line = ColorTranslator.FromHtml("#E4E7EC");
        static readonly Color Pale = ColorTranslator.FromHtml("#F7F8FA");
        readonly List<BookItem> books = new List<BookItem>();
        readonly DataGridView grid = new DataGridView();
        readonly TextBox details = new TextBox();
        readonly TextBox outputFolder = new TextBox();
        readonly CheckBox besideSource = new CheckBox();
        readonly RadioButton azw3 = new RadioButton();
        readonly RadioButton epub = new RadioButton();
        readonly Label queueCount = new Label();
        readonly Label status = new Label();
        readonly Label formatHint = new Label();
        readonly ComboBox languagePicker = new ComboBox();
        readonly ToolTip toolTips = new ToolTip();
        readonly Dictionary<Control, string> captions = new Dictionary<Control, string>();
        readonly Button about;
        readonly Button convert;
        readonly Button cancel;
        readonly Button openFolder;
        readonly Button add;
        readonly Button remove;
        readonly Button clear;
        readonly Button browse;
        readonly ProgressBar progress = new ProgressBar();
        readonly DropPanel drop = new DropPanel();
        CancellationTokenSource cancellation;
        bool busy;
        string lastFolder;
        string statusTemplate = "Добавьте книги для конвертации";
        object[] statusArguments = new object[0];

        public MainForm()
        {
            Text = "FB2 Kindle";
            ClientSize = new Size(1040, 730);
            MinimumSize = new Size(900, 715);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            ForeColor = Ink;
            BackColor = Color.White;
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            Icon = MakeIcon();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(28, 22, 28, 18);
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowCount = 7;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 89));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 81));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            Controls.Add(layout);

            TableLayoutPanel header = Table(2);
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            Panel wordmark = new Panel { Dock = DockStyle.Fill };
            wordmark.Controls.Add(LabelAt("FB2  /  KINDLE", new Font("Segoe UI Semibold", 25), Ink, 0, 0, 500, 45));
            wordmark.Controls.Add(LabelAt("Конвертер электронных книг", Font, Muted, 2, 48, 480, 25));
            header.Controls.Add(wordmark, 0, 0);
            FlowLayoutPanel preferences = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 8, 0, 0), Margin = Padding.Empty };
            about = Button("?", false, 35); about.Height = 32; about.Margin = new Padding(8, 0, 0, 0);
            about.Click += delegate { using (AboutForm dialog = new AboutForm()) dialog.ShowDialog(this); };
            languagePicker.DropDownStyle = ComboBoxStyle.DropDownList; languagePicker.Width = 145;
            languagePicker.Font = new Font("Segoe UI", 9); languagePicker.Margin = new Padding(0, 3, 0, 0);
            languagePicker.Items.Add(new LanguageChoice("ru", "Русский"));
            languagePicker.Items.Add(new LanguageChoice("en", "English"));
            languagePicker.Items.Add(new LanguageChoice("zh", "简体中文"));
            foreach (LanguageChoice choice in languagePicker.Items) if (choice.Code == L10n.LanguageCode) languagePicker.SelectedItem = choice;
            if (languagePicker.SelectedIndex < 0) languagePicker.SelectedIndex = 0;
            languagePicker.SelectedIndexChanged += delegate
            {
                LanguageChoice choice = languagePicker.SelectedItem as LanguageChoice;
                if (choice == null || choice.Code == L10n.LanguageCode) return;
                L10n.SetLanguage(choice.Code); RefreshLanguage();
            };
            preferences.Controls.Add(about); preferences.Controls.Add(languagePicker);
            header.Controls.Add(preferences, 1, 0);
            layout.Controls.Add(header, 0, 0);

            TableLayoutPanel toolbar = Table(2);
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            FlowLayoutPanel tools = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
            add = Button("Добавить книги", true, 155);
            remove = Button("Убрать выбранные", false, 169);
            clear = Button("Очистить", false, 104);
            tools.Controls.Add(add); tools.Controls.Add(remove); tools.Controls.Add(clear);
            add.Click += delegate { ChooseBooks(); };
            remove.Click += delegate { RemoveBooks(); };
            clear.Click += delegate { books.Clear(); grid.Rows.Clear(); RefreshCount(); details.Text = L10n.T("Выберите книги FB2 или архивы ZIP, RAR, 7Z."); };
            toolbar.Controls.Add(tools, 0, 0);
            queueCount.TextAlign = ContentAlignment.MiddleRight; queueCount.ForeColor = Muted; queueCount.Dock = DockStyle.Fill;
            toolbar.Controls.Add(queueCount, 1, 0);
            layout.Controls.Add(toolbar, 0, 1);

            drop.Dock = DockStyle.Fill; drop.Margin = new Padding(0, 0, 0, 13); drop.Cursor = Cursors.Hand;
            drop.Click += delegate { if (!busy) ChooseBooks(); };
            layout.Controls.Add(drop, 0, 2);

            TableLayoutPanel content = Table(2);
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 275));
            TableLayoutPanel fileArea = Table(1);
            fileArea.RowCount = 2;
            fileArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            fileArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 77));
            fileArea.Margin = new Padding(0, 0, 20, 0);
            ConfigureGrid();
            fileArea.Controls.Add(grid, 0, 0);
            details.Dock = DockStyle.Fill; details.Multiline = true; details.ReadOnly = true; details.BorderStyle = BorderStyle.None;
            details.BackColor = Pale; details.ForeColor = Muted; details.Font = new Font("Segoe UI", 9);
            details.ScrollBars = ScrollBars.Vertical;
            details.Text = L10n.T("Выберите книги FB2 или архивы ZIP, RAR, 7Z.");
            Panel detailPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 9, 8, 8), BackColor = Pale, Margin = new Padding(0, 12, 0, 0) };
            detailPanel.Controls.Add(details); fileArea.Controls.Add(detailPanel, 0, 1);
            content.Controls.Add(fileArea, 0, 0);

            BorderedPanel formatPanel = new BorderedPanel { Dock = DockStyle.Fill, Padding = new Padding(17, 16, 15, 10), BackColor = Pale };
            TableLayoutPanel formatLayout = Table(1);
            formatLayout.RowCount = 6;
            foreach (int height in new int[] { 32, 28, 45, 28, 46 }) formatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            formatLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Label formatTitle = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 10), ForeColor = Ink }; BindText(formatTitle, "ФОРМАТ ФАЙЛА");
            azw3.Text = "AZW3"; azw3.Checked = true; azw3.Dock = DockStyle.Fill; azw3.Font = new Font("Segoe UI Semibold", 12); azw3.AutoSize = true;
            epub.Text = "EPUB"; epub.Dock = DockStyle.Fill; epub.Font = azw3.Font; epub.AutoSize = true;
            formatLayout.Controls.Add(formatTitle, 0, 0); formatLayout.Controls.Add(azw3, 0, 1);
            formatLayout.Controls.Add(Description("Для переноса на Kindle\nпо USB."), 0, 2);
            formatLayout.Controls.Add(epub, 0, 3);
            formatLayout.Controls.Add(Description("Для отправки через\nAmazon Send to Kindle."), 0, 4);
            BindText(formatHint, "Обложка, оглавление,\nиллюстрации и сноски."); formatHint.ForeColor = Muted; formatHint.Font = new Font("Segoe UI", 9); formatHint.Dock = DockStyle.Fill;
            formatLayout.Controls.Add(formatHint, 0, 5);
            formatPanel.Controls.Add(formatLayout); content.Controls.Add(formatPanel, 1, 0);
            layout.Controls.Add(content, 0, 3);

            TableLayoutPanel destination = Table(3);
            destination.Margin = new Padding(0, 14, 0, 0);
            destination.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245));
            destination.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            destination.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            BindText(besideSource, "Рядом с исходной книгой"); besideSource.Checked = true; besideSource.Dock = DockStyle.Fill; besideSource.AutoSize = true;
            outputFolder.ReadOnly = true; outputFolder.BackColor = Pale; outputFolder.BorderStyle = BorderStyle.FixedSingle;
            outputFolder.Text = L10n.T("Папка каждой исходной книги"); outputFolder.Dock = DockStyle.Fill; outputFolder.Margin = new Padding(0, 16, 12, 0);
            browse = Button("Выбрать папку…", false, 156); browse.Margin = new Padding(0, 13, 0, 0); browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browse.Click += delegate { ChooseFolder(); };
            besideSource.CheckedChanged += delegate { UpdateDestinationText(); };
            destination.Controls.Add(besideSource, 0, 0); destination.Controls.Add(outputFolder, 1, 0); destination.Controls.Add(browse, 2, 0);
            layout.Controls.Add(destination, 0, 4);

            TableLayoutPanel actions = Table(2);
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 438));
            SetStatus("Добавьте книги для конвертации"); status.Dock = DockStyle.Fill; status.TextAlign = ContentAlignment.MiddleLeft; status.ForeColor = Muted;
            actions.Controls.Add(status, 0, 0);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 13, 0, 0), Margin = Padding.Empty };
            convert = Button("Конвертировать", true, 180); convert.Enabled = false;
            cancel = Button("Остановить", false, 116); cancel.Visible = false;
            openFolder = Button("Папка результата", false, 165); openFolder.Enabled = false;
            buttons.Controls.Add(convert); buttons.Controls.Add(cancel); buttons.Controls.Add(openFolder);
            convert.Click += async delegate { await ConvertBooks(); };
            cancel.Click += delegate { cancellation.Cancel(); cancel.Enabled = false; SetStatus("Остановка после текущей операции…"); };
            openFolder.Click += delegate { OpenResultFolder(); };
            actions.Controls.Add(buttons, 1, 0); layout.Controls.Add(actions, 0, 5);

            TableLayoutPanel bottom = Table(2);
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            Label privacy = new Label { ForeColor = Muted, Font = new Font("Segoe UI", 8), Dock = DockStyle.Fill }; BindText(privacy, "Книги обрабатываются на вашем компьютере");
            progress.Dock = DockStyle.Fill; progress.Margin = new Padding(5, 4, 0, 6); progress.Style = ProgressBarStyle.Continuous;
            bottom.Controls.Add(privacy, 0, 0); bottom.Controls.Add(progress, 1, 0); layout.Controls.Add(bottom, 0, 6);
            AcceptButton = convert;
            EnableDrop(this);
            FormClosing += delegate(object sender, FormClosingEventArgs e) { if (busy) { e.Cancel = true; cancellation.Cancel(); cancel.Enabled = false; SetStatus("Остановка… Затем окно можно закрыть."); } };
            FormClosed += delegate { toolTips.Dispose(); };
            RefreshCount();
            RefreshLanguage();
            float displayScale = CurrentAutoScaleDimensions.Width / 96f;
            AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size((int)Math.Round(1040 * displayScale), (int)Math.Round(730 * displayScale));
            MinimumSize = new Size((int)Math.Round(900 * displayScale), (int)Math.Round(715 * displayScale));
        }

        static TableLayoutPanel Table(int columns)
        {
            TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent };
            if (columns == 1) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            else table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return table;
        }
        Label Description(string text)
        {
            Label label = new Label { ForeColor = Muted, Dock = DockStyle.Fill, Margin = new Padding(24, 0, 0, 0), Font = new Font("Segoe UI", 9) };
            BindText(label, text); return label;
        }
        Label LabelAt(string text, Font font, Color color, int x, int y, int width, int height)
        {
            Label label = new Label { Font = font, ForeColor = color, Location = new Point(x, y), Size = new Size(width, height) };
            BindText(label, text); return label;
        }
        Button Button(string text, bool primary, int width)
        {
            Button button = primary ? new PrimaryButton() : new Button();
            BindText(button, text); button.Width = width; button.Height = 38; button.FlatStyle = FlatStyle.Flat;
            button.BackColor = primary ? Blue : Color.White; button.ForeColor = primary ? Color.White : Ink;
            button.Cursor = Cursors.Hand; button.Margin = new Padding(0, 0, 9, 0);
            button.UseVisualStyleBackColor = false; button.Font = new Font("Segoe UI Semibold", 9);
            button.FlatAppearance.BorderColor = primary ? Blue : Line; button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = primary ? ColorTranslator.FromHtml("#004EEB") : Pale;
            button.FlatAppearance.MouseDownBackColor = primary ? ColorTranslator.FromHtml("#0040C1") : Line;
            return button;
        }

        void BindText(Control control, string russian)
        {
            captions[control] = russian; control.Text = L10n.T(russian);
        }
        void SetStatus(string russian, params object[] arguments)
        {
            statusTemplate = russian; statusArguments = arguments;
            status.Text = String.Format(L10n.T(russian), arguments);
        }
        void UpdateDestinationText()
        {
            outputFolder.Text = besideSource.Checked ? L10n.T("Папка каждой исходной книги") : (lastFolder ?? L10n.T("Выберите папку сохранения"));
        }
        static void SetRowState(DataGridViewRow row, string russian)
        {
            row.Cells[2].Tag = russian; row.Cells[2].Value = L10n.T(russian);
        }
        void RefreshLanguage()
        {
            SuspendLayout();
            foreach (KeyValuePair<Control, string> caption in captions) caption.Key.Text = L10n.T(caption.Value);
            grid.Columns[0].HeaderText = L10n.T("КНИГА"); grid.Columns[1].HeaderText = L10n.T("РАЗМЕР"); grid.Columns[2].HeaderText = L10n.T("СТАТУС");
            foreach (DataGridViewRow row in grid.Rows)
            {
                string original = row.Cells[2].Tag as string;
                if (original != null) row.Cells[2].Value = L10n.T(original);
                BookItem book = row.Tag as BookItem;
                if (book != null)
                {
                    row.Cells[1].Value = FormatBytes(book.Size);
                    row.Cells[0].ToolTipText = book.SourceDescription;
                    if (original == "Остановлено") row.Cells[2].ToolTipText = L10n.T("Конвертация остановлена.") + "\r\n" + book.SourceDescription;
                }
            }
            queueCount.Text = String.Format(L10n.T("{0} В ОЧЕРЕДИ"), grid.Rows.Count);
            status.Text = String.Format(L10n.T(statusTemplate), statusArguments);
            about.AccessibleName = L10n.T("О приложении"); toolTips.SetToolTip(about, L10n.T("О приложении"));
            languagePicker.AccessibleName = L10n.T("Язык интерфейса"); toolTips.SetToolTip(languagePicker, L10n.T("Язык интерфейса"));
            UpdateDestinationText(); drop.Invalidate(); ShowDetails();
            ResumeLayout(true);
        }

        void ConfigureGrid()
        {
            grid.Dock = DockStyle.Fill; grid.Margin = Padding.Empty; grid.ReadOnly = true; grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false; grid.AllowUserToResizeRows = false; grid.RowHeadersVisible = false;
            grid.MultiSelect = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None; grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; grid.GridColor = Line;
            grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Pale; grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9); grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight = 37; grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9); grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#EFF4FF"); grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.RowTemplate.Height = 43;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "file", HeaderText = L10n.T("КНИГА"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 65, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "size", HeaderText = L10n.T("РАЗМЕР"), Width = 86, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "state", HeaderText = L10n.T("СТАТУС"), Width = 149, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.SelectionChanged += delegate { ShowDetails(); };
            grid.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Delete && !busy) { RemoveBooks(); e.Handled = true; } };
        }

        void EnableDrop(Control control)
        {
            control.AllowDrop = true;
            control.DragEnter += delegate(object sender, DragEventArgs e) { e.Effect = !busy && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; if (!busy) drop.Highlight = true; };
            control.DragLeave += delegate { drop.Highlight = false; };
            control.DragDrop += async delegate(object sender, DragEventArgs e) { drop.Highlight = false; if (!busy && e.Data.GetDataPresent(DataFormats.FileDrop)) await AddBooksAsync((string[])e.Data.GetData(DataFormats.FileDrop)); };
            foreach (Control child in control.Controls) EnableDrop(child);
        }

        async void ChooseBooks()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = L10n.T("Добавить книги"); dialog.Filter = L10n.T("Книги и архивы|*.fb2;*.zip;*.rar;*.7z|Книги FB2|*.fb2|Архивы ZIP|*.zip|Архивы RAR|*.rar|Архивы 7Z|*.7z");
                dialog.Multiselect = true; if (dialog.ShowDialog(this) == DialogResult.OK) await AddBooksAsync(dialog.FileNames);
            }
        }

        public void AddBooks(string[] files)
        {
            if (busy) return;
            AddScannedBooks(ScanBooks(files, CancellationToken.None));
        }

        public async Task AddBooksAsync(string[] files)
        {
            if (busy) return;
            cancellation = new CancellationTokenSource(); SetBusy(true);
            SetStatus("Поиск книг в архивах…");
            progress.Style = ProgressBarStyle.Marquee;
            try
            {
                ScanResult scanned = await Task.Run(delegate { return ScanBooks(files, cancellation.Token); });
                cancellation.Token.ThrowIfCancellationRequested();
                AddScannedBooks(scanned);
            }
            catch (OperationCanceledException) { SetStatus("Добавление книг остановлено."); }
            catch (Exception error) { SetStatus("Не удалось добавить книги."); details.Text = error.Message; }
            finally { progress.Style = ProgressBarStyle.Continuous; SetBusy(false); cancellation.Dispose(); cancellation = null; }
        }

        static ScanResult ScanBooks(string[] files, CancellationToken token)
        {
            ScanResult result = new ScanResult();
            foreach (string rawPath in files)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    string path = Path.GetFullPath(rawPath); string extension = Path.GetExtension(path).ToLowerInvariant();
                    if (!File.Exists(path)) throw new FileNotFoundException(L10n.T("Файл не найден."));
                    if (extension == ".fb2") result.Books.Add(new BookItem { Source = path, Size = new FileInfo(path).Length });
                    else if (extension == ".zip" || extension == ".rar" || extension == ".7z")
                    {
                        foreach (ArchiveBook entry in ArchiveBooks.List(path, token)) result.Books.Add(new BookItem { Source = path, Entry = entry, Size = entry.Size });
                    }
                    else throw new InvalidDataException(L10n.T("Поддерживаются FB2, ZIP, RAR и 7Z."));
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception error) { result.Errors.Add(rawPath + "\r\n" + error.Message); }
            }
            return result;
        }

        void AddScannedBooks(ScanResult scanned)
        {
            int added = 0;
            grid.SuspendLayout();
            try
            {
                foreach (BookItem book in scanned.Books)
                {
                    if (books.Exists(delegate(BookItem item) { return String.Equals(item.Source, book.Source, StringComparison.OrdinalIgnoreCase) &&
                        ((item.Entry == null && book.Entry == null) || (item.Entry != null && book.Entry != null && item.Entry.Index == book.Entry.Index)); })) continue;
                    if (books.Count >= 10000) { scanned.Errors.Add(L10n.T("В очередь можно добавить до 10000 книг за раз. Обработайте текущую очередь, затем очистите её.")); break; }
                    int row = grid.Rows.Add(book.FileName, FormatBytes(book.Size), L10n.T("Ожидает"));
                    book.Row = grid.Rows[row]; book.Row.Tag = book; book.Row.Cells[0].ToolTipText = book.SourceDescription;
                    SetRowState(book.Row, "Ожидает");
                    books.Add(book); added++;
                }
            }
            finally { grid.ResumeLayout(); }
            RefreshCount();
            if (scanned.Errors.Count > 0) SetStatus("Добавлено книг: {0}. Ошибок: {1}", added, scanned.Errors.Count);
            else SetStatus("Добавлено книг: {0}", added);
            ShowDetails();
            if (scanned.Errors.Count > 0) details.Text = String.Join("\r\n\r\n", scanned.Errors);
        }

        void RemoveBooks()
        {
            if (busy) return;
            List<BookItem> removeList = new List<BookItem>();
            foreach (DataGridViewRow row in grid.SelectedRows) if (row.Tag is BookItem) removeList.Add((BookItem)row.Tag);
            foreach (BookItem book in removeList) { books.Remove(book); grid.Rows.Remove(book.Row); }
            RefreshCount(); ShowDetails();
        }

        void RefreshCount()
        {
            queueCount.Text = String.Format(L10n.T("{0} В ОЧЕРЕДИ"), books.Count); convert.Enabled = !busy && books.Count > 0;
            remove.Enabled = !busy && books.Count > 0; clear.Enabled = !busy && books.Count > 0;
        }

        void ShowDetails()
        {
            if (grid.SelectedRows.Count == 0) { details.Text = L10n.T("Выберите книгу, чтобы увидеть путь и результат."); return; }
            BookItem book = grid.SelectedRows[0].Tag as BookItem;
            if (book == null) return;
            string state = book.Row.Cells[2].Tag as string;
            details.Text = state == "Остановлено" ? L10n.T("Конвертация остановлена.") + "\r\n" + book.SourceDescription : String.IsNullOrEmpty(book.Detail) ? book.SourceDescription : book.Detail;
        }

        void ChooseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = L10n.T("Папка для готовых EPUB / AZW3"); dialog.ShowNewFolderButton = true;
                if (!String.IsNullOrEmpty(lastFolder) && Directory.Exists(lastFolder)) dialog.SelectedPath = lastFolder;
                if (dialog.ShowDialog(this) == DialogResult.OK) { lastFolder = dialog.SelectedPath; besideSource.Checked = false; outputFolder.Text = lastFolder; }
            }
        }

        async Task ConvertBooks()
        {
            if (busy || books.Count == 0) return;
            string directory = besideSource.Checked ? null : lastFolder;
            if (directory != null && !Directory.Exists(directory)) { SetStatus("Папка сохранения больше не существует."); return; }
            if (!besideSource.Checked && directory == null) { ChooseFolder(); if (besideSource.Checked || lastFolder == null) return; directory = lastFolder; }
            string extension = azw3.Checked ? ".azw3" : ".epub";
            if (extension == ".azw3" && KindleConverter.FindCalibre() == null) { SetStatus("Для AZW3 не найден необходимый компонент. Переустановите программу."); return; }
            cancellation = new CancellationTokenSource(); SetBusy(true);
            int successful = 0, failed = 0; bool stopped = false;
            progress.Maximum = books.Count; progress.Value = 0;
            try
            {
                foreach (BookItem book in books)
                {
                    if (cancellation.IsCancellationRequested) { stopped = true; break; }
                    book.Output = null; book.Detail = book.SourceDescription;
                    SetRowState(book.Row, "Обработка…"); book.Row.Cells[2].Style.ForeColor = Blue;
                    SetStatus("Книга {0} из {1}", successful + failed + 1, books.Count);
                    string targetDirectory = directory ?? Path.GetDirectoryName(book.Source);
                    try
                    {
                        string output = KindleConverter.AvailableOutput(book.Entry == null ? book.Source : book.FileName, targetDirectory, extension);
                        ConversionResult result = await Task.Run(delegate { return KindleConverter.Convert(book.Source, book.Entry, output, cancellation.Token); });
                        book.Output = output; lastResultDirectory = targetDirectory;
                        book.Detail = result.Title + (String.IsNullOrEmpty(result.Author) ? "" : " · " + result.Author) + "\r\n" + output;
                        if (result.Warnings.Count > 0) book.Detail += "\r\n" + String.Join("\r\n", result.Warnings);
                        SetRowState(book.Row, result.Warnings.Count > 0 ? "Готово · замечания" : "Готово");
                        book.Row.Cells[2].Style.ForeColor = result.Warnings.Count > 0 ? ColorTranslator.FromHtml("#B54708") : ColorTranslator.FromHtml("#067647");
                        successful++;
                    }
                    catch (OperationCanceledException) { SetRowState(book.Row, "Остановлено"); book.Detail = L10n.T("Конвертация остановлена.") + "\r\n" + book.SourceDescription; book.Row.Cells[2].ToolTipText = book.Detail; stopped = true; break; }
                    catch (Exception error) { SetRowState(book.Row, "Ошибка"); book.Row.Cells[2].Style.ForeColor = ColorTranslator.FromHtml("#B42318"); book.Detail = error.Message + "\r\n" + book.SourceDescription; failed++; }
                    book.Row.Cells[2].ToolTipText = book.Detail; progress.Value = successful + failed; ShowDetails();
                }
                if (failed > 0) SetStatus(stopped ? "Остановлено. Готово: {0}. Ошибок: {1}" : "Готово: {0}. Ошибок: {1}", successful, failed);
                else SetStatus(stopped ? "Остановлено. Готово: {0}" : "Готово: {0}", successful);
                openFolder.Enabled = lastResultDirectory != null;
            }
            finally { SetBusy(false); cancellation.Dispose(); cancellation = null; ShowDetails(); }
        }

        string lastResultDirectory;
        void OpenResultFolder()
        {
            string folder = lastResultDirectory;
            if (grid.SelectedRows.Count > 0)
            {
                BookItem book = grid.SelectedRows[0].Tag as BookItem;
                if (book != null && book.Output != null) folder = Path.GetDirectoryName(book.Output);
            }
            if (folder == null) return;
            try { Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true }); }
            catch (Exception error) { SetStatus("Не удалось открыть папку: {0}", error.Message); }
        }
        void SetBusy(bool value)
        {
            busy = value; add.Enabled = !value; browse.Enabled = !value; besideSource.Enabled = !value; languagePicker.Enabled = !value;
            azw3.Enabled = !value; epub.Enabled = !value; drop.Enabled = !value;
            cancel.Visible = value; cancel.Enabled = value; openFolder.Visible = !value;
            RefreshCount();
        }

        public void Preview()
        {
            int row = grid.Rows.Add("Путешествие к северному маяку.fb2", FormatBytes(862208), L10n.T("Готово"));
            SetRowState(grid.Rows[row], "Готово");
            grid.Rows[row].Cells[2].Style.ForeColor = ColorTranslator.FromHtml("#067647");
            row = grid.Rows.Add("Сборник рассказов.fb2.zip", FormatBytes(1468006), L10n.T("Ожидает")); SetRowState(grid.Rows[row], "Ожидает");
            row = grid.Rows.Add("Краткая история времени.fb2", FormatBytes(2936013), L10n.T("Ожидает")); SetRowState(grid.Rows[row], "Ожидает");
            queueCount.Text = String.Format(L10n.T("{0} В ОЧЕРЕДИ"), 3); SetStatus("Книги готовы к конвертации");
            details.Text = "Путешествие к северному маяку · Алексей Северный\r\n" + String.Format(L10n.T("Готовый файл: {0}"), "Путешествие к северному маяку.azw3");
            convert.Enabled = true; clear.Enabled = true; remove.Enabled = true;
            progress.Maximum = 3; progress.Value = 1;
        }

        static string FormatBytes(long bytes)
        { return bytes >= 1048576 ? (bytes / 1048576.0).ToString("0.0") + " " + L10n.T("МБ") : Math.Max(1, bytes / 1024).ToString() + " " + L10n.T("КБ"); }
        static Icon MakeIcon()
        {
            using (Bitmap bitmap = new Bitmap(32, 32))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Blue);
                using (Pen pen = new Pen(Color.White, 2)) { graphics.DrawRectangle(pen, 8, 5, 16, 22); graphics.DrawLine(pen, 12, 11, 20, 11); graphics.DrawLine(pen, 12, 16, 20, 16); graphics.DrawLine(pen, 12, 21, 17, 21); }
                IntPtr handle = bitmap.GetHicon();
                try { using (Icon icon = Icon.FromHandle(handle)) return (Icon)icon.Clone(); }
                finally { DestroyIcon(handle); }
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
        class LanguageChoice
        {
            public readonly string Code;
            readonly string name;
            public LanguageChoice(string code, string name) { Code = code; this.name = name; }
            public override string ToString() { return name; }
        }
        class ScanResult { public List<BookItem> Books = new List<BookItem>(); public List<string> Errors = new List<string>(); }
        class BookItem
        {
            public string Source; public ArchiveBook Entry; public long Size; public string Output; public string Detail; public DataGridViewRow Row;
            public string FileName
            {
                get
                {
                    if (Entry == null) return Path.GetFileName(Source);
                    string name = Entry.EntryName.Replace('\\', '/');
                    return name.Substring(name.LastIndexOf('/') + 1);
                }
            }
            public string SourceDescription { get { return Source + (Entry == null ? "" : "\r\n" + String.Format(L10n.T("Книга в архиве: {0}"), Entry.EntryName)); } }
        }

        class BorderedPanel : Panel
        {
            public BorderedPanel() { DoubleBuffered = true; }
            protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (Pen pen = new Pen(Line)) e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1); }
        }
        class PrimaryButton : Button
        {
            bool hovered, mousePressed, keyboardPressed;
            public PrimaryButton()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                // Draw the caption explicitly: native themes can replace a button's ForeColor.
                bool pressed = keyboardPressed || (mousePressed && hovered);
                Color background = !Enabled ? Pale : pressed ? ColorTranslator.FromHtml("#0040C1") : hovered ? ColorTranslator.FromHtml("#004EEB") : Blue;
                Color caption = Enabled ? Color.White : Muted;
                e.Graphics.Clear(background);
                using (Pen border = new Pen(Enabled ? background : Line)) e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
                Rectangle textBounds = new Rectangle(6, 3, Math.Max(0, Width - 12), Math.Max(0, Height - 6));
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
                if (!ShowKeyboardCues) flags |= TextFormatFlags.HidePrefix;
                TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, caption, background, flags);
                if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(4, 4, Math.Max(0, Width - 8), Math.Max(0, Height - 8)), caption, background);
            }
            protected override void OnMouseEnter(EventArgs e) { hovered = true; base.OnMouseEnter(e); Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { hovered = false; base.OnMouseLeave(e); Invalidate(); }
            protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) mousePressed = true; base.OnMouseDown(e); Invalidate(); }
            protected override void OnMouseUp(MouseEventArgs e) { mousePressed = false; base.OnMouseUp(e); Invalidate(); }
            protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) keyboardPressed = true; base.OnKeyDown(e); Invalidate(); }
            protected override void OnKeyUp(KeyEventArgs e) { keyboardPressed = false; base.OnKeyUp(e); Invalidate(); }
            protected override void OnEnabledChanged(EventArgs e) { mousePressed = keyboardPressed = false; base.OnEnabledChanged(e); Invalidate(); }
            protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
            protected override void OnLostFocus(EventArgs e) { mousePressed = keyboardPressed = false; base.OnLostFocus(e); Invalidate(); }
        }
        class DropPanel : Panel
        {
            bool highlight;
            public bool Highlight { get { return highlight; } set { highlight = value; Invalidate(); } }
            public DropPanel() { DoubleBuffered = true; BackColor = Pale; }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.Clear(highlight ? ColorTranslator.FromHtml("#EFF4FF") : Pale);
                using (Pen pen = new Pen(highlight ? Blue : Line)) { pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1); }
                string title = L10n.T("Перетащите книги сюда или нажмите для выбора");
                string caption = L10n.T("FB2 / ZIP / RAR / 7Z  ·  все книги из архива");
                using (Font headingFont = new Font("Segoe UI Semibold", 10))
                using (Font captionFont = new Font("Segoe UI", 9))
                {
                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                    Size available = new Size(Math.Max(1, Width - 20), Int32.MaxValue);
                    int headingHeight = TextRenderer.MeasureText(e.Graphics, title, headingFont, available, flags).Height;
                    int captionHeight = TextRenderer.MeasureText(e.Graphics, caption, captionFont, available, flags).Height;
                    int gap = Math.Max(4, (int)Math.Round(4 * e.Graphics.DpiY / 96f));
                    int top = (Height - headingHeight - gap - captionHeight) / 2;
                    TextRenderer.DrawText(e.Graphics, title, headingFont, new Rectangle(10, top, Math.Max(1, Width - 20), headingHeight), Ink, flags);
                    TextRenderer.DrawText(e.Graphics, caption, captionFont, new Rectangle(10, top + headingHeight + gap, Math.Max(1, Width - 20), captionHeight), Muted, flags);
                }
            }
        }
    }
}
