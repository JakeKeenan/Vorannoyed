using System;

namespace Vorannoyed
{
    internal class MinHeap
    {
        private int capacity = 10;
        public int Size { get; private set; } = 0;

        private VEvent[] array;
        public MinHeap()
        {
            array = new VEvent[capacity];
        }

        private int getLeftChildIndex(int parentIndex)
        {
            return 2 * parentIndex + 1;
        }

        private int getRightChildIndex(int parentIndex)
        {
            return 2 * parentIndex + 2;
        }
        private int getParentIndex(int childIndex)
        {
            return (childIndex - 1) / 2;
        }

        private bool hasLeftChild(int index)
        {
            return getLeftChildIndex(index) < Size;
        }
        private bool hasRightChild(int index)
        {
            return getRightChildIndex(index) < Size;
        }
        private bool hasParent(int index)
        {
            if (index == 0)
            {
                return false;
            }
            return getParentIndex(index) >= 0;
        }

        private VEvent leftChild(int index)
        {
            return array[getLeftChildIndex(index)];
        }
        private VEvent rightChild(int index)
        {
            return array[getRightChildIndex(index)];
        }
        private VEvent parent(int index)
        {
            return array[getParentIndex(index)];
        }

        private void swap(int indexOne, int indexTwo)
        {
            VEvent temp = array[indexOne];
            array[indexOne] = array[indexTwo];
            array[indexTwo] = temp;
        }

        private void ensureExtraCapacity()
        {
            if (Size == capacity)
            {
                VEvent[] newArray = new VEvent[capacity * 2];
                Array.Copy(array, newArray, array.Length);
                array = newArray;
                capacity = array.Length;
            }
        }

        public VEvent peek()
        {
            if (Size == 0)
            {
                throw new IndexOutOfRangeException();
            }
            return array[0];
        }

        public VEvent poll()
        {
            if (Size == 0)
            {
                throw new IndexOutOfRangeException();
            }
            VEvent value = array[0];
            array[0] = array[Size - 1];
            Size--;
            heapifyDown();
            return value;
        }

        public void add(VEvent value)
        {
            ensureExtraCapacity();
            array[Size] = value;
            Size++;
            heapifyUp();
        }

        public void remove(int index)
        {
            if (index > Size || index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            swap(index, Size - 1);
            int parentIndex = getParentIndex(index);
            if (array[parentIndex].EventLocation.Y < array[index].EventLocation.Y)
            {
                heapifyUp();
            }
            else
            {
                heapifyDown();
            }
        }

        public void heapifyUp()
        {
            int index = Size - 1;
            //while (hasParent(index) && parent(index) > array[index])
            bool thasParent = hasParent(index);
            VEvent tparent = parent(index);
            while (hasParent(index) && parent(index).EventLocation.Y <= array[index].EventLocation.Y)
            {
                if (parent(index).EventLocation.Y == array[index].EventLocation.Y && parent(index).EventLocation.X < array[index].EventLocation.X)
                {
                    break;
                }
                else
                {
                    swap(getParentIndex(index), index);
                    index = getParentIndex(index);
                }
            }
        }

        public void heapifyDown()
        {
            int index = 0;
            while (hasLeftChild(index))
            {
                int smallerChildIndex = getLeftChildIndex(index);
                //if (hasRightChild(index) && rightChild(index) < leftChild(index))
                if (hasRightChild(index) && rightChild(index).EventLocation.Y >= leftChild(index).EventLocation.Y)
                {
                    if (rightChild(index).EventLocation.Y == leftChild(index).EventLocation.Y)
                    {
                        if (rightChild(index).EventLocation.X < leftChild(index).EventLocation.X)
                        {
                            smallerChildIndex = getRightChildIndex(index);
                        }
                    }
                    else
                    {
                        smallerChildIndex = getRightChildIndex(index);
                    }
                }
                //if (array[index] < array[smallerChildIndex])
                if (array[index].EventLocation.Y >= array[smallerChildIndex].EventLocation.Y)
                {
                    if (array[index].EventLocation.Y == array[smallerChildIndex].EventLocation.Y)
                    {
                        if (array[index].EventLocation.X < array[smallerChildIndex].EventLocation.X)
                        {
                            break;
                        }
                        else
                        {
                            swap(index, smallerChildIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    swap(index, smallerChildIndex);
                }
                index = smallerChildIndex;
            }
        }
    }
}