using System;
using System.Diagnostics;

namespace MicroQueue {
    public static class TraceHelper {
        private static readonly TraceSource _source = new TraceSource("MicroQueue");

        public static void TraceMessage(string message) {
            _source.TraceEvent(TraceEventType.Information, 1, message);
        }

        public static void TraceError(Exception e) {
            _source.TraceEvent(TraceEventType.Error, 0, e.Message);
        }
    }
}