using System;
using System.Collections.Generic;

public static class Utilities {
    static Random rng = new Random();

    public static List<int> GetNRandomIndices(int numElements, int n) {
        List<int> selectedIndices = new List<int>();
//        int numElementsRemaining = numElements;
        int numIndicesNeeded = n;
        for (int i = 0; i < numElements; i++) {
            int numElementsRemaining = numElements - i;
            float selectionProbability = ((float) numIndicesNeeded) / numElementsRemaining;
            float randomFloat = (float) rng.NextDouble();
            if (randomFloat <= selectionProbability) {
                selectedIndices.Add(i);
                numIndicesNeeded--;
            }
            if (numIndicesNeeded == 0) {
                break;
            }
        }
        return selectedIndices;
    }

    
}
