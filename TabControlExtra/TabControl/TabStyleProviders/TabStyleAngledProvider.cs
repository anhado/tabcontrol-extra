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
    public class TabStyleAngledProvider : TabStyleDefaultProvider
    {
        public TabStyleAngledProvider(TabControlExtra tabControl) : base(tabControl)
        {
            ImageAlign = ContentAlignment.MiddleRight;
            Overlap    = 7;
            Radius     = 10;

            //	Must set after the _Radius as this is used in the calculations of the actual padding
            Padding = new Point(10, 3);
        }

        public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
        {
            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X + Radius - 2,      tabBounds.Y                  + 2);
                    path.AddLine(tabBounds.X                                         + Radius, tabBounds.Y, tabBounds.Right - Radius, tabBounds.Y);
                    path.AddLine(tabBounds.Right - Radius                            + 2,      tabBounds.Y                  + 2,      tabBounds.Right, tabBounds.Bottom);
                    break;
                case TabAlignment.Bottom:
                    path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right - Radius + 2,      tabBounds.Bottom              - 2);
                    path.AddLine(tabBounds.Right                                        - Radius, tabBounds.Bottom, tabBounds.X + Radius, tabBounds.Bottom);
                    path.AddLine(tabBounds.X + Radius                                   - 2,      tabBounds.Bottom              - 2,      tabBounds.X, tabBounds.Y);
                    break;
                case TabAlignment.Left:
                    path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X + 2,      tabBounds.Bottom - Radius + 2);
                    path.AddLine(tabBounds.X,     tabBounds.Bottom              - Radius, tabBounds.X, tabBounds.Y  + Radius);
                    path.AddLine(tabBounds.X                                    + 2,      tabBounds.Y + Radius      - 2, tabBounds.Right, tabBounds.Y);
                    break;
                case TabAlignment.Right:
                    path.AddLine(tabBounds.X,     tabBounds.Y, tabBounds.Right - 2,      tabBounds.Y + Radius              - 2);
                    path.AddLine(tabBounds.Right, tabBounds.Y                  + Radius, tabBounds.Right, tabBounds.Bottom - Radius);
                    path.AddLine(tabBounds.Right                               - 2,      tabBounds.Bottom - Radius         + 2, tabBounds.X, tabBounds.Bottom);
                    break;
            }
        }
    }
}