# GitHub Actions Configuration

This file contains the GitHub Actions configuration for the Sterling Auctions repository.

## Workflow Files

- `ci-cd.yml` - Main CI/CD pipeline
- `e2e-tests.yml` - End-to-end testing
- `security.yml` - Security scanning
- `performance.yml` - Performance testing
- `release.yml` - Release management
- `dependencies.yml` - Dependency updates

## Required Secrets

Configure these secrets in your repository settings:

### AWS Deployment
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`

### Security Scanning
- `SNYK_TOKEN`
- `SEMGREP_APP_TOKEN`

### Notifications
- `SLACK_WEBHOOK_URL`
- `DISCORD_WEBHOOK_URL`

### Code Coverage
- `CODECOV_TOKEN`

## Branch Protection Rules

### Master Branch
- Require pull request reviews (2 reviewers)
- Require status checks to pass before merging
- Require branches to be up to date before merging
- Require linear history
- Restrict pushes that create files larger than 100MB

### Develop Branch
- Require pull request reviews (1 reviewer)
- Require status checks to pass before merging
- Allow force pushes

## Environment Variables

Set these environment variables in your repository settings:

- `NODE_VERSION`: 18
- `DOTNET_VERSION`: 9.0.x
- `REGISTRY`: ghcr.io
- `IMAGE_NAME`: ${{ github.repository }}

## Workflow Triggers

### CI/CD Pipeline
- Push to master/develop branches
- Pull requests to master/develop branches
- Release events

### E2E Tests
- Push to master/develop branches
- Pull requests to master/develop branches
- Manual dispatch

### Security Scanning
- Push to master/develop branches
- Pull requests to master/develop branches
- Weekly schedule (Monday 2 AM)

### Performance Testing
- Push to master/develop branches
- Pull requests to master/develop branches
- Weekly schedule (Sunday 3 AM)

### Release Management
- Git tags (v*)
- Manual dispatch with version input

### Dependency Updates
- Weekly schedule (Monday 9 AM)
- Manual dispatch

## Artifacts

### Test Artifacts
- Playwright HTML reports
- Code coverage reports
- Performance test results
- Security scan results

### Build Artifacts
- Docker images
- Release packages
- Bundle analysis reports

## Monitoring

### Success Metrics
- Build success rate
- Test pass rate
- Deployment success rate
- Security scan pass rate

### Failure Alerts
- Build failures
- Test failures
- Security vulnerabilities
- Performance regressions

## Troubleshooting

### Common Issues
1. Build failures - Check dependencies and environment
2. Test failures - Verify test environment and data
3. Deployment failures - Check AWS credentials and ECS config
4. Security scan failures - Review vulnerability reports

### Debug Commands
```bash
# Check workflow status
gh run list --workflow=ci-cd.yml

# View workflow logs
gh run view <run-id>

# Download artifacts
gh run download <run-id>

# Rerun failed jobs
gh run rerun <run-id>
```

## Best Practices

- Use matrix strategies for parallel execution
- Implement proper error handling
- Use caching for dependencies
- Set appropriate timeouts
- Use least-privilege secrets
- Rotate secrets regularly
- Scan for exposed credentials
- Use dependency scanning
- Parallel job execution
- Efficient caching strategies
- Optimized Docker builds
- Resource management
- Regular dependency updates
- Workflow optimization
- Documentation updates
- Monitoring and alerting
