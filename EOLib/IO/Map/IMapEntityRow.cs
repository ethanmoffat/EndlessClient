// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.IO.Map
{
	public interface IMapEntityRow<T>
	{
		int Y { get; set; }
		List<IMapEntityItem<T>> EntityItems { get; set; }
	}
}
