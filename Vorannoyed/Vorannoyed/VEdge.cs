namespace Vorannoyed
{
    internal class VEdge
    {
        public int LeftArcIndex { get; set; }
        public int RightArcIndex { get; set; }
        public int HalfEdgeIndex { get; set; }
        public VEdge(int lArc, int rArc)
        {
            LeftArcIndex = lArc;
            RightArcIndex = rArc;
        }
    }
}