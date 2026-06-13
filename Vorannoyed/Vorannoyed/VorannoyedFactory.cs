using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Vorannoyed
{
    public class VorannoyedFactory
    {
        private const float Epsilon = 0.0001f;

        private static VTile[] tiles;
        private static int currentTileIndex;
        private static List<Vector2> vertices;
        private static List<VHalfEdge> halfEdges;
        private static HalfEdgeTracker halfEdgeTracker;

        private static PriorityQueue priorityQueue;
        private static Dictionary<VEvent, VEventInfo> events;

        private static BeachLine beachLine;

        public static DelaunayTriangulation MakeDelaunay(List<Vector2> seeds)
        {
            throw new NotImplementedException();
        }

        public static VoronoiDiagram MakeVoronoiFromDelaunay()
        {
            throw new NotImplementedException();
        }

        public static VoronoiDiagram MakeVoronoiBZ()
        {
            throw new NotImplementedException();
        }

        public static VoronoiDiagram MakeVoronoiSF(List<Vector2> seeds, Vector2 boundry)
        {
            tiles = new VTile[seeds.Count];
            currentTileIndex = 0;
            vertices = new List<Vector2>();
            halfEdges = new List<VHalfEdge>();
            halfEdgeTracker = new HalfEdgeTracker();

            priorityQueue = new PriorityQueue();
            events = new Dictionary<VEvent, VEventInfo>();

            foreach (Vector2 seed in seeds)
            {
                VEvent newEvent = new VEvent(seed, EventType.VertexEvent);
                priorityQueue.Enqueue(newEvent);
                events.Add(newEvent, null);
            }

            beachLine = new BeachLine();

            while (priorityQueue.NotEmpty)
            {
                VEvent nextEvent = priorityQueue.Dequeue();
                VEventInfo evtInfo = events[nextEvent];
                if (evtInfo == null || evtInfo.Deleted != true)
                {
                    handleEvent(nextEvent, nextEvent.EventType, halfEdges);
                }
            }

            ClipUnfinishedHalfEdges(boundry);

            VoronoiDiagram retVal = new VoronoiDiagram()
            {
                Verticies = vertices.ToArray(),
                Tiles = tiles,
                HalfEdges = halfEdges
            };

            tiles = null;
            vertices = null;
            halfEdges = null;
            priorityQueue = null;
            events = null;
            beachLine = null;
            halfEdgeTracker = null;

            return retVal;
        }

        private static void handleEvent(VEvent vEvent, EventType et, List<VHalfEdge> halfEdges)
        {
            if (et == EventType.VertexEvent)
            {
                beachLine.HandleVertexEvent(vEvent.EventLocation, halfEdges, halfEdgeTracker, ref events, ref tiles, currentTileIndex, ref priorityQueue);
                currentTileIndex++;
            }
            else if (et == EventType.CircleEvent)
            {
                beachLine.HandleCircleEvent(vEvent, halfEdges, halfEdgeTracker, ref events, ref vertices, ref priorityQueue);
            }
        }

        private static void ClipUnfinishedHalfEdges(Vector2 boundry)
        {
            Bounds bounds = Bounds.FromOriginAndExtent(boundry);
            List<VHalfEdge> unfinishedHalfEdges = new List<VHalfEdge>(halfEdgeTracker.UnfinishedHalfEdges);

            foreach (VHalfEdge halfEdge in unfinishedHalfEdges)
            {
                if (halfEdge == null || halfEdge.Twin == null)
                {
                    continue;
                }

                if (halfEdge.HasEnd && halfEdge.Twin.HasEnd)
                {
                    continue;
                }

                if (halfEdge.HasEnd || halfEdge.Twin.HasEnd)
                {
                    ClipUnfinishedRay(halfEdge, halfEdge.Twin, bounds);
                }
                else
                {
                    ClipUnfinishedLine(halfEdge, halfEdge.Twin, bounds);
                }
            }
        }

        private static void ClipUnfinishedRay(VHalfEdge halfEdge, VHalfEdge twinHalfEdge, Bounds bounds)
        {
            VHalfEdge finishedHalfEdge = halfEdge.HasEnd ? halfEdge : twinHalfEdge;
            VHalfEdge unfinishedHalfEdge = halfEdge.HasEnd ? twinHalfEdge : halfEdge;
            Vector2 direction = GetHalfEdgeDirection(unfinishedHalfEdge);

            if (!TryClipRayToBox(finishedHalfEdge.End, direction, bounds, out Vector2 clippedStart, out Vector2 clippedEnd))
            {
                return;
            }

            halfEdgeTracker.SetEnd(finishedHalfEdge, clippedStart);
            halfEdgeTracker.SetEnd(unfinishedHalfEdge, clippedEnd);
        }

        private static void ClipUnfinishedLine(VHalfEdge halfEdge, VHalfEdge twinHalfEdge, Bounds bounds)
        {
            if (halfEdge.Tile == null || twinHalfEdge.Tile == null)
            {
                return;
            }

            Vector2 origin = (halfEdge.Tile.Site + twinHalfEdge.Tile.Site) * 0.5f;
            Vector2 direction = GetHalfEdgeDirection(halfEdge);

            if (!TryClipLineToBox(origin, direction, bounds, out Vector2 clippedStart, out Vector2 clippedEnd))
            {
                return;
            }

            halfEdgeTracker.SetEnd(halfEdge, clippedStart);
            halfEdgeTracker.SetEnd(twinHalfEdge, clippedEnd);
        }

        private static Vector2 GetHalfEdgeDirection(VHalfEdge halfEdge)
        {
            if (halfEdge == null || halfEdge.Tile == null || halfEdge.Twin == null || halfEdge.Twin.Tile == null)
            {
                return Vector2.Zero;
            }

            Vector2 tileSite = halfEdge.Tile.Site;
            Vector2 twinSite = halfEdge.Twin.Tile.Site;
            Vector2 direction = new Vector2(tileSite.Y - twinSite.Y, twinSite.X - tileSite.X);

            if (direction.LengthSquared() <= Epsilon * Epsilon)
            {
                return Vector2.Zero;
            }

            direction /= (float)Math.Sqrt(direction.LengthSquared());
            return direction;
        }

        private static bool TryClipRayToBox(Vector2 origin, Vector2 direction, Bounds bounds, out Vector2 clippedStart, out Vector2 clippedEnd)
        {
            clippedStart = Vector2.Zero;
            clippedEnd = Vector2.Zero;

            if (direction.LengthSquared() <= Epsilon * Epsilon)
            {
                return false;
            }

            float tEnter = 0f;
            float tExit = float.PositiveInfinity;

            if (!ClipAxis(origin.X, direction.X, bounds.Min.X, bounds.Max.X, ref tEnter, ref tExit) ||
                !ClipAxis(origin.Y, direction.Y, bounds.Min.Y, bounds.Max.Y, ref tEnter, ref tExit) ||
                tExit < tEnter ||
                IsNonFinite(tEnter) ||
                IsNonFinite(tExit))
            {
                return false;
            }

            clippedStart = origin + direction * Math.Max(tEnter, 0f);
            clippedEnd = origin + direction * tExit;
            return IsFinite(clippedStart) && IsFinite(clippedEnd);
        }

        private static bool TryClipLineToBox(Vector2 origin, Vector2 direction, Bounds bounds, out Vector2 clippedStart, out Vector2 clippedEnd)
        {
            clippedStart = Vector2.Zero;
            clippedEnd = Vector2.Zero;

            if (direction.LengthSquared() <= Epsilon * Epsilon)
            {
                return false;
            }

            List<Vector2> intersections = new List<Vector2>();

            if (Math.Abs(direction.X) > Epsilon)
            {
                AddLineIntersection(origin, direction, (bounds.Min.X - origin.X) / direction.X, bounds, intersections);
                AddLineIntersection(origin, direction, (bounds.Max.X - origin.X) / direction.X, bounds, intersections);
            }

            if (Math.Abs(direction.Y) > Epsilon)
            {
                AddLineIntersection(origin, direction, (bounds.Min.Y - origin.Y) / direction.Y, bounds, intersections);
                AddLineIntersection(origin, direction, (bounds.Max.Y - origin.Y) / direction.Y, bounds, intersections);
            }

            if (intersections.Count < 2)
            {
                return false;
            }

            clippedStart = intersections[0];
            clippedEnd = intersections[1];
            return Vector2.DistanceSquared(clippedStart, clippedEnd) > Epsilon * Epsilon;
        }

        private static void AddLineIntersection(
            Vector2 origin,
            Vector2 direction,
            float t,
            Bounds bounds,
            List<Vector2> intersections)
        {
            if (IsNonFinite(t))
            {
                return;
            }

            Vector2 point = origin + direction * t;
            if (!IsFinite(point) || !bounds.Contains(point))
            {
                return;
            }

            foreach (Vector2 intersection in intersections)
            {
                if (Vector2.DistanceSquared(intersection, point) <= Epsilon * Epsilon)
                {
                    return;
                }
            }

            intersections.Add(point);
        }

        private static bool ClipAxis(float origin, float direction, float min, float max, ref float tEnter, ref float tExit)
        {
            if (Math.Abs(direction) < Epsilon)
            {
                return origin >= min - Epsilon && origin <= max + Epsilon;
            }

            float t1 = (min - origin) / direction;
            float t2 = (max - origin) / direction;
            float axisEnter = Math.Min(t1, t2);
            float axisExit = Math.Max(t1, t2);

            tEnter = Math.Max(tEnter, axisEnter);
            tExit = Math.Min(tExit, axisExit);
            return tExit >= tEnter;
        }

        private static bool IsFinite(Vector2 point)
        {
            return !IsNonFinite(point.X) && !IsNonFinite(point.Y);
        }

        private static bool IsNonFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
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
