using System.Net;

namespace ERP_AI.Desktop.Services
{
    public interface IErrorHandlingService
    {
        // Retry Configuration
        int MaxRetryAttempts { get; set; }
        TimeSpan BaseDelay { get; set; }
        TimeSpan MaxDelay { get; set; }
        double BackoffMultiplier { get; set; }

        // Retry Methods
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = "");
        Task ExecuteWithRetryAsync(Func<Task> operation, string operationName = "");
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, RetryPolicy policy, string operationName = "");

        // Error Classification
        bool IsRetryableError(Exception exception);
        bool IsNetworkError(Exception exception);
        bool IsTimeoutError(Exception exception);
        bool IsServerError(Exception exception);
        bool IsClientError(Exception exception);

        // Error Handling
        Task HandleErrorAsync(Exception exception, string context = "");
        Task LogErrorAsync(Exception exception, string context = "", Dictionary<string, object>? properties = null);
        Task ShowUserFriendlyErrorAsync(Exception exception, string context = "");

        // Circuit Breaker
        Task<bool> IsCircuitOpenAsync(string operation);
        Task RecordSuccessAsync(string operation);
        Task RecordFailureAsync(string operation, Exception exception);
        Task ResetCircuitAsync(string operation);

        // Health Monitoring
        Task<ServiceHealthStatus> GetServiceHealthAsync(string serviceName);
        Task<List<ServiceHealthStatus>> GetAllServicesHealthAsync();
        Task<bool> IsServiceHealthyAsync(string serviceName);

        // Recovery Actions
        Task<bool> AttemptRecoveryAsync(string operation, Exception lastError);
        Task<bool> FallbackToOfflineModeAsync();
        Task<bool> ReconnectToServiceAsync(string serviceName);
    }

    public class RetryPolicy
    {
        public int MaxAttempts { get; set; } = 3;
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
        public double BackoffMultiplier { get; set; } = 2.0;
        public bool UseExponentialBackoff { get; set; } = true;
        public bool UseJitter { get; set; } = true;
        public List<Type> RetryableExceptions { get; set; } = new();
        public List<HttpStatusCode> RetryableStatusCodes { get; set; } = new();
        public Func<Exception, bool>? CustomRetryCondition { get; set; }
    }

    public class ServiceHealthStatus
    {
        public string ServiceName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public DateTime LastCheck { get; set; }
        public DateTime LastSuccess { get; set; }
        public DateTime LastFailure { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate => SuccessCount + FailureCount > 0 ? (double)SuccessCount / (SuccessCount + FailureCount) : 0;
        public TimeSpan AverageResponseTime { get; set; }
        public List<string> Issues { get; set; } = new();
        public CircuitBreakerState CircuitState { get; set; } = CircuitBreakerState.Closed;
    }

    public enum CircuitBreakerState
    {
        Closed,     // Normal operation
        Open,       // Circuit is open, requests are blocked
        HalfOpen    // Testing if service is back
    }

    public class ErrorContext
    {
        public string Operation { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    public class RetryResult<T>
    {
        public bool Success { get; set; }
        public T? Result { get; set; }
        public Exception? LastException { get; set; }
        public int Attempts { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public List<Exception> AllExceptions { get; set; } = new();
    }
}
