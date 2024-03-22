/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TradeWright.UI.Forms
{
    [ToolboxItem(false)]
    public class TabStyleChromeProvider : TabStyleProvider
    {
        public TabStyleChromeProvider(TabControlExtra tabControl) : base(tabControl)
        {
            Overlap = 16;
            //this.Radius = 16;
            ShowTabCloser = true;

            CloserColorFocused           = Color.Black;
            CloserColorFocusedActive     = Color.White;
            CloserColorSelected          = Color.Black;
            CloserColorSelectedActive    = Color.White;
            CloserColorHighlighted       = Color.Black;
            CloserColorHighlightedActive = Color.White;
            CloserColorUnselected        = Color.Empty;

            CloserButtonFillColorFocused           = Color.Empty;
            CloserButtonFillColorFocusedActive     = Color.FromArgb(244, 159, 148);
            CloserButtonFillColorSelected          = Color.Empty;
            CloserButtonFillColorSelectedActive    = Color.FromArgb(244, 159, 148);
            CloserButtonFillColorHighlighted       = Color.Empty;
            CloserButtonFillColorHighlightedActive = Color.FromArgb(244, 159, 148);
            CloserButtonFillColorUnselected        = Color.Empty;

            CloserButtonOutlineColorFocused           = Color.Empty;
            CloserButtonOutlineColorFocusedActive     = Color.FromArgb(209, 106, 94);
            CloserButtonOutlineColorSelected          = Color.Empty;
            CloserButtonOutlineColorSelectedActive    = Color.FromArgb(209, 106, 94);
            CloserButtonOutlineColorHighlighted       = Color.Empty;
            CloserButtonOutlineColorHighlightedActive = Color.FromArgb(209, 106, 94);
            CloserButtonOutlineColorUnselected        = Color.Empty;


            //	Must set after the _Radius as this is used in the calculations of the actual padding
            Padding = new Point(16, 5);
        }

        public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
        {
            int spread;
            int eigth;
            int sixth;
            int quarter;

            if (TabControl.Alignment <= TabAlignment.Bottom)
            {
                spread  = (int)Math.Floor((decimal)tabBounds.Height * 2 / 3);
                eigth   = (int)Math.Floor((decimal)tabBounds.Height * 1 / 8);
                sixth   = (int)Math.Floor((decimal)tabBounds.Height * 1 / 6);
                quarter = (int)Math.Floor((decimal)tabBounds.Height * 1 / 4);
            }
            else
            {
                spread  = (int)Math.Floor((decimal)tabBounds.Width * 2 / 3);
                eigth   = (int)Math.Floor((decimal)tabBounds.Width * 1 / 8);
                sixth   = (int)Math.Floor((decimal)tabBounds.Width * 1 / 6);
                quarter = (int)Math.Floor((decimal)tabBounds.Width * 1 / 4);
            }

            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:

                    path.AddCurve(new[] { new Point(tabBounds.X, tabBounds.Bottom), new Point(tabBounds.X + sixth, tabBounds.Bottom - eigth), new Point(tabBounds.X + spread - quarter, tabBounds.Y + eigth), new Point(tabBounds.X + spread, tabBounds.Y) });
                    path.AddLine(tabBounds.X + spread, tabBounds.Y, tabBounds.Right - spread, tabBounds.Y);
                    path.AddCurve(new[] { new Point(tabBounds.Right - spread, tabBounds.Y), new Point(tabBounds.Right - spread + quarter, tabBounds.Y + eigth), new Point(tabBounds.Right - sixth, tabBounds.Bottom - eigth), new Point(tabBounds.Right, tabBounds.Bottom) });
                    break;
                case TabAlignment.Bottom:
                    path.AddCurve(new[] { new Point(tabBounds.Right, tabBounds.Y), new Point(tabBounds.Right - sixth, tabBounds.Y + eigth), new Point(tabBounds.Right - spread + quarter, tabBounds.Bottom - eigth), new Point(tabBounds.Right - spread, tabBounds.Bottom) });
                    path.AddLine(tabBounds.Right - spread, tabBounds.Bottom, tabBounds.X + spread, tabBounds.Bottom);
                    path.AddCurve(new[] { new Point(tabBounds.X + spread, tabBounds.Bottom), new Point(tabBounds.X + spread - quarter, tabBounds.Bottom - eigth), new Point(tabBounds.X + sixth, tabBounds.Y + eigth), new Point(tabBounds.X, tabBounds.Y) });
                    break;
                case TabAlignment.Left:
                    path.AddCurve(new[] { new Point(tabBounds.Right, tabBounds.Bottom), new Point(tabBounds.Right - eigth, tabBounds.Bottom - sixth), new Point(tabBounds.X + eigth, tabBounds.Bottom - spread + quarter), new Point(tabBounds.X, tabBounds.Bottom - spread) });
                    path.AddLine(tabBounds.X, tabBounds.Bottom - spread, tabBounds.X, tabBounds.Y + spread);
                    path.AddCurve(new[] { new Point(tabBounds.X, tabBounds.Y + spread), new Point(tabBounds.X + eigth, tabBounds.Y + spread - quarter), new Point(tabBounds.Right - eigth, tabBounds.Y + sixth), new Point(tabBounds.Right, tabBounds.Y) });

                    break;
                case TabAlignment.Right:
                    path.AddCurve(new[] { new Point(tabBounds.X, tabBounds.Y), new Point(tabBounds.X + eigth, tabBounds.Y + sixth), new Point(tabBounds.Right - eigth, tabBounds.Y + spread - quarter), new Point(tabBounds.Right, tabBounds.Y + spread) });
                    path.AddLine(tabBounds.Right, tabBounds.Y + spread, tabBounds.Right, tabBounds.Bottom - spread);
                    path.AddCurve(new[] { new Point(tabBounds.Right, tabBounds.Bottom - spread), new Point(tabBounds.Right - eigth, tabBounds.Bottom - spread + quarter), new Point(tabBounds.X + eigth, tabBounds.Bottom - sixth), new Point(tabBounds.X, tabBounds.Bottom) });
                    break;
            }
        }

        protected internal override GraphicsPath GetTabCloserPath(Rectangle closerButtonRect)
        {
            GraphicsPath closerPath = new GraphicsPath();
            closerPath.AddLine(closerButtonRect.X + 4, closerButtonRect.Y + 4, closerButtonRect.Right - 4, closerButtonRect.Bottom - 4);
            closerPath.CloseFigure();
            closerPath.AddLine(closerButtonRect.Right - 4, closerButtonRect.Y + 4, closerButtonRect.X + 4, closerButtonRect.Bottom - 4);
            closerPath.CloseFigure();

            return closerPath;
        }

        protected internal override GraphicsPath GetTabCloserButtonPath(Rectangle closerButtonRect)
        {
            GraphicsPath closerPath = new GraphicsPath();
            closerPath.AddEllipse(new Rectangle(closerButtonRect.X, closerButtonRect.Y, closerButtonRect.Width, closerButtonRect.Height));
            closerPath.CloseFigure();
            return closerPath;
        }
    }
}