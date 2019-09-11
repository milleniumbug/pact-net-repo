namespace PactNet
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Mocks.MockHttpService;
    using Mocks.MockHttpService.Host;
    using Mocks.MockHttpService.Models;
    using Models;
    using Newtonsoft.Json;
    using static System.String;

    public class PactBuilder : IPactBuilder
    {
        public string ConsumerName { get; private set; }
        public string ProviderName { get; private set; }

        private readonly
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, IMockProviderService>
            _mockProviderServiceFactory;

        private IMockProviderService _mockProviderService;

        internal PactBuilder(
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, IMockProviderService>
                mockProviderServiceFactory)
        {
            _mockProviderServiceFactory = mockProviderServiceFactory;
        }

        public PactBuilder()
            : this(new PactConfig())
        {
        }

        public PactBuilder(PactConfig config)
            : this((port, enableSsl, consumerName, providerName, host, jsonSerializerSettings, sslCert, sslKey) =>
                new MockProviderServiceRust(port, enableSsl, consumerName, providerName, config, host,
                    jsonSerializerSettings, sslCert, sslKey))
        {
        }

        public IPactBuilder ServiceConsumer(string consumerName)
        {
            if (String.IsNullOrEmpty(consumerName))
            {
                throw new ArgumentException("Please supply a non null or empty consumerName");
            }

            ConsumerName = consumerName;

            return this;
        }

        public IPactBuilder HasPactWith(string providerName)
        {
            if (String.IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("Please supply a non null or empty providerName");
            }

            ProviderName = providerName;

            return this;
        }

        public IMockProviderService MockService(
            int port,
            bool enableSsl = false,
            IPAddress host = IPAddress.Loopback,
            string sslCert = null,
            string sslKey = null)
        {
            return MockService(port, jsonSerializerSettings: null, enableSsl: enableSsl, host: host, sslCert: sslCert, sslKey: sslKey);
        }

        public IMockProviderService MockService(
            int port,
            JsonSerializerSettings jsonSerializerSettings,
            bool enableSsl = false,
            IPAddress host = IPAddress.Loopback,
            string sslCert = null,
            string sslKey = null)
        {
            if (String.IsNullOrEmpty(ConsumerName))
            {
                throw new InvalidOperationException(
                    "ConsumerName has not been set, please supply a consumer name using the ServiceConsumer method.");
            }

            if (String.IsNullOrEmpty(ProviderName))
            {
                throw new InvalidOperationException(
                    "ProviderName has not been set, please supply a provider name using the HasPactWith method.");
            }

            if (_mockProviderService != null)
            {
                _mockProviderService.Stop();
            }

            _mockProviderService = _mockProviderServiceFactory(port, enableSsl, ConsumerName, ProviderName, host,
                jsonSerializerSettings, sslCert, sslKey);

            _mockProviderService.Start();

            return _mockProviderService;
        }

        public void Build()
        {
            if (_mockProviderService == null)
            {
                throw new InvalidOperationException(
                    "The Pact file could not be saved because the mock provider service is not initialised. Please initialise by calling the MockService() method.");
            }

            PersistPactFile();
            _mockProviderService.Stop();
        }

        private void PersistPactFile()
        {
            // TODO
            throw new NotImplementedException();
        }
    }

    public class MockProviderServiceRust : IMockProviderService
    {
        private readonly Func<Uri, IHttpHost> _hostFactory;
        private IHttpHost _host;
        private readonly AdminHttpClient _adminHttpClient;

        private string _providerState;
        private string _description;
        private ProviderServiceRequest _request;
        private ProviderServiceResponse _response;

        public Uri BaseUri { get; }

        internal MockProviderServiceRust(
            Func<Uri, IHttpHost> hostFactory,
            int port,
            bool enableSsl,
            Func<Uri, AdminHttpClient> adminHttpClientFactory)
        {
            _hostFactory = hostFactory;
            BaseUri = new Uri($"{(enableSsl ? "https" : "http")}://localhost:{port}");
            _adminHttpClient = adminHttpClientFactory(BaseUri);
        }

        public MockProviderServiceRust(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress)
            : this(port, enableSsl, consumerName, providerName, config, ipAddress, null, null, null)
        {
        }

        public MockProviderServiceRust(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings)
            : this(port, enableSsl, consumerName, providerName, config, ipAddress, jsonSerializerSettings, null, null)
        {
        }

        public MockProviderServiceRust(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings, string sslCert, string sslKey)
            : this(
                baseUri => new RubyHttpHostOld(baseUri, consumerName, providerName, config, ipAddress, sslCert, sslKey),
                port,
                enableSsl,
                baseUri => new AdminHttpClient(baseUri, jsonSerializerSettings))
        {
        }

        public IMockProviderService Given(string providerState)
        {
            if (IsNullOrEmpty(providerState))
            {
                throw new ArgumentException("Please supply a non null or empty providerState");
            }

            _providerState = providerState;

            return this;
        }

        public IMockProviderService UponReceiving(string description)
        {
            if (IsNullOrEmpty(description))
            {
                throw new ArgumentException("Please supply a non null or empty description");
            }

            _description = description;

            return this;
        }

        public IMockProviderService With(ProviderServiceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Please supply a non null request");
            }

            if (request.Method == HttpVerb.NotSet)
            {
                throw new ArgumentException("Please supply a request Method");
            }

            if (!IsContentTypeSpecifiedForBody(request))
            {
                throw new ArgumentException("Please supply a Content-Type request header");
            }

            _request = request;

            return this;
        }

        public void WillRespondWith(ProviderServiceResponse response)
        {
            if (response == null)
            {
                throw new ArgumentException("Please supply a non null response");
            }

            if (response.Status <= 0)
            {
                throw new ArgumentException("Please supply a response Status");
            }

            if (!IsContentTypeSpecifiedForBody(response))
            {
                throw new ArgumentException("Please supply a Content-Type response header");
            }

            _response = response;

            RegisterInteraction();
        }

        public void VerifyInteractions()
        {
            // TODO
            throw new NotImplementedException();
        }

        public void SendAdminHttpRequest(HttpVerb method, string path)
        {
            // not applicable
            throw new NotImplementedException();
        }

        public void Start()
        {
            StopRunningHost();

            _host = _hostFactory(BaseUri);
            _host.Start();
        }

        public void Stop()
        {
            ClearAllState();
            StopRunningHost();
        }

        public void ClearInteractions()
        {
            if (_host != null)
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        private void RegisterInteraction()
        {
            if (IsNullOrEmpty(_description))
            {
                throw new InvalidOperationException("description has not been set, please supply using the UponReceiving method.");
            }

            if (_request == null)
            {
                throw new InvalidOperationException("request has not been set, please supply using the With method.");
            }

            if (_response == null)
            {
                throw new InvalidOperationException("response has not been set, please supply using the WillRespondWith method.");
            }

            var interaction = new ProviderServiceInteraction
            {
                ProviderState = _providerState,
                Description = _description,
                Request = _request,
                Response = _response
            };

            // TODO
            throw new NotImplementedException();

            ClearTrasientState();
        }

        private void StopRunningHost()
        {
            if (_host != null)
            {
                _host.Stop();
                _host = null;
            }
        }

        private void ClearAllState()
        {
            ClearTrasientState();
            ClearInteractions();
        }

        private void ClearTrasientState()
        {
            _request = null;
            _response = null;
            _providerState = null;
            _description = null;
        }

        private bool IsContentTypeSpecifiedForBody(IHttpMessage message)
        {
            //No content-type required if there is no body
            if (message.Body == null)
            {
                return true;
            }

            IDictionary<string, object> headers = null;
            if (message.Headers != null)
            {
                headers = new Dictionary<string, object>(message.Headers, StringComparer.OrdinalIgnoreCase);
            }

            return headers != null && headers.ContainsKey("Content-Type");
        }
    }
}