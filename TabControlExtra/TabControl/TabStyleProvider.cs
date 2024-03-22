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
    public abstract class TabStyleProvider : Component
    {
        #region Constructor

        protected TabStyleProvider(TabControlExtra tabControl)
        {
            TabControl = tabControl;

            FocusColor = Color.Orange;

            if (TabControl.RightToLeftLayout)
                ImageAlign = ContentAlignment.MiddleRight;
            else
                ImageAlign = ContentAlignment.MiddleLeft;

            HotTrack = true;

            Padding = new Point(6, 3);
        }

        #endregion

        #region Factory Methods

        public static TabStyleProvider CreateProvider(TabControlExtra tabControl)
        {
            TabStyleProvider provider;

            //	Depending on the display style of the tabControl generate an appropriate provider.
            switch (tabControl.DisplayStyle)
            {
                case TabStyle.None:
                    provider = new TabStyleNoneProvider(tabControl);
                    break;

                case TabStyle.Default:
                    provider = new TabStyleDefaultProvider(tabControl);
                    break;

                case TabStyle.Angled:
                    provider = new TabStyleAngledProvider(tabControl);
                    break;

                case TabStyle.Rounded:
                    provider = new TabStyleRoundedProvider(tabControl);
                    break;

                case TabStyle.VisualStudio:
                    provider = new TabStyleVisualStudioProvider(tabControl);
                    break;

                case TabStyle.Chrome:
                    provider = new TabStyleChromeProvider(tabControl);
                    break;

                case TabStyle.IE8:
                    provider = new TabStyleIE8Provider(tabControl);
                    break;

                case TabStyle.VS2010:
                    provider = new TabStyleVS2010Provider(tabControl);
                    break;

                case TabStyle.Rectangular:
                    provider = new TabStyleRectangularProvider(tabControl);
                    break;

                case TabStyle.VS2012:
                    provider = new TabStyleVS2012Provider(tabControl);
                    break;

                default:
                    provider = new TabStyleDefaultProvider(tabControl);
                    break;
            }

            provider.DisplayStyle = tabControl.DisplayStyle;
            return provider;
        }

        #endregion

        #region Tab border and rect

        public GraphicsPath GetTabBorder(Rectangle tabBounds)
        {
            GraphicsPath path = new GraphicsPath();

            AddTabBorder(path, tabBounds);

            path.CloseFigure();
            return path;
        }

        #endregion

        #region Instance variables

        protected TabControlExtra TabControl { get; }

        private Point _Padding;
        private bool  _HotTrack;


        private int   _Radius = 1;
        private int   _Overlap;
        private float _Opacity = 1;
        private bool  _ShowTabCloser;
        private bool  _SelectedTabIsLarger;

        private BlendStyle _BlendStyle = BlendStyle.Normal;

        private Color _BorderColorDisabled    = Color.Empty;
        private Color _BorderColorFocused     = Color.Empty;
        private Color _BorderColorHighlighted = Color.Empty;
        private Color _BorderColorSelected    = Color.Empty;
        private Color _BorderColorUnselected  = Color.Empty;

        private Color _PageBackgroundColorDisabled    = Color.Empty;
        private Color _PageBackgroundColorFocused     = Color.Empty;
        private Color _PageBackgroundColorHighlighted = Color.Empty;
        private Color _PageBackgroundColorSelected    = Color.Empty;
        private Color _PageBackgroundColorUnselected  = Color.Empty;

        private Color _TabColorDisabled1    = Color.Empty;
        private Color _TabColorDisabled2    = Color.Empty;
        private Color _TabColorFocused1     = Color.Empty;
        private Color _TabColorFocused2     = Color.Empty;
        private Color _TabColorSelected1    = Color.Empty;
        private Color _TabColorSelected2    = Color.Empty;
        private Color _TabColorUnSelected1  = Color.Empty;
        private Color _TabColorUnSelected2  = Color.Empty;
        private Color _TabColorHighLighted1 = Color.Empty;
        private Color _TabColorHighLighted2 = Color.Empty;

        private Color _TextColorDisabled    = Color.Empty;
        private Color _TextColorFocused     = Color.Empty;
        private Color _TextColorHighlighted = Color.Empty;
        private Color _TextColorSelected    = Color.Empty;
        private Color _TextColorUnselected  = Color.Empty;

        private Padding _TabPageMargin = new Padding(1);

        private int _TabPageRadius;

        #endregion

        #region overridable Methods

        public virtual void AddTabBorder(GraphicsPath path, Rectangle tabBounds)
        {
            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    path.AddLine(tabBounds.X,     tabBounds.Bottom, tabBounds.X,     tabBounds.Y);
                    path.AddLine(tabBounds.X,     tabBounds.Y,      tabBounds.Right, tabBounds.Y);
                    path.AddLine(tabBounds.Right, tabBounds.Y,      tabBounds.Right, tabBounds.Bottom);
                    break;
                case TabAlignment.Bottom:
                    path.AddLine(tabBounds.Right, tabBounds.Y,      tabBounds.Right, tabBounds.Bottom);
                    path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X,     tabBounds.Bottom);
                    path.AddLine(tabBounds.X,     tabBounds.Bottom, tabBounds.X,     tabBounds.Y);
                    break;
                case TabAlignment.Left:
                    path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X,     tabBounds.Bottom);
                    path.AddLine(tabBounds.X,     tabBounds.Bottom, tabBounds.X,     tabBounds.Y);
                    path.AddLine(tabBounds.X,     tabBounds.Y,      tabBounds.Right, tabBounds.Y);
                    break;
                case TabAlignment.Right:
                    path.AddLine(tabBounds.X,     tabBounds.Y,      tabBounds.Right, tabBounds.Y);
                    path.AddLine(tabBounds.Right, tabBounds.Y,      tabBounds.Right, tabBounds.Bottom);
                    path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X,     tabBounds.Bottom);
                    break;
            }
        }

        public virtual Rectangle GetTabRect(Rectangle baseTabRect, Rectangle pageBounds, bool tabIsSelected)
        {
            Rectangle tabRect = baseTabRect;

            //	Adjust to meet the tabpage
            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    tabRect.Height += pageBounds.Top - tabRect.Bottom;
                    break;
                case TabAlignment.Bottom:
                    tabRect.Height += tabRect.Top - pageBounds.Bottom;
                    tabRect.Y      -= tabRect.Top - pageBounds.Bottom;
                    break;
                case TabAlignment.Left:
                    tabRect.Width += pageBounds.Left - tabRect.Right;
                    break;
                case TabAlignment.Right:
                    tabRect.Width += tabRect.Left - pageBounds.Right;
                    tabRect.X     -= tabRect.Left - pageBounds.Right;
                    break;
            }

            if (SelectedTabIsLarger) tabRect = EnlargeTab(tabRect, tabIsSelected);

            //	Create Overlap
            if (TabControl.Alignment <= TabAlignment.Bottom)
            {
                tabRect.X     -= _Overlap;
                tabRect.Width += _Overlap;
            }
            else
            {
                tabRect.Y      -= _Overlap;
                tabRect.Height += _Overlap;
            }

            tabRect = EnsureTabIsInView(tabRect, pageBounds);

            return tabRect;
        }

        private Rectangle EnlargeTab(Rectangle tabBounds, bool tabIsSelected)
        {
            Rectangle newTabBounds    = tabBounds;
            int       widthIncrement  = tabIsSelected ? 1 : 0;
            int       heightIncrement = tabIsSelected ? 1 : -1;

            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    newTabBounds.Y      -= heightIncrement;
                    newTabBounds.Height += heightIncrement;
                    newTabBounds.X      -= widthIncrement;
                    newTabBounds.Width  += 2 * widthIncrement;
                    break;
                case TabAlignment.Bottom:
                    newTabBounds.Height += heightIncrement;
                    newTabBounds.X      -= widthIncrement;
                    newTabBounds.Width  += 2 * widthIncrement;
                    break;
                case TabAlignment.Left:
                    newTabBounds.X      -= heightIncrement;
                    newTabBounds.Width  += heightIncrement;
                    newTabBounds.Y      -= widthIncrement;
                    newTabBounds.Height += 2 * widthIncrement;
                    break;
                case TabAlignment.Right:
                    newTabBounds.Width  += heightIncrement;
                    newTabBounds.Y      -= widthIncrement;
                    newTabBounds.Height += 2 * widthIncrement;
                    break;
            }

            return newTabBounds;
        }

        protected virtual Rectangle EnsureTabIsInView(Rectangle tabBounds, Rectangle pageBounds)
        {
            //	Adjust tab to fit within the page bounds.
            //	Make sure we only reposition visible tabs, as we may have scrolled out of view.

            if (!TabControl.IsTabVisible(tabBounds, pageBounds)) return tabBounds;

            Rectangle newTabBounds = tabBounds;

            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    if (newTabBounds.X <= pageBounds.X + 4) newTabBounds.X = pageBounds.X;
                    newTabBounds.Intersect(new Rectangle(pageBounds.X, tabBounds.Y, pageBounds.Width, tabBounds.Height));
                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    if (newTabBounds.Y <= pageBounds.Y + 4) newTabBounds.Y = pageBounds.Y;
                    newTabBounds.Intersect(new Rectangle(tabBounds.X, pageBounds.Y, tabBounds.Width, pageBounds.Height));
                    break;
            }

            return newTabBounds;
        }

        #endregion

        #region Base Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabStyle DisplayStyle { get; set; } = TabStyle.Default;

        public BlendStyle BlendStyle
        {
            get => _BlendStyle;
            set
            {
                _BlendStyle = value;
                TabControl.Invalidate();
            }
        }

        [Category("Appearance")] public ContentAlignment ImageAlign { get; set; }

        [Category("Appearance")]
        public Point Padding
        {
            get => _Padding;
            set
            {
                _Padding = value;
                if (_ShowTabCloser)
                {
                    if (value.X + _Radius / 2 < -TabControlExtra.TabCloserButtonSize)
                        ((TabControl)TabControl).Padding = new Point(0, value.Y);
                    else
                        ((TabControl)TabControl).Padding = new Point(value.X + _Radius + (TabControlExtra.TabCloserButtonSize + 10) / 2, value.Y);
                }
                else
                {
                    if (value.X + _Radius / 2 < 1)
                        ((TabControl)TabControl).Padding = new Point(0, value.Y);
                    else
                        ((TabControl)TabControl).Padding = new Point(value.X + _Radius, value.Y);
                }
            }
        }


        [Category("Appearance")]
        [DefaultValue(1)]
        [Browsable(true)]
        public int Radius
        {
            get => _Radius;
            set
            {
                if (value < 1) throw new ArgumentException("The radius cannot be less than 1", nameof(value));

                _Radius = value;
                //	Adjust padding
                Padding = _Padding;
            }
        }

        [Category("Appearance")]
        public int Overlap
        {
            get => _Overlap;
            set
            {
                if (value < 0) throw new ArgumentException("The tabs cannot have a negative overlap", nameof(value));
                _Overlap = value;
            }
        }


        [Category("Appearance")] public bool FocusTrack { get; set; }

        [Category("Appearance")]
        public bool HotTrack
        {
            get => _HotTrack;
            set
            {
                _HotTrack                         = value;
                ((TabControl)TabControl).HotTrack = value;
            }
        }

        [Category("Appearance")]
        public bool SelectedTabIsLarger
        {
            get => _SelectedTabIsLarger;
            set
            {
                _SelectedTabIsLarger = value;
                TabControl.Invalidate();
            }
        }

        [Category("Appearance")]
        public bool ShowTabCloser
        {
            get => _ShowTabCloser;
            set
            {
                _ShowTabCloser = value;
                //	Adjust padding
                Padding = _Padding;
            }
        }

        [Category("Appearance")]
        public float Opacity
        {
            get => _Opacity;
            set
            {
                if (value < 0) throw new ArgumentException("The opacity must be between 0 and 1", nameof(value));
                if (value > 1) throw new ArgumentException("The opacity must be between 0 and 1", nameof(value));
                _Opacity = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorDisabled
        {
            get
            {
                if (_BorderColorDisabled.IsEmpty)
                    return SystemColors.ControlLight;
                return _BorderColorDisabled;
            }
            set
            {
                if (value.Equals(SystemColors.ControlLight))
                    _BorderColorDisabled = Color.Empty;
                else
                    _BorderColorDisabled = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorFocused
        {
            get
            {
                if (_BorderColorFocused.IsEmpty)
                    return ThemedColors.ToolBorder;
                return _BorderColorFocused;
            }
            set
            {
                if (!value.Equals(BorderColorFocused))
                {
                    if (value.Equals(ThemedColors.ToolBorder))
                        _BorderColorFocused = Color.Empty;
                    else
                        _BorderColorFocused = value;
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorHighlighted
        {
            get
            {
                if (_BorderColorHighlighted.IsEmpty)
                    return SystemColors.ControlDark;
                return _BorderColorHighlighted;
            }
            set
            {
                if (value.Equals(SystemColors.ControlDark))
                    _BorderColorHighlighted = Color.Empty;
                else
                    _BorderColorHighlighted = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorSelected
        {
            get
            {
                if (_BorderColorSelected.IsEmpty)
                    return SystemColors.ControlDark;
                return _BorderColorSelected;
            }
            set
            {
                if (value.Equals(SystemColors.ControlDark))
                    _BorderColorSelected = Color.Empty;
                else
                    _BorderColorSelected = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorUnselected
        {
            get
            {
                if (_BorderColorUnselected.IsEmpty)
                    return SystemColors.ControlDark;
                return _BorderColorUnselected;
            }
            set
            {
                if (value.Equals(SystemColors.ControlDark))
                    _BorderColorUnselected = Color.Empty;
                else
                    _BorderColorUnselected = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color PageBackgroundColorDisabled
        {
            get
            {
                if (_PageBackgroundColorDisabled.IsEmpty)
                    return SystemColors.Control;
                return _PageBackgroundColorDisabled;
            }
            set => _PageBackgroundColorDisabled = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color PageBackgroundColorFocused
        {
            get
            {
                if (_PageBackgroundColorFocused.IsEmpty)
                    return SystemColors.ControlLight;
                return _PageBackgroundColorFocused;
            }
            set => _PageBackgroundColorFocused = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color PageBackgroundColorHighlighted
        {
            get
            {
                if (_PageBackgroundColorHighlighted.IsEmpty)
                    return PageBackgroundColorUnselected;
                return _PageBackgroundColorHighlighted;
            }
            set => _PageBackgroundColorHighlighted = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color PageBackgroundColorSelected
        {
            get
            {
                if (_PageBackgroundColorSelected.IsEmpty)
                    return SystemColors.ControlLightLight;
                return _PageBackgroundColorSelected;
            }
            set => _PageBackgroundColorSelected = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color PageBackgroundColorUnselected
        {
            get
            {
                if (_PageBackgroundColorUnselected.IsEmpty)
                    return SystemColors.Control;
                return _PageBackgroundColorUnselected;
            }
            set => _PageBackgroundColorUnselected = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorDisabled1
        {
            get
            {
                if (_TabColorDisabled1.IsEmpty)
                    return PageBackgroundColorDisabled;
                return _TabColorDisabled1;
            }
            set => _TabColorDisabled1 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorDisabled2
        {
            get
            {
                if (_TabColorDisabled2.IsEmpty)
                    return TabColorDisabled1;
                return _TabColorDisabled2;
            }
            set => _TabColorDisabled2 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorFocused1
        {
            get
            {
                if (_TabColorFocused1.IsEmpty)
                    return PageBackgroundColorFocused;
                return _TabColorFocused1;
            }
            set => _TabColorFocused1 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorFocused2
        {
            get
            {
                if (_TabColorFocused2.IsEmpty)
                    return TabColorFocused1;
                return _TabColorFocused2;
            }
            set => _TabColorFocused2 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorSelected1
        {
            get
            {
                if (_TabColorSelected1.IsEmpty)
                    return PageBackgroundColorSelected;
                return _TabColorSelected1;
            }
            set => _TabColorSelected1 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorSelected2
        {
            get
            {
                if (_TabColorSelected2.IsEmpty)
                    return TabColorSelected1;
                return _TabColorSelected2;
            }
            set => _TabColorSelected2 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorUnSelected1
        {
            get
            {
                if (_TabColorUnSelected1.IsEmpty)
                    return PageBackgroundColorUnselected;
                return _TabColorUnSelected1;
            }
            set => _TabColorUnSelected1 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorUnSelected2
        {
            get
            {
                if (_TabColorUnSelected2.IsEmpty)
                    return TabColorUnSelected1;
                return _TabColorUnSelected2;
            }
            set => _TabColorUnSelected2 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorHighLighted1
        {
            get
            {
                if (_TabColorHighLighted1.IsEmpty)
                    return PageBackgroundColorHighlighted;
                return _TabColorHighLighted1;
            }
            set => _TabColorHighLighted1 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TabColorHighLighted2
        {
            get
            {
                if (_TabColorHighLighted2.IsEmpty)
                    return TabColorHighLighted1;
                return _TabColorHighLighted2;
            }
            set => _TabColorHighLighted2 = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TextColorDisabled
        {
            get
            {
                if (_TextColorUnselected.IsEmpty)
                    return SystemColors.ControlDark;
                return _TextColorDisabled;
            }
            set
            {
                if (value.Equals(SystemColors.ControlDark))
                    _TextColorDisabled = Color.Empty;
                else
                    _TextColorDisabled = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TextColorFocused
        {
            get
            {
                if (_TextColorFocused.IsEmpty)
                    return TextColorSelected;
                return _TextColorFocused;
            }
            set => _TextColorFocused = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TextColorHighlighted
        {
            get
            {
                if (_TextColorHighlighted.IsEmpty)
                    return TextColorUnselected;
                return _TextColorHighlighted;
            }
            set => _TextColorHighlighted = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TextColorSelected
        {
            get
            {
                if (_TextColorSelected.IsEmpty)
                    return SystemColors.ControlText;
                return _TextColorSelected;
            }
            set
            {
                if (value.Equals(SystemColors.ControlText))
                    _TextColorSelected = Color.Empty;
                else
                    _TextColorSelected = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TextColorUnselected
        {
            get
            {
                if (_TextColorUnselected.IsEmpty)
                    return SystemColors.ControlText;
                return _TextColorUnselected;
            }
            set
            {
                if (value.Equals(SystemColors.ControlText))
                    _TextColorUnselected = Color.Empty;
                else
                    _TextColorUnselected = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color FocusColor { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorFocused { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorFocusedActive { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorSelected { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorSelectedActive { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorHighlighted { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "ControlDark")]
        public Color CloserColorHighlightedActive { get; set; } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserColorUnselected { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorFocused { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorFocusedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorSelected { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorSelectedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorHighlighted { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorHighlightedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonFillColorUnselected { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorFocused { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorFocusedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorSelected { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorSelectedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorHighlighted { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorHighlightedActive { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "Empty")]
        public Color CloserButtonOutlineColorUnselected { get; set; } = Color.Empty;

        [Category("Appearance")]
        [DefaultValue(typeof(Padding), "{1,1,1,1}")]
        public Padding TabPageMargin
        {
            get => _TabPageMargin;
            set
            {
                if (value.Left   < 0) value.Left   = 0;
                if (value.Right  < 0) value.Right  = 0;
                if (value.Top    < 0) value.Top    = 0;
                if (value.Bottom < 0) value.Bottom = 0;

                if (value.Left   > 4) value.Left   = 4;
                if (value.Right  > 4) value.Right  = 4;
                if (value.Top    > 4) value.Top    = 4;
                if (value.Bottom > 4) value.Bottom = 4;

                _TabPageMargin = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(int), "0")]
        public int TabPageRadius
        {
            get => _TabPageRadius;
            set
            {
                if (value < 0) value = 0;
                if (value > 4) value = 4;
                _TabPageRadius = value;
            }
        }

        #endregion

        #region Painting

        protected internal virtual void DrawTabCloser(GraphicsPath closerPath, GraphicsPath closerButtonPath, Graphics graphics, TabState state, Point mousePosition)
        {
            bool active = closerButtonPath.GetBounds().Contains(mousePosition);
            switch (state)
            {
                case TabState.Disabled:
                    DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorUnselected, CloserButtonFillColorUnselected, CloserButtonOutlineColorUnselected);
                    break;
                case TabState.Focused:
                    if (active)
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorFocusedActive, CloserButtonFillColorFocusedActive, CloserButtonOutlineColorFocusedActive);
                    else
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorFocused, CloserButtonFillColorFocused, CloserButtonOutlineColorFocused);
                    break;
                case TabState.Highlighted:
                    if (active)
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorHighlightedActive, CloserButtonFillColorHighlightedActive, CloserButtonOutlineColorHighlightedActive);
                    else
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorHighlighted, CloserButtonFillColorHighlighted, CloserButtonOutlineColorHighlighted);
                    break;
                case TabState.Selected:
                    if (active)
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorSelectedActive, CloserButtonFillColorSelectedActive, CloserButtonOutlineColorSelectedActive);
                    else
                        DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorSelected, CloserButtonFillColorSelected, CloserButtonOutlineColorSelected);
                    break;
                case TabState.Unselected:
                    DrawTabCloser(closerPath, closerButtonPath, graphics, CloserColorUnselected, CloserButtonFillColorUnselected, CloserButtonOutlineColorUnselected);
                    break;
            }
        }

        private void DrawTabCloser(GraphicsPath closerPath, GraphicsPath closerButtonPath, Graphics graphics, Color closerColor, Color closerFillColor, Color closerOutlineColor)
        {
            if (closerButtonPath != null)
            {
                if (closerFillColor != Color.Empty)
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                    using (Brush closerBrush = new SolidBrush(closerFillColor))
                    {
                        graphics.FillPath(closerBrush, closerButtonPath);
                    }
                }

                if (closerOutlineColor != Color.Empty)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen closerPen = new Pen(closerOutlineColor))
                    {
                        graphics.DrawPath(closerPen, closerButtonPath);
                    }
                }
            }

            if (closerColor != Color.Empty)
                using (Pen closerPen = new Pen(closerColor))
                {
                    closerPen.Width        = 1;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.DrawPath(closerPen, closerPath);
                }
        }

        protected internal virtual GraphicsPath GetTabCloserButtonPath(Rectangle closerButtonRect)
        {
            GraphicsPath closerPath = new GraphicsPath();
            closerPath.AddLine(closerButtonRect.X,     closerButtonRect.Y,      closerButtonRect.Right, closerButtonRect.Y);
            closerPath.AddLine(closerButtonRect.Right, closerButtonRect.Y,      closerButtonRect.Right, closerButtonRect.Bottom);
            closerPath.AddLine(closerButtonRect.Right, closerButtonRect.Bottom, closerButtonRect.X,     closerButtonRect.Bottom);
            closerPath.AddLine(closerButtonRect.X,     closerButtonRect.Bottom, closerButtonRect.X,     closerButtonRect.Y);
            closerPath.CloseFigure();
            return closerPath;
        }

        public void DrawTabCloser(Rectangle closerButtonRect, Graphics graphics, TabState state, Point mousePosition)
        {
            if (!_ShowTabCloser) return;
            using (GraphicsPath closerPath = GetTabCloserPath(closerButtonRect))
            {
                using (GraphicsPath closerButtonPath = GetTabCloserButtonPath(closerButtonRect))
                {
                    DrawTabCloser(closerPath, closerButtonPath, graphics, state, mousePosition);
                }
            }
        }

        protected internal virtual GraphicsPath GetTabCloserPath(Rectangle closerButtonRect)
        {
            GraphicsPath closerPath = new GraphicsPath();
            closerPath.AddLine(closerButtonRect.X + 4, closerButtonRect.Y + 4, closerButtonRect.Right - 4, closerButtonRect.Bottom - 4);
            closerPath.CloseFigure();
            closerPath.AddLine(closerButtonRect.Right - 4, closerButtonRect.Y + 4, closerButtonRect.X + 4, closerButtonRect.Bottom - 4);
            closerPath.CloseFigure();

            return closerPath;
        }

        public virtual void DrawTabFocusIndicator(GraphicsPath tabpath, TabState state, Graphics graphics)
        {
            if (FocusTrack && state == TabState.Focused)
            {
                Brush      focusBrush = null;
                RectangleF pathRect   = tabpath.GetBounds();
                Rectangle  focusRect  = Rectangle.Empty;
                switch (TabControl.Alignment)
                {
                    case TabAlignment.Top:
                        focusRect  = new Rectangle((int)pathRect.X, (int)pathRect.Y, (int)pathRect.Width, 4);
                        focusBrush = new LinearGradientBrush(focusRect, FocusColor, SystemColors.Window, LinearGradientMode.Vertical);
                        break;
                    case TabAlignment.Bottom:
                        focusRect  = new Rectangle((int)pathRect.X, (int)pathRect.Bottom - 4, (int)pathRect.Width, 4);
                        focusBrush = new LinearGradientBrush(focusRect, SystemColors.ControlLight, FocusColor, LinearGradientMode.Vertical);
                        break;
                    case TabAlignment.Left:
                        focusRect  = new Rectangle((int)pathRect.X, (int)pathRect.Y, 4, (int)pathRect.Height);
                        focusBrush = new LinearGradientBrush(focusRect, FocusColor, SystemColors.ControlLight, LinearGradientMode.Horizontal);
                        break;
                    case TabAlignment.Right:
                        focusRect  = new Rectangle((int)pathRect.Right - 4, (int)pathRect.Y, 4, (int)pathRect.Height);
                        focusBrush = new LinearGradientBrush(focusRect, SystemColors.ControlLight, FocusColor, LinearGradientMode.Horizontal);
                        break;
                }

                //	Ensure the focus strip does not go outside the tab
                Region focusRegion = new Region(focusRect);
                focusRegion.Intersect(tabpath);
                graphics.FillRegion(focusBrush, focusRegion);
                focusRegion.Dispose();
                focusBrush.Dispose();
            }
        }

        protected internal virtual void PaintTabBackground(GraphicsPath tabBorder, TabState state, Graphics graphics)
        {
            using (Brush fillBrush = GetTabBackgroundBrush(state, tabBorder))
            {
                //	Paint the background
                graphics.FillPath(fillBrush, tabBorder);
            }
        }

        #endregion

        #region Background brushes

        public virtual Brush GetPageBackgroundBrush(TabState state)
        {
            Color color = Color.Empty;

            switch (state)
            {
                case TabState.Disabled:
                    color = PageBackgroundColorDisabled;
                    break;
                case TabState.Focused:
                    color = PageBackgroundColorFocused;
                    break;
                case TabState.Highlighted:
                    color = PageBackgroundColorHighlighted;
                    break;
                case TabState.Selected:
                    color = PageBackgroundColorSelected;
                    break;
                case TabState.Unselected:
                    color = PageBackgroundColorUnselected;
                    break;
            }

            return new SolidBrush(color);
        }

        protected internal Brush GetTabBackgroundBrush(TabState state, GraphicsPath tabBorder)
        {
            Color color1 = GetTabBackgroundColor1(state, tabBorder);
            Color color2 = GetTabBackgroundColor2(state, tabBorder);

            return CreateTabBackgroundBrush(color1, color2, state, tabBorder);
        }

        protected internal virtual Brush CreateTabBackgroundBrush(Color color1, Color color2, TabState state, GraphicsPath tabBorder)
        {
            LinearGradientBrush fillBrush = null;

            //	Get the correctly aligned gradient
            RectangleF tabBounds = tabBorder.GetBounds();
            //tabBounds.Inflate(3, 3);
            //tabBounds.X -= 1;
            //tabBounds.Y -= 1;
            switch (TabControl.Alignment)
            {
                case TabAlignment.Top:
                    tabBounds.Height += 1;
                    fillBrush        =  new LinearGradientBrush(tabBounds, color2, color1, LinearGradientMode.Vertical);
                    break;
                case TabAlignment.Bottom:
                    fillBrush = new LinearGradientBrush(tabBounds, color1, color2, LinearGradientMode.Vertical);
                    break;
                case TabAlignment.Left:
                    fillBrush = new LinearGradientBrush(tabBounds, color2, color1, LinearGradientMode.Horizontal);
                    break;
                case TabAlignment.Right:
                    fillBrush = new LinearGradientBrush(tabBounds, color1, color2, LinearGradientMode.Horizontal);
                    break;
            }

            //	Add the blend
            fillBrush.Blend = GetBackgroundBlend();
            return fillBrush;
        }

        protected virtual Color GetTabBackgroundColor1(TabState state, GraphicsPath tabBorder)
        {
            Color color = Color.Empty;

            switch (state)
            {
                case TabState.Disabled:
                    color = TabColorDisabled1;
                    break;
                case TabState.Focused:
                    color = TabColorFocused1;
                    break;
                case TabState.Highlighted:
                    color = TabColorHighLighted1;
                    break;
                case TabState.Selected:
                    color = TabColorSelected1;
                    break;
                case TabState.Unselected:
                    color = TabColorUnSelected1;
                    break;
            }

            return color;
        }

        protected virtual Color GetTabBackgroundColor2(TabState state, GraphicsPath tabBorder)
        {
            Color color = Color.Empty;

            switch (state)
            {
                case TabState.Disabled:
                    color = TabColorDisabled2;
                    break;
                case TabState.Focused:
                    color = TabColorFocused2;
                    break;
                case TabState.Highlighted:
                    color = TabColorHighLighted2;
                    break;
                case TabState.Selected:
                    color = TabColorSelected2;
                    break;
                case TabState.Unselected:
                    color = TabColorUnSelected2;
                    break;
            }

            return color;
        }

        protected virtual Blend GetBackgroundBlend()
        {
            float[] relativeIntensities = { 0f, 0.7f, 1f };
            float[] relativePositions   = { 0f, 0.6f, 1f };

            //	Glass look to top aligned tabs
            if (BlendStyle == BlendStyle.Glass)
            {
                relativeIntensities = new[] { 0f, 0.5f, 1f, 1f };
                relativePositions   = new[] { 0f, 0.5f, 0.51f, 1f };
            }

            Blend blend = new Blend();
            blend.Factors   = relativeIntensities;
            blend.Positions = relativePositions;

            return blend;
        }

        #endregion
    }
}