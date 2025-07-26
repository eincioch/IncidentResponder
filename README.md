# IncidentResponderDemo - .NET 9 MCP Integration

This solution demonstrates how GitHub Copilot Agent can work with custom Model Context Protocol (MCP) servers to help developers respond to production incidents more effectively.

## 🎯 Scenario

The **PaymentsService** has production issues with NullReferenceExceptions, while **OrdersService** is running smoothly. GitHub Copilot Agent uses MCP servers to:

1. **Query logs** to find error patterns and stack traces
2. **Analyze metrics** to understand service health and performance
3. **Identify code issues** by correlating logs with source code

## 🏗️ Architecture

```
IncidentResponderDemo/
├── 📁 Services/
│   ├── PaymentsService/          # 🐛 Contains deliberate bugs
│   └── OrdersService/            # ✅ Clean implementation
├── 📁 MCP/
│   ├── LogQueryServer/           # 🔍 Log analysis MCP server
│   └── MetricsServer/            # 📊 Metrics monitoring MCP server
├── 📁 Logs/
│   └── payments.log              # 📝 Sample production logs
├── .mcp.json                     # ⚙️ MCP server configuration
└── global.json                  # 🎯 .NET 9 SDK version
```

## 🚀 Getting Started

### Prerequisites
- .NET 9 Preview SDK
- Visual Studio 17.14+ with GitHub Copilot Agent
- MCP server support enabled

### Setup

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Start MCP servers:**
   ```bash
   # Terminal 1 - Log Query Server
   dotnet run --project MCP/LogQueryServer
   
   # Terminal 2 - Metrics Server  
   dotnet run --project MCP/MetricsServer
   ```

3. **Configure GitHub Copilot Agent** to use the MCP servers via `.mcp.json`

## 🔧 MCP Tools Available

### LogQueryServer
- `query_logs(service, search_term)` - Search logs for specific patterns
- `get_recent_logs(service, count)` - Get recent log entries

### MetricsServer  
- `get_metrics(service, time_range)` - Get system performance metrics
- `get_alert_status(service?)` - Check active alerts and incidents

## 🐛 Deliberate Bugs in PaymentsService

The `PaymentsProcessor` class contains several intentional issues:

1. **NullReferenceException** in `ProcessPayment()` - Missing null checks
2. **KeyNotFoundException** in `CalculateTransactionFee()` - Unsafe dictionary access  
3. **ArgumentNullException** scenarios - Poor parameter validation

## 📊 How GitHub Copilot Agent Helps

1. **Log Analysis**: Query logs to find error patterns
   ```
   @copilot query the PaymentsService logs for NullReference errors
   ```

2. **Metrics Investigation**: Check service health metrics
   ```  
   @copilot get metrics for PaymentsService to see error rates
   ```

3. **Code Correlation**: Link log errors to specific code lines
   ```
   @copilot analyze the PaymentsProcessor.cs file for null reference issues
   ```

4. **Solution Suggestions**: Get AI-powered fix recommendations
   ```
   @copilot suggest fixes for the null reference exceptions in PaymentsProcessor
   ```

## 📈 Expected Metrics

### PaymentsService (Problematic)
- Error Rate: ~15% (threshold: 10%)
- Response Time: ~900ms (threshold: 800ms)  
- NullReferenceExceptions: 10+ per hour
- CPU Usage: ~55% (elevated)

### OrdersService (Healthy)
- Error Rate: ~0.8% (well below threshold)
- Response Time: ~150ms (fast)
- Exceptions: <2 per hour
- CPU Usage: ~20% (normal)

## 🎓 Learning Objectives

This demo teaches:

- **MCP Integration**: How to build custom MCP servers for Visual Studio
- **AI-Assisted Debugging**: Using GitHub Copilot Agent with contextual data
- **Production Incident Response**: Systematic approach to investigating issues
- **Code Quality Patterns**: Contrasting buggy vs. clean implementations

## 🔍 Incident Response Workflow

1. **Alert Detection** → MCP metrics show high error rates
2. **Log Investigation** → MCP log server finds NullReference patterns  
3. **Code Analysis** → GitHub Copilot Agent identifies vulnerable code
4. **Solution Implementation** → AI suggests defensive programming fixes
5. **Verification** → Metrics confirm issue resolution

---

**Note**: This is a demo solution with simulated production data. The bugs are intentional for educational purposes.