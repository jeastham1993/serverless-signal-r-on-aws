namespace SignalRRedisApprunner;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

using Constructs;

public class NetworkStack : Construct
{
    public Vpc Vpc { get; private set; }
    public ISecurityGroup ApplicationSecurityGroup { get; private set; }
    
    public NetworkStack(Construct scope, string id) : base(scope, id)
    {
        Vpc = new Vpc(this, "Network", new VpcProps()
        {
            IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
            EnableDnsHostnames = true,
            EnableDnsSupport = true,
            NatGateways = 2,
            MaxAzs = 3,
        });

        ApplicationSecurityGroup = new SecurityGroup(this, "ApplicationSecurityGroup", new SecurityGroupProps()
        {
            SecurityGroupName = "ApplicationSG",
            Vpc = Vpc,
            AllowAllOutbound = true,
        });

        var sgOutput = new CfnOutput(this, "app-sg", new CfnOutputProps()
        {
            ExportName = "ApplicationSecurityGroupID",
            Value = ApplicationSecurityGroup.SecurityGroupId
        });
        
        var appSubnet1Output = new CfnOutput(this, "app-subnet-1", new CfnOutputProps()
        {
            ExportName = "ApplicationSubnetID1",
            Value = Vpc.PrivateSubnets[0].SubnetId
        });
        var appSubnet2Output = new CfnOutput(this, "app-subnet-2", new CfnOutputProps()
        {
            ExportName = "ApplicationSubnetID2",
            Value = Vpc.PrivateSubnets[1].SubnetId
        });
    }
}