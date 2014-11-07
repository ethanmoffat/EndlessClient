using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient
{
	public class EOInventory : XNAControl
	{
		//This is going to be the inventory manager - need to figure out dragging/dropping
		//Start with just showing the inventory items in the grid.
		//add the drop/junk/paperdoll buttons
		//add label for weight
		
		//similar to EOChatRenderer
		//need to add in mouse handling, clicking, etc.

		public EOInventory(Game g)
			: base(g)
		{
			
		}
	}
}
