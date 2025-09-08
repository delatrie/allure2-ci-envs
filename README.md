# CI environments with Allure 2

This repository demonstrates how to create an Allure 2 report for tests executed across multiple environments. Check out the final report at [https://delatrie.github.io/allure2-ci-envs/](https://delatrie.github.io/allure2-ci-envs/).

> [!NOTE]
> Check out an alternative solution based on Allure 3 at [https://github.com/delatrie/allure3-ci-envs/](https://github.com/delatrie/allure3-ci-envs/). It utilizes the new "Environments" feature, explicitly designed for tasks like that.

## The problem

Suppose we have a CI job with a matrix strategy that runs the same set of tests in its instances:

```yml
jobs:
  build and test:
    strategy:
      matrix:
        os: ['windows-latest', 'ubuntu-latest', 'macos-latest']
    runs-on: ${{ matrix.os }}
    steps:
      # ...

      - name: Run tests
        run: dotnet test --logger trx --results-directory 'TestResults-${{ matrix.os }}'

      # ...
```

If we then gather all test results together and try to build an Allure 3 report, we'll only see one set of test results instead of three.

The rest of the results will be shown as retries, even though these are all independent test results that come from different environments.

## Some background

Allure 2 relies on a specific identifier called `historyId` to decide if two test results should be reported independently. This identifier is calculated based on two things:

  1. Something that uniquely identifies the test in the source code.
  2. The set of arguments passed to the test.

If we run a test in multiple environments, these environments can be seen as extra arguments to the test. Unfortunately, Allure 2 cannot retrieve this information from most test results formats, such as TRX or JUnit XML.

Luckily, Allure Report can integrate with the test framework to overcome this limitation. Once the integration is set up, it will generate test results in the [Allure 2 format](https://allurereport.org/docs/how-it-works-test-result-file/). It also allows adding more information to the result files.

Therefore, the key idea in this demo is to utilize the Allure integration for NUnit to provide Allure 2 with enough information to tell the results from different environments apart.

## Walkthrough

Let's do a quick overview of all the steps taken. Click the title of the section to view the diff of the step.

> [!NOTE]
> Follow [the commit history](https://github.com/delatrie/allure2-ci-envs/commits/main/) to get a better understanding of each step.

### [Step 1 - Install Allure.NUnit](https://github.com/delatrie/allure2-ci-envs/compare/init...step1)

This demo uses NUnit as a test framework. [`Allure.NUnit`](https://www.nuget.org/packages/Allure.NUnit) is the official Allure Report integration for it.

It requires applying `[AllureNUnit]` to all test classes. The easiest way to do this is by introducing a common base test class and applying the attribute to it.

Now, after `dotnet test` is done, the Allure result files will be created in the `allure-results` folder inside the build output directory.

> [!NOTE]
> The documentation for `Allure.NUnit` is available [here](https://allurereport.org/docs/nunit/).

### [Step 2 - Add the env info to the test results](https://github.com/delatrie/allure2-ci-envs/compare/step1...step2)

In step 2, we use an environment variable to create a parameter for each test result. The easiest way to do that is to define a teardown in the base test class:

```csharp
[AllureNUnit]
class BaseTestClass
{
    [TearDown]
    public void AddEnvInfo()
    {
        var value = Environment.GetEnvironmentVariable($"ALLURE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(value))
        {
            AllureApi.AddTestParameter("env", value);
        }
    }
}
```

The variable is set by the workflow:

```yml
- run: dotnet test
  env:
    ALLURE_ENVIRONMENT: '${{ matrix.os-title }}, ${{ matrix.framework }}, ${{ matrix.configuration }}'
```

This solves the main problem: from now on, the test results we get in the CI workflow will be associated with a specific environment.

We also define an explicit three-level hierarchy to group tests by environment, namespace, and class. The hierarchy will be visible on the `Suites` tab of the report.

### [Step 3 - Publish Allure 2 reports on GitHub Pages](https://github.com/delatrie/allure2-ci-envs/compare/step2...step3)

Now that our test results have all the required data, it's time to add a workflow job that creates and publishes the report.

Our recommended approach for publishing Allure 2 reports is by using the [simple-elf/allure-report-action](https://github.com/simple-elf/allure-report-action) action. It has the advantage of managing the history, including cross-links to historical reports.

We store the generated reports alongside historical data in a separate branch [used by GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site#publishing-from-a-branch) for deployments.

> [!NOTE]
> If the branch doesn't exist yet, it can be created with:
>
> ```shell
> git switch --orphan gh-pages
> git commit --allow-empty -m "init gh-pages"
> git push -u origin gh-pages
> ```

We also use [peaceiris/actions-gh-pages](https://github.com/peaceiris/actions-gh-pages) to deploy the resulting static files to GitHub Pages.

Refer to the [diff](https://github.com/delatrie/allure2-ci-envs/compare/step2...step3#diff-faff1af3d8ff408964a57b2e475f69a6b7c7b71c9978cccc8f471798caac2c88) to view the changes in the workflow definition.

Now the workflow creates and publishes a new version of the report each time it runs. Previous reports can be accessed from the "History" tab of a test result, or by manually editing the job number to a lesser value in the URL. The total number of available historical reports is 20 by default, but it can be changed:

```yml
- uses: simple-elf/allure-report-action@v1.13
  with:
    keep_reports: 50
```

> [!NOTE]
> The "History" tab only shows up to 20 entries. You can open the earliest one to access up to yet another 20 if you have them.

Check out the final Allure 2 report [here](https://delatrie.github.io/allure2-ci-envs/).
