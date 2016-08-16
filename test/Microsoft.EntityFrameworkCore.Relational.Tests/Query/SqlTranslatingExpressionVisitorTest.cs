// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Query
{
    public class SqlTranslatingExpressionVisitorTest
    {
        [Fact]
        public void Null_conditional_expression_gets_pruned_during_sql_translation()
        {
            var foo = Expression.Constant("Foo");
            var toUpper = Expression.Call(foo, "ToUpper", new Type[] { });
            var nullConditional1 = new NullConditionalExpression(foo, foo, toUpper);
            var toLower = Expression.Call(toUpper, "ToLower", new Type[] { });
            var nullConditional2 = new NullConditionalExpression(nullConditional1, toUpper, toLower);

            var relationalAnnotationProvider = new Mock<IRelationalAnnotationProvider>();
            var compositeExpressionFragmentTranslator = new Mock<IExpressionFragmentTranslator>();
            var methodCallTranslator = new Mock<IMethodCallTranslator>();
            methodCallTranslator.Setup(m => m.Translate(It.IsAny<MethodCallExpression>())).Returns<MethodCallExpression>(r => r.Method.Name.Contains("ToUpper") ? toUpper : toLower);

            var memberTranslator = new Mock<IMemberTranslator>();
            var relationalTypeMapper = new Mock<IRelationalTypeMapper>();
            relationalTypeMapper.Setup(m => m.FindMapping(typeof(string))).Returns(new RelationalTypeMapping("string", typeof(string)));

            var queryOptimizer = new Mock<IQueryOptimizer>();
            var navigationRewritingExpressionVisitorFactory = new Mock<INavigationRewritingExpressionVisitorFactory>();
            var subQueryMemberPushDownExpressionVisitor = new Mock<SubQueryMemberPushDownExpressionVisitor>();
            var querySourceTracingExpressionVisitorFactory = new Mock<IQuerySourceTracingExpressionVisitorFactory>();
            var entityResultFindingExpressionVisitorFactory = new Mock<IEntityResultFindingExpressionVisitorFactory>();
            var taskBlockingExpressionVisitor = new Mock<ITaskBlockingExpressionVisitor>();
            var memberAccessBindingExpressionVisitorFactory = new Mock<IMemberAccessBindingExpressionVisitorFactory>();
            var orderingExpressionVisitorFactory = new Mock<IOrderingExpressionVisitorFactory>();
            var projectionExpressionVisitorFactory = new Mock<IProjectionExpressionVisitorFactory>();
            var entityQueryableExpressionVisitorFactory = new Mock<IEntityQueryableExpressionVisitorFactory>();
            var queryAnnotationExtractor = new Mock<IQueryAnnotationExtractor>();
            var resultOperatorHandler = new Mock<IResultOperatorHandler>();
            var entityMaterializerSource = new Mock<IEntityMaterializerSource>();
            var expressionPrinter = new Mock<IExpressionPrinter>();
            var includeExpressionVisitorFactory = new Mock<IIncludeExpressionVisitorFactory>();
            var sqlTranslatingExpressionVisitorFactory = new Mock<ISqlTranslatingExpressionVisitorFactory>();
            var compositePredicateExpressionVisitorFactory = new Mock<ICompositePredicateExpressionVisitorFactory>();
            var conditionalRemovingExpressionVisitorFactory = new Mock<ConditionalRemovingExpressionVisitorFactory>();
            var queryFlattenerFactory = new Mock<IQueryFlattenerFactory>();
            var dbContextOptions = new Mock<IDbContextOptions>();



            var model = new Mock<IModel>();
            var logger = new Mock<ISensitiveDataLogger>();
            var entityQueryModelVisitorFactory = new Mock<IEntityQueryModelVisitorFactory>();
            var requiresMaterializationExpressionVisitorFactory = new Mock<IRequiresMaterializationExpressionVisitorFactory>();
            var linqOperatorProvider = new Mock<ILinqOperatorProvider>();
            var queryMethodProvider = new Mock<IQueryMethodProvider>();

            var queryCompilationContext = new RelationalQueryCompilationContext(
                model.Object,
                logger.Object,
                entityQueryModelVisitorFactory.Object,
                requiresMaterializationExpressionVisitorFactory.Object,
                linqOperatorProvider.Object,
                queryMethodProvider.Object,
                contextType: typeof(int),
                trackQueryResults: false);

            var relationalQueryModelVisitor = new RelationalQueryModelVisitor(
                queryOptimizer.Object,
                navigationRewritingExpressionVisitorFactory.Object,
                subQueryMemberPushDownExpressionVisitor.Object,
                querySourceTracingExpressionVisitorFactory.Object,
                entityResultFindingExpressionVisitorFactory.Object,
                taskBlockingExpressionVisitor.Object,
                memberAccessBindingExpressionVisitorFactory.Object,
                orderingExpressionVisitorFactory.Object,
                projectionExpressionVisitorFactory.Object,
                entityQueryableExpressionVisitorFactory.Object,
                queryAnnotationExtractor.Object,
                resultOperatorHandler.Object,
                entityMaterializerSource.Object,
                expressionPrinter.Object,
                relationalAnnotationProvider.Object,
                includeExpressionVisitorFactory.Object,
                sqlTranslatingExpressionVisitorFactory.Object,
                compositePredicateExpressionVisitorFactory.Object,
                conditionalRemovingExpressionVisitorFactory.Object,
                queryFlattenerFactory.Object,
                dbContextOptions.Object,
                queryCompilationContext,
                parentQueryModelVisitor: null);

            var sqlTranslator = new SqlTranslatingExpressionVisitor(
                relationalAnnotationProvider.Object,
                compositeExpressionFragmentTranslator.Object,
                methodCallTranslator.Object,
                memberTranslator.Object,
                relationalTypeMapper.Object,
                relationalQueryModelVisitor);

            var result = sqlTranslator.Visit(nullConditional2);

            Assert.Same(toLower, result);
        }
    }
}
