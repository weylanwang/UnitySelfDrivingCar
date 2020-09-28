using System;
using System.Text;

public class NeuralLayer {
    #region Variables
    // Weight matrix for the neural layer.
    // For every weight [i,j], we are encoding the weight from 
    // This layer's jth node to the ith node in the next layer.
    private float[,] biasedWeights;
    public float BiasedWeights(int i, int j) { return biasedWeights[i, j]; }

    // Create a random variable that randomizes based on the seed
    private static System.Random random;

    // This delegate encodes which activation function we will be using
    public delegate float ActivationFunction(float inputValue);

    // Change this to whatever activation function we want to be testing
    public ActivationFunction NeuralActivation = SigmoidFunction;

    // This delegate encodes which randomization function we will be using 
    public delegate float NewRandomizer(float stdDev);

    // Change this to whatever randomization function we want to be testing
    public NewRandomizer CustomRandom = ThreePartsRandom;
    #endregion
   
    #region Constructors
    // This region contains the different types of constructors available for use

    // A constructor to be used when we want to create a random neural, specifying rows, columns,
    // and whether or not to use the custom randomizer 
    public NeuralLayer(uint row, uint col, Int32 seed, bool experimental = false) {
        random = new System.Random(seed);
        biasedWeights = new float[row, col + 1];
        float sd = 1f / col;
        for (int i = 0; i < biasedWeights.GetLength(0); i++) {
            for (int j = 0; j < biasedWeights.GetLength(1); j++)
                if (experimental)
                    biasedWeights[i, j] = CustomRandom(sd);
                else {
                    float temp = RandomRange(-3 * sd, 3 * sd);
                    biasedWeights[i, j] = temp;
                }
        }
    }

    // A constructor to be used when an existing weight matrix already exists
    // This constructor assumes biases have already been added and taken care of
    public NeuralLayer(float[,] initialWeight) {
        random = new System.Random();
        biasedWeights = initialWeight;
    }

    // A constructor to be used when creating a neural layer from a string representation of the weights
    public NeuralLayer(string initialWeight, uint row, uint col) {
        biasedWeights = new float[row, col];
        int i = 0, j = 0;
        foreach (string line in initialWeight.Trim().Split('\n')) {
            foreach (string value in line.Trim().Split(','))
                biasedWeights[i, j++] = (float)Convert.ToDouble(value);
            j = 0;
            i++;
        }
    }
    #endregion

    #region Math Functions
    // This region contains the various math functions necessary for the neural layer

    // This function multiplies a matrix by a single column vector
    private float[] MatrixVectorMultiplication(float[,] matrix, float[] vector) {
        // first argument is the rows, second argument is columns
        if (matrix == null || vector == null) {
            throw new ArgumentException("Inputs cannot be null. Currently matrix = " + matrix + " and vector = " + vector);
        }

        if (matrix.GetLength(1) != vector.GetLength(0)) {
            throw new ArgumentException("Matrix dimensions not compatible");
        }
        float[] result = new float[matrix.GetLength(0)];

        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                result[i] += matrix[i, j] * vector[j];
            }
        }
        return result;
    }

    // The Sigmoid activation function
    private static float SigmoidFunction(float inputValue) {
        return 1.0f / (1.0f + (float)Math.Exp(-inputValue));
    }

    // The Leaky Rectified Linear Unit function
    private static float LeakyReLU(float inputValue) {
        if (inputValue > 0) return inputValue;
        else return 0.01f * inputValue;
    }

    // A randomization function that imitates a standard bell curve by utilizing three divisions
    private static float ThreePartsRandom(float stdDev) {
        float[] possibleWeight = new float[3];
        possibleWeight[0] = RandomRange(-stdDev, stdDev);
        possibleWeight[1] = RandomRange(-2 * stdDev, 2 * stdDev);
        possibleWeight[2] = RandomRange(-3 * stdDev, 3 * stdDev);
        int pick = random.Next(0, 3);
        return possibleWeight[pick];
    }

    // A randomization function that returns a random value within the given bounds
    private static float RandomRange(float min, float max) {
        return ((float)random.NextDouble() * (max - min) + min);
    }
    #endregion

    #region Neural Processing
    // Computes outputs by multiplying the weight matrix by the input vector,
    // and then taking the raw results and running them through the activation function
    public float[] ProcessInputs(float[] inputs) {
        float[] biasedInputs = InputBiased(inputs);
        float[] results = MatrixVectorMultiplication(biasedWeights, biasedInputs);
        for (int i = 0; i < results.Length; i++) {
            // We multiply by 2 and subtract by 1 since activation functions return between 0 and 1
            results[i] = (NeuralActivation(results[i]))*2 - 1;
        }
        return results;
    }

    // A helper function to append a bias to the input vector
    private float[] InputBiased(float[] input) {
        float[] biasedInputs = new float[input.Length + 1];
        input.CopyTo(biasedInputs, 0);
        biasedInputs[input.Length] = 1.0f;
        return biasedInputs;
    }
    #endregion

    #region Utility Functions
    // A utility function to convert weights into string representation
    public override string ToString() {
        StringBuilder sb = new StringBuilder("", CalculateSpace());
        sb.Append(biasedWeights.GetLength(0) + "," + biasedWeights.GetLength(1) + "\n");
        for (int i = 0; i < biasedWeights.GetLength(0); i++) {
            sb.Append(biasedWeights[i, 0]);
            for (int j = 1; j < biasedWeights.GetLength(1); j++)
                sb.Append("," + biasedWeights[i, j].ToString());
            sb.Append("\n");
        }
        return sb.ToString();
    }

    // A helper function to calculate the size needed for the ToString stringbuilder
    private int CalculateSpace() {
        int count = 0;
        count += biasedWeights.GetLength(0) * (biasedWeights.GetLength(1) + 1);
        for (int i = 0; i < biasedWeights.GetLength(0); i++)
            for (int j = 0; j < biasedWeights.GetLength(1); j++)
                count += biasedWeights[i, j].ToString().Length;

        return count;
    }

    // A utility function to create a copy of this Neural Layer
    public NeuralLayer DeepCopy() {
        int rows = biasedWeights.GetLength(0);
        int cols = biasedWeights.GetLength(1);
        float[,] newWeights = new float[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                newWeights[i, j] = biasedWeights[i, j];

        return new NeuralLayer(newWeights);
    }

    // Return the specified dimension of the biasedWeights
    public int GetDimensions(int index) {
        if (index == 0 || index == 1)
            return biasedWeights.GetLength(index);
        return 0;
    }
    #endregion
}
