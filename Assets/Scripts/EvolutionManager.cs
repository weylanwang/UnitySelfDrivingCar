using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

[RequireComponent(typeof(AgentManager))]
public class EvolutionManager : MonoBehaviour {
    #region Private Variables
    [Header("Evolution Settings")]

    [SerializeField]
    [Tooltip("The total number of generations for the program to run")]
    [Range(1, 30)]
    private uint totalGenerations;
    private static uint currentGeneration;

    [SerializeField]
    [Tooltip("Best2/Best2Alt: The probability a child chooses the better parent's weight. In BestAlt2, it reduces variety.")]
    [Range(0, 0.5f)]
    private float recombinationChance;
    private static float staticRecombinationChance;

    [SerializeField]
    [Tooltip("Roulette Wheel: The number of cars passing their AI to the next generation")]
    [Range(2, 10)]
    private int parentsPerGeneration;
    private static int staticParentsPerGeneration;

    [SerializeField]
    [Tooltip("The probability a child's weight mutates.")]
    [Range(0, 1)]
    private float mutationChance;
    private static float staticMutationChance;

    [SerializeField]
    [Tooltip("One: Defines the boundaries for the limits of mutation.")]
    private float mutationAmount;
    private static float staticMutationAmount;

    private AgentManager agentManager;
    private static int generationSize;
    private static uint[] layerSizes;
    private static Agent[] parentsList;
    private static NeuralNetwork[] newGeneration;
    private static System.Random random;

    [Header("UI Elements")]

    [SerializeField]
    [Tooltip("Generation Number Text")]
    private Text generationText;
    #endregion

    #region Delegates
    // This delegate encodes which recombination method we want to use
    public delegate void RecombinationFunction();
    public static RecombinationFunction Recombine = BestTwo;

    // This delegate encodes which mutation method we want to use
    public delegate float[,] MutationFunction(float[,] weights);
    public static MutationFunction Mutate = MutationOne;
    #endregion

    #region Recomination/Mutation Key
    /*
     * Recombination:
     * Best Two - Creates children where each weight is randomly selected from either parent
     * Best Two Weighted - A child randomly takes the weight from either parent, but the probability of inheritance is proportional to the parent's score
     * Best Two Alt - Creates children in pairs. A parent's weight is randomly assigned to a child while the other child get the remaining parent's weight
     * Truncated Roulette Wheel - A child randomly takes the weight from a pool of n parents, but the probability of inheritance is proportional to the parent's score
     * Stocastic Recombination - An attempt to use stochastic sampling for recombination rather than sampling
     * 
     * Mutation:
     * MutationOne - According to a set chance, a weight may be randomly reassigned to a value within range
     * MutationTwo - According to set chances, a weight may increase or decrease within two ranges, one larger than the other
     * MutationThree - According to a set chance, a weight may increase or decrease by a value within range
     * */
    #endregion

    #region StartUp
    // Assignment Region
    void Start() {
        agentManager = transform.GetComponent<AgentManager>();
        agentManager.SimulationEndedEvent += CreateNewGeneration;
        generationSize = agentManager.CarsPerGeneration;
        newGeneration = new NeuralNetwork[generationSize];
        layerSizes = agentManager.LayerSizes;
        random = new System.Random();
        staticRecombinationChance = recombinationChance;
        staticMutationChance = mutationChance;
        staticMutationAmount = mutationAmount;
        //staticParentsPerGeneration = parentsPerGeneration;
        staticParentsPerGeneration = 5;
        currentGeneration = 0;
    }

    // Initializes values
    // Called by CreateProject before Start()
    public void Initialize(uint genCount, float recombinationChance, float mutationChance, float mutationAmount, string algorithm, Text generationText)
    {
        this.totalGenerations = genCount;
        staticRecombinationChance = recombinationChance;
        this.recombinationChance = recombinationChance;
        staticMutationChance = mutationChance;
        this.mutationChance = mutationChance;
        staticMutationAmount = mutationAmount;
        this.mutationAmount = mutationAmount;
        this.generationText = generationText;

        if (algorithm == "RouletteWheel")
            Recombine = TruncatedRouletteWheel;
        else
            Recombine = BestTwo;
    }
    #endregion

    #region WaitForEvent
    // Create the new generation of Neural Networks
    private void CreateNewGeneration() {
        currentGeneration++;
        if (currentGeneration >= totalGenerations) {
            // Print Neural Networks and delete Agents when generation limit reached
            agentManager.PrintNeuralNetworks();
            agentManager.DestroyAgents();
            return;
        }

        // Print current generation
        Debug.Log("Starting Generation " + (currentGeneration + 1));
        generationText.text = (currentGeneration + 1).ToString();

        parentsList = agentManager.Parents;

        // Call Recombination Function to create new list of NN
        // Recombination Function also calls Mutation Function
        Recombine();

        // Delete remaining agents
        agentManager.DestroyAgents();

        // Begin Simulation of next generation
        agentManager.StartSimulation(newGeneration);

    }
    #endregion


    #region Recombination Methods

    #region BestTwo
    // Best Two - Creates children where each weight is randomly selected from either parent
    private static void BestTwo() {
        CarryOverBest2();

        // staticRecombinationChance determines when the child NN chooses their weight from the 2nd best parent
        for (int i = 2; i < generationSize; i++) {
            newGeneration[i] = Best2Helper(newGeneration[0], newGeneration[1], staticRecombinationChance);
        }
    }

    // Best Two Weighted - A child randomly takes the weight from either parent,
    // but the probability of inheritance is proportional to the parent's score
    private static void BestTwoWeighted() {
        CarryOverBest2();

        int score1 = parentsList[0].Score;
        int score2 = parentsList[1].Score;

        float weight = score2 / (score1 + score2);

        for (int i = 2; i < generationSize; i++) {
            newGeneration[i] = Best2Helper(newGeneration[0], newGeneration[1], weight);
        }
    }

    public static NeuralNetwork Best2Helper(NeuralNetwork n1, NeuralNetwork n2, float recombination) {
        NeuralLayer[] tempLayers = new NeuralLayer[layerSizes.Length - 1];

        for (int i = 0; i < layerSizes.Length - 1; i++) {
            float[,] tempWeights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            for (int j = 0; j < layerSizes[i + 1]; j++)
                for (int k = 0; k < layerSizes[i] + 1; k++) {
                    if (UnityEngine.Random.value < recombination)
                        tempWeights[j, k] = n2.Layers[i].BiasedWeights(j, k);
                    else
                        tempWeights[j, k] = n1.Layers[i].BiasedWeights(j, k);
                }
            tempWeights = Mutate(tempWeights);
            tempLayers[i] = new NeuralLayer(tempWeights);
        }
        return new NeuralNetwork(tempLayers);
    }

    // Best Two Alt - Creates children in pairs. A parent's weight is randomly assigned
    // to a child while the other child gets the remaining parent's weight
    // Therefore a smaller recombination chance will split the new generation into 2 groups,
    // with each group resembling one of the parents very closely
    private static void BestTwoAlternative() {
        CarryOverBest2();

        for (int i = 2; i < generationSize; i += 2) {
            NeuralNetwork[] children = Best2AltHelper(newGeneration[0], newGeneration[1], staticRecombinationChance);
            newGeneration[i] = children[0];
            if (i + 1 < generationSize)
                newGeneration[i + 1] = newGeneration[i + 1] = children[1];
        }
    }

    private static NeuralNetwork[] Best2AltHelper(NeuralNetwork n1, NeuralNetwork n2, float recombination) {
        NeuralNetwork[] childArray = new NeuralNetwork[2];

        NeuralLayer[] child1Layers = new NeuralLayer[layerSizes.Length - 1];
        NeuralLayer[] child2Layers = new NeuralLayer[layerSizes.Length - 1];


        for (int i = 0; i < layerSizes.Length - 1; i++) {
            float[,] child1Weights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            float[,] child2Weights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            for (int j = 0; j < layerSizes[i + 1]; j++) {
                for (int k = 0; k < layerSizes[i] + 1; k++) {
                    if (UnityEngine.Random.value < recombination) {
                        child1Weights[j, k] = n1.Layers[i].BiasedWeights(j, k);
                        child2Weights[j, k] = n2.Layers[i].BiasedWeights(j, k);
                    }
                    else {
                        child1Weights[j, k] = n2.Layers[i].BiasedWeights(j, k);
                        child2Weights[j, k] = n1.Layers[i].BiasedWeights(j, k);
                    }
                }
            }
            child1Weights = Mutate(child1Weights);
            child2Weights = Mutate(child2Weights);

            child1Layers[i] = new NeuralLayer(child1Weights);
            child2Layers[i] = new NeuralLayer(child2Weights);
        }

        childArray[0] = new NeuralNetwork(child1Layers);
        childArray[1] = new NeuralNetwork(child2Layers);

        return childArray;
    }
    #endregion

    #region Roulette Wheel
    // Truncated Roulette Wheel - A child randomly takes the weight from a pool of parentsPerGeneration parents,
    // but the probability of inheritance is proportional to the parent's score
    private static void TruncatedRouletteWheel() {
        // Truncation is done in agent manager

        // Check if parentsPerGeneration is greater than 2
        if (staticParentsPerGeneration < 2)
            throw new System.Exception("ParentsPerGeneration is less than 2!");

        CarryOverBest2();

        for (int i = 2; i < generationSize; i++) {
            newGeneration[i] = RouletteHelper(newGeneration[0], newGeneration[1]);
        }
    }

    private static NeuralNetwork RouletteHelper(NeuralNetwork n1, NeuralNetwork n2) {
        NeuralLayer[] tempLayers = new NeuralLayer[layerSizes.Length - 1];
        int runningTotal = 0;
        int totalScore = 0;

        for (int i = 0; i < parentsList.Length; i++)
            totalScore += parentsList[i].Score;

        for (int i = 0; i < layerSizes.Length - 1; i++) {
            float[,] tempWeights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            for (int j = 0; j < layerSizes[i + 1]; j++)
                for (int k = 0; k < layerSizes[i] + 1; k++)
                    for (int l = 0; l < parentsList.Length; l++) {
                        double indexValue = random.NextDouble() * totalScore;
                        runningTotal += parentsList[l].Score;
                        if (indexValue <= runningTotal) {
                            tempWeights[j, k] = parentsList[l].Brain.Layers[i].BiasedWeights(j, k);
                            runningTotal = 0;
                            break;
                        }
                    }

            tempWeights = Mutate(tempWeights);
            tempLayers[i] = new NeuralLayer(tempWeights);
        }
        return new NeuralNetwork(tempLayers);
    }
    #endregion

    #region Stochastic
    // Stocastic Recombination - An attempt to use stochastic sampling for recombination rather than sampling
    private static void StochasticRecombination() {
        CarryOverBest2();

        for (int i = 2; i < generationSize; i++) {
            newGeneration[i] = StochasticHelper(newGeneration[0], newGeneration[1], 2);
        }
    }

    private static NeuralNetwork StochasticHelper(NeuralNetwork n1, NeuralNetwork n2, int stochasticInterval) {
        NeuralLayer[] tempLayers = new NeuralLayer[layerSizes.Length - 1];
        int counter = UnityEngine.Random.Range(0, 101);
        int totalScore = 0;
        int[] chances = new int[parentsList.Length];

        for (int i = 0; i < parentsList.Length; i++)
            totalScore += parentsList[i].Score;

        for (int i = 0; i < parentsList.Length; i++)
            chances[i] = Mathf.RoundToInt(parentsList[i].Score / totalScore * 100);

        Array.Reverse(chances);

        for (int i = 0; i < layerSizes.Length - 1; i++) {
            float[,] tempWeights = new float[layerSizes[i + 1], layerSizes[i] + 1];
            for (int j = 0; j < layerSizes[i + 1]; j++)
                for (int k = 0; k < layerSizes[i] + 1; k++) {

                    counter += stochasticInterval;
                    counter = counter % totalScore;
                    for (int l = 0; l < chances.Length; l++) {
                        if (counter < chances[l]) {
                            tempWeights[j, k] = parentsList[parentsList.Length - l].Brain.Layers[i].BiasedWeights(j, k);
                            break;
                        }
                    }
                }
            tempWeights = Mutate(tempWeights);
            tempLayers[i] = new NeuralLayer(tempWeights);
        }
        return new NeuralNetwork(tempLayers);
    }
    #endregion

    #endregion

    #region Mutation Methods

    // According to a set chance, a weight may be randomly reassigned to a value within range
    private static float[,] MutationOne(float[,] weights) {
        for (int j = 0; j < weights.GetLength(0); j++) {
            for (int k = 0; k < weights.GetLength(1); k++) {
                if (UnityEngine.Random.value < staticMutationChance) {
                    weights[j, k] += UnityEngine.Random.Range(-staticMutationAmount, staticMutationAmount);
                }
            }
        }
        return weights;
    }

    // According to set chances, a weight may increase or decrease within two ranges, one larger than the other
    private static float[,] MutationTwo(float[,] weights) {
        float runningMax = 0;
        float runningMin = 0;
        float extremeMutateChance = 0.05f;
        float mutationPercentage = 0.3f;

        for (int i = 0; i < weights.GetLength(0); i++) {
            for (int j = 0; j < weights.GetLength(1); j++) {
                // This block keeps track of running min/max
                if (weights[i, j] > runningMax) runningMax = weights[i, j];
                else if (weights[i, j] < runningMin) runningMin = weights[i, j];

                // Mutate
                if (UnityEngine.Random.value < staticMutationChance) {
                    // Percentage based mutation
                    if (UnityEngine.Random.value > extremeMutateChance) {
                        if (UnityEngine.Random.value > 0.5f)
                            weights[i, j] *= (1 + (UnityEngine.Random.Range(0f, mutationPercentage)));
                        else
                            weights[i, j] *= (1 - (UnityEngine.Random.Range(0f, mutationPercentage)));
                    }
                    // Extreme Mutation based on min/max
                    else
                        weights[i, j] = UnityEngine.Random.Range(runningMin, runningMax);
                }
            }
        }
        return weights;
    }

    // According to a set chance, a weight may increase or decrease by a value within range
    private static float[,] MutationThree(float[,] weights) {
        // Mutate values according to average for that layer
        int col = weights.GetLength(1);
        float mutationAmount = 1f / col;

        for (int i = 0; i < weights.GetLength(0); i++) {
            for (int j = 0; j < col; j++) {
                if (UnityEngine.Random.value < staticMutationChance)
                    weights[i, j] += UnityEngine.Random.Range(-mutationAmount, mutationAmount);
            }
        }
        return weights;
    }
    #endregion

    #region Control
    // The 2 best cars are carried over from every past generation
    private static void CarryOverBest2() {
        // The minimum number of cars per generation is 3. This check is added for redundancy
        Assert.IsTrue(parentsList.Length > 1);

        // Best 2 cars are copied straight over as a control and to combat devolution
        newGeneration[0] = parentsList[0].Brain.DeepCopy();
        newGeneration[1] = parentsList[1].Brain.DeepCopy();
    }
    #endregion

    #region OnDestroy
    private void OnDestroy() {
        agentManager.SimulationEndedEvent -= CreateNewGeneration;
    }
    #endregion
}