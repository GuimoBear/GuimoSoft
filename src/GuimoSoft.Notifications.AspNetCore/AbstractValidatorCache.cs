using FluentValidation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GuimoSoft.Notifications.AspNetCore
{
    public static class AbstractValidatorCache<TErrorCode>
        where TErrorCode : struct, Enum
    {
        internal static readonly ConcurrentDictionary<Type, IEnumerable<ValidationExecutorBase<TErrorCode>>> _validatorsCache
            = new ConcurrentDictionary<Type, IEnumerable<ValidationExecutorBase<TErrorCode>>>();

        internal static void FindValidators(params Assembly[] assemblies)
        {
            _validatorsCache.Clear();

            var validatorTypes = assemblies.SelectMany(a => a.GetTypes().Where(t => (t.BaseType?.IsGenericType ?? false) &&
                                                                                    t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>) &&
                                                                                    typeof(NotifiableObject).IsAssignableFrom(t.BaseType.GetGenericArguments()[0])))
                                           .GroupBy(t => t.BaseType.GetGenericArguments()[0])
                                           .ToList();

            foreach (var g in validatorTypes)
            {
                _validatorsCache.TryAdd(g.Key, g.Select(type =>
                {
                    return Activator.CreateInstance
                    (
                        typeof(ValidationExecutor<,>).MakeGenericType(typeof(TErrorCode), type.BaseType.GetGenericArguments()[0]),
                        Activator.CreateInstance(type, true)
                    ) as ValidationExecutorBase<TErrorCode>;
                }).ToList());
            }
        }

        public static IEnumerable<ValidationExecutorBase<TErrorCode>> Get(Type validatorObjectType)
        {
            if (_validatorsCache.TryGetValue(validatorObjectType, out var validators))
            {
                foreach (var validator in validators)
                    yield return validator;
            }
        }
    }
}
