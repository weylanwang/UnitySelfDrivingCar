using System;
using System.Diagnostics;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour {
    private void Start() {
        float[,] threeByFour = new float[3, 5] { { 0.2f, -0.48f, 0.75f, 0.6f, 0.35f }, { 0.18f, 0.002f, 0.31f, -0.1f, -0.9f }, { 0.62f, -0.082f, 0.43f, 0.9f, 0.46f } };
        float[,] twoByThree = new float[2, 4] { { 0.25f, 0.31f, -0.56f, 0.66f }, { -0.98f, -0.09f, -0.16f, -0.23f } };
        float[,] oneByTwo = new float[1, 3] { { 0.45f, 0.67f, -0.12f } };

        NeuralLayer[] layers = new NeuralLayer[3];
        layers[0] = new NeuralLayer(threeByFour);
        layers[1] = new NeuralLayer(twoByThree);
        layers[2] = new NeuralLayer(oneByTwo);

        float[] input1 = new float[4] { -1, 1, 0, 1 };
        float[] input2 = new float[4] { 0, 1, 0, 1 };
        float[] input3 = new float[4] { 1, 1, -1, -1 };

        string neuralNetString = "3\n" + layers[0].ToString() + layers[1].ToString() + layers[2].ToString();

        UnityEngine.Debug.Log(neuralNetString);

        var stopwatchWey = new Stopwatch();
        var stopwatchDan = new Stopwatch();
        var stopwatchNam = new Stopwatch();

        stopwatchDan.Start();
        NeuralNetwork danNet = new NeuralNetwork(neuralNetString, 1f);
        stopwatchDan.Stop();
        UnityEngine.Debug.Log("Dans code took " + stopwatchDan.ElapsedMilliseconds + " milliseconds");
        UnityEngine.Debug.Log(danNet.NetworkProcessing(input1)[0]);

        stopwatchWey.Start();
        NeuralNetwork weyNet = new NeuralNetwork(neuralNetString, 1);
        stopwatchWey.Stop();
        UnityEngine.Debug.Log("Weys code took " + stopwatchWey.ElapsedMilliseconds + " milliseconds");
        UnityEngine.Debug.Log(weyNet.NetworkProcessing(input1)[0]);

        stopwatchNam.Start();
        NeuralNetwork namNet = new NeuralNetwork(neuralNetString, false);
        stopwatchNam.Stop();
        UnityEngine.Debug.Log("Nams code took " + stopwatchNam.ElapsedMilliseconds + " milliseconds");
        UnityEngine.Debug.Log(namNet.NetworkProcessing(input1));
    }

    #region Variables
    //The expected number of inputs and outputs
    uint inputs = 0, outputs = 0;

    //The individual neural layers which form the neural network
    NeuralLayer[] layers;
    #endregion

    #region Constructors
    //The constructor for a neural network with random weights given the dimensions
    public NeuralNetwork(uint[] sizes) {
        //layerSizes = sizes; <-- Currently does nothing since layerSizes doesn't exist
        layers = new NeuralLayer[sizes.Length - 1];
        for (int i = 0; i < layers.Length; i++)
            layers[i] = new NeuralLayer(sizes[i + 1], sizes[i], false);
    }

    //The constructor for a neural network given a string representation of each layer
    public NeuralNetwork(string network, int identifier) {
        //Wey's Code

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

    public NeuralNetwork(string network, float identifier) {
        // Daniel's Code
        string[] lines = network.Trim().Split('\n');
        layers = new NeuralLayer[StringToUInt(lines[0])];
        int layerIndex = 0;
        uint rows, cols;
        for (int i = 1; i < lines.Length; i++) {
            string sb = "";
            string[] values = lines[i].Trim().Split(',');
            rows = StringToUInt(values[0]); cols = StringToUInt(values[1]);

            // The number of inputs is cols - 1 because the weights include bias
            if (inputs == 0)
                inputs = cols - 1;
            // The number of outputs is the last weight's rows
            outputs = rows;

            // Ignore line holding dimensions
            int temp = i + 1;

            for (int j = temp; j < temp + rows; j++)
                sb += lines[j] + '\n';

            layers[layerIndex++] = new NeuralLayer(sb, rows, cols);
            i += (int)rows;
        }
    }

    public NeuralNetwork(string network, bool identifier) {
        //Nam's Code 
        uint rows = 0, cols = 0;
        int track = 0, layerIndex = 0, sizesIndex = 0;
        string[] builds = network.Trim().Split('\n');
        layers = new NeuralLayer[StringToUInt(builds[0])];
        string entry = "";

        for (int i = 1; i <= builds.Length; i++) {
            if (track == rows) {
                if (i == 1) {
                    string[] temp = builds[i].Trim().Split(',');
                    rows = StringToUInt(temp[0]);
                    cols = StringToUInt(temp[1]);
                    //cols - 1 because weight string includes bias
                    if (inputs == 0) {
                        inputs = cols - 1;
                    }
                }
                else {
                    layers[layerIndex++] = new NeuralLayer(entry, rows, cols);
                    if (i != builds.Length) {
                        string[] temp = builds[i].Trim().Split(',');
                        rows = StringToUInt(temp[0]);
                        cols = StringToUInt(temp[1]);
                        track = 0;
                        entry = "";
                        outputs = rows;
                    }
                }
            }
            else {
                entry = entry + builds[i] + '\n';
                track++;
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