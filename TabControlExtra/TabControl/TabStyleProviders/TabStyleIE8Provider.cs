/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TradeWright.UI.Forms
{
    [ToolboxItem(false)]
    public class TabStyleIE8Provider : TabStyleRoundedProvider
    {
        public TabStyleIE8Provider(TabControlExtra tabControl)
            : base(tabControl)
        {
            Radius              = 3;
            ShowTabCloser       = true;
            SelectedTabIsLarger = true;

            CloserColorFocusedActive = Color.Red;
            CloserColorFocused       = Color.Black;
            CloserColorSelected      = Color.Black;
            CloserColorHighlighted   = Color.Black;
            CloserColorUnselected    = Color.Empty;

            CloserButtonFillColorFocusedActive = Color.White;
            CloserButtonFillColorFocused       = Color.Empty;
            CloserButtonFillColorSelected      = Color.Empty;
            CloserButtonFillColorHighlighted   = Color.Empty;
            CloserButtonFillColorUnselected    = Color.Empty;

            CloserButtonOutlineColorFocusedActive = SystemColors.ControlDark;
            CloserButtonOutlineColorFocused       = Color.Empty;
            CloserButtonOutlineColorSelected      = Color.Empty;
            CloserButtonOutlineColorHighlighted   = Color.Empty;
            CloserButtonOutlineColorUnselected    = Color.Empty;

            PageBackgroundColorDisabled    = Color.FromArgb(247, 247, 255);
            PageBackgroundColorFocused     = Color.FromArgb(247, 247, 255);
            PageBackgroundColorHighlighted = Color.FromArgb(247, 247, 255);
            PageBackgroundColorSelected    = Color.FromArgb(247, 247, 255);
            PageBackgroundColorUnselected  = Color.FromArgb(198, 223, 255);

            TabColorFocused2     = Color.FromArgb(198, 223, 255);
            TabColorHighLighted2 = Color.FromArgb(198, 223, 255);
            TabColorSelected2    = Color.FromArgb(198, 223, 255);

            Padding = new Point(6, 5);

            TabPageMargin = new Padding(0, 4, 0, 4);
        }

        protected internal override void PaintTabBackground(GraphicsPath tabBorder, TabState state, Graphics graphics)
        {
            // first draw a white-ish line inside the tab boundary
            RectangleF tabBounds = tabBorder.GetBounds();
            switch (TabControl.Alignment)
            {
                case TabAlignment.Bottom:
                    tabBounds.X      += 1;
                    tabBounds.Width  -= 2;
                    tabBounds.Height -= 1;
                    break;
                case TabAlignment.Top:
                    tabBounds.X     += 1;
                    tabBounds.Width -= 2;
                    tabBounds.Y     += 1;
                    break;
                case TabAlignment.Left:
                    tabBounds.X      += 1;
                    tabBounds.Width  -= 1;
                    tabBounds.Y      += 1;
                    tabBounds.Height -= 2;
                    break;
                case TabAlignment.Right:
                    tabBounds.Width  -= 1;
                    tabBounds.Y      += 1;
                    tabBounds.Height -= 2;
                    break;
            }

            using (Pen pen = new Pen(Color.FromArgb(247, 247, 255)))
            {
                graphics.DrawPath(pen, tabBorder);
            }

            // now paint the tab so that the full gradient lies inside the white line
            tabBounds.X      += 1;
            tabBounds.Width  -= 2;
            tabBounds.Y      += 1;
            tabBounds.Height -= 1;

            base.PaintTabBackground(GetTabBorder(Rectangle.Round(tabBounds)), state, graphics);
        }
    }
}