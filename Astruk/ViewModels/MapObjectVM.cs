using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Astruk.ViewModels
{
    public class MapObjectVM
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string[] Parameters { get; set; }
    }
}