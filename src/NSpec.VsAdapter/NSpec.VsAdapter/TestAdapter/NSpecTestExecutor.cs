﻿using Autofac;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.TestAdapter
{
    [ExtensionUri(Constants.ExecutorUriString)]
    public class NSpecTestExecutor : ITestExecutor, IDisposable
    {
        // used by Visual Studio test infrastructure, by integration tests
        public NSpecTestExecutor()
        {
            var scope = DIContainer.Instance.BeginScope();

            disposable = scope;

            this.multiSourceTestExecutorFactory = scope.Resolve<IMultiSourceTestExecutorFactory>();
        }

        // used by unit tests
        public NSpecTestExecutor(IMultiSourceTestExecutorFactory multiSourceTestExecutorFactory)
        {
            this.multiSourceTestExecutorFactory = multiSourceTestExecutorFactory;

            disposable = Disposable.Empty;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            multiSourceTestExecutor = multiSourceTestExecutorFactory.Create(sources);

            multiSourceTestExecutor.RunTests(frameworkHandle);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            multiSourceTestExecutor = multiSourceTestExecutorFactory.Create(tests);

            multiSourceTestExecutor.RunTests(frameworkHandle);
        }

        public void Cancel()
        {
            if (multiSourceTestExecutor != null)
            {
                multiSourceTestExecutor.CancelRun();
            }
        }

        IMultiSourceTestExecutor multiSourceTestExecutor;

        readonly IMultiSourceTestExecutorFactory multiSourceTestExecutorFactory;
        readonly IDisposable disposable;
    }
}
