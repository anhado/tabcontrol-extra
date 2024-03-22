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
    public class TabStyleRoundedProvider : TabStyleDefaultProvider
    {
        public TabStyleRoundedProvider(TabControlExtra tabControl) : base(tabControl)
        {
            Radius              = 10;
            SelectedTabIsLarger = false;

            Padding = new Point(6, 3);
        }

        public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
        {
            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y + Radius);
                    path.AddArc(tabBounds.X, tabBounds.Y, Radius * 2, Radius * 2, 180, 90);
                    path.AddLine(tabBounds.X                  + Radius, tabBounds.Y, tabBounds.Right - Radius, tabBounds.Y);
                    path.AddArc(tabBounds.Right               - Radius * 2, tabBounds.Y, Radius * 2, Radius * 2, 270, 90);
                    path.AddLine(tabBounds.Right, tabBounds.Y + Radius, tabBounds.Right, tabBounds.Bottom);
                    break;
                case TabAlignment.Bottom:
                    path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom - Radius);
                    path.AddArc(tabBounds.Right                                                  - Radius * 2, tabBounds.Bottom          - Radius * 2, Radius * 2, Radius * 2, 0, 90);
                    path.AddLine(tabBounds.Right                                                 - Radius, tabBounds.Bottom, tabBounds.X + Radius, tabBounds.Bottom);
                    path.AddArc(tabBounds.X, tabBounds.Bottom                                    - Radius * 2, Radius * 2, Radius * 2, 90, 90);
                    path.AddLine(tabBounds.X, tabBounds.Bottom                                   - Radius, tabBounds.X, tabBounds.Y);
                    break;
                case TabAlignment.Left:
                    path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X + Radius, tabBounds.Bottom);
                    path.AddArc(tabBounds.X, tabBounds.Bottom                   - Radius * 2, Radius * 2, Radius * 2, 90, 90);
                    path.AddLine(tabBounds.X, tabBounds.Bottom                  - Radius, tabBounds.X, tabBounds.Y + Radius);
                    path.AddArc(tabBounds.X, tabBounds.Y, Radius * 2, Radius * 2, 180, 90);
                    path.AddLine(tabBounds.X + Radius, tabBounds.Y, tabBounds.Right, tabBounds.Y);
                    break;
                case TabAlignment.Right:
                    path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right - Radius, tabBounds.Y);
                    path.AddArc(tabBounds.Right                            - Radius * 2, tabBounds.Y, Radius * 2, Radius * 2, 270, 90);
                    path.AddLine(tabBounds.Right, tabBounds.Y              + Radius, tabBounds.Right, tabBounds.Bottom - Radius);
                    path.AddArc(tabBounds.Right                            - Radius * 2, tabBounds.Bottom              - Radius * 2, Radius * 2, Radius * 2, 0, 90);
                    path.AddLine(tabBounds.Right                           - Radius, tabBounds.Bottom, tabBounds.X, tabBounds.Bottom);
                    break;
            }
        }
    }
}