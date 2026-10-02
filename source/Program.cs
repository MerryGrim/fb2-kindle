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
[assembly: AssemblyVersion("1.2.1.0")]
[assembly: AssemblyFileVersion("1.2.1.0")]

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
                using (MainForm form = new MainForm())
                {
                    if (args.Length >= 2 && args[0] == "--preview")
                    {
                        if (args.Length == 4) form.ClientSize = new Size(Int32.Parse(args[2]), Int32.Parse(args[3]));
                        form.Preview();
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.Manual;
                        form.Location = new Point(-30000, -30000);
                        form.Show();
                        Application.DoEvents();
                        form.PerformLayout();
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
                if (args.Length > 0 && (args[0] == "--convert" || args[0] == "--preview"))
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
