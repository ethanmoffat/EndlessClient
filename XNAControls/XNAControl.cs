using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	/// <summary>
	/// The layer that the control should be drawn on. This is set by default but can be modified by the user.
	/// Lower layers (numerically) are drawn first. Higher numbers are drawn on top of things.
	/// These constants are rather arbitrarily defined.
	/// </summary>
	public enum ControlDrawLayer
	{
		/// <summary>
		/// The base layer for drawing controls. Offset from other components that might be drawn on layer 0.
		/// </summary>
		BaseLayer = 10,

		/// <summary>
		/// The layer on which to begin drawing dialogs.
		/// Each new dialog added to XNAControl.Dialogs will take the previous top dialog's draw order
		///		and add 10 to it as it's own base draw order.
		///	Controls contained in a dialog will be drawn up 1 level (see SetParent)
		/// </summary>
		DialogLayer = 30,
	}

	public abstract class XNAControl : DrawableGameComponent
	{
		/// <summary>
		/// Static members: Dialogs are pushed onto the stack as they are opened. Only the
		///		top-most dialog can be interacted with. Dialogs are popped off the stack
		///		once they are closed. Optionally, they are given a Closing event that has a
		///		particular dialog result.
		/// </summary>
		public static Stack<XNADialog> Dialogs = new Stack<XNADialog>();

		public static bool DrawOrderVisible = false;

		protected static SpriteBatch SpriteBatch;

		/// <summary>
		/// Returns the DrawLocation on the screen (no offset)
		/// Sets the location to draw on the screen for moving controls
		/// Only way of moving the control
		/// </summary>
		public Vector2 DrawLocation
		{
			get { return drawLocation; }
			set
			{
				drawLocation = value;
				drawArea.X = (int)value.X;
				drawArea.Y = (int)value.Y;

				//update all the child offsets in this code recursively
				foreach (XNAControl child in children)
				{
					child.SetParent(this);
				}
			}
		}

		/// <summary>
		/// Returns the Draw Area (no offset)
		/// </summary>
		public Rectangle DrawArea { get { return drawArea; } }

		/// <summary>
		/// Returns the Draw Area, factoring in any offset (this would be set by parent control)
		/// </summary>
		public Rectangle DrawAreaWithOffset
		{
			get
			{
				return new Rectangle(drawArea.X + xOff, drawArea.Y + yOff, drawArea.Width, drawArea.Height);
			}
		}

		/// <summary>
		/// Gets the very top parent control of this control.
		/// </summary>
		public XNAControl TopParent
		{
			get
			{
				XNAControl ret = parent;
				if (ret != null)
					while (ret.parent != null)
						ret = ret.parent;
				return ret;
			}
		}
		
		public bool MouseOver
		{
			get
			{
				MouseState state = Mouse.GetState();
				return DrawAreaWithOffset.ContainsPoint(state.X, state.Y);
			}
		}

		public bool MouseOverPreviously
		{
			get
			{
				return DrawAreaWithOffset.ContainsPoint(PreviousMouseState.X, PreviousMouseState.Y);
			}
		}

		protected MouseState PreviousMouseState { get; set; }
		protected KeyboardState PreviousKeyState { get; set; }

		public event EventHandler OnMouseOver;
		public event EventHandler OnMouseEnter;
		public event EventHandler OnMouseLeave;

		protected Vector2 drawLocation;
		protected Rectangle drawArea;

		protected void _setSize(int width, int height)
		{
			drawArea.Width = width;
			drawArea.Height = height;

			if (SizeChanged != null)
				SizeChanged(null, null);
		}

		protected XNAControl parent;
		protected int xOff, yOff;

		protected bool shouldClickDrag = true;

		protected EventHandler SizeChanged;

		protected readonly List<XNAControl> children = new List<XNAControl>();
		protected List<Type> IgnoreDialogs= new List<Type>(); 
		
		/// <summary>
		/// Construct a generic XNAControl with an encapsulating game, a location on screen, and an area.
		/// Side effects: adds this control to the game's components collection
		/// </summary>
		/// <param name="location">Location to draw the control on screen</param>
		/// <param name="area">The draw area of the control</param>
		public XNAControl(Vector2? location, Rectangle? area) 
			: base(XNAControls.IsInitialized ? XNAControls.Game : null)
		{
			drawLocation = location ?? new Vector2(0, 0);
			drawArea = area ?? new Rectangle((int)drawLocation.X, (int)drawLocation.Y, 1, 1);

			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			parent = null;
			if(SpriteBatch == null)
				SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

			xOff = yOff = 0;

			DrawOrder = (int)ControlDrawLayer.BaseLayer; //normal controls drawn at this level

			Game.Components.Add(this); //add this control to the game components
		}

		/// <summary>
		/// Instantiates the control but does NOT add this control to the components collection.
		/// Parent is set to the specified parent control (null for no parent)
		/// </summary>
		/// <param name="location">Draw location on the screen, from the top-left point (DrawLocation) of the parent control (or 0,0 for no parent)</param>
		/// <param name="area">Area of the control. Provides a rectangle for SpriteBatch draw operations</param>
		/// <param name="parentControl">Parent control. Null for no parent. Offsets updated automatically. Parent controls will draw their own children.</param>
		public XNAControl(Vector2? location = null, Rectangle? area = null, XNAControl parentControl = null)
			: base(XNAControls.IsInitialized ? XNAControls.Game : null)
		{
			drawLocation = location ?? new Vector2(0, 0);
			drawArea = area ?? new Rectangle((int)drawLocation.X, (int)drawLocation.Y, 1, 1);

			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			if(SpriteBatch == null)
				SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
			xOff = yOff = 0;

			DrawOrder = (int)ControlDrawLayer.BaseLayer;

			if(parentControl != null)
				SetParent(parentControl);
		}

		/// <summary>
		/// Set the parent of this control to the specified control and update relevant members
		/// </summary>
		/// <param name="newParent">The new parent control</param>
		public void SetParent(XNAControl newParent)
		{
			//When newParent is null:		this control is in Game.Components. Any children are removed from Game.Components. This is removed from parent's children.
			//When newParent is not null:	this control is removed from Game.Components. This is added to parent's child controls.
			if(newParent == null)
			{
				if (parent != null && parent.children.Contains(this))
				{
					parent.children.Remove(this);
					//update DrawOrder?
				}

				if (!Game.Components.Contains(this))
					Game.Components.Add(this);

				parent = null;
				xOff = yOff = 0;
			}
			else
			{
				if (Game.Components.Contains(this))
					Game.Components.Remove(this);

				parent = newParent;

				if (!parent.children.Contains(this))
					parent.children.Add(this);

				//DrawOrder of children is auto-updated in OnDrawOrderChanged
				DrawOrder = parent.DrawOrder + 1;

				//update offsets of children
				UpdateOffsets();
			}

			if (children.Count > 0)
				children.Sort((x, y) => x.DrawOrder - y.DrawOrder);
		}

		public XNAControl GetParent()
		{
			return parent;
		}

		//helper for SetParent
		private void UpdateOffsets()
		{
			xOff = parent.DrawAreaWithOffset.X;
			yOff = parent.DrawAreaWithOffset.Y;

			foreach (XNAControl child in children)
				child.UpdateOffsets();
		}

		protected virtual bool ShouldUpdate()
		{
			//precondition - update should only happen when the game window is active!
			if (!Game.IsActive) return false;

			//IgnoreDialogs contains dialogs that if shown, the control will update anyway
			//This is a special case for EndlessClient but I can see it being useful elsewhere too
			if (Visible && Dialogs.Count > 0 && IgnoreDialogs.Contains(Dialogs.Peek().GetType()))
				return true;

			if (!Visible || (Dialogs.Count > 0 && (Dialogs.Peek() != TopParent as XNADialog || TopParent == null)))
				return false;

			return true;
		}

		/// <summary>
		/// Call this method with the typeof(DialogType) to ignore that type of dialog when checking for modal dialogs during call to ShouldUpdate
		/// <para>Multiple dialog types can be ignored</para>
		/// </summary>
		/// <param name="dlgType">Type of the dialog to ignore</param>
		public void IgnoreDialog(Type dlgType)
		{
			if(IgnoreDialogs.Contains(dlgType))
				throw new ArgumentException("That dialog is already a part of the ignored dialogs list for this control.", "dlgType");

			IgnoreDialogs.Add(dlgType);
		}

		/// <summary>
		/// Updates base class properties and child controls. Sets previous mouse state. Call this after the derived class does it's update logic.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			if (drawArea.Width == 0 || drawArea.Height == 0)
				throw new InvalidOperationException("The drawn area of the control must not be 0.");

			if (!Visible)
				return;

			if (MouseOver && OnMouseOver != null)
				OnMouseOver(this, new EventArgs());

			if(MouseOver && !MouseOverPreviously && OnMouseEnter != null)
				OnMouseEnter(this, new EventArgs());
			else if(!MouseOver && MouseOverPreviously && OnMouseLeave != null)
				OnMouseLeave(this, new EventArgs());

			for (int i = 0; i < children.Count; ++i)
				children[i].Update(gameTime);

			if (TopParent == null) //child controls can have negative offsets! only check for TopParents
			{
				Rectangle gdm = Game.Window.ClientBounds;
				if (gdm.Width > 0 && gdm.Height > 0) //when the client is minimized, this goes to shit
				{
					if (DrawLocation.X < 0)
						DrawLocation = new Vector2(0, DrawLocation.Y);
					if (DrawLocation.Y < 0)
						DrawLocation = new Vector2(DrawLocation.X, 0);
					if (DrawLocation.X > gdm.Width - DrawAreaWithOffset.Width)
						DrawLocation = new Vector2(gdm.Width - DrawAreaWithOffset.Width, DrawLocation.Y);
					if (DrawLocation.Y > gdm.Height - DrawAreaWithOffset.Height)
						DrawLocation = new Vector2(DrawLocation.X, gdm.Height - DrawAreaWithOffset.Height);
				}
			}

			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			base.Update(gameTime);
		}

		private SpriteFont dbg;
		/// <summary>
		/// Draws any child controls. Call this after the derived class does it's draw logic.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			if (!Visible) //child controls will not be drawn if the parent control is not visible
				return;

			for (int i = 0; i < children.Count; ++i)
				children[i].Draw(gameTime);

			if (DrawOrderVisible)
			{
				//used for debugging draworders. Uncomment these lines to see the draw order of each control. 
				//Updated dynamically with changing parents/children and opening/closing dialogs
				if (dbg == null)
					dbg = Game.Content.Load<SpriteFont>("dbg");
				SpriteBatch.Begin();
				SpriteBatch.DrawString(dbg, "" + DrawOrder, new Vector2(DrawAreaWithOffset.X + 3, DrawAreaWithOffset.Y + 3), Color.White);
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}
		
		/// <summary>
		/// Change drawing order for all child controls when this control's draw order is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnDrawOrderChanged(object sender, EventArgs args)
		{
			base.OnDrawOrderChanged(sender, args);

			foreach(XNAControl child in children)
			{
				child.DrawOrder = DrawOrder + 1;
			}

			if(parent != null && parent.children.Count > 1)
				parent.children.Sort((x, y) => x.DrawOrder >  y.DrawOrder ? 1 : x.DrawOrder == y.DrawOrder ? 0 : -1);
		}

		/// <summary>
		/// Called when a child control is being dragged and the parent should not respond to click drag
		/// Example: scroll bar being dragged within a dialog
		/// </summary>
		/// <param name="suppress"></param>
		protected void SuppressParentClickDrag(bool suppress)
		{
			if (parent != null)
			{
				parent.shouldClickDrag = !suppress;
				parent.SuppressParentClickDrag(suppress);
			}
		}

		/// <summary>
		/// Closes a component, removing it from the list of Game components in encapsulatingGame
		/// </summary>
		public virtual void Close()
		{			
			//remove all children from the game components list too
			foreach (XNAControl child in children)
				child.Close();

			if (Game.Components.Contains(this))
				Game.Components.Remove(this);

			Dispose();
		}
	}
}
