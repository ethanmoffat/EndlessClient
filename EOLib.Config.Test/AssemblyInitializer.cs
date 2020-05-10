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
