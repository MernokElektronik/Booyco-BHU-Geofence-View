using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Implements a List structure as a cyclical list where indices are wrapped.
    /// </summary>
    /// <typeparam name="T">The Type to hold in the list.</typeparam>
    class CyclicalList<T> : List<T>
    {
        public new T this[int index]
        {
            get
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;
                return base[index];
            }
            set
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;
                base[index] = value;
            }
        }

        public CyclicalList() { }

        public CyclicalList(IEnumerable<T> collection) : base(collection){ }

        public new void RemoveAt(int index)
        {
            Remove(this[index]);
        }
    }

    class IndexableCyclicalLinkedList<T> : LinkedList<T>
    {
        /// <summary>
        /// Gets the LinkedListNode at a particular index.
        /// </summary>
        /// <param name="index">The index of the node to retrieve.</param>
        /// <returns>The LinkedListNode found at the index given.</returns>
        public LinkedListNode<T> this[int index]
        {
            get
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;

                //find the proper node
                LinkedListNode<T> node = First;
                for (int i = 0; i < index; i++)
                    node = node.Next;

                return node;
            }
        }
        /// <summary>
        /// Removes the node at a given index.
        /// </summary>
        /// <param name="index">The index of the node to remove.</param>
        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }
        /// <summary>
        /// Finds the index of a given item.
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>The index of the item if found; -1 if the item is not found.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Value.Equals(item))
                    return i;

            return -1;
        }
    }
}
