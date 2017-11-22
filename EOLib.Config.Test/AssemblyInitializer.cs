// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using NUnit.Framework;
using System.IO;

[SetUpFixture]
public class AssemblyInitializer
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
    }
}
