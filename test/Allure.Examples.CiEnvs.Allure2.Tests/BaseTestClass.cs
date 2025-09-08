using System;
using Allure.Net.Commons;
using Allure.NUnit;
using NUnit.Framework;

namespace Allure.Examples.CiEnvs.Allure2.Tests;

[AllureNUnit]
abstract class BaseTestClass
{
    [TearDown]
    public void AddEnvInfo()
    {
        var value = Environment.GetEnvironmentVariable($"ALLURE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(value))
        {
            AllureApi.AddTestParameter("env", value);
            AllureApi.AddParentSuite(value);
        }

        var type = this.GetType();
        var @namespace = type.Namespace;

        if (!string.IsNullOrEmpty(@namespace))
        {
            AllureApi.AddSuite(@namespace);
        }

        AllureApi.AddSubSuite(type.Name);
    }
}
