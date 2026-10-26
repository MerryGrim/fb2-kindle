using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Fb2Kindle;

public static class WindowPlacementTests
{
    static int checks;
    static readonly List<string> receipt=new List<string>();
    static string artifacts;
    static void Check(bool condition,string message)
    {
        if(!condition)throw new InvalidOperationException(message);
        checks++;receipt.Add("PASS "+message);Console.WriteLine("PASS "+message);
    }
    static string Rect(Rectangle r){return String.Join(",",new[]{r.X,r.Y,r.Width,r.Height});}
    static Rectangle ParseRect(string value){string[] p=value.Split(',');return new Rectangle(Int32.Parse(p[0]),Int32.Parse(p[1]),Int32.Parse(p[2]),Int32.Parse(p[3]));}
    static void Pump(){for(int i=0;i<10;i++){Application.DoEvents();Thread.Sleep(20);}}
    static Dictionary<string,string> ReadReceipt(string path)
    {
        Dictionary<string,string> values=new Dictionary<string,string>();
        foreach(string line in File.ReadAllLines(path)){int p=line.IndexOf('=');if(p>0)values[line.Substring(0,p)]=line.Substring(p+1);}
        return values;
    }
    static Dictionary<string,string> Child(string directory,string action)
    {
        Directory.CreateDirectory(directory);
        ProcessStartInfo start=new ProcessStartInfo(Assembly.GetExecutingAssembly().Location,"--child \""+directory+"\" "+action){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
        using(Process process=Process.Start(start))
        {
            if(!process.WaitForExit(20000)){process.Kill();throw new TimeoutException("Window placement child timed out: "+action);}
            string output=process.StandardOutput.ReadToEnd(),error=process.StandardError.ReadToEnd();
            if(process.ExitCode!=0)throw new Exception("Child "+action+" failed: "+output+error);
        }
        return ReadReceipt(Path.Combine(directory,action+".txt"));
    }
    [STAThread] public static int Main(string[] args)
    {
        try
        {
            if(args.Length>=3&&args[0]=="--child")return RunChild(args[1],args[2]);
            artifacts=Path.GetFullPath(args[0]);Directory.CreateDirectory(artifacts);
            PurePlacement();NativeRoundTrips();
            receipt.Add("checks="+checks);File.WriteAllLines(Path.Combine(artifacts,"receipt.txt"),receipt.ToArray(),new UTF8Encoding(false));
            Console.WriteLine("All "+checks+" window placement checks passed.");return 0;
        }
        catch(Exception error){Console.WriteLine("FAIL "+error);if(artifacts!=null)File.WriteAllText(Path.Combine(artifacts,"error.txt"),error.ToString());return 1;}
    }
    static void PurePlacement()
    {
        Rectangle work=new Rectangle(0,0,1920,1040),left=new Rectangle(-1600,60,1600,900);
        Check(WindowPlacement.Center(new Size(960,980),work)==new Rectangle(480,30,960,980),"first-run placement is centered within working area");
        Rectangle normal=new Rectangle(170,90,1100,820);
        Check(WindowPlacement.Resolve(normal,work,work,new Size(900,740))==normal,"valid size and position are preserved exactly");
        Rectangle negative=new Rectangle(-1500,100,1000,800);
        Check(WindowPlacement.Resolve(negative,left,left,new Size(900,740))==negative,"valid negative-coordinate monitor position is preserved");
        Rectangle recovered=WindowPlacement.Resolve(negative,left,work,new Size(900,740));
        Check(work.Contains(recovered)&&recovered.Size==negative.Size,"disconnected monitor restores the whole window to an available screen");
        Rectangle small=new Rectangle(40,30,1280,720);
        Rectangle fitted=WindowPlacement.Resolve(new Rectangle(120,90,1700,980),work,small,new Size(900,740));
        Check(small.Contains(fitted)&&fitted.Width>0&&fitted.Height>0,"lower-resolution screen keeps the whole window accessible even below preferred minimum height");
        Rectangle taskbarWork=new Rectangle(80,40,1840,1000);
        Check(taskbarWork.Contains(WindowPlacement.Resolve(new Rectangle(0,0,1100,820),work,taskbarWork,new Size(900,740))),"work-area change does not leave title controls under a taskbar");
        Rectangle extreme=WindowPlacement.Resolve(new Rectangle(Int32.MaxValue-5,Int32.MinValue+5,1400,900),work,small,new Size(900,740));
        Check(small.Contains(extreme),"extreme saved coordinates are recovered without overflow");
        WindowPlacement parsed;
        Check(!WindowPlacement.TryParse("",out parsed)&&!WindowPlacement.TryParse("garbage",out parsed),"empty and malformed stored placement is rejected");
        Check(!WindowPlacement.TryParse("0,0,0,800,0,0,1920,1040,0",out parsed)&&!WindowPlacement.TryParse("0,0,1000,800,0,0,1920,1040,2",out parsed)&&!WindowPlacement.TryParse("2147483647,0,1000,800,0,0,1920,1040,0",out parsed),"invalid sizes, window states and extreme serialized coordinates are rejected");
        WindowPlacement original=new WindowPlacement{Bounds=negative,WorkingArea=left,Maximized=true};
        Check(WindowPlacement.TryParse(original.ToString(),out parsed)&&parsed.Bounds==negative&&parsed.WorkingArea==left&&parsed.Maximized,"stored placement round-trips negative coordinates and maximized state");
    }
    static void NativeRoundTrips()
    {
        string root=Path.Combine(artifacts,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        Dictionary<string,string> first=Child(Path.Combine(root,"first"),"read");
        Rectangle firstBounds=ParseRect(first["bounds"]),firstWork=ParseRect(first["work"]);
        Check(Math.Abs(firstBounds.Left-(firstWork.Left+(firstWork.Width-firstBounds.Width)/2))<=1&&Math.Abs(firstBounds.Top-(firstWork.Top+(firstWork.Height-firstBounds.Height)/2))<=1,"native first launch is visibly centered");
        int expectedWidth=Math.Min((int)Math.Round(960*Single.Parse(first["scale"],CultureInfo.InvariantCulture)),firstWork.Width-(int)Math.Round(24*Single.Parse(first["scale"],CultureInfo.InvariantCulture)));
        Check(firstBounds.Width==expectedWidth,"native first launch uses the narrower 960 logical pixel width");
        foreach(string mode in new[]{"normal","max","min-normal","min-max"})
        {
            string directory=Path.Combine(root,mode);
            Dictionary<string,string> saved=Child(directory,"write-"+mode);
            Dictionary<string,string> reopened=Child(directory,"read");
            bool max=mode=="max"||mode=="min-max";
            Check(reopened["state"]==(max?"Maximized":"Normal"),"separate process restores state after "+mode+" close");
            if(max)Check(ParseRect(reopened["bounds"])==ParseRect(reopened["work"]),"maximized native window fills only the monitor working area after "+mode);
            Check(reopened["icon"]==(max?"restore":"square"),"titlebar icon agrees with restored window state after "+mode);
            Check(ParseRect(reopened["normal"])==ParseRect(saved["normal"]),"separate process restores normal size and position after "+mode+" close");
            Check(reopened["theme"]=="dark"&&reopened["format"]=="EPUB"&&reopened["folder"]==@"D:\QA output=books"&&reopened["beside"]=="False","window persistence preserves existing preferences after "+mode);
        }
        string corrupt=Path.Combine(root,"corrupt");Directory.CreateDirectory(corrupt);File.WriteAllText(Path.Combine(corrupt,"ui.txt"),"theme=dark\r\nformat=EPUB\r\nwindow=malformed\r\n");
        Dictionary<string,string> fallback=Child(corrupt,"read");Rectangle fallbackBounds=ParseRect(fallback["bounds"]),fallbackWork=ParseRect(fallback["work"]);
        Check(fallbackWork.Contains(fallbackBounds)&&fallback["state"]=="Normal"&&fallback["theme"]=="dark"&&fallback["format"]=="EPUB","corrupt placement falls back safely without losing other preferences");
    }
    static int RunChild(string directory,string action)
    {
        UiSettings.StoragePathOverride=Path.Combine(directory,"ui.txt");UiSettings.Isolated=false;L10n.Isolated=true;
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        using(MainForm form=new MainForm())
        {
            if(action=="interactive"){Application.Run(form);return 0;}
            form.ShowInTaskbar=false;form.Show();Pump();
            Rectangle work=Screen.FromControl(form).WorkingArea;
            Rectangle startupBounds=form.Bounds;
            Rectangle normal=form.WindowState==FormWindowState.Normal?form.Bounds:form.RestoreBounds;
            FormWindowState startupState=form.WindowState;
            string startupIcon=((MiraButton)typeof(MainForm).GetField("maximize",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form)).IconName;
            if(action.StartsWith("write-",StringComparison.Ordinal))
            {
                form.WindowState=FormWindowState.Normal;Pump();
                int width=Math.Min(work.Width-80,Math.Max(form.MinimumSize.Width,(int)Math.Round(1000*form.UiScale)));
                int height=Math.Min(work.Height-70,Math.Max(form.MinimumSize.Height,(int)Math.Round(780*form.UiScale)));
                Rectangle target=new Rectangle(work.Left+31,work.Top+27,width,height);
                form.Bounds=target;Pump();normal=form.Bounds;
                BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
                typeof(MainForm).GetMethod("SetTheme",flags).Invoke(form,new object[]{true,true});
                typeof(MainForm).GetMethod("SetFormat",flags).Invoke(form,new object[]{"EPUB"});
                typeof(MainForm).GetMethod("SetDestination",flags).Invoke(form,new object[]{false});
                ((TextBox)typeof(MainForm).GetField("outputFolder",flags).GetValue(form)).Text=@"D:\QA output=books";
                if(action=="write-max"||action=="write-min-max"){form.WindowState=FormWindowState.Maximized;Pump();}
                if(action=="write-min-normal"||action=="write-min-max"){form.WindowState=FormWindowState.Minimized;Pump();}
                form.Close();Pump();
            }
            else
            {
                if(startupState==FormWindowState.Maximized){form.WindowState=FormWindowState.Normal;Pump();normal=form.Bounds;}
                form.Close();Pump();
            }
            float scale=form.UiScale;
            File.WriteAllLines(Path.Combine(directory,action+".txt"),new[]{"bounds="+Rect(startupBounds),"normal="+Rect(normal),"work="+Rect(work),"state="+startupState,"icon="+startupIcon,"scale="+scale.ToString(CultureInfo.InvariantCulture),"theme="+UiSettings.Theme,"format="+UiSettings.Format,"folder="+UiSettings.Folder,"beside="+UiSettings.Beside},new UTF8Encoding(false));
        }
        return 0;
    }
}
