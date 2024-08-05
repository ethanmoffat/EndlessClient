using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    [AutoMappedType(IsSingleton = true)]
    public class ExperienceTableProvider : IExperienceTableProvider
    {
        public IReadOnlyList<int> ExperienceByLevel { get; }

        public ExperienceTableProvider()
        {
            var exp_table = new List<int> { 0 };
            for (int i = 1; i < byte.MaxValue - 1; ++i)
                exp_table.Add((int)Math.Round(Math.Pow(i, 3) * 133.1));

            ExperienceByLevel = exp_table;
        }
    }

    public interface IExperienceTableProvider
    {
        IReadOnlyList<int> ExperienceByLevel { get; }
    }
}
