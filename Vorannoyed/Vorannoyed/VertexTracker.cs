using System;
using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    internal sealed class VertexTracker
    {
        private const float Epsilon = 0.0001f;

        private readonly List<Vector2> vertices;
        private readonly HashSet<int> outsideVertexIndexes;
        private readonly Bounds bounds;

        public VertexTracker(Vector2 boundary)
        {
            vertices = new List<Vector2>();
            outsideVertexIndexes = new HashSet<int>();
            bounds = Bounds.FromOriginAndExtent(boundary);
        }

        public List<Vector2> Vertices
        {
            get { return vertices; }
        }

        public void Add(Vector2 vertex)
        {
            int index = vertices.Count;
            vertices.Add(vertex);

            if (!bounds.Contains(vertex))
            {
                outsideVertexIndexes.Add(index);
            }
        }

        public Vector2[] GetBoundedVertices()
        {
            if (outsideVertexIndexes.Count == 0)
            {
                return vertices.ToArray();
            }

            List<Vector2> boundedVertices = new List<Vector2>(vertices.Count - outsideVertexIndexes.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                if (!outsideVertexIndexes.Contains(i))
                {
                    boundedVertices.Add(vertices[i]);
                }
            }

            return boundedVertices.ToArray();
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
