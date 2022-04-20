/***************************************************************************
 *   CopyRight (C) 2009 by Cristinel Mazarine                              *
 *   Author:   Cristinel Mazarine                                          *
 *   Contact:  cristinel@osec.ro                                           *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the Crom Free License as published by           *
 *   the SC Crom-Osec SRL; version 1 of the License                        *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   Crom Free License for more details.                                   *
 *                                                                         *
 *   You should have received a copy of the Crom Free License along with   *
 *   this program; if not, write to the contact@osec.ro                    *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Crom.Controls.Docking
{
    /// <summary>
    /// Implementation of forms decorator
    /// </summary>
    internal class FormsDecorator : Label
    {
        #region Fields

        private FormsDecoratorControlCollection _controls = null;

        private TitleBarRenderer _titleRenderer = new TitleBarRenderer();

        private ControlPositioner _positioner = null;
        private bool _isFocused = false;

        private Point _mouseDownScreenPos = new Point();
        private Point _positionerPositionOnMouseDown = new Point();
        private Size _positionerSizeOnMouseDown = new Size();
        private zSizeMode _sizeMode = zSizeMode.None;
        private bool _moving = false;

        private Timer _unhighlightTimer = new Timer();
        private bool _canResizeByMouse = true;
        Form m_general_form;
        #endregion Fields

        #region Instance

        /// <summary>
        /// Default constructor
        /// </summary>
        public FormsDecorator(Form generalForm)
        {
            m_general_form = generalForm;
            if(generalForm != null)
                m_general_form.TextChanged += OnHeaderFormChanged;
            FormsPanel.Bounds = ClientRectangle;
            FormsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            var ic = InternalControls;
            this.Paint += OnFormDecoratorPaint;
            //FormsPanel.Paint += OnFormDecoratorPaint;
            TitleBar.Height = 20;
            TitleBar.Cursor = Cursors.Default;
            TitleBar.MouseClick += BringToFrontHelper.OnBringToFront;
            TitleBar.Paint += OnPaintTitleBar;
            TitleBar.MouseDown += OnMouseDownInTitleBar;
            TitleBar.MouseMove += OnMouseMoveInTitleBar;
            TitleBar.MouseUp += OnMouseUpFromTitleBar;
            TopMargin.Height = 28;
            //TopMargin.Location = new Point(TopMargin.Location.X, 55);
            TopMargin.Cursor = Cursors.SizeNS;
            TopMargin.MouseDown += OnMouseDownInTopMargin;
            TopMargin.MouseMove += OnMouseMoveInTopMargin;
            TopMargin.MouseUp += OnMouseUpFromTopMargin;

            LeftMargin.Width = 4;
            LeftMargin.Cursor = Cursors.SizeWE;
            LeftMargin.MouseDown += OnMouseDownInLeftMargin;
            LeftMargin.MouseMove += OnMouseMoveInLeftMargin;
            LeftMargin.MouseUp += OnMouseUpFromLeftMargin;

            RightMargin.Width = 4;
            RightMargin.Cursor = Cursors.SizeWE;
            RightMargin.MouseDown += OnMouseDownInRightMargin;
            RightMargin.MouseMove += OnMouseMoveInRightMargin;
            RightMargin.MouseUp += OnMouseUpFromRightMargin;

            BottomMargin.Height = 4;
            BottomMargin.Cursor = Cursors.SizeNS;
            BottomMargin.MouseDown += OnMouseDownInBottomMargin;
            BottomMargin.MouseMove += OnMouseMoveInBottomMargin;
            BottomMargin.MouseUp += OnMouseUpFromBottomMargin;

            
            LeftMargin.BackColor = SystemColors.Control;
            RightMargin.BackColor = SystemColors.Control;
            BottomMargin.BackColor = SystemColors.Control;

            FormsPanel.BackColor = SystemColors.Control;
            FormsPanel.Visible = false;

            FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;
            forms.TopControlChanged += OnTopFormChanged;

            _unhighlightTimer.Tick += OnUnhighlightTimer;
            _unhighlightTimer.Interval = 200;
            _unhighlightTimer.Enabled = true;
        }

        #endregion Instance

        #region Public section

        /// <summary>
        /// Occurs when the context button was clicked
        /// </summary>
        public event EventHandler ContextButtonClick;

        /// <summary>
        /// Occurs when the auto-hide button was clicked
        /// </summary>
        public event EventHandler AutohideButtonClick;

        /// <summary>
        /// Occurs when the close button was clicked
        /// </summary>
        public event EventHandler CloseButtonClick;


        /// <summary>
        /// Change is focused state
        /// </summary>
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    TitleBar.Invalidate();
                }
            }
        }

        /// <summary>
        /// Positioner
        /// </summary>
        public ControlPositioner Positioner
        {
            get
            {
                if (_positioner == null)
                {
                    _positioner = new ControlPositioner(this);
                    _positioner.Disposed += OnPositionerDisposed;
                }

                return _positioner;
            }
            set
            {
                if (_positioner != null)
                {
                    _positioner.Disposed -= OnPositionerDisposed;
                    _positioner.CanMoveChanged -= OnPositionerCanMoveChanged;
                    _positioner.CanSizeLeftChanged -= OnPositionerCanSizeLeftChanged;
                    _positioner.CanSizeRightChanged -= OnPositionerCanSizeRightChanged;
                    _positioner.CanSizeTopChanged -= OnPositionerCanSizeTopChanged;
                    _positioner.CanSizeBottomChanged -= OnPositionerCanSizeBottomChanged;
                }

                _positioner = value;

                if (_positioner != null)
                {
                    _positioner.Disposed += OnPositionerDisposed;
                    _positioner.CanMoveChanged += OnPositionerCanMoveChanged;
                    _positioner.CanSizeLeftChanged += OnPositionerCanSizeLeftChanged;
                    _positioner.CanSizeRightChanged += OnPositionerCanSizeRightChanged;
                    _positioner.CanSizeTopChanged += OnPositionerCanSizeTopChanged;
                    _positioner.CanSizeBottomChanged += OnPositionerCanSizeBottomChanged;
                }
                OnPositionerCanMoveChanged(null, EventArgs.Empty);
                OnPositionerCanSizeLeftChanged(null, EventArgs.Empty);
                OnPositionerCanSizeRightChanged(null, EventArgs.Empty);
                OnPositionerCanSizeTopChanged(null, EventArgs.Empty);
                OnPositionerCanSizeBottomChanged(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Get the count of forms inside this control
        /// </summary>
        public int FormsCount
        {
            get { return FormsPanel.Controls.Count; }
        }

        /// <summary>
        /// Add form
        /// </summary>
        /// <param name="form">form</param>
        public void Add(Form form)
        {
            form.TopLevel = false;
            form.ShowInTaskbar = false;
            FormsPanel.Controls.Add(form);
            form.Visible = true;
        }

        /// <summary>
        /// Remove a form from decorator
        /// </summary>
        /// <param name="form">form to remove</param>
        /// <returns>true if form was removed</returns>
        public bool Remove(Form form)
        {
            if (FormsPanel.Controls.Contains(form))
            {
                FormsPanel.Controls.Remove(form);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the form at zero based index
        /// </summary>
        /// <param name="index">zero based form index</param>
        /// <returns>form at index</returns>
        public Form GetFormAt(int index)
        {
            return (Form)FormsPanel.Controls[index];
        }

        /// <summary>
        /// Select a form
        /// </summary>
        /// <param name="form">form to select</param>
        public void SelectForm(Form form)
        {
            FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;
            forms.SetChildIndex(form, 0);
        }


        /// <summary>
        /// Flag indicating if can resize the control
        /// </summary>
        public bool CanResizeByMouse
        {
            get { return _canResizeByMouse; }
            set
            {
                if (_canResizeByMouse != value)
                {
                    _canResizeByMouse = value;

                    OnPositionerCanSizeLeftChanged(null, EventArgs.Empty);
                    OnPositionerCanSizeRightChanged(null, EventArgs.Empty);
                    OnPositionerCanSizeTopChanged(null, EventArgs.Empty);
                    OnPositionerCanSizeBottomChanged(null, EventArgs.Empty);

                    SetFormsPanelBounds();
                    ApplyTopFormMargins();
                }
            }
        }

        /// <summary>
        /// Begin movement by mouse
        /// </summary>
        /// <param name="mouseScreenPos">mouse down screen position</param>
        public void BeginMovementByMouse(Point mouseScreenPos)
        {
            if (Positioner.CanMove)
            {
                _mouseDownScreenPos = mouseScreenPos;
                _positionerPositionOnMouseDown = Positioner.Location;
                _sizeMode = zSizeMode.Move;
                _moving = false;
            }
        }

        /// <summary>
        /// Continue movement by mouse
        /// </summary>
        /// <param name="mouseScreenPos">mouse screen position</param>
        /// <returns>true if movement is continued</returns>
        public bool ContinueMovementByMouse(Point mouseScreenPos)
        {
            if (_sizeMode != zSizeMode.Move)
                return false;
            int dx = mouseScreenPos.X - _mouseDownScreenPos.X;
            int dy = mouseScreenPos.Y - _mouseDownScreenPos.Y;
            if (_moving == false)
            {
                _moving = true;
                Positioner.StartMoveByMouse();
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
            }
            Positioner.PerformMoveByMouse(_positionerPositionOnMouseDown.X + dx, _positionerPositionOnMouseDown.Y + dy);
            return true;
        }

        /// <summary>
        /// End movement by mouse
        /// </summary>
        public void EndMovementByMouse()
        {
            if (_moving)
            {
                Positioner.StopMoveByMouse();
                _moving = false;
            }

            _sizeMode = zSizeMode.None;
        }

        /// <summary>
        /// Flag indicating if the form is in auto-hidden mode or not
        /// </summary>
        public bool AutoHidden
        {
            get { return _titleRenderer.Autohide; }
            set
            {
                if (_titleRenderer.Autohide != value)
                {
                    _titleRenderer.Autohide = value;
                    _controls.TitleBar.Invalidate();
                }
            }
        }

        /// <summary>
        /// Show close button
        /// </summary>
        public bool ShowCloseButton
        {
            get { return _titleRenderer.ShowCloseButton; }
            set
            {
                if (_titleRenderer.ShowCloseButton != value)
                {
                    _titleRenderer.ShowCloseButton = value;
                    _controls.TitleBar.Invalidate();
                }
            }
        }

        /// <summary>
        /// Show autohide button
        /// </summary>
        public bool ShowAutohideButton
        {
            get { return _titleRenderer.ShowAutohideButton; }
            set
            {
                if (_titleRenderer.ShowAutohideButton != value)
                {
                    _titleRenderer.ShowAutohideButton = value;
                    _controls.TitleBar.Invalidate();
                }
            }
        }

        /// <summary>
        /// Show context menu button
        /// </summary>
        public bool ShowContextMenuButton
        {
            get { return _titleRenderer.ShowContextMenuButton; }
            set
            {
                if (_titleRenderer.ShowContextMenuButton != value)
                {
                    _titleRenderer.ShowContextMenuButton = value;
                    _controls.TitleBar.Invalidate();
                }
            }
        }

        #endregion Public section

        #region Protected section

        /// <summary>
        /// Dispose current instance
        /// </summary>
        /// <param name="fromIDisposableDispose">called from IDisposable.Dispose</param>
        protected override void Dispose(bool fromIDisposableDispose)
        {
            if (fromIDisposableDispose)
            {
                if (_unhighlightTimer != null)
                {
                    _unhighlightTimer.Enabled = false;
                    _unhighlightTimer.Tick -= OnUnhighlightTimer;
                    _unhighlightTimer.Dispose();
                    _unhighlightTimer = null;
                }
            }

            base.Dispose(fromIDisposableDispose);
        }

        /// <summary>
        /// Occurs when visible changed
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            FormsPanel.Visible = Visible;
        }

        /// <summary>
        /// Это событие происходит при любом изменении размера окна, включая
        /// его привязку/отвязку к намагниченным областям. Различия между OnTopFormResize не ясны
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            SetFormsPanelBounds();
            ApplyTopFormMargins();
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Create controls
        /// </summary>
        /// <returns>controls</returns>
        protected override ControlCollection CreateControlsInstance()
        {
            return InternalControls;
        }

        #endregion Protected section

        #region Private section
        #region Received events

        /// <summary>
        /// Occurs when the positioner was disposed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPositionerDisposed(object sender, EventArgs e)
        {
            Positioner = null;
        }


        /// <summary>
        /// Это событие происходит при закрытии окна.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">e</param>
        private void OnTopFormChanged(object sender, ControlSwitchedEventArgs e)
        {
            if (e.OldControl != null)
            {
                e.OldControl.Resize -= OnTopFormResize;
            }

            SetFormsPanelBounds();
            ApplyTopFormMargins();

            if (e.NewControl != null)
            {
                e.NewControl.Resize += OnTopFormResize;
            }

            return;
        }

        /// <summary>
        /// Это событие происходит при любом изменении размера окна, включая
        /// его привязку/отвязку к намагниченным областям.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnTopFormResize(object sender, EventArgs e)
        {
            //ApplyTopFormMargins();
        }

        /// <summary>
        /// On paint title bar
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnPaintTitleBar(object sender, PaintEventArgs e)
        {
            Font font = Font;
            //Font font = new Font(Font.Name, 8);
            //font = new Font("Calibri", 8, FontStyle.Bold);
            //font = new Font(font.Name, 9, FontStyle.Regular);
            //Color c1 = Color.White;
            //Color c1 = Color.White;
            //Color c2 = IsFocused ? Color.SkyBlue : Color.White;
            if(_sizeMode == zSizeMode.None)
            {
                //Point location = TitleBar.PointToScreen(e.Location);
                //ContinueMovementByMouse(location);
                
            }
            //_controls.SetChildIndex(this, 0);
            /*Rectangle r = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            using (LinearGradientBrush backBrush = new LinearGradientBrush(r, Color.Red, Color.Red, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }*/
            /*if (IsFocused == false)
            {
                if (font.Bold)
                    font = new Font(font, FontStyle.Regular);
            }
            else
            {
                if (font.Bold == false)
                    font = new Font(font, FontStyle.Bold);
            }*/
            if(IsFocused)
                _titleRenderer.Draw(font, e.Graphics, Color.White, Color.SkyBlue);
            else
                _titleRenderer.Draw(font, e.Graphics, Color.White, Color.White);
        }

        private void OnFormDecoratorPaint(object sender, PaintEventArgs e)
        {
            //Pen p = new Pen(Color.Red, 4);
            //this.BackColor = Color.Black;
            /*RectangleF Rect = new RectangleF(0, 0, this.Width, this.Height);
            GraphicsPath GraphPath = GetSPath(Rect);
            this.Region = new Region(GraphPath);
            using (Pen pen = new Pen(Color.Lime, 1.75f))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }*/
            //e.Graphics.DrawLine(p, new Point(0, 0), new Point(0, 100));
            /*if (ClientRectangle.Width != 0)
            {
                using (LinearGradientBrush backBrush = new LinearGradientBrush(ClientRectangle, Color.Red, Color.Blue, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(backBrush, ClientRectangle);
                    e.Graphics.Save();
                }
            }*/
        }
        GraphicsPath GetRoundPath(RectangleF Rect, int radius)
        {
            float r2 = radius / 2f;
            GraphicsPath GraphPath = new GraphicsPath();

            GraphPath.AddArc(Rect.X, Rect.Y, radius, radius, 180, 90);
            GraphPath.AddLine(Rect.X + r2, Rect.Y, Rect.Width - r2, Rect.Y);
            GraphPath.AddArc(Rect.X + Rect.Width - radius, Rect.Y, radius, radius, 270, 90);
            GraphPath.AddLine(Rect.Width, Rect.Y + r2, Rect.Width, Rect.Height - r2);
            GraphPath.AddArc(Rect.X + Rect.Width - radius,
                                Rect.Y + Rect.Height - radius, radius, radius, 0, 90);
            GraphPath.AddLine(Rect.Width - r2, Rect.Height, Rect.X + r2, Rect.Height);
            GraphPath.AddArc(Rect.X, Rect.Y + Rect.Height - radius, radius, radius, 90, 90);
            GraphPath.AddLine(Rect.X, Rect.Height - r2, Rect.X, Rect.Y + r2);

            GraphPath.CloseFigure();
            return GraphPath;
        }
        GraphicsPath GetSPath(RectangleF Rect)
        {
            GraphicsPath GraphPath = new GraphicsPath();
            GraphPath.AddLine(Rect.X, Rect.Y, Rect.Width, Rect.Y);
            GraphPath.AddLine(Rect.Width, Rect.Y, Rect.Width, Rect.Height);
            GraphPath.AddLine(Rect.Width, Rect.Height, Rect.X, Rect.Height);
            GraphPath.AddLine(Rect.X, Rect.Height, Rect.X, Rect.Y);
            GraphPath.CloseFigure();
            return GraphPath;
        }
        /// <summary>
        /// On un-hightlight timer
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">even argument</param>
        private void OnUnhighlightTimer(object sender, EventArgs e)
        {
            int buttonIndex = _titleRenderer.TitleBarButtonIndexUnderMouse;
            if (buttonIndex >= 0)
            {
                Point mousePos = TitleBar.PointToClient(Control.MousePosition);

                _titleRenderer.UpdateTitleBarButtonIndexUnderMouse(mousePos);

                if (_titleRenderer.TitleBarButtonIndexUnderMouse != buttonIndex)
                {
                    TitleBar.Invalidate();
                }
            }
        }

        private void SetFront(MouseEventArgs e)
        {
            //Point location = TitleBar.PointToScreen(e.Location);
            //Point p0 = new Point(location.X, location.Y);
            //BeginMovementByMouse(p0);
            TryMove();
            //EndMovementByMouse();
        }
        private bool TryMove()
        {
            //if (_sizeMode != zSizeMode.Move)
            //    return false;
            //int dx = mouseScreenPos.X - _mouseDownScreenPos.X;
            //int dy = mouseScreenPos.Y - _mouseDownScreenPos.Y;
            if (_moving == false)
            {
                _moving = true;
                Positioner.StartMoveByMouse();
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
                EndMovementByMouse();

            }
            //Positioner.PerformMoveByMouse(_positionerPositionOnMouseDown.X + dx, _positionerPositionOnMouseDown.Y + dy);
            return true;
        }
        /// <summary>
        /// Происходит, когда пользователь щелкает мышью по заголовку окна
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseDownInTitleBar(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
            //BeginMovementByMouse(e.Location);
            //ContinueMovementByMouse(e.Location);
            //EndMovementByMouse();
            
            if (e.Button == MouseButtons.Left)
            {
                //SetFront(e);
                if (_titleRenderer.TitleBarButtonIndexUnderMouse >= 0)
                {
                    EventHandler handler = null;

                    int index = -1;
                    if (_titleRenderer.ShowContextMenuButton)
                    {
                        index++;
                        if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                        {
                            handler = ContextButtonClick;
                        }
                    }

                    if (_titleRenderer.ShowAutohideButton)
                    {
                        index++;
                        if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                        {
                            handler = AutohideButtonClick;
                        }
                    }

                    if (_titleRenderer.ShowCloseButton)
                    {
                        index++;
                        if (index == _titleRenderer.TitleBarButtonIndexUnderMouse)
                        {
                            handler = CloseButtonClick;
                        }
                    }

                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
                else
                {
                    BeginMovementByMouse(TitleBar.PointToScreen(e.Location));
                }
            }
        }

        /// <summary>
        /// Это событие происходит, когда указатель мыши двигается через
        /// заголовок окна.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseMoveInTitleBar(object sender, MouseEventArgs e)
        {
            if (_sizeMode == zSizeMode.Move)
            {
                Point location = TitleBar.PointToScreen(e.Location);
                ContinueMovementByMouse(location);

                return;
            }

            int buttonIndex = _titleRenderer.TitleBarButtonIndexUnderMouse;
            _titleRenderer.UpdateTitleBarButtonIndexUnderMouse(e.Location);
            if (buttonIndex != _titleRenderer.TitleBarButtonIndexUnderMouse)
            {
                TitleBar.Invalidate();
            }

            Cursor cursor = Cursors.Default;
            if (buttonIndex >= 0)
            {
                cursor = Cursors.Hand;
            }
            else if (_positioner != null)
            {
                if (_positioner.CanMove)
                {
                    cursor = Cursors.Default;
                }
            }

            TitleBar.Cursor = cursor;
        }

        /// <summary>
        /// Это событие происходит при отжатии клавиши мыши установленной
        /// на заголовке окна.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseUpFromTitleBar(object sender, MouseEventArgs e)
        {
            EndMovementByMouse();
        }


        /// <summary>
        /// Происходит, когда пользователь щелкает по верхней границе окна,
        /// например захватывает верхнюю границу окна для изменения размера.
        /// Это событие происходит только для плавающего окна, не примагниченного.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseDownInTopMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
            Focus();
            if (e.Button == MouseButtons.Left && Positioner.CanSizeTop && CanResizeByMouse)
            {
                _mouseDownScreenPos = TopMargin.PointToScreen(e.Location);
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
                _sizeMode = zSizeMode.Top;
            }
        }

        /// <summary>
        /// On mouse move over title top margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseMoveInTopMargin(object sender, MouseEventArgs e)
        {
            if (_sizeMode == zSizeMode.Top)
            {
                Point location = LeftMargin.PointToScreen(e.Location);
                int dy = location.Y - _mouseDownScreenPos.Y;

                Rectangle bounds = new Rectangle();
                bounds.X = _positionerPositionOnMouseDown.X;
                bounds.Y = _positionerPositionOnMouseDown.Y + dy;
                bounds.Width = _positionerSizeOnMouseDown.Width;
                bounds.Height = _positionerSizeOnMouseDown.Height - dy;

                Positioner.Bounds = bounds;
            }
        }

        /// <summary>
        /// On mouse released from top margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseUpFromTopMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
        }

        private void OnHeaderFormChanged(object sender, EventArgs e)
        {
            TitleBar.Text = m_general_form.Text;
            ApplyTopFormMargins();
        }
        /// <summary>
        /// On mouse down in title left margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseDownInLeftMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;

            if (e.Button == MouseButtons.Left && Positioner.CanSizeLeft && CanResizeByMouse)
            {
                _mouseDownScreenPos = LeftMargin.PointToScreen(e.Location);
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
                _sizeMode = zSizeMode.Left;
            }
        }

        /// <summary>
        /// On mouse move over title left margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseMoveInLeftMargin(object sender, MouseEventArgs e)
        {
            if (_sizeMode == zSizeMode.Left)
            {
                Point location = LeftMargin.PointToScreen(e.Location);
                int dx = location.X - _mouseDownScreenPos.X;

                Rectangle bounds = new Rectangle();
                bounds.X = _positionerPositionOnMouseDown.X + dx;
                bounds.Width = _positionerSizeOnMouseDown.Width - dx;
                bounds.Y = _positionerPositionOnMouseDown.Y;
                bounds.Height = _positionerSizeOnMouseDown.Height;

                Positioner.Bounds = bounds;
            }
        }

        /// <summary>
        /// On mouse released from left margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseUpFromLeftMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
        }


        /// <summary>
        /// On mouse down in title right margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseDownInRightMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;

            if (e.Button == MouseButtons.Left && Positioner.CanSizeRight && CanResizeByMouse)
            {
                _mouseDownScreenPos = RightMargin.PointToScreen(e.Location);
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
                _sizeMode = zSizeMode.Right;
            }
        }

        /// <summary>
        /// On mouse move over title right margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseMoveInRightMargin(object sender, MouseEventArgs e)
        {
            if (_sizeMode == zSizeMode.Right)
            {
                Point location = RightMargin.PointToScreen(e.Location);
                int dx = location.X - _mouseDownScreenPos.X;

                Positioner.Size = new Size(_positionerSizeOnMouseDown.Width + dx, Positioner.Size.Height);
            }
        }

        /// <summary>
        /// On mouse released from right margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseUpFromRightMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
        }


        /// <summary>
        /// On mouse down in title bottom margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseDownInBottomMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;

            if (e.Button == MouseButtons.Left && Positioner.CanSizeBottom && CanResizeByMouse)
            {
                _mouseDownScreenPos = BottomMargin.PointToScreen(e.Location);
                _positionerPositionOnMouseDown = Positioner.Location;
                _positionerSizeOnMouseDown = Positioner.Size;
                _sizeMode = zSizeMode.Bottom;
            }
        }

        /// <summary>
        /// On mouse move over title bottom margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseMoveInBottomMargin(object sender, MouseEventArgs e)
        {
            if (_sizeMode == zSizeMode.Bottom)
            {
                Point location = BottomMargin.PointToScreen(e.Location);
                int dy = location.Y - _mouseDownScreenPos.Y;

                Positioner.Size = new Size(
                   Positioner.Size.Width,
                   _positionerSizeOnMouseDown.Height + dy);
            }
        }

        /// <summary>
        /// On mouse released from bottom margin
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arg</param>
        private void OnMouseUpFromBottomMargin(object sender, MouseEventArgs e)
        {
            _sizeMode = zSizeMode.None;
        }




        /// <summary>
        /// Occurs when positioner can move changed
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPositionerCanMoveChanged(object sender, EventArgs e)
        {
            Cursor cursor = Cursors.Default;
            if (_positioner != null)
            {
                if (_positioner.CanMove)
                {
                    cursor = Cursors.Default;
                }
            }

            TitleBar.Cursor = cursor;
        }

        /// <summary>
        /// Occurs when positioner can size left changed
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPositionerCanSizeLeftChanged(object sender, EventArgs e)
        {
            Cursor cursor = Cursors.Default;
            if (_positioner != null)
            {
                if (_positioner.CanSizeLeft && CanResizeByMouse)
                {
                    cursor = Cursors.SizeWE;
                }
            }

            LeftMargin.Cursor = cursor;
        }

        /// <summary>
        /// Occurs when positioner can size right changed
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPositionerCanSizeRightChanged(object sender, EventArgs e)
        {
            Cursor cursor = Cursors.Default;
            if (_positioner != null)
            {
                if (_positioner.CanSizeRight && CanResizeByMouse)
                {
                    cursor = Cursors.SizeWE;
                }
            }

            RightMargin.Cursor = cursor;
        }

        /// <summary>
        /// Occurs when positioner can size top changed
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPositionerCanSizeTopChanged(object sender, EventArgs e)
        {
            Cursor cursor = Cursors.Default;
            if (_positioner != null)
            {
                if (_positioner.CanSizeTop && CanResizeByMouse)
                {
                    cursor = Cursors.SizeNS;
                }
            }

            TopMargin.Cursor = cursor;
        }

        /// <summary>
        /// Occurs when positioner can size bottom changed
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPositionerCanSizeBottomChanged(object sender, EventArgs e)
        {
            Cursor cursor = Cursors.Default;
            if (_positioner != null)
            {
                if (_positioner.CanSizeBottom && CanResizeByMouse)
                {
                    cursor = Cursors.SizeNS;
                }
            }

            BottomMargin.Cursor = cursor;
        }


        #endregion Received events

        /// <summary>
        /// Accessor of the controls
        /// </summary>
        private FormsDecoratorControlCollection InternalControls
        {
            get
            {
                if (_controls == null)
                {
                    _controls = new FormsDecoratorControlCollection(this);
                }

                return _controls;
            }
        }

        /// <summary>
        /// Accessor of the forms panel
        /// </summary>
        private FormsContainer FormsPanel
        {
            get { return InternalControls.FormsPanel; }
        }

        /// <summary>
        /// Accessor of the forms title bar
        /// </summary>
        private Control TitleBar
        {
            get { return InternalControls.TitleBar; }
        }

        /// <summary>
        /// Accessor of the forms top margin
        /// </summary>
        private Control TopMargin
        {
            get { return InternalControls.TopMargin; }
        }

        /// <summary>
        /// Accessor of the forms left margin
        /// </summary>
        private Control LeftMargin
        {
            get { return InternalControls.LeftMargin; }
        }

        /// <summary>
        /// Accessor of the forms right margin
        /// </summary>
        private Control RightMargin
        {
            get { return InternalControls.RightMargin; }
        }

        /// <summary>
        /// Accessor of the forms bottom margin
        /// </summary>
        private Control BottomMargin
        {
            get { return InternalControls.BottomMargin; }
        }


        /// <summary>
        /// Apply top form margins
        /// </summary>
        private void ApplyTopFormMargins()
        {
            FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;

            Margins magins = FormWrapper.GetMargins(forms.TopControl);

            TopMargin.Top = FormsPanel.Top;
            TopMargin.Left = FormsPanel.Left;
            TopMargin.Width = FormsPanel.Width;
            TopMargin.Height = magins.Bottom;
            //-- для лучшей отзывчивости ресайза окна сверху
            //TopMargin.Location = new Point(TopMargin.Location.X, -6);
            TitleBar.Top = TopMargin.Bottom;
            TitleBar.Left = TopMargin.Left;
            TitleBar.Width = TopMargin.Width;
            TitleBar.Height = magins.Top - magins.Bottom + 2;

            BottomMargin.Top = FormsPanel.Bottom - magins.Bottom;
            BottomMargin.Left = FormsPanel.Left;
            BottomMargin.Width = FormsPanel.Width;
            BottomMargin.Height = magins.Bottom;


            LeftMargin.Top = TitleBar.Bottom;
            LeftMargin.Left = FormsPanel.Left;
            LeftMargin.Width = magins.Left;
            LeftMargin.Height = BottomMargin.Top - TitleBar.Bottom;

            RightMargin.Top = TitleBar.Bottom;
            RightMargin.Left = FormsPanel.Right - magins.Right;
            RightMargin.Width = magins.Right;
            RightMargin.Height = BottomMargin.Top - TitleBar.Bottom;


            Form selectedForm = forms.TopControl as Form;
            if (selectedForm != null)
            {
                _titleRenderer.Icon = selectedForm.Icon;
                _titleRenderer.Text = selectedForm.Text;
                //TitleBar.Height = 80;
                selectedForm.Bounds = FormsPanel.ClientRectangle;
            }

            _titleRenderer.TitleBarBounds = TitleBar.ClientRectangle;


            TitleBar.Invalidate();
        }

        /// <summary>
        /// Set the bounds of the forms panel
        /// </summary>
        private void SetFormsPanelBounds()
        {
            //-- Эта секция обрабатывается, когда окно отсоединено от намагниченной области,
            //-- т.е. границы устанавливаются для плавающего окна
            if (CanResizeByMouse)
            {
                Rectangle nr = new Rectangle(-5, -7, ClientRectangle.Width + 10, ClientRectangle.Height + 10);
                //Rectangle nr = new Rectangle(+15, +7, ClientRectangle.Width - 25, ClientRectangle.Height - 25);
                FormsPanel.Bounds = nr;
                BorderStyle = BorderStyle.FixedSingle;
                //FormsPanel.Visible = false;
            }
            //-- Эта секция обрабатывается, кода окно намагничено
            else
            {
                FormsContainerControlCollection forms = (FormsContainerControlCollection)FormsPanel.Controls;
                Margins magins = FormWrapper.GetMargins(forms.TopControl);
                FormsPanel.Bounds = new Rectangle(-magins.Left, -magins.Bottom, ClientSize.Width + magins.Left + magins.Right, ClientSize.Height + 2 * magins.Bottom);
                BorderStyle = BorderStyle.FixedSingle;
            }
        }

        #endregion Private section
    }
}
