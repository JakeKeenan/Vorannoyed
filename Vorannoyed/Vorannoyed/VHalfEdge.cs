using System.Numerics;

namespace Vorannoyed
{
    public class VHalfEdge
    {
        //managed with vertex events
        public VTile Tile { get; set; }
        //manged with circle events
        //End: If it results in <0, 0> then that means it needs to be clipped
        public Vector2 End { get; set; }
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