using Core.Helpers;
using Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Test
{
    [TestClass]
    public class UnitTest1
    {
        private static EventWaitHandle quitOK = new EventWaitHandle(false, EventResetMode.AutoReset, "quitTrigger");

        [TestMethod]
        public void TestMethod1()
        {
            //WebRequestHelper.Get("https://google.com", TestGetCallback);
            WebRequestHelper.Post<PostTestModel>("https://localhost:5001/jank", new PostTestModel() , TestPostCallback);
            quitOK.WaitOne();
        }

        private void TestGetCallback(string data)
        {
            Debug.Print(data);
        }

        private void TestPostCallback(string data)
        {
            Debug.Print(data);
            quitOK.Set();
        }
    }
}
