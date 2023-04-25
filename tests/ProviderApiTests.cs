using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using tests.XUnitHelpers;
using Xunit;
using Xunit.Abstractions;

namespace tests
{
    public class ProviderApiTests : IDisposable
    {
        private string _providerUri { get; }
        private string _pactServiceUri { get; }
        private IWebHost _webHost { get; }
        private ITestOutputHelper _outputHelper { get; }

        public ProviderApiTests(ITestOutputHelper output)
        {
            _outputHelper = output;
            _providerUri = "http://localhost:9000";
            _pactServiceUri = "http://localhost:9001";

            _webHost = WebHost.CreateDefaultBuilder()
                .UseUrls(_pactServiceUri)
                .UseStartup<TestStartup>()
                .Build();

            _webHost.Start();
        }

        [Fact]
        public void EnsureProviderApiHonoursPactWithConsumer()
        {
            // Arrange
            var config = new PactVerifierConfig
            {

                // NOTE: We default to using a ConsoleOutput,
                // however xUnit 2 does not capture the console output,
                // so a custom outputter is required.
                Outputters = new List<IOutput>
                                {
                                    new ConsoleOutput()
                                },

                // Output verbose verification logs to the test output
                Verbose = true,
                PublishVerificationResults = !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_PUBLISH_VERIFICATION_RESULTS")) ? true : false,
                ProviderVersion = System.Environment.GetEnvironmentVariable("GIT_COMMIT"),
            };

            IPactVerifier pactVerifier = new PactVerifier(config);
            string pactUrl = System.Environment.GetEnvironmentVariable("PACT_URL");
            pactVerifier
                .ProviderState($"{_pactServiceUri}/provider-states")
                .ServiceProvider("pactflow-example-provider-dotnet", _providerUri);

            if (pactUrl != "" && pactUrl != null)
            {
                // Webhook path - verify the specific pact
                pactVerifier.PactUri(pactUrl, !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")) ? 
                                        new PactUriOptions(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")) : 
                                            !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME")) ?
                                                new PactUriOptions(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME"), 
                                                    System.Environment.GetEnvironmentVariable("PACT_BROKER_PASSWORD")) : null);
            }
            else
            {
                // Standard verification path - run the
                pactVerifier.PactBroker(System.Environment.GetEnvironmentVariable("PACT_BROKER_BASE_URL"),
                    uriOptions: !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")) ? 
                                        new PactUriOptions(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")) : 
                                            !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME")) ?
                                                new PactUriOptions(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME"), 
                                                    System.Environment.GetEnvironmentVariable("PACT_BROKER_PASSWORD")) : null,
                    consumerVersionTags: new List<string> { "master", "prod" });
            }

            // Act / Assert
            pactVerifier.Verify();

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _webHost.StopAsync().GetAwaiter().GetResult();
                    _webHost.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
