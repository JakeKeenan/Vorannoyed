using System;
using System.Collections.Generic;
using System.Text;

namespace Vorannoyed
{
    internal static class GaussianElim
    {
        public enum SolutionResult
        {
            OneSolution,
            InfiniteSolutions,
            NoSolution
        }

        // To check whether infinite solutions  
        // exists or no solution exists 
        static SolutionResult CheckConsistency(float[,] a, int n)
        {
            int i, j;
            float sum;

            // flag == 2 for infinite solution 
            // flag == 3 for No solution 
            SolutionResult result = SolutionResult.NoSolution;
            for (i = 0; i < n; i++)
            {
                sum = 0;
                for (j = 0; j < n; j++)
                    sum = sum + a[i, j];
                if (sum == a[i, j])
                    result = SolutionResult.InfiniteSolutions;
            }
            return result;
        }

        // function to reduce matrix to reduced 
        // row echelon form. 
        public static SolutionResult Solve(float[,] a, int n)
        {
            int i, j, k = 0, c;
            bool issueFlag = false;

            // Performing elementary operations 
            for (i = 0; i < n; i++)
            {
                if (a[i, i] == 0)
                {
                    c = 1;
                    while ((i + c) < n && a[i + c, i] == 0)
                        c++;
                    if ((i + c) == n)
                    {
                        issueFlag = true;
                        break;
                    }
                    for (j = i, k = 0; k <= n; k++)
                    {
                        float temp = a[j, k];
                        a[j, k] = a[j + c, k];
                        a[j + c, k] = temp;
                    }
                }

                for (j = 0; j < n; j++)
                {

                    // Excluding all i == j 
                    if (i != j)
                    {

                        // Converting Matrix to reduced row 
                        // echelon form(diagonal matrix) 
                        float p = a[j, i] / a[i, i];

                        for (k = 0; k <= n; k++)
                            a[j, k] = a[j, k] - (a[i, k]) * p;
                    }
                }
            }
            if (issueFlag == true)
            {
                return CheckConsistency(a, n);
            }
            return SolutionResult.OneSolution;
        }
    }
}
