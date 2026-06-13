using System.Numerics;

namespace Vorannoyed
{
    public class VHalfEdge
    {
        private Vector2 end;

        //managed with vertex events
        public VTile Tile { get; set; }
        //manged with circle events
        public Vector2 End
        {
            get { return end; }
            set
            {
                end = value;
                HasEnd = true;
            }
        }

        public bool HasEnd { get; private set; }

        public VHalfEdge Next { get; set; }//going counter clockwise
        public VHalfEdge Prev { get; set; }
        //managed with vertex events
        public VHalfEdge Twin { get; set; }
    }
}

//public int StartVertexIndex { get; set; }
//public int EndVertexIndex { get; set; }
//public int NextIndex { get; set; }
//public int PreviousIndex { get; set; }
//public int TwinIndex { get; set; }
