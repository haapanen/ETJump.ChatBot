using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ETJump.Communication.Tests
{
    public class ClientTest
    {
        [Fact]
        public void ClientConstructor_ThrowsSocketExceptionIfTargetCouldNotBeFound()
        {
            Assert.Throws<SocketException>(() =>
            {
                var c = new Client("foobar.localhost.nosuchdomain", 27227, "123");
            });
        }

        [Fact]
        public void ClientConstructor_DoesntThrowIfServerIsOffline()
        {
            var client = new Client("localhost", 27960, "none");
        }

        [Fact]
        public async Task GetStatusAsync_ReturnsStatusWithSettings()
        {
            var client = new Client("trickjump.net", 27960, "");

            var response = await client.GetStatusAsync(TimeSpan.FromSeconds(10));
            Assert.True(!string.IsNullOrEmpty(response.Hostname));
        }

        [Fact]
        public async Task GetStatusAsync_ThrowsTimeoutExceptionIfServerDoesNotRespond()
        {
            var client = new Client("trickjump.net", 1, "");

            await Assert.ThrowsAsync<TimeoutException>(async () => { await client.GetStatusAsync(TimeSpan.Zero); });
        }
    }
}
