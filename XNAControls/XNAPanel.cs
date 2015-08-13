using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace XNAControls
{
	public class XNAComponentPanel : DrawableGameComponent
	{
		public List<DrawableGameComponent> Components { get; set; }

		public XNAComponentPanel(Game game)
			: base(game)
		{
			Components = new List<DrawableGameComponent>();
		}

		public override void Initialize()
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Initialize();

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			for (int i = 0; i < Components.Count; i++)
				Components[i].Draw(gameTime);

			base.Draw(gameTime);
		}

		public void ClearTextBoxes()
		{
			List<DrawableGameComponent> tbs = Components.FindAll(x => x is XNATextBox);
			foreach (DrawableGameComponent tb in tbs)
			{
				((XNATextBox)tb).Text = "";
			}
		}

		protected override void Dispose(bool disposing)
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Dispose();

			base.Dispose(disposing);
		}
	}

		
	/// <summary>
	/// Note: I've changed XNAPanel to act more like an actual panel of controls, as in WinForms
	/// It derives from control so we have a lot of the convenience functions and can specify an
	/// offset for the child components
	/// 
	/// Use XNAComponentPanel to get old functionality
	/// </summary>
	public class XNAPanel : XNAControl
	{
		public Microsoft.Xna.Framework.Graphics.Texture2D BackgroundImage { get; set; }

		public XNAPanel(Rectangle? area = null)
			: base(area == null ? null : new Vector2?(new Vector2(area.Value.X, area.Value.Y)), area) { }
		
		public void ClearTextBoxes()
		{
			foreach(XNAControl child in children)
			{
				if(child is XNATextBox)
					(child as XNATextBox).Text = "";
			}
		}

		/// <summary>
		/// Sets the parent of the control to this XNAPanel instance. Rendering offsets are updated accordingly for the control and it's children.
		/// </summary>
		/// <param name="ctrl">The control to add this panel</param>
		/// <param name="force">Force override any existing parent for ctrl, default value false.</param>
		public void AddControl(XNAControl ctrl, bool force = false)
		{
			if(force || ctrl.TopParent == null)
				ctrl.SetParent(this);

			foreach(Type t in IgnoreDialogs)
				ctrl.IgnoreDialog(t);
		}

		/// <summary>
		/// Sets the parent of the control to null if the controls parent is this XNAPanel instance. Rendering offsets are updated accordingly for the control and it's children.
		/// </summary>
		/// <param name="ctrl">The control to remove from this panel</param>
		public void RemoveControl(XNAControl ctrl)
		{
			if (ctrl.TopParent == this)
				ctrl.SetParent(null);
		}

		//hide the base class method to set parent for this XNAPanel instance...
		public new void SetParent(XNAControl ctrl)
		{
			throw new InvalidOperationException("A panel may not have a parent. It should be a top-level control.");
		}

		public override void Draw(GameTime gameTime)
		{
			if (BackgroundImage != null)
			{
				SpriteBatch.Begin();
				SpriteBatch.Draw(BackgroundImage, DrawAreaWithOffset, Color.White);
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}
	}
}
