# Unit testing frameworks

This project has unit tests.

### Frontend:

:::info
There are frontend tests available in this application, however, most of them are redundant and only check whether components are rendered, and don't test business logic
:::

For the frontend we use vitest.

In order to run the tests, you have a number of options avaliable to you:

1. Just run the tests using `npm run test`
    * this will run the test, and dynamically reruns them when changes occur.
    * adding the `-- --coverage` flag will also calculate code coverage.
    * further add `-- --reporter=junit --outputFile=vitest-report.xml` will create an report. This is also used in the CI/CD pipeline for running tests on creation of merge requests.

2. Using the command `npm run testui` will bring up a webpage which shows a little more informations about the run tests, this also dynamically updates like you are used to with vite.

### Backend

#### Unit tests
For the backend, we use Xunit.

In order to run the tests, there are two options available:

1. In your IDE of choice, hopefully Jetbrains Rider, there is a built in system to run, debug, and generate coverage on unit tests. Use this if possible

2. If that doesn't work, you can similarly run `dotnet test`, with `-- --coverage` for coverage. similarly, adding `--coverage-output coverage.xml` will create a coverage report. This is also used in the CI/CD pipeline for running tests on creation of merge requests

#### Integration tests
Besides unit tests, integration tests are also written. For this [Microsoft.AspNetCore.Mvc.Testing](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing) is used and integrates nicely with Xunit. [For further detail see](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0&pivots=xunit).

These integration tests are ran the same way as normal unit tests, see above how to do this.

##### Workings of integration tests
A `WebApplicationFactory<Program>` is implemented by the `TestWebApplicationFactory` to create an in memory HTTP server of the main `Program` with an in memory SQLite database. 

##### Creating an integration test
Creating an integration test is fairly easy:
1. Create a new test class implementing the `AbstractAsyncLifetime` class
    1. Both the `InitializeAsync` and `DisposeAsync` can be overwritten to add functionality, but make sure to always call the base method.
2. Create the tests with the Xunit `[Fact]` attribute (Theory can't be used because it is executed in parallel and causes parallel database connection issues. When using theory, sometimes the database can be closed whilst some tests are still running causing an error to be thrown. This would make the tests flaky.)
    1. To add entries to the database, the database should be created using the method of the factory:
        ```csharp
        var db = _factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        ```
    2. To make calls to the controllers, an [HTTPClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0) is created and used. This client makes HTTP requests to the provided destination, with an optional body or query parameters
        ```csharp
        var client = _factory.CreateClientAs(userId, $"{userId}@test.com", role);
        var response = await client.GetAsync("/api/example/example");
        ```
    3. Scopes can be used to access methods and classes of the main program
    ```csharp
    using var scope = _factory.Services.CreateScope();
    var dummyService = scope.ServiceProvider.GetRequiredService<IDummyService>();
    ```
    4. The test's database caches entries in its change tracker, this causes methods that update an entry to not update in the tests database. To ensure that the database gets updated, the change tracker should be cleared
    ```csharp
    db.ChangeTracker.Clear();
    ```
3. Some integration tests make use of classes that use external API's or classes that should be mocked to ensure stable tests. 
For this a mocked class should be created inside the `MockClasses` folder. In the `TestWebApplicationFactory` the class that is mocked, should be removed from the scope, and the mock of the class should be added to the scope.
    - For example: `DummyClass` should be mocked because it uses an external API, the class `MockDummyClass` is created as a mock.
    ```csharp
    services.RemoveAll<DummyClass>(); // all instances of the DummyClass should be removed from the services scope
    services.AddScoped<MockDummyClass>(); // Add the mock of the dummy class to the scope
    ``` 