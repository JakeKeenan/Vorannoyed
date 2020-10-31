using System.Numerics;

namespace Vorannoyed
{
    public class VHalfEdge
    {
        public Vector2 VTileKey { get; set; }
        public int StartVertexIndex { get; set; }
        public int EndVertexIndex { get; set; }
        public int NextIndex { get; set; }
        public int PreviousIndex { get; set; }
        public int TwinIndex { get; set; }
    }
}