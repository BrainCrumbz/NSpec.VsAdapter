﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NSpec.VsAdapter.Core.Execution;
using NSpec.VsAdapter.Logging;
using NSpec.VsAdapter.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSpec.VsAdapter.TestAdapter.Execution
{
    public class MultiSourceTestExecutor : IMultiSourceTestExecutor
    {
        public MultiSourceTestExecutor(IEnumerable<string> sources,
            IBinaryTestExecutor binaryTestExecutor, 
            IProgressRecorderFactory progressRecorderFactory,
            ISettingsRepository settingsRepository,
            ILoggerFactory loggerFactory)
            : this(binaryTestExecutor, progressRecorderFactory, settingsRepository, loggerFactory)
        {
            this.testableItems = sources.Select(source => new SourceTestableItem(source));

            this.sourceDescription = "source paths";
        }

        public MultiSourceTestExecutor(IEnumerable<TestCase> tests,
            IBinaryTestExecutor binaryTestExecutor, 
            IProgressRecorderFactory progressRecorderFactory,
            ISettingsRepository settingsRepository,
            ILoggerFactory loggerFactory)
            : this(binaryTestExecutor, progressRecorderFactory, settingsRepository, loggerFactory)
        {
            var testCaseGroupsBySource = tests.GroupBy(t => t.Source);

            this.testableItems = testCaseGroupsBySource.Select(group => new TestCaseGroupTestableItem(group));

            this.sourceDescription = "TestCases";
        }

        MultiSourceTestExecutor(
            IBinaryTestExecutor binaryTestExecutor, 
            IProgressRecorderFactory progressRecorderFactory,
            ISettingsRepository settingsRepository,
            ILoggerFactory loggerFactory)
        {
            this.binaryTestExecutor = binaryTestExecutor;
            this.progressRecorderFactory = progressRecorderFactory;
            this.settingsRepository = settingsRepository;
            this.loggerFactory = loggerFactory;

            this.isCanceled = false;
        }

        public void RunTests(IFrameworkHandle frameworkHandle, IRunContext runContext)
        {
            var settings = settingsRepository.Load(runContext);

            var outputLogger = loggerFactory.CreateOutput(frameworkHandle, settings);

            outputLogger.Info(String.Format("Execution by {0} started", sourceDescription));

            isCanceled = false;

            using (var progressRecorder = progressRecorderFactory.Create((ITestExecutionRecorder)frameworkHandle))
            using (var crossDomainLogger = new CrossDomainLogger(outputLogger))
            {
                foreach (var item in testableItems)
                {
                    if (isCanceled)
                    {
                        break;
                    }

                    progressRecorder.BinaryPath = item.BinaryPath;

                    // TODO pass canceler to ITestableItem.Execute

                    item.Execute(binaryTestExecutor, progressRecorder, outputLogger, crossDomainLogger);
                }
            }

            outputLogger.Info(String.Format("Execution by {0} finished", sourceDescription));
        }

        public void CancelRun()
        {
            isCanceled = true;
        }

        bool isCanceled;

        readonly IEnumerable<ITestableItem> testableItems;
        readonly string sourceDescription;
        readonly IBinaryTestExecutor binaryTestExecutor;
        readonly IProgressRecorderFactory progressRecorderFactory;
        readonly ISettingsRepository settingsRepository;
        readonly ILoggerFactory loggerFactory;

        interface ITestableItem
        {
            string BinaryPath { get; }

            void Execute(
                IBinaryTestExecutor binaryTestExecutor,
                IProgressRecorder progressRecorder,
                IOutputLogger outputLogger, ICrossDomainLogger crossDomainLogger);
        }

        class SourceTestableItem : ITestableItem
        {
            public SourceTestableItem(string source)
            {
                this.source = source;
            }

            public string BinaryPath { get { return source; } }

            public void Execute(
                IBinaryTestExecutor binaryTestExecutor,
                IProgressRecorder progressRecorder,
                IOutputLogger outputLogger, ICrossDomainLogger crossDomainLogger)
            {
                // TODO pass canceler to binaryTestExecutor.Execute

                binaryTestExecutor.ExecuteAll(BinaryPath, progressRecorder, outputLogger, crossDomainLogger);
            }

            readonly string source;
        }

        class TestCaseGroupTestableItem : ITestableItem
        {
            public TestCaseGroupTestableItem(IGrouping<string, TestCase> testCaseGroup)
            {
                this.testCaseGroup = testCaseGroup;
            }

            public string BinaryPath { get { return testCaseGroup.Key; } }

            public void Execute(
                IBinaryTestExecutor binaryTestExecutor,
                IProgressRecorder progressRecorder,
                IOutputLogger outputLogger, ICrossDomainLogger crossDomainLogger)
            {
                var testCaseFullNames = testCaseGroup.Select(tc => tc.FullyQualifiedName);

                // TODO pass canceler to binaryTestExecutor.Execute

                binaryTestExecutor.ExecuteSelected(BinaryPath, testCaseFullNames, progressRecorder, outputLogger, crossDomainLogger);
            }

            readonly IGrouping<string, TestCase> testCaseGroup;
        }
    }
}
