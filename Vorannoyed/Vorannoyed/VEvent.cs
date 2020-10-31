using System.Numerics;

namespace Vorannoyed
{
    internal enum EventType
    {
        VertexEvent,
        CircleEvent
    }
    internal class VEvent
    {
        public EventType EventType;
        public Vector2 EventLocation;

        public VEvent(Vector2 eventLocation, EventType eventType)
        {
            EventType = eventType;
            EventLocation = eventLocation;
        }
    }
}