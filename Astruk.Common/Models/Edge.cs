﻿using System.Collections.Generic;

namespace Astruk.Common.Models
{
    public class Edge
    {
        public DeluanVertex start;
        public DeluanVertex end;
        public Triangle baseTriangle;

        public Edge(DeluanVertex start, DeluanVertex end)
        {
            this.start = start;
            this.end = end;
        }

        public override string ToString()
        {
            return $"({start},{end})";
        }
    }
}
