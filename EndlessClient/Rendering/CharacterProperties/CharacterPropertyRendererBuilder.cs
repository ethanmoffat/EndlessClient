using System.Collections.Generic;
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

        public CharacterRenderLayers BuildLayers(ICharacterTextures textures,
                                                 CharacterRenderProperties renderProperties)
        {
            const float BaseLayer = 0.00001f;
            var hatMaskType = GetHatMaskType(renderProperties.HatGraphic);
            var shieldIsBehindCharacter = IsShieldBehindCharacter(renderProperties);
            var weaponIsBehindCharacter = IsWeaponBehindCharacter(renderProperties);
            var behind = new List<ICharacterPropertyRenderer>();
            var main = new List<ICharacterPropertyRenderer>();

            // Melee weapons render extra behind the character
            behind.Add(new WeaponRenderer(renderProperties, textures.WeaponExtra) { LayerDepth = BaseLayer });
            AddRenderer(
                new ShieldRenderer(renderProperties, textures.Shield, IsShieldOnBack(renderProperties.ShieldGraphic))
                {
                    LayerDepth = BaseLayer * (shieldIsBehindCharacter ? 2 : 13)
                },
                shieldIsBehindCharacter);
            AddRenderer(
                new WeaponRenderer(renderProperties, textures.Weapon)
                {
                    LayerDepth = BaseLayer * (weaponIsBehindCharacter ? 3 : 12)
                },
                weaponIsBehindCharacter);

            if (hatMaskType != HatMaskType.HideHair)
                behind.Add(new BackHairRenderer(renderProperties, textures.BackHair)
                {
                    LayerDepth = BaseLayer * 3.5f
                });

            main.Add(new SkinRenderer(renderProperties, textures.Skin) { LayerDepth = BaseLayer * 4 });
            main.Add(new FaceRenderer(renderProperties, textures.Face, textures.Skin) { LayerDepth = BaseLayer * 5 });
            main.Add(new EmoteRenderer(renderProperties, textures.Emote, textures.Skin) { LayerDepth = BaseLayer * 6 });

            main.Add(new BootsRenderer(renderProperties, textures.Boots) { LayerDepth = BaseLayer * 7 });
            main.Add(new ArmorRenderer(renderProperties, textures.Armor) { LayerDepth = BaseLayer * 8 });

            main.Add(new HatRenderer(renderProperties, textures.Hat, textures.Hair)
            {
                LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 10 : 11)
            });

            if (hatMaskType != HatMaskType.HideHair)
                main.Add(new HairRenderer(renderProperties, textures.Hair)
                {
                    LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 11 : 10)
                });

            main.Add(new WeaponSlashRenderer(renderProperties, textures.WeaponSlash) { LayerDepth = BaseLayer * 14 });

            return new CharacterRenderLayers(behind, main);

            void AddRenderer(ICharacterPropertyRenderer renderer, bool renderBehind)
            {
                if (renderBehind)
                    behind.Add(renderer);
                else
                    main.Add(renderer);
            }
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
