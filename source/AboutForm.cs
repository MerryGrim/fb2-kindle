using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Fb2Kindle
{
    sealed class AboutForm : Form
    {
        public const string GitHubProfile = "https://github.com/MerryGrim";

        public AboutForm()
        {
            Text = L10n.T("О приложении");
            Font = new Font("Segoe UI", 10);
            ForeColor = Color.FromArgb(24, 32, 48);
            BackColor = Color.White;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(740, 540);
            MinimumSize = new Size(740, 540);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), RowCount = 5, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "FB2 Kindle  /  1.2.1", Font = new Font("Segoe UI Semibold", 20), Dock = DockStyle.Fill }, 0, 0);
            layout.Controls.Add(new Label { Text = L10n.T("Конвертер FB2 и архивов ZIP, RAR, 7Z в EPUB и AZW3.\r\nКниги обрабатываются на вашем компьютере."), ForeColor = Color.FromArgb(96, 108, 128), Dock = DockStyle.Fill }, 0, 1);

            LinkLabel github = new LinkLabel { Text = "GitHub  /  MerryGrim", AutoSize = true, LinkColor = Color.FromArgb(21, 94, 239), VisitedLinkColor = Color.FromArgb(21, 94, 239), Margin = new Padding(0, 8, 0, 0) };
            github.AccessibleName = L10n.T("Профиль автора на GitHub");
            github.LinkClicked += delegate { OpenProfile(); };
            layout.Controls.Add(github, 0, 2);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Multiline = true };
            AddTextTab(tabs, "FB2 Kindle · MIT", L10n.T("Собственный код, документация и ресурсы FB2 Kindle распространяются по MIT. Сторонние компоненты в папке engine сохраняют свои лицензии.") + "\r\n\r\n" + ReadDocuments("LICENSE"));
            AddTextTab(tabs, L10n.T("Компоненты"), L10n.T("Calibre 9.15.0 — создание AZW3.\r\n7-Zip 26.00 — чтение архивов.\r\nInno Setup 7.1.0 — установочный пакет.\r\n\r\nДвижки Calibre и 7-Zip не изменены и запускаются отдельными процессами. Лицензии приведены на языке оригинала. Уведомления об авторстве и исходные архивы движков находятся в папке engine рядом с программой."));
            AddTextTab(tabs, "Calibre · GPL v3", ReadDocuments("engine/Calibre/LICENSE"));
            AddTextTab(tabs, L10n.T("Calibre · авторство"), ReadDocuments("engine/Calibre/COPYRIGHT"));
            AddTextTab(tabs, "7-Zip", ReadDocuments("engine/Archive7z/LICENSE.txt", "engine/Archive7z/COPYING.txt", "engine/Archive7z/UNRAR-LICENSE.txt"));
            AddTextTab(tabs, "Inno Setup", ReadDocuments("engine/InnoSetup/LICENSE.txt"));
            layout.Controls.Add(tabs, 0, 3);

            Button close = new Button { Text = L10n.T("Закрыть"), DialogResult = DialogResult.OK, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(247, 248, 250), ForeColor = ForeColor, UseVisualStyleBackColor = false, Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Margin = new Padding(0, 14, 0, 0) };
            close.FlatAppearance.BorderColor = Color.FromArgb(222, 227, 235);
            layout.Controls.Add(close, 0, 4);
            AcceptButton = close;
            CancelButton = close;
        }

        static void AddTextTab(TabControl tabs, string title, string text)
        {
            TabPage page = new TabPage(title) { BackColor = Color.White, Padding = new Padding(10) };
            TextBox content = new TextBox { Text = text, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9), WordWrap = true };
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }

        static string ReadDocuments(params string[] files)
        {
            StringBuilder text = new StringBuilder();
            foreach (string relative in files)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
                if (text.Length != 0) text.Append("\r\n\r\n────────────────────────\r\n\r\n");
                try { text.Append(File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n").Replace("\n", "\r\n")); }
                catch (IOException) { text.Append(L10n.T("Файл лицензии не найден: ") + relative); }
                catch (UnauthorizedAccessException) { text.Append(L10n.T("Не удалось прочитать файл лицензии: ") + relative); }
            }
            return text.ToString();
        }

        void OpenProfile()
        {
            try { Process.Start(new ProcessStartInfo(GitHubProfile) { UseShellExecute = true }); }
            catch (Exception error) { MessageBox.Show(this, L10n.T("Не удалось открыть ссылку: ") + error.Message + "\r\n" + GitHubProfile, "FB2 Kindle", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }
    }
}
