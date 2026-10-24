using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Fb2Kindle
{
    enum BookState { Waiting, Working, Done, Warning, Error, Stopped }
    sealed class BookItem
    {
        public string Source, Output, Detail, Author, Title;
        public ArchiveBook Entry;
        public long Size;
        public int Percent;
        public BookState State;
        public string StageKey="Ожидает своей очереди",ResultFormat;
        public bool Processed;
        public string FileName { get {if(Entry==null)return Path.GetFileName(Source);string n=Entry.EntryName.Replace('\\','/');return n.Substring(n.LastIndexOf('/')+1);} }
        public string SourceDescription {get {return Source+(Entry==null?"":"\r\n"+L10n.T("Книга в архиве: ")+Entry.EntryName);}}
        public string Subtitle {get {string source=Entry==null?L10n.T("Отдельный файл"):Path.GetFileName(Source);return String.IsNullOrEmpty(Author)?source:Author+" · "+source;}}
        public string StatusText {get {if(State==BookState.Done)return L10n.T("Готово")+" · "+ResultFormat;if(State==BookState.Warning)return L10n.T("Готово · замечания")+" · "+ResultFormat;return L10n.T(StageKey);}}
        public string AllDetails {get {return String.IsNullOrEmpty(Detail)?SourceDescription:Detail;}}
    }
    sealed class MiraQueue : ScrollableControl
    {
        public readonly List<BookItem> Books=new List<BookItem>();
        public readonly HashSet<int> Selected=new HashSet<int>();
        public float UiScale=1;
        public event Action SelectionChanged,DeleteRequested,DetailsRequested;
        readonly ToolTip tips=new ToolTip {AutoPopDelay=20000,InitialDelay=500,ReshowDelay=100};
        int anchor,focused,hover=-1;
        public MiraQueue(){DoubleBuffered=true;AutoScroll=true;TabStop=true;SetStyle(ControlStyles.Selectable|ControlStyles.ResizeRedraw,true);AccessibleRole=AccessibleRole.List;}
        int RowHeight {get {int visible=Math.Min(7,Math.Max(1,Books.Count));return Math.Min((int)Math.Round(96*UiScale),Math.Max((int)Math.Round(64*UiScale),ClientSize.Height/visible));}}
        public void EnsureBookVisible(int index){if(index<0||index>=Books.Count)return;int top=-AutoScrollPosition.Y,y=index*RowHeight;int target=top;if(y<top)target=y;else if(y+RowHeight>top+ClientSize.Height)target=y+RowHeight-ClientSize.Height;AutoScrollPosition=new Point(0,Math.Max(0,target));Invalidate();}
        public string VerifyNavigation()
        {
            if(Books.Count<10)throw new InvalidOperationException("Navigation fixture needs at least ten rows");
            EnsureBookVisible(8);int offset=-AutoScrollPosition.Y;if(offset<=0||8*RowHeight<offset||9*RowHeight>offset+ClientSize.Height)throw new InvalidOperationException("Active row is not visible");
            OnKeyDown(new KeyEventArgs(Keys.End));if(!Selected.Contains(Books.Count-1)||AutoScrollPosition.Y>=0)throw new InvalidOperationException("End key did not select and reveal the final row");
            OnKeyDown(new KeyEventArgs(Keys.Home));if(!Selected.Contains(0)||AutoScrollPosition.Y!=0)throw new InvalidOperationException("Home key did not return to first row");
            OnMouseWheel(new MouseEventArgs(MouseButtons.None,0,20,20,-120));if(AutoScrollPosition.Y>=0)throw new InvalidOperationException("Wheel did not scroll queue");
            return "autoScrollRow9=PASS\r\nkeyboardHomeEnd=PASS\r\nwheel=PASS";
        }
        public void RefreshBooks(){AutoScrollMinSize=new Size(0,Books.Count*RowHeight);List<int> invalid=new List<int>();foreach(int i in Selected)if(i>=Books.Count)invalid.Add(i);foreach(int i in invalid)Selected.Remove(i);focused=Math.Max(0,Math.Min(focused,Books.Count-1));Invalidate();}
        public BookItem FirstSelected {get {foreach(int i in Selected)if(i>=0&&i<Books.Count)return Books[i];return null;}}
        public static string FormatBytes(long bytes){return bytes>=1048576?(bytes/1048576.0).ToString("0.0")+" "+L10n.T("МБ"):Math.Max(1,bytes/1024).ToString()+" "+L10n.T("КБ");}
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);Graphics g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(MiraTheme.Background);
            Rectangle outer=new Rectangle(0,0,ClientSize.Width,ClientSize.Height);MiraTheme.Panel(g,outer,MiraTheme.Surface,MiraTheme.Border,(int)(11*UiScale));
            if(Books.Count==0)
            {
                int icon=(int)(27*UiScale);MiraIcons.Draw(g,"book",new Rectangle((ClientSize.Width-icon)/2,Math.Max(20,Height/2-(int)(52*UiScale)),icon,icon),MiraTheme.Muted);
                using(Font f=new Font("Segoe UI",11))MiraTheme.TextAt(g,L10n.T("Добавьте книги или архивы"),f,new Rectangle(16,Height/2-(int)(12*UiScale),Math.Max(1,Width-32),(int)(30*UiScale)),MiraTheme.Text,true);
                using(Font f=new Font("Segoe UI",9))MiraTheme.TextAt(g,"FB2 · ZIP · RAR · 7Z",f,new Rectangle(16,Height/2+(int)(20*UiScale),Math.Max(1,Width-32),(int)(24*UiScale)),MiraTheme.Muted,true);
                return;
            }
            GraphicsState saved=g.Save();using(GraphicsPath clip=MiraTheme.Round(new RectangleF(1,1,ClientSize.Width-2,ClientSize.Height-2),10*UiScale))g.SetClip(clip);
            int width=ClientSize.Width,offset=AutoScrollPosition.Y;
            int start=Math.Max(0,-offset/RowHeight),end=Math.Min(Books.Count-1,(Height-offset)/RowHeight);
            using(Font title=new Font("Segoe UI Semibold",10))using(Font small=new Font("Segoe UI",9))
            {
                for(int i=start;i<=end;i++)
                {
                    BookItem book=Books[i];int y=i*RowHeight+offset;int extra=RowHeight-(int)(64*UiScale),nameY=(int)(5*UiScale+extra*.2),sourceY=(int)(24*UiScale+extra*.34),stageY=(int)(40*UiScale+extra*.47);
                    Rectangle row=new Rectangle(0,y,width,RowHeight);
                    bool active=book.State==BookState.Working;
                    using(SolidBrush b=new SolidBrush(active?MiraTheme.Tint:MiraTheme.Surface))g.FillRectangle(b,row);
                    if(Selected.Contains(i))using(SolidBrush b=new SolidBrush(MiraTheme.Accent))g.FillRectangle(b,1,y,(int)Math.Max(3,3*UiScale),RowHeight);
                    int px=(int)(17*UiScale),iconW=(int)(31*UiScale),textX=(int)(62*UiScale),right=(int)(18*UiScale);
                    MiraTheme.Panel(g,new Rectangle(px,y+(RowHeight-(int)(40*UiScale))/2,iconW,(int)(40*UiScale)),MiraTheme.Soft,MiraTheme.Border,(int)(5*UiScale));
                    MiraIcons.Draw(g,"file",new Rectangle(px+(int)(7*UiScale),y+(RowHeight-(int)(22*UiScale))/2,(int)(17*UiScale),(int)(22*UiScale)),active?MiraTheme.Accent:MiraTheme.Muted);
                    int available=Math.Max(30,width-textX-right);
                    MiraTheme.TextAt(g,book.FileName,title,new Rectangle(textX,y+nameY,Math.Max(20,available-(int)(90*UiScale)),(int)(20*UiScale)),MiraTheme.Text,false);
                    MiraTheme.TextAt(g,FormatBytes(book.Size),small,new Rectangle(width-(int)(108*UiScale),y+nameY,(int)(90*UiScale),(int)(20*UiScale)),MiraTheme.Muted,true);
                    MiraTheme.TextAt(g,book.Subtitle,small,new Rectangle(textX,y+sourceY,available,(int)(16*UiScale)),MiraTheme.Muted,false);
                    Color color=book.State==BookState.Error?MiraTheme.Error:active||book.State==BookState.Done||book.State==BookState.Warning?MiraTheme.Accent:MiraTheme.Muted;
                    string status=book.StatusText;int statusX=textX;
                    if(book.State==BookState.Done||book.State==BookState.Warning){MiraIcons.Draw(g,"check",new Rectangle(textX,y+stageY+(int)(2*UiScale),(int)(13*UiScale),(int)(13*UiScale)),color);statusX+=(int)(18*UiScale);}
                    MiraTheme.TextAt(g,status,small,new Rectangle(statusX,y+stageY,Math.Max(20,width-statusX-(int)(76*UiScale)),(int)(16*UiScale)),color,false);
                    MiraTheme.TextAt(g,book.Percent+"%",small,new Rectangle(width-(int)(68*UiScale),y+stageY,(int)(50*UiScale),(int)(16*UiScale)),MiraTheme.Muted,true);
                    Color bar=book.State==BookState.Error?MiraTheme.Error:book.State==BookState.Done||book.State==BookState.Warning?Color.FromArgb(130,MiraTheme.Accent):MiraTheme.Accent;
                    MiraTheme.TrackBar(g,new Rectangle(textX,y+RowHeight-(int)(6*UiScale),available,(int)Math.Max(3,4*UiScale)),book.Percent,bar);
                    using(Pen line=new Pen(MiraTheme.Border))g.DrawLine(line,0,y+RowHeight-1,width,y+RowHeight-1);
                    if(Focused&&i==focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,y+4,width-10,RowHeight-8),MiraTheme.Accent,MiraTheme.Surface);
                }
            }
            g.Restore(saved);
        }
        int IndexAt(int y){return (y-AutoScrollPosition.Y)/RowHeight;}
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();int i=IndexAt(e.Y);if(i<0||i>=Books.Count)return;SelectIndex(i,(ModifierKeys&Keys.Control)!=0,(ModifierKeys&Keys.Shift)!=0);}
        void SelectIndex(int i,bool ctrl,bool shift){if(shift){Selected.Clear();for(int k=Math.Min(anchor,i);k<=Math.Max(anchor,i);k++)Selected.Add(k);}else if(ctrl){if(!Selected.Add(i))Selected.Remove(i);anchor=i;}else{Selected.Clear();Selected.Add(i);anchor=i;}focused=i;Invalidate();if(SelectionChanged!=null)SelectionChanged();AccessibilityNotifyClients(AccessibleEvents.Selection,i);}
        protected override void OnDoubleClick(EventArgs e){base.OnDoubleClick(e);if(FirstSelected!=null&&DetailsRequested!=null)DetailsRequested();}
        protected override bool IsInputKey(Keys keyData){Keys k=keyData&Keys.KeyCode;return k==Keys.Up||k==Keys.Down||k==Keys.Home||k==Keys.End||base.IsInputKey(keyData);}
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);if(e.KeyCode==Keys.Delete){if(DeleteRequested!=null)DeleteRequested();e.Handled=true;return;}if(e.KeyCode==Keys.Enter&&DetailsRequested!=null){DetailsRequested();e.Handled=true;return;}
            if(Books.Count==0)return;
            if(e.Control&&e.KeyCode==Keys.A){Selected.Clear();for(int i=0;i<Books.Count;i++)Selected.Add(i);Invalidate();if(SelectionChanged!=null)SelectionChanged();e.Handled=true;return;}
            int target=focused;if(e.KeyCode==Keys.Up)target=Math.Max(0,focused-1);else if(e.KeyCode==Keys.Down)target=Math.Min(Books.Count-1,focused+1);else if(e.KeyCode==Keys.Home)target=0;else if(e.KeyCode==Keys.End)target=Books.Count-1;else return;
            SelectIndex(target,e.Control,e.Shift);EnsureBookVisible(target);e.Handled=true;
        }
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);int i=IndexAt(e.Y);if(i==hover)return;hover=i;tips.SetToolTip(this,i>=0&&i<Books.Count?Books[i].AllDetails:null);}
        protected override void OnScroll(ScrollEventArgs se){base.OnScroll(se);Invalidate();}
        protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
        protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
        protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
        protected override AccessibleObject CreateAccessibilityInstance(){return new QueueAccessibility(this);}
        sealed class QueueAccessibility:ControlAccessibleObject
        {
            readonly MiraQueue queue;
            public QueueAccessibility(MiraQueue q):base(q){queue=q;}
            public override int GetChildCount(){return queue.Books.Count;}
            public override AccessibleObject GetChild(int index){return index<0||index>=queue.Books.Count?null:new RowAccessibility(queue,index);}
        }
        sealed class RowAccessibility:AccessibleObject
        {
            readonly MiraQueue queue;readonly int index;
            public RowAccessibility(MiraQueue q,int i){queue=q;index=i;}
            public override string Name {get{return queue.Books[index].FileName+", "+queue.Books[index].StatusText+", "+queue.Books[index].Percent+"%";}set{}}
            public override AccessibleRole Role {get{return AccessibleRole.ListItem;}}
            public override AccessibleStates State {get{return AccessibleStates.Selectable|(queue.Selected.Contains(index)?AccessibleStates.Selected:AccessibleStates.None);}}
            public override Rectangle Bounds {get{return queue.RectangleToScreen(new Rectangle(0,index*queue.RowHeight+queue.AutoScrollPosition.Y,queue.ClientSize.Width,queue.RowHeight));}}
            public override void Select(AccessibleSelection flags){queue.SelectIndex(index,(flags&AccessibleSelection.AddSelection)!=0,false);}
            public override string Description {get{return queue.Books[index].AllDetails;}}
        }
    }
}
