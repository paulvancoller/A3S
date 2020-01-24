/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace za.co.grindrodbank.a3s.Extensions
{
    static class QueryableExtensions
    {
        public static IOrderedQueryable<T> AppendOrderBy<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> sortSelector, bool ascending = true)
        {
            var ordered = source as IOrderedQueryable<T>;
            if (ordered != null)
            {
                var lastMethod = (source.Expression as MethodCallExpression)?.Method;

                if (lastMethod?.DeclaringType == typeof(Queryable))
                    switch (lastMethod.Name)
                    {
                        case nameof(Queryable.OrderBy):
                        case nameof(Queryable.OrderByDescending):
                        case nameof(Queryable.ThenBy):
                        case nameof(Queryable.ThenByDescending):
                            return ascending ? ordered.ThenBy(sortSelector) : ordered.ThenByDescending(sortSelector);
                        default:
                            return ascending ? ordered.OrderBy(sortSelector) : ordered.OrderByDescending(sortSelector);
                    }
            }

            return source.OrderBy(sortSelector);
        }
    }
}
