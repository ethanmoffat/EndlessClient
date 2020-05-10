using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.CharacterProperties
{
    [MappedType(BaseType = typeof(ICharacterPropertyRendererBuilder))]
    public class CharacterPropertyRendererBuilder : ICharacterPropertyRendererBuilder
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IHatConfigurationProvider _hatConfigurationProvider;
        private readonly IShaderProvider _shaderProvider;

        public CharacterPropertyRendererBuilder(IEIFFileProvider eifFileProvider,
                                                IHatConfigurationProvider hatConfigurationProvider,
                                                IShaderProvider shaderProvider)
        {
            _eifFileProvider = eifFileProvider;
            _hatConfigurationProvider = hatConfigurationProvider;
            _shaderProvider = shaderProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures textures,
                                                                 ICharacterRenderProperties renderProperties)
        {
            const float BaseLayer = 0.00001f;

            // Melee weapons render extra behind the character
            yield return new WeaponRenderer(renderProperties, textures.WeaponExtra) { LayerDepth = BaseLayer };
            yield return new ShieldRenderer(renderProperties, textures.Shield, EIFFile.IsShieldOnBack(renderProperties.ShieldGraphic))
            {
                LayerDepth = BaseLayer * (IsShieldBehindCharacter(renderProperties) ? 2 : 9)
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

            var hatMaskType = GetHatMaskType(renderProperties);
            yield return new HatRenderer(_shaderProvider, renderProperties, textures.Hat, textures.Hair)
            {
                LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 10 : 11)
            };

            if (hatMaskType != HatMaskType.HideHair)
                yield return new HairRenderer(renderProperties, textures.Hair)
                {
                    LayerDepth = BaseLayer * (hatMaskType == HatMaskType.FaceMask ? 11 : 10)
                };
        }

        private bool IsShieldBehindCharacter(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.IsFacing(EODirection.Right, EODirection.Down) && EIFFile.IsShieldOnBack(renderProperties.ShieldGraphic);
        }

        private bool IsWeaponBehindCharacter(ICharacterRenderProperties renderProperties)
        {
             var weaponInfo = EIFFile.Data.FirstOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == renderProperties.WeaponGraphic);

            var pass1 = renderProperties.AttackFrame < 2;
            var pass2 = renderProperties.IsFacing(EODirection.Up, EODirection.Left);
            var pass3 = weaponInfo == null || weaponInfo.SubType == ItemSubType.Ranged;

            return pass1 || pass2 || pass3;
        }

        private HatMaskType GetHatMaskType(ICharacterRenderProperties renderProperties)
        {
            var hatInfo = EIFFile.Data.FirstOrDefault(
                x => x.Type == ItemType.Hat &&
                     x.DollGraphic == renderProperties.HatGraphic);

            _hatConfigurationProvider.HatMasks.TryGetValue(hatInfo?.ID ?? 0, out var hatMaskType);
            return hatMaskType;
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile ?? new EIFFile();
    }
}
