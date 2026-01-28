using Tensorflow.NumPy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ia
{
    public class ReplayBuffer
    {
        private List<float[]> states = new();
        private List<float[]> actions = new();
        private List<float> rewards = new();
        private List<float[]> nextStates = new();
        private Random rnd = new();

        public void Add(float[] s, float[] a, float r, float[] ns)
        {
            states.Add(s);
            actions.Add(a);
            rewards.Add(r);
            nextStates.Add(ns);
        }

        public (NDArray, NDArray, NDArray, NDArray) Sample(int batchSize = 32)
        {
            int count = Math.Min(batchSize, states.Count);
            var idx = new HashSet<int>();
            while (idx.Count < count)
                idx.Add(rnd.Next(states.Count));

            var batchStates = idx.Select(i => states[i]).ToArray();
            var batchActions = idx.Select(i => actions[i]).ToArray();
            var batchRewards = idx.Select(i => rewards[i]).ToArray();
            var batchNextStates = idx.Select(i => nextStates[i]).ToArray();

            return (
                np.array(batchStates),
                np.array(batchActions),
                np.array(batchRewards),
                np.array(batchNextStates)
            );
        }
    }
}
