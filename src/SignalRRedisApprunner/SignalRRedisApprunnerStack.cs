using Amazon.CDK;
using Constructs;

namespace SignalRRedisApprunner
{
    public class SignalRRedisApprunnerStack : Stack
    {
        internal SignalRRedisApprunnerStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var networkStack = new NetworkStack(
                this,
                "NetworkStack");

            var shared = new SharedInfrastructureStack(
                this,
                "SharedInfrastructure",
                new SharedInfrastructureStackProps(
                    networkStack.Vpc,
                    networkStack.ApplicationSecurityGroup));
        }
    }
}
