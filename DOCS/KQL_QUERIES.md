# Essential KQL Queries for PoDebateRap Application Insights

This document contains the 3 essential KQL (Kusto Query Language) queries for monitoring the fundamental health and performance of the PoDebateRap application in Azure Application Insights.

## Table of Contents
1. [User Activity (Last 7 Days)](#user-activity-last-7-days)
2. [Top 10 Slowest Requests](#top-10-slowest-requests)
3. [Error Rate (Last 24 Hours)](#error-rate-last-24-hours)

---

## 1. User Activity (Last 7 Days)

**Purpose**: Track active users and sessions over the past week to understand user engagement and traffic patterns.

**Business Value**: 
- Identify peak usage times
- Track user growth trends
- Detect anomalous traffic patterns
- Plan infrastructure scaling

**Query**:
```kql
// User Activity: Active users/sessions over the last 7 days
// Shows daily active users, page views, and unique sessions
customEvents
| where timestamp > ago(7d)
| where name in ("PageView", "DebateStarted", "DebateCompleted")
| summarize 
    UniqueUsers = dcount(user_Id),
    UniqueSessions = dcount(session_Id),
    TotalEvents = count(),
    PageViews = countif(name == "PageView"),
    DebatesStarted = countif(name == "DebateStarted"),
    DebatesCompleted = countif(name == "DebateCompleted")
    by bin(timestamp, 1d)
| project 
    Date = format_datetime(timestamp, 'yyyy-MM-dd'),
    UniqueUsers,
    UniqueSessions,
    TotalEvents,
    PageViews,
    DebatesStarted,
    DebatesCompleted,
    CompletionRate = round(todouble(DebatesCompleted) / todouble(DebatesStarted) * 100, 2)
| order by Date desc
```

**Expected Output Columns**:
- `Date`: The day (YYYY-MM-DD format)
- `UniqueUsers`: Number of unique users that day
- `UniqueSessions`: Number of unique sessions
- `TotalEvents`: Total number of tracked events
- `PageViews`: Number of page view events
- `DebatesStarted`: Number of debates initiated
- `DebatesCompleted`: Number of debates finished
- `CompletionRate`: Percentage of debates that were completed

**Interpretation**:
- **Healthy Pattern**: Steady or growing unique users/sessions
- **Warning Signs**: 
  - Sudden drops in unique users (potential outage or UX issue)
  - Low completion rate (< 50%) indicates users abandoning debates
  - High page views but low debates started (discovery vs. conversion issue)

**Alternative: Hourly Breakdown for Recent Activity**
```kql
// Last 24 hours with hourly breakdown
customEvents
| where timestamp > ago(1d)
| summarize 
    Users = dcount(user_Id),
    Sessions = dcount(session_Id),
    Events = count()
    by bin(timestamp, 1h)
| render timechart
```

---

## 2. Top 10 Slowest Requests

**Purpose**: Identify performance bottlenecks by finding the slowest server-side operations.

**Business Value**:
- Detect performance regressions
- Prioritize optimization efforts
- Identify problematic endpoints
- Improve user experience

**Query**:
```kql
// Top 10 Slowest Requests: Server-side performance analysis
// Identifies endpoints with the highest response times
requests
| where timestamp > ago(1d)
| where success == true  // Focus on successful requests (failed ones may skew data)
| summarize 
    RequestCount = count(),
    AvgDuration = round(avg(duration), 2),
    P50Duration = round(percentile(duration, 50), 2),
    P95Duration = round(percentile(duration, 95), 2),
    P99Duration = round(percentile(duration, 99), 2),
    MaxDuration = round(max(duration), 2)
    by name, url
| where RequestCount > 5  // Filter out rarely-called endpoints
| top 10 by P95Duration desc
| project 
    EndpointName = name,
    URL = url,
    RequestCount,
    AvgDurationMs = AvgDuration,
    P50Ms = P50Duration,
    P95Ms = P95Duration,
    P99Ms = P99Duration,
    MaxMs = MaxDuration,
    PerformanceGrade = case(
        P95Duration < 100, "Excellent",
        P95Duration < 500, "Good",
        P95Duration < 1000, "Fair",
        P95Duration < 3000, "Poor",
        "Critical"
    )
```

**Expected Output Columns**:
- `EndpointName`: HTTP endpoint name (e.g., "GET Health/status")
- `URL`: Full URL path
- `RequestCount`: Number of requests in the time period
- `AvgDurationMs`: Average response time in milliseconds
- `P50Ms`: Median response time (50th percentile)
- `P95Ms`: 95th percentile response time (95% of requests faster than this)
- `P99Ms`: 99th percentile response time
- `MaxMs`: Maximum response time observed
- `PerformanceGrade`: Performance classification

**Interpretation**:
- **Target SLA**: P95 < 500ms for most endpoints, < 1000ms for complex operations
- **Warning Signs**:
  - P95 > 1000ms indicates user-perceivable slowness
  - Large gap between P50 and P95 suggests inconsistent performance
  - High P99/Max values indicate outliers (investigate specific requests)

**Performance Grades**:
- **Excellent** (< 100ms): Imperceptible delay
- **Good** (100-500ms): Acceptable for most operations
- **Fair** (500-1000ms): Noticeable but tolerable
- **Poor** (1000-3000ms): Frustrating user experience
- **Critical** (> 3000ms): Urgent optimization needed

**Detailed Investigation Query**:
```kql
// For a specific slow endpoint, get individual slow requests
requests
| where timestamp > ago(1d)
| where name == "GET /api/debate/generate-turn"  // Replace with specific endpoint
| where duration > 1000  // Requests slower than 1 second
| project timestamp, duration, resultCode, customDimensions
| order by duration desc
| take 20
```

---

## 3. Error Rate (Last 24 Hours)

**Purpose**: Monitor application stability by tracking the percentage of failed requests.

**Business Value**:
- Detect production incidents immediately
- Track error trends and patterns
- Measure service reliability (SLA compliance)
- Identify problematic code paths

**Query**:
```kql
// Error Rate: Percentage of failed requests in the last 24 hours
// Breaks down by hour and error type for trend analysis
let timeRange = 24h;
let errorThreshold = 5.0;  // Alert if error rate > 5%
requests
| where timestamp > ago(timeRange)
| summarize 
    TotalRequests = count(),
    FailedRequests = countif(success == false),
    ServerErrors = countif(resultCode >= 500 and resultCode < 600),
    ClientErrors = countif(resultCode >= 400 and resultCode < 500)
    by bin(timestamp, 1h)
| extend 
    ErrorRate = round(todouble(FailedRequests) / todouble(TotalRequests) * 100, 2),
    ServerErrorRate = round(todouble(ServerErrors) / todouble(TotalRequests) * 100, 2),
    ClientErrorRate = round(todouble(ClientErrors) / todouble(TotalRequests) * 100, 2)
| project 
    Hour = format_datetime(timestamp, 'yyyy-MM-dd HH:00'),
    TotalRequests,
    FailedRequests,
    ServerErrors,
    ClientErrors,
    ErrorRate,
    ServerErrorRate,
    ClientErrorRate,
    HealthStatus = case(
        ErrorRate == 0, "✓ Healthy",
        ErrorRate < errorThreshold, "⚠ Warning",
        "❌ Critical"
    )
| order by Hour desc
```

**Expected Output Columns**:
- `Hour`: Time bucket (hourly)
- `TotalRequests`: Total HTTP requests
- `FailedRequests`: Requests with success=false
- `ServerErrors`: HTTP 5xx errors (server-side issues)
- `ClientErrors`: HTTP 4xx errors (client-side issues)
- `ErrorRate`: Overall percentage of failures
- `ServerErrorRate`: Percentage of server errors
- `ClientErrorRate`: Percentage of client errors
- `HealthStatus`: Visual health indicator

**Interpretation**:
- **Target SLA**: < 1% error rate (99%+ success rate)
- **Warning Threshold**: 1-5% error rate
- **Critical Threshold**: > 5% error rate

**Error Type Analysis**:
- **Server Errors (5xx)**: Application bugs, dependency failures, timeouts - **requires immediate attention**
- **Client Errors (4xx)**: Invalid requests, authentication issues - **monitor for patterns but less urgent**

**Current Overall Error Rate**:
```kql
// Simple current error rate for dashboard
requests
| where timestamp > ago(24h)
| summarize 
    Total = count(),
    Failed = countif(success == false)
| extend ErrorRate = round(todouble(Failed) / todouble(Total) * 100, 2)
| project ErrorRate, Total, Failed, SuccessRate = 100 - ErrorRate
```

**Error Details by Endpoint**:
```kql
// Which endpoints are failing?
requests
| where timestamp > ago(24h)
| where success == false
| summarize 
    FailureCount = count(),
    UniqueResultCodes = make_set(resultCode)
    by name
| order by FailureCount desc
| take 10
```

**Exception Correlation**:
```kql
// Link errors to specific exceptions
requests
| where timestamp > ago(24h)
| where success == false
| join kind=inner (
    exceptions
    | where timestamp > ago(24h)
) on operation_Id
| summarize 
    ErrorCount = count(),
    SampleErrors = take_any(outerMessage, 3)
    by problemId, name
| order by ErrorCount desc
| take 10
```

---

## How to Use These Queries

### In Azure Portal:
1. Navigate to your Application Insights resource: `appi-PoDebateRap-*`
2. Click on **Logs** in the left sidebar
3. Paste any of the above queries into the query editor
4. Click **Run** to execute
5. Use **Pin to dashboard** to add to your monitoring dashboard

### Creating Alerts:
Based on these queries, set up the following alerts:

1. **High Error Rate Alert**:
   - Condition: Error rate > 5% for 5 minutes
   - Action: Email/SMS to on-call engineer

2. **Slow Performance Alert**:
   - Condition: P95 response time > 2000ms
   - Action: Create work item for investigation

3. **Low Activity Alert**:
   - Condition: No events for 1 hour during business hours
   - Action: Check for outage

### Dashboard Recommendations:
Create an Azure Dashboard with:
- **Tile 1**: Current error rate (big number)
- **Tile 2**: Active users today vs. yesterday
- **Tile 3**: Top 5 slowest endpoints (table)
- **Tile 4**: Error rate trend (line chart, last 24h)
- **Tile 5**: Request volume (bar chart, last 7 days)

---

## Advanced Queries

### Custom Event Analysis (Debate-Specific)
```kql
// Analyze debate completion patterns
customEvents
| where timestamp > ago(7d)
| where name == "DebateCompleted"
| extend 
    Winner = tostring(customDimensions.Winner),
    TotalTurns = toint(customMeasurements.TotalTurns),
    Duration = todouble(customMeasurements.DurationSeconds)
| summarize 
    DebateCount = count(),
    AvgTurns = avg(TotalTurns),
    AvgDuration = avg(Duration)
    by Winner
| order by DebateCount desc
```

### AI Model Performance
```kql
// Track AI model response times and token usage
customEvents
| where timestamp > ago(1d)
| where name == "AIModelUsage"
| extend 
    Model = tostring(customDimensions.ModelName),
    Operation = tostring(customDimensions.Operation),
    Tokens = toint(customMeasurements.TokenCount),
    ResponseTime = todouble(customMeasurements.ResponseTimeMs)
| summarize 
    Calls = count(),
    TotalTokens = sum(Tokens),
    AvgTokens = avg(Tokens),
    AvgResponseMs = avg(ResponseTime),
    P95ResponseMs = percentile(ResponseTime, 95)
    by Model, Operation
| order by Calls desc
```

### Storage Performance
```kql
// Monitor Azure Table Storage operations
customEvents
| where timestamp > ago(1d)
| where name == "StorageOperation"
| extend 
    Operation = tostring(customDimensions.Operation),
    Success = tobool(customDimensions.Success),
    Duration = todouble(customMeasurements.DurationMs)
| summarize 
    Count = count(),
    SuccessRate = round(100.0 * countif(Success) / count(), 2),
    AvgDurationMs = round(avg(Duration), 2),
    P95DurationMs = round(percentile(Duration, 95), 2)
    by Operation
| order by Count desc
```

---

## Troubleshooting Tips

### No Data Appearing?
1. Verify Application Insights connection string is configured
2. Check that `AddApplicationInsightsTelemetry()` is called in Program.cs
3. Ensure Serilog is configured with ApplicationInsights sink
4. Wait 1-2 minutes for telemetry to propagate

### High Noise in Logs?
- Adjust Serilog minimum level in appsettings.json
- Use `MinimumLevel.Override` for specific namespaces
- Filter out health check requests in queries: `| where name !contains "health"`

### Performance Impact?
- Application Insights sampling automatically reduces data at high volumes
- Custom telemetry is asynchronous and has minimal impact
- Consider sampling configuration if > 100k events/day

---

## References

- [Kusto Query Language Documentation](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [Application Insights Query Examples](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/examples)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [PoDebateRap Health Checks](../Server/PoDebateRap.ServerApi/HealthChecks/)

**Last Updated**: Phase 4 Implementation - October 28, 2025
