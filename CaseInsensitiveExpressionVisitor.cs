using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Mono.Linq.Expressions;

namespace Funcular.ExtensionMethods
{
    public class CaseInsensitiveExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression b)
        {
            // if (b.NodeType == ExpressionType.AndAlso)
            if (b.NodeType == ExpressionType.Equal && b.Left.Type == typeof(string) && b.Right.Type == typeof(string))
            {
                Expression left = this.Visit(b.Left);
                Expression right = this.Visit(b.Right);

                // Make this binary expression an OrElse operation instead of an AndAlso operation. 
                // PredicateBuilder.False<string>().Or((string s) => s.Equals)
                return Expression.MakeBinary(ExpressionType.OrElse, left, right, b.IsLiftedToNull, b.Method);
            }

            return base.VisitBinary(b);
        }
    }
}
