using System;
using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    internal sealed class HalfEdgeTracker
    {
        private readonly Dictionary<VHalfEdge, VHalfEdge> canonicalHalfEdges;
        private readonly HashSet<VHalfEdge> unfinishedHalfEdges;

        public HalfEdgeTracker()
        {
            canonicalHalfEdges = new Dictionary<VHalfEdge, VHalfEdge>();
            unfinishedHalfEdges = new HashSet<VHalfEdge>();
        }

        public IReadOnlyCollection<VHalfEdge> UnfinishedHalfEdges
        {
            get { return unfinishedHalfEdges; }
        }

        public void AddPair(VHalfEdge halfEdge, VHalfEdge twinHalfEdge)
        {
            if (halfEdge == null)
            {
                throw new ArgumentNullException(nameof(halfEdge));
            }

            if (twinHalfEdge == null)
            {
                throw new ArgumentNullException(nameof(twinHalfEdge));
            }

            halfEdge.Twin = twinHalfEdge;
            twinHalfEdge.Twin = halfEdge;
            canonicalHalfEdges[halfEdge] = halfEdge;
            canonicalHalfEdges[twinHalfEdge] = halfEdge;
            UpdatePair(halfEdge);
        }

        public void SetEnd(VHalfEdge halfEdge, Vector2 end)
        {
            if (halfEdge == null)
            {
                throw new ArgumentNullException(nameof(halfEdge));
            }

            halfEdge.End = end;
            UpdatePair(halfEdge);
        }

        private void UpdatePair(VHalfEdge halfEdge)
        {
            if (!canonicalHalfEdges.TryGetValue(halfEdge, out VHalfEdge canonicalHalfEdge))
            {
                return;
            }

            if (canonicalHalfEdge.Twin == null)
            {
                unfinishedHalfEdges.Add(canonicalHalfEdge);
                return;
            }

            if (canonicalHalfEdge.HasEnd && canonicalHalfEdge.Twin.HasEnd)
            {
                unfinishedHalfEdges.Remove(canonicalHalfEdge);
                return;
            }

            unfinishedHalfEdges.Add(canonicalHalfEdge);
        }
    }
}
