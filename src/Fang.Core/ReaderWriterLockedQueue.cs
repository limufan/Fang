using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fang.Core
{
    public class ReaderWriterLockedQueue<T>
    {
        public ReaderWriterLockedQueue()
        {
            this._lock = new ReaderWriterLock();
            this._queue = new Queue<T>();
            this.MaxCount = 100000000;
        }

        ReaderWriterLock _lock;
        Queue<T> _queue;

        public int MaxCount { set; get; }

        public void Enqueue(T t)
        {
            this._lock.AcquireWriterLock(1000);
            try
            {
                if (this.Count > this.MaxCount)
                {
                    FangLog.Logger.WarnFormat("ReaderWriterLockedQueue {0} Count: {1}", typeof(T).Name, this.Count);
                    this.Clear();
                }

                this._queue.Enqueue(t);
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public T Dequeue()
        {
            this._lock.AcquireWriterLock(1000);
            try
            {
                return this._queue.Dequeue();
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public T Peek()
        {
            this._lock.AcquireWriterLock(1000);
            try
            {
                return this._queue.Peek();
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public void Clear()
        {
            this._lock.AcquireWriterLock(1000);
            try
            {
                this._queue.Clear();
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public int Count
        {
            get
            {
                return this._queue.Count;
            }
        }
    }
}
