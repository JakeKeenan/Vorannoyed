using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    public class VTile
    {
        public Vector2 Site { get; set; }
        
        public LinkedList<VHalfEdge> Edges { get; set; }
        public LinkedList<VTile> Neighbors { get; set; }
        public VTile(Vector2 site)
        {
            Site = site;
            Neighbors = new LinkedList<VTile>();
            Edges = new LinkedList<VHalfEdge>();
        }
    }
}