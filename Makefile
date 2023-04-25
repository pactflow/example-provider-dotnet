PACTICIPANT := "pactflow-example-provider-dotnet"
GITHUB_REPO := "pactflow/pactflow-example-provider-dotnet"
PACT_CHANGED_WEBHOOK_UUID := "46ed3f10-d03f-43cd-b945-ce45ff42d324"
TRIGGER_PROVIDER_BUILD_URL := "https://api.travis-ci.com/repo/pactflow%2Fexample-provider-dotnet/requests"
PACT_CLI="docker run --rm -v ${PWD}:${PWD} -e PACT_BROKER_BASE_URL -e PACT_BROKER_TOKEN pactfoundation/pact-cli:latest"

# Only deploy from master
ifeq ($(GIT_BRANCH),master)
	DEPLOY_TARGET=deploy
else
	DEPLOY_TARGET=no_deploy
endif

all: test

## ====================
## CI tasks 
## ====================

restore:
	dotnet restore src
	dotnet restore tests

run:
	cd src && dotnet run

ci: run_tests can_i_deploy $(DEPLOY_TARGET)

start: server.PID

server.PID:
	{ dotnet run --project src & echo $$! > $@; }

stop: server.PID
	kill `cat $<` && rm $<

# Run the ci target from a developer machine with the environment variables
# set as if it was on Travis CI.
# Use this for quick feedback when playing around with your workflows.
fake_ci: .env
	CI=true \
	GIT_COMMIT=`git rev-parse --short HEAD`+`date +%s` \
	GIT_BRANCH=`git rev-parse --abbrev-ref HEAD` \
	make ci

ci_webhook: run_tests

## =====================
## Build/test tasks
## =====================

test: .env
	GIT_COMMIT=`git rev-parse --short HEAD`+`date +%s` \
	GIT_BRANCH=`git rev-parse --abbrev-ref HEAD` \
	dotnet test tests

run_tests: restore start test stop 

## =====================
## Deploy tasks
## =====================

deploy: deploy_app record_deployment

no_deploy:
	@echo "Not deploying as not on master branch"

can_i_deploy: .env
	@"${PACT_CLI}" broker can-i-deploy --pacticipant ${PACTICIPANT} --version ${GIT_COMMIT} --to-environment prod

deploy_app:
	@echo "Deploying to prod"

record_deployment: .env
	@"${PACT_CLI}" broker record_deployment --pacticipant ${PACTICIPANT} --version ${GIT_COMMIT} --environment production

## =====================
## PactFlow set up tasks
## =====================

# export the GITHUB_TOKEN environment variable before running this
create_github_token_secret:
	curl -v -X POST ${PACT_BROKER_BASE_URL}/secrets \
	-H "Authorization: Bearer ${PACT_BROKER_TOKEN}" \
	-H "Content-Type: application/json" \
	-H "Accept: application/hal+json" \
	-d  "{\"name\":\"githubToken\",\"description\":\"Github token\",\"value\":\"${GITHUB_TOKEN}\"}"

# NOTE: the github token secret must be created (either through the UI or using the
# `create_travis_token_secret` target) before the webhook is invoked.
create_or_update_pact_changed_webhook:
	"${PACT_CLI}" \
	  broker create-or-update-webhook \
	  "https://api.github.com/repos/${GITHUB_REPO}/dispatches" \
	  --header 'Content-Type: application/json' 'Accept: application/vnd.github.everest-preview+json' 'Authorization: Bearer $${user.githubToken}' \
	  --request POST \
	  --data '{ "event_type": "pact_changed", "client_payload": { "pact_url": "$${pactbroker.pactUrl}" } }' \
	  --uuid ${PACT_CHANGED_WEBHOOK_UUID} \
	  --consumer ${PACTICIPANT} \
	  --contract-content-changed \
	  --description "Pact content changed for ${PACTICIPANT}"

test_pact_changed_webhook:
	@curl -v -X POST ${PACT_BROKER_BASE_URL}/webhooks/${PACT_CHANGED_WEBHOOK_UUID}/execute -H "Authorization: Bearer ${PACT_BROKER_TOKEN}"

## ======================
## Misc
## ======================

.env:
	touch .env

.PHONY: start stop
