using Fang.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fang.Data
{
    public class ErshouFangDataModel
    {
        public virtual int ID { set; get; }

        public virtual string Name { set; get; }

        public virtual string Url { set; get; }

        public virtual string Huxing { set; get; }

        public virtual string Loucheng { set; get; }

        public virtual string JianzhuNiandai { set; get; }

        public virtual string Loupan { set; get; }

        public virtual string Zhongjie { set; get; }

        public virtual double Mianji { set; get; }

        public virtual double Zongjia { set; get; }

        public virtual double Danjia { set; get; }

        public virtual DateTime ZhuanquShijian { set; get; }

        public virtual string ZhuanquBiaoti { set; get; }
    }
}
