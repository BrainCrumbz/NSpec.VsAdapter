﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter
{
    public interface IDebugInfoProvider
    {
        DiaNavigationData GetNavigationData(string binaryPath, string declaringTypeName, string methodName);
    }
}
