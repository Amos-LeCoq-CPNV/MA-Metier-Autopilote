using ia;
using SimConnect.NET;
using System;
using System.Threading.Tasks;
using Tensorflow.NumPy;

namespace ia
{
    public static class Trainer
    {
        public static async Task Run()
        {
            var client = new SimConnectClient();
            await client.ConnectAsync();
            await Task.Delay(500);

            var env = new GliderEnv(client);
            var actor = new ActorNetwork();
            var buffer = new ReplayBuffer();
            float[] state = env.Reset();
            Random rnd = new();

            for (int step = 0; step < 200_000; step++)
            {
                var action = actor.Predict(state);

                // Exploration (bruit)
                for (int i = 0; i < action.Length; i++)
                    action[i] = Math.Clamp(action[i] + (float)(rnd.NextDouble() * 0.2 - 0.1), -1f, 1f);

                var (nextState, reward, done) = env.Step(action);

                buffer.Add(state, action, reward, nextState);

                if (step % 32 == 0 && step > 0)
                {
                    var (batchStates, batchActions, batchRewards, batchNextStates) = buffer.Sample();

                    // Convertir batchActions (NDArray) en float[,] pour pouvoir le modifier
                    int batchSize = (int)batchActions.shape[0];
                    int actionSize = (int)batchActions.shape[1];
                    float[,] targetArray = new float[batchSize, actionSize];

                    for (int i = 0; i < batchSize; i++)
                        for (int j = 0; j < actionSize; j++)
                            targetArray[i, j] = (float)batchActions[i, j]; // ✅ juste caster

                    NDArray targetActions = np.array(targetArray);


                    //NDArray targetActions = np.copy(batchActions);

                    for (int i = 0; i < batchRewards.shape[0]; i++)
                        for (int j = 0; j < targetActions.shape[1]; j++)
                            targetActions[i, j] += batchRewards[i] * 0.05f;

                    actor.Train(batchStates, targetActions);
                    actor.Save(); // sauvegarde automatique
                }

                state = nextState;

                // Redémarrage si crash
                if (done)
                    state = env.Reset();
            }
        }
    }
}
