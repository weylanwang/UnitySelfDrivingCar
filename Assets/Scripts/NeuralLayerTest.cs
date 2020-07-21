using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralLayerTest : MonoBehaviour
{

    NeuralLayer[] layers = new NeuralLayer[3];

    // A 4 by 3 float double array
    float[,] testFloat = new float [,] { { 1, 2, 3 }, { 11, 12, 13 }, { 21, 22, 23 }, { 31, 32, 33 } };

    // A 2 by 5 string representation of a float double array
    string testString = "10.1,10.2,10.3,10.4,10.5\n" + "20.1,20.2,20.3,20.4,20.5\n";

    // Start is called before the first frame update
    void Start()
    {
        RunTest();
    }

    private void RunTest()
    {
        // Test Constructors
        layers[0] = new NeuralLayer(3, 5);
        layers[1] = new NeuralLayer(testFloat);
        layers[2] = new NeuralLayer(testString, 2, 5);

        Debug.Log(layers[0].ToString());
        Debug.Log(layers[1].ToString());
        Debug.Log(layers[2].ToString());
    }
}
