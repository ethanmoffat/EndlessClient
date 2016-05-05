// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Domain.Login
{
	public interface INewsRepository
	{
		List<string> NewsText { get; set; }
	}

	public interface INewsProvider
	{
		IReadOnlyList<string> NewsText { get; }
	}

	public class NewsRepository : INewsRepository, INewsProvider
	{
		public List<string> NewsText { get; set; }

		IReadOnlyList<string> INewsProvider.NewsText { get { return NewsText; } }
	}
}
