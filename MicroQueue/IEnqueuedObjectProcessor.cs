namespace MicroQueue {
	public interface IEnqueuedObjectProcessor<T> {
		void Process(T obj);
	}
}