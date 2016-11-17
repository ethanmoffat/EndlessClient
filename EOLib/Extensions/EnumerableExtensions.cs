// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Extensions
{
    public static class EnumerableExtensions
    {
        public static Optional<T> OptionalSingle<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
            where T : class
        {
            var single = enumerable.SingleOrDefault(predicate);
            if (single == null)
                return Optional<T>.Empty;

            return new Optional<T>(single);
        }
    }
}
