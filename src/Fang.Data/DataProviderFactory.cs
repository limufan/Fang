using Fang.Core;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Fang.Data
{
    public class SQLWatcher : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            System.Diagnostics.Debug.WriteLine("sql语句:" + sql);
            return base.OnPrepareStatement(sql);
        }
    }

    public class OpenNhibernateSessionInfo
    {
        public ISession Session {set;get;}

        public DateTime OpenTime {set;get;}

        public string Code { set; get; }
    }

    public class DataProviderFactory
    {
        public DataProviderFactory(CacheManagerContainer cacheManagerContainer, string databaseConfigPath)
        {
            if (string.IsNullOrEmpty(databaseConfigPath))
            {
                throw new ArgumentNullException("databaseConfigPath");
            }
            Configuration config = new NHibernate.Cfg.Configuration().Configure(databaseConfigPath);
            this.SessionFactory = config.BuildSessionFactory();
            this.CacheManagerContainer = cacheManagerContainer;
            this.DataProviders = new List<IDataProvider>();
            this.DataProviderTypes = ReflectionHelper.GetSubclass<IDataProvider>(this.GetType().Assembly);

            this.OpenNhibernateSessionInfoList = new ReaderWriterLockedList<OpenNhibernateSessionInfo>();
        }

        public string YunweiConnectionString { set; get; }

        public ISessionFactory SessionFactory { private set; get; }

        public CacheManagerContainer CacheManagerContainer { private set; get; }

        public List<IDataProvider> DataProviders { private set; get; }

        public Type[] DataProviderTypes { set; get; }

        public ReaderWriterLockedList<OpenNhibernateSessionInfo> OpenNhibernateSessionInfoList { set; get; }

        public void ConfigCommandTimeout(int seconds)
        {
            ISessionFactoryImplementor factoryImplementor = this.SessionFactory as ISessionFactoryImplementor;
            Dictionary<string, string> configSetting = new Dictionary<string, string>();
            configSetting.Add("command_timeout", seconds.ToString());
            factoryImplementor.ConnectionProvider.Driver.Configure(configSetting);
        }

        public ISession OpenSession(string code)
        {
#if DEBUG
            ISession session = this.SessionFactory.OpenSession(new SQLWatcher());
#else
            ISession session = this.SessionFactory.OpenSession();
#endif
            this.OpenNhibernateSessionInfoList.Add(new OpenNhibernateSessionInfo { Session = session, OpenTime = DateTime.Now, Code = code });
            return session;
        }

        public virtual T CreateYunweiDataProvider<T>() where T : IDataProvider
        {
            SqlConnection connection = new SqlConnection(this.YunweiConnectionString);
            connection.Open();
            ISession session = this.SessionFactory.OpenSession(connection);

            return this.CreateDataProvider<T>(session);
        }

        public virtual T CreateDataProvider<T>() where T: IDataProvider
        {
            ISession session = this.OpenSession(typeof(T).Name);

            return this.CreateDataProvider<T>(session);
        }

        public virtual T CreateDataProvider<T>(ISession session) where T : IDataProvider
        {
            T dataProvder = (T)this.CreateDataProvider(typeof(T), session);

            return dataProvder;
        }
        
        public virtual DataProvider<ModelType> CreateDataProviderByModelType<ModelType>() where ModelType : class
        {
            DataProvider<ModelType> dataProvider = this.CreateSubclassDataProvider<DataProvider<ModelType>>();

            return dataProvider;
        }

        protected virtual T CreateSubclassDataProvider<T>() where T : class
        {
            Type dataProviderType = ReflectionHelper.GetSingleSubclass<T>(this.DataProviderTypes);
            if (dataProviderType == null)
            {
                throw new ArgumentException("无法获取DataProvider类型");
            }

            T dataProvider = this.CreateDataProvider(dataProviderType) as T;

            if (dataProvider == null)
            {
                throw new ArgumentException("无法创建DataProvider");
            }

            return dataProvider;
        }

        protected virtual object CreateDataProvider(Type type)
        {
            ISession session = this.OpenSession(type.Name);

            return this.CreateDataProvider(type, session);
        }

        protected virtual object CreateDataProvider(Type type, ISession session) 
        {
            IDataProvider dataProvider =  Activator.CreateInstance(type, this.CacheManagerContainer, session) as IDataProvider;

            return dataProvider;
        }
    }
}
