using System.Collections.Generic;
using System.Collections.Specialized;

namespace Exercise_4.Models
{
    public class DataCollection : List<Data>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Add(Data data)
        {
            base.Add(data);
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, data));
        }

        public new void Clear()
        {
            base.Clear();
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}