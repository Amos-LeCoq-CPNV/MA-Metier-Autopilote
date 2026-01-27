using NumSharp;
using Tensorflow.NumPy;
public class ReplayBuffer
{
    private List<float[]> states = new();
    private List<float[]> actions = new();
    private List<float> rewards = new();
    private List<float[]> nextStates = new();

    public void Add(float[] s, float[] a, float r, float[] ns)
    {
        states.Add(s);
        actions.Add(a);
        rewards.Add(r);
        nextStates.Add(ns);
    }

    public (NDArray, NDArray, NDArray, NDArray) Sample()
    {
        return (
            np.array(states.ToArray()),
            np.array(actions.ToArray()),
            np.array(rewards.ToArray()),
            np.array(nextStates.ToArray())
        );
    }
}
