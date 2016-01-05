﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NSpec.VsAdapter.NSpecModding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.TestAdapter
{
    [FileExtension(Constants.DllExtension)]
    [FileExtension(Constants.ExeExtension)]
    [DefaultExecutorUri(Constants.ExecutorUriString)]
    public class NSpecTestDiscoverer : ITestDiscoverer
    {
        // used by Visual Studio test infrastructure
        public NSpecTestDiscoverer() 
        {
            var crossDomainRunner = new CrossDomainRunner();

            this.crossDomainTestDiscoverer = new CrossDomainTestDiscoverer(crossDomainRunner);
        }

        // used to test this adapter
        public NSpecTestDiscoverer(ICrossDomainTestDiscoverer crossDomainTestDiscoverer)
        {
            this.crossDomainTestDiscoverer = crossDomainTestDiscoverer;
        }

        public void DiscoverTests(
            IEnumerable<string> sources, 
            IDiscoveryContext discoveryContext, 
            IMessageLogger logger, 
            ITestCaseDiscoverySink discoverySink)
        {
            // TODO implement custom runtime TestSettings, e.g. to enable debug logging
            // E.g. as https://github.com/mmanela/chutzpah/blob/master/VS2012.TestAdapter/ChutzpahTestDiscoverer.cs

            logger.SendMessage(TestMessageLevel.Informational, "Discovery started");

            var specificationGroups =
                from assemblyPath in sources
                select crossDomainTestDiscoverer.Discover(assemblyPath, logger);

            var specifications = specificationGroups.SelectMany(group => group);

            var testCases =
                from spec in specifications
                select new TestCase(spec.FullName, Constants.ExecutorUri, spec.SourceFilePath);

            testCases.Do(discoverySink.SendTestCase);

            logger.SendMessage(TestMessageLevel.Informational, "Discovery finished");
        }

        readonly ICrossDomainTestDiscoverer crossDomainTestDiscoverer;
    }
}