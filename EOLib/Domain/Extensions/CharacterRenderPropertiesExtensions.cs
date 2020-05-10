using System.Linq;
using EOLib.Domain.Character;

namespace EOLib.Domain.Extensions
{
    public static class CharacterRenderPropertiesExtensions
    {
        public static bool IsFacing(this ICharacterRenderProperties renderProperties, params EODirection[] directions)
        {
            return directions.Contains(renderProperties.Direction);
        }

        public static bool IsActing(this ICharacterRenderProperties renderProperties, params CharacterActionState[] actions)
        {
            return actions.Contains(renderProperties.CurrentAction);
        }

        public static int GetDestinationX(this ICharacterRenderProperties renderProperties)
        {
            var offset = GetXOffset(renderProperties.Direction);
            return renderProperties.MapX + offset;
        }

        public static int GetDestinationY(this ICharacterRenderProperties renderProperties)
        {
            var offset = GetYOffset(renderProperties.Direction);
            return renderProperties.MapY + offset;
        }

        private static int GetXOffset(EODirection direction)
        {
            return direction == EODirection.Right ? 1 :
                   direction == EODirection.Left ? -1 : 0;
        }

        private static int GetYOffset(EODirection direction)
        {
            return direction == EODirection.Down ? 1 :
                   direction == EODirection.Up ? -1 : 0;
        }
    }
}
