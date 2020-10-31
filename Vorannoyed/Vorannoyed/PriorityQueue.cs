namespace Vorannoyed
{
    internal class PriorityQueue
    {
        public bool NotEmpty
        {
            get
            {
                return minHeap.Size != 0;
            }
        }

        private MinHeap minHeap;

        public PriorityQueue()
        {
            minHeap = new MinHeap();
        }

        public void Enqueue(VEvent value)
        {

            minHeap.add(value);
        }


        public VEvent Dequeue()
        {
            return minHeap.poll();
        }

        public void Remove(int index)
        {
            minHeap.remove(index);
        }
    }

}