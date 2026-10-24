using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fb2Kindle;

public static class BatchUiTests
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static string artifacts;
    static int exit=1;
    static object Field(object owner,string name){return owner.GetType().GetField(name,Private).GetValue(owner);}
    static int Number(object owner,string name){return (int)owner.GetType().GetField(name).GetValue(owner);}
    static string State(object owner){return owner.GetType().GetField("State").GetValue(owner).ToString();}
    static void Invoke(object owner,string name,params object[] values){owner.GetType().GetMethod(name,Private).Invoke(owner,values);}
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);Console.WriteLine("PASS "+message);}
    [STAThread] public static int Main(string[] args)
    {
        artifacts=Path.GetFullPath(args[0]);Directory.CreateDirectory(artifacts);
        Assembly asm=typeof(MainForm).Assembly;
        asm.GetType("Fb2Kindle.UiSettings").GetField("Isolated").SetValue(null,true);
        asm.GetType("Fb2Kindle.L10n").GetField("Isolated").SetValue(null,true);
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        MainForm form=new MainForm();
        if(args.Length>1&&args[1]=="interactive")
        {
            form.Shown+=async delegate {await form.AddBooksAsync(Directory.GetFiles(Path.Combine(artifacts,"input"),"*.fb2"));};
            Application.Run(form);return 0;
        }
        form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-20000,-20000);form.ShowInTaskbar=false;
        form.Shown+=async delegate {try{await Run(form);exit=0;}catch(Exception error){Console.WriteLine("FAIL "+error);File.WriteAllText(Path.Combine(artifacts,"error.txt"),error.ToString());}finally{form.Close();}};
        Application.Run(form);return exit;
    }
    static async Task Run(MainForm form)
    {
        string input=Path.Combine(artifacts,"input"),output=Path.Combine(artifacts,"output");Directory.CreateDirectory(input);Directory.CreateDirectory(output);
        string[] files=new string[9];
        for(int i=0;i<files.Length;i++)
        {
            files[i]=Path.Combine(input,"Synthetic book "+(i+1)+".fb2");
            string xml="<?xml version=\"1.0\" encoding=\"utf-8\"?><FictionBook xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\"><description><title-info><genre>sf</genre><author><first-name>Test</first-name><last-name>Author</last-name></author><book-title>Synthetic book "+(i+1)+"</book-title><lang>en</lang></title-info><document-info><author><nickname>FB2 Kindle QA</nickname></author><date>2026-10-24</date><id>synthetic-"+i+"</id><version>1.0</version></document-info></description><body><section><title><p>Chapter one</p></title><p>Batch integration proof "+i+". Кириллица сохраняется.</p></section></body></FictionBook>";
            File.WriteAllText(files[i],xml,new UTF8Encoding(false));
        }
        await form.AddBooksAsync(files);
        ScrollableControl queue=(ScrollableControl)Field(form,"queue");IList books=(IList)queue.GetType().GetField("Books").GetValue(queue);
        Check(books.Count==9,"nine real FB2 files added to queue");
        Rectangle queueBounds=queue.Bounds,settingsBounds=((Control)Field(form,"destinationModes")).Bounds,footerBounds=((Control)Field(form,"convert")).Bounds;
        Check(queue.VerticalScroll.Visible,"nine books scroll inside queue");
        Invoke(form,"SetDestination",false);((TextBox)Field(form,"outputFolder")).Text=output;Invoke(form,"SetFormat","EPUB");
        await form.ConvertBooks();
        Check(Directory.GetFiles(output,"*.epub").Length==9,"nine real EPUB outputs produced");
        foreach(object book in books){Check(Number(book,"Percent")==100,"completed book progress is 100 percent");Check(State(book)=="Done"||State(book)=="Warning","book finished successfully");}
        Check(queue.AutoScrollPosition.Y<0,"batch auto-scrolled to later books");
        Check(queue.Bounds==queueBounds&&((Control)Field(form,"destinationModes")).Bounds==settingsBounds&&((Control)Field(form,"convert")).Bounds==footerBounds,"queue scroll leaves settings and footer fixed");
        Check(!form.AutoScroll,"whole application does not scroll");
        using(Bitmap bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(artifacts,"batch-completed.png"));}
        File.WriteAllText(Path.Combine(artifacts,"receipt.txt"),"batch=9\r\noutputs=9\r\nformat=EPUB\r\nallPercent=100\r\nqueueOffset="+queue.AutoScrollPosition.Y+"\r\nfixedSettings=true\r\nfixedFooter=true\r\nwholeAppScroll=false\r\n");
    }
}
