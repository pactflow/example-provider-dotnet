using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using PactNet;
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
                                    new XunitOutput(_outputHelper),
                                    new ConsoleOutput()
                                },

                // Output verbose verification logs to the test output
                LogLevel = PactLogLevel.Debug,
            };

            IPactVerifier pactVerifier = new PactVerifier(config);
            string pactUrl = System.Environment.GetEnvironmentVariable("PACT_URL");
            string pactFile = System.Environment.GetEnvironmentVariable("PACT_FILE");
            string providerName = !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_PROVIDER_NAME"))
                                    ? System.Environment.GetEnvironmentVariable("PACT_PROVIDER_NAME")
                                    : "pactflow-example-provider-dotnet";
            string version = Environment.GetEnvironmentVariable("GIT_COMMIT");
            string branch = Environment.GetEnvironmentVariable("GIT_BRANCH");
            string buildUri = $"{Environment.GetEnvironmentVariable("GITHUB_SERVER_URL")}/{Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")}/actions/runs/{Environment.GetEnvironmentVariable("GITHUB_RUN_ID")}";


            if (pactFile != "" && pactFile != null)
            // Verify a local file, provided by PACT_FILE, verification results are never published
            // This step does not require a Pact Broker
            {

                pactVerifier.ServiceProvider(providerName, new Uri(_providerUri))
                .WithFileSource(new FileInfo(pactUrl))
                .WithProviderStateUrl(new Uri($"{_pactServiceUri}/provider-states"))
                .Verify();
            }
            else if (pactUrl != "" && pactUrl != null)
            // Verify a remote file fetched from a pact broker, provided by PACT_URL, verification results may be published
            // This step requires a Pact Broker
            {
                pactVerifier.ServiceProvider(providerName, new Uri(_providerUri))
                .WithUriSource(new Uri(pactUrl), options =>
                {
                    if (!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")))
                    {
                        options.TokenAuthentication(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN"));
                    }
                    else if (!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME")))
                    {
                        options.BasicAuthentication(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME"), System.Environment.GetEnvironmentVariable("PACT_BROKER_PASSWORD"));
                    }
                    options.PublishResults(!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_PUBLISH_VERIFICATION_RESULTS")), version, results =>
                        {
                            results.ProviderBranch(branch)
                            .BuildUri(new Uri(buildUri));
                        });
                })
                .WithProviderStateUrl(new Uri($"{_pactServiceUri}/provider-states"))
                .Verify();
            }
            else
            {
                // Verify remote pacts, provided by querying the Pact Broker via consumer version selectors, verification results may be published
                // This step requires a Pact Broker
                pactVerifier.ServiceProvider(providerName, new Uri(_providerUri))
                    .WithPactBrokerSource(new Uri(System.Environment.GetEnvironmentVariable("PACT_BROKER_BASE_URL")), options =>
                    {
                        options.ConsumerVersionSelectors(
                                    new ConsumerVersionSelector { DeployedOrReleased = true },
                                    new ConsumerVersionSelector { MainBranch = true }
                                )
                                .ProviderBranch(branch)
                                .PublishResults(!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_PUBLISH_VERIFICATION_RESULTS")), version, results =>
                                {
                                    results.ProviderBranch(branch)
                                   .BuildUri(new Uri(buildUri));
                                })
                                .EnablePending()
                                .IncludeWipPactsSince(new DateTime(2022, 1, 1));
                        // Conditionally set authentication depending on if you are using an Pact Broker / PactFlow Broker
                        // You may not have credentials with your own broker.
                        if (!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN")))
                        {
                            options.TokenAuthentication(System.Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN"));
                        }
                        else if (!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME")))
                        {
                            options.BasicAuthentication(System.Environment.GetEnvironmentVariable("PACT_BROKER_USERNAME"), System.Environment.GetEnvironmentVariable("PACT_BROKER_PASSWORD"));
                        }

                    })
                    .WithProviderStateUrl(new Uri($"{_pactServiceUri}/provider-states"))
                    .Verify();
            }



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
