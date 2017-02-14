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
        const string HC_ESF = "合川二手房";
        const string CQ_DZL_ESF = "重庆大竹林二手房";

        public FangHost()
        {
            CacheManagerContainer cacheManagerContainer = new CacheManagerContainer();

            string databaseConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NH.config");
            this.DataProviderFactory = new DataProviderFactory(cacheManagerContainer, databaseConfigPath);
        }

        public DataProviderFactory DataProviderFactory { set; get; }

        public void Zhuaqu()
        {
            ErshouFangZhuanquqi ershouFangZhuanquqi = new ErshouFangZhuanquqi(this.DataProviderFactory, HC_ESF, "/house-a011841-b0261/c230-d265-l3100/");
            ershouFangZhuanquqi.Zhuanqu();

            ershouFangZhuanquqi = new ErshouFangZhuanquqi(this.DataProviderFactory, CQ_DZL_ESF, "/house-a058-b013070/c240-d290-j250-k2110-l3110/");
            ershouFangZhuanquqi.Zhuanqu();
        }

        public void Fenxi()
        {
            ErshouFangHangqingFenxiqi ershouFangHangqingFenxiqi = new ErshouFangHangqingFenxiqi(this.DataProviderFactory, HC_ESF, DateTime.Today, DateTime.Today.AddDays(-1).Date);
            ershouFangHangqingFenxiqi.Fenxi();

            ershouFangHangqingFenxiqi = new ErshouFangHangqingFenxiqi(this.DataProviderFactory, CQ_DZL_ESF, DateTime.Today, DateTime.Today.AddDays(-1).Date);
            ershouFangHangqingFenxiqi.Fenxi();
        }
    }
}
