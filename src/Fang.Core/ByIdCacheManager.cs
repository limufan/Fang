using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fang.Core
{
    public class ByIdCacheManager<T> : CacheManager<T>
        where T : class, IIdProvider
    {
        public ByIdCacheManager()
        {
            this.DicById = new Dictionary<int, T>();
        }

        protected Dictionary<int, T> DicById { private set; get; }

        protected override void _Add(T cache)
        {
            base._Add(cache);

            if (this.DicById.ContainsKey(cache.ID))
            {
                FangLog.Logger.WarnFormat("{0} ID 重复ID: {1}", this.GetType().Name, cache.ID);
                return;
            }
            this.DicById.Add(cache.ID, cache);
        }

        public virtual void Remove(int id)
        {
            T cache = this.GetById(id);
            if (cache != null)
            {
                this.Remove(cache);
            }
        }

        protected override void _Remove(T cache)
        {
            base._Remove(cache);

            this.DicById.Remove(cache.ID);
        }

        protected override void _Clear()
        {
            base._Clear();

            this.DicById.Clear();
        }

        public virtual T GetById(int id)
        {
            this.EnableValidate();

            this.Lock.AcquireReaderLock(10000);
            try
            {
                if (this.DicById.ContainsKey(id))
                {
                    return this.DicById[id];
                }
                return default(T);
            }
            finally
            {
                this.Lock.ReleaseReaderLock();
            }
        }

        public override object Get(object key)
        {
            if (key == null)
            {
                return null;
            }

            if (key is int)
            {
                return this.GetById((int)key);
            }

            throw new ArgumentException(string.Format("不支持{0}类型获取", key.GetType().Name));
        }


        public int GetByIdCacheCount()
        {
            return this.DicById.Count;
        }
    }
}
