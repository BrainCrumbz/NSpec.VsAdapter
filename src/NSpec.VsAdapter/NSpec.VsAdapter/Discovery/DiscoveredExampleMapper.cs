﻿using NSpec.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NSpec.VsAdapter.Discovery
{
    public class DiscoveredExampleMapper
    {
        public DiscoveredExampleMapper(string binaryPath, IDebugInfoProvider debugInfoProvider)
        {
            this.binaryPath = binaryPath;
            this.debugInfoProvider = debugInfoProvider;
        }

        public DiscoveredExample FromExample(ExampleBase example)
        {
            var methodInfo = ReflectExampleMethod(example);

            string specClassName = methodInfo.DeclaringType.FullName;
            string exampleMethodName = methodInfo.Name;

            var navigationData = debugInfoProvider.GetNavigationData(specClassName, exampleMethodName);

            var discoveredExample = new DiscoveredExample()
            {
                FullName = example.FullName(),
                SourceAssembly = binaryPath,
                SourceFilePath = navigationData.FileName,
                SourceLineNumber = navigationData.MinLineNumber,
                Tags = example.Tags.Select(tag => tag.Replace("_", " ")).ToArray(),
            };

            return discoveredExample;
        }

        readonly string binaryPath;
        readonly IDebugInfoProvider debugInfoProvider;

        // taken from https://github.com/BrainCrumbz/NSpecTestAdapter/blob/master/NSpec.TestAdapter/Discoverer.cs

        static MethodInfo ReflectExampleMethod(ExampleBase example)
        {
            const string methodPrivateFieldName = "method";
            const string actionPrivateFieldName = "action";

            Type exampleType = example.GetType();

            MethodInfo info;

            if (example is MethodExample)
            {
                info = exampleType
                    .GetField(methodPrivateFieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(example) as MethodInfo;
            }
            else if (example is Example)
            {
                var action = exampleType
                    .GetField(actionPrivateFieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(example) as Action;

                info = action.Method;
            }
            else
            {
                throw new ArgumentOutOfRangeException("example", String.Format("Unexpected example type: {0}", exampleType));
            }

            return info;
        }
    }
}
