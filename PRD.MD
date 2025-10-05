# Product Requirements Document (PRD)
## PoDebateRap - AI-Powered Rap Debate Application

---

## 📋 Application Overview

PoDebateRap is an interactive web application that orchestrates AI-powered rap battles between legendary hip-hop artists. The application leverages Azure OpenAI for intelligent debate generation, Azure Speech Services for realistic text-to-speech audio, and Azure Table Storage for data persistence. Users select two rappers and a debate topic, then witness an engaging back-and-forth rap battle judged by AI.

### **Core Value Proposition**
- **Entertainment**: Experience legendary rappers debating modern topics through AI-generated rap verses
- **Education**: Learn debate techniques and rhetorical skills through hip-hop culture
- **Innovation**: Showcase cutting-edge AI integration (GPT-4, Neural TTS, Cloud Services)

---

## 🎯 Key Features

### 1. **Rapper Selection**
- Browse a curated list of 10 legendary hip-hop artists
- Visual cards with rapper names and images
- Validation to prevent selecting the same rapper twice
- Real-time feedback on selection state

### 2. **Topic Selection**
- Choose from trending news headlines (powered by NewsAPI)
- Enter custom debate topics
- Real-time topic validation
- Minimum character requirements

### 3. **AI-Powered Debate**
- Multi-turn rap battles (configurable rounds)
- Context-aware verses that respond to opponent's arguments
- Personality-based responses matching each rapper's style
- AI judge provides winner determination with reasoning

### 4. **Text-to-Speech Audio**
- Neural voice synthesis for realistic audio
- Voice matching based on rapper demographics
- Real-time audio playback during debate
- WAV format audio generation (16kHz, 16-bit mono PCM)

### 5. **Real-time State Management**
- SignalR hub for live debate updates
- Progress indicators and loading states
- Error handling with user-friendly messages
- Cancellation support for long-running debates

### 6. **Diagnostics & Health Monitoring**
- System health checks for all Azure services
- Test audio functionality
- Service status dashboard
- Logging and troubleshooting tools

---

## 🏗️ Architecture

### **Technology Stack**
- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: ASP.NET Core Web API (.NET 9)
- **AI Services**: Azure OpenAI (GPT-4), Azure Speech Services
- **Data Storage**: Azure Table Storage / Azurite (local development)
- **Real-time Communication**: SignalR
- **External APIs**: NewsAPI for trending topics

### **Design Principles**
- **Vertical Slice Architecture**: Features organized by functionality, not technical layers
- **SOLID Principles**: Single responsibility, dependency injection, interface segregation
- **Clean Architecture**: Clear separation of concerns (API, Services, Models)
- **Test-Driven Development**: Comprehensive unit, integration, and E2E test coverage

---

## 🎨 UI Components & Functionality

### **Page: Home (`/`)**
**Component**: `Home.razor`
- **Purpose**: Main application interface hosting the entire debate experience
- **Responsibilities**:
  - Orchestrate child components (DebateSetup, DebateArena)
  - Manage application-level state (current debate, selected rappers, topic)
  - Handle API communication for fetching rappers and topics
  - Error boundary for component failures

### **Component: DebateSetup**
**File**: `DebateSetup.razor`
- **Purpose**: User input interface for configuring debate parameters
- **Features**:
  - **Rapper Selector 1 & 2**: Dropdown menus with rapper names
    - Validation: Prevents duplicate selection
    - Two-way binding with parent component
    - Event callbacks for selection changes
  - **Topic Input**: Text box or dropdown for debate subject
    - Supports custom topics or news headlines
    - Real-time validation (minimum 10 characters)
  - **BEGIN DEBATE Button**:
    - Enabled state: Two different rappers + valid topic
    - Triggers debate initiation via callback
    - Loading state during API calls

### **Component: DebateArena**
**File**: `DebateArena.razor`
- **Purpose**: Real-time display of ongoing rap battle
- **Features**:
  - **Turn-by-turn Display**:
    - Rapper name and avatar
    - Verse text with animated appearance
    - Audio playback controls
  - **Progress Indicators**:
    - Current turn number / total turns
    - Loading spinners during AI generation
    - Audio playback status
  - **Judge Decision Display**:
    - Winner announcement
    - Reasoning breakdown
    - Visual highlighting of winning rapper

### **Component: RapperCard**
**File**: `RapperCard.razor` (if exists)
- **Purpose**: Reusable card component for displaying rapper information
- **Features**:
  - Rapper name, image, and biographical info
  - Selection state visualization
  - Hover effects and accessibility support

### **Page: Diagnostics (`/diag`)**
**Component**: `Diag.razor`
- **Purpose**: System health monitoring and troubleshooting
- **Features**:
  - **Service Health Checks**:
    - Internet connectivity
    - Azure OpenAI service
    - Azure Speech service
    - Table Storage connection
  - **Test Audio Button**: 
    - Generate sample TTS audio
    - Verify audio pipeline functionality
    - Display first 50 bytes of audio data for debugging
  - **Results Table**:
    - Green checkmarks (✔) for passing checks
    - Red X marks for failures
    - Detailed error messages

### **Layout: MainLayout**
**File**: `MainLayout.razor`
- **Purpose**: Application-wide layout and navigation
- **Features**:
  - Navigation header with app branding
  - Route-based content rendering
  - Responsive design for mobile/desktop
  - Modal support for diagnostics

---

## 🔄 User Flows

### **Primary Flow: Start a Debate**
1. User navigates to homepage
2. App fetches rapper list and trending topics
3. User selects Rapper 1 (e.g., Eminem)
4. User selects Rapper 2 (e.g., Tupac Shakur)
5. User chooses topic (e.g., "Social Media Impact")
6. BEGIN DEBATE button becomes enabled
7. User clicks BEGIN DEBATE
8. SignalR connection established
9. **Turn 1**: Rapper 1 generates verse → TTS audio → Plays
10. **Turn 2**: Rapper 2 responds → TTS audio → Plays
11. Repeat for configured number of turns (default: 3)
12. AI judge analyzes debate
13. Winner announced with reasoning
14. User can start new debate or share results

### **Secondary Flow: Diagnostics Check**
1. User navigates to `/diag`
2. Click "Run All Checks" or individual tests
3. View service health status
4. Click "🔊 Test Audio" to verify TTS
5. Review console output for debugging
6. Address any failures shown in results table

---

## 📊 Data Models

### **Rapper**
```csharp
- Name: string
- ImageUrl: string
- Biography: string
- Style: string (e.g., "fast-paced", "storytelling")
```

### **DebateTurn**
```csharp
- TurnNumber: int
- RapperName: string
- Verse: string
- AudioData: byte[]
- Timestamp: DateTime
```

### **DebateResult**
```csharp
- Winner: string
- Reasoning: string
- Rapper1Score: int
- Rapper2Score: int
```

### **Topic**
```csharp
- Title: string
- Description: string
- Source: string (NewsAPI or Custom)
```

---

## 🔐 Security & Configuration

### **User Secrets (Development)**
Stored via `dotnet user-secrets`:
- `Azure:OpenAI:Endpoint`
- `Azure:OpenAI:ApiKey`
- `Azure:OpenAI:DeploymentName`
- `Azure:Speech:Region`
- `Azure:Speech:SubscriptionKey`
- `Azure:StorageConnectionString`
- `NewsApi:ApiKey`

### **Production Configuration**
- Azure App Service Application Settings
- Azure Key Vault integration (recommended)
- Managed Identity for Azure services

---

## 🧪 Testing Strategy

### **Unit Tests** (`PoDebateRap.UnitTests`)
- Service layer logic (NewsService, etc.)
- Model validation
- Helper functions

### **Integration Tests** (`PoDebateRap.IntegrationTests`)
- Azure OpenAI service integration
- Azure Speech service (TTS)
- Table Storage CRUD operations
- NewsAPI integration
- Health check endpoints

### **System Tests** (`PoDebateRap.SystemTests`)
- End-to-end Playwright tests
- Audio playback verification
- Button state validation
- Full debate flow simulation

### **API Tests** (`PoDebateRap.ApiTests`)
- Controller endpoint validation
- Request/response contracts
- Error handling

---

## 📈 Success Metrics

### **Technical Metrics**
- API response time: < 2 seconds (excluding AI generation)
- Audio generation time: < 3 seconds per verse
- Uptime: 99.5%
- Test coverage: > 70%

### **User Experience Metrics**
- Debate completion rate: > 80%
- Average debate duration: 2-5 minutes
- Audio playback success rate: > 95%
- User error rate: < 5%

---

## 🚀 Future Enhancements

### **Phase 1: Core Features (✅ Complete)**
- Basic debate flow
- AI integration
- TTS audio generation

### **Phase 2: Enhanced UX**
- Social sharing features
- Debate history/archives
- User accounts and profiles
- Leaderboard and statistics

### **Phase 3: Advanced AI**
- Multi-language support
- Custom rapper creation
- Audience voting system
- Live debate mode with multiple participants

### **Phase 4: Monetization**
- Premium rappers/voices
- Extended debate rounds
- Ad-free experience
- Custom topic packs

---

## 📝 Compliance & Legal

- **Content Moderation**: AI-generated content filtered for inappropriate language
- **Attribution**: Proper credit to rapper personas (educational/entertainment use)
- **API Usage**: Compliance with Azure, OpenAI, and NewsAPI terms of service
- **Data Privacy**: No personal user data collected (GDPR compliant)
- **Accessibility**: WCAG 2.1 Level AA compliance for UI components

---

## 👥 Stakeholders

- **Product Owner**: Project lead and vision holder
- **Development Team**: Full-stack developers (.NET, Blazor, Azure)
- **QA Team**: Test automation and quality assurance
- **DevOps**: CI/CD pipeline and Azure infrastructure management
- **End Users**: Hip-hop enthusiasts, educators, AI/tech enthusiasts

---

*Last Updated: October 5, 2025*
