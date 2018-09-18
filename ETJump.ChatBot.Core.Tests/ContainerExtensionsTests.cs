using System;
using System.Collections.Generic;
using ETJump.ChatBot.Core.Extensions;
using Xunit;

namespace ETJump.ChatBot.Core.Tests
{
    public class ContainerExtensionsTests
    {
        [Fact]
        public void GetOrDefault_ReturnsValueIfItExists()
        {
            var dictionary = new Dictionary<string, int>
            {
                {"val", 1}
            };

            Assert.Equal(1, dictionary.GetOrDefault("val"));
        }

        [Fact]
        public void GetOrDefault_ReturnsDefaultIfKeyDoesntExist()
        {
            var dictionary = new Dictionary<string, int>
            {
                {"val", 1}
            };

            Assert.Equal(default(int), dictionary.GetOrDefault("val2"));
        }
    }

}
