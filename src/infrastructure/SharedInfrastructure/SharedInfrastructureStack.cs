using Amazon.CDK;
using Constructs;

namespace SharedInfrastructure
{
    public class SharedInfrastructureStack : Stack
    {
        internal SharedInfrastructureStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var networkStack = new NetworkStack(
                this,
                "NetworkStack");

            var shared = new ApplicationInfrastructureStack(
                this,
                "ApplicationInfrastructure",
                new ApplicationInfrastructureStackProps(
                    networkStack.Vpc,
                    networkStack.ApplicationSecurityGroup));
        }
    }
}
