# 🎤 PoDebateRap - AI-Powered Rap Debate Arena

**Experience legendary rappers debating modern topics through AI-generated rap battles!**

PoDebateRap is an interactive web application that orchestrates AI-powered rap debates between iconic hip-hop artists. Select two legendary rappers, choose a controversial topic, and watch them battle it out in real-time with AI-generated verses and neural text-to-speech audio. An AI judge determines the winner based on lyrical prowess, argumentation, and style.

## ✨ Key Features

- 🎭 **10 Legendary Rappers** - Choose from icons like Eminem, Tupac, Nas, Jay-Z, and more
- 🤖 **AI-Powered Debates** - GPT-4 generates contextual rap verses matching each artist's unique style
- 🔊 **Neural Text-to-Speech** - Realistic voice synthesis brings the battle to life
- 📰 **Trending Topics** - Debate current events from NewsAPI or create custom topics
- ⚖️ **AI Judge** - Impartial AI analysis determines the winner with detailed reasoning
- 🔴 **Real-time Updates** - SignalR provides live debate progression
- 🏥 **Diagnostics Dashboard** - Monitor system health and test audio functionality

## 🏗️ Architecture

**Technology Stack:**
- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: ASP.NET Core Web API (.NET 9)
- **AI Services**: Azure OpenAI (GPT-4) + Azure Speech Services
- **Database**: Azure Table Storage / Azurite (local)
- **Real-time**: SignalR Hub
- **External APIs**: NewsAPI

**Design Principles:**
- Vertical Slice Architecture for feature organization
- SOLID principles with dependency injection
- Clean Architecture separation (API → Services → Data)
- Test-Driven Development (TDD)

## 📁 Project Structure

```
PoDebateRap/
├── Client/PoDebateRap.Client/          # Blazor WebAssembly UI
│   ├── Components/
│   │   ├── Debate/                     # Debate-specific components
│   │   │   ├── DebateSetup.razor       # Rapper & topic selection
│   │   │   └── DebateArena.razor       # Live debate display
│   │   ├── Diagnostics/                # System diagnostics
│   │   │   └── Diag.razor              # Health checks & audio tests
│   │   └── Pages/
│   │       └── Home.razor              # Main application page
├── Server/PoDebateRap.ServerApi/       # ASP.NET Core API
│   ├── Controllers/                    # API endpoints
│   ├── Services/                       # Business logic
│   │   ├── AI/                        # Azure OpenAI integration
│   │   ├── Speech/                    # Text-to-speech services
│   │   ├── Data/                      # Table Storage repositories
│   │   ├── Orchestration/             # Debate orchestration
│   │   └── News/                      # NewsAPI integration
│   └── Hubs/                          # SignalR real-time hubs
├── Shared/                            # Shared models & DTOs
├── Tests/                             # Comprehensive test suite
│   ├── PoDebateRap.UnitTests/        # Unit tests
│   ├── PoDebateRap.IntegrationTests/  # Azure service integration
│   ├── PoDebateRap.SystemTests/       # Playwright E2E tests
│   └── PoDebateRap.ApiTests/         # API endpoint tests
└── Diagrams/                          # Architecture diagrams (Mermaid SVGs)
```

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Azure account with:
  - Azure OpenAI Service
  - Azure Speech Services
  - Azure Storage Account
- [NewsAPI Key](https://newsapi.org/) (free tier available)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/punkouter25/PoDebateRap.git
   cd PoDebateRap
   ```

2. **Configure User Secrets**
   
   Run these commands from the project root to set up your Azure credentials:
   
   ```bash
   # Azure OpenAI Configuration
   dotnet user-secrets set "Azure:OpenAI:Endpoint" "https://your-resource.openai.azure.com/" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   dotnet user-secrets set "Azure:OpenAI:ApiKey" "your-api-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   dotnet user-secrets set "Azure:OpenAI:DeploymentName" "gpt-4" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   
   # Azure Speech Services
   dotnet user-secrets set "Azure:Speech:Region" "eastus" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   dotnet user-secrets set "Azure:Speech:SubscriptionKey" "your-speech-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   
   # Azure Storage (use Azurite for local development)
   dotnet user-secrets set "Azure:StorageConnectionString" "UseDevelopmentStorage=true" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   
   # NewsAPI
   dotnet user-secrets set "NewsApi:ApiKey" "your-newsapi-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   ```

3. **Run the Application**
   ```bash
   dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   ```
   
   The application will be available at:
   - **Main App**: http://localhost:5000
   - **Diagnostics**: http://localhost:5000/diag
   - **Swagger API**: http://localhost:5000/swagger

### Running Tests

```bash
# Run all tests (Unit + API)
dotnet test --filter "FullyQualifiedName!~SystemTests"

# Run E2E tests (requires server running)
dotnet test Tests/PoDebateRap.SystemTests

# Run integration tests (requires Azure credentials)
dotnet test Tests/PoDebateRap.IntegrationTests
```

## 🎮 How to Use

1. **Select Your Rappers**
   - Choose Rapper 1 from the dropdown (e.g., Eminem)
   - Choose Rapper 2 from the dropdown (e.g., Tupac Shakur)
   - Note: You cannot select the same rapper twice

2. **Choose a Topic**
   - Select from trending news headlines, OR
   - Enter your own custom debate topic (minimum 10 characters)

3. **Start the Battle**
   - Click the **"BEGIN DEBATE"** button
   - Watch as each rapper takes turns delivering verses
   - Listen to the AI-generated audio for each verse
   - See the AI judge's final decision and reasoning

4. **Check System Health** (Optional)
   - Navigate to `/diag` for diagnostics
   - Click "🔊 Test Audio" to verify text-to-speech
   - Review service health checks

## 📊 Architecture Diagrams

All architecture diagrams are available in the `/Diagrams` folder as SVG files:

- **Project Dependencies** - Visualizes .NET project relationships and dependencies
- **Domain Model Class Diagram** - Shows core business entities and their relationships
- **API Call Sequence Diagram** - Traces request flow for debate initiation
- **User Flow Flowchart** - Outlines the complete debate flow and decision points
- **Component Hierarchy** - Blazor component tree structure

## 🧪 Testing

The project includes comprehensive test coverage:

| Test Suite | Coverage | Purpose |
|------------|----------|---------|
| **Unit Tests** | 4 tests | Core business logic, service mocks |
| **API Tests** | 4 tests | Controller endpoints, request/response validation |
| **Integration Tests** | 16 tests | Azure services (OpenAI, Speech, Storage, NewsAPI) |
| **System/E2E Tests** | 12 tests | Playwright browser automation, full user flows |

**Current Test Status**: 15/21 runnable tests passing (71% success rate)

## 🔧 Troubleshooting

### Audio Not Playing
1. Check diagnostics page: `/diag`
2. Click "🔊 Test Audio" button
3. Verify first 50 bytes show `52 49 46 46` (RIFF header), not `00 00 00 00`
4. Check browser console for errors

### Azure Service Errors
1. Verify user secrets are configured correctly
2. Check Azure portal for service availability
3. Confirm API keys and endpoints are valid
4. Review `Server/log.txt` for detailed error messages

### BEGIN DEBATE Button Disabled
- Ensure two **different** rappers are selected
- Verify topic has at least 10 characters
- Check browser console for validation errors

## 📚 Documentation

- **[PRD.MD](./PRD.MD)** - Complete Product Requirements Document
- **[AGENTS.MD](./AGENTS.MD)** - Machine-readable documentation for AI coding agents
- **[DOCS/KQL_QUERIES.md](./DOCS/KQL_QUERIES.md)** - Essential Application Insights queries for monitoring
- **[Diagrams/](./Diagrams/)** - Architecture and flow diagrams (Mermaid SVG format)

## 🚢 Deployment

The application is configured for Azure App Service deployment via GitHub Actions using Azure Developer CLI (azd).

### Live Production URL
🌐 **https://podebaterap.azurewebsites.net**

### Azure Resources
- **App Service**: PoDebateRap (F1 Free tier, shared plan from PoShared resource group)
- **Application Insights**: appi-PoDebateRap-* (monitoring and telemetry)
- **Log Analytics**: law-PoDebateRap-* (structured logging)
- **Storage Account**: stdebaterap* (Azure Table Storage)

### CI/CD Pipeline
The deployment pipeline uses GitHub Actions with federated credentials (no secrets required):

**Workflow**: Build → Test → Deploy
- **Trigger**: Push to `main` branch
- **Build**: Compile .NET 9 application, create release artifacts
- **Test**: Run unit tests and integration tests
- **Deploy**: Provision infrastructure (Bicep) and deploy to Azure App Service

**Pipeline Status**: [View GitHub Actions](https://github.com/punkouter26/PoDebateRap/actions)

### Manual Deployment with Azure Developer CLI
```bash
# Provision infrastructure + deploy application
azd up

# Or run separately:
azd provision  # Create/update Azure resources
azd deploy     # Deploy application code
```

### Infrastructure as Code
All Azure resources are defined in Bicep templates (`/infra` folder):
- `main.bicep` - Subscription-level deployment
- `resources.bicep` - Resource definitions (App Service, Application Insights, Storage)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is for educational and entertainment purposes.

## 🙏 Acknowledgments

- Azure OpenAI for GPT-4 debate generation
- Azure Speech Services for neural text-to-speech
- NewsAPI for trending topics
- The legendary hip-hop artists who inspire this project

---

**Built with ❤️ using .NET 9, Blazor, and Azure AI Services**

*Last Updated: October 28, 2025 - Phase 5 Complete*
