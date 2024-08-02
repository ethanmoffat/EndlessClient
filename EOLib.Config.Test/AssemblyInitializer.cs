using System.IO;
using NUnit.Framework;

[SetUpFixture]
public class AssemblyInitializer
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
    }
}