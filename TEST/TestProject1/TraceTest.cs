using System;

namespace DAVID.Test.Diagnostics;

[TestClass]
public class SHGetDiskFreeSpaceEx_Tests
{
    [TestMethod]
    public void SHGetDiskFreeSpaceEx_Test()
    {
        var freeSpace = new DriveInfo(AppDomain.CurrentDomain.BaseDirectory).TotalFreeSpace;
        Console.WriteLine(freeSpace);
        Console.WriteLine(freeSpace/1024/1024/1024);
    }
}
