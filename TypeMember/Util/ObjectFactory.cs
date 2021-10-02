using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Exceptions;

namespace TypeMember.Util
{
    public static class ObjectFactory<T> where T : class
    {
        private static Func<T> CreateUsingActivator()
        {
            var type = typeof(T);

            T Func() => Activator.CreateInstance(type, true) as T;

            return Func;
        }

        private static Func<T> CreateUsingLambdas()
        {
            var ctor = GetConstructor();

            if (ctor != null)
            {
                var ctorExpression = Expression.New(ctor);
                return Expression.Lambda<Func<T>>(ctorExpression).Compile();
            }

            throw new NoDefaultConstructorException();
        }

        public static T Create(Action<T> init, bool useSystemActivator = false)
        {
            var instance = useSystemActivator ? CreateUsingActivator().Invoke() : CreateUsingLambdas().Invoke();

            init(instance);

            return instance;
        }

        public static T Create(bool useSystemActivator = false)
        {
            return useSystemActivator ? CreateUsingActivator().Invoke() : CreateUsingLambdas().Invoke();
        }

        public static object Create(Type type)
        {
            return Reflector.Type.IsPrimitive(type) ? default(Type) : Activator.CreateInstance(type);
        }
        
        public static ConstructorInfo GetConstructor(Type[] parameterTypes = null)
        {
            return GetConstructor(typeof(T), parameterTypes);
        }

        public static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes = null)
        {
            var ctor = type.GetConstructor(Reflector.DefaultBindings, null, parameterTypes ?? Type.EmptyTypes, null);

            if (ctor == null)
            {
                var constructors = type.GetConstructors();
                ctor = constructors.SingleOrDefault(x => !x.GetParameters().Any());
            }

            return ctor;
        }

        public static T Hydrate()
        {
            var source = Create();

            var propertiesPaths = typeof(T).GetAllPropertiesPaths();

            foreach (var propertiesPath in propertiesPaths)
            {
                var success = source.HydrateProperty(propertiesPath);
                if (success == false)
                {
                    throw new Exception($"Could not hydrate property {propertiesPath} of type {typeof(T)}");
                }
            }

            return source;
        }
    }
}