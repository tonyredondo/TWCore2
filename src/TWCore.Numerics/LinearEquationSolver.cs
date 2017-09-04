/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

#pragma warning disable 1591

using System;
using System.Collections.Generic;

namespace TWCore.Numerics
{
    /// <summary>
    /// Linear equation solver status
    /// </summary>
    public enum LinearEquationSolverStatus
    {
        Success,
        IllConditioned,
        Singular,
    };

    /// <summary>
    /// This class solves systems of linear equations.
    /// </summary>
    public class LinearEquationSolver
    {
        /// <summary>
        /// Smallest floating point value
        /// </summary>
        public static readonly double s_smallFloatingPointValue = 5.69E-14;

        /// <summary>
        /// This function solves simultaneous equations in matrix form.
        /// The equations are represented by the matrix equation:
        /// 
        ///     aMatrix xVector = bVector
        ///
        /// The algorithm is from the book:
        /// "Mathematical Methods For Digital Computers" Volume 2
        /// Edited by Anthony Ralston and Herbert S. Wilf, 1967,
        /// John Wiley and Sons, pages 65-93.
        /// "The solution of ill-conditioned linear equations"
        /// by J. H. Wilkinson.
        /// </summary>
        /// <param name="numberOfEquations">The number of equations</param>
        /// <param name="aMatrix">A square matrix</param>
        /// <param name="bVector">A vector that contains the right hand side of the matrix equations shown below.</param>
        /// <param name="xVector">A vector to contain the solution of the matrix equations.</param>
        /// <returns>
        /// This function returns an enumerated value of type Status_T
        /// that is the value LinearEquationSolverStatus.Singular if the
        /// coefficient matrix is singular to working accuracy. A value of
        /// LinearEquationSolverStatus.IllConditioned is returned if the
        /// coefficient matrix is singular to working accuracy, i.e. the
        /// floating point arithmetic does not have enough precision to
        /// guarantee convergence to an accurate solution.
        /// The value LinearEquationSolverStatus.Success is returned
        /// if the solution vector was calculated successfully.
        /// </returns>
        public static LinearEquationSolverStatus Solve(int numberOfEquations,
                                                       Sparse2DMatrix<int, int, double> aMatrix,
                                                       SparseArray<int, double> bVector,
                                                       SparseArray<int, double> xVector)
        {
            //----------------------------------------------------------
            // Matrix a_matrix is copied into working matrix aMatrixCopy.
            //----------------------------------------------------------

            Sparse2DMatrix<int, int, double> aMatrixCopy = new Sparse2DMatrix<int, int, double>(aMatrix);

            //----------------------------------------------------------
            // The maximum value rowMaximumVector[i], i = 0 to n - 1
            // is stored
            //----------------------------------------------------------

            SparseArray<int, double> rowMaximumVector = new SparseArray<int, double>();

            int i = 0;
            for (i = 0; i < numberOfEquations; i++)
            {
                double temp = 0.0;

                for (int j = 0; j < numberOfEquations; j++)
                {
                    double test = System.Math.Abs(aMatrix[i, j]);

                    if (test > temp)
                    {
                        temp = test;
                    }
                }

                rowMaximumVector[i] = temp;

                //----------------------------------------------------------
                // Test for singular matrix.
                //----------------------------------------------------------

                if (Math.Abs(temp) <= 0.0)
                {
                    return LinearEquationSolverStatus.Singular;
                }
            }

            //----------------------------------------------------------
            // The r'th column of "l", the r'th pivotal position r', and
            // the r'th row of "u" are determined.
            //----------------------------------------------------------

            SparseArray<int, int> pivotRowArray = new SparseArray<int, int>();

            for (int r = 0; r < numberOfEquations; r++)
            {
                double maximumValue = 0.0;
                int rowMaximumValueIndex = r;

                //----------------------------------------------------------
                // The l[i][r], i = r to n - 1 are determined.
                // l[i][r] is a lower triangular matrix. It is calculated
                // using the variable temp and copied into matrix
                // "aMatrixCopy". The variable "maximumValue" contains
                // the largest Math.Abs(l[i][r] / pivotRowArray[i]) and
                // rowMaximumValueIndex stores the "i" which corresponds
                // to the value in variable maximumValue.
                //----------------------------------------------------------

                double temp;

                for (i = r; i < numberOfEquations; i++)
                {
                    temp = aMatrixCopy[i, r];

                    for (int j = 0; j < r; j++)
                    {
                        temp = temp - aMatrixCopy[i, j] * aMatrixCopy[j, r];
                    }

                    aMatrixCopy[i, r] = temp;

                    double test = System.Math.Abs(temp / rowMaximumVector[i]);

                    if (test > maximumValue)
                    {
                        maximumValue = test;
                        rowMaximumValueIndex = i;
                    }
                }

                //----------------------------------------------------------
                // Test for matrix which is singular to working precision.
                //----------------------------------------------------------

                if (Math.Abs(maximumValue) <= 0.0)
                {
                    return LinearEquationSolverStatus.IllConditioned;
                }

                //----------------------------------------------------------
                // "rowMaximumValueIndex" = r' and "pivotRowArray[r]"
                // is the pivotal row.
                //----------------------------------------------------------

                rowMaximumVector[rowMaximumValueIndex] = rowMaximumVector[r];
                pivotRowArray[r] = rowMaximumValueIndex;

                //----------------------------------------------------------
                // Rows "r" and "pivotRowArray[r]" are exchanged.
                //----------------------------------------------------------

                for (i = 0; i < numberOfEquations; i++)
                {
                    temp = aMatrixCopy[r, i];
                    aMatrixCopy[r, i] = aMatrixCopy[rowMaximumValueIndex, i];
                    aMatrixCopy[rowMaximumValueIndex, i] = temp;
                }

                //----------------------------------------------------------
                // The u[r][i], i = r + 1 to n - 1 are determined.
                // "u[r][i]" is an upper triangular matrix. It is copied
                // into aMatrixCopy.
                //----------------------------------------------------------

                for (i = r + 1; i < numberOfEquations; i++)
                {
                    temp = aMatrixCopy[r, i];

                    for (int j = 0; j < r; j++)
                    {
                        temp = temp - aMatrixCopy[r, j] * aMatrixCopy[j, i];
                    }

                    aMatrixCopy[r, i] = temp / aMatrixCopy[r, r];
                }
            }

            //----------------------------------------------------------
            // The first solution vector is set equal to the null vector
            // and the first residuals are set equal to the equation
            // constant vector.
            //----------------------------------------------------------

            SparseArray<int, double> residualVector = new SparseArray<int, double>();

            for (i = 0; i < numberOfEquations; i++)
            {
                xVector[i] = 0.0;
                residualVector[i] = bVector[i];
            }

            //----------------------------------------------------------
            // The iteration counter is initialized.
            //----------------------------------------------------------

            int iteration = 0;
            bool notConvergedFlag = true;

            do
            {
                //----------------------------------------------------------
                // The forward substitution is performed and the solution
                // of l y = p r is calculated where p r is the current
                // residual after interchanges.
                //----------------------------------------------------------

                for (i = 0; i < numberOfEquations; i++)
                {
                    int pivotRowIndex = pivotRowArray[i];
                    double temp = residualVector[pivotRowIndex];
                    residualVector[pivotRowIndex] = residualVector[i];

                    for (int j = 0; j < i; j++)
                    {
                        temp = temp - aMatrixCopy[i, j] * residualVector[j];
                    }

                    residualVector[i] = temp / aMatrixCopy[i, i];
                }

                //----------------------------------------------------------
                // The back substitution is performed and the solution of
                // u e = y is calculated. The current correction is stored
                // in variable residualVector.
                //----------------------------------------------------------

                for (i = numberOfEquations - 1; i >= 0; i--)
                {
                    double temp = residualVector[i];

                    for (int j = i + 1; j < numberOfEquations; j++)
                    {
                        temp = temp - aMatrixCopy[i, j] * residualVector[j];
                    }

                    residualVector[i] = temp;
                }

                //----------------------------------------------------------
                // The norm of the error in the residuals and the norm of
                // the present solution vector are calculated.
                //----------------------------------------------------------

                double normOfX = 0.0;
                double normOfError = 0.0;

                for (i = 0; i < numberOfEquations; i++)
                {
                    double test = System.Math.Abs(xVector[i]);

                    if (test > normOfX)
                    {
                        normOfX = test;
                    }

                    test = System.Math.Abs(residualVector[i]);

                    if (test > normOfError)
                    {
                        normOfError = test;
                    }
                }

                //----------------------------------------------------------
                // If iteration is zero then skip this section because
                // no correction exists on the first iteration.
                //----------------------------------------------------------

                if (iteration != 0)
                {
                    //------------------------------------------------------
                    // Test for matrix which is singular to working
                    // precision.
                    //------------------------------------------------------

                    if ((iteration == 1) && (normOfError / normOfX > 0.5))
                    {
                        return LinearEquationSolverStatus.IllConditioned;
                    }

                    //------------------------------------------------------
                    // Check to see if "normOfError" / "normOfX" is greater
                    // than 2 ^ (1 - t), where "t" is the number of bits
                    // in the mantissa of a double precision number. If
                    // this is not true then the last correction is almost
                    // negligible and "notConvergedFlag" is set to false.
                    //------------------------------------------------------

                    notConvergedFlag = normOfError / normOfX >= s_smallFloatingPointValue;

#if DEBUGCODE
                    double normRatioForDebug = normOfError / normOfX;
#endif
                }

                //----------------------------------------------------------
                // The corrections (residuals) are added to the
                // solution vector.
                //----------------------------------------------------------

                for (i = 0; i < numberOfEquations; i++)
                {
                    xVector[i] = xVector[i] + residualVector[i];
                }

                //----------------------------------------------------------
                // The new residuals corresponding to the solution vector
                // are calculated.
                //----------------------------------------------------------

                for (i = 0; i < numberOfEquations; i++)
                {
                    double temp = bVector[i];

                    for (int j = 0; j < numberOfEquations; j++)
                    {
                        temp = temp - aMatrix[i, j] * xVector[j];
                    }

                    residualVector[i] = temp;
                }

                //----------------------------------------------------------
                // The iteration counter is incremented and the flag
                // "notConvergedFlag" is tested. If notConvergedFlag is false
                // then the solution vector has converged to sufficient
                // accuracy.
                //----------------------------------------------------------

                iteration++;
            }
            while (notConvergedFlag);

            return LinearEquationSolverStatus.Success;
        }

        /// <summary>
        /// Solve a group of linear equations
        /// </summary>
        /// <param name="equations">IEnumerable with the equations to solve</param>
        /// <param name="response">Dictionary with the equation results</param>
        /// <returns>Linear equation solver status value</returns>
        public static LinearEquationSolverStatus SolveEquations(IEnumerable<string> equations, Dictionary<string, double> response)
        {
            var aMatrix = new Sparse2DMatrix<int, int, double>();
            var bVector = new SparseArray<int, double>();
            var variableNameIndexMap = new SparseArray<string, int>();
            var numberOfEquations = 0;

            var parser = new LinearEquationParser();
            var parserStatus = LinearEquationParserStatus.Success;
            var solverStatus = LinearEquationSolverStatus.IllConditioned;

            foreach (var line in equations)
            {
                parserStatus = parser.Parse(line, aMatrix, bVector, variableNameIndexMap, ref numberOfEquations);
                if (parserStatus != LinearEquationParserStatus.Success) break;
            }
            if (numberOfEquations == variableNameIndexMap.Count)
            {
                var xVector = new SparseArray<int, double>();
                solverStatus = Solve(numberOfEquations, aMatrix, bVector, xVector);
                switch (solverStatus)
                {
                    case LinearEquationSolverStatus.Success:
                        foreach (var pair in variableNameIndexMap)
                            response.Add(pair.Key, xVector[pair.Value]);
                        break;
                    case LinearEquationSolverStatus.IllConditioned:
                        break;
                    case LinearEquationSolverStatus.Singular:
                        break;
                }
            }
            return solverStatus;
        }
    }
}
