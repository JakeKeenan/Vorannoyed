using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    public class VArc
    {
        internal Vector2 Focus { get; set; }
        //public List<Vector3> CircleEventLocations { get; set; }
        internal List<VEvent> CircleEventLocations { get; set; }
        internal VTile Tile { get; set; }
        internal VArc(Vector2 focus, VTile tile)
        {
            Focus = focus;
            Tile = tile;
            CircleEventLocations = new List<VEvent>();
        }
    }
}