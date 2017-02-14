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
using System.IO;

namespace Fang.ConsoleApplication
{
    public class ErshouFangHangqingFenxiqi
    {
        public ErshouFangHangqingFenxiqi(DataProviderFactory dataProviderFactory)
        {
            ISession session = dataProviderFactory.OpenSession("ErshouFangHangqingFenxiqi");
            DataProvider<ErshouFangDataModel> dataProvider = new DataProvider<ErshouFangDataModel>(session);

            this.DataProvider = dataProvider;
        }

        public DataProvider<ErshouFangDataModel> DataProvider { set; get; }

        public void Fenxi()
        {
            IList<ErshouFangDataModel> models = this.DataProvider.GetModels();

            List<IGrouping<DateTime, ErshouFangDataModel>> groupingByZhuaquShijian = 
                models.GroupBy(m => m.ZhuanquShijian).OrderByDescending(x => x.Key).ToList();

            //this.FenxiXinzengFangyuan(groupingByZhuaquShijian[0].ToList(), groupingByZhuaquShijian[1].ToList());

            this.FenxiJiangjiaFangyuan(groupingByZhuaquShijian[0].ToList(), groupingByZhuaquShijian[1].ToList());
        }

        private void FenxiXinzengFangyuan(List<ErshouFangDataModel> ershoufangList, List<ErshouFangDataModel> duibiErshoufangList)
        {
            List<ErshouFangDataModel> zuixingFabuFangyuanList = ershoufangList
                .Where(esf => !duibiErshoufangList.Any(duibiEsf => duibiEsf.Url == esf.Url))
                .ToList();
            //最新发布
            Console.WriteLine(string.Format("最新发布：{0}", zuixingFabuFangyuanList.Count));

            List<IGrouping<string, ErshouFangDataModel>> zuixingFabuFangyuanGroupbyLoupan = 
                zuixingFabuFangyuanList.GroupBy(fangyuan => fangyuan.Loupan).OrderByDescending(g => g.Count()).ToList();

            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            StreamWriter sw = File.CreateText(@"C:\Users\Administrator\Desktop\Fang\最新发布\" + fileName);

            foreach (IGrouping<string, ErshouFangDataModel> grouping in zuixingFabuFangyuanGroupbyLoupan)
            {
                string zuixingFabuErshoufang = string.Format("{0}  最新发布：{1}", grouping.Key, grouping.Count());
                Console.WriteLine(zuixingFabuErshoufang);
                sw.WriteLine(zuixingFabuErshoufang);
                sw.WriteLine(string.Format("==============={0}===============", grouping.Key));
                List<ErshouFangDataModel> groupingList = grouping.OrderBy(m => m.Danjia).ToList();
                foreach (ErshouFangDataModel model in groupingList)
                {
                    sw.WriteLine(string.Format("{0}，面积: {1}， 总价: {2}， 单价: {3}，链接：{4}", 
                        model.Name, model.Mianji, model.Zongjia, model.Danjia, model.Url));
                }
            }

            sw.Close();

            //减少房源

            //降价房源

            //加价房源
        }

        private void FenxiJiangjiaFangyuan(List<ErshouFangDataModel> ershoufangList, List<ErshouFangDataModel> duibiErshoufangList)
        {
            List<ErshouFangDataModel> jiangjiaFangyuanList = ershoufangList
                .Where(esf => duibiErshoufangList.Any(duibiEsf => duibiEsf.Url == esf.Url && duibiEsf.Danjia > esf.Danjia))
                .ToList();
            //最新发布
            Console.WriteLine(string.Format("降价房源：{0}", jiangjiaFangyuanList.Count));

            List<IGrouping<string, ErshouFangDataModel>> zuixingFabuFangyuanGroupbyLoupan =
                jiangjiaFangyuanList.GroupBy(fangyuan => fangyuan.Loupan).OrderByDescending(g => g.Count()).ToList();

            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            StreamWriter sw = File.CreateText(@"C:\Users\Administrator\Desktop\Fang\降价\" + fileName);

            foreach (IGrouping<string, ErshouFangDataModel> grouping in zuixingFabuFangyuanGroupbyLoupan)
            {
                string jiangjiaErshoufang = string.Format("{0}  降价：{1}", grouping.Key, grouping.Count());
                Console.WriteLine(jiangjiaErshoufang);
                sw.WriteLine(jiangjiaErshoufang);
                sw.WriteLine(string.Format("==============={0}===============", grouping.Key));
                List<ErshouFangDataModel> groupingList = grouping.OrderBy(m => m.Danjia).ToList();
                foreach (ErshouFangDataModel model in groupingList)
                {
                    ErshouFangDataModel duibiErshoufang = duibiErshoufangList.Find(duibiEsf => duibiEsf.Url == model.Url);

                    sw.WriteLine(string.Format("{0}，面积: {1}， 总价: {2}， 单价: {3}, 降价: {4}，链接：{5}",
                        model.Name.Trim(), model.Mianji, model.Zongjia, model.Danjia, duibiErshoufang.Danjia - model.Danjia, model.Url));
                }
            }

            sw.Close();

            //减少房源

            //降价房源

            //加价房源
        }
    }
}
