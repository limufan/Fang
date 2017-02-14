using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fang.Core
{
    public class CacheMapper: ObjectMapper
    {
        public CacheMapper(CacheManagerContainer coreManager)
            : base()
        {
            this._coreManager = coreManager;
        }
        CacheManagerContainer _coreManager;

        protected override bool Map(object source, Type resultType, out object result)
        {
            Type sourceType = source.GetType();
            if (this.KeyToObject(source, resultType, out result))
            {
                return true;
            }
            else if (this.ObjectToKey(source, resultType, out result))
            {
                return true;
            }
            else if (this.ModelToObject(source, resultType, out result))
            {
                return true;
            }

            return base.Map(source, resultType, out result);
        }

        private bool KeyToObject(object source, Type resultType, out object result)
        {
            result = null;

            if ((source is string || source is int) && !ReflectionHelper.IsIList(resultType))
            {
                if (this._coreManager.Contains(resultType))
                {
                    object key = source;
                    result = this._coreManager.Get(key, resultType);
                    return true;
                }
            }
            else if (source is string && ReflectionHelper.IsIList(resultType))
            {
                Type resultItemType = ReflectionHelper.GetCollectionItemType(resultType);
                if (this._coreManager.Contains(resultItemType))
                {
                    ICacheManager manager = this._coreManager.GetManager(resultItemType);
                    string formatedKey = source as string;
                    string[] keys = formatedKey.Split(',');
                    result = Activator.CreateInstance(resultType);
                    IList resultList = result as IList;
                    foreach (string key in keys)
                    {
                        resultList.Add(manager.Get(key));
                    }
                    return true;
                }   
            }
            return false;
        }

        private bool ObjectToKey(object source, Type resultType, out object result)
        {
            result = null;
            Type sourceType = source.GetType();

            if (!ReflectionHelper.IsIList(sourceType) && (resultType == typeof(string) || resultType == typeof(int)))
            {
                if(resultType == typeof(int) && source is IIdProvider)
                {
                    result = (source as IIdProvider).ID;
                    return true;
                }
                else if (resultType == typeof(string) && source is IGuidProvider)
                {
                    result = (source as IGuidProvider).Guid;
                    return true;
                }
                else if (resultType == typeof(string) && source is ICodeProvider)
                {
                    result = (source as ICodeProvider).Code;
                    return true;
                }

                return false;
            }
            else if (ReflectionHelper.IsIList<IIdProvider>(sourceType) && resultType == typeof(string))
            {
                List<object> keyList = new List<object>();
                IList list = source as IList;
                foreach (object obj in list)
                {
                    object key = key = (source as IIdProvider).ID;
                    keyList.Add(key);
                }
                result = string.Join(",", keyList);
                return true;
            }

            return false;
        }

        private bool ModelToObject(object source, Type resultType, out object result)
        {
            result = null;
            Type sourceType = source.GetType();

            if ((ReflectionHelper.Is<IIdProvider>(sourceType) || ReflectionHelper.Is<ICodeProvider>(sourceType) || ReflectionHelper.Is<IGuidProvider>(sourceType)) 
                && !this._coreManager.Contains(sourceType) 
                && this._coreManager.Contains(resultType))
            {
                if (source is IIdProvider)
                {
                    IIdProvider idProviderSource = source as IIdProvider;
                    result = this._coreManager.Get(idProviderSource.ID, resultType);
                    return true;
                }
                else if (source is IGuidProvider)
                {
                    IGuidProvider guidProviderSource = source as IGuidProvider;
                    result = this._coreManager.Get(guidProviderSource.Guid, resultType);
                    return true;
                }
                else if (source is ICodeProvider)
                {
                    ICodeProvider codeProviderSource = source as ICodeProvider;
                    result = this._coreManager.Get(codeProviderSource.Code, resultType);
                    return true;
                }
            }

            return false;
        }
    }

    public class TCacheMapper<MapType, SourceType> : CacheMapper
    {
        public TCacheMapper(CacheManagerContainer cacheManagerContainer)
            : base(cacheManagerContainer)
        {
            this.CacheManagerContainer = cacheManagerContainer;
        }

        public CacheManagerContainer CacheManagerContainer { set; get; }
    }

    public class CacheMapperFactory
    {
        public CacheMapperFactory(CacheManagerContainer cacheManagerContainer)
        {
            this.CacheManagerContainer = cacheManagerContainer;
        }

        public CacheManagerContainer CacheManagerContainer { set; get; }

        public virtual TCacheMapper<MapType, SourceType> Create<MapType, SourceType>()
        {
            TCacheMapper<MapType, SourceType> mapper = null;

            Type mapperType = ReflectionHelper.GetSingleSubclass<TCacheMapper<MapType, SourceType>>(this.GetTypes());
            if (mapperType == null)
            {
                mapper = new TCacheMapper<MapType, SourceType>(this.CacheManagerContainer);
            }
            else
            {
                mapper = Activator.CreateInstance(mapperType, this.CacheManagerContainer) as TCacheMapper<MapType, SourceType>;
            }

            if (mapper == null)
            {
                throw new ArgumentNullException("mapper");
            }

            return mapper;
        }

        public virtual CacheMapper Create<T>()
        {
            return new CacheMapper(this.CacheManagerContainer);
        }

        protected virtual Type[] GetTypes()
        {
            return this.GetType().Assembly.GetExportedTypes();
        }
    }
}
