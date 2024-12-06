using Enrollments.Domain.Pagination;
using Enrollments.Services.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Enrollments.Services
{
    public class PaginationService<TSource> : IPaginationService<TSource> where TSource : class
    {
        #region Private static members

        private const string NotOperator = " not ";

        public int TotalRecords { get; private set; }

        #endregion Private static members

        public Task<List<TSource>> ApplyPaginationAsync(IQueryable<TSource> data, PageRequest request)
        {
            var pe = Expression.Parameter(typeof(TSource), "SourceData");
            Expression searchExpression = null;

            if (request.Filters != null)
            {
                foreach (var filter in request.Filters)
                {
                    var filterExpression = GetExpression(filter.Values, filter.FieldName, pe);
                    searchExpression = searchExpression == null ? filterExpression : Expression.AndAlso(searchExpression, filterExpression);
                }
            }

            if (searchExpression != null)
                data = data.Where(Expression.Lambda<Func<TSource, bool>>(searchExpression, pe));

            TotalRecords = data.Count();

            if (!String.IsNullOrEmpty(request.SortField))
            {
                var sortOrder = request.SortOrder == EnumSortOrder.Descending ? "DESC" : "ASC";
                data = data.OrderBy($"{request.SortField} {sortOrder}");
            }

            data = data.Skip(request.PageOffset)
                .Take(request.PageSize);

            switch (data.Expression.Type.Namespace)
            {
                case "MongoDB.Driver.Linq":
                    return ((IMongoQueryable<TSource>)data).ToListAsync();

                case "System.Linq":
                default:
                    return Task.FromResult(data.ToList());
            }
        }

        private static Expression GetExpression(string[] searchTerms, string propertyName,
            ParameterExpression pe)
        {
            // Compose the expression tree that represents the parameter to the predicate.
            Expression propertyExp = pe;
            foreach (var member in propertyName.Split('.'))
            {
                propertyExp = Expression.PropertyOrField(propertyExp, member);
            }
            Expression searchExpression = null;
            Expression left;
            System.Reflection.MethodInfo method;
            if (propertyExp.Type != typeof(string))
            {
                left = propertyExp;
                method = propertyExp.Type.GetMethod("Equals", new[] { propertyExp.Type });
            }
            else
            {
                left = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            }

            for (int count = 0; count < searchTerms.Length; count++)
            {
                string searchTerm = searchTerms[count].ToLower();
                searchTerm = searchTerm.Replace("*", string.Empty);
                searchTerm = searchTerm.Replace("\"", string.Empty);
                Expression methodCallExpresssion;
                Expression rightExpression;
                if (searchTerm.Contains(NotOperator.TrimStart()))
                {
                    searchTerm = searchTerm.Replace(NotOperator.TrimStart(), string.Empty).Trim();
                    rightExpression = Expression.Constant(Convert.ChangeType(searchTerm, propertyExp.Type));
                    methodCallExpresssion = Expression.Call(left, method, rightExpression);
                    methodCallExpresssion = Expression.Not(methodCallExpresssion);
                }
                else
                {
                    rightExpression = Expression.Constant(Convert.ChangeType(searchTerm, propertyExp.Type));
                    methodCallExpresssion = Expression.Call(left, method, rightExpression);
                }

                if (count == 0)
                {
                    searchExpression = methodCallExpresssion;
                }
                else
                {
                    searchExpression = Expression.OrElse(searchExpression, methodCallExpresssion);
                }
            }
            if (propertyExp.Type == typeof(string))
            {
                Expression nullorEmptyCheck = Expression.Not(Expression.Call(typeof(string), typeof(string).GetMethod("IsNullOrEmpty").Name, null, propertyExp));
                return Expression.AndAlso(nullorEmptyCheck, searchExpression);
            }
            else
            {
                return searchExpression;
            }
        }
    }
}