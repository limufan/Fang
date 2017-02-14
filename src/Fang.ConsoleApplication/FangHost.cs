using Fang.Core;
using Fang.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fang.ConsoleApplication
{
    public class FangHost
    {
        public FangHost()
        {
            CacheManagerContainer cacheManagerContainer = new CacheManagerContainer();

            string databaseConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NH.config");
            this.DataProviderFactory = new DataProviderFactory(cacheManagerContainer, databaseConfigPath);
        }

        public DataProviderFactory DataProviderFactory { set; get; }

        public void Zhuanqu()
        {
            ErshouFangZhuanquqi ershouFangZhuanquqi = new ErshouFangZhuanquqi(this.DataProviderFactory, DateTime.Now);
            List<ErshouFangInfo> list = ershouFangZhuanquqi.Zhuanqu();
        }

        public void Fenxi()
        {
            ErshouFangHangqingFenxiqi ershouFangHangqingFenxiqi = new ErshouFangHangqingFenxiqi(this.DataProviderFactory);
            ershouFangHangqingFenxiqi.Fenxi();
        }
    }
}
