﻿using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace SharedInfrastructure;

public record ApplicationInfrastructureStackProps(IVpc Vpc, ISecurityGroup CacheSecurityGroup);

public class ApplicationInfrastructureStack : Construct
{
    public ApplicationInfrastructureStack(Construct scope, string id, ApplicationInfrastructureStackProps props) : base(scope, id)
    {
        var applicationRepository = new Repository(
            this,
            "AppRunnerSourceRepo",
            new RepositoryProps()
            {
                RepositoryName = "app-runner-source-repo",
            });

        var translationDeadLetterQueue = new Queue(this, "TranslationDLQ", new QueueProps()
        {
            QueueName = "TranslationDeadLetterQueue"
        });

        var translationQueue = new Queue(this, "TranslationQueue", new QueueProps
        {
            DeadLetterQueue = new DeadLetterQueue
            {
                MaxReceiveCount = 3,
                Queue = translationDeadLetterQueue
            },
            QueueName = "TranslationQueue"
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
        
        var translationQueueOuput = new CfnOutput(this, "translation-queue", new CfnOutputProps()
        {
            ExportName = "TranslationQueue",
            Value = translationQueue.QueueUrl
        });
    }
}