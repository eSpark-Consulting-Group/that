using System;
using System.Linq.Expressions;
using System.Text;

namespace APTAT.DAL.Storage.Core.Sorting
{
    internal static class SortingExpressionFactory<TSource>
    {
        public static ISortingExpression<TSource> GetSortingExpression(string keyName)
        {
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            if (keyName.Length == 0)
                throw new ArgumentException("The value should not be an empty string.", "keyName");

            ISortingExpression<TSource> result = CreateSortingExpessionInstance(keyName);

            return result;
        }

        private static ISortingExpression<TSource> CreateSortingExpessionInstance(string propertyName)
        {
            Type sourceType = typeof(TSource);
            string name = GetParamName(sourceType);
            ParameterExpression param = Expression.Parameter(sourceType, name);
            MemberExpression member = Expression.Property(param, propertyName);
            Type returnType = member.Type;

            Type funcType = typeof(Func<,>).MakeGenericType(sourceType, returnType);

            LambdaExpression lambdaExpression = Expression.Lambda(funcType, member, param);

            ISortingExpression<TSource> sortingExpessionInstance = CreateSortingExpessionInstance(lambdaExpression, sourceType, returnType);

            return sortingExpessionInstance;
        }

        private static string GetParamName(Type paramType)
        {
            var paramName = new StringBuilder(paramType.Name);
            char firstChar = paramName[0];
            paramName[0] = Char.IsLower(firstChar) ? '_' : Char.ToLowerInvariant(firstChar);
            return paramName.ToString();
        }

        private static ISortingExpression<TSource> CreateSortingExpessionInstance(LambdaExpression lambdaExpression, Type sourceType, Type returnType)
        {
            Type sortingExpressionType = typeof(SortingExpression<,>).MakeGenericType(sourceType, returnType);
            object instance = Activator.CreateInstance(sortingExpressionType, new object[] { lambdaExpression });

            return (ISortingExpression<TSource>)instance;
        }
    }
}
