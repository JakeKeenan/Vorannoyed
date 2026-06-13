using System;
using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    internal sealed class HalfEdgeTracker
    {
        private const float Epsilon = 0.0001f;

        private readonly Dictionary<VHalfEdge, VHalfEdge> canonicalHalfEdges;
        private readonly HashSet<VHalfEdge> clipHalfEdges;
        private readonly Bounds bounds;

        public HalfEdgeTracker(Vector2 boundary)
        {
            canonicalHalfEdges = new Dictionary<VHalfEdge, VHalfEdge>();
            clipHalfEdges = new HashSet<VHalfEdge>();
            bounds = Bounds.FromOriginAndExtent(boundary);
        }

        public IReadOnlyCollection<VHalfEdge> ClipHalfEdges
        {
            get { return clipHalfEdges; }
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
                clipHalfEdges.Add(canonicalHalfEdge);
                return;
            }

            if (canonicalHalfEdge.HasEnd && canonicalHalfEdge.Twin.HasEnd)
            {
                if (bounds.Contains(canonicalHalfEdge.End) && bounds.Contains(canonicalHalfEdge.Twin.End))
                {
                    clipHalfEdges.Remove(canonicalHalfEdge);
                    return;
                }

                clipHalfEdges.Add(canonicalHalfEdge);
                return;
            }

            clipHalfEdges.Add(canonicalHalfEdge);
        }

        private readonly struct Bounds
        {
            public Vector2 Min { get; }
            public Vector2 Max { get; }

            public Bounds(Vector2 min, Vector2 max)
            {
                Min = min;
                Max = max;
            }

            public bool Contains(Vector2 point)
            {
                return point.X >= Min.X - Epsilon &&
                    point.X <= Max.X + Epsilon &&
                    point.Y >= Min.Y - Epsilon &&
                    point.Y <= Max.Y + Epsilon;
            }

            public static Bounds FromOriginAndExtent(Vector2 extent)
            {
                return new Bounds(
                    new Vector2(Math.Min(0f, extent.X), Math.Min(0f, extent.Y)),
                    new Vector2(Math.Max(0f, extent.X), Math.Max(0f, extent.Y)));
            }
        }
    }
}
