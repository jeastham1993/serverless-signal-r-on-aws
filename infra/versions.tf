terraform {
  required_version = ">= 0.12"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.16.2"
    }
  }
}

provider "aws" {
  region = "eu-west-1"
}