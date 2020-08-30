using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    float recombinationChance = 0.33f;
    int[] layerSizes = new int[] { 6, 4, 3, 2 };

    NeuralLayer aFourBySeven;
    NeuralLayer aThreeByFive;
    NeuralLayer aTwoByFour;
    NeuralLayer bFourBySeven;
    NeuralLayer bThreeByFive;
    NeuralLayer bTwoByFour;

    float[,] a1 = new float[4,7] { {1, 2, 3, 4, 5, 6, 7}, { 10, 20, 30, 40, 50, 60, 70 }, { 100, 200, 300, 400, 500, 600, 700 }, { 1000, 2000, 3000, 4000, 5000, 6000, 7000 } };
    float[,] a2 = new float[3, 5] { { 1, 2, 3, 4, 5 }, { 10, 20, 30, 40, 50 }, { 100, 200, 300, 400, 500 } };
    float[,] a3 = new float[2, 4] { { 1, 2, 3, 4 }, { 10, 20, 30, 40 } };

    float[,] b1 = new float[4, 7] { { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 6.1f, 7.1f }, { 10.1f, 20.1f, 30.1f, 40.1f, 50.1f, 60.1f, 70.1f },
        { 100.1f, 200.1f, 300.1f, 400.1f, 500.1f, 600.1f, 700.1f }, { 1000.1f, 2000.1f, 3000.1f, 4000.1f, 5000.1f, 6000.1f, 7000.1f } };
    float[,] b2 = new float[3, 5] { { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f }, { 10.1f, 20.1f, 30.1f, 40.1f, 50.1f }, { 100.1f, 200.1f, 300.1f, 400.1f, 500.1f } };
    float[,] b3 = new float[2, 4] { { 1.1f, 2.1f, 3.1f, 4.1f }, { 10.1f, 20.1f, 30.1f, 40.1f } };

    // Start is called before the first frame update
    void Start()
    {
        aFourBySeven = new NeuralLayer(a1);
        aThreeByFive = new NeuralLayer(a2);
        aTwoByFour = new NeuralLayer(a3);

        bFourBySeven = new NeuralLayer(b1);
        bThreeByFive = new NeuralLayer(b2);
        bTwoByFour = new NeuralLayer(b3);

        NeuralLayer[] aLayers = new NeuralLayer[] { aFourBySeven, aThreeByFive, aTwoByFour };
        NeuralLayer[] bLayers = new NeuralLayer[] { bFourBySeven, bThreeByFive, bTwoByFour };

        NeuralNetwork aNN = new NeuralNetwork(aLayers);
        NeuralNetwork bNN = new NeuralNetwork(bLayers);

        NeuralLayer temp = aFourBySeven.DeepCopy();
        //Debug.Log(temp.ToString());

        //Debug.Log(aNN);
        //Debug.Log(bNN);

        NeuralNetwork result = CrossOver(aNN, bNN);
        //Debug.Log(result.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //private NeuralNetwork StochasticHelper(NeuralNetwork n1, NeuralNetwork n2, int stochasticInterval) {
    //    NeuralLayer[] tempLayers = new NeuralLayer[layerSizes.Length - 1];
    //    int counter = UnityEngine.Random.Range(0, 101);
    //    int totalScore = 0;
    //    int[] chances = new int[parentsList.Length];

    //    for (int i = 0; i < parentsList.Length; i++)
    //        totalScore += parentsList[i].Score;

    //    for (int i = 0; i < parentsList.Length; i++)
    //        chances[i] = Mathf.RoundToInt(parentsList[i].Score / totalScore * 100);

    //    Array.Reverse(chances);

    //    for (int i = 0; i < layerSizes.Length - 1; i++) {
    //        float[,] tempWeights = new float[layerSizes[i + 1], layerSizes[i] + 1];
    //        for (int j = 0; j < layerSizes[i + 1]; j++)
    //            for (int k = 0; k < layerSizes[i] + 1; k++) {

    //                counter += stochasticInterval;
    //                counter = counter % totalScore;
    //                for (int l = 0; l < chances.Length; l++) {
    //                    if (counter < chances[l]) {
    //                        tempWeights[j, k] = parentsList[parentsList.Length - l].Brain.Layers[i].BiasedWeights[j, k];
    //                        break;
    //                    }
    //                }
    //            }

    //        tempLayers[i] = new NeuralLayer(tempWeights);
    //    }
    //    return new NeuralNetwork(tempLayers);
    //}

    public NeuralNetwork CrossOver(NeuralNetwork n1, NeuralNetwork n2)
    {
        NeuralLayer[] tempLayers = new NeuralLayer[layerSizes.Length - 1];

        //Debug.Log("Layer Sizes is 4 = " + layerSizes.Length);
        //Debug.Log("Neural Network number of layers is 3 = " + n1.Layers.Length);
        //for (int i = 0; i < n1.Layers.Length; i++)
        //    Debug.Log("Dimesions for this neural layers is expected to be " + (layerSizes[i] + 1) + " by " + layerSizes[i + 1] + " = " +
        //        n1.Layers[i].BiasedWeights.GetLength(0) + " by " + n1.Layers[i].BiasedWeights.GetLength(1));

        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            float[,] tempWeights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            for (int j = 0; j < layerSizes[i + 1]; j++)
                for (int k = 0; k < layerSizes[i] + 1; k++)
                {
                    if (Random.value < recombinationChance)
                    {
                        tempWeights[j, k] = n2.Layers[i].BiasedWeights(j, k);
                        //Debug.Log(n2.Layers[i].BiasedWeights[j, k]);
                    }
                    else
                    {
                        tempWeights[j, k] = n1.Layers[i].BiasedWeights(j, k);
                        //Debug.Log(n1.Layers[i].BiasedWeights[j, k]);
                    }
                }

            tempLayers[i] = new NeuralLayer(tempWeights);
            //Debug.Log(tempLayers[i].ToString());
        }
        NeuralNetwork tempNetwork = new NeuralNetwork(tempLayers);
        //Debug.Log(tempNetwork.ToString());

        return tempNetwork;
    }

  
}
