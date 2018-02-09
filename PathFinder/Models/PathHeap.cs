using System;
using System.Collections.Generic;
using System.Linq;

namespace PathFinder.Models
{
    internal class PathHeap
    {
        private IDictionary<int, Node> _heap = new Dictionary<int, Node>();

        private void Swap(int i, int j)
        {
            // swap indexes first
            _heap[i].Index = j;
            _heap[j].Index = i;

            var temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;
        }

        private int GetParentIndex(int index)
        {
            return (index - 1) / 2;
        }

        private int GetLeftChildIndex(int index)
        {
            return 2 * index + 1;
        }

        private int GetRightChildIndex(int index)
        {
            return 2 * index + 2;
        }

        private bool IsLeaf(int index)
        {
            return (GetLeftChildIndex(index) >= _heap.Count) && (GetRightChildIndex(index) >= _heap.Count);
        }

        private bool IsOneChild(int index)
        {
            return (GetLeftChildIndex(index) < _heap.Count) && (GetRightChildIndex(index) >= _heap.Count);
        }

        private void BuildHeap(IList<Node> nodes)
        {
            _heap.Clear();

            // add all nodes to the heap
            for (var i = 0; i < nodes.Count; i++)
            {
                AddToHeap(nodes[i]);
            }
        }

        public void AddToHeap(Node node)
        {
            var index = _heap.Count;
            node.Index = index;

            _heap.Add(index, node);

            UpHeapify(index);
        }

        public Node RemoveMin()
        {
            // take minimum
            var minimum = _heap[0];
            minimum.Index = -1;

            // take last node
            var tempNode = _heap[_heap.Count - 1];
            // remove the last node (we're going to put it back in the heap)
            _heap.Remove(_heap.Count - 1);
            // if the heap is empty, this was the only node
            if (_heap.Any())
            {
                // set the last node to the top of the heap
                tempNode.Index = 0;
                _heap[0] = tempNode;
                // reset the order starting from the top
                DownHeapify(0);
            }

            return minimum;
        }

        public void UpHeapify(int index)
        {
            // this is the root node, we're done
            if (index == 0)
                return;

            var parentIndex = GetParentIndex(index);
            // nothing to do, we are satisfied
            if (_heap[index].Value >= _heap[parentIndex].Value)
                return;

            // swap parrent and the child
            Swap(index, parentIndex);
            
            // up heapify starting from the parent
            UpHeapify(parentIndex);
        }

        // Call this routine if the heap rooted at i satisfies the heap property
        // *except* perhaps i to its immediate children
        public void DownHeapify(int i)
        {
            //// If i is a leaf, heap property holds
            if (IsLeaf(i))
                return;

            var leftIndex = GetLeftChildIndex(i);
            var rightIndex = GetRightChildIndex(i);

            // If i has one child...
            if (IsOneChild(i))
            {
                // check heap property
                if (_heap[i].Value > _heap[leftIndex].Value)
                // If it fails, swap, fixing i and its child (a leaf)
                {
                    Swap(i, leftIndex);
                }
                return;
            }

            // If i has two children...
            // check heap property
            if (Math.Min(_heap[leftIndex].Value, _heap[rightIndex].Value) >= _heap[i].Value)
                return;

            // If it fails, see which child is the smaller
            // and swap i's value into that child
            // Afterwards, recurse into that child, which might violate
            if (_heap[leftIndex].Value < _heap[rightIndex].Value)
            {
                // Swap into left child
                Swap(i, leftIndex);
                DownHeapify(leftIndex);
                return;
            }
            else
            {
                // Swap into right child
                Swap(i, rightIndex);
                DownHeapify(rightIndex);
                return;
            }
        }

        public bool Any()
        {
            return _heap.Any();
        }
    }
}
