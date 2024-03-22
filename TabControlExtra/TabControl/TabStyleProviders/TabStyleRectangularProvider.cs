/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeWright.UI.Forms
{
    [ToolboxItem(false)]
    public class TabStyleRectangularProvider : TabStyleProvider
    {
        public TabStyleRectangularProvider(TabControlExtra tabControl) : base(tabControl)
        {
            Radius        = 1;
            ShowTabCloser = true;

            CloserColorFocused           = Color.FromArgb(208, 230, 245);
            CloserColorFocusedActive     = Color.White;
            CloserColorSelected          = Color.FromArgb(109, 109, 112);
            CloserColorSelectedActive    = Color.FromArgb(113, 113, 113);
            CloserColorHighlighted       = Color.FromArgb(129, 195, 241);
            CloserColorHighlightedActive = Color.White;
            CloserColorUnselected        = Color.Empty;

            CloserButtonFillColorFocused           = Color.Empty;
            CloserButtonFillColorFocusedActive     = Color.FromArgb(28, 151, 234);
            CloserButtonFillColorSelected          = Color.Empty;
            CloserButtonFillColorSelectedActive    = Color.FromArgb(230, 231, 237);
            CloserButtonFillColorHighlighted       = Color.Empty;
            CloserButtonFillColorHighlightedActive = Color.FromArgb(82, 176, 239);
            CloserButtonFillColorUnselected        = Color.Empty;

            CloserButtonOutlineColorFocused           = Color.Empty; //Color.FromArgb(0, 122, 204);
            CloserButtonOutlineColorFocusedActive     = Color.Empty;
            CloserButtonOutlineColorSelected          = Color.Empty;
            CloserButtonOutlineColorSelectedActive    = Color.Empty;
            CloserButtonOutlineColorHighlighted       = Color.Empty; //Color.FromArgb(28, 151, 234);
            CloserButtonOutlineColorHighlightedActive = Color.Empty;
            CloserButtonOutlineColorUnselected        = Color.Empty;

            TextColorFocused     = Color.White;
            TextColorHighlighted = Color.White;
            TextColorSelected    = Color.FromArgb(113, 113, 113);
            TextColorUnselected  = Color.White;

            PageBackgroundColorDisabled    = SystemColors.Control;
            PageBackgroundColorFocused     = Color.FromArgb(0,   122, 204);
            PageBackgroundColorHighlighted = Color.FromArgb(28,  151, 234);
            PageBackgroundColorSelected    = Color.FromArgb(204, 206, 219);
            PageBackgroundColorUnselected  = Color.Transparent;

            BorderColorDisabled    = PageBackgroundColorDisabled;
            BorderColorFocused     = PageBackgroundColorFocused;
            BorderColorHighlighted = PageBackgroundColorHighlighted;
            BorderColorSelected    = PageBackgroundColorSelected;
            BorderColorUnselected  = PageBackgroundColorUnselected;

            TabPageRadius = 0;

            //	Must set after the _Radius as this is used in the calculations of the actual padding
            Padding = new Point(6, 5);

            TabPageMargin = new Padding(0, 2, 0, 2);
        }
    }
}