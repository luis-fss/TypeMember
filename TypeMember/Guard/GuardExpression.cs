using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMember.Util;

namespace TypeMember.Guard
{
    public class GuardExpression
    {
        private readonly bool _execute;

        public GuardExpression(bool execute)
        {
            _execute = execute;
        }

        public void Throw<TException>(params object[] args) where TException : Exception
        {
            if (_execute == false) return;

            Exception exception;

            try
            {
                NewExpression ctorExpression;
                ConstructorInfo ctor;

                if (args.Length > 0)
                {
                    var argsEx = new Expression[args.Length];
                    var parameterTypes = new Type[args.Length];

                    for (var i = 0; i < args.Length; i++)
                    {
                        argsEx[i] = Expression.Constant(args[i]);
                        parameterTypes[i] = args[i].GetType();
                    }

                    ctor = ObjectFactory.GetConstructor<TException>(parameterTypes);
                    ctorExpression = Expression.New(ctor, argsEx);
                }
                else
                {
                    ctor = ObjectFactory.GetConstructor<TException>();
                    ctorExpression = Expression.New(ctor);
                }

                exception = Expression.Lambda<Func<TException>>(ctorExpression).Compile().Invoke();
            }
            catch (Exception)
            {
                var msg = args.Where(o => o is string).Aggregate(string.Empty, (current, o) => string.Concat(current, ", " + o));

                throw new Exception(msg.Remove(0, 2));
            }

            throw exception;
        }
    }
}