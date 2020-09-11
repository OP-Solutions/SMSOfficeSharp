using System;
using SMSOfficeSharp;
using Xunit;
namespace SMSOfficeSharpTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var s = new Sender {ApiKey = "api key here"};

            s.Send("GG", "995591915519");
        }
    }
}