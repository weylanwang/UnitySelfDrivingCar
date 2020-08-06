using System;

public class NeuralNetwork {
    #region Variables
    //The expected number of inputs and outputs
    uint inputs = 0, outputs = 0;

    //The individual neural layers which form the neural network
    NeuralLayer[] layers;
    #endregion

    #region Constructors
    //The constructor for a neural network with random weights given the dimensions
    public NeuralNetwork(uint[] sizes, Int32 seed) {
        layers = new NeuralLayer[sizes.Length - 1];
        for (int i = 0; i < layers.Length; i++)
            layers[i] = new NeuralLayer(sizes[i + 1], sizes[i], seed, false);
    }

    //The constructor for a neural network given a string representation of each layer
    public NeuralNetwork(string network) {
        //String representation of neural network:
        //Line 0 = Number of neural layers
        //Line 2......n: Descriptions of the neural layers
        //First line of singular neural layer: x, y representing the dimensions of the weight matrix
        //Next x lines: the y values per line representing individual weights to a singular neuron
        //Repeat the two above lines until termination of neural network

        // For example
        // 2  <--- the number of layers in this neural network
        // 2,3  <--- the dimensions of layer one
        // 1,2,3  <--- the weights of the first neuron set
        // 11,21,31  <--- the weights of the second neuron set
        // 1,2  <--- the dimensions of layer two
        // 2,3  <--- the weights of the first neuron set

        //Variable for tracking which layer we're creating
        uint layerCounter = 0;

        //Variable for building the string representation of a single neural layer
        string layerString = "";

        //Variables for tracking dimensions of each neural layer
        uint numRows = 0;
        uint numCols = 0;

        //Index for tracking position within a layer
        int layerIndex = 0; //0 means we're looking at the size of the weight matrix

        bool firstline = true;

        //At index 0, note the dimensions of the current neural layerIndex
        //For the next numRows lines, create the string representation of a neural layerIndex
        //Once done layer creation, add the layer to the neural network
        foreach (string line in network.Trim().Split('\n')) {
            if (firstline) {
                firstline = false;
                //Trims the first value of the string and uses it as the network size
                uint size = StringToUInt(line);
                layers = new NeuralLayer[size];
                continue;
            }
            if (layerIndex == 0) {
                string[] dimensions = line.Trim().Split(',');
                numRows = StringToUInt(dimensions[0]);
                numCols = StringToUInt(dimensions[1]);

                if (inputs == 0)
                    inputs = numCols - 1;
                outputs = numRows;

                layerString = "";
                layerIndex++;
                continue;
            }

            layerString += line + '\n';
            layerIndex++;
            if (layerIndex > numRows) {
                NeuralLayer layer = new NeuralLayer(layerString, numRows, numCols);
                layers[layerCounter++] = layer;
                numRows = 0; numCols = 0; layerIndex = 0;
            }
        }
    }
    #endregion

    #region Neural Network Functionality
    //Returns an output vector from the neural network given an input vector
    public float[] NetworkProcessing(float[] inputs) {
        float[] output = inputs;
        foreach (NeuralLayer layer in layers)
            output = layer.ProcessInputs(output);
        return output;
    }
    #endregion

    #region Utility Functions
    public override string ToString() {
        //Add layers.Length to beginning of string
        string neuralNetwork = layers.Length.ToString() + "\n";

        // Add layer dimensions and weights in each layer
        for (int i = 0; i < layers.Length; i++)
            neuralNetwork += layers[i].ToString();
        return neuralNetwork;
    }

    public uint StringToUInt(string value) {
        return Convert.ToUInt32(value.Trim());
    }
    #endregion
}