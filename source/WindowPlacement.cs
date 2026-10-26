using System;
using System.Drawing;
using System.Globalization;

namespace Fb2Kindle
{
    internal sealed class WindowPlacement
    {
        public Rectangle Bounds, WorkingArea;
        public bool Maximized;

        internal static Rectangle Center(Size preferred, Rectangle work)
        {
            int width=Math.Min(preferred.Width,work.Width),height=Math.Min(preferred.Height,work.Height);
            return new Rectangle(work.Left+(work.Width-width)/2,work.Top+(work.Height-height)/2,width,height);
        }

        internal static Rectangle Resolve(Rectangle savedBounds,Rectangle savedWorkingArea,Rectangle targetWorkingArea,Size minimumSize)
        {
            int width=Math.Min(targetWorkingArea.Width,Math.Max(minimumSize.Width,savedBounds.Width));
            int height=Math.Min(targetWorkingArea.Height,Math.Max(minimumSize.Height,savedBounds.Height));
            // A disconnected monitor must not leave the window outside the desktop.
            if(!savedBounds.IntersectsWith(targetWorkingArea))return Center(new Size(width,height),targetWorkingArea);
            long x=savedBounds.X,y=savedBounds.Y;
            if(savedWorkingArea!=targetWorkingArea)
            {
                x+=(long)targetWorkingArea.Left-savedWorkingArea.Left;
                y+=(long)targetWorkingArea.Top-savedWorkingArea.Top;
            }
            x=Math.Max(targetWorkingArea.Left,Math.Min(x,(long)targetWorkingArea.Right-width));
            y=Math.Max(targetWorkingArea.Top,Math.Min(y,(long)targetWorkingArea.Bottom-height));
            return new Rectangle((int)x,(int)y,width,height);
        }

        public override string ToString()
        {
            int[] values={Bounds.X,Bounds.Y,Bounds.Width,Bounds.Height,WorkingArea.X,WorkingArea.Y,WorkingArea.Width,WorkingArea.Height,Maximized?1:0};
            return String.Join(",",Array.ConvertAll(values,delegate(int n){return n.ToString(CultureInfo.InvariantCulture);}));
        }

        internal static bool TryParse(string value,out WindowPlacement placement)
        {
            placement=null;string[] parts=value.Split(',');if(parts.Length!=9)return false;int[] n=new int[9];
            for(int i=0;i<n.Length;i++)if(!Int32.TryParse(parts[i],NumberStyles.Integer,CultureInfo.InvariantCulture,out n[i]))return false;
            // Reject broken sizes and overflowing coordinates before using Rectangle.Right/Bottom.
            if(n[2]<1||n[3]<1||n[6]<1||n[7]<1||(n[8]!=0&&n[8]!=1))return false;
            for(int i=0;i<8;i++)if(n[i]<-1000000||n[i]>1000000)return false;
            placement=new WindowPlacement{Bounds=new Rectangle(n[0],n[1],n[2],n[3]),WorkingArea=new Rectangle(n[4],n[5],n[6],n[7]),Maximized=n[8]==1};return true;
        }
    }
}
