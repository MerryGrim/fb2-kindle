using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

[assembly: AssemblyTitle("FB2 Kindle")]
[assembly: AssemblyDescription("FB2 to EPUB and AZW3 converter")]
[assembly: AssemblyCompany("MerryGrim")]
[assembly: AssemblyProduct("FB2 Kindle")]
[assembly: AssemblyCopyright("Copyright (c) 2026 MerryGrim")]
[assembly: AssemblyVersion("1.3.1.0")]
[assembly: AssemblyFileVersion("1.3.1.0")]

namespace Fb2Kindle
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                if (args.Length == 3 && args[0] == "--convert")
                {
                    KindleConverter.Convert(args[1], args[2], CancellationToken.None);
                    return 0;
                }
                bool previewMode=args.Length>=2&&(args[0]=="--preview"||args[0]=="--verify-ui"||args[0]=="--preview-about");
                if(previewMode){UiSettings.Isolated=true;L10n.Isolated=true;}
                using (MainForm form = new MainForm())
                {
                    if (previewMode)
                    {
                        string lang=args.Length>2?args[2]:"ru",theme=args.Length>3?args[3]:"light";
                        int oldWidth;
                        if(args[0]=="--preview"&&args.Length==4&&Int32.TryParse(args[2],out oldWidth)){lang="ru";theme="light";form.ClientSize=new Size(oldWidth,Int32.Parse(args[3]));}
                        int count=args[0]=="--preview"&&args.Length>4?Int32.Parse(args[4]):args[0]=="--verify-ui"?7:4;
                        if(args[0]=="--preview"&&args.Length>=7){form.MinimumSize=Size.Empty;form.ClientSize=new Size(Int32.Parse(args[5]),Int32.Parse(args[6]));}
                        if(args[0]=="--verify-ui"&&args.Length>=6){form.MinimumSize=Size.Empty;form.ClientSize=new Size(Int32.Parse(args[4]),Int32.Parse(args[5]));}
                        form.Preview(lang,theme,count);
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.Manual;
                        form.Location = new Point(-30000, -30000);
                        form.Show();
                        Application.DoEvents();
                        form.PerformLayout();
                        if(args[0]=="--verify-ui"){File.WriteAllText(Path.GetFullPath(args[1]),form.VerifyUi());return 0;}
                        if(args[0]=="--preview-about")
                        {
                            using(AboutForm about=new AboutForm()){about.StartPosition=FormStartPosition.Manual;about.Location=new Point(-30000,-30000);about.Show(form);Application.DoEvents();using(Bitmap image=new Bitmap(about.Width,about.Height)){about.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save(Path.GetFullPath(args[1]),System.Drawing.Imaging.ImageFormat.Png);}}return 0;
                        }
                        using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
                        {
                            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                            bitmap.Save(Path.GetFullPath(args[1]), System.Drawing.Imaging.ImageFormat.Png);
                        }
                        return 0;
                    }
                    if (args.Length > 0) form.Shown += async delegate { await form.AddBooksAsync(args); };
                    Application.Run(form);
                }
                return 0;
            }
            catch (Exception error)
            {
                if (args.Length > 0 && (args[0] == "--convert" || args[0].StartsWith("--preview",StringComparison.Ordinal) || args[0] == "--verify-ui"))
                {
                    try { File.WriteAllText(Path.Combine(Path.GetTempPath(), "FB2Kindle-last-error.txt"), error.ToString()); } catch { }
                    return 1;
                }
                MessageBox.Show(error.Message, "FB2 Kindle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}
