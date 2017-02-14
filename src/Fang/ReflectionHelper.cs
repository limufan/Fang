using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fang
{
    public class ReflectionHelper
    {
        public static bool Is<T>(Type type)
        {
            Type tType = typeof(T);
            if (tType.IsInterface)
            {
                Type[] types = type.GetInterfaces();
                if (types != null && types.Any(t => t == tType))
                {
                    return true;
                }
            }
            else
            {
                return type.IsSubclassOf(tType);
            }
            return false;
        }

        public static bool IsValueType(Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            return false;
        }

        public static bool IsSerializable(Type type)
        {
            if (IsIList(type))
            {
                Type itemType = GetCollectionItemType(type);
                return IsSerializable(itemType);
            }
            else
            {
                object[] attributes = type.GetCustomAttributes(false);
                if (attributes == null)
                {
                    return false;
                }
                return attributes.Any(a => a is SerializableAttribute);
            }
        }

        public static List<TValue> GetPropertyValues<TAttribute, TValue>(object source) where TAttribute : Attribute
        { 
            List<PropertyInfo> properties = ReflectionHelper.GetProperties<TAttribute>(source.GetType())
                .Where(p => p.PropertyType == typeof(TValue)).ToList();
            List<TValue> keys = new List<TValue>();
            foreach (PropertyInfo property in properties)
            {
                TAttribute attribute = ReflectionHelper.GetAttribute<TAttribute>(property);
                object propertyValue = property.GetValue(source, null);
                if (propertyValue != null && propertyValue is TValue)
                {
                    TValue key = (TValue)propertyValue ;
                    if (!keys.Contains(key))
                    {
                        keys.Add(key);
                    }
                }
            }
            return keys;
        }

        public static T GetValue<T>(PropertyInfo property, object obj)
        {
            object value = property.GetValue(obj, null);
            if (value != null)
            {
                return (T)value;
            }
            return default(T);
        }

        public static object GetValue(PropertyInfo property, object obj)
        {
            object value = property.GetValue(obj, null);

            return value;
        }

        public static List<T> GetValue<T>(List<PropertyInfo> propertys, object obj)
        {
            List<T> list = new List<T>();
            foreach (PropertyInfo property in propertys)
            {
                T value = GetValue<T>(property, obj);
                if (value != null)
                {
                    list.Add(value);
                }
            }
            return list;
        }

        public static object GetPropertyValue<TAttribute>(object source) where TAttribute : Attribute
        { 
            PropertyInfo property = ReflectionHelper.GetProperties<TAttribute>(source.GetType()).ToList().FirstOrDefault();
            if (property != null)
            {
                return property.GetValue(source, null);
            }
            return null;
        }

        public static bool HasAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            object[] attributes = type.GetCustomAttributes(true);
            if (attributes != null && attributes.Any(a => a is TAttribute))
            {
                return true;
            }
            return false;
        }

        public static bool HasAttribute<TAttribute>(PropertyInfo propertyInfo) where TAttribute : Attribute
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);
            if (attributes != null && attributes.Any(a => a is TAttribute))
            {
                return true;
            }
            return false;
        }

        public static List<PropertyInfo> GetProperties<TAttribute>(Type type) where TAttribute : Attribute
        {
            List<PropertyInfo> resultList = new List<PropertyInfo>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(false);
                if(attributes != null && attributes.Any(a => a is TAttribute))
                {
                    resultList.Add(property);
                }
            }
            return resultList;
        }

        public static List<PropertyInfo> GetProperties<TAttribute, TProperty>(Type type) where TAttribute : Attribute
        {
            List<PropertyInfo> resultList = new List<PropertyInfo>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType != typeof(TProperty))
                {
                    continue;
                }
                object[] attributes = property.GetCustomAttributes(false);
                if(attributes != null && attributes.Any(a => a is TAttribute))
                {
                    resultList.Add(property);
                }
            }
            return resultList;
        }

        public static PropertyInfo GetProperty<TAttribute>(Type type) where TAttribute : Attribute
        {
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(false);
                if(attributes != null && attributes.Any(a => a is TAttribute))
                {
                    return property;
                }
            }
            return null;
        }

        public static TAttribute GetAttribute<TAttribute>(ICustomAttributeProvider provider) where TAttribute : Attribute
        {
            object[] attributes = provider.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes != null)
            {
                object  attribute = attributes.FirstOrDefault(a => a is TAttribute);
                return attribute as TAttribute;
            }
            return null;
        }
        
        public static bool IsValueTypeIList(Type type)
        {
            if (!IsIList(type))
            {
                return false;
            }

            Type itemType = GetCollectionItemType(type);
            return IsValueType(itemType);
        }

        public static bool IsReferenceTypeIList(Type type)
        {
            if (!IsIList(type))
            {
                return false;
            }

            Type itemType = GetCollectionItemType(type);
            return !IsValueType(itemType);
        }

        public static bool IsIList<T>(Type type)
        {
            if (!IsIList(type))
            {
                return false;
            }

            Type itemType = GetCollectionItemType(type);
            return Is<T>(itemType);
        }

        public static Type GetCollectionItemType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Type genericListType;

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out genericListType))
            {
                if (!genericListType.IsGenericTypeDefinition)
                {
                    return genericListType.GetGenericArguments()[0];
                }
            }
            
            throw new Exception("无法获取集合元素类型.");
        }

        public static bool IsIList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return typeof(IList).IsAssignableFrom(type);
        }

        public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            if (!type.IsGenericType)
                return false;

            Type t = type.GetGenericTypeDefinition();
            return (t == genericInterfaceDefinition);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            Type implementingType;
            return ImplementsGenericDefinition(type, genericInterfaceDefinition, out implementingType);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (genericInterfaceDefinition == null)
            {
                throw new ArgumentNullException("genericInterfaceDefinition");
            }

            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException("not a generic interface definition.");

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool HasDefaultConstructor(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsValueType)
                return true;

            return (GetDefaultConstructor(type) != null);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t)
        {
            return GetDefaultConstructor(t, false);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic)
                bindingFlags = bindingFlags | BindingFlags.NonPublic;

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
        }

        public static ConstructorInfo GetConstructor(Type t, Type paramType)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => { 
                ParameterInfo[] cotrParams = c.GetParameters();
                return cotrParams.Length == 1 && cotrParams[0].ParameterType == paramType;
            });
        }

        public static ConstructorInfo GetConstructor(Type t)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public ;

            return t.GetConstructors(bindingFlags).FirstOrDefault();
        }

        public static Type GetDefinitionType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static PropertyInfo GetProperty<T, R>(System.Linq.Expressions.Expression<Func<T, R>> expression)
        {
            MemberExpression me = expression.Body as MemberExpression;
            if(me == null)
            {
                throw new ArgumentException("只能解析MemberExpression");
            }
            PropertyInfo propertyInfo = me.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("不是PropertyInfo类型");
            }
            return propertyInfo;
        }

        public static T CreateSubClassInstance<T>(Assembly assembly, params object[] args)
        {
            Type type = GetSingleSubclass<T>(assembly);
            if(type == null)
            {
                return default(T);
            }
            T obj = (T)Activator.CreateInstance(type, args);

            return obj;
        }

        public static Type[] GetSubclass<T>(Type[] types)
        {
            List<Type> subclassTypes = new List<Type>();

            foreach (Type type in types)
            {
                if (Is<T>(type))
                {
                    subclassTypes.Add(type);
                }
            }

            return subclassTypes.ToArray();
        }

        public static Type[] GetSubclass<T>(Assembly assembly)
        {
            Type[] types = assembly.GetExportedTypes();

            return GetSubclass<T>(types);
        }

        public static Type GetSingleSubclass<T>(Assembly assembly)
        {
            Type[] types = assembly.GetExportedTypes();

            return GetSingleSubclass<T>(types);
        }

        public static Type GetSingleSubclass<T>(Type[] types)
        {
            Type[] subclassTypes = GetSubclass<T>(types);
            if (subclassTypes.Length > 1)
            {
                throw new Exception(string.Format("查找到{0}多子类", typeof(T).Name));
            }

            if (subclassTypes.Length == 0)
            {
                return null;
            }
            return subclassTypes.First();
        }
    }
}
