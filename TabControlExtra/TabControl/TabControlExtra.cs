/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security.Permissions;
using System.Windows.Forms;

namespace TradeWright.UI.Forms
{
    [ToolboxBitmapAttribute(typeof(TabControl))]
    public class TabControlExtra : TabControl
    {
        #region String formatting

        private StringFormat GetStringFormat()
        {
            StringFormat format = null;

            //	Rotate Text by 90 degrees for left and right tabs
            switch (Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    format = new StringFormat(StringFormatFlags.NoWrap);
                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    format = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.DirectionVertical);
                    break;
            }

            format.Alignment     = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            if (FindForm() != null && FindForm().KeyPreview)
                format.HotkeyPrefix = HotkeyPrefix.Show;
            else
                format.HotkeyPrefix = HotkeyPrefix.Hide;
            if (RightToLeft == RightToLeft.Yes) format.FormatFlags = format.FormatFlags | StringFormatFlags.DirectionRightToLeft;
            return format;
        }

        #endregion

        #region Private methods

        public IntPtr SendMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            Message message = new Message
            {
                HWnd   = Handle,
                LParam = lParam,
                WParam = wParam,
                Msg    = msg
            };

            WndProc(ref message);

            return message.Result;
        }

        #endregion

        #region constants

        public const int TabCloserButtonSize = 15;

        private const int AnyRightAlign  = (int)ContentAlignment.BottomRight  | (int)ContentAlignment.MiddleRight  | (int)ContentAlignment.TopRight;
        private const int AnyLeftAlign   = (int)ContentAlignment.BottomLeft   | (int)ContentAlignment.MiddleLeft   | (int)ContentAlignment.TopLeft;
        private const int AnyTopAlign    = (int)ContentAlignment.TopRight     | (int)ContentAlignment.TopCenter    | (int)ContentAlignment.TopLeft;
        private const int AnyBottomAlign = (int)ContentAlignment.BottomRight  | (int)ContentAlignment.BottomCenter | (int)ContentAlignment.BottomLeft;
        private const int AnyMiddleAlign = (int)ContentAlignment.MiddleRight  | (int)ContentAlignment.MiddleCenter | (int)ContentAlignment.MiddleLeft;
        private const int AnyCenterAlign = (int)ContentAlignment.BottomCenter | (int)ContentAlignment.MiddleCenter | (int)ContentAlignment.TopCenter;

        #endregion

        #region Construction

        public TabControlExtra()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);

            _BackBuffer         = new Bitmap(Width, Height);
            _BackBufferGraphics = Graphics.FromImage(_BackBuffer);
            _TabBuffer          = new Bitmap(Width, Height);
            _TabBufferGraphics  = Graphics.FromImage(_TabBuffer);

            SuspendLayout();
            DisplayStyle = TabStyle.Default;
            ResumeLayout();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            OnFontChanged(EventArgs.Empty);
        }

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams cp = base.CreateParams;
                if (EffectiveRightToLeft)
                    cp.ExStyle = cp.ExStyle | NativeMethods.WS_EX_LAYOUTRTL | NativeMethods.WS_EX_NOINHERITLAYOUT;
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_BackImage          != null) _BackImage.Dispose();
                if (_BackBufferGraphics != null) _BackBufferGraphics.Dispose();
                if (_BackBuffer         != null) _BackBuffer.Dispose();
                if (_TabBufferGraphics  != null) _TabBufferGraphics.Dispose();
                if (_TabBuffer          != null) _TabBuffer.Dispose();

                if (_StyleProvider != null) _StyleProvider.Dispose();
            }
        }

        #endregion

        #region Private variables

        private Bitmap   _BackImage;
        private Bitmap   _BackBuffer;
        private Graphics _BackBufferGraphics;
        private Bitmap   _TabBuffer;
        private Graphics _TabBufferGraphics;

        private GraphicsPath _PrevTabCloserButtonPath;

        private int   _oldValue;
        private Point _dragStartPosition = Point.Empty;

        private TabStyle         _Style;
        private TabStyleProvider _StyleProvider;

        private List<TabPage> _TabPages;

        private bool _SuspendDrawing;

        #endregion

        #region Public properties

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TabStyleProvider DisplayStyleProvider
        {
            get
            {
                if (_StyleProvider == null) DisplayStyle = TabStyle.Default;

                return _StyleProvider;
            }
            set => _StyleProvider = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(TabStyle), "Default")]
        [RefreshProperties(RefreshProperties.All)]
        public TabStyle DisplayStyle
        {
            get => _Style;
            set
            {
                if (_Style != value)
                {
                    _Style         = value;
                    _StyleProvider = TabStyleProvider.CreateProvider(this);
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Point MousePosition
        {
            get
            {
                Point loc = PointToClient(Control.MousePosition);
                loc = AdjustPointForRightToLeft(loc);
                return loc;
            }
        }

        [Category("Appearance")]
        [RefreshProperties(RefreshProperties.All)]
        public new bool Multiline
        {
            get => base.Multiline;
            set => base.Multiline = value;
        }


        //	Hide the Padding attribute so it can not be changed
        //	We are handling this on the Style Provider
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Point Padding
        {
            get => DisplayStyleProvider.Padding;
            set => DisplayStyleProvider.Padding = value;
        }

        [Category("Appearance")]
        [RefreshProperties(RefreshProperties.All)]
        public override bool RightToLeftLayout
        {
            get => base.RightToLeftLayout;
            set
            {
                base.RightToLeftLayout = value;
                UpdateStyles();
            }
        }

        //	Hide the HotTrack attribute so it can not be changed
        //	We are handling this on the Style Provider
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool HotTrack
        {
            get => DisplayStyleProvider.HotTrack;
            set => DisplayStyleProvider.HotTrack = value;
        }

        [Category("Appearance")]
        public new TabAlignment Alignment
        {
            get => base.Alignment;
            set
            {
                base.Alignment = value;
                switch (value)
                {
                    case TabAlignment.Top:
                    case TabAlignment.Bottom:
                        Multiline = false;
                        break;
                    case TabAlignment.Left:
                    case TabAlignment.Right:
                        Multiline = true;
                        break;
                }
            }
        }

        //	Hide the Appearance attribute so it can not be changed
        //	We don't want it as we are doing all the painting.
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public new TabAppearance Appearance
        {
            get { return base.Appearance; }
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
            set
            {
#pragma warning restore RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
                //	Don't permit setting to other appearances as we are doing all the painting
                base.Appearance = TabAppearance.Normal;
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                //	Special processing to hide tabs
                if (_Style == TabStyle.None) return new Rectangle(0, 0, Width, Height);

                int tabStripHeight = 0;
                int itemHeight     = 0;

                if (Alignment <= TabAlignment.Bottom)
                    itemHeight = ItemSize.Height;
                else
                    itemHeight = ItemSize.Width;

                tabStripHeight = 5 + itemHeight * RowCount;

                Rectangle rect = new Rectangle(4, tabStripHeight, Width - 8, Height - tabStripHeight - 4);
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        rect = new Rectangle(4, tabStripHeight, Width - 8, Height - tabStripHeight - 4);
                        break;
                    case TabAlignment.Bottom:
                        rect = new Rectangle(4, 4, Width - 8, Height - tabStripHeight - 4);
                        break;
                    case TabAlignment.Left:
                        rect = new Rectangle(tabStripHeight, 4, Width - tabStripHeight - 4, Height - 8);
                        break;
                    case TabAlignment.Right:
                        rect = new Rectangle(4, 4, Width - tabStripHeight - 4, Height - 8);
                        break;
                }

                return rect;
            }
        }

        [DebuggerStepThrough]
        public int GetActiveIndex(Point mousePosition)
        {
            NativeMethods.TCHITTESTINFO hitTestInfo = new NativeMethods.TCHITTESTINFO(mousePosition);
            int                         index       = SendMessage(NativeMethods.TCM_HITTEST, IntPtr.Zero, NativeMethods.ToIntPtr(hitTestInfo)).ToInt32();
            if (index == -1) return -1;

            if (TabPages[index].Enabled)
                return index;
            return -1;
        }

        public TabPage GetActiveTab(Point mousePosition)
        {
            int activeIndex = GetActiveIndex(mousePosition);
            if (activeIndex > -1)
                return TabPages[activeIndex];
            return null;
        }

        #endregion

        #region Public methods

        public void HideTab(TabPage page)
        {
            if (page != null && TabPages.Contains(page))
            {
                BackupTabPages();
                TabPages.Remove(page);
            }
        }

        public void HideTab(int index)
        {
            if (IsValidTabIndex(index)) HideTab(_TabPages[index]);
        }

        public void HideTab(string key)
        {
            if (TabPages.ContainsKey(key)) HideTab(TabPages[key]);
        }

        public void ShowTab(TabPage page)
        {
            if (page != null)
            {
                if (_TabPages != null)
                {
                    if (!TabPages.Contains(page)
                     && _TabPages.Contains(page))
                    {
                        //	Get insert point from backup of pages
                        int pageIndex = _TabPages.IndexOf(page);
                        if (pageIndex > 0)
                        {
                            int start = pageIndex - 1;

                            //	Check for presence of earlier pages in the visible tabs
                            for (int index = start; index >= 0; index--)
                                if (TabPages.Contains(_TabPages[index]))
                                {
                                    //	Set insert point to the right of the last present tab
                                    pageIndex = TabPages.IndexOf(_TabPages[index]) + 1;
                                    break;
                                }
                        }

                        //	Insert the page, or add to the end
                        if (pageIndex >= 0 && pageIndex < TabPages.Count)
                            TabPages.Insert(pageIndex, page);
                        else
                            TabPages.Add(page);
                    }
                }
                else
                {
                    //	If the page is not found at all then just add it
                    if (!TabPages.Contains(page)) TabPages.Add(page);
                }
            }
        }

        public void ShowTab(int index)
        {
            if (IsValidTabIndex(index)) ShowTab(_TabPages[index]);
        }

        public void ShowTab(string key)
        {
            if (_TabPages != null)
            {
                TabPage tab = _TabPages.Find(delegate(TabPage page) { return page.Name.Equals(key, StringComparison.OrdinalIgnoreCase); });
                ShowTab(tab);
            }
        }

        public void ResumeDrawing()
        {
            _SuspendDrawing = false;
        }

        public void SuspendDrawing()
        {
            _SuspendDrawing = true;
        }

        #endregion

        #region Drag 'n' Drop

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point mousePosition = new Point(e.X, e.Y);
            int   index         = GetActiveIndex(mousePosition);
            if (!DesignMode && index > -1 && _StyleProvider.ShowTabCloser && GetTabCloserButtonRect(index).Contains(mousePosition))
            {
                //	If we are clicking on a closer then remove the tab instead of raising the standard mouse down event
                //	But raise the tab closing event first
                TabPage                   tab  = GetActiveTab(mousePosition);
                TabControlCancelEventArgs args = new TabControlCancelEventArgs(tab, index, false, TabControlAction.Deselecting);
                OnTabClosing(args);
            }
            else
            {
                base.OnMouseDown(e);
                if (AllowDrop) _dragStartPosition = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (AllowDrop) _dragStartPosition = Point.Empty;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            if (drgevent.Data.GetDataPresent(typeof(TabPage)))
            {
                TabPage dragTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
                Cursor         = Cursors.Arrow;
                dragTab.Cursor = Cursors.Arrow;

                if (GetActiveTab(new Point(drgevent.X, drgevent.Y)) == dragTab) return;

                int insertPoint = GetActiveIndex(new Point(drgevent.X, drgevent.Y));
                if (insertPoint < 0) return;

                SuspendDrawing();

                //	Remove from current position (could be another tabcontrol)
                ((TabControl)dragTab.Parent).TabPages.Remove(dragTab);

                //	Add to current position
                TabPages.Insert(insertPoint, dragTab);
                SelectedTab = dragTab;

                ResumeDrawing();

                Invalidate();

                //	deal with hidden tab handling?
            }
            else
            {
                drgevent.Effect = DragDropEffects.None;
            }
        }

        private void StartDragDrop()
        {
            if (!_dragStartPosition.IsEmpty)
            {
                TabPage dragTab = SelectedTab;
                if (dragTab != null)
                {
                    //	Test for movement greater than the drag activation trigger area
                    Rectangle dragTestRect = new Rectangle(_dragStartPosition, Size.Empty);
                    dragTestRect.Inflate(SystemInformation.DragSize);
                    Point pt = PointToClient(Control.MousePosition);
                    if (!dragTestRect.Contains(pt))
                    {
                        DoDragDrop(dragTab, DragDropEffects.Move);
                        _dragStartPosition = Point.Empty;
                    }
                }
            }
        }

        #endregion

        #region Events

        [Category("Action")] public event ScrollEventHandler HScroll;

        [Category("Action")] public event EventHandler<TabControlEventArgs> TabImageClick;

        [Category("Action")] public event EventHandler<TabControlCancelEventArgs> TabClosing;

        [Category("Action")] public event EventHandler<TabControlEventArgs> TabClosed;

        #endregion

        #region Base class event processing

        protected override void OnFontChanged(EventArgs e)
        {
            //IntPtr hFont = this.Font.ToHfont();
            //NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETFONT, hFont, (IntPtr)(-1));
            //NativeMethods.SendMessage(this.Handle, NativeMethods.WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
            UpdateStyles();
        }

        private void CreateGraphicsBuffers()
        {
            //	Recreate the buffer for manual double buffering
            if (Width > 0 && Height > 0)
            {
                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }

                if (_BackBufferGraphics != null) _BackBufferGraphics.Dispose();
                if (_BackBuffer         != null) _BackBuffer.Dispose();

                _BackBuffer         = new Bitmap(Width, Height);
                _BackBufferGraphics = Graphics.FromImage(_BackBuffer);

                if (_TabBufferGraphics != null) _TabBufferGraphics.Dispose();
                if (_TabBuffer         != null) _TabBuffer.Dispose();

                _TabBuffer         = new Bitmap(Width, Height);
                _TabBufferGraphics = Graphics.FromImage(_TabBuffer);

                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            //var start = DateTime.Now;
            CreateGraphicsBuffers();
            base.OnResize(e);
            //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " TabControl " + this.GetHashCode() + " resized: " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms; size: " + this.Size.ToString() + " location: " + this.Location.ToString());
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            if (_BackImage != null)
            {
                _BackImage.Dispose();
                _BackImage = null;
            }

            base.OnParentBackColorChanged(e);
        }

        protected override void OnParentBackgroundImageChanged(EventArgs e)
        {
            if (_BackImage != null)
            {
                _BackImage.Dispose();
                _BackImage = null;
            }

            base.OnParentBackgroundImageChanged(e);
        }

        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            //	Do not allow selecting of disabled tabs
            if (e.Action == TabControlAction.Selecting && e.TabPage != null && !e.TabPage.Enabled) e.Cancel = true;
            base.OnSelecting(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (Width > 0 && Height > 0)
                if (_BackImage != null)
                {
                    _BackImage.Dispose();
                    _BackImage = null;
                }

            base.OnMove(e);
            Invalidate();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (Visible) OnPaint(new PaintEventArgs(CreateGraphics(), ClientRectangle));
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (Visible) OnPaint(new PaintEventArgs(CreateGraphics(), ClientRectangle));
        }

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessMnemonic(char charCode)
        {
            foreach (TabPage page in TabPages)
                if (IsMnemonic(charCode, page.Text))
                {
                    SelectedTab = page;
                    return true;
                }

            return base.ProcessMnemonic(charCode);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_HSCROLL:

                    //	Raise the scroll event when the scroller is scrolled
                    base.WndProc(ref m);
                    OnHScroll(new ScrollEventArgs((ScrollEventType)NativeMethods.LoWord(m.WParam), _oldValue, NativeMethods.HiWord(m.WParam), ScrollOrientation.HorizontalScroll));
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int index = GetActiveIndex(new Point(e.X, e.Y));

            //	If we are clicking on an image then raise the ImageClicked event before raising the standard mouse click event
            //	if there if a handler.
            if (index > -1 && TabImageClick != null
                           && TabHasImage(index)
                           && GetTabImageRect(index).Contains(MousePosition))
                OnTabImageClick(new TabControlEventArgs(TabPages[index], index, TabControlAction.Selected));
            //	Fire the base event
            base.OnMouseClick(e);
        }

        protected virtual void OnTabImageClick(TabControlEventArgs e)
        {
            TabImageClick?.Invoke(this, e);
        }

        protected virtual void OnTabClosed(TabControlEventArgs e)
        {
            TabClosed?.Invoke(this, e);
        }

        protected virtual void OnTabClosing(TabControlCancelEventArgs e)
        {
            TabClosing?.Invoke(this, e);
            if (e.Cancel)
                return;

            int selectedIndex = SelectedIndex;
            TabPages.Remove(e.TabPage);
            e.TabPage.Dispose();
            if (selectedIndex == TabPages.Count)
                SelectedIndex = selectedIndex - 1;
            else
                SelectedIndex = selectedIndex;

            OnTabClosed(new TabControlEventArgs(e.TabPage, e.TabPageIndex, e.Action));
        }

        protected virtual void OnHScroll(ScrollEventArgs e)
        {
            //	repaint the moved tabs
            Invalidate();

            //	Raise the event
            HScroll?.Invoke(this, e);

            if (e.Type == ScrollEventType.EndScroll) _oldValue = e.NewValue;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point mousePos = MousePosition;

            if (_PrevTabCloserButtonPath != null && _PrevTabCloserButtonPath.IsVisible(mousePos))
            {
                // mouse is still in highlighted tab closer
            }
            else
            {
                bool needsRepainting = false;
                if (_PrevTabCloserButtonPath != null)
                {
                    _PrevTabCloserButtonPath.Dispose();
                    _PrevTabCloserButtonPath = null;
                    needsRepainting          = true;
                }

                _PrevTabCloserButtonPath = GetTabCloserButtonPathAtPosition(mousePos);
                if (_PrevTabCloserButtonPath != null) needsRepainting = true;
                if (needsRepainting) CustomPaint(mousePos);
            }

            //	Initialise Drag Drop
            if (AllowDrop && e.Button == MouseButtons.Left) StartDragDrop();
        }

        #endregion

        #region Basic drawing methods

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_SuspendDrawing) return;

            //	We must always paint the entire area of the tab control, since our actual tab sizes 
            //  may differ from those of the underlying TabControl, which frequently requests painting 
            //  of just a single tab or the whole row of tabs (ie the clip rectangle supplied in the
            //  event args only covers those areas).

            //  So we create a new Graphics object rather than use the one in the event args, to avoid the clipping.
            DateTime start = DateTime.Now;
            Point    posn  = MousePosition;
            CustomPaint(posn);
            Debug.WriteLine(DateTime.Now + " TabControl " + GetHashCode() + " painted: " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms; size: " + Size + " location: " + Location + " clip: " + e.ClipRectangle);
        }

        private void CustomPaint(Point mousePosition)
        {
            //	We render into a bitmap that is then drawn in one shot rather than using
            //	double buffering built into the control as the built in buffering
            // 	messes up the background painting.
            //	Equally the .Net 2.0 BufferedGraphics object causes the background painting
            //	to mess up, which is why we use this .Net 1.1 buffering technique.

            //	Buffer code from Gil. Schmidt http://www.codeproject.com/KB/graphics/DoubleBuffering.aspx

            if (Width > 0 && Height > 0)
            {
                if (_BackImage == null)
                {
                    //	Cached Background Image
                    _BackImage = new Bitmap(Width, Height);
                    Graphics backGraphics = Graphics.FromImage(_BackImage);
                    backGraphics.Clear(Color.Transparent);
                    PaintTransparentBackground(backGraphics, ClientRectangle);
                }

                _BackBufferGraphics.Clear(Color.Transparent);
                _BackBufferGraphics.DrawImageUnscaled(_BackImage, 0, 0);

                if (EffectiveRightToLeft)
                {
                    Matrix m = new Matrix();
                    m.Translate(_TabBuffer.Width, 0f);
                    m.Scale(-1f, 1f);
                    _TabBufferGraphics.Transform = m;
                    m.Dispose();
                }

                _TabBufferGraphics.Clear(Color.Transparent);

                if (TabCount > 0)
                {
                    //	When top or bottom and scrollable we need to clip the sides from painting the tabs.
                    //	Left and right are always multiline.
                    if (Alignment <= TabAlignment.Bottom && !Multiline)
                    {
                        Rectangle rect = ClientRectangle;
                        _TabBufferGraphics.Clip = new Region(new RectangleF(rect.X + 4 - _StyleProvider.TabPageMargin.Left,
                                                                            rect.Y,
                                                                            rect.Width - 8 + _StyleProvider.TabPageMargin.Left + _StyleProvider.TabPageMargin.Right,
                                                                            rect.Height));
                    }

                    //	Draw each tabpage from right to left.  We do it this way to handle
                    //	the overlap correctly.
                    if (Multiline)
                    {
                        for (int row = 0; row < RowCount; row++)
                        for (int index = TabCount - 1; index >= 0; index--)
                            if (index != SelectedIndex && (RowCount == 1 || GetTabRow(index) == row))
                                DrawTabPage(index, mousePosition, _TabBufferGraphics);
                    }
                    else
                    {
                        for (int index = TabCount - 1; index >= 0; index--)
                            if (index != SelectedIndex)
                                DrawTabPage(index, mousePosition, _TabBufferGraphics);
                    }

                    //	The selected tab must be drawn last so it appears on top.
                    if (SelectedIndex > -1) DrawTabPage(SelectedIndex, mousePosition, _TabBufferGraphics);
                }

                _TabBufferGraphics.Flush();

                //	Paint the tabs on top of the background

                // Create a new color matrix and set the alpha value to the required opacity
                ColorMatrix alphaMatrix                     = new ColorMatrix();
                alphaMatrix.Matrix00 = alphaMatrix.Matrix11 = alphaMatrix.Matrix22 = alphaMatrix.Matrix44 = 1;
                alphaMatrix.Matrix33 = _StyleProvider.Opacity;

                // Create a new image attribute object and set the color matrix to
                // the one just created
                using (ImageAttributes alphaAttributes = new ImageAttributes())
                {
                    alphaAttributes.SetColorMatrix(alphaMatrix);

                    // Draw the original image with the image attributes specified
                    _BackBufferGraphics.DrawImage(_TabBuffer,
                                                  new Rectangle(0, 0, _TabBuffer.Width, _TabBuffer.Height),
                                                  0, 0, _TabBuffer.Width, _TabBuffer.Height, GraphicsUnit.Pixel,
                                                  alphaAttributes);
                }

                _BackBufferGraphics.Flush();

                //	Now paint this to the screen


                //	We want to paint the whole tabstrip and border every time
                //	so that the hot areas update correctly, along with any overlaps

                //	paint the tabs etc.
                using (Graphics g = CreateGraphics())
                {
                    if (EffectiveRightToLeft)
                        g.DrawImageUnscaled(_BackBuffer, -1, 0);
                    else
                        g.DrawImageUnscaled(_BackBuffer, 0, 0);
                }
            }
        }

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect)
        {
            if (Parent != null)
            {
                //	Set the cliprect to be relative to the parent
                clipRect.Offset(Location);

                //	Save the current state before we do anything.
                GraphicsState state = graphics.Save();

                //	Set the graphicsobject to be relative to the parent
                graphics.TranslateTransform(-Location.X, -Location.Y);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;

                //	Paint the parent
                PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
                try
                {
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                }
                finally
                {
                    //	Restore the graphics state and the clipRect to their original locations
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        private void DrawTabPage(int index, Point mousePosition, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle baseTabRect = GetBaseTabRect(index);
            Rectangle pageBounds  = GetPageBounds(index);

            Rectangle tabBounds      = _StyleProvider.GetTabRect(baseTabRect, pageBounds, SelectedIndex == index);
            Rectangle tabContentRect = Rectangle.Intersect(baseTabRect, tabBounds);

            TabState state        = GetTabState(index, mousePosition);
            bool     isTabEnabled = TabPages[index].Enabled;
            bool     isTabVisible = _Style != TabStyle.None && IsTabVisible(tabBounds, pageBounds);

            using (GraphicsPath tabPageBorder = GetTabPageBorder(pageBounds, tabBounds),
                                tabBorder = _StyleProvider.GetTabBorder(tabBounds))
            {
                Rectangle tabCloserButtonRect                         = Rectangle.Empty;
                if (_StyleProvider.ShowTabCloser) tabCloserButtonRect = GetTabCloserButtonRect(tabContentRect, tabBorder);

                Image     tabImage     = null;
                Rectangle tabImageRect = Rectangle.Empty;
                if (TabHasImage(index))
                {
                    tabImage     = GetTabImage(index);
                    tabImageRect = GetTabImageRect(tabContentRect, tabBorder);
                }

                Rectangle tabTextRect = GetTabTextRect(tabBorder, tabContentRect, tabCloserButtonRect, tabImageRect);

                //	Paint the background
                using (Brush fillBrush = _StyleProvider.GetPageBackgroundBrush(state))
                {
                    graphics.FillPath(fillBrush, tabPageBorder);
                }

                if (isTabVisible)
                {
                    //	Paint the tab
                    PaintTab(tabBorder, tabCloserButtonRect, state, graphics, mousePosition);

                    //	Draw any image
                    if (tabImageRect != Rectangle.Empty) DrawTabImage(tabImage, tabImageRect, graphics, isTabEnabled);

                    //	Draw the text
                    DrawTabText(TabPages[index].Text, state, graphics, tabTextRect);
                }

                //	Paint the border
                DrawTabPageBorder(tabPageBorder, state, graphics);
            }
        }

        private void PaintTab(GraphicsPath tabBorder, Rectangle tabCloserButtonRect, TabState state, Graphics graphics, Point mousePosition)
        {
            _StyleProvider.PaintTabBackground(tabBorder, state, graphics);

            //	Paint a focus indication
            _StyleProvider.DrawTabFocusIndicator(tabBorder, state, graphics);
            //	Paint the closer
            _StyleProvider.DrawTabCloser(tabCloserButtonRect, graphics, state, mousePosition);
        }

        private void DrawTabPageBorder(GraphicsPath path, TabState state, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color borderColor = Color.Empty;

            switch (state)
            {
                case TabState.Disabled:
                    borderColor = _StyleProvider.BorderColorDisabled;
                    break;
                case TabState.Focused:
                    borderColor = _StyleProvider.BorderColorFocused;
                    break;
                case TabState.Highlighted:
                    borderColor = _StyleProvider.BorderColorHighlighted;
                    break;
                case TabState.Selected:
                    borderColor = _StyleProvider.BorderColorSelected;
                    break;
                case TabState.Unselected:
                    borderColor = _StyleProvider.BorderColorUnselected;
                    break;
            }

            if (borderColor != Color.Empty)
                using (Pen borderPen = new Pen(borderColor))
                {
                    graphics.DrawPath(borderPen, path);
                }
        }

        private void DrawTabText(string text, TabState state, Graphics graphics, Rectangle textBounds)
        {
            graphics.SmoothingMode     = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color textColor = Color.Empty;

            switch (state)
            {
                case TabState.Disabled:
                    textColor = _StyleProvider.TextColorDisabled;
                    break;
                case TabState.Focused:
                    textColor = _StyleProvider.TextColorFocused;
                    break;
                case TabState.Highlighted:
                    textColor = _StyleProvider.TextColorHighlighted;
                    break;
                case TabState.Selected:
                    textColor = _StyleProvider.TextColorSelected;
                    break;
                case TabState.Unselected:
                    textColor = _StyleProvider.TextColorUnselected;
                    break;
            }

            using (Brush textBrush = new SolidBrush(textColor))
            {
                using (StringFormat format = GetStringFormat())
                {
                    if (EffectiveRightToLeft)
                        using (Matrix oldTransform = graphics.Transform, m = new Matrix())
                        {
                            m.Translate(Width - textBounds.Right - textBounds.Left, 0f);
                            graphics.Transform = m;
                            graphics.DrawString(text, Font, textBrush, textBounds, format);
                            graphics.Transform = oldTransform;
                        }
                    else
                        graphics.DrawString(text, Font, textBrush, textBounds, format);
                }
            }
        }

        private void DrawTabImage(Image tabImage, Rectangle imageRect, Graphics graphics, bool isTabEnabled)
        {
            if (tabImage == null) return;

            if (EffectiveRightToLeft) tabImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

            if (isTabEnabled)
                graphics.DrawImage(tabImage, imageRect);
            else
                ControlPaint.DrawImageDisabled(graphics, tabImage, imageRect.X, imageRect.Y, Color.Transparent);
        }

        #endregion

        #region Tab borders and bounds properties

        private void AdjustPoint(ref Point point, bool adjustHorizontally, int increment)
        {
            if (adjustHorizontally)
                point.X += increment;
            else
                point.Y += increment;
        }

        private Point AdjustPointForRightToLeft(Point point)
        {
            Point newPoint = new Point(point.X, point.Y);
            switch (Alignment)
            {
                case TabAlignment.Bottom:
                case TabAlignment.Top:
                    if (EffectiveRightToLeft) newPoint.X = Width - newPoint.X;
                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    if (EffectiveRightToLeft) newPoint.Y = Height - newPoint.Y;
                    break;
            }

            return newPoint;
        }

        private void BackupTabPages()
        {
            if (_TabPages == null)
            {
                _TabPages = new List<TabPage>();
                foreach (TabPage page in TabPages) _TabPages.Add(page);
            }
        }

        private Rectangle AdjustRectangleDimensionsToFitInPath(Rectangle rect, GraphicsPath path)
        {
            Rectangle newRect = rect;
            int       offset;
            switch (Alignment)
            {
                case TabAlignment.Bottom:
                    offset        =  GetOffsetToEnsurePointIsWithinPath(path, newRect.Right, newRect.Bottom, true, -1, p => p.X > newRect.X);
                    newRect.Width += offset;
                    offset        =  GetOffsetToEnsurePointIsWithinPath(path, newRect.X, newRect.Bottom, true, 1, p => p.X < newRect.Right);
                    newRect.X     += offset;
                    newRect.Width -= offset;
                    break;
                case TabAlignment.Top:
                    offset        =  GetOffsetToEnsurePointIsWithinPath(path, newRect.Right, newRect.Y, true, -1, p => p.X > newRect.X);
                    newRect.Width += offset;
                    offset        =  GetOffsetToEnsurePointIsWithinPath(path, newRect.X, newRect.Y, true, 1, p => p.X < newRect.Right);
                    newRect.X     += offset;
                    newRect.Width -= offset;
                    break;
                case TabAlignment.Left:
                    offset         =  GetOffsetToEnsurePointIsWithinPath(path, newRect.X, newRect.Bottom, false, -1, p => p.Y > newRect.Y);
                    newRect.Height += offset;
                    offset         =  GetOffsetToEnsurePointIsWithinPath(path, newRect.X, newRect.Top, false, 1, p => p.Y < newRect.Bottom);
                    newRect.Y      += offset;
                    newRect.Height -= offset;
                    break;
                case TabAlignment.Right:
                    offset         =  GetOffsetToEnsurePointIsWithinPath(path, newRect.Right, newRect.Bottom, false, -1, p => p.Y > newRect.Y);
                    newRect.Height += offset;
                    offset         =  GetOffsetToEnsurePointIsWithinPath(path, newRect.Right, newRect.Top, false, 1, p => p.Y < newRect.Bottom);
                    newRect.Y      += offset;
                    newRect.Height -= offset;
                    break;
            }

            return newRect;
        }

        private void AddPageBorder(GraphicsPath path, Rectangle pageBounds, Rectangle tabBounds)
        {
            int radius = _StyleProvider.TabPageRadius;

            if (!IsTabVisible(tabBounds, pageBounds))
            {
                AddRoundedRectangle(path, pageBounds, radius);
                return;
            }

            int diamX = Math.Min(2 * radius, pageBounds.Width);
            int radX  = diamX / 2;
            int diamY = Math.Min(2 * radius, pageBounds.Height);
            int radY  = diamY / 2;

            switch (Alignment)
            {
                case TabAlignment.Top:
                    if (tabBounds.Right > pageBounds.Right && tabBounds.Left < pageBounds.Right)
                    {
                    }
                    else if (tabBounds.Right > pageBounds.Right - radX)
                    {
                        path.AddLine(tabBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Top + radY);
                    }
                    else
                    {
                        path.AddLine(tabBounds.Right, pageBounds.Top, pageBounds.Right - radX, pageBounds.Top);
                        if (radius != 0) path.AddArc(pageBounds.Right - diamX, pageBounds.Top, diamX, diamY, 270, 90);
                    }

                    path.AddLine(pageBounds.Right, pageBounds.Top + radY, pageBounds.Right, pageBounds.Bottom - radY);
                    if (radius != 0) path.AddArc(pageBounds.Right - diamX, pageBounds.Bottom - diamY, diamX, diamY, 0, 90);
                    path.AddLine(pageBounds.Right - radX, pageBounds.Bottom, pageBounds.Left + radX, pageBounds.Bottom);
                    if (radius != 0) path.AddArc(pageBounds.Left, pageBounds.Bottom - diamY, diamX, diamY, 90, 90);
                    path.AddLine(pageBounds.Left, pageBounds.Bottom - radY, pageBounds.Left, pageBounds.Top + radY);

                    if (tabBounds.Left < pageBounds.Left && tabBounds.Right > pageBounds.Left)
                    {
                    }
                    else if (tabBounds.Left < pageBounds.Left + radX)
                    {
                        path.AddLine(pageBounds.Left, pageBounds.Top + radY, tabBounds.Left, pageBounds.Top);
                    }
                    else
                    {
                        if (radius != 0) path.AddArc(pageBounds.Left, pageBounds.Top, diamX, diamY, 180, 90);
                        path.AddLine(pageBounds.Left + radX, pageBounds.Top, tabBounds.Left, pageBounds.Top);
                    }

                    break;
                case TabAlignment.Bottom:
                    if (tabBounds.Left < pageBounds.Left && tabBounds.Right > pageBounds.Left)
                    {
                    }
                    else if (tabBounds.Left < pageBounds.Left + radX)
                    {
                        path.AddLine(tabBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom - radY);
                    }
                    else
                    {
                        path.AddLine(tabBounds.Left, pageBounds.Bottom, pageBounds.Left + radX, pageBounds.Bottom);
                        if (radius != 0) path.AddArc(pageBounds.Left, pageBounds.Bottom - diamY, diamX, diamY, 90, 90);
                    }

                    path.AddLine(pageBounds.Left, pageBounds.Bottom - radY, pageBounds.Left, pageBounds.Top + radY);
                    if (radius != 0) path.AddArc(pageBounds.Left, pageBounds.Top, diamX, diamY, 180, 90);
                    path.AddLine(pageBounds.Left + radX, pageBounds.Top, pageBounds.Right - radX, pageBounds.Top);
                    if (radius != 0) path.AddArc(pageBounds.Right - diamX, pageBounds.Top, diamX, diamY, 270, 90);
                    path.AddLine(pageBounds.Right, pageBounds.Top + radY, pageBounds.Right, pageBounds.Bottom - radY);

                    if (tabBounds.Right > pageBounds.Right && tabBounds.Left < pageBounds.Right)
                    {
                    }
                    else if (tabBounds.Right > pageBounds.Right - radX)
                    {
                        path.AddLine(pageBounds.Right, pageBounds.Bottom - radY, tabBounds.Right, pageBounds.Bottom);
                    }
                    else
                    {
                        if (radius != 0) path.AddArc(pageBounds.Right - diamX, pageBounds.Bottom - diamY, diamX, diamY, 0, 90);
                        path.AddLine(pageBounds.Right - radX, pageBounds.Bottom, tabBounds.Right, pageBounds.Bottom);
                    }

                    break;
                case TabAlignment.Left:
                    path.AddLine(pageBounds.Left,  tabBounds.Top,     pageBounds.Left,  pageBounds.Top);
                    path.AddLine(pageBounds.Left,  pageBounds.Top,    pageBounds.Right, pageBounds.Top);
                    path.AddLine(pageBounds.Right, pageBounds.Top,    pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left,  pageBounds.Bottom);
                    path.AddLine(pageBounds.Left,  pageBounds.Bottom, pageBounds.Left,  tabBounds.Bottom);
                    break;
                case TabAlignment.Right:
                    path.AddLine(pageBounds.Right, tabBounds.Bottom,  pageBounds.Right, pageBounds.Bottom);
                    path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left,  pageBounds.Bottom);
                    path.AddLine(pageBounds.Left,  pageBounds.Bottom, pageBounds.Left,  pageBounds.Top);
                    path.AddLine(pageBounds.Left,  pageBounds.Top,    pageBounds.Right, pageBounds.Top);
                    path.AddLine(pageBounds.Right, pageBounds.Top,    pageBounds.Right, tabBounds.Top);
                    break;
            }
        }

        private void AddRoundedRectangle(GraphicsPath path, Rectangle pageBounds, int radius)
        {
            if (radius == 0)
            {
                path.AddRectangle(pageBounds);
                return;
            }

            Size d = new Size(Math.Min(2 * radius, pageBounds.Width), Math.Min(2 * radius, pageBounds.Height));

            path.AddArc(pageBounds.Left,            pageBounds.Top,               d.Width, d.Height, 180, 90);
            path.AddArc(pageBounds.Right - d.Width, pageBounds.Top,               d.Width, d.Height, 270, 90);
            path.AddArc(pageBounds.Right - d.Width, pageBounds.Bottom - d.Height, d.Width, d.Height, 0,   90);
            path.AddArc(pageBounds.Left,            pageBounds.Bottom - d.Height, d.Width, d.Height, 90,  90);
        }

        private Rectangle EnsureRectIsInPath(GraphicsPath tabBorder, Rectangle rect, bool increaseCoordinate)
        {
            Rectangle newRect = rect;

            switch (Alignment)
            {
                case TabAlignment.Top:
                    if (increaseCoordinate)
                        newRect.X += 4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.X, newRect.Y, true, +1, p => p.X < Width);
                    else
                        newRect.X += -4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Right, newRect.Y, true, -1, p => p.X > 0);
                    break;
                case TabAlignment.Bottom:
                    if (increaseCoordinate)
                        newRect.X += 4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.X, newRect.Bottom, true, +1, p => p.X < Width);
                    else
                        newRect.X += -4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Right, newRect.Bottom, true, -1, p => p.X > 0);
                    break;
                case TabAlignment.Left:
                    if (increaseCoordinate)
                        newRect.Y += 4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Left, newRect.Y, false, +1, p => p.Y < Height);
                    else
                        newRect.Y += -4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Left, newRect.Bottom, false, -1, p => p.Y > 0);
                    break;
                case TabAlignment.Right:
                    if (increaseCoordinate)
                        newRect.Y += 4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Right, newRect.Y, false, +1, p => p.Y < Height);
                    else
                        newRect.Y += -4 + GetOffsetToEnsurePointIsWithinPath(tabBorder, newRect.Right, newRect.Bottom, false, -1, p => p.Y > 0);
                    break;
            }

            return newRect;
        }

        private bool EffectiveRightToLeft =>
            (RightToLeft == RightToLeft.Yes ||
             (RightToLeft        == RightToLeft.Inherit &&
              Parent.RightToLeft == RightToLeft.Yes))
         && RightToLeftLayout;

        private Rectangle GetBaseTabRect(int index)
        {
            Rectangle rect = GetTabRect(index);
            switch (Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    if (EffectiveRightToLeft)
                        rect.X = Width - rect.Right;
                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    if (EffectiveRightToLeft)
                        rect.Y = Height - rect.Bottom;
                    break;
            }

            return rect;
        }

        private int GetOffsetToEnsurePointIsWithinPath(GraphicsPath path, int X, int Y, bool adjustHorizontally, int increment, Func<Point, bool> constraint)
        {
            Point point = new Point(X, Y);
            while (!path.IsVisible(point) && constraint(point))
                AdjustPoint(ref point, adjustHorizontally, increment);
            return adjustHorizontally ? point.X - X : point.Y - Y;
        }

        public Rectangle GetPageBounds(int index)
        {
            if (index < 0)
                return new Rectangle();

            Rectangle pageBounds = TabPages[index].Bounds;

            pageBounds.Width  += _StyleProvider.TabPageMargin.Left + _StyleProvider.TabPageMargin.Right  - 1;
            pageBounds.Height += _StyleProvider.TabPageMargin.Top  + _StyleProvider.TabPageMargin.Bottom - 1;
            pageBounds.X      -= _StyleProvider.TabPageMargin.Left;
            pageBounds.Y      -= _StyleProvider.TabPageMargin.Top;

            return pageBounds;
        }

        public Rectangle GetTabBounds(int index)
        {
            return _StyleProvider.GetTabRect(GetTabRect(index), GetPageBounds(index), index == SelectedIndex);
        }

        private GraphicsPath GetTabCloserButtonPathAtPosition(Point position)
        {
            if (DesignMode || !_StyleProvider.ShowTabCloser)
                return null;
            for (int i = 0; i < TabCount; i++)
            {
                Rectangle    rect             = GetTabCloserButtonRect(i);
                GraphicsPath closerButtonPath = _StyleProvider.GetTabCloserButtonPath(rect);
                if (closerButtonPath.IsVisible(position))
                    return closerButtonPath;
            }

            return null;
        }

        public Rectangle GetTabCloserButtonRect(int index)
        {
            Rectangle baseTabRect = GetTabRect(index);
            Rectangle pageBounds  = GetPageBounds(index);

            Rectangle tabBounds      = _StyleProvider.GetTabRect(baseTabRect, pageBounds, SelectedIndex == index);
            Rectangle tabContentRect = Rectangle.Intersect(baseTabRect, tabBounds);
            return GetTabCloserButtonRect(tabContentRect, _StyleProvider.GetTabBorder(tabBounds));
        }

        private Rectangle GetTabCloserButtonRect(Rectangle tabContentRect, GraphicsPath tabBorder)
        {
            Rectangle closerRect         = new Rectangle();
            bool      increaseCoordinate = false;

            switch (Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    if (EffectiveRightToLeft)
                    {
                        closerRect         = RectangleUtils.GetRectangleWithinRectangle(tabContentRect, TabCloserButtonSize, ContentAlignment.MiddleLeft);
                        increaseCoordinate = true;
                    }
                    else
                    {
                        closerRect         = RectangleUtils.GetRectangleWithinRectangle(tabContentRect, TabCloserButtonSize, ContentAlignment.MiddleRight);
                        increaseCoordinate = false;
                    }

                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    if (EffectiveRightToLeft)
                    {
                        closerRect         = RectangleUtils.GetRectangleWithinRectangle(tabContentRect, TabCloserButtonSize, ContentAlignment.TopCenter);
                        increaseCoordinate = true;
                    }
                    else
                    {
                        closerRect         = RectangleUtils.GetRectangleWithinRectangle(tabContentRect, TabCloserButtonSize, ContentAlignment.BottomCenter);
                        increaseCoordinate = false;
                    }

                    break;
            }

            return EnsureRectIsInPath(tabBorder, closerRect, increaseCoordinate);
        }

        private Image GetTabImage(int index)
        {
            Image tabImage = null;
            if (ImageList == null)
            {
            }
            else if (TabPages[index].ImageIndex > -1 && ImageList.Images.Count > TabPages[index].ImageIndex)
            {
                tabImage = ImageList.Images[TabPages[index].ImageIndex];
            }
            else if (!string.IsNullOrEmpty(TabPages[index].ImageKey) && !TabPages[index].ImageKey.Equals("(none)", StringComparison.OrdinalIgnoreCase)
                                                                     && ImageList.Images.ContainsKey(TabPages[index].ImageKey))
            {
                tabImage = ImageList.Images[TabPages[index].ImageKey];
            }

            return tabImage;
        }

        private Rectangle GetTabImageRect(int index)
        {
            Rectangle tabRect = _StyleProvider.GetTabRect(GetTabRect(index), GetPageBounds(index), index == SelectedIndex);
            using (GraphicsPath tabBorderPath = _StyleProvider.GetTabBorder(tabRect))
            {
                return GetTabImageRect(tabRect, tabBorderPath);
            }
        }

        private Rectangle GetTabImageRect(Rectangle tabRect, GraphicsPath tabBorderPath)
        {
            Rectangle imageRect = new Rectangle();
            Size      imageSize = ImageList.ImageSize;

            imageRect = RectangleUtils.GetRectangleWithinRectangle(tabRect, imageSize, _StyleProvider.ImageAlign);

            ContentAlignment imageAlignment = _StyleProvider.ImageAlign;
            bool             horizontalTabs = Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom;
            bool adjustPosition = (horizontalTabs  && (IsLeftAligned(imageAlignment)   || IsRightAligned(imageAlignment)))
                               || (!horizontalTabs && (IsBottomAligned(imageAlignment) || IsTopAligned(imageAlignment)));
            bool increaseCoordinate = (horizontalTabs && IsLeftAligned(imageAlignment)) || (!horizontalTabs && IsTopAligned(imageAlignment));

            if (adjustPosition) imageRect = EnsureRectIsInPath(tabBorderPath, imageRect, increaseCoordinate);

            if (_StyleProvider.ShowTabCloser)
            {
                if (EffectiveRightToLeft)
                {
                    if (horizontalTabs  && IsLeftAligned(imageAlignment)) imageRect.X += TabCloserButtonSize + 4;
                    if (!horizontalTabs && IsTopAligned(imageAlignment)) imageRect.Y  += TabCloserButtonSize + 4;
                }
                else
                {
                    if (horizontalTabs  && IsRightAligned(imageAlignment)) imageRect.X  -= TabCloserButtonSize + 4;
                    if (!horizontalTabs && IsBottomAligned(imageAlignment)) imageRect.Y -= TabCloserButtonSize + 4;
                }
            }

            return imageRect;
        }

        private GraphicsPath GetTabPageBorder(Rectangle pageBounds, Rectangle tabBounds)
        {
            GraphicsPath path = new GraphicsPath();
            if (IsTabVisible(tabBounds, pageBounds))
                _StyleProvider.AddTabBorder(path, tabBounds);
            AddPageBorder(path, pageBounds, tabBounds);

            path.CloseFigure();
            return path;
        }

        public Point GetTabPosition(int index)
        {
            //	If we are not multiline then the column is the index and the row is 0.
            if (!Multiline) return new Point(0, index);

            //	If there is only one row then the column is the index
            if (RowCount == 1) return new Point(0, index);

            //	We are in a true multi-row scenario
            int       row    = GetTabRow(index);
            Rectangle rect   = GetTabRect(index);
            int       column = -1;

            //	Scan from left to right along rows, skipping to next row if it is not the one we want.
            for (int testIndex = 0; testIndex < TabCount; testIndex++)
            {
                Rectangle testRect = GetTabRect(testIndex);
                if (Alignment <= TabAlignment.Bottom)
                {
                    if (testRect.Y == rect.Y) column += 1;
                }
                else
                {
                    if (testRect.X == rect.X) column += 1;
                }

                if (testRect.Location.Equals(rect.Location)) return new Point(row, column);
            }

            return new Point(0, 0);
        }

        public int GetTabRow(int index)
        {
            //	All calculations will use this rect as the base point
            //	because the itemsize does not return the correct width.
            Rectangle rect = GetTabRect(index);

            int row = -1;

            switch (Alignment)
            {
                case TabAlignment.Top:
                    row = (rect.Y - 2) / rect.Height;
                    break;
                case TabAlignment.Bottom:
                    row = (Height - rect.Y - 2) / rect.Height - 1;
                    break;
                case TabAlignment.Left:
                    row = (rect.X - 2) / rect.Width;
                    break;
                case TabAlignment.Right:
                    row = (Width - rect.X - 2) / rect.Width - 1;
                    break;
            }

            return row;
        }

        private TabState GetTabState(int index, Point mousePosition)
        {
            if (SelectedIndex == index)
            {
                if (ContainsFocus)
                    return TabState.Focused;
                return TabState.Selected;
            }

            if (!TabPages[index].Enabled)
                return TabState.Disabled;
            if (DisplayStyleProvider.HotTrack && index == GetActiveIndex(mousePosition))
                return TabState.Highlighted;
            return TabState.Unselected;
        }

        private Rectangle GetTabTextRect(GraphicsPath tabBorder, Rectangle tabBounds, Rectangle closerRect, Rectangle imageRect)
        {
            int              left           = tabBounds.X      + 1;
            int              right          = tabBounds.Right  - 1;
            int              top            = tabBounds.Y      + 1;
            int              bottom         = tabBounds.Bottom - 1;
            ContentAlignment imageAlignment = _StyleProvider.ImageAlign;

            switch (Alignment)
            {
                case TabAlignment.Bottom:
                case TabAlignment.Top:
                    if (closerRect != Rectangle.Empty)
                    {
                        if (EffectiveRightToLeft)
                            left = closerRect.Right + 4;
                        else
                            right = closerRect.X - 4;
                    }

                    if (imageRect != Rectangle.Empty)
                    {
                        if (IsLeftAligned(imageAlignment))
                            left                                       = imageRect.Right + 4;
                        else if (IsRightAligned(imageAlignment)) right = imageRect.X     - 4;
                    }

                    break;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    if (closerRect != Rectangle.Empty)
                    {
                        if (EffectiveRightToLeft)
                            top = closerRect.Bottom + 4;
                        else
                            bottom = closerRect.Y - 4;
                    }

                    if (imageRect != Rectangle.Empty)
                    {
                        if (IsTopAligned(imageAlignment))
                            top                                          = imageRect.Bottom + 4;
                        else if (IsBottomAligned(imageAlignment)) bottom = imageRect.Y      - 4;
                    }

                    break;
            }

            Rectangle textRect = new Rectangle(left, top, right - left, bottom - top);

            //	Ensure it fits inside the path
            return AdjustRectangleDimensionsToFitInPath(textRect, tabBorder);
        }

        protected internal bool IsTabVisible(Rectangle tabBounds, Rectangle pageBounds)
        {
            switch (Alignment)
            {
                case TabAlignment.Top:
                case TabAlignment.Bottom:
                    return tabBounds.Right > pageBounds.Left + _StyleProvider.TabPageMargin.Left && tabBounds.Left < pageBounds.Right - _StyleProvider.TabPageMargin.Right;
                case TabAlignment.Left:
                case TabAlignment.Right:
                    return tabBounds.Bottom > pageBounds.Top + _StyleProvider.TabPageMargin.Top && tabBounds.Top < pageBounds.Bottom - _StyleProvider.TabPageMargin.Bottom;
            }

            return false;
        }

        private bool IsValidTabIndex(int index)
        {
            BackupTabPages();
            return index >= 0 && index < _TabPages.Count;
        }

        private bool TabHasImage(int index)
        {
            return ImageList != null &&
                   (TabPages[index].ImageIndex > -1 ||
                    (!string.IsNullOrEmpty(TabPages[index].ImageKey) && !TabPages[index].ImageKey.Equals("(none)", StringComparison.OrdinalIgnoreCase)));
        }

        #endregion

        #region Alignment predicates

        public static bool IsLeftAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyLeftAlign) != 0;
        }

        public static bool IsRightAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyRightAlign) != 0;
        }

        public static bool IsTopAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyTopAlign) != 0;
        }

        public static bool IsBottomAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyBottomAlign) != 0;
        }

        public static bool IsMiddleAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyMiddleAlign) != 0;
        }

        public static bool IsCenterAligned(ContentAlignment alignment)
        {
            return ((int)alignment & AnyCenterAlign) != 0;
        }

        #endregion
    }
}