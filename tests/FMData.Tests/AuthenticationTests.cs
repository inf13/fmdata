using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FMData.Responses;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void NewUp_DataClient_ShouldBeAuthenticated()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            Assert.True(fdc.IsAuthenticated);
        }

        [Fact]
        public async Task RefreshToken_ShouldGet_NewToken()
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "layout";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, user, pass, layout);

            var response = await fdc.RefreshTokenAsync("integration", "test", "someLayout");
            Assert.Equal("someOtherToken", response.Token);
        }

        [Theory]
        [InlineData("", "test", "layout")]
        [InlineData("integration", "", "layout")]
        [InlineData("integration", "test", "")]
        public async Task RefreshToken_Requires_AllParameters(string user, string pass, string layout)
        {
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";

            mockHttp.When($"{server}/fmi/rest/api/auth/{file}")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication("someOtherToken"));

            // pass in actual values here since we DON'T want this to blow up on constructor 
            var fdc = new FMDataClient(mockHttp.ToHttpClient(), server, file, "user", "pass", "layout");

            await Assert.ThrowsAsync<ArgumentException>(async () => await fdc.RefreshTokenAsync(user, pass, layout));
        }
    }
}