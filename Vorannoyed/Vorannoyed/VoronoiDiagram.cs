using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    public class VoronoiDiagram
    {
        public VTile[] Tiles { get; internal set; }
        public Vector2[] Verticies { get; internal set; }
        public List<VHalfEdge> HalfEdges { get; internal set; } 
    }
}