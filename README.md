# Example Provider

[![Build Status](https://travis-ci.com/pactflow/pactflow-example-provider-dotnet.svg?branch=master)](https://travis-ci.com/pactflow/pactflow-example-provider-dotnet)

[![Pact Status](https://test.pactflow.io/pacts/provider/pactflow-example-provider-dotnet/consumer/pactflow-pactflow-example-provider-dotnet/latest/badge.svg?label=provider)](https://test.pactflow.io/pacts/provider/pactflow-example-provider-dotnet/consumer/pactflow-pactflow-example-provider-dotnet/latest) (latest pact)

[![Pact Status](https://test.pactflow.io/matrix/provider/pactflow-example-provider-dotnet/latest/prod/consumer/pactflow-pactflow-example-provider-dotnet/latest/prod/badge.svg?label=provider)](https://test.pactflow.io/pacts/provider/pactflow-example-provider-dotnet/consumer/pactflow-pactflow-example-provider-dotnet/latest/prod) (prod/prod pact)

This is an example of a Node provider that uses Pact, [Pactflow](https://pactflow.io) and Travis CI to ensure that it is compatible with the expectations its consumers have of it.

The project uses a Makefile to simulate a very simple build pipeline with two stages - test and deploy.

It is using a public tenant on Pactflow, which you can access [here](https://test.pact.dius.com.au) using the credentials `dXfltyFMgNOFZAxr8io9wJ37iUpY42M`/`O5AIZWxelWbLvqMd8PkAVycBJh2Psyg1`. The latest version of the Example Consumer/Example Provider pact is published [here](https://test.pact.dius.com.au/pacts/provider/pactflow-example-provider/consumer/pactflow-example-provider/latest).

## Pact verifications

When using Pact in a CI/CD pipeline, there are two reasons for a pact verification task to take place:

   * When the provider changes (to make sure it does not break any existing consumer expectations)
   * When a pact changes (to see if the provider is compatible with the new expectations)

When the provider changes, the pact verification task runs as part the provider's normal build pipeline, generally after the unit tests, and before any deployment takes place. This pact verification task is configured to dynamically fetch all the relevant pacts for the specified provider from Pactflow, verify them, and publish the results back to Pactflow.

To ensure that a verification is also run whenever a pact changes, we create a webhook in Pactflow that triggers a provider build, and passes in the URL of the changed pact. Ideally, this would be a completely separate build from your normal provider pipeline, and it should just verify the changed pact.

Because Travis CI only allows us to have one build configuration per repository, we switch between the main pipeline mode and the webhook-triggered mode based on the presence of an environment variable that is only set via the webhook. Keep in mind that this is just a constraint of the tools we're using for this example, and is not necessarily the way you would implement Pact your own pipeline.

## Project Phases

The project uses a Makefile to simulate a very simple build pipeline with two stages - test and deploy.

* Test
  * Run tests (including the pact tests that generate the contract)
  * Publish pacts, tagging the consumer version with the name of the current branch
  * Check if we are safe to deploy to prod (ie. has the pact content been successfully verified)
* Deploy (only from master)
  * Deploy app (just pretend for the purposes of this example!)
  * Tag the deployed consumer version as 'prod'

## Usage

See the [Pactflow CI/CD Workshop](https://github.com/pactflow/ci-cd-workshop).

The below commands are designed for a Linux/OSX environment, please translate for use on Windows/PowerShell as necessary:

### Run tests

1. Sign up to a Pactflow account at https://pactflow.io/register.
1. Grab a **read/write** API token from the "Settings > API Tokens" page
1. Export the following into your shell before running
    ```
    export PACT_BROKER_TOKEN=<insert token here> # do not a user/email/password here e.g. it should like something like 8Axvelr123xCq98h1
    export PACT_BROKER_HOST=<insert host here> # e.g. https://foo.pactflow.io
    ```
    ```
    make fake_ci
    ```

`make test`