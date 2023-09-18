# Serverless SignalR on AWS

Building &amp; Deploying WebSockets Applications on AWS with SignalR.

## Deployment

1. Init and apply the terraform code in the `infra` directory
2. Clone the repository
3. Run `copilot env init` to initialize your environment
    - When asked, opt to import a VPC and select the VPC deployed as part of the Terraform apply
4. Run `copilot env deploy --name test` to deploy the test environment
5. Run `copilot svc init --name signal-r-front` to initialize the frontend service
6. Run `copilot svc init --name signal-r-gateway` to initialize the gateway
7. Run `copilot svc init --name translation-worker` to initialize the worker service
8. Go through each svc running the `copilot deploy --name` command. E.g. `copilot deploy --name signal-r-front`
    - Before deploying `signal-r-gateway` and `translation-worker` ensure you open the manifests under `copilot/signal-r-gateway/manifest.yml` and update the Redis endpoint and SQS queue URL's