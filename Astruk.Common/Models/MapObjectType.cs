using System.Collections.Generic;

namespace Astruk.Common.Models
{
    public class MapObjectType
    {
        public MapObjectType(int id, string name, List<KeyValuePair<string, string>> parameters)
        {
            Id = id;
            Name = name;
            Parameters = parameters;
        }

        public int Id { get; }
        public string Name { get; }
        public List<KeyValuePair<string, string>> Parameters { get; }
    }
}