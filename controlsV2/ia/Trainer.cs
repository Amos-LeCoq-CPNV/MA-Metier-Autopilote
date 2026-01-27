using NumSharp;
using SimConnect.NET;

public static class Trainer
{
    public static void Run()
    {
        var client = new SimConnectClient();
        var env = new GliderEnv(client);
        var actor = new ActorNetwork();
        var buffer = new ReplayBuffer();

        float[] state = env.Reset();

        for (int step = 0; step < 200000; step++)
        {
            // 1) Action depuis le réseau
            var action = actor.Predict(state);

            // 2) Step environnement
            var (nextState, reward, done) = env.Step(action);

            // 3) Stocker expérience
            buffer.Add(state, action);

            // 4) Entrainer si assez de données
            if (step % 32 == 0)
            {
                var (batchStates, batchActions) = buffer.Sample();
                actor.Train(batchStates, batchActions);
            }

            state = nextState;

            if (done)
                state = env.Reset();
        }
    }
}