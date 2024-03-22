/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System.ComponentModel;
using System.Drawing;

namespace TradeWright.UI.Forms
{
    [ToolboxItem(false)]
    public class TabStyleDefaultProvider : TabStyleProvider
    {
        public TabStyleDefaultProvider(TabControlExtra tabControl) : base(tabControl)
        {
            Radius = 2;

            SelectedTabIsLarger = true;

            TabColorHighLighted1 = Color.FromArgb(236, 244, 252);
            TabColorHighLighted2 = Color.FromArgb(221, 237, 252);

            PageBackgroundColorHighlighted = TabColorHighLighted1;
        }
    }
}