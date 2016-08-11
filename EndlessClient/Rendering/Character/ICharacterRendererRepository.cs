// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRendererRepository
    {
        ICharacterRenderer MainCharacterRenderer { get; set; }

        Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }
    }

    public interface ICharacterRendererProvider
    {
        ICharacterRenderer MainCharacterRenderer { get; }

        IReadOnlyDictionary<int, ICharacterRenderer> CharacterRenderers { get; }
    }

    public class CharacterRendererRepository : ICharacterRendererRepository, ICharacterRendererProvider, ICharacterRendererResetter
    {
        public ICharacterRenderer MainCharacterRenderer { get; set; }

        public Dictionary<int, ICharacterRenderer> CharacterRenderers { get; set; }

        IReadOnlyDictionary<int, ICharacterRenderer> ICharacterRendererProvider.CharacterRenderers
        {
            get { return CharacterRenderers; }
        }

        public CharacterRendererRepository()
        {
            CharacterRenderers = new Dictionary<int, ICharacterRenderer>(64);
        }

        public void ResetRenderers()
        {
            if (MainCharacterRenderer != null)
                MainCharacterRenderer.Dispose();
            MainCharacterRenderer = null;

            foreach (var renderer in CharacterRenderers.Values)
                renderer.Dispose();
            CharacterRenderers.Clear();
        }

        public void Dispose()
        {
            ResetRenderers();
        }
    }
}