using System;
using System.Collections;

namespace GenericListManager.SuperList
{
    public class SuperList<T> : IList<T>
    {
        public event EventHandler<ItemChangedEventArgs<T>> ItemChanged;
        private Node<T> Head { get; set; }
        private Node<T> Tail { get; set; }
        public int Count { get; private set; }
        public bool IsReadOnly => false;
        public T this[int index]
        {
            get => GetNodeAt(index).Value;
            set => GetNodeAt(index).Value = value;
        }

        private Node<T> GetNodeAt(int index)
        {
            CheckIndex(index);

            var current = Head;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }

            return current;
        }

        public void Add(T item)
        {
            var newNode = new Node<T>
            {
                Value = item,
                Prev = Tail
            };

            if (Tail != null)
            {
                Tail.Next = newNode;
            }
            else
            {
                Head = newNode;
            }

            Tail = newNode;
            Count++;

            ItemChanged?.Invoke(this, new ItemChangedEventArgs<T>(item, Count - 1, "Added"));
        }

        public void Clear()
        {
            Tail = null;
            Head = null;
            Count = 0;
        }

        public bool Contains(T item)
        {
           if (IndexOf(item) == -1)
           {
                return false;
           }

            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException(Strings.CopyToArrayTooSmall);

            Node<T> current = Head;
            for (int i = 0; i < Count && current != null; i++)
            {
                array[arrayIndex + i] = current.Value;
                current = current.Next;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Node<T> current = Head;

            while (current != null)
            {
                yield return current.Value;

                current = current.Next;
            }
        }

        public int IndexOf(T item)
        {
            Node<T> current = Head;

            int index = 0;
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, item))
                    return index;

                current = current.Next;
                index++;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            CheckIndex(index);

            var newNode = new Node<T> { Value = item };

            if (index == 0)
            {
                newNode.Next = Head;

                if (Head != null)
                {
                    Head.Prev = newNode;
                }
                else
                {
                    Tail = newNode;
                }

                Head = newNode;
            }
            else if (index == Count)
            {
                newNode.Prev = Tail;

                if (Tail != null)
                {
                    Tail.Next = newNode;
                }
                else
                {
                    Head = newNode;
                }

                Tail = newNode;
            }
            else
            {
                var next = GetNodeAt(index);
                var prev = next.Prev;

                newNode.Next = next;
                newNode.Prev = prev;

                next.Prev = newNode;
                prev.Next = newNode;
            }

            Count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            RemoveAt(index);
            ItemChanged?.Invoke(this, new ItemChangedEventArgs<T>(item, index, "Removed"));

            return true;
        }

        public void RemoveAt(int index)
        {
            CheckIndex(index);

            var node = GetNodeAt(index);

            if (node.Prev != null)
            {
                node.Prev.Next = node.Next;
            }
            else
            {
                Head = node.Next;
            }

            if (node.Next != null)
            {
                node.Next.Prev = node.Prev;
            }

            Count--;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(" <-> ", this);
        }

        public T ApplyTo(int index, Func<T, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var node = GetNodeAt(index);

            return func(node.Value);
        }
        
        public void ApplyTo(int index, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var node = GetNodeAt(index);
            action(node.Value);
        }

        public SuperList<T> ApplyToAll(Func<T, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var result = new SuperList<T>();

            foreach (var item in this)
            {
                result.Add(func(item));
            }

            return result;
        }
        
        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var item in this)
            {
                action(item);
            }
        }

        public SuperList<T> ApplyWhere(Predicate<T> condition, Func<T, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var filtered = Filter(condition);

            return filtered.ApplyToAll(func);
        }
        
        public void ApplyWhere(Predicate<T> condition, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var filtered = Filter(condition);

            filtered.ForEach(action);
        }

        private SuperList<T> Filter(Predicate<T> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var filtered = new SuperList<T>();

            foreach (var item in this)
            {
                if (condition(item))
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }

        private void CheckIndex(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
