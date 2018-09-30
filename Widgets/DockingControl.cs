using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

// Allow an arbitrary control to be moved between docking positions and resized
// see here:
//https://www.codeguru.com/csharp/csharp/cs_syntax/controls/article.php/c5849/Docking-Control-in-C-That-Can-Be-Dragged-and-Resized.htm
class DockingControl : UserControl
{
	// Instance variables
	private Form _form;					// The Form hosting this docking control
	private DockingResize _resize;		// Provide resizing functionality
	private DockingHandle _handle;		// Handle for grabbing and moving
	private BorderControl _wrapper;		// Wrapper to place border around user control

	public DockingControl(Form form, DockStyle ds, Control userControl)
	{
        Initialize(form, ds, userControl);
	}
    void Initialize(Form form, DockStyle ds, Control userControl) {
        // Remember the form we are hosted on
        _form = form;

        // Create the resizing bar, gripper handle and border control
        _resize = new DockingResize(ds);
        _handle = new DockingHandle(this, ds);
        _wrapper = new BorderControl(userControl);

        // Wrapper should always fill remaining area
        _wrapper.Dock = DockStyle.Fill;

        // Define our own initial docking position for when we are added to host form
        this.Dock = ds;

        // NOTE: Order of array contents is important
        // Controls in the array are positioned from right to left when the 
        // form makes size/position calculations for docking controls, so the
        // _wrapper is placed last in calculation (therefore first in array) 
        // because we want it to fill the remaining space.
        Controls.AddRange(new Control[] { _wrapper, _handle, _resize });
    }
	public Form HostForm { get { return _form; } }

	// Override the base class property to allow extra work
	public override DockStyle Dock
	{
		get { return base.Dock; }

		set
		{
			// Our size before docking position is changed
			Size size = this.ClientSize;
		
			// Remember the current docking position
			DockStyle dsOldResize = _resize.Dock;

			// New handle size is dependant on the orientation of the new docking position
			_handle.SizeToOrientation(value);

			// Modify docking position of child controls based on our new docking position
			_resize.Dock = DockingControl.ResizeStyleFromControlStyle(value);
			_handle.Dock = DockingControl.HandleStyleFromControlStyle(value);

			// Now safe to update ourself through base class
			base.Dock = value;

			// Change in orientation occured?
			if (dsOldResize != _resize.Dock)
			{
				// Must update our client size to ensure the correct size is used when
				// the docking position changes.  We have to transfer the value that determines
				// the vector of the control to the opposite dimension
				if ((this.Dock == DockStyle.Top) || 
					(this.Dock == DockStyle.Bottom))
					size.Height = size.Width;
				else
					size.Width = size.Height;

				this.ClientSize = size;
			}

			// Repaint of the our controls 
			_handle.Invalidate();
			_resize.Invalidate();
		}
	}

	// Static variables defining colours for drawing
	private static Pen _lightPen = new Pen(Color.FromKnownColor(KnownColor.ControlLightLight));
	private static Pen _darkPen = new Pen(Color.FromKnownColor(KnownColor.ControlDark));
	private static Brush _plainBrush = Brushes.LightGray;

	// Static properties for read-only access to drawing colours
	public static Pen LightPen		{ get { return _lightPen;	} }
	public static Pen DarkPen		{ get { return _darkPen;	} }
	public static Brush PlainBrush	{ get { return _plainBrush; } }

	public static DockStyle ResizeStyleFromControlStyle(DockStyle ds)
	{
		switch(ds)
		{
		case DockStyle.Left:
			return DockStyle.Right;
		case DockStyle.Top:
			return DockStyle.Bottom;
		case DockStyle.Right:
			return DockStyle.Left;
		case DockStyle.Bottom:
			return DockStyle.Top;
		default:
			// Should never happen!
			throw new ApplicationException("Invalid DockStyle argument");
		}
	}

	public static DockStyle HandleStyleFromControlStyle(DockStyle ds)
	{
		switch(ds)
		{
		case DockStyle.Left:
			return DockStyle.Top;
		case DockStyle.Top:
			return DockStyle.Left;
		case DockStyle.Right:
			return DockStyle.Top;
		case DockStyle.Bottom:
			return DockStyle.Left;
		default:
			// Should never happen!
			throw new ApplicationException("Invalid DockStyle argument");
		}
	}

    private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // DockingControl
            // 
            this.Name = "DockingControl";
            this.Size = new System.Drawing.Size(155, 245);
            this.ResumeLayout(false);

    }
}

// A bar used to resize the parent DockingControl
class DockingResize : UserControl
{
	// Class constants
	private const int _fixedLength = 4;

	// Instance variables
	private Point _pointStart;
	private Point _pointLast;
	private Size _size;

	public DockingResize(DockStyle ds)
	{
		this.Dock = DockingControl.ResizeStyleFromControlStyle(ds);
		this.Size = new Size(_fixedLength, _fixedLength);
	}	

	protected override void OnMouseDown(MouseEventArgs e)
	{
		// Remember the mouse position and client size when capture occured
		_pointStart = _pointLast = PointToScreen(new Point(e.X, e.Y));
		_size = Parent.ClientSize;

		// Ensure delegates are called
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		// Cursor depends on if we a vertical or horizontal resize
		if ((this.Dock == DockStyle.Top) || 
			(this.Dock == DockStyle.Bottom))
			this.Cursor = Cursors.HSplit;
		else
			this.Cursor = Cursors.VSplit;

		// Can only resize if we have captured the mouse
		if (this.Capture)
		{
			// Find the new mouse position
			Point point = PointToScreen(new Point(e.X, e.Y));

			// Have we actually moved the mouse?
			if (point != _pointLast)
			{
				// Update the last processed mouse position
				_pointLast = point;

				// Find delta from original position
				int xDelta = _pointLast.X - _pointStart.X;
				int yDelta = _pointLast.Y - _pointStart.Y;

				// Resizing from bottom or right of form means inverse movements
				if ((this.Dock == DockStyle.Top) || 
					(this.Dock == DockStyle.Left))
				{
					xDelta = -xDelta;
					yDelta = -yDelta;
				}

				// New size is original size plus delta
				if ((this.Dock == DockStyle.Top) || 
					(this.Dock == DockStyle.Bottom))
					Parent.ClientSize = new Size(_size.Width, _size.Height + yDelta);
				else
					Parent.ClientSize = new Size(_size.Width + xDelta, _size.Height);

				// Force a repaint of parent so we can see changed appearance
				Parent.Refresh();
			}
		}

		// Ensure delegates are called
		base.OnMouseMove(e);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		// Create objects used for drawing
		Point[] ptLight = new Point[2];
		Point[] ptDark = new Point[2];
		Rectangle rectMiddle = new Rectangle();

		// Drawing is relative to client area
		Size sizeClient = this.ClientSize;

		// Painting depends on orientation
		if ((this.Dock == DockStyle.Top) || 
			(this.Dock == DockStyle.Bottom))
		{
			// Draw as a horizontal bar
			ptDark[1].Y = ptDark[0].Y = sizeClient.Height - 1;
			ptLight[1].X = ptDark[1].X = sizeClient.Width;
			rectMiddle.Width = sizeClient.Width;
			rectMiddle.Height = sizeClient.Height - 2;
			rectMiddle.X = 0;
			rectMiddle.Y = 1;
		}
		else if ((this.Dock == DockStyle.Left) || 
				 (this.Dock == DockStyle.Right))
		{
			// Draw as a vertical bar
			ptDark[1].X = ptDark[0].X = sizeClient.Width - 1;
			ptLight[1].Y = ptDark[1].Y = sizeClient.Height;
			rectMiddle.Width = sizeClient.Width - 2;
			rectMiddle.Height = sizeClient.Height;
			rectMiddle.X = 1;
			rectMiddle.Y = 0;
		}

		// Use colours defined by docking control that is using us
		pe.Graphics.DrawLine(DockingControl.LightPen, ptLight[0], ptLight[1]);
		pe.Graphics.DrawLine(DockingControl.DarkPen, ptDark[0], ptDark[1]);
		pe.Graphics.FillRectangle(DockingControl.PlainBrush, rectMiddle);

		// Ensure delegates are called
		base.OnPaint(pe);
	}
}

class DockingHandle : UserControl 
{
	// Class constants
	private const int _fixedLength = 12;
	private const int _hotLength = 20;
	private const int _offset = 3;
	private const int _inset = 3;

	// Instance variables
	private DockingControl _dockingControl = null;

	public DockingHandle(DockingControl dockingControl, DockStyle ds)
	{
		_dockingControl = dockingControl;
		this.Dock = DockingControl.HandleStyleFromControlStyle(ds);
		SizeToOrientation(ds);
	}	

	public void SizeToOrientation(DockStyle ds)
	{
		if ((ds == DockStyle.Top) || (ds == DockStyle.Bottom))
			this.ClientSize = new Size(_fixedLength, 0);
		else
			this.ClientSize = new Size(0, _fixedLength);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		// Can only move the DockingControl is we have captured the
		// mouse otherwise the mouse is not currently pressed
		if (this.Capture)
		{
			// Must have reference to parent object
			if (null != _dockingControl)
			{
				this.Cursor = Cursors.Hand;

				// Convert from client point of DockingHandle to client of DockingControl
				Point screenPoint = PointToScreen(new Point(e.X, e.Y));
				Point parentPoint =  _dockingControl.HostForm.PointToClient(screenPoint);

				// Find the client rectangle of the form
				Size parentSize = _dockingControl.HostForm.ClientSize;

				// New docking position is defaulted to current style
				DockStyle ds = _dockingControl.Dock;

				// Find new docking position
				if (parentPoint.X < _hotLength)
				{
					ds = DockStyle.Left;
				}
				else if (parentPoint.Y < _hotLength)
				{
					ds = DockStyle.Top;
				}
				else if (parentPoint.X >= (parentSize.Width - _hotLength))
				{
					ds = DockStyle.Right;
				}
				else if (parentPoint.Y >= (parentSize.Height - _hotLength))
				{
					ds = DockStyle.Bottom;
				}

				// Update docking position of DockingControl we are part of
				if (_dockingControl.Dock != ds)
					_dockingControl.Dock = ds;
			}
		}
		else
			this.Cursor = Cursors.Default;

		// Ensure delegates are called
		base.OnMouseMove(e);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Size sizeClient = this.ClientSize;
		Point[] ptLight = new Point[4];
		Point[] ptDark = new Point[4];
			
		// Depends on orientation
		if ((_dockingControl.Dock == DockStyle.Top) || 
			(_dockingControl.Dock == DockStyle.Bottom))
		{
			int iBottom = sizeClient.Height - _inset - 1;
			int iRight = _offset + 2;

			ptLight[3].X = ptLight[2].X = ptLight[0].X = _offset;
			ptLight[2].Y = ptLight[1].Y = ptLight[0].Y = _inset;
			ptLight[1].X = _offset + 1;
			ptLight[3].Y = iBottom;
		
			ptDark[2].X = ptDark[1].X = ptDark[0].X = iRight;
			ptDark[3].Y = ptDark[2].Y = ptDark[1].Y = iBottom;
			ptDark[0].Y = _inset;
			ptDark[3].X = iRight - 1;
		}
		else
		{
			int iBottom = _offset + 2;
			int iRight = sizeClient.Width - _inset - 1;
			
			ptLight[3].X = ptLight[2].X = ptLight[0].X = _inset;
			ptLight[1].Y = ptLight[2].Y = ptLight[0].Y = _offset;
			ptLight[1].X = iRight;
			ptLight[3].Y = _offset + 1;
		
			ptDark[2].X = ptDark[1].X = ptDark[0].X = iRight;
			ptDark[3].Y = ptDark[2].Y = ptDark[1].Y = iBottom;
			ptDark[0].Y = _offset;
			ptDark[3].X = _inset;
		}
	
		Pen lightPen = DockingControl.LightPen;
		Pen darkPen = DockingControl.DarkPen;

	 	pe.Graphics.DrawLine(lightPen, ptLight[0], ptLight[1]);
	 	pe.Graphics.DrawLine(lightPen, ptLight[2], ptLight[3]);
	 	pe.Graphics.DrawLine(darkPen, ptDark[0], ptDark[1]);
	 	pe.Graphics.DrawLine(darkPen, ptDark[2], ptDark[3]);

		// Shift coordinates to draw section grab bar
		if ((_dockingControl.Dock == DockStyle.Top) || 
			(_dockingControl.Dock == DockStyle.Bottom))
		{
			for(int i=0; i<4; i++)
			{
				ptLight[i].X += 4;
				ptDark[i].X += 4;
			}
		}
		else
		{
			for(int i=0; i<4; i++)
			{
				ptLight[i].Y += 4;
				ptDark[i].Y += 4;
			}
		}

	 	pe.Graphics.DrawLine(lightPen, ptLight[0], ptLight[1]);
	 	pe.Graphics.DrawLine(lightPen, ptLight[2], ptLight[3]);
	 	pe.Graphics.DrawLine(darkPen, ptDark[0], ptDark[1]);
	 	pe.Graphics.DrawLine(darkPen, ptDark[2], ptDark[3]);

		// Ensure delegates are called
		base.OnPaint(pe);
	}
}

// Position the provided control inside a border to give a portrait picture effect
class BorderControl : UserControl 
{
	// Instance variables
	private int _borderWidth = 3;
	private int _borderDoubleWidth = 6;
	private Control _userControl = null;

	public BorderControl(Control userControl)
	{
		_userControl = userControl;
		Controls.Add(_userControl);
	}	

	public int BorderWidth
	{
		get 
		{ 
			return _borderWidth; 
		}

		set 
		{ 
			// Only interested if value has changed
			if (_borderWidth != value)
			{
				_borderWidth = value; 
				_borderDoubleWidth = _borderWidth + _borderWidth;

				// Force resize of control to get the embedded control 
				// respositioned according to new border width
				this.Size = this.Size; 
			}
		}
	}

	// Must reposition the embedded control whenever we change size
	protected override void OnResize(EventArgs e)
	{
		// Can be called before instance constructor
		if (null != _userControl)
		{
			Size sizeClient = this.Size;

			// Move the user control to enforce the border area we want	
			_userControl.Location = new Point(_borderWidth, _borderWidth);

			_userControl.Size = new Size(sizeClient.Width - _borderDoubleWidth, 
										 sizeClient.Height - _borderDoubleWidth);
		}

		// Ensure delegates are called
		base.OnResize(e);
	}
}
