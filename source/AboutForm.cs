using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Fb2Kindle
{
    sealed class MiraMessage : MiraWindow
    {
        public MiraMessage(string title,string text)
        {
            Text=title;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);BackColor=MiraTheme.Background;ForeColor=MiraTheme.Text;
            using(Graphics g=CreateGraphics())UiScale=g.DpiX/96f;
            ClientSize=new Size((int)(680*UiScale),(int)(450*UiScale));MinimumSize=MaximumSize=Size;
            Label heading=new Label{Text=title,AutoEllipsis=true,ForeColor=MiraTheme.Text,BackColor=MiraTheme.Background,Font=new Font("Segoe UI Semibold",14),Location=new Point((int)(24*UiScale),(int)(18*UiScale)),Size=new Size((int)(560*UiScale),(int)(34*UiScale))};Controls.Add(heading);heading.MouseDown+=delegate{DragWindow();};
            MiraButton x=new MiraButton{IconName="x",Quiet=true,UiScale=UiScale,ForeColor=MiraTheme.Text,AccessibleName=L10n.T("Закрыть"),Bounds=new Rectangle(Width-(int)(56*UiScale),(int)(18*UiScale),(int)(34*UiScale),(int)(34*UiScale))};x.Click+=delegate{Close();};Controls.Add(x);
            TextBox body=new TextBox{Text=text,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.None,BackColor=MiraTheme.Surface,ForeColor=MiraTheme.Text,Font=Font,Bounds=new Rectangle((int)(24*UiScale),(int)(70*UiScale),Width-(int)(48*UiScale),Height-(int)(150*UiScale))};Controls.Add(body);
            MiraButton close=new MiraButton{Text=L10n.T("Закрыть"),UiScale=UiScale,Primary=true,Bounds=new Rectangle(Width-(int)(144*UiScale),Height-(int)(60*UiScale),(int)(120*UiScale),(int)(38*UiScale))};close.Click+=delegate{Close();};Controls.Add(close);AcceptButton=CancelButton=close;
        }
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);using(Pen p=new Pen(MiraTheme.Border))e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);}
    }
    sealed class AboutForm : MiraWindow
    {
        public const string GitHubProfile="https://github.com/MerryGrim";
        readonly MiraButton documents=new MiraButton();
        readonly MiraContextMenu documentMenu=MiraMenus.Create();
        readonly TextBox license=new TextBox();
        readonly List<string> content=new List<string>();
        readonly List<string> titles=new List<string>();
        public AboutForm()
        {
            Text=L10n.T("О приложении");Font=new Font("Segoe UI",10);BackColor=MiraTheme.Background;ForeColor=MiraTheme.Text;StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;
            using(Graphics g=CreateGraphics())UiScale=g.DpiX/96f;
            ClientSize=new Size(S(760),S(600));MinimumSize=MaximumSize=Size;
            Label title=new Label{Text="FB2 Kindle",Font=new Font("Segoe UI Semibold",21),Bounds=new Rectangle(S(26),S(22),S(600),S(42)),ForeColor=MiraTheme.Text};Controls.Add(title);title.MouseDown+=delegate{DragWindow();};
            Label version=new Label{Text=L10n.T("Версия 1.3.1 · MiraUI"),Font=new Font("Segoe UI",10),Bounds=new Rectangle(S(28),S(69),S(650),S(25)),ForeColor=MiraTheme.Muted};Controls.Add(version);
            MiraButton x=new MiraButton{IconName="x",Quiet=true,UiScale=UiScale,ForeColor=MiraTheme.Text,Bounds=new Rectangle(Width-S(58),S(24),S(34),S(34)),AccessibleName=L10n.T("Закрыть")};x.Click+=delegate{Close();};Controls.Add(x);
            Label description=new Label{Text=L10n.T("Конвертер FB2 и архивов ZIP, RAR, 7Z в EPUB и AZW3.\r\nКниги обрабатываются на вашем компьютере."),Bounds=new Rectangle(S(28),S(111),S(700),S(52)),ForeColor=MiraTheme.Muted};Controls.Add(description);
            MiraButton author=new MiraButton{Text="GitHub / MerryGrim",IconName="folder-open",UiScale=UiScale,ForeColor=MiraTheme.Accent,Bounds=new Rectangle(S(28),S(173),S(214),S(38)),AccessibleName=L10n.T("Профиль автора на GitHub")};author.Click+=delegate{try{Process.Start(new ProcessStartInfo(GitHubProfile){UseShellExecute=true});}catch(Exception error){using(MiraMessage msg=new MiraMessage(L10n.T("Не удалось открыть ссылку: "),error.Message+"\r\n"+GitHubProfile))msg.ShowDialog(this);}};Controls.Add(author);
            Label legal=new Label{Text=L10n.T("Лицензии и компоненты"),Font=new Font("Segoe UI Semibold",10),Bounds=new Rectangle(S(28),S(230),S(700),S(24))};Controls.Add(legal);
            documents.Font=new Font("Segoe UI",10);documents.UiScale=UiScale;documents.Bounds=new Rectangle(S(28),S(262),S(704),S(34));documents.BackColor=MiraTheme.Background;documents.ForeColor=MiraTheme.Text;Controls.Add(documents);
            AddDocument("FB2 Kindle · MIT",L10n.T("Собственный код, документация и ресурсы FB2 Kindle распространяются по MIT. Сторонние компоненты в папке engine сохраняют свои лицензии.")+"\r\n\r\n"+ReadDocuments("LICENSE"));
            AddDocument(L10n.T("Компоненты"),L10n.T("Calibre и 7-Zip работают отдельными процессами и сохраняют свои лицензии. Версии и соответствующие исходные архивы указаны в THIRD-PARTY-NOTICES.md и папке engine.")+"\r\n\r\n"+ReadDocuments("THIRD-PARTY-NOTICES.md"));
            AddDocument("Calibre · GPL v3",ReadDocuments("engine/Calibre/LICENSE"));AddDocument(L10n.T("Calibre · авторство"),ReadDocuments("engine/Calibre/COPYRIGHT"));AddDocument("7-Zip",ReadDocuments("engine/Archive7z/LICENSE.txt","engine/Archive7z/COPYING.txt","engine/Archive7z/UNRAR-LICENSE.txt"));AddDocument("Inno Setup",ReadDocuments("engine/InnoSetup/LICENSE.txt"));
            license.Multiline=true;license.ReadOnly=true;license.ScrollBars=ScrollBars.Vertical;license.WordWrap=true;license.BorderStyle=BorderStyle.None;license.BackColor=MiraTheme.Surface;license.ForeColor=MiraTheme.Text;license.Font=new Font("Segoe UI",9);license.Bounds=new Rectangle(S(28),S(306),S(704),S(210));Controls.Add(license);
            documents.Click+=delegate{ShowDocumentMenu();};documents.Text=titles[0];license.Text=content[0];
            MiraButton close=new MiraButton{Text=L10n.T("Закрыть"),Primary=true,UiScale=UiScale,Bounds=new Rectangle(Width-S(148),Height-S(60),S(120),S(38))};close.Click+=delegate{Close();};Controls.Add(close);AcceptButton=CancelButton=close;
        }
        int S(int n){return(int)Math.Round(n*UiScale);}
        void AddDocument(string title,string text){titles.Add(title);content.Add(text);}
        void ShowDocumentMenu(){MiraContextMenu menu=documentMenu;menu.Items.Clear();for(int i=0;i<titles.Count;i++){int index=i;ToolStripMenuItem item=new ToolStripMenuItem(titles[i]){Checked=titles[i]==documents.Text,ForeColor=MiraTheme.Text};item.Click+=delegate{documents.Text=titles[index];license.Text=content[index];};menu.Items.Add(item);}menu.Show(documents,new Point(0,documents.Height));}
        internal void VerifyDocumentMenu(){ShowDocumentMenu();Application.DoEvents();((ToolStripMenuItem)documentMenu.Items[1]).PerformClick();documentMenu.Close();Application.DoEvents();if(documents.Text!=titles[1]||license.Text!=content[1]||documentMenu.IsDisposed)throw new InvalidOperationException("License menu click failed");ShowDocumentMenu();Application.DoEvents();documentMenu.Items[2].Select();documentMenu.VerifyEnter();Application.DoEvents();if(documents.Text!=titles[2]||license.Text!=content[2]||documentMenu.IsDisposed)throw new InvalidOperationException("License menu Enter failed");}
        static string ReadDocuments(params string[] files){StringBuilder text=new StringBuilder();foreach(string file in files){if(text.Length>0)text.Append("\r\n\r\n────────────────────────\r\n\r\n");try{text.Append(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,file.Replace('/',Path.DirectorySeparatorChar)),Encoding.UTF8).Replace("\r\n","\n").Replace("\n","\r\n"));}catch(IOException){text.Append(L10n.T("Файл лицензии не найден: ")+file);}catch(UnauthorizedAccessException){text.Append(L10n.T("Не удалось прочитать файл лицензии: ")+file);}}return text.ToString();}
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);using(Pen p=new Pen(MiraTheme.Border))e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);}
        protected override void Dispose(bool disposing){if(disposing)documentMenu.Dispose();base.Dispose(disposing);}
    }
}
