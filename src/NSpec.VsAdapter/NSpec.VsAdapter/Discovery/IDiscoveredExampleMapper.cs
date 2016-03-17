﻿using NSpec.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSpec.VsAdapter.Discovery
{
    public interface IDiscoveredExampleMapper
    {
        DiscoveredExample FromExample(ExampleBase example);
    }
}
