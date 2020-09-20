using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Exceptions;

namespace TypeMember.Util
{
    public static class ObjectFactory
    {
        const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public;

        public static ConstructorInfo GetConstructor<T>(Type[] parameterTypes = null)
        {
            return GetConstructor(typeof(T), parameterTypes);
        }

        public static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes = null)
        {
            var ctor = type.GetConstructor(DefaultBindingFlags, null, parameterTypes ?? Type.EmptyTypes, null);

            if (ctor == null)
            {
                var constructors = type.GetConstructors();
                ctor = constructors.SingleOrDefault(x => !x.GetParameters().Any());
            }

            return ctor;
        }

        public static T Hydrate<T>() where T : class, new()
        {
            var source = ObjectFactory<T>.Create();

            var propertiesPaths = typeof(T).GetAllPropertiesPaths();

            foreach (var propertiesPath in propertiesPaths)
            {
                var success = source.HydrateProperty(propertiesPath);
                if (success == false)
                    throw new Exception($"Could not hydrate property {propertiesPath} of type {typeof(T)}");
            }

            return source;
        }

        public static object Create(Type type)
        {
            if (Reflector.IsNotPrimitive(type))
            {
                return Activator.CreateInstance(type);
            }

            return default(Type);
        }
    }

    public static class ObjectFactory<T> where T : class
    {
        private static readonly Func<T> LambdaFactoryFn;
        private static readonly Func<T> ActivatorFactoryFn;

        static ObjectFactory()
        {
            ActivatorFactoryFn = CreateUsingActivator();
            LambdaFactoryFn = CreateUsingLambdas();
        }

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

        public static ConstructorInfo GetConstructor()
        {
            return ObjectFactory.GetConstructor<T>();
        }

        public static T Create(Action<T> init, bool useSystemActivator = false)
        {
            var instance = useSystemActivator ? ActivatorFactoryFn() : LambdaFactoryFn();

            init(instance);

            return instance;
        }

        public static T Create(bool useSystemActivator = false)
        {
            return useSystemActivator ? ActivatorFactoryFn() : LambdaFactoryFn();
        }
    }
}