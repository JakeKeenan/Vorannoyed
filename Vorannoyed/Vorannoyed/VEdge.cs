namespace Vorannoyed
{
    internal class VEdge
    {
        public int LeftArcIndex { get; set; }
        public int RightArcIndex { get; set; }
        //public int HalfEdgeIndex { get; set; }
        public VHalfEdge HalfEdge { get; set; }
        public VEdge(int lArc, int rArc, VHalfEdge halfEdge)
        {
            HalfEdge = halfEdge;
            LeftArcIndex = lArc;
            RightArcIndex = rArc;
        }
    }
}