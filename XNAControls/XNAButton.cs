using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public class XNAButton : XNAControl
	{
		public delegate void ButtonClickEvent(object sender, EventArgs e = null);

		private string _text;
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				_TEXTture = Game.DrawText(_text, new System.Drawing.Font("Arial", 12), System.Drawing.Color.Black);
			}
		}

		private bool m_dragging;
		private readonly Texture2D _out; //texture for mouse out (primary display texture)
		private readonly Texture2D _over; //texture for mouse over
		private Texture2D _TEXTture; //get it?
		private Texture2D _drawTexture; //texture to be drawn, selected based on MouseOver in Update method

		/// <summary>
		/// If FlashSpeed is set, the button will change between over/out textures once every 'FlashSpeed' milliseconds
		/// </summary>
		public int? FlashSpeed { get; set; }

		private Rectangle? area; //nullable rectangle for optional area that will respond to mouse click/hover
		/// <summary>
		/// Get/set the area that should respond to a click event.
		/// If null, the entire button area responds to a click event.
		/// </summary>
		public Rectangle? ClickArea
		{
			get { return area; }
			set
			{
				area = value;
				if(area != null)
				{
					//factor in the draw coordinates and the offset from the parent to the new click area
					area = new Rectangle(area.Value.X + DrawAreaWithOffset.X, area.Value.Y + DrawAreaWithOffset.Y,
						area.Value.Width, area.Value.Height);
				}
			}
		}

		/// <summary>
		/// Construct a button where the textures for over/out are a part of a sprite sheet.
		/// </summary>
		/// <param name="sheet">The sprite sheet texture</param>
		/// <param name="location">Location to draw the button</param>
		/// <param name="outSource">Source within the sprite sheet that contains the texture to draw on MouseOut</param>
		/// <param name="overSource">Source within the sprite sheet that contains the texture to draw on MouseOver</param>
		public XNAButton(Texture2D sheet, Vector2 location, Rectangle? outSource = null, Rectangle? overSource = null)
			: base(location, null)
		{
			if (outSource == null && overSource == null)
				throw new Exception("Unable to create button without any image");

			drawArea = new Rectangle((int) location.X,
				(int) location.Y,
				outSource == null ? overSource.Value.Width : outSource.Value.Width,
				outSource == null ? overSource.Value.Height : outSource.Value.Height);

			ClickArea = null;

			if (outSource.HasValue)
			{
				_out = new Texture2D(sheet.GraphicsDevice, outSource.Value.Width, outSource.Value.Height);
				Color[] outData = new Color[outSource.Value.Width * outSource.Value.Height];
				sheet.GetData(0, outSource, outData, 0, outData.Length);
				_out.SetData(outData);
			}

			if (overSource.HasValue)
			{
				_over = new Texture2D(sheet.GraphicsDevice, overSource.Value.Width, overSource.Value.Height);
				Color[] overData = new Color[overSource.Value.Width * overSource.Value.Height];
				sheet.GetData(0, overSource, overData, 0, overData.Length);
				_over.SetData(overData);
			}

			_drawTexture = _out;
		}

		/// <summary>
		/// Construct a button where the textures for over/out are individual textures.
		/// </summary>
		/// <param name="textures">An array of length 2 containing an over texture and an out texture</param>
		/// <param name="location">Location to draw the button</param>
		public XNAButton(Texture2D[] textures, Vector2 location)
			: base(location, new Rectangle((int)location.X, (int)location.Y, textures[0].Width, textures[0].Height))
		{
			if (textures.Length != 2)
				throw new ArgumentException("You must specify an array of two textures for this constructor.");

			ClickArea = null;

			_out = textures[0];
			_over = textures[1];
			_drawTexture = _out;
		}

		public XNAButton(Vector2 location, string text = "default")
			: base(location, null)
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.button.png"))
			{
				_out = Texture2D.FromStream(Game.GraphicsDevice, s);
			}

			using(System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.button_hover.png"))
			{
				_over = Texture2D.FromStream(Game.GraphicsDevice, s);
			}

			ClickArea = null;
			//_setSize(_out.Width, _out.Height);
			_setSize(80, 30);

			Text = text;
			_drawTexture = _out;
		}

		public override void Update(GameTime gt)
		{
			if(!ShouldUpdate())
				return;

			if (MouseOver && OnClick != null && PreviousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released)
				OnClick(this);

			MouseState currentState = Mouse.GetState();

			if (MouseOver && OnClickDrag != null
				&& PreviousMouseState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Pressed
				&& shouldClickDrag && !m_dragging)
			{
				SuppressParentClickDrag(true);
				m_dragging = true;
			}
			else if (PreviousMouseState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released && m_dragging)
			{
				m_dragging = false;
				SuppressParentClickDrag(false);
			}

			if (m_dragging) //dragging does not require MouseOver - but starting dragging does
			{
				OnClickDrag(this);
			}
			
			if (!MouseOver && FlashSpeed != null && (int) gt.TotalGameTime.TotalMilliseconds%FlashSpeed == 0)
			{
				_drawTexture = _drawTexture == _over ? _out : _over;
			}
			else if (MouseOver)
			{
				_drawTexture = _over;
			}
			else if(FlashSpeed == null)
			{
				_drawTexture = _out;
			}

			PreviousMouseState = currentState;

			base.Update(gt);
		}
		
		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;
			SpriteBatch.Begin();
			
			//draw the background texture
			try
			{
				if(_drawTexture != null)
					SpriteBatch.Draw(_drawTexture, 
						new Rectangle((int)DrawLocation.X + xOff, (int)DrawLocation.Y + yOff, DrawArea.Width, DrawArea.Height), 
						Color.White);
			}
			catch(ArgumentException)
			{
				SpriteBatch.End();
				return;
			}
			catch(NullReferenceException)
			{
				SpriteBatch.End();
				return;
			}

			//draw the text string
			if (_TEXTture != null)
			{
				float xCoord = (DrawAreaWithOffset.Width / 2) - (_TEXTture.Width / 2) + DrawAreaWithOffset.X;
				float yCoord = (DrawAreaWithOffset.Height / 2) - (_TEXTture.Height / 2) + DrawAreaWithOffset.Y;
				SpriteBatch.Draw(_TEXTture, new Vector2(xCoord, yCoord), Color.White);
			}
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		public event ButtonClickEvent OnClick;
		public event ButtonClickEvent OnClickDrag;
	}
}
