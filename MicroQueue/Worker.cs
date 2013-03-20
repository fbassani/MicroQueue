using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MicroQueue {
	public class Worker<T> {
	    private readonly string _name;
	    private readonly CancellationToken _cancellationToken;
		private readonly Func<IEnqueuedObjectProcessor<T>> _enqueuedObjectProcessorFactory;

		public Worker(string name, CancellationToken cancellationToken, Func<IEnqueuedObjectProcessor<T>> enqueuedObjectProcessorFactory) {
		    _name = name;
		    _cancellationToken = cancellationToken;
			_enqueuedObjectProcessorFactory = enqueuedObjectProcessorFactory;
		}

		public void DoWork(object state) {
			var queue = (BlockingCollection<T>)state;
            while (!_cancellationToken.IsCancellationRequested) {
		        try {
		            DequeueAndProcess(queue);
		        } catch (OperationCanceledException) {
                    TraceHelper.TraceMessage(GetTraceMessage("Operation canceled"));
                } catch (ThreadAbortException) {
                    TraceHelper.TraceMessage(GetTraceMessage("Aborted"));
		        } catch (Exception e) {
                    TraceHelper.TraceError(e);
		        }
		    }
		}

	    private void DequeueAndProcess(BlockingCollection<T> queue) {
	        var enqueuedObject = queue.Take(_cancellationToken);
	        if (!_cancellationToken.IsCancellationRequested) {
	            var processorObject = _enqueuedObjectProcessorFactory();
	            string obj = enqueuedObject.ToString();
                TraceHelper.TraceMessage(GetTraceMessage("Processing " + obj));
	            processorObject.Process(enqueuedObject);
                TraceHelper.TraceMessage(GetTraceMessage("Finished processing " + obj));
	        }
	    }

        private string GetTraceMessage(string message) {
            return String.Format("[{0}] {1}", _name, message);
        }
	}
}