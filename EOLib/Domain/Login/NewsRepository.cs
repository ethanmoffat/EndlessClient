using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    public interface INewsRepository
    {
        string NewsHeader { get; set; }

        List<string> NewsText { get; set; }
    }

    public interface INewsProvider
    {
        string NewsHeader { get; }

        IReadOnlyList<string> NewsText { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class NewsRepository : INewsRepository, INewsProvider
    {
        public string NewsHeader { get; set; }

        public List<string> NewsText { get; set; }

        IReadOnlyList<string> INewsProvider.NewsText => NewsText;
    }
}