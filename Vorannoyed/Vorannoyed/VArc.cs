using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    public class VArc
    {
        private static int IDCounter = 0;
        internal int ID { get; set; }
        internal Vector2 Focus { get; set; }
        //public List<Vector3> CircleEventLocations { get; set; }
        internal List<VEvent> CircleEventLocations { get; set; }
        internal VArc(Vector2 focus)
        {
            Focus = focus;
            CircleEventLocations = new List<VEvent>();
            ID = IDCounter;
            IDCounter++;
        }
    }
}