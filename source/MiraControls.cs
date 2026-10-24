using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Fb2Kindle
{
    static class MiraTheme
    {
        public static bool Dark;
        public static event Action Changed;
        static Color Hex(string color) { return ColorTranslator.FromHtml(color); }
        public static Color Background { get { return Hex(Dark ? "#11191F" : "#F3F7F5"); } }
        public static Color Surface { get { return Hex(Dark ? "#1B252C" : "#FEFFFF"); } }
        public static Color Soft { get { return Hex(Dark ? "#202E35" : "#F6F9F7"); } }
        public static Color Text { get { return Hex(Dark ? "#E7EEEB" : "#202E2B"); } }
        public static Color Muted { get { return Hex(Dark ? "#A3B5AD" : "#65766E"); } }
        public static Color Border { get { return Hex(Dark ? "#34444A" : "#DCE6DF"); } }
        public static Color Accent { get { return Hex(Dark ? "#78E0D1" : "#16856D"); } }
        public static Color AccentText { get { return Hex(Dark ? "#102B25" : "#FFFFFF"); } }
        public static Color Tint { get { return Hex(Dark ? "#213D39" : "#E5F3ED"); } }
        public static Color Track { get { return Hex(Dark ? "#35464B" : "#E1EAE4"); } }
        public static Color Error { get { return Hex(Dark ? "#FFB4A6" : "#AD3329"); } }
        public static void Set(bool dark) { Dark = dark; if (Changed != null) Changed(); }
        public static bool SystemDark()
        {
            try { using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")) return key != null && Convert.ToInt32(key.GetValue("AppsUseLightTheme", 1)) == 0; }
            catch { return false; }
        }
        public static GraphicsPath Round(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (d <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90); path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90); path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path;
        }
        public static void Panel(Graphics g, Rectangle bounds, Color fill, Color border, int radius)
        {
            if (bounds.Width < 2 || bounds.Height < 2) return;
            using (GraphicsPath path = Round(new RectangleF(bounds.X + .5f, bounds.Y + .5f, bounds.Width - 1, bounds.Height - 1), radius))
            using (SolidBrush brush = new SolidBrush(fill)) { g.FillPath(brush, path); using (Pen pen = new Pen(border)) g.DrawPath(pen, path); }
        }
        public static void TextAt(Graphics g, string text, Font font, Rectangle bounds, Color color, bool center)
        {
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            if (center) flags |= TextFormatFlags.HorizontalCenter;
            TextRenderer.DrawText(g, text ?? "", font, bounds, color, flags);
        }
        public static void TrackBar(Graphics g, Rectangle bounds, int percent, Color color)
        {
            if (bounds.Width < 2 || bounds.Height < 2) return;
            using (GraphicsPath path = Round(bounds, bounds.Height / 2f)) using (SolidBrush brush = new SolidBrush(Track)) g.FillPath(brush, path);
            if (percent <= 0) return;
            Rectangle mark = new Rectangle(bounds.X, bounds.Y, Math.Max(bounds.Height, (int)Math.Round(bounds.Width * Math.Min(100, percent) / 100.0)), bounds.Height);
            using (GraphicsPath path = Round(mark, bounds.Height / 2f)) using (SolidBrush brush = new SolidBrush(color)) g.FillPath(brush, path);
        }
    }

    static class MiraIcons
    {
        public static void Draw(Graphics g, string icon, Rectangle box, Color color)
        {
            if (String.IsNullOrEmpty(icon)) return;
            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias; g.TranslateTransform(box.X, box.Y); g.ScaleTransform(box.Width / 24f, box.Height / 24f);
            using (Pen p = new Pen(color, 1.6f))
            {
                p.StartCap = p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
                switch (icon)
                {
                    case "minus": g.DrawLine(p, 5, 12, 19, 12); break;
                    case "square": g.DrawRectangle(p, 5, 5, 14, 14); break;
                    case "restore": g.DrawLines(p, new PointF[] { new PointF(9,5),new PointF(19,5),new PointF(19,15) }); g.DrawRectangle(p,5,9,10,10); break;
                    case "x": g.DrawLine(p,6,6,18,18); g.DrawLine(p,18,6,6,18); break;
                    case "plus": g.DrawLine(p,5,12,19,12); g.DrawLine(p,12,5,12,19); break;
                    case "play": g.DrawPolygon(p,new PointF[]{new PointF(8,5),new PointF(19,12),new PointF(8,19)}); break;
                    case "stop": g.DrawRectangle(p,6,6,12,12); break;
                    case "check": g.DrawLines(p,new PointF[]{new PointF(5,12),new PointF(10,17),new PointF(20,6)}); break;
                    case "info": g.DrawEllipse(p,3,3,18,18);g.DrawLine(p,12,11,12,17);g.DrawLine(p,12,7,12,7.3f);break;
                    case "theme": g.DrawArc(p,6,6,12,12,270,180);g.DrawLine(p,12,6,12,18);g.DrawEllipse(p,7,7,10,10);g.DrawLine(p,12,1,12,3);g.DrawLine(p,12,21,12,23);g.DrawLine(p,1,12,3,12);g.DrawLine(p,21,12,23,12);break;
                    case "folder": g.DrawLines(p,new PointF[]{new PointF(3,19),new PointF(3,5),new PointF(9,5),new PointF(11,8),new PointF(21,8),new PointF(21,19),new PointF(3,19)});break;
                    case "folder-open": g.DrawLines(p,new PointF[]{new PointF(3,18),new PointF(3,5),new PointF(9,5),new PointF(11,8),new PointF(20,8),new PointF(20,10)});g.DrawPolygon(p,new PointF[]{new PointF(6,11),new PointF(23,11),new PointF(19,19),new PointF(3,19)});break;
                    case "ellipsis": for(int x=5;x<=19;x+=7)g.DrawEllipse(p,x-1,11,2,2);break;
                    case "trash":g.DrawLine(p,5,6,19,6);g.DrawLine(p,9,3,15,3);g.DrawLines(p,new PointF[]{new PointF(7,6),new PointF(8,21),new PointF(16,21),new PointF(17,6)});g.DrawLine(p,10,10,10,17);g.DrawLine(p,14,10,14,17);break;
                    case "upload":g.DrawLines(p,new PointF[]{new PointF(5,15),new PointF(5,20),new PointF(19,20),new PointF(19,15)});g.DrawLine(p,12,16,12,3);g.DrawLines(p,new PointF[]{new PointF(7,8),new PointF(12,3),new PointF(17,8)});break;
                    case "book":g.DrawLines(p,new PointF[]{new PointF(12,5),new PointF(8,3),new PointF(3,3),new PointF(3,19),new PointF(8,19),new PointF(12,21),new PointF(16,19),new PointF(21,19),new PointF(21,3),new PointF(16,3),new PointF(12,5),new PointF(12,21)});break;
                    case "file":g.DrawLines(p,new PointF[]{new PointF(6,2),new PointF(14,2),new PointF(19,7),new PointF(19,22),new PointF(5,22),new PointF(5,2),new PointF(6,2)});g.DrawLines(p,new PointF[]{new PointF(14,2),new PointF(14,7),new PointF(19,7)});g.DrawLine(p,8,11,16,11);g.DrawLine(p,8,15,16,15);g.DrawLine(p,8,18,13,18);break;
                }
            }
            g.Restore(state);
        }
    }

    class MiraButton : Button
    {
        public string IconName;
        public bool Primary, Quiet, Selected, Dashed, Segment;
        public float UiScale = 1;
        bool hover, down;
        public MiraButton() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true); FlatStyle = FlatStyle.Flat; Cursor = Cursors.Hand; UseVisualStyleBackColor = false; BackColor=MiraTheme.Background; TabStop = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = Segment ? (Selected ? MiraTheme.Surface : hover ? MiraTheme.Background : MiraTheme.Soft) : Primary ? MiraTheme.Accent : Selected ? MiraTheme.Tint : hover ? MiraTheme.Soft : Quiet ? BackColor : Dashed ? MiraTheme.Background : MiraTheme.Surface;
            if (!Enabled) bg = MiraTheme.Soft;
            Color fg = !Enabled ? MiraTheme.Muted : Segment ? (Selected ? MiraTheme.Text : MiraTheme.Muted) : Primary ? MiraTheme.AccentText : Selected ? MiraTheme.Accent : ForeColor;
            e.Graphics.Clear(BackColor);
            Rectangle bounds = new Rectangle(0,0,Width,Height);
            if (Dashed)
            {
                using(GraphicsPath path=MiraTheme.Round(new RectangleF(.5f,.5f,Width-1,Height-1),8*UiScale))using(Pen p=new Pen(hover?MiraTheme.Accent:MiraTheme.Border)){p.DashStyle=DashStyle.Dash;e.Graphics.DrawPath(p,path);}
            }
            else if (!Quiet || hover || Selected || Primary) MiraTheme.Panel(e.Graphics,bounds,bg,Segment?bg:Primary?bg:Selected?MiraTheme.Accent:Quiet?bg:MiraTheme.Border,(int)(8*UiScale));
            int iconSize = (int)Math.Round((IconName == "minus" || IconName == "square" || IconName == "restore" || IconName == "x" ? 20 : 17)*UiScale);
            int textWidth = String.IsNullOrEmpty(Text) ? 0 : TextRenderer.MeasureText(Text,Font,new Size(Int32.MaxValue,Int32.MaxValue),TextFormatFlags.NoPadding).Width;
            int gap = textWidth>0 && IconName!=null ? (int)(7*UiScale):0;
            int whole = textWidth + (IconName==null?0:iconSize) + gap;
            int start = Math.Max((int)(6*UiScale),(Width-whole)/2);
            if (IconName != null) MiraIcons.Draw(e.Graphics,IconName,new Rectangle(textWidth==0?(Width-iconSize)/2:start,(Height-iconSize)/2,iconSize,iconSize),fg);
            if (textWidth>0) MiraTheme.TextAt(e.Graphics,Text,Font,new Rectangle(IconName==null?5:start+iconSize+gap,0,IconName==null?Width-10:Math.Max(1,Width-start-iconSize-gap-6),Height),fg,IconName==null);
            if (down && Enabled) using(SolidBrush b=new SolidBrush(Color.FromArgb(20,MiraTheme.Text)))e.Graphics.FillRectangle(b,2,2,Width-4,Height-4);
            if(Focused&&ShowFocusCues)using(Pen p=new Pen(MiraTheme.Accent,1)){p.DashStyle=DashStyle.Dot;e.Graphics.DrawRectangle(p,4,4,Width-9,Height-9);}
        }
        protected override void OnMouseEnter(EventArgs e){hover=true;base.OnMouseEnter(e);Invalidate();}
        protected override void OnMouseLeave(EventArgs e){hover=false;down=false;base.OnMouseLeave(e);Invalidate();}
        protected override void OnMouseDown(MouseEventArgs e){down=true;base.OnMouseDown(e);Invalidate();}
        protected override void OnMouseUp(MouseEventArgs e){down=false;base.OnMouseUp(e);Invalidate();}
        protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Space)down=true;base.OnKeyDown(e);Invalidate();}
        protected override void OnKeyUp(KeyEventArgs e){down=false;base.OnKeyUp(e);Invalidate();}
        protected override void OnEnabledChanged(EventArgs e){base.OnEnabledChanged(e);Invalidate();}
        protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
        protected override void OnLostFocus(EventArgs e){down=false;base.OnLostFocus(e);Invalidate();}
    }

    class MiraFormatButton : MiraButton
    {
        public string Subtitle;
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(Parent.BackColor);
            MiraTheme.Panel(e.Graphics,new Rectangle(0,0,Width,Height),Selected?MiraTheme.Tint:MiraTheme.Surface,Selected?MiraTheme.Accent:MiraTheme.Border,(int)(9*UiScale));
            int left=(int)(17*UiScale),circle=(int)(15*UiScale),textLeft=left+circle+(int)(11*UiScale);
            int block=(int)(44*UiScale),top=(Height-block)/2;
            using(Pen p=new Pen(!Enabled?MiraTheme.Muted:Selected?MiraTheme.Accent:MiraTheme.Muted,1.4f*UiScale))e.Graphics.DrawEllipse(p,left,top+(int)(5*UiScale),circle,circle);
            if(Selected)using(SolidBrush b=new SolidBrush(MiraTheme.Accent))e.Graphics.FillEllipse(b,left+3*UiScale,top+8*UiScale,circle-6*UiScale,circle-6*UiScale);
            MiraTheme.TextAt(e.Graphics,Text,Font,new Rectangle(textLeft,top,Width-textLeft-10,(int)(23*UiScale)),Enabled?MiraTheme.Text:MiraTheme.Muted,false);
            using(Font small=new Font(Font.FontFamily,9,FontStyle.Regular))MiraTheme.TextAt(e.Graphics,Subtitle,small,new Rectangle(textLeft,top+(int)(23*UiScale),Width-textLeft-10,(int)(20*UiScale)),MiraTheme.Muted,false);
            if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(5,5,Width-10,Height-10),MiraTheme.Accent,MiraTheme.Surface);
        }
    }

    class MiraPathPanel : Panel
    {
        public MiraPathPanel(){DoubleBuffered=true;}
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;MiraTheme.Panel(e.Graphics,new Rectangle(0,0,Width,Height),MiraTheme.Surface,MiraTheme.Border,8);MiraIcons.Draw(e.Graphics,"folder",new Rectangle(12,(Height-18)/2,18,18),MiraTheme.Muted);}
    }
    class MiraSegmentPanel : Panel
    {
        public MiraSegmentPanel(){DoubleBuffered=true;}
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(MiraTheme.Background);MiraTheme.Panel(e.Graphics,new Rectangle(0,0,Width,Height),MiraTheme.Soft,MiraTheme.Border,9);}
    }

    class MiraProgress : Control
    {
        public int Percent;
        public MiraProgress(){DoubleBuffered=true;TabStop=false;}
        protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;MiraTheme.TrackBar(e.Graphics,new Rectangle(0,0,Width,Height),Percent,MiraTheme.Accent);}
    }

    sealed class MiraMenuColors:ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground{get{return MiraTheme.Surface;}}
        public override Color MenuBorder{get{return MiraTheme.Border;}}
        public override Color MenuItemBorder{get{return MiraTheme.Tint;}}
        public override Color MenuItemSelected{get{return MiraTheme.Tint;}}
        public override Color MenuItemSelectedGradientBegin{get{return MiraTheme.Tint;}}
        public override Color MenuItemSelectedGradientEnd{get{return MiraTheme.Tint;}}
        public override Color MenuItemPressedGradientBegin{get{return MiraTheme.Tint;}}
        public override Color MenuItemPressedGradientEnd{get{return MiraTheme.Tint;}}
        public override Color ImageMarginGradientBegin{get{return MiraTheme.Surface;}}
        public override Color ImageMarginGradientMiddle{get{return MiraTheme.Surface;}}
        public override Color ImageMarginGradientEnd{get{return MiraTheme.Surface;}}
        public override Color CheckBackground{get{return MiraTheme.Tint;}}
        public override Color CheckSelectedBackground{get{return MiraTheme.Tint;}}
        public override Color CheckPressedBackground{get{return MiraTheme.Tint;}}
    }
    static class MiraMenus
    {
        // Menus are owned by their form. Closed fires inside the native item-click
        // stack, so disposing here would invalidate ToolStrip's remaining work.
        public static MiraContextMenu Create(){return new MiraContextMenu{Renderer=new ToolStripProfessionalRenderer(new MiraMenuColors()),BackColor=MiraTheme.Surface,ForeColor=MiraTheme.Text,Font=new Font("Segoe UI",10),ShowImageMargin=false,ShowCheckMargin=true};}
    }
    sealed class MiraContextMenu:ContextMenuStrip
    {
        internal bool VerifyEnter(){return base.ProcessDialogKey(Keys.Enter);}
    }

    public class MiraWindow : Form
    {
        public float UiScale=1;
        public MiraWindow(){FormBorderStyle=FormBorderStyle.None;AutoScaleMode=AutoScaleMode.None;DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);}
        protected override CreateParams CreateParams {get {CreateParams cp=base.CreateParams;cp.Style|=0x00020000;return cp;}}
        protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);int value=2;try{DwmSetWindowAttribute(Handle,33,ref value,4);}catch{} }
        public void DragWindow(){ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}
        public static Point DecodeHitPoint(IntPtr packedValue){long packed=packedValue.ToInt64();return new Point(unchecked((short)(packed&0xffff)),unchecked((short)((packed>>16)&0xffff)));}
        protected override void WndProc(ref Message m)
        {
            if(m.Msg==0x84&&WindowState!=FormWindowState.Maximized)
            {
                Point p=PointToClient(DecodeHitPoint(m.LParam));
                int edge=Math.Max(5,(int)(6*UiScale));bool l=p.X<edge,r=p.X>=Width-edge,t=p.Y<edge,b=p.Y>=Height-edge;
                if(l||r||t||b){m.Result=(IntPtr)(t?(l?13:r?14:12):b?(l?16:r?17:15):l?10:11);return;}
            }
            base.WndProc(ref m);
        }
        [DllImport("user32.dll")]static extern bool ReleaseCapture();
        [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
        [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr h,int attr,ref int val,int size);
    }

    static class UiSettings
    {
        public static bool Isolated;
        public static string Theme="system",Format="AZW3",Folder="";
        public static bool Beside=true;
        static string PathName { get {return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"FB2Kindle","ui.txt");}}
        public static void Load(){if(Isolated)return;try{foreach(string line in File.ReadAllLines(PathName,Encoding.UTF8)){int split=line.IndexOf('=');if(split<0)continue;string key=line.Substring(0,split),val=line.Substring(split+1);if(key=="theme"&&(val=="dark"||val=="light"||val=="system"))Theme=val;else if(key=="format"&&(val=="AZW3"||val=="EPUB"))Format=val;else if(key=="folder")Folder=val;else if(key=="beside")Beside=val=="1";}}catch(IOException){}catch(UnauthorizedAccessException){} }
        public static void Save(){if(Isolated)return;try{Directory.CreateDirectory(Path.GetDirectoryName(PathName));File.WriteAllLines(PathName,new[]{"theme="+Theme,"format="+Format,"folder="+Folder,"beside="+(Beside?"1":"0")},new UTF8Encoding(false));}catch(IOException){}catch(UnauthorizedAccessException){} }
    }
}
