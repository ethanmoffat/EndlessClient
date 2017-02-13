// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Domain.Character
{
    public class ExperienceTableProvider : IExperienceTableProvider
    {
        public IReadOnlyList<int> ExperienceByLevel { get; }

        public ExperienceTableProvider()
        {
            var exp_table = new List<int>();
            for (int i = 1; i < byte.MaxValue - 1; ++i)
                exp_table.Add((int) Math.Round(Math.Pow(i, 3)*133.1));

            ExperienceByLevel = exp_table;
        }
    }

    public interface IExperienceTableProvider
    {
        IReadOnlyList<int> ExperienceByLevel { get; }
    }
}
