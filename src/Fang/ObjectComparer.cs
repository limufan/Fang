using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fang
{
    public class ObjectComparer
    {
        public ObjectComparer()
        {
        }

        public ObjectComparer(params PropertyInfo[] ignoreProerties)
            : this()
        {
            this.IgnoreProerties = new List<PropertyInfo>();
            this.IgnoreProerties.AddRange(ignoreProerties);
        }

        public List<PropertyInfo> IgnoreProerties { set; get; }

        public bool OnlySamePropertyType { set; get; }

        public bool Compare(object obj1, object obj2)
        {
            return this.Compare(obj1, obj2, 1);
        }

        protected virtual bool Compare(object obj1, object obj2, int depth)
        {
            if (depth > 3 || obj1 == obj2)
            {
                return true;
            }

            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            if (obj1 is IEnumerable)
            {
                return this.CompareEnumer(obj1 as IEnumerable, obj2 as IEnumerable);
            }
            else
            {
                Type obj1Type = obj1.GetType();
                Type obj2Type = obj2.GetType();
                PropertyInfo[] obj1Propertys = obj1Type.GetProperties();
                foreach (PropertyInfo obj1Property in obj1Propertys)
                {
                    if (this.IgnoreProerties != null && this.IgnoreProerties.Any(ip => ip.Name == obj1Property.Name))
                    {
                        continue;
                    }
                    PropertyInfo obj2Property = obj2Type.GetProperty(obj1Property.Name);
                    if (obj2Property == null)
                    {
                        continue;
                    }

                    if (this.OnlySamePropertyType && obj1Property.PropertyType != obj2Property.PropertyType)
                    {
                        continue;
                    }

                    object obj1Value = obj1Property.GetValue(obj1, null);
                    object obj2Value = obj2Property.GetValue(obj2, null);

                    if (ReflectionHelper.IsValueType(obj1Property.PropertyType))
                    {
                        if (obj1Value == obj2Value)
                        {
                            continue;
                        }
                        if (obj1Value != null && obj1Value.Equals(obj2Value))
                        {
                            continue;
                        }
                        if (JsonConvert.SerializeObject(obj1Value) == JsonConvert.SerializeObject(obj2Value))
                        {
                            continue;
                        }
                        if (obj1Value != null && obj2Value != null && obj1Value.ToString() == obj2Value.ToString())
                        {
                            continue;
                        }
                        throw new Exception(string.Format("{0} {1} 不相等, 期望值: {2}, 实际值: {3}",
                                obj1Type.Name, obj1Property.Name, obj1Value, obj2Value));
                    }
                    else
                    {
                        if (!this.Compare(obj1Value, obj2Value, depth + 1))
                        {
                            throw new Exception(string.Format("{0} {1} 不相等, 期望值: {2}, 实际值: {3}",
                                obj1Type.Name, obj1Property.Name, obj1Value, obj2Value));
                        }
                    }
                }
            }
            return true;
        }

        private bool CompareEnumer(IEnumerable obj1, IEnumerable obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            IEnumerator enumerator1 = obj1.GetEnumerator();
            IEnumerator enumerator2 = obj2.GetEnumerator();
            while (true)
            {
                bool enumerator1HasItem = enumerator1.MoveNext();
                bool enumerator2HasItem = enumerator2.MoveNext();

                if (enumerator1HasItem != enumerator2HasItem)
                {
                    return false;
                }

                if (!enumerator1HasItem)
                {
                    break;
                }

                if (!this.Compare(enumerator1.Current, enumerator2.Current, 1))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
