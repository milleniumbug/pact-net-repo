﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService.Models
{
    public class ProviderServiceResponseTests
    {
        [Fact]
        public void SerializeObject_WithDefaultApiSerializerSettings_ReturnsCorrectJson()
        {
            var request = new ProviderServiceResponse
            {
                Status = 200,
                Headers = new Dictionary<string, object> { { "Content-Type", "application/json" } },
                Body = new
                {
                    Test1 = "hi",
                    test2 = 2,
                    test3 = (string)null
                }
            };

            var responseJson = JsonConvert.SerializeObject(request, JsonConfig.ApiSerializerSettings);
            var expectedJson = "{\"status\":200,\"headers\":{\"Content-Type\":\"application/json\"},\"body\":{\"Test1\":\"hi\",\"test2\":2,\"test3\":null}}";
            Assert.Equal(expectedJson, responseJson);
        }

        [Fact]
        public void SerializeObject_WithDefaultApiSerializerSettingsAndNoHeadersOrBody_ReturnsCorrectJson()
        {
            var response = new ProviderServiceResponse
            {
                Status = 500
            };

            var responseJson = JsonConvert.SerializeObject(response, JsonConfig.ApiSerializerSettings);
            var expectedJson = "{\"status\":500}";
            Assert.Equal(expectedJson, responseJson);
        }

        [Fact]
        public void SerializeObject_WithCamelCaseApiSerializerSettings_ReturnsCorrectJson()
        {
            var response = new ProviderServiceResponse
            {
                Status = 200,
                Headers = new Dictionary<string, object> { { "Content-Type", "application/json" } },
                Body = new
                {
                    Test1 = "hi",
                    test2 = 2
                }
            };

            var responseJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var expectedJson = "{\"status\":200,\"headers\":{\"Content-Type\":\"application/json\"},\"body\":{\"test1\":\"hi\",\"test2\":2}}";
            Assert.Equal(expectedJson, responseJson);
        }
    }
}