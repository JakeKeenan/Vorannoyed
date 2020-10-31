using System.Numerics;

namespace Vorannoyed
{
    public class VTile
    {
        public Vector2 Site { get; set; }
        public VHalfEdge[] Edges { get; set; }
        public VTile[] Neighbors { get; set; }
        public VTile(Vector2 site)
        {
            Site = site;
        }
    }
}