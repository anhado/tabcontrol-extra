/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TradeWright.UI.Forms
{
    [ToolboxItem(false)]
    public class TabStyleNoneProvider : TabStyleProvider
    {
        public TabStyleNoneProvider(TabControlExtra tabControl) : base(tabControl)
        {
        }

        public override void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
        {
        }
    }
}