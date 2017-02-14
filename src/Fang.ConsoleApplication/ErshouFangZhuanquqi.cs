using AngleSharp;
using AngleSharp.Dom;
using Fang.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fang.Data;
using NHibernate;
using System.Threading;

namespace Fang.ConsoleApplication
{
    public class ErshouFangZhuanquqi
    {
        public ErshouFangZhuanquqi(DataProviderFactory dataProviderFactory, DateTime zhuanquShijian)
        {
            ISession session = dataProviderFactory.OpenSession("ershoufang");
            DataProvider<ErshouFangDataModel> dataProvider = new DataProvider<ErshouFangDataModel>(session);

            this.DataProvider = dataProvider;
            this.ZhuanquShijian = zhuanquShijian;
        }

        public DataProvider<ErshouFangDataModel> DataProvider { set; get; }

        public DateTime ZhuanquShijian { set; get; }

        public List<ErshouFangInfo> Zhuanqu()
        {
            List<ErshouFangInfo> list = this.ZhuanquInfo("http://esf.cq.fang.com/house-a011841-b0261/y71/");
            this.InsertDatabase(list);
            Console.WriteLine("抓取完成");

            return list;
        }

        public List<ErshouFangInfo> ZhuanquInfo(string url)
        {
            Console.WriteLine("抓取：" + url);

            var config = Configuration.Default.WithDefaultLoader();
            var document = BrowsingContext.New(config).OpenAsync(url).Result;
            var houseListSelector = ".houseList dl";
            var houseList = document.QuerySelectorAll(houseListSelector);

            List<ErshouFangInfo> infoList = houseList.Select(e => this.Map(e)).ToList();

            if (document.QuerySelector(".btnRight.mt8.ml10.floatl") != null)
            {
                var nextPageUrl = document.QuerySelector(".btnRight.mt8.ml10.floatl").Attributes["href"].Value;
                if (nextPageUrl != null)
                {
                    Thread.Sleep(1000);
                    nextPageUrl = "http://esf.cq.fang.com" + nextPageUrl;
                    infoList.AddRange(this.ZhuanquInfo(nextPageUrl));
                }
            }

            return infoList;

        }

        public void InsertDatabase(List<ErshouFangInfo> infoList)
        {
            List<ErshouFangDataModel> models = infoList.Select(info => ObjectMapperHelper.Map<ErshouFangDataModel>(info)).ToList();
            foreach(ErshouFangDataModel model in models)
            {
                this.DataProvider.Insert(model);
            }
        }


        public ErshouFangInfo Map(IElement e)
        {
            ErshouFangInfo info = new ErshouFangInfo();
            info.Name = e.QuerySelector(".title").TextContent.Trim();
            info.Url = e.QuerySelector(".title a").Attributes["href"].Value.Trim();
            info.Huxing = e.QuerySelector(".mt12").TextContent.Trim();
            info.Loupan = e.QuerySelector(".mt10 span").TextContent.Trim();
            if(e.QuerySelector(".gray6 a") != null)
            {
                info.Zhongjie = e.QuerySelector(".gray6 a").TextContent.Trim();
            }
            string mianjiContent = e.QuerySelector(".area p").FirstChild.TextContent.Replace("?", "");
            info.Mianji = double.Parse(mianjiContent);
            info.Zongjia = double.Parse(e.QuerySelector(".price").TextContent);
            string danjiaContent = e.QuerySelector(".danjia").TextContent.Replace("元/?", "");
            info.Danjia = double.Parse(danjiaContent);
            info.ZhuanquShijian = this.ZhuanquShijian;

            return info;
        }
    }
}
