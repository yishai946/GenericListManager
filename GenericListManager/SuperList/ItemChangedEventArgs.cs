namespace GenericListManager.SuperList
{
    public class ItemChangedEventArgs<T> : EventArgs
    {
        public T Item { get; }
        public int Index { get; }
        public string Action { get; }

        public ItemChangedEventArgs(T item, int index, string action)
        {
            Item = item;
            Index = index;
            Action = action;
        }
    }
}
