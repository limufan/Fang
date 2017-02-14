using NHibernate;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Fang.Data
{
    public interface IDataProvider
    {

    }

    public class DataProvider<ModelType> : IDisposable, IDataProvider
        where ModelType : class
    {
        public DataProvider(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this.Session = session;
        }

        public DataProvider(ISessionFactory sessionFactory)
        {
            this.Session = sessionFactory.OpenSession();
        }

        internal ISession Session { set; get; }

        public ITransaction Transaction
        {
            get
            {
                return this.Session.Transaction;
            }
        }

        public ITransaction BeginTransaction()
        {
            if (this.Transaction != null && this.Transaction.IsActive)
            {
                throw new Exception("不能启动多个事务");
            }

            return this.Session.BeginTransaction();
        }

        public virtual object Insert(ModelType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            object id;

            id = this.Session.Save(model);
            this.Session.Flush();

            return id;
        }

        public virtual void Delete(ModelType model)
        {
            if (model == null)
            {
                return;
            }

            this.Session.Delete(model);
            this.Session.Flush();
        }

        public virtual void Update(ModelType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            this.Session.Update(model);
            this.Session.Flush();
        }

        public virtual void Refresh(ModelType model)
        {
            this.Session.Refresh(model);
        }

        public virtual object GetId(ModelType model)
        {
            return this.Session.SessionFactory.GetClassMetadata(typeof(ModelType)).GetIdentifier(model, EntityMode.Poco);
        }

        public virtual ModelType SelectById(object id)
        {
            ModelType model = this.Session.Get<ModelType>(id);

            return model;
        }

        public virtual ModelType TrySelectById(object id)
        {
            try
            {
                return this.SelectById(id);
            }
            catch
            {
                //出错以后再尝试一次
                Thread.Sleep(100);
                return this.SelectById(id);
            }
        }

        public virtual void DeleteById(object id)
        {
            ModelType model = this.SelectById(id);
            this.Delete(model);
        }

        public virtual List<ModelType> GetTopModels(int count)
        {
            List<ModelType> models =
                this.Session.QueryOver<ModelType>().Take(count)
                    .List()
                    .Distinct()
                    .ToList();

            return models;
        }

        public virtual IList<ModelType> GetModels()
        {
            return this.Session.QueryOver<ModelType>()
                    .List()
                    .Distinct()
                    .ToList();
        }

        public virtual IList<ModelType> SelectModels(Expression<Func<ModelType, bool>> expression)
        {
            return this.Session.QueryOver<ModelType>().Where(expression)
                    .List()
                    .Distinct()
                    .ToList();
        }

        public virtual ModelType SelectFirst(Expression<Func<ModelType, bool>> expression)
        {
            return this.Session.QueryOver<ModelType>().Where(expression).Take(1).SingleOrDefault();
        }

        public virtual int Count()
        {
            return this.Session.QueryOver<ModelType>().RowCount();
        }

        public virtual bool Exist(Expression<Func<ModelType, bool>> expression)
        {
            return this.Session.QueryOver<ModelType>().RowCount() > 0;
        }

        protected virtual object GetValueFromSqlDataReader(PropertyInfo prop, SqlDataReader reader)
        {
            object value = reader[prop.Name];
            if (value == DBNull.Value || value == null)
            {
                return null;
            }
            Type valueType = value.GetType();
            Type propertyType = ReflectionHelper.GetDefinitionType(prop.PropertyType);
            if (valueType == propertyType)
            {
                return value;
            }
            else if (valueType == typeof(Guid) && propertyType == typeof(string))
            {
                return value.ToString();
            }
            else if (valueType == typeof(int) && propertyType.IsEnum)
            {
                return Enum.Parse(propertyType, value.ToString());
            }
            return value;
        }

        public virtual List<ModelType> SelectBySqlDataReader()
        {
            return SelectBySqlDataReader(null, "");
        }

        public virtual List<ModelType> SelectBySqlDataReader(int? count, string additionalSql)
        {
            int modelCount = 0;
            if (count.HasValue)
            {
                modelCount = count.Value;
            }
            else
            {
                modelCount = this.Count();
            }
            PropertyInfo[] properties = typeof(ModelType).GetProperties();
            List<ModelType> models = new List<ModelType>(modelCount);
            string sql = this.GetSelectSql(count, additionalSql);

            SqlCommand cmd = new SqlCommand(sql, this.Session.Connection as SqlConnection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ModelType model = Activator.CreateInstance<ModelType>();
                foreach (PropertyInfo prop in properties)
                {
                    object value = this.GetValueFromSqlDataReader(prop, reader);
                    if (value != null)
                    {
                        prop.SetValue(model, value, null);
                    }
                }
                models.Add(model);
                if (models.Count % 1000000 == 0)
                {
                    Console.WriteLine(models.Count + " " + DateTime.Now);
                }
            }
            reader.Close();

            return models;
        }

        private string GetSelectSql(int? count, string additionalSql)
        {
            IClassMetadata metadata = this.Session.SessionFactory.GetClassMetadata(typeof(ModelType));
            SingleTableEntityPersister entityPersister = metadata as SingleTableEntityPersister;
            string[] subclassColumnClosure = entityPersister.GetType().GetProperty("SubclassColumnClosure", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(entityPersister, null) as string[];
            List<string> selectColumns = new List<string>();
            string idColumn = entityPersister.IdentifierPropertyName;
            selectColumns.Add(idColumn);
            for (int i = 0; i < entityPersister.PropertyNames.Length; i++)
            {
                string propertyName = entityPersister.PropertyNames[i];
                string columnName = subclassColumnClosure[i];
                string selectColumn = columnName;
                if (propertyName != columnName)
                {
                    selectColumn = string.Format("{0} as {1}", columnName, propertyName);
                }
                selectColumns.Add(selectColumn);
            }
            string top = "";
            if (count.HasValue)
            {
                top = "top " + count;
            }

            string sql = string.Format("SELECT {0} {1} from {2} {3}", top, string.Join(",", selectColumns), entityPersister.TableName, additionalSql);

            return sql;
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            this.Session.Close();
        }
    }
}
