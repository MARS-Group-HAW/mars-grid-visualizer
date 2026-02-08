using MarsGridVisualizer.Domain;

namespace MarsGridVisualizer.Presentation;

public interface IRenderer
{
	/// <summary>
	/// Takes in some form of stateless state and creates Godot Nodes from them.
	/// </summary>
	public void Render(State state);
}
