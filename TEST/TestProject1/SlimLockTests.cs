using System;

namespace DAVID.Test.Synchronization;
[TestClass]
public class SlimLockTests
{
    [TestMethod]
    public void SigleSlimLockTest()
    {
        using (var slimLock = new DAVID.Synchronization.SlimLock().Lock())
        {

        }
    }
}
