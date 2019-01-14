using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Astruk.ViewModels
{
    public class MapObjectVM
    {
        public int Id { get; set; }
        public MapObjectTypeVM Type { get; set; }
        private Dictionary<string, string> Parameters { get; set; }
    }
}