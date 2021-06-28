# Ed-Fi-LearningStandards-Client
.NET Standard Learning Standards core client assembly and reference CLI implementation

## Core Client Assembly Integration
The anticipated usage of the client core assembly is in two parts.

+ Startup
+ Invocation

### Startup
The Learning Standards Core Client Assembly utilizes the .NET Standard Dependency Injection (DI) container abstractions. These can either be satisfied using the `LearningStandardsServiceCollectionExtensions` or by using the `LearningStandardsCorePluginConnector`. The connector allows for an isolated container configuration, whereas the installer will blend the registrations in the host container.

#### LearningStandardsCorePluginConnector
The connector has an interface of `ILearningStandardsCorePluginConnector` that allows it to be optionally registered as a singleton in the host application's DI container, if it has one.

Construction of the plugin connector utilizes
* `IServiceCollection` which could be derived from a parent container, or a new container.
* `Func<IServiceCollection, IServiceProvider>` to convert the IServiceCollection to an IServiceProvider.
* `ILoggerProvider` provides the ability to link to the host's logging provider.
* `IEdFiOdsApiClientConfiguration` a configuration class for ODS API client parameters.

Example:
``` csharp
var edfiOdsApiClientConfiguration = new EdFiOdsApiClientConfiguration();
var serviceCollection = new ServiceCollection();
// Register your ChangeSequencePerister here:
serviceCollection.AddSingleton<IChangeSequencePersister, DatabaseChangeSequencePersister>();
IServiceProvider ServiceProviderFunc(IServiceCollection collection) => collection.BuildServiceProvider();

var pluginConnector = new LearningStandardsCorePluginConnector(
    serviceCollection,
    ServiceProviderFunc,
    new log4netLogProvider(),
    edfiOdsApiClientConfiguration
);

// Optional host container registration
container.Register(Component.For<ILearningStandardsCorePluginConnector>().Instance(pluginConnector));
```

The following NuGet Packages are suggested for use with the plugin connector:
* [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/2.1.1)
* For log4net in .NET framework [log4net.Extensions.AspNetCore](https://www.nuget.org/packages/log4net.Extensions.AspNetCore/)
* For log4net in .NET Core [Microsoft.Extensions.Logging.Log4Net.AspNetCore](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Log4Net.AspNetCore/) (this package is very opinionated about needing to load the log4net config, and not using existing statics)

#### LearningStandardsServiceCollectionExtensions
This class includes the extension method `AddLearningStandardsServices`
Usage:
``` csharp
// Where serviceCollection is a variable of type IServiceCollection
var odsApiClientConfiguration = new EdFiOdsApiClientConfiguration(retries:3,maxSimultaneousRequests:6);
serviceCollection.AddLearningStandardsServices(odsApiClientConfiguration);
```
The container used could be any container that supports IServiceCollection.

### Invocation

When using the `ILearningStandardsCorePluginConnector` there are properties on the connector for `LearningStandardsSynchronizer`, `LearningStandardsConfigurationValidator`, and `LearningStandardsChangesAvailable`. 

For the direct container registration scenario, the registered Learning Standards services are available via their interfaces from the container constructed from the service collection. i.e. `ILearningStandardsConfigurationValidator`, `ILearningStandardsSynchronizer`, `ILearningStandardsChangesAvailable`.

#### Configuration

There are 2 configuration interfaces related to the invocation, `IAuthenticationConfiguration` and `IEdFiOdsApiConfiguration`.
Both interfaces have a default implementations with constructors that look like this:

``` csharp
/// <summary>
/// Authentication configuration
/// </summary>
/// <param name="key">The key to use for authentication</param>
/// <param name="secret">The secret to use for authentication</param>
public AuthenticationConfiguration(string key, string secret)
{
    Key = key;
    Secret = secret;
}
```

``` csharp
/// <summary>
/// EdFiOdsApiConfiguration for the ODS API to be populated with LearningStandards
/// </summary>
/// <param name="url">The base url for the ODS API </param>
/// <param name="version">The major version of the ODS API</param>
/// <param name="oAuthAuthenticationConfiguration">The OAuth authentication credentials</param>
/// <param name="schoolYear">The school year (if applicable), required for v2</param>
/// <param name="authenticationUrl">Optionally specify the path to the base url for authorization. (Should not include /oauth route)</param>
public EdFiOdsApiConfiguration(
    string url, 
    EdFiOdsApiCompatibilityVersion version, 
    IAuthenticationConfiguration oAuthAuthenticationConfiguration, 
    int? schoolYear = null,
    string authenticationUrl = null)
{
    Url = url;
    Version = version;
    OAuthAuthenticationConfiguration = oAuthAuthenticationConfiguration;
    SchoolYear = schoolYear;
    AuthenticationUri = authenticationUrl ?? url;
}
```

#### ILearningStandardsConfigurationValidator

This interface allows for configuration objects to be validated before being used with the Learning Standard Synchronization process.

``` csharp
public interface ILearningStandardsConfigurationValidator
{
    Task<IResponse> ValidateConfigurationAsync(IAuthenticationConfiguration learningStandardsAuthenticationConfiguration, IEdFiOdsApiConfiguration edFiOdsApiConfiguration);
    Task<IResponse> ValidateLearningStandardProviderConfigurationAsync(IAuthenticationConfiguration learningStandardsAuthenticationConfiguration);
    Task<IResponse> ValidateEdFiOdsApiConfigurationAsync(IEdFiOdsApiConfiguration edFiOdsApiConfiguration);
}
```

#### ILearningStandardsSynchronizer

This interface allows for the learnings standards for the provided learningStandardsAuthenticationConfiguration to be loaded into the Ed-Fi ODS API indicated by the odsApiConfiguration.

``` csharp
    public interface ILearningStandardsSynchronizer
    {
        [Obsolete("This method implicitly does a full sync. It is maintained for backwards compatibility and may be removed in a future release.")]
        Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progress);

        Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            ILearningStandardsSynchronizationOptions options,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progress);
    }
```
The first signature is deprecated, calls for a full sync, and does not utilize change support, this is to maintain backwards compatibility.
The second signature is recommended and allows for the specification of an ILearningStandardsSynchronizationOptions (or null which defaults to the changes supported configuration). This enables greater flexibility in specifying options for the synchronization process.

``` csharp
    public interface ILearningStandardsSynchronizationOptions
    {
        bool ForceFullSync { get; set; }
    }
```

Both signatures also expose an IProgress<LearningStandardsSynchronizerProgressInfo> parameter as part of the [TAP pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap#progress-reporting-optional) that can be used to get progress during the execution.

Example:
``` csharp
var progress = new Progress<LearningStandardsSynchronizerProgressInfo>(
                info => Console.WriteLine($"{info.TaskName} is {info.TaskState}. Sync Process is {info.CompletedPercentage}% complete."));
```

#### ILearningStandardsChangesAvailable

This interface allows for returning an indicator of available learnings standards for the provided learningStandardsAuthenticationConfiguration and odsApiConfiguration. It will indicate these changes in reference to the stored change sequoence state indicated by the IChangeSequencePersister.

``` csharp
    public interface ILearningStandardsChangesAvailable
    {
        Task<IChangesAvailableResponse> ChangesAvailableAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            CancellationToken cancellationToken = default);
    }
```
The IChangesAvailableResponse implements the IResponse interface that the Synchronization and Verification processes return, but adds IChangesAvailableInformation. The content property of the IResponse provides a plaintext rendering of the information, but the intent is that the IChangeSequenceInformation should be used for processing.

``` csharp
    public interface IChangesAvailableResponse : IResponse
    {
        IChangesAvailableInformation ChangesAvailableInformation { get; }
    }
```
``` csharp
    public interface IChangesAvailableInformation
    {
        bool Available { get; set; }

        IChangeSequence Current { get; set; }

        IChangeSequence MaxAvailable { get; set; }
    }
```
#### IChangeSequencePersister

This interface allows for the persistence and retrieval of the change event sequence id, by Ed-Fi Key and AB Id. By default, the core library uses a no-op store with logging. 
This interface must be implemented and the corresponding type added to the services collection before change sequences can be persisted. For the CLI a json file backed store is used.

``` csharp
    public interface IChangeSequencePersister
    {
        Task SaveAsync(
            IChangeSequence changeSequence,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IChangeSequence> GetAsync(
            string edFiApiKey,
            string learningStandardCredentialId,
            CancellationToken cancellationToken = default(CancellationToken));
    }
```

When retrieving, an `IChangeSequence` object will be returned, containing the persisted change event sequence id, and it's key, the Ed-Fi API key and the learning standards credential id. Basic json serializable class implementations of these interfaces can be found in the core library (namespace: EdFi.Admin.LearningStandards.Core.Models).

``` csharp
    public interface IChangeSequence
    {
        long Id { get; set; }
        IChangeSequenceKey Key { get; set; }
    }

    public interface IChangeSequenceKey
        : IEquatable<IChangeSequenceKey>
    {
        string EdFiApiKey { get; }
        string LearningStandardCredentialId { get; }
    }
```
