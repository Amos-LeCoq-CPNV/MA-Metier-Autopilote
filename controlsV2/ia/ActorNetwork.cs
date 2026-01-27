using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

public class ActorNetwork
{
    private Model model;

    public ActorNetwork()
    {
        var inputs = keras.Input(shape: 4);

        var x = new Dense(128, activation: "relu").Apply(inputs);
        x = new Dense(128, activation: "relu").Apply(x);

        var outputs = new Dense(3, activation: "tanh").Apply(x);

        model = keras.Model(inputs, outputs);
        model.compile(
            optimizer: keras.optimizers.Adam(0.0001f),
            loss: "mse"
        );
    }

    public float[] Predict(float[] state)
    {
        var npState = np.array(new float[][] { state });
        var pred = model.predict(npState);
        return pred[0].ToArray<float>();
    }

    public void Train(NDArray states, NDArray actions)
    {
        model.train_on_batch(states, actions);
    }

    public void TrainWithCritic(CriticNetwork critic, NDArray states)
    {
        using var tape = tf.GradientTape();
        tape.watch(model.trainable_variables);

        var actions = model.Apply(states);
        var qValues = critic.model.Apply(new Tensors(states, actions));
        var loss = -tf.reduce_mean(qValues);

        var grads = tape.gradient(loss, model.trainable_variables);
        keras.optimizers.Adam(0.0001f)
            .apply_gradients(zip(grads, model.trainable_variables));
    }
}