using System.Numerics;

namespace Vorannoyed
{
    public class VoronoiDiagram
    {
        public VTile[] Tiles { get; internal set; }
        public Vector2[] Seeds { get; internal set; }
        public Vector2[] Verticies { get; internal set; }
    }
}