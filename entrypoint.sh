#!/bin/bash

dotnet nuget add source --name Artifactory https://artifactory.aws.wiley.com/artifactory/api/nuget/nuget
dotnet watch run --project=Enrollments.API --urls="https://0.0.0.0:5114;http://0.0.0.0:5113" --no-launch-profile
