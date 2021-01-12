public interface IUpdatable
{
	bool PauseWhileUnfocused { get; }
	void DoUpdate ();
}