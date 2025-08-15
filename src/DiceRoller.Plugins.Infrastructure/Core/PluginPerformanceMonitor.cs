using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Default implementation of plugin performance monitor
    /// </summary>
    public class PluginPerformanceMonitor : IPluginPerformanceMonitor
    {
        private readonly ILogger<PluginPerformanceMonitor> _logger;
        private readonly ConcurrentDictionary<string, OperationStatsTracker> _operationStats = new();
        private readonly ConcurrentDictionary<string, MetricStatsTracker> _metricStats = new();
        private readonly DateTime _startTime = DateTime.UtcNow;
        private DateTime _lastActivity = DateTime.UtcNow;

        public PluginPerformanceMonitor(ILogger<PluginPerformanceMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable StartOperation(string operationName)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));

            _lastActivity = DateTime.UtcNow;
            return new OperationTracker(operationName, this);
        }

        public void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null)
        {
            if (string.IsNullOrEmpty(metricName))
                return;

            _lastActivity = DateTime.UtcNow;

            _metricStats.AddOrUpdate(metricName,
                _ => new MetricStatsTracker(metricName, value),
                (_, existing) => existing.AddValue(value));

            _logger.LogDebug("Recorded metric {MetricName}: {Value}", metricName, value);
        }

        public PluginPerformanceStats GetStats()
        {
            var operationStats = new Dictionary<string, OperationStats>();
            var metricStats = new Dictionary<string, MetricStats>();

            foreach (var (name, tracker) in _operationStats)
            {
                operationStats[name] = tracker.GetStats();
            }

            foreach (var (name, tracker) in _metricStats)
            {
                metricStats[name] = tracker.GetStats();
            }

            return new PluginPerformanceStats(
                operationStats,
                metricStats,
                DateTime.UtcNow - _startTime,
                _lastActivity);
        }

        internal void RecordOperation(string operationName, TimeSpan duration)
        {
            _operationStats.AddOrUpdate(operationName,
                _ => new OperationStatsTracker(operationName, duration),
                (_, existing) => existing.AddOperation(duration));
        }

        private class OperationTracker : IDisposable
        {
            private readonly string _operationName;
            private readonly PluginPerformanceMonitor _monitor;
            private readonly Stopwatch _stopwatch;
            private bool _disposed = false;

            public OperationTracker(string operationName, PluginPerformanceMonitor monitor)
            {
                _operationName = operationName;
                _monitor = monitor;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (_disposed) return;

                _stopwatch.Stop();
                _monitor.RecordOperation(_operationName, _stopwatch.Elapsed);
                _disposed = true;
            }
        }

        private class OperationStatsTracker
        {
            private readonly string _name;
            private long _count;
            private TimeSpan _totalTime;
            private TimeSpan _minTime = TimeSpan.MaxValue;
            private TimeSpan _maxTime = TimeSpan.MinValue;
            private readonly object _lock = new();

            public OperationStatsTracker(string name, TimeSpan initialDuration)
            {
                _name = name;
                AddOperation(initialDuration);
            }

            public OperationStatsTracker AddOperation(TimeSpan duration)
            {
                lock (_lock)
                {
                    _count++;
                    _totalTime += duration;
                    if (duration < _minTime) _minTime = duration;
                    if (duration > _maxTime) _maxTime = duration;
                }
                return this;
            }

            public OperationStats GetStats()
            {
                lock (_lock)
                {
                    var averageTime = _count > 0 ? TimeSpan.FromTicks(_totalTime.Ticks / _count) : TimeSpan.Zero;
                    return new OperationStats(_name, _count, _totalTime, averageTime, _minTime, _maxTime);
                }
            }
        }

        private class MetricStatsTracker
        {
            private readonly string _name;
            private long _count;
            private double _sum;
            private double _min = double.MaxValue;
            private double _max = double.MinValue;
            private readonly object _lock = new();

            public MetricStatsTracker(string name, double initialValue)
            {
                _name = name;
                AddValue(initialValue);
            }

            public MetricStatsTracker AddValue(double value)
            {
                lock (_lock)
                {
                    _count++;
                    _sum += value;
                    if (value < _min) _min = value;
                    if (value > _max) _max = value;
                }
                return this;
            }

            public MetricStats GetStats()
            {
                lock (_lock)
                {
                    var average = _count > 0 ? _sum / _count : 0;
                    return new MetricStats(_name, _count, _sum, average, _min, _max);
                }
            }
        }
    }
}