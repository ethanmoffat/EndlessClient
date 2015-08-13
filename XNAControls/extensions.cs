using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAFramework = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.IO;

namespace XNAControls
{
	public static class ExtensionMethods
	{
		public static bool ContainsPoint(this XNAFramework.Rectangle rect, int x, int y)
		{
			return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
		}

		public static bool ContainsPoint(this Rectangle rect, int x, int y)
		{
			return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
		}

		public static Texture2D ToTexture2D(this Bitmap bitmap, XNAFramework.Game game)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				return Texture2D.FromStream(game.GraphicsDevice, ms);
			}
		}

		public static Texture2D DrawRectangle(this XNAFramework.Game game, Size size, Color fillColor, Color? borderColor = null)
		{
			using (Bitmap bm = new Bitmap(size.Width, size.Height))
			{
				using (Graphics g = Graphics.FromImage(bm))
				{
					using (Brush brush = new SolidBrush(fillColor))
						g.FillRectangle(brush, 0, 0, size.Width, size.Height);

					using (Brush brush = new SolidBrush(borderColor ?? Color.Transparent))
					{
						using (Pen pen = new Pen(brush, 1))
							g.DrawRectangle(pen, 0, 0, size.Width - 1, size.Height - 1);
					}
				}

				return bm.ToTexture2D(game);
			}
		}

		public static Texture2D DrawText(this XNAFramework.Game game, string text, Font font, Color foreColor, int? textWidth = null, int rowSpacing = 0)
		{
			if (string.IsNullOrEmpty(text))
				return new Texture2D(game.GraphicsDevice, 1, 1);

			List<string> drawStrings = new List<string>();
			Size size;
			using (Bitmap tmpBitmap = new Bitmap(1, 1)) //added a bunch of using blocks around disposable resources
			{
				using (Graphics tmpGraphics = Graphics.FromImage(tmpBitmap))
				{
					size = tmpGraphics.MeasureString(text, font).ToSize();

					if (textWidth != null && size.Width > textWidth)
					{
						int heightfactor = (int)Math.Round((float)size.Width / textWidth.Value);
						size.Height *= heightfactor;
					}

					if(textWidth != null && size.Width > textWidth.Value)
						size.Width = textWidth.Value;

					// size can't have zero width or height
					if (size.Width == 0)
						size.Width = 1;
					if (size.Height == 0)
						size.Height = 1;

					if (textWidth == null)
					{
						drawStrings.Add(text);
					}
					else
					{
						string buffer = text, newLine = "";
						char[] whiteSpace = { ' ', '\t', '\n' };
						string nextWord = "";
						while (buffer.Length > 0) //keep going until the buffer is empty
						{
							//get the next word
							bool endOfWord = true, lineOverFlow = true;
							//keep going until:
							// - buffer is empty OR
							// - the end of the word is reached (found white space character) OR
							// - the line overflows
							while (buffer.Length > 0 && !(endOfWord = whiteSpace.Contains(buffer[0])) &&
								   !(lineOverFlow = !(tmpGraphics.MeasureString(newLine + nextWord, font).Width < size.Width)))
							{
								nextWord += buffer[0];
								buffer = buffer.Remove(0, 1);
							}

							if (endOfWord)
							{
								newLine += nextWord + buffer[0];
								buffer = buffer.Remove(0, 1);
								nextWord = "";
							}
							else if (lineOverFlow)
							{
								//possible condition where newLine is empty and nextWord is not
								//results in infinite loop - set newLine to nextWord here
								if (newLine.Length == 0 && nextWord.Length > 0)
								{
									newLine = nextWord;
									nextWord = "";
								}
								drawStrings.Add(newLine);
								newLine = "";
							}
							else
							{
								newLine += nextWord;
								drawStrings.Add(newLine);
							}
						}

						float height = drawStrings.Sum(s => tmpGraphics.MeasureString(s, font).Height + rowSpacing + 2);
						size.Height = (int)Math.Round(height);
					}
				}
			}

			// Resize
			using (Bitmap bitmap = new Bitmap(size.Width, size.Height))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					// Set drawing options
					graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

					int unitHeight = (int)graphics.MeasureString("WygqpTM", font).Height + rowSpacing;
					using (SolidBrush brush = new SolidBrush(foreColor))
					{
						int i = 0;
						foreach (string drawString in drawStrings)
						{
							int yCoord = textWidth == null ? 0 : (i * unitHeight);
							graphics.DrawString(drawString, font, brush, new Point(0, yCoord));
							i++;
						}
					}

					graphics.Flush();
					return bitmap.ToTexture2D(game);
				}
			}
		}
	}
}
