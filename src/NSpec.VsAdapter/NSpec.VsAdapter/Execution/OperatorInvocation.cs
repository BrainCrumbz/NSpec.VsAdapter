﻿using NSpec.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSpec.VsAdapter.Execution
{
    public class OperatorInvocation : IOperatorInvocation
    {
        public OperatorInvocation(string assemblyPath, 
            IExecutionObserver executionObserver, ISerializableLogger logger)
            : this(assemblyPath, runAll, executionObserver, logger)
        {
        }

        public OperatorInvocation(string assemblyPath, string[] exampleFullNames, 
            IExecutionObserver executionObserver, ISerializableLogger logger)
        {
            this.assemblyPath = assemblyPath;
            this.exampleFullNames = exampleFullNames;
            this.executionObserver = executionObserver;
            this.logger = logger;
        }

        public int Operate()
        {
            logger.Debug(String.Format("Start operating tests in '{0}'", assemblyPath));

            var contexts = BuildContexts(assemblyPath);

            IEnumerable<ExampleBase> ranExamples;

            if (exampleFullNames == runAll)
            {
                contexts.Run(executionObserver, false);

                ranExamples = contexts.SelectMany(ctx => ctx.Examples);
            }
            else
            {
                // original idea taken from https://github.com/osoftware/NSpecTestAdapter/blob/master/NSpec.TestAdapter/Executor.cs

                var allExamples = contexts.SelectMany(ctx => ctx.Examples);

                var selectedNames = new HashSet<string>(exampleFullNames);

                var selectedExamples = allExamples.Where(exm => selectedNames.Contains(exm.FullName()));

                var selectedContexts = selectedExamples.Select(exm => exm.Context);

                foreach (var context in selectedContexts)
                {
                    context.Run(executionObserver, false);
                }

                ranExamples = selectedContexts.SelectMany(ctx => ctx.Examples);
            }

            int count = ranExamples.Count();

            logger.Debug(String.Format("Finish operating tests in '{0}'", assemblyPath));

            logger.Flush();

            return count;
        }

        static ContextCollection BuildContexts(string assemblyPath)
        {
            // TODO this is common to XxxInvocation: extract it, maybe in a base class or dependency

            var reflector = new Reflector(assemblyPath);

            var finder = new SpecFinder(reflector);

            var conventions = new DefaultConventions();

            var contextBuilder = new ContextBuilder(finder, conventions);

            var contexts = contextBuilder.Contexts();

            contexts.Build();

            return contexts;
        }

        readonly string assemblyPath;
        readonly string[] exampleFullNames;
        readonly IExecutionObserver executionObserver;
        readonly ISerializableLogger logger;

        static readonly string[] runAll = null;
    }
}