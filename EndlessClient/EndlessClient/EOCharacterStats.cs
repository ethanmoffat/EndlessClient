using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient
{
	public class EOCharacterStats : XNAControl
	{
		//Note: pretty much the only reason(s) I didn't use enums here are because I've been working with c++ a lot
		//		and because I didn't want to deal with typecasting the values away from enums for freaking everything

		private const int STR = 0, INT = 1, WIS = 2, AGI = 3, CON = 4, CHA = 5;
		private readonly XNALabel[] m_basicStats = new XNALabel[6];
		private readonly XNAButton[] m_arrows = new XNAButton[6];

		private const int HP = 0, TP = 1, DAM = 2, ACC = 3, ARM = 4, EVA = 5;
		private readonly XNALabel[] m_characterStats = new XNALabel[6];

		private const int NAME = 0, LEVEL = 1, GUILD = 2;
		private readonly XNALabel[] m_charInfo = new XNALabel[3];

		private const int WEIGHT = 0, STATPTS = 1, SKILLPTS = 2, ELEM = 3, 
			GOLD = 4, EXP = 5, TNL = 6, KARMA = 7;
		private readonly XNALabel[] m_otherInfo = new XNALabel[8];

		private readonly Character c;
		private bool m_training;

		public EOCharacterStats(XNAPanel parent) 
			: base(null, null, parent)
		{
			c = World.Instance.MainPlayer.ActiveCharacter;
		}

		public override void Initialize()
		{
			base.Initialize();

			//position for these: x=50, y = 8,26,44,...
			for (int i = 0; i < m_basicStats.Length; ++i)
			{
				m_basicStats[i] = new XNALabel(new Rectangle(50, 8 + i*18, 73, 13), "Microsoft Sans Serif", 8.5f)
				{
					Visible = true,
					ForeColor = System.Drawing.Color.White,
					AutoSize = false,
				};
				m_basicStats[i].SetParent(this);
				m_arrows[i] = new XNAButton(GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 27, true), new Vector2(106, 7 + i*18), new Rectangle(215, 386, 19, 15), new Rectangle(234, 386, 19, 15))
				{
					Visible = false, //for testing - this should only be visible when statpoints > 0
					FlashSpeed = 500
				};
				m_arrows[i].SetParent(this);
				m_arrows[i].OnClick += (s, e) =>
				{
					if (!m_training)
					{
						EODialog dlg = new EODialog("Do you want to train?", "Character training", XNADialogButtons.OkCancel, true);
						dlg.DialogClosing += (sender, args) =>
						{
							if (args.Result != XNADialogResult.OK) return;
							m_training = true;
						};
					}
					else
					{
						short index = (short)(m_arrows.ToList().FindIndex(_btn => _btn == s) + 1); //1-based index (server-side)
						if(!Handlers.StatSkill.AddStatPoint(index))
							EOGame.Instance.LostConnectionDialog();
					}
				};
			}

			//x=158, y = 8, 26, 44, ...
			for (int i = 0; i < m_characterStats.Length; ++i)
			{
				m_characterStats[i] = new XNALabel(new Rectangle(158, 8 + i * 18, 73, 13), "Microsoft Sans Serif", 8.5f)
				{
					Visible = true,
					ForeColor = System.Drawing.Color.White,
					AutoSize = false,
				};
				m_characterStats[i].SetParent(this);
			}

			for (int i = 0; i < m_otherInfo.Length; ++i)
			{
				m_otherInfo[i] = new XNALabel(new Rectangle(i < 4 ? 280 : 379 , 44 + (i%4)*18, i < 4 ? 60 : 94, 13), "Microsoft Sans Serif", 8.5f)
				{
					Visible = true,
					ForeColor = System.Drawing.Color.White,
					AutoSize = false,
				};
				m_otherInfo[i].SetParent(this);
			}

			//these labels have non-standard sizes so they're done individually
			//name= 280,8 144,13
			m_charInfo[NAME] = new XNALabel(new Rectangle(280, 8, 144, 13), "Microsoft Sans Serif", 8.5f)
			{
				Visible = true,
				ForeColor = System.Drawing.Color.White,
				AutoSize = false,
				Text = c.Name
			};
			//guild = 280,26 193,13
			m_charInfo[GUILD] = new XNALabel(new Rectangle(280, 26, 193, 13), "Microsoft Sans Serif", 8.5f)
			{
				Visible = true,
				ForeColor = System.Drawing.Color.White,
				AutoSize = false,
				Text = c.GuildName
			};
			//level = 453,8, 20,13
			m_charInfo[LEVEL] = new XNALabel(new Rectangle(453, 8, 20, 13), "Microsoft Sans Serif", 8.5f)
			{
				Visible = true,
				ForeColor = System.Drawing.Color.White,
				AutoSize = false,
			};
			foreach(XNALabel lbl in m_charInfo) lbl.SetParent(this);
		}

		public void Refresh()
		{
			for (int i = 0; i < m_basicStats.Length; ++i)
			{
				switch (i)
				{
					case STR:
						m_basicStats[i].Text = c.Stats.disp_str.ToString(CultureInfo.InvariantCulture);
						break;
					case INT:
						m_basicStats[i].Text = c.Stats.disp_int.ToString(CultureInfo.InvariantCulture);
						break;
					case WIS:
						m_basicStats[i].Text = c.Stats.disp_wis.ToString(CultureInfo.InvariantCulture);
						break;
					case AGI:
						m_basicStats[i].Text = c.Stats.disp_agi.ToString(CultureInfo.InvariantCulture);
						break;
					case CON:
						m_basicStats[i].Text = c.Stats.disp_con.ToString(CultureInfo.InvariantCulture);
						break;
					case CHA:
						m_basicStats[i].Text = c.Stats.disp_cha.ToString(CultureInfo.InvariantCulture);
						break;
				}
			}

			for (int i = 0; i < m_characterStats.Length; ++i)
			{
				switch (i)
				{
					case HP:
						m_characterStats[i].Text = c.Stats.hp.ToString(CultureInfo.InvariantCulture);
						break;
					case TP:
						m_characterStats[i].Text = c.Stats.tp.ToString(CultureInfo.InvariantCulture);
						break;
					case DAM:
						m_characterStats[i].Text = string.Format("{0} - {1}", c.Stats.mindam, c.Stats.maxdam);
						break;
					case ACC:
						m_characterStats[i].Text = c.Stats.accuracy.ToString(CultureInfo.InvariantCulture);
						break;
					case ARM:
						m_characterStats[i].Text = c.Stats.evade.ToString(CultureInfo.InvariantCulture);
						break;
					case EVA:
						m_characterStats[i].Text = c.Stats.armor.ToString(CultureInfo.InvariantCulture);
						break;
				}
			}

			for (int i = 0; i < m_otherInfo.Length; ++i)
			{
				switch (i)
				{
					case WEIGHT:
						m_otherInfo[i].Text = string.Format("{0} / {1}", c.Weight, c.MaxWeight);
						break;
					case STATPTS:
						m_otherInfo[i].Text = c.Stats.statpoints.ToString(CultureInfo.InvariantCulture);
						break;
					case SKILLPTS:
						m_otherInfo[i].Text = c.Stats.skillpoints.ToString(CultureInfo.InvariantCulture);
						break;
					case ELEM:
						//m_otherInfo[i].Text = "";
						break;
					case GOLD:
						InventoryItem inv;
						if ((inv = c.Inventory.Find(_i => _i.id == 1)).id == 1)
						{
							m_otherInfo[i].Text = inv.amount.ToString(CultureInfo.InvariantCulture);
						}
						break;
					case EXP:
						m_otherInfo[i].Text = c.Stats.exp.ToString(CultureInfo.InvariantCulture);
						break;
					case TNL:
						m_otherInfo[i].Text = (World.Instance.exp_table[c.Stats.level + 1] - c.Stats.exp).ToString(CultureInfo.InvariantCulture);
						break;
					case KARMA:
						//todo: figure out the different strings for karma values
						m_otherInfo[i].Text = "Neutral";
						break;
				}
			}

			m_charInfo[LEVEL].Text = c.Stats.level.ToString(CultureInfo.InvariantCulture);

			if (c.Stats.statpoints > 0)
			{
				foreach (XNAButton btn in m_arrows) btn.Visible = true;
			}
			else
			{
				foreach (XNAButton btn in m_arrows) btn.Visible = false;
				m_training = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach(XNALabel lbl in m_basicStats)
				lbl.Dispose();
			foreach(XNALabel lbl in m_characterStats)
				lbl.Dispose();
			foreach(XNALabel lbl in m_charInfo)
				lbl.Dispose();
			foreach(XNALabel lbl in m_otherInfo)
				lbl.Dispose();

			base.Dispose(disposing);
		}
	}
}
