using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fang.Core
{
    public interface ICacheManager
    {
        CacheStatus Status { set; get; }

        bool Enable { get; }

        void Clear();

        int Count();


        event TEventHandler<ICacheManager> Enabled;

        object Get(object key);

        bool IsCache(Type type);

        List<object> Take(int count);
    }
}
