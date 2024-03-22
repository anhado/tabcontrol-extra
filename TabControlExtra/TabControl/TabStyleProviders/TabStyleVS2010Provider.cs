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
    public class TabStyleVS2010Provider : TabStyleRoundedProvider
    {
        public TabStyleVS2010Provider(TabControlExtra tabControl) : base(tabControl)
        {
            Radius        = 3;
            ShowTabCloser = true;

            CloserColorFocused           = Color.FromArgb(117, 99, 61);
            CloserColorFocusedActive     = Color.Black;
            CloserColorSelected          = Color.FromArgb(95, 102, 115);
            CloserColorSelectedActive    = Color.Black;
            CloserColorHighlighted       = Color.FromArgb(206, 212, 221);
            CloserColorHighlightedActive = Color.Black;
            CloserColorUnselected        = Color.Empty;

            CloserButtonFillColorFocused           = Color.Empty;
            CloserButtonFillColorFocusedActive     = Color.White;
            CloserButtonFillColorSelected          = Color.Empty;
            CloserButtonFillColorSelectedActive    = Color.White;
            CloserButtonFillColorHighlighted       = Color.Empty;
            CloserButtonFillColorHighlightedActive = Color.White;
            CloserButtonFillColorUnselected        = Color.Empty;

            CloserButtonOutlineColorFocused           = Color.Empty;
            CloserButtonOutlineColorFocusedActive     = Color.FromArgb(229, 195, 101);
            CloserButtonOutlineColorSelected          = Color.Empty;
            CloserButtonOutlineColorSelectedActive    = Color.FromArgb(229, 195, 101);
            CloserButtonOutlineColorHighlighted       = Color.Empty;
            CloserButtonOutlineColorHighlightedActive = Color.FromArgb(229, 195, 101);
            CloserButtonOutlineColorUnselected        = Color.Empty;

            TextColorUnselected    = Color.White;
            TextColorDisabled      = Color.WhiteSmoke;
            BorderColorDisabled    = Color.FromArgb(41,  57,  85);
            BorderColorFocused     = Color.FromArgb(255, 243, 205);
            BorderColorHighlighted = Color.FromArgb(155, 167, 183);
            BorderColorSelected    = Color.FromArgb(206, 212, 223);
            BorderColorUnselected  = Color.Transparent;

            PageBackgroundColorDisabled    = Color.FromArgb(41,  57,  85);
            PageBackgroundColorFocused     = Color.FromArgb(229, 195, 101);
            PageBackgroundColorHighlighted = Color.FromArgb(75,  92,  116);
            PageBackgroundColorSelected    = Color.FromArgb(206, 212, 223);
            PageBackgroundColorUnselected  = Color.Transparent;

            TabColorDisabled1    = PageBackgroundColorDisabled;
            TabColorDisabled2    = TabColorDisabled1;
            TabColorFocused1     = PageBackgroundColorFocused;
            TabColorFocused2     = SystemColors.Window;
            TabColorHighLighted1 = PageBackgroundColorHighlighted;
            TabColorHighLighted2 = TabColorHighLighted1;
            TabColorSelected1    = PageBackgroundColorSelected;
            TabColorSelected2    = TabColorSelected1;
            TabColorUnSelected1  = Color.Transparent;
            TabColorUnSelected1  = Color.Transparent;

            //	Must set after the _Radius as this is used in the calculations of the actual padding
            Padding = new Point(6, 5);

            TabPageMargin = new Padding(0, 4, 0, 4);
            TabPageRadius = 2;
        }
    }
}