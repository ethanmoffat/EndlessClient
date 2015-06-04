using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient
{
	public class TextSplitter
	{
		/// <summary>
		/// Gets or sets a string that is used as a measurement padding in front of every string
		/// <para>Note: this is NOT inserted in front of the strings being processed, but is used as padding for clipping measurement</para>
		/// </summary>
// ReSharper disable MemberCanBePrivate.Global
		public string LineStart { get; set; }
		/// <summary>
		/// Gets or sets a string that is inserted at the end of every line of text that is processed
		/// </summary>
		public string LineEnd { get; set; }
		/// <summary>
		/// Gets or sets the text to be processed
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets the number of pixels at which text will be wrapped to a new line
		/// </summary>
		public int LineLength { get; set; }
// ReSharper restore MemberCanBePrivate.Global
		/// <summary>
		/// Gets a value determining whether or not the text is long enough to require processing
		/// </summary>
		public bool NeedsProcessing { get { return _textIsOverflowFunc(Text); } }

		private Func<string, bool> _textIsOverflowFunc;
		private Font _font;
		private SpriteFont _spriteFont;

		public TextSplitter(string text, SpriteFont font)
		{
			Text = text;
			SetFont(font);
			ResetToDefaults();
		}

		public TextSplitter(string text, Font font)
		{
			Text = text;
			SetFont(font);
			ResetToDefaults();
		}

		/// <summary>
		/// Resets optional parameters to their default value
		/// </summary>
// ReSharper disable once MemberCanBePrivate.Global
		public void ResetToDefaults()
		{
			LineStart = "";
			LineEnd = "";
			LineLength = 200;
		}

// ReSharper disable once MemberCanBePrivate.Global
		public void SetFont(SpriteFont newFont)
		{
			if (_font != null)
			{
				_font.Dispose();
				_font = null;
			}
			_spriteFont = newFont;
			_textIsOverflowFunc = s => _spriteFontOverflow(s, _spriteFont);
		}

// ReSharper disable once MemberCanBePrivate.Global
		public void SetFont(Font newFont)
		{
			_spriteFont = null;
			if (_font != null)
			{
				_font.Dispose();
			}
			_font = newFont;
			_textIsOverflowFunc = s => _sdFontOverflow(s, _font);
		}

		/// <summary>
		/// Splits text into lines based on the class member parameters that have been set
		/// </summary>
		/// <returns>List of strings</returns>
		public List<string> SplitIntoLines()
		{
			string buffer = Text;
			buffer = buffer.TrimEnd(new[] {' '});
			List<string> retList = new List<string>();
			char[] whiteSpace = {' ', '\t', '\n'};
			string nextWord = "", newLine = "";
			while (buffer.Length > 0) //keep going until the buffer is empty
			{
				//get the next word
				bool endOfWord = true, lineOverFlow = true; //these are negative logic booleans: will be set to false when flagged
				while (buffer.Length > 0 && (endOfWord = !whiteSpace.Contains(buffer[0])) &&
					   (lineOverFlow = !_textIsOverflowFunc(LineStart + newLine + nextWord + LineEnd)))
				{
					nextWord += buffer[0];
					buffer = buffer.Remove(0, 1);
				}

				//flip the bools so the program reads more logically
				endOfWord = !endOfWord;
				lineOverFlow = !lineOverFlow;

				if (endOfWord)
				{
					newLine += nextWord + buffer[0];
					buffer = buffer.Remove(0, 1);
					nextWord = "";
				}
				else if (lineOverFlow)
				{
					if (LineEnd.Length > 0)
					{
						newLine += nextWord;
						nextWord = "";
					}

					if (newLine.Contains('\n'))
					{
						retList.AddRange(newLine.Split(new[] {'\n'}));
					}
					else
					{
						newLine += LineEnd;
						retList.Add(newLine);
					}
					newLine = "";
				}
				else
				{
					newLine += nextWord;
					retList.Add(newLine);
				}
			}

			return retList;
		}

		private bool _spriteFontOverflow(string toMeasure, SpriteFont font)
		{
			return font.MeasureString(toMeasure).X > LineLength;
		}

		private bool _sdFontOverflow(string toMeasure, Font font)
		{
			using(Bitmap b = new Bitmap(LineLength + 20, font.Height))
			using (Graphics g = Graphics.FromImage(b))
				return g.MeasureString(toMeasure, font).Width > LineLength;
		}
	}
}
