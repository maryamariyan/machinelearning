﻿// Licensed to the .NET Foundation under one or more agreements.\r
// The .NET Foundation licenses this file to you under the MIT license.\r
// See the LICENSE file in the project root for more information.

using System.IO;

namespace Microsoft.DotNet.Configurer
{
    public static class ToolPackageFolderPathCalculator
    {
        private const string NestedToolPackageFolderName = ".store";
        public static string GetToolPackageFolderPath(string toolsShimPath)
        {
            return Path.Combine(toolsShimPath, NestedToolPackageFolderName);
        }
    }
}
