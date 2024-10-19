﻿namespace EndlessClient.Audio
{
    // These are 0 based indexes even though the files start at sfx001
    // sfx001 will be id 0
    // sfx060 will be id 59
    public enum SoundEffectID
    {
        NONE,
        LayeredTechIntro = 1,
        ButtonClick,
        DialogButtonClick,
        TextBoxFocus = 4,
        ChestOpen = TextBoxFocus,
        SpellActivate = TextBoxFocus,
        ServerCommand = TextBoxFocus,
        TradeItemOfferChanged = TextBoxFocus,
        Login,
        ServerMessage = Login,
        DeleteCharacter,
        MapMutation = DeleteCharacter,
        Banned,
        Reboot = Banned,
        ScreenCapture = 8,
        PrivateMessageReceived,
        PunchAttack,
        UnknownWarpSound,
        PrivateMessageTargetNotFound = 12,
        HudStatusBarClick,
        AdminAnnounceReceived,
        MeleeWeaponAttack,
        MemberLeftParty = 16,
        TradeAccepted,
        JoinParty = TradeAccepted,
        GroupChatReceived,
        PrivateMessageSent,
        InventoryPickup = 20,
        InventoryPlace,
        Earthquake,
        DoorClose,
        DoorOpen = 24,
        DoorOrChestLocked,
        BuySell,
        Craft,
        PlayerFrozen = 28,
        AdminChatReceived,
        AdminChatSent = AdminChatReceived,
        AlternateMeleeAttack,
        PotionOfFlamesEffect,
        AdminWarp = 32,
        NoWallWalk,
        GhostPlayer = NoWallWalk,
        ScrollTeleport = NoWallWalk,
        PotionOfEvilTerrorEffect,
        PotionOfFireworksEffect,
        PotionOfSparklesEffect = 36,
        LearnNewSpell,
        PotionOfLoveEffect = LearnNewSpell,
        InnSignUp = LearnNewSpell,
        AttackBow,
        LevelUp,
        Dead = 40,
        JumpStone,
        Water,
        Heal,
        Harp1 = 44,
        Harp2,
        Harp3,
        Guitar1,
        Guitar2 = 48,
        Guitar3,
        Thunder,
        MapEvacTimer,
        ArenaTickSound = MapEvacTimer,
        ArenaWin = 52,
        Gun,
        UltimaBlastSpell,
        ShieldSpell,
        RingOfFireSpell = 56,
        IceBlastSpell1,
        EnergyBallSpell,
        WhirlSpell,
        BouldersSpell = 60,
        AuraSpell,
        HeavenSpell,
        IceBlastSpell2,
        MapAmbientNoiseWater = 64,
        MapAmbientNoiseDrone1,
        AdminHide,
        MapAmbientNoiseLavaBubbles1,
        AdminRequestSent = 68,
        MapAmbientNoiseFactory,
        MapEffectHPDrain,
        MapEffectTPDrain,
        Spikes = 72,
        NoArrows,
        EnterPkMap,
        UnknownMapAmbientNoise5,
        DarkHandSpell = 76,
        TentaclesSpell,
        MagicWhirlSpell,
        PowerWindSpell,
        FireBlastSpell = 80,
        MapAmbientNoiseLavaBubbles2,
    }
}
