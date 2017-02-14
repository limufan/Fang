using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fang
{
    public class ObjectMapperHelper
    {
        public static void Map(object target, object source, params ObjectMapper[] propertyMappers)
        {
            ObjectMapper mapper = new ObjectMapper(propertyMappers);
            mapper.Map(target, source);
        }

        public static ResultType Map<ResultType>(object source, params ObjectMapper[] propertyMappers)
        {
            ObjectMapper mapper = new ObjectMapper(propertyMappers);
            return mapper.Map<ResultType>(source);
        }
    }

    public class ObjectMapper
    {
        public ObjectMapper(params ObjectMapper[] propertyMappers)
        {
            this.PropertyMappers = new List<ObjectMapper>();
            this.PropertyMappers.AddRange(propertyMappers);
        }

        protected List<ObjectMapper> PropertyMappers {set;get;}

        protected virtual bool Map(object source, Type resultType, out object result)
        {
            Type sourceType = source.GetType();

            if (this.GuidToString(source, resultType, out result))
            {
                return true;
            }
            else if (this.ChildrenMap(source, resultType, out result))
            {
                return true;
            }
            else if (this.ListMap(source, resultType, out result))
            {
                return true;
            }
            else if (this.SerializabeMap(source, resultType, out result))
            {
                return true;
            }
            else if (this.ConstructorMap(source, resultType, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        private bool GuidToString(object source, Type resultType, out object result)
        {
            result = null;
            if (source is Guid && resultType == typeof(string))
            {
                Guid guid = (Guid)source;
                result = guid.ToString();
                return true;
            }
            else if (source is Guid? && resultType == typeof(string))
            {
                Guid? guid = (Guid?)source;
                if (guid.HasValue)
                {
                    result = guid.Value.ToString();
                }
                return true;
            }
            return false;
        }

        public virtual ResultType Map<ResultType>(object source)
        {
            if (source != null)
            {
                Type resultType = typeof(ResultType);
                object result = this.Map(source, resultType);
                if (result is ResultType)
                {
                    return (ResultType)result;
                }
            }
            return default(ResultType);
        }

        public object Map(object source, Type resultType)
        {
            object result = null;
            if (source != null)
            {
                this.Map(source, resultType, out result);
            }
            return result ;
        }

        /// <summary>
        /// 映射对象的字段
        /// </summary>
        /// <param name="target">要映射对象</param>
        /// <param name="source">映射数据源</param>
        public void Map(object target, object source)
        {
            if (source == null || target == null)
            {
                return;
            }
            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                PropertyInfo targetProperty = this.ResolveTargetProperty(targetType, sourceProperty.Name);
                if (targetProperty == null || !targetProperty.CanWrite)
                {
                    continue;
                }
                object sourceValue = sourceProperty.GetValue(source, null);
                if (sourceValue == null)
                {
                    targetProperty.SetValue(target, null, null);
                    continue;
                }
                object targetValue = null;
                if (targetProperty.PropertyType == sourceProperty.PropertyType)
                {
                    targetValue = sourceValue;
                    targetProperty.SetValue(target, targetValue, null);
                }
                else if (this.Map(sourceValue, targetProperty.PropertyType, out targetValue))
                {
                    targetProperty.SetValue(target, targetValue, null);
                }
                else if (targetProperty.PropertyType.IsEnum && sourceProperty.PropertyType.IsEnum)
                {
                    targetValue = sourceValue;
                    targetProperty.SetValue(target, targetValue, null);
                }
            }
        }

        protected virtual PropertyInfo ResolveTargetProperty(Type targetType, string name)
        {
            return targetType.GetProperty(name);
        }

        protected virtual bool ChildrenMap(object source, Type resultType, out object result)
        {
            if (this.PropertyMappers != null)
            {
                foreach (ObjectMapper mapper in this.PropertyMappers)
                {
                    if (mapper.Map(source, resultType, out result))
                    {
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        protected virtual bool ListMap(object source, Type resultType, out object result)
        {
            Type sourceType = source.GetType();
            if (ReflectionHelper.IsIList(sourceType) && ReflectionHelper.IsIList(resultType))
            {
                Type sourceItemType = ReflectionHelper.GetCollectionItemType(sourceType);
                Type resultItemType = ReflectionHelper.GetCollectionItemType(resultType);
                IList resultList = Activator.CreateInstance(resultType) as IList;
                IList sourceList = source as IList;
                foreach (object sourceItem in sourceList)
                {
                    object resultItem = null;
                    if(this.Map(sourceItem, resultItemType, out resultItem))
                    {
                        if (resultItem != null)
                        {
                            resultList.Add(resultItem);
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                result = resultList;
                return true;
            }

            result = null;
            return false;
        }

        protected virtual bool SerializabeMap(object source, Type resultType, out object result)
        {
            if (source is string && ReflectionHelper.IsSerializable(resultType))
            {
                string json = source as string;
                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject(json, resultType);
                }
                else
                {
                    result = null;
                }
                return true;
            }
            else if (ReflectionHelper.IsSerializable(source.GetType()) && resultType == typeof(string))
            {
                result = JsonConvert.SerializeObject(source);
                return true;
            }
            
            result = null;
            return false;
        }

        protected virtual bool ConstructorMap(object source, Type resultType, out object result)
        {
            ConstructorInfo cotrBySourceType = ReflectionHelper.GetConstructor(resultType, source.GetType());

            if (cotrBySourceType != null)
            {
                result = Activator.CreateInstance(resultType, source);
                return true;
            }
            else if (!ReflectionHelper.IsValueType(resultType) && ReflectionHelper.HasDefaultConstructor(resultType))
            {
                result = Activator.CreateInstance(resultType);
                this.Map(result, source);
                return true;
            }

            result = null;
            return false;
        }

    }
}
