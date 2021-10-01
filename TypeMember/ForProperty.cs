using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMember.TinyCache;
using TypeMember.Util;

namespace TypeMember
{
    public class ForProperty
    {
        private static ITinyCache<string> Cache =>
            // 3600000 Milliseconds = 60 Minutes
            // 900000 Milliseconds = 15 Minutes
            //                         60 Min,  15 Min
            new TimedTinyCache<string>(3600000, 900000);
        
        public bool IsValidPropertyPath<T>(string propertyPath)
        {
            return typeof(T).IsValidPropertyPath(propertyPath);
        }

        public string GetPropertyPath<T>(Expression<Func<T, object>> expression, string collectionSuffix = null)
        {
            return expression.GetPropertyPath(collectionSuffix);
        }

        public HashSet<string> GetAllPropertiesPaths<T>()
        {
            return GetAllPropertiesPathsWithCache(typeof(T));
        }

        internal HashSet<string> GetAllPropertiesPathsWithCache(Type type)
        {
            var getAllPropertiesPaths = Cache.GetOrSetItem("getAllPropertiesPathsCacheKey-{93e7c805-2277-4c03-a82b-e7a4b54c8e94}",
                () => new ConcurrentDictionary<Type, HashSet<string>>());

            return getAllPropertiesPaths.GetOrAdd(type,
                tp => GetAllPropertiesPathsWithoutCache(tp, null));
        }

        private HashSet<string> GetAllPropertiesPathsWithoutCache(Type objectType, string rootPath)
        {
            if (!string.IsNullOrWhiteSpace(rootPath) && rootPath.StartsWith("."))
                rootPath = rootPath.Substring(1);

            var dicProperties = new HashSet<string>();
            var properties = objectType.GetProperties();

            foreach (var property in properties)
            {
                var val = property.PropertyType;
                if (property.PropertyType.IsClass == false || property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var path = string.IsNullOrWhiteSpace(rootPath) ? property.Name : $"{rootPath}.{property.Name}";
                    dicProperties.Add(path);
                }
                else
                {
                    dicProperties.UnionWith(GetAllPropertiesPathsWithoutCache(val, $"{rootPath}.{property.Name}"));
                }
            }

            return dicProperties;
        }

        public bool IsNotPrimitive(Type t)
        {
            return new[] {
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime)
            }.Contains(t) == false /*&& !t.IsInterface && !t.IsArray && t.IsClass && !t.Module.Name.Equals("mscorlib.dll")*/;
        }
    }
}