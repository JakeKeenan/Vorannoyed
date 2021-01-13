using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Vorannoyed
{
    public class VorannoyedFactory
    { 
        private static VTile[] tiles;
        private static int currentTileIndex;
        private static List<Vector2> vertices;
        private static List<VHalfEdge> halfEdges;

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

            return retVal;
        }

        private static void handleEvent(VEvent vEvent, EventType et, List<VHalfEdge> halfEdges)
        {
            if (et == EventType.VertexEvent)
            {
                beachLine.HandleVertexEvent(vEvent.EventLocation, halfEdges, ref events, ref tiles, currentTileIndex, ref priorityQueue);
                currentTileIndex++;
            }
            else if (et == EventType.CircleEvent)
            {
                beachLine.HandleCircleEvent(vEvent, halfEdges, ref events, ref vertices, ref priorityQueue);
            }
        }
    }
}
