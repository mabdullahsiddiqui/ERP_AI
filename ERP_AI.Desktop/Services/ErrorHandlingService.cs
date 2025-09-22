using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ERP_AI.Desktop.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly Dictionary<string, ServiceHealthStatus> _serviceHealth = new();
        private readonly Dictionary<string, CircuitBreakerState> _circuitStates = new();
        private readonly Dictionary<string, DateTime> _lastFailureTimes = new();
        private readonly Dictionary<string, int> _consecutiveFailures = new();
        private readonly object _lock = new object();

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }

        // Retry Configuration
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
        public double BackoffMultiplier { get; set; } = 2.0;

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = "")
        {
            var policy = new RetryPolicy
            {
                MaxAttempts = MaxRetryAttempts,
                BaseDelay = BaseDelay,
                MaxDelay = MaxDelay,
                BackoffMultiplier = BackoffMultiplier,
                UseExponentialBackoff = true,
                UseJitter = true,
                RetryableExceptions = new List<Type>
                {
                    typeof(HttpRequestException),
                    typeof(TaskCanceledException),
                    typeof(TimeoutException)
                },
                RetryableStatusCodes = new List<HttpStatusCode>
                {
                    HttpStatusCode.RequestTimeout,
                    HttpStatusCode.TooManyRequests,
                    HttpStatusCode.InternalServerError,
                    HttpStatusCode.BadGateway,
                    HttpStatusCode.ServiceUnavailable,
                    HttpStatusCode.GatewayTimeout
                }
            };

            return await ExecuteWithRetryAsync(operation, policy, operationName);
        }

        public async Task ExecuteWithRetryAsync(Func<Task> operation, string operationName = "")
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true;
            }, operationName);
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, RetryPolicy policy, string operationName = "")
        {
            var result = new RetryResult<T>
            {
                Attempts = 0,
                TotalDuration = TimeSpan.Zero
            };

            var startTime = DateTime.UtcNow;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
            {
                result.Attempts = attempt;

                try
                {
                    // Check circuit breaker
                    if (await IsCircuitOpenAsync(operationName))
                    {
                        throw new InvalidOperationException($"Circuit breaker is open for operation: {operationName}");
                    }

                    var operationResult = await operation();
                    result.Success = true;
                    result.Result = operationResult;
                    result.TotalDuration = DateTime.UtcNow - startTime;

                    // Record success
                    await RecordSuccessAsync(operationName);

                    _logger.LogInformation("Operation {OperationName} succeeded on attempt {Attempt}", operationName, attempt);
                    return operationResult;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    result.AllExceptions.Add(ex);
                    result.LastException = ex;

                    _logger.LogWarning(ex, "Operation {OperationName} failed on attempt {Attempt}: {ErrorMessage}", 
                        operationName, attempt, ex.Message);

                    // Check if we should retry
                    if (!ShouldRetry(ex, policy, attempt))
                    {
                        break;
                    }

                    // Record failure
                    await RecordFailureAsync(operationName, ex);

                    // Wait before retry (except on last attempt)
                    if (attempt < policy.MaxAttempts)
                    {
                        var delay = CalculateDelay(attempt, policy);
                        _logger.LogInformation("Waiting {Delay}ms before retry {NextAttempt} for {OperationName}", 
                            delay.TotalMilliseconds, attempt + 1, operationName);
                        await Task.Delay(delay);
                    }
                }
            }

            result.TotalDuration = DateTime.UtcNow - startTime;
            result.Success = false;

            _logger.LogError(lastException, "Operation {OperationName} failed after {Attempts} attempts", 
                operationName, result.Attempts);

            throw lastException ?? new InvalidOperationException($"Operation {operationName} failed after {result.Attempts} attempts");
        }

        private bool ShouldRetry(Exception exception, RetryPolicy policy, int attempt)
        {
            if (attempt >= policy.MaxAttempts)
                return false;

            // Check custom retry condition
            if (policy.CustomRetryCondition != null)
                return policy.CustomRetryCondition(exception);

            // Check retryable exceptions
            if (policy.RetryableExceptions.Any(t => t.IsInstanceOfType(exception)))
                return true;

            // Check HTTP status codes
            if (exception is HttpRequestException httpEx && httpEx.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)httpEx.Data["StatusCode"]!;
                if (policy.RetryableStatusCodes.Contains(statusCode))
                    return true;
            }

            // Default retryable conditions
            return IsRetryableError(exception);
        }

        private TimeSpan CalculateDelay(int attempt, RetryPolicy policy)
        {
            var delay = policy.BaseDelay;

            if (policy.UseExponentialBackoff)
            {
                delay = TimeSpan.FromMilliseconds(
                    delay.TotalMilliseconds * Math.Pow(policy.BackoffMultiplier, attempt - 1));
            }

            // Apply jitter to prevent thundering herd
            if (policy.UseJitter)
            {
                var jitter = Random.Shared.NextDouble() * 0.1; // 0-10% jitter
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));
            }

            return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, policy.MaxDelay.TotalMilliseconds));
        }

        // Error Classification
        public bool IsRetryableError(Exception exception)
        {
            return IsNetworkError(exception) || 
                   IsTimeoutError(exception) || 
                   IsServerError(exception);
        }

        public bool IsNetworkError(Exception exception)
        {
            return exception is HttpRequestException ||
                   exception is TaskCanceledException ||
                   exception is SocketException ||
                   exception is WebException;
        }

        public bool IsTimeoutError(Exception exception)
        {
            return exception is TaskCanceledException taskEx && taskEx.InnerException is TimeoutException ||
                   exception is TimeoutException ||
                   (exception is HttpRequestException httpEx && 
                    httpEx.Data.Contains("StatusCode") && 
                    (HttpStatusCode)httpEx.Data["StatusCode"]! == HttpStatusCode.RequestTimeout);
        }

        public bool IsServerError(Exception exception)
        {
            if (exception is HttpRequestException httpEx && httpEx.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)httpEx.Data["StatusCode"]!;
                return (int)statusCode >= 500;
            }
            return false;
        }

        public bool IsClientError(Exception exception)
        {
            if (exception is HttpRequestException httpEx && httpEx.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)httpEx.Data["StatusCode"]!;
                return (int)statusCode >= 400 && (int)statusCode < 500;
            }
            return false;
        }

        // Error Handling
        public async Task HandleErrorAsync(Exception exception, string context = "")
        {
            await LogErrorAsync(exception, context);
            await ShowUserFriendlyErrorAsync(exception, context);
        }

        public async Task LogErrorAsync(Exception exception, string context = "", Dictionary<string, object>? properties = null)
        {
            var logProperties = new Dictionary<string, object>
            {
                ["Context"] = context,
                ["ExceptionType"] = exception.GetType().Name,
                ["Message"] = exception.Message,
                ["StackTrace"] = exception.StackTrace ?? string.Empty
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    logProperties[prop.Key] = prop.Value;
                }
            }

            _logger.LogError(exception, "Error in {Context}: {Message}", context, exception.Message);

            // In a real application, you might also send to external logging service
            await Task.CompletedTask;
        }

        public async Task ShowUserFriendlyErrorAsync(Exception exception, string context = "")
        {
            string userMessage;

            if (IsNetworkError(exception))
            {
                userMessage = "Unable to connect to the server. Please check your internet connection and try again.";
            }
            else if (IsTimeoutError(exception))
            {
                userMessage = "The request timed out. Please try again.";
            }
            else if (IsServerError(exception))
            {
                userMessage = "The server is temporarily unavailable. Please try again later.";
            }
            else if (IsClientError(exception))
            {
                userMessage = "There was an error with your request. Please check your input and try again.";
            }
            else
            {
                userMessage = "An unexpected error occurred. Please try again or contact support if the problem persists.";
            }

            // In a real application, you would show this in the UI
            _logger.LogInformation("User-friendly error message: {Message}", userMessage);

            await Task.CompletedTask;
        }

        // Circuit Breaker
        public async Task<bool> IsCircuitOpenAsync(string operation)
        {
            lock (_lock)
            {
                if (!_circuitStates.ContainsKey(operation))
                {
                    _circuitStates[operation] = CircuitBreakerState.Closed;
                    return false;
                }

                var state = _circuitStates[operation];
                if (state == CircuitBreakerState.Open)
                {
                    // Check if enough time has passed to try half-open
                    if (_lastFailureTimes.ContainsKey(operation))
                    {
                        var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTimes[operation];
                        if (timeSinceLastFailure > TimeSpan.FromMinutes(5)) // 5 minute timeout
                        {
                            _circuitStates[operation] = CircuitBreakerState.HalfOpen;
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            }
        }

        public async Task RecordSuccessAsync(string operation)
        {
            lock (_lock)
            {
                if (!_serviceHealth.ContainsKey(operation))
                {
                    _serviceHealth[operation] = new ServiceHealthStatus
                    {
                        ServiceName = operation,
                        LastCheck = DateTime.UtcNow
                    };
                }

                var health = _serviceHealth[operation];
                health.IsHealthy = true;
                health.LastSuccess = DateTime.UtcNow;
                health.SuccessCount++;
                health.LastCheck = DateTime.UtcNow;

                _circuitStates[operation] = CircuitBreakerState.Closed;
                _consecutiveFailures[operation] = 0;
            }
            await Task.CompletedTask;
        }

        public async Task RecordFailureAsync(string operation, Exception exception)
        {
            lock (_lock)
            {
                if (!_serviceHealth.ContainsKey(operation))
                {
                    _serviceHealth[operation] = new ServiceHealthStatus
                    {
                        ServiceName = operation,
                        LastCheck = DateTime.UtcNow
                    };
                }

                var health = _serviceHealth[operation];
                health.IsHealthy = false;
                health.LastFailure = DateTime.UtcNow;
                health.FailureCount++;
                health.LastCheck = DateTime.UtcNow;

                _consecutiveFailures[operation] = _consecutiveFailures.GetValueOrDefault(operation, 0) + 1;
                _lastFailureTimes[operation] = DateTime.UtcNow;

                // Open circuit if too many consecutive failures
                if (_consecutiveFailures[operation] >= 5)
                {
                    _circuitStates[operation] = CircuitBreakerState.Open;
                    health.CircuitState = CircuitBreakerState.Open;
                }
            }
            await Task.CompletedTask;
        }

        public async Task ResetCircuitAsync(string operation)
        {
            lock (_lock)
            {
                _circuitStates[operation] = CircuitBreakerState.Closed;
                _consecutiveFailures[operation] = 0;

                if (_serviceHealth.ContainsKey(operation))
                {
                    _serviceHealth[operation].CircuitState = CircuitBreakerState.Closed;
                }
            }
            await Task.CompletedTask;
        }

        // Health Monitoring
        public async Task<ServiceHealthStatus> GetServiceHealthAsync(string serviceName)
        {
            lock (_lock)
            {
                if (!_serviceHealth.ContainsKey(serviceName))
                {
                    return new ServiceHealthStatus
                    {
                        ServiceName = serviceName,
                        IsHealthy = false,
                        LastCheck = DateTime.UtcNow
                    };
                }

                return _serviceHealth[serviceName];
            }
        }

        public async Task<List<ServiceHealthStatus>> GetAllServicesHealthAsync()
        {
            lock (_lock)
            {
                return _serviceHealth.Values.ToList();
            }
        }

        public async Task<bool> IsServiceHealthyAsync(string serviceName)
        {
            var health = await GetServiceHealthAsync(serviceName);
            return health.IsHealthy;
        }

        // Recovery Actions
        public async Task<bool> AttemptRecoveryAsync(string operation, Exception lastError)
        {
            _logger.LogInformation("Attempting recovery for operation: {Operation}", operation);

            // Reset circuit breaker
            await ResetCircuitAsync(operation);

            // Wait a bit before retrying
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Check if service is back online
            return await IsServiceHealthyAsync(operation);
        }

        public async Task<bool> FallbackToOfflineModeAsync()
        {
            _logger.LogInformation("Falling back to offline mode");
            // Implementation for offline mode fallback
            return true;
        }

        public async Task<bool> ReconnectToServiceAsync(string serviceName)
        {
            _logger.LogInformation("Attempting to reconnect to service: {ServiceName}", serviceName);
            
            // Reset circuit breaker
            await ResetCircuitAsync(serviceName);
            
            // Wait before attempting reconnection
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            return await IsServiceHealthyAsync(serviceName);
        }
    }
}
