using System;
using System.IO;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Losses;
using Tensorflow.Keras.Optimizers;
using Tensorflow.NumPy;
using static Tensorflow.KerasApi;

namespace ia
{
    public class ActorNetwork
    {
        private Sequential model;

        private const string MODEL_DIR = "models";
        private const string WEIGHTS_PATH = "models/actor.weights";

        public ActorNetwork(int stateSize = 5, int actionSize = 3)
        {
            model = keras.Sequential();

            model.add(new Dense(new DenseArgs
            {
                Units = 128,
                Activation = keras.activations.Relu,
                InputShape = new Shape(stateSize)
            }));

            model.add(new Dense(new DenseArgs
            {
                Units = 128,
                Activation = keras.activations.Relu
            }));

            model.add(new Dense(new DenseArgs
            {
                Units = actionSize,
                Activation = keras.activations.Tanh
            }));

            model.compile(
                optimizer: new Adam(learning_rate: 0.0001f),
                loss: new MeanSquaredError()
            );

            Load();
        }

        // =====================
        // PRÉDICTION (SAFE)
        // =====================
        public float[] Predict(float[] state)
        {
            float[,] input = new float[1, state.Length];
            for (int i = 0; i < state.Length; i++)
                input[0, i] = state[i];

            NDArray npState = np.array(input);

            // 🔴 predict retourne Tensors
            var tensors = model.predict(npState);

            // ✅ On récupère le premier tensor
            NDArray pred = tensors[0].numpy();

            float[] action = new float[pred.shape[1]];
            for (int i = 0; i < action.Length; i++)
                action[i] = (float)pred[0, i];

            return action;
        }



        public void Train(NDArray states, NDArray actions)
        {
            model.fit(states, actions, batch_size: (int)states.shape[0], epochs: 1);
        }

        // =====================
        // SAUVEGARDE / CHARGEMENT
        // =====================
        public void Save()
        {
            Directory.CreateDirectory(MODEL_DIR);
            model.save_weights(WEIGHTS_PATH);
        }

        private void Load()
        {
            if (File.Exists(WEIGHTS_PATH))
            {
                model.load_weights(WEIGHTS_PATH);
                Console.WriteLine("✔ Modèle chargé");
            }
            else
            {
                Console.WriteLine("ℹ Aucun modèle existant, démarrage à zéro");
            }
        }
    }
}
