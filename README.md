# PoDebateRap

PoDebateRap is a web application that pits two AI-generated rappers against each other in a debate-style rap battle. Users can select topics and rappers, and then watch the debate unfold. The application uses AI to generate the rap lyrics and to judge the winner of the debate.

## Technologies Used

- .NET 9
- ASP.NET Core
- Blazor WebAssembly
- Azure OpenAI for text generation
- Azure Speech Services for text-to-speech
- Azure Table Storage for data persistence

## Running Locally

1.  **Prerequisites:**
    - .NET 9 SDK
    - An Azure account with access to OpenAI, Speech, and Storage services.

2.  **Configuration:**
    - The application uses .NET user secrets to store API keys and connection strings for local development. You will need to configure the following secrets for the `PoDebateRap.ServerApi` project:
        - `Azure:OpenAI:Endpoint`
        - `Azure:OpenAI:ApiKey`
        - `Azure:OpenAI:DeploymentName`
        - `Azure:Speech:Region`
        - `Azure:Speech:SubscriptionKey`
        - `Azure:StorageConnectionString`
        - `NewsApi:ApiKey`

    - To set a secret, run the following command from the root of the repository, replacing `<SECRET_NAME>` and `<SECRET_VALUE>` with the appropriate values:
      ```bash
      dotnet user-secrets set "<SECRET_NAME>" "<SECRET_VALUE>" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
      ```

3.  **Running the Application:**
    - Once the secrets are configured, you can run the application from the root of the repository:
      ```bash
      dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
      ```
    - The application will be available at `https://localhost:5001`.

## Building and Deployment

The application is configured to build and deploy to Azure App Service on every push to the `main` branch using GitHub Actions.
