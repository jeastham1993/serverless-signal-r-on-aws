module "vpc" {
  source = "terraform-aws-modules/vpc/aws"

  name = "serverless-signal-r"
  cidr = "10.0.0.0/16"

  azs             = ["eu-west-1a", "eu-west-1b", "eu-west-1c"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]

  enable_nat_gateway = true

  tags = {
    Terraform = "true"
  }
}

resource "aws_security_group" "signal_r_backplane_sg" {
  #checkov:skip=CKV2_AWS_5: Security Group is attached to the EKS module
  name        = "signal_r_backplane_sg"
  vpc_id      = module.vpc.vpc_id
  ingress {
    description = "Internal traffic ingress"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["10.0.0.0/16"]
  }
  egress {
    description = "Egress to internet"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_elasticache_subnet_group" "signal_r_backplane_subnet" {
  name       = "signal-r-backplane-subnet"
  subnet_ids = module.vpc.private_subnets
}

resource "aws_cloudwatch_log_group" "signal_r_backplane_logs" {
  name = "signal-r-backplane-logs"

  tags = {
    Environment = "production"
    Application = "serviceA"
  }
}

resource "aws_elasticache_cluster" "signal_r_backplane" {
  cluster_id           = "signal-r-backplane"
  engine               = "redis"
  node_type            = "cache.m4.large"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  engine_version       = "7.0"
  port                 = 6379
  subnet_group_name    = aws_elasticache_subnet_group.signal_r_backplane_subnet.name
   security_group_ids   = [aws_security_group.signal_r_backplane_sg.id]
  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.signal_r_backplane_logs.name
    destination_type = "cloudwatch-logs"
    log_format       = "text"
    log_type         = "slow-log"
  }
}

resource "aws_sqs_queue" "translation_queue" {
  name                      = "translation-queue"
}

resource "aws_sqs_queue" "translation_response_queue" {
  name                      = "translation-response-queue"
}