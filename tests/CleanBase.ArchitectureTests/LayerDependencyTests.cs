using System.Reflection;
using CleanBase.Domain.Common;
using NetArchTest.Rules;
using TestResult = NetArchTest.Rules.TestResult;

namespace CleanBase.ArchitectureTests;

/// <summary>
/// Enforces the Dependency Rule: source code dependencies only point inwards
/// (Hosts -> Infrastructure -> Application -> Domain). A violating reference makes these tests fail.
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    // Derived from the Domain's namespace so the tests keep working after renaming the solution.
    private static readonly string RootNamespace = DomainAssembly.GetName().Name!.Replace(".Domain", string.Empty, StringComparison.Ordinal);
    private static readonly string ApplicationNamespace = $"{RootNamespace}.Application";
    private static readonly string InfrastructureNamespace = $"{RootNamespace}.Infrastructure";
    private static readonly string ApiNamespace = $"{RootNamespace}.Api";
    private static readonly string FunctionAppNamespace = $"{RootNamespace}.FunctionApp";

    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace, FunctionAppNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrHosts()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace, FunctionAppNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnHosts()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, FunctionAppNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void DomainAndApplication_ShouldNotDependOnEntityFrameworkCore()
    {
        TestResult result = Types.InAssemblies([DomainAssembly, ApplicationAssembly])
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Handlers_ShouldBeSealedAndInternal()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler", StringComparison.Ordinal)
            .Should()
            .BeSealed()
            .And()
            .NotBePublic()
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(TestResult result) =>
        result.IsSuccessful.ShouldBeTrue(
            $"Violating types: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
