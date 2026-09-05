# GitLab CI/CD Pipeline Documentation

This document explains the GitLab CI/CD pipeline configuration for the Drools.NET project.

## Pipeline Overview

The pipeline consists of the following stages:

1. **Validate** - Project structure and dependency validation
2. **Build** - Compile the entire solution
3. **Test** - Run unit tests, examples, and security scans
4. **Package** - Create NuGet packages
5. **Deploy** - Deploy packages to registries

## Pipeline Stages Detail

### 1. Validate Stage

- **validate**: Checks project structure and restores dependencies
- **quality:check**: Runs code formatting checks using `dotnet format`

### 2. Build Stage

- **build**: Compiles the solution in Release configuration
- Artifacts: Compiled binaries and build outputs

### 3. Test Stage

- **test:unit**: Runs comprehensive unit tests with code coverage
- **test:examples**: Runs example project tests
- **security:scan**: Scans for known vulnerabilities in dependencies
- **test:performance**: Runs performance benchmarks (optional)

### 4. Package Stage

- **package:nuget**: Creates NuGet packages for distribution
- **docs:generate**: Generates project documentation (if configured)

### 5. Deploy Stage

- **deploy:gitlab**: Automatically deploys to GitLab Package Registry
- **deploy:nuget**: Manually deploys to NuGet.org (tags only)

## Required Environment Variables

### For NuGet.org Deployment

Set these in GitLab Project Settings > CI/CD > Variables:

- `NUGET_API_KEY`: Your NuGet.org API key (masked, protected)

### Automatic Variables (provided by GitLab)

- `CI_JOB_TOKEN`: Used for GitLab Package Registry authentication
- `CI_PROJECT_ID`: Project identifier
- `CI_API_V4_URL`: GitLab API URL

## Pipeline Triggers

### Automatic Triggers

- **Merge Requests**: Runs validate, build, and test stages
- **Main Branch**: Runs full pipeline including GitLab registry deployment
- **Develop Branch**: Runs full pipeline including GitLab registry deployment
- **Tags**: Runs full pipeline including manual NuGet.org deployment

### Manual Triggers

- **deploy:nuget**: Manual deployment to NuGet.org (requires approval)

## Code Coverage

The pipeline generates code coverage reports using:
- XPlat Code Coverage collector
- Cobertura format for GitLab integration
- Coverage percentage displayed in merge requests

## Artifacts

### Build Artifacts
- **Expiry**: 1 hour
- **Content**: Compiled binaries, build outputs

### Test Artifacts
- **Expiry**: 1 week  
- **Content**: Test results, coverage reports
- **Reports**: JUnit test results, Cobertura coverage

### Package Artifacts
- **Expiry**: 1 month
- **Content**: NuGet packages ready for deployment

## Security Features

### Vulnerability Scanning
- **SAST**: Static Application Security Testing
- **Dependency Scanning**: Checks for vulnerable dependencies
- **Package Vulnerability Check**: Uses `dotnet list package --vulnerable`

### Security Best Practices
- Secrets stored in GitLab CI variables (masked)
- Manual approval required for production deployments
- Separate environments for different deployment targets

## Performance Optimization

### Caching
- NuGet packages cached between builds
- Build artifacts cached for 1 hour between stages

### Parallel Execution
- Unit tests and example tests run in parallel
- Security scans run alongside other tests

### Conditional Execution
- Some jobs allow failure to prevent pipeline blocking
- Documentation and performance tests are optional

## Local Development

### Prerequisites
- .NET 8.0 SDK
- Git

### Running Pipeline Stages Locally

```bash
# Validate (restore dependencies)
dotnet restore

# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release --collect:"XPlat Code Coverage"

# Package
dotnet pack --configuration Release --output ./packages/
```

## Troubleshooting

### Common Issues

1. **Build Failures**
   - Check .NET SDK version compatibility
   - Verify all dependencies are available
   - Review compiler warnings and errors

2. **Test Failures**
   - Check test isolation and cleanup
   - Verify test data and resources
   - Review test output logs

3. **Deployment Issues**
   - Verify API keys are correctly set
   - Check package version conflicts
   - Review deployment logs

### Debug Mode

To enable verbose logging, modify pipeline variables:
```yaml
variables:
  DOTNET_CLI_VERBOSITY: "detailed"
```

## Pipeline Configuration

### Customizing the Pipeline

1. **Adding New Stages**: Add to the `stages` array
2. **Modifying Jobs**: Edit job configurations in `.gitlab-ci.yml`
3. **Environment Variables**: Add to project CI/CD settings
4. **Artifacts**: Adjust expiry times and paths as needed

### Branch-Specific Behavior

- **Feature Branches**: Run validation, build, and tests only
- **Main/Develop**: Full pipeline with automatic GitLab registry deployment
- **Tags**: Full pipeline with optional NuGet.org deployment

## Monitoring and Notifications

### Pipeline Status
- View in GitLab project pipeline section
- Email notifications for failures (configurable)
- Slack/Teams integration available

### Coverage Trends
- Track coverage changes over time
- View coverage reports in merge requests
- Set minimum coverage requirements

## Best Practices

1. **Commit Messages**: Use conventional commits for better pipeline integration
2. **Branching**: Use feature branches for development
3. **Testing**: Ensure comprehensive test coverage before merging
4. **Versioning**: Use semantic versioning for tags
5. **Dependencies**: Keep dependencies up to date
6. **Security**: Regularly review vulnerability reports

## Support

For pipeline issues:
1. Check the GitLab CI/CD documentation
2. Review pipeline logs in GitLab
3. Contact the development team
4. Create an issue in the project repository