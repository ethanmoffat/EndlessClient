using System.Collections.Generic;

namespace EndlessClient.Rendering.CharacterProperties
{
    public sealed record CharacterRenderLayers(IReadOnlyList<ICharacterPropertyRenderer> Behind,
                                               IReadOnlyList<ICharacterPropertyRenderer> Main);
}
