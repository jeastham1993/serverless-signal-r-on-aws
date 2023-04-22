namespace SignalRRedisApprunner;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.IAM;

using Constructs;

public record SharedInfrastructureStackProps(IVpc Vpc, ISecurityGroup CacheSecurityGroup);

public class SharedInfrastructureStack : Construct
{
    public SharedInfrastructureStack(Construct scope, string id, SharedInfrastructureStackProps props) : base(scope, id)
    {
        var applicationRepository = new Repository(
            this,
            "AppRunnerSourceRepo",
            new RepositoryProps()
            {
                RepositoryName = "app-runner-source-repo",
            });
        
        var applicationRole = new Role(this, "ApplicationRole", new RoleProps
        {
            AssumedBy =new ServicePrincipal("tasks.apprunner.amazonaws.com"),
            RoleName = "SignalRAppRunnerApplicationRole"
        });

        var ecrAccessRole = new Role(this, "EcrAccessRole", new RoleProps()
        {
            AssumedBy = new ServicePrincipal("build.apprunner.amazonaws.com"),
            RoleName = "SignalRECRAccessRole",
        });

        applicationRepository.GrantPull(ecrAccessRole);
        
        var subnets = props.Vpc.SelectSubnets(new SubnetSelection()
        {
            SubnetType = SubnetType.PRIVATE_WITH_EGRESS
        });

        var cacheSubnetGroup = new CfnSubnetGroup(
            this,
            "CacheSubnetGroup",
            new CfnSubnetGroupProps()
            {
                CacheSubnetGroupName = "mysubnetgroup",
                SubnetIds = subnets.SubnetIds,
                Description = "Subnet group for redis cache"
            });
        
        /*var cache = new CfnCacheCluster(
            this,
            "RedisCluster",
            new CfnCacheClusterProps()
            {
                Engine = "redis",
                CacheNodeType = "cache.t3.small",
                NumCacheNodes = 1,
                CacheSubnetGroupName = "mysubnetgroup",
                VpcSecurityGroupIds = new[] {props.CacheSecurityGroup.SecurityGroupId},
                AzMode = "multi-az"
            });
        
        cache.AddDependency(cacheSubnetGroup);
        
        var redisConnectionArn = new CfnOutput(this, "redis-connection-endpoint", new CfnOutputProps()
        {
            ExportName = "RedisConnectionEndpoint",
            Value = cache.AttrRedisEndpointAddress
        });*/
    }
}