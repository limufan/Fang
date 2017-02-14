using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fang.Core
{
    public class CacheChangeArgs<InfoType, ChangeInfoType>
    {
        public InfoType SnapshotInfo { set; get; }

        public ChangeInfoType ChangeInfo { set; get; }
    }

    public class CacheCodeChangedArgs
    {
        public string SnapshotCode { set; get; }

        public string ChangeCode { set; get; }
    }

    public class CacheGuidChangedArgs
    {
        public string SnapshotGuid { set; get; }

        public string ChangeGuid { set; get; }
    }

    public interface ICacheCodeChanged<CacheType>
    {
        event TEventHandler<CacheType, CacheCodeChangedArgs> CodeChanged;
    }

    public interface ICacheGuidChanged<CacheType>
    {
        event TEventHandler<CacheType, CacheGuidChangedArgs> GuidChanged;
    }
}
