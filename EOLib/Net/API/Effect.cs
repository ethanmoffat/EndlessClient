// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net
{
	public delegate void PlayerTakeSpikeDamageEvent(short damage, short hp, short maxhp);
	public delegate void OtherPlayerTakeSpikeDamageEvent(short playerID, byte playerPercentHealth, bool isPlayerDead, int damageAmount);
	public delegate void TimedMapDrainHPEvent(short damage, short hp, short maxhp, List<TimedMapHPDrainData> otherCharacterData);
	public delegate void TimedMapDrainTPEvent(short amount, short tp, short maxtp);
	public delegate void EffectPotionUseEvent(short playerID, int effectID);

	public enum EffectDamageType : byte
	{
		TimedDrainTP = 1,
		SpikeDamage = 2
	}

	public struct TimedMapHPDrainData
	{
		private readonly short _playerID;
		private readonly byte _playerPercentHealth;
		private readonly short _damageDealt;

		public short PlayerID { get { return _playerID; } }
		public byte PlayerPercentHealth { get { return _playerPercentHealth; } }
		public short DamageDealt { get { return _damageDealt; } }

		internal TimedMapHPDrainData(short playerID, byte percentHealth, short damageDealt)
		{
			_playerID = playerID;
			_playerPercentHealth = percentHealth;
			_damageDealt = damageDealt;
		}
	}

	partial class PacketAPI
	{
		public event Action OnTimedSpike;
		public event PlayerTakeSpikeDamageEvent OnPlayerTakeSpikeDamage;
		public event OtherPlayerTakeSpikeDamageEvent OnOtherPlayerTakeSpikeDamage;
		public event TimedMapDrainHPEvent OnTimedMapDrainHP;
		public event TimedMapDrainTPEvent OnTimedMapDrainTP;
		public event EffectPotionUseEvent OnEffectPotion;

		private void _createEffectMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.Spec), _handleEffectSpec, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.Admin), _handleEffectAdmin, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.Report), _handleEffectReport, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.TargetOther), _handleEffectTargetOther, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.Player), _handleEffectPlayer, true);
		}

		//sent to the player taking spike damage and map TP drains
		private void _handleEffectSpec(Packet pkt)
		{
			//1 in eoserv Map::TimedDrains - tp
			//2 in eoserv Character::SpikeDamage
			EffectDamageType damageType = (EffectDamageType) pkt.GetChar();
			switch (damageType)
			{
				case EffectDamageType.TimedDrainTP:
				{
					short amount = pkt.GetShort();
					short tp = pkt.GetShort();
					short maxtp = pkt.GetShort();

					if (OnTimedMapDrainTP != null)
						OnTimedMapDrainTP(amount, tp, maxtp);
				}
					break;
				case EffectDamageType.SpikeDamage:
				{
					short damage = pkt.GetShort();
					short hp = pkt.GetShort();
					short maxhp = pkt.GetShort();

					if (OnPlayerTakeSpikeDamage != null)
						OnPlayerTakeSpikeDamage(damage, hp, maxhp);
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		//sent to players around a player taking spike damage
		private void _handleEffectAdmin(Packet pkt)
		{
			if (OnOtherPlayerTakeSpikeDamage == null) return;

			short playerID = pkt.GetShort();
			byte playerPercentHealth = pkt.GetChar();
			bool playerIsDead = pkt.GetChar() != 0;
			int damageAmount = pkt.GetThree();

			OnOtherPlayerTakeSpikeDamage(playerID, playerPercentHealth, playerIsDead, damageAmount);
		}

		//timed spikes
		private void _handleEffectReport(Packet pkt)
		{
			pkt.GetChar(); //always 83 - sent from eoserv when Map::TimedSpikes is called
			//as of rev 487 this is not sent anywhere else. May need to update event handler if this changes.
			if (OnTimedSpike != null)
				OnTimedSpike();
		}

		//map hp drain
		private void _handleEffectTargetOther(Packet pkt)
		{
			if (OnTimedMapDrainHP == null)
				return;

			short damage = pkt.GetShort();
			short hp = pkt.GetShort();
			short maxhp = pkt.GetShort();

			var otherCharacters = new List<TimedMapHPDrainData>((pkt.Length - pkt.ReadPos) / 5);
			while (pkt.ReadPos != pkt.Length)
			{
				otherCharacters.Add(new TimedMapHPDrainData(
					playerID: pkt.GetShort(),
					percentHealth: pkt.GetChar(),
					damageDealt: pkt.GetShort())
				);
			}

			OnTimedMapDrainHP(damage, hp, maxhp, otherCharacters);
		}

		//potion effect (only known use based on eoserv code)
		private void _handleEffectPlayer(Packet pkt)
		{
			if (OnEffectPotion != null)
				OnEffectPotion(playerID: pkt.GetShort(),
							   effectID: pkt.GetThree());
		}
	}
}
