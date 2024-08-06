﻿using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.CharacterProperties
{
    [AutoMappedType]
    public class CharacterPropertyRendererBuilder : ICharacterPropertyRendererBuilder
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IMetadataProvider<HatMetadata> _hatMetadataProvider;
        private readonly IMetadataProvider<ShieldMetadata> _shieldMetadataProvider;

        public CharacterPropertyRendererBuilder(IEIFFileProvider eifFileProvider,
                                                IMetadataProvider<HatMetadata> hatMetadataProvider,
                                                IMetadataProvider<ShieldMetadata> shieldMetadataProvider)
        {
            _eifFileProvider = eifFileProvider;
            _hatMetadataProvider = hatMetadataProvider;
            _shieldMetadataProvider = shieldMetadataProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures textures,
                                                                 CharacterRenderProperties renderProperties)
        {
            const float BaseLayer = 0.00001f;

            // Melee weapons render extra behind the character
            yield return new WeaponRenderer(renderProperties, textures.WeaponExtra) { LayerDepth = BaseLayer };
            yield return new ShieldRenderer(renderProperties, textures.Shield, IsShieldOnBack(renderProperties.ShieldGraphic))
            {
                LayerDepth = BaseLayer * (IsShieldBehindCharacter(renderProperties) ? 2 : 13)
            };
            yield return new WeaponRenderer(renderProperties, textures.Weapon)
            {
                LayerDepth = BaseLayer * (IsWeaponBehindCharacter(renderProperties) ? 3 : 12)
            };

            yield return new SkinRenderer(renderProperties, textures.Skin) { LayerDepth = BaseLayer * 4 };
            yield return new FaceRenderer(renderProperties, textures.Face, textures.Skin) { LayerDepth = BaseLayer * 5 };
            yield return new EmoteRenderer(renderProperties, textures.Emote, textures.Skin) { LayerDepth = BaseLayer * 6 };

            yield return new BootsRenderer(renderProperties, textures.Boots) { LayerDepth = BaseLayer * 7 };
            yield return new ArmorRenderer(renderProperties, textures.Armor) { LayerDepth = BaseLayer * 8 };

            var hatMaskType = GetHatMaskType(renderProperties.HatGraphic);
            yield return new HatRenderer(renderProperties, textures.Hat, textures.Hair)
            {
                LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 10 : 11)
            };

            if (hatMaskType != HatMaskType.HideHair)
                yield return new HairRenderer(renderProperties, textures.Hair)
                {
                    LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 11 : 10)
                };

            yield return new WeaponSlashRenderer(renderProperties, textures.WeaponSlash) { LayerDepth = BaseLayer * 14 };
        }

        private bool IsShieldBehindCharacter(CharacterRenderProperties renderProperties)
        {
            return renderProperties.IsFacing(EODirection.Right, EODirection.Down) && IsShieldOnBack(renderProperties.ShieldGraphic);
        }

        private bool IsWeaponBehindCharacter(CharacterRenderProperties renderProperties)
        {
            var weaponInfo = EIFFile.FirstOrDefault(
               x => x.Type == ItemType.Weapon &&
                    x.DollGraphic == renderProperties.WeaponGraphic);

            var pass1 = renderProperties.RenderAttackFrame < 2;
            var pass2 = renderProperties.IsFacing(EODirection.Up, EODirection.Left);
            var pass3 = weaponInfo == null || weaponInfo.SubType == ItemSubType.Ranged;

            return pass1 || pass2 || pass3;
        }

        private HatMaskType GetHatMaskType(int hatGraphic)
        {
            if (hatGraphic == 0) return HatMaskType.Standard;
            return _hatMetadataProvider.GetValueOrDefault(hatGraphic).ClipMode;
        }

        private bool IsShieldOnBack(int shieldGraphic)
        {
            if (shieldGraphic == 0) return false;
            return _shieldMetadataProvider.GetValueOrDefault(shieldGraphic).IsShieldOnBack;
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile ?? new EIFFile();
    }
}
