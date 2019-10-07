namespace Components.Interfaces {
	public interface IScrollable {
		/// <summary>
		/// returns a value between 0 and 1, indicating current scroll value
		/// </summary>
		float ScrollProgress { get; }
	}
}