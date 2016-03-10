// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Data.BLL;

namespace EndlessClient.Rendering.CharacterProperties
{
	public static class RenderPropertiesExtensions
	{
		public static bool IsFacing(this ICharacterRenderProperties renderProperties, params EODirection[] directions)
		{
			return directions.Contains(renderProperties.Direction);
		}
	}
}
