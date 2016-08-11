// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRendererRepository
    {
        ICharacterRenderer ActiveCharacterRenderer { get; set; }

        List<ICharacterRenderer> CharacterRenderers { get; set; }
    }

    public interface ICharacterRendererProvider
    {
        ICharacterRenderer ActiveCharacterRenderer { get; }

        IReadOnlyList<ICharacterRenderer> CharacterRenderers { get; }
    }

    public class CharacterRendererRepository : ICharacterRendererRepository, ICharacterRendererProvider, ICharacterRendererResetter
    {
        public ICharacterRenderer ActiveCharacterRenderer { get; set; }

        public List<ICharacterRenderer> CharacterRenderers { get; set; }

        IReadOnlyList<ICharacterRenderer> ICharacterRendererProvider.CharacterRenderers
        {
            get { return CharacterRenderers; }
        }

        public CharacterRendererRepository()
        {
            CharacterRenderers = new List<ICharacterRenderer>(64);
        }

        public void ResetRenderers()
        {
            if (ActiveCharacterRenderer != null)
                ActiveCharacterRenderer.Dispose();
            ActiveCharacterRenderer = null;

            foreach (var renderer in CharacterRenderers)
                renderer.Dispose();
            CharacterRenderers.Clear();
        }

        public void Dispose()
        {
            ResetRenderers();
        }
    }
}