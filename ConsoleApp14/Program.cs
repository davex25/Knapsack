using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ConsoleApp14
{
    static class Program
    {
        static void Main(string[] args)
        {
            var inputs = new List<decimal>();
            var inNum = 3;
            for (int i = 0; i < inNum; i++)
            {
                if (i % 32 == 0)
                {
                    inputs.Add(decimal.Parse(new Random().Next(0, 1000) + "." + new Random().Next(0, 9999)));
                }
                else if (i % 16 == 0)
                {
                    inputs.Add(decimal.Parse("0.0" + new Random().Next(0, 99)));
                }
                else if (i % 8 == 0)
                {
                    inputs.Add(decimal.Parse(new Random().Next(0, 100) + "." + new Random().Next(0, 9999)));
                }
                else if(i % 4 == 0)
                {
                    inputs.Add(decimal.Parse(new Random().Next(0, 10) + "." + new Random().Next(0, 9999)));
                }
                else
                {
                    inputs.Add(decimal.Parse(new Random().Next(0, 2) + "." + new Random().Next(0, 9999)));
                }
            }

            inputs.Sort();
            Console.WriteLine("IN:");
            foreach (var i in inputs)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            var doubleInputs = new List<(decimal amount, decimal id)>();
            foreach (var i in inputs)
            {
                doubleInputs.Add((i, i));
            }
            // decimal: output, decimal: input
            List<(decimal, decimal)> outputsCurrentlyTested = DoRenard(doubleInputs);
            List<(decimal, decimal)> outputsAlwaysSecondGreatest = DoAlwaysSecondGreatest(doubleInputs);
            List<(decimal, decimal)> outputsSmallerMiddleOut = DoSmallerMiddleOut(doubleInputs);

            var inputSum = inputs.Sum();
            decimal a1 = outputsCurrentlyTested.Select(x => x.Item1).Sum();
            if (a1 != inputSum)
            {
                throw new Exception();
            }
            decimal b1 = outputsAlwaysSecondGreatest.Select(x => x.Item1).Sum();
            if (b1 != inputSum)
            {
                throw new Exception();
            }
            decimal c1 = outputsSmallerMiddleOut.Select(x => x.Item1).Sum();
            if (c1 != inputSum)
            {
                throw new Exception();
            }

            // decimal: input, decimal: outputs
            List<(decimal, List<decimal>)> outputsPerInputsCurrentlyTested = CalculateOutputsPerInputs(outputsCurrentlyTested);
            outputsPerInputsCurrentlyTested = outputsPerInputsCurrentlyTested.OrderBy(x => x.Item1).ToList();
            List<(decimal, List<decimal>)> outputsPerInputsAlwaysSecondGreatest = CalculateOutputsPerInputs(outputsAlwaysSecondGreatest);
            outputsPerInputsAlwaysSecondGreatest = outputsPerInputsAlwaysSecondGreatest.OrderBy(x => x.Item1).ToList();
            List<(decimal, List<decimal>)> outputsPerInputsSmallerMiddleOut = CalculateOutputsPerInputs(outputsSmallerMiddleOut);
            outputsPerInputsSmallerMiddleOut = outputsPerInputsSmallerMiddleOut.OrderBy(x => x.Item1).ToList();

            var anonSetSums = new decimal[] { 0, 0, 0 };
            var anonSetWeightedByValueSum = new decimal[] { 0, 0, 0 };

            Console.WriteLine("CURRENTLY TESTED");
            foreach (var opi in outputsPerInputsCurrentlyTested)
            {
                Console.Write($"{opi.Item1}:\t");
                foreach (var o in opi.Item2)
                {
                    decimal allOccurences = outputsPerInputsCurrentlyTested.SelectMany(x => x.Item2).Count(x => x == o);
                    decimal occurencesOfThisUser = outputsPerInputsCurrentlyTested.Where(x => x.Item1 == opi.Item1).SelectMany(x => x.Item2).Count(x => x == o);
                    decimal anonimitySet = (allOccurences / occurencesOfThisUser) - 1;
                    Console.Write($"{o} ({anonimitySet}) ");
                    anonSetSums[0] += anonimitySet;
                    anonSetWeightedByValueSum[0] += anonimitySet * o;
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("ALWAYS GREATEST:");
            foreach (var opi in outputsPerInputsAlwaysSecondGreatest)
            {
                Console.Write($"{opi.Item1}:\t");
                foreach (var o in opi.Item2)
                {
                    decimal allOccurences = outputsPerInputsAlwaysSecondGreatest.SelectMany(x => x.Item2).Count(x => x == o);
                    decimal occurencesOfThisUser = outputsPerInputsAlwaysSecondGreatest.Where(x => x.Item1 == opi.Item1).SelectMany(x => x.Item2).Count(x => x == o);
                    decimal anonimitySet = (allOccurences / occurencesOfThisUser) - 1;
                    Console.Write($"{o} ({anonimitySet}) ");
                    anonSetSums[1] += anonimitySet;
                    anonSetWeightedByValueSum[1] += anonimitySet * o;
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("ALWAYS SMALLER MIDDLE OUT:");
            foreach (var opi in outputsPerInputsSmallerMiddleOut)
            {
                Console.Write($"{opi.Item1}:\t");
                foreach (var o in opi.Item2)
                {
                    decimal allOccurences = outputsPerInputsSmallerMiddleOut.SelectMany(x => x.Item2).Count(x => x == o);
                    int occurencesOfThisUser = outputsPerInputsSmallerMiddleOut.Where(x => x.Item1 == opi.Item1).SelectMany(x => x.Item2).Count(x => x == o);
                    decimal anonimitySet = (allOccurences / occurencesOfThisUser) - 1;
                    Console.Write($"{o} ({anonimitySet}) ");
                    anonSetSums[2] += anonimitySet;
                    anonSetWeightedByValueSum[2] += anonimitySet * o;
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            //Console.WriteLine($"anonsetsums: {anonSetSums[0]} {anonSetSums[1]} {anonSetSums[2]}");
            Console.WriteLine($"anonsetweightedbyvaluesums: {anonSetWeightedByValueSum[0]} {anonSetWeightedByValueSum[1]} {anonSetWeightedByValueSum[2]}");
            Console.WriteLine($"outputnums: {outputsCurrentlyTested.Count} {outputsAlwaysSecondGreatest.Count} {outputsSmallerMiddleOut.Count}");
            //Console.WriteLine($"outputnums/anonsetsums: {outputsAlwaysSmallest.Count / (double)anonSetSums[0]} {outputsAlwaysSecondGreatest.Count / (double)anonSetSums[1]} {outputsSmallerMiddleOut.Count / (double)anonSetSums[2]}");
            Console.WriteLine($"anonsetweightedbyvaluesums/outputnums: {anonSetWeightedByValueSum[0] / outputsCurrentlyTested.Count} {anonSetWeightedByValueSum[1] / outputsAlwaysSecondGreatest.Count} {anonSetWeightedByValueSum[2] / outputsSmallerMiddleOut.Count}");
            Console.WriteLine($"vSizes: {CalculateVsize(inNum, outputsCurrentlyTested.Count)} {CalculateVsize(inNum, outputsAlwaysSecondGreatest.Count)} {CalculateVsize(inNum, outputsSmallerMiddleOut.Count)}");
            Console.WriteLine($"% of max strd tx size: { 100 * ((decimal)CalculateVsize(inNum, outputsCurrentlyTested.Count) / Constants.MaxStandardTxSizeInBytes)} {100 * ((decimal)CalculateVsize(inNum, outputsAlwaysSecondGreatest.Count) / Constants.MaxStandardTxSizeInBytes)} {100 * ((decimal)CalculateVsize(inNum, outputsSmallerMiddleOut.Count) / Constants.MaxStandardTxSizeInBytes)}");

            Console.ReadKey();
        }

        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                yield break;
            }

            var list = sequence.ToList();

            if (!list.Any())
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var startingElementIndex = 0;

                foreach (var startingElement in list)
                {
                    var remainingItems = AllExcept(list, startingElementIndex);

                    foreach (var permutationOfRemainder in Permute(remainingItems))
                    {
                        yield return Concat(startingElement, permutationOfRemainder);
                    }

                    startingElementIndex++;
                }
            }
        }

        private static IEnumerable<T> Concat<T>(this T firstElement, IEnumerable<T> secondSequence)
        {
            yield return firstElement;
            if (secondSequence == null)
            {
                yield break;
            }

            foreach (var item in secondSequence)
            {
                yield return item;
            }
        }

        private static IEnumerable<T> AllExcept<T>(this IEnumerable<T> sequence, int indexToSkip)
        {
            if (sequence == null)
            {
                yield break;
            }

            var index = 0;

            foreach (var item in sequence.Where(item => index++ != indexToSkip))
            {
                yield return item;
            }
        }

        private static int CalculateVsize(int inNum, int outNum)
        {
            var origTxSize = inNum * Constants.P2pkhInputSizeInBytes + outNum * Constants.OutputSizeInBytes + 10;
            var newTxSize = inNum * Constants.P2wpkhInputSizeInBytes + outNum * Constants.OutputSizeInBytes + 10; // BEWARE: This assumes segwit only inputs!
            var vSize = (int)Math.Ceiling(((3 * newTxSize) + origTxSize) / 4m);
            return vSize;
        }

        private static List<(decimal, List<decimal>)> CalculateOutputsPerInputs(List<(decimal, decimal)> outputs)
        {
            var outputsPerInputs = new List<(decimal, List<decimal>)>();

            foreach (var o in outputs)
            {
                (decimal, List<decimal>) found = outputsPerInputs.SingleOrDefault(x => x.Item1 == o.Item2);
                if (found.Item2 == null)
                {
                    outputsPerInputs.Add((o.Item2, new List<decimal> { o.Item1 }));
                }
                else
                {
                    found.Item2.Add(o.Item1);
                    found.Item2.Sort();
                }
            }

            return outputsPerInputs;
        }

        private static List<(decimal, decimal)> DoSmallerMiddleOut(List<(decimal amount, decimal id)> inputs)
        {
            // decimal: output, decimal: input
            var outputs = new List<(decimal outputAmount, decimal inputId)>();
            var unfixedOutputs = new List<(decimal outputAmount, decimal inputId)>();

            int smallerMiddleIndex = inputs.Count % 2 == 0 ? inputs.Count / 2 : inputs.Count / 2 + 1;
            var smallerMiddle = inputs.Select(x => x.amount).OrderBy(x => x).Skip(smallerMiddleIndex - 1).First();

            foreach (var i in inputs)
            {
                var diff = i.amount - smallerMiddle;
                if (diff > 0)
                {
                    outputs.Add((smallerMiddle, i.id));
                    unfixedOutputs.Add((diff, i.id));
                }
                else if (diff == 0)
                {
                    outputs.Add((i.amount, i.id));
                }
                else
                {
                    unfixedOutputs.Add((i.amount, i.id));
                }
            }

            if (unfixedOutputs.Count >= 2)
            {
                unfixedOutputs = DoSmallerMiddleOut(unfixedOutputs);
            }

            foreach (var o in unfixedOutputs)
            {
                outputs.Add(o);
            }

            return outputs.ToList();
        }

        private static List<(decimal, decimal)> DoClosestAverage(List<(decimal amount, decimal id)> inputs)
        {
            // decimal: output, decimal: input
            var outputs = new List<(decimal outputAmount, decimal inputId)>();
            var unfixedOutputs = new List<(decimal outputAmount, decimal inputId)>();

            var avg = inputs.Select(x => x.amount).Average();
            var closestAvg = inputs.Select(x => x.amount).Aggregate((x, y) => Math.Abs(x - avg) < Math.Abs(y - avg) ? x : y);

            foreach (var i in inputs)
            {
                var diff = i.amount - closestAvg;
                if (diff > 0)
                {
                    outputs.Add((closestAvg, i.id));
                    unfixedOutputs.Add((diff, i.id));
                }
                else if (diff == 0)
                {
                    outputs.Add((i.amount, i.id));
                }
                else
                {
                    unfixedOutputs.Add((i.amount, i.id));
                }
            }

            if (unfixedOutputs.Count >= 2)
            {
                unfixedOutputs = DoSmallerMiddleOut(unfixedOutputs);
            }

            foreach (var o in unfixedOutputs)
            {
                outputs.Add(o);
            }

            return outputs.ToList();
        }

        private static List<(decimal, decimal)> DoAlwaysSecondGreatest(List<(decimal amount, decimal id)> inputs)
        {
            // decimal: output, decimal: input
            var outputs = new List<(decimal outputAmount, decimal inputId)>();
            var unfixedOutputs = new List<(decimal outputAmount, decimal inputId)>();

            var secondGreatest = inputs.Select(x => x.amount).OrderByDescending(x => x).Skip(1).First();

            foreach (var i in inputs)
            {
                var diff = i.amount - secondGreatest;
                if (diff > 0)
                {
                    outputs.Add((secondGreatest, i.id));
                    unfixedOutputs.Add((diff, i.id));
                }
                else if (diff == 0)
                {
                    outputs.Add((i.amount, i.id));
                }
                else
                {
                    unfixedOutputs.Add((i.amount, i.id));
                }
            }

            if (unfixedOutputs.Count >= 2)
            {
                unfixedOutputs = DoAlwaysSecondGreatest(unfixedOutputs);
            }

            foreach (var o in unfixedOutputs)
            {
                outputs.Add(o);
            }
            
            return outputs.ToList();
        }

        // decimal: output, decimal: input
        private static List<(decimal, decimal)> DoRenard(List<(decimal inputAmount, decimal inputId)> inputs)
        {
            // Denominations: 0.01, 0.1, 1, 10, 100
            ApplyDenomination(out List<(decimal outputAmount, decimal inputId)>  remains, out List<(decimal outputAmount, decimal inputId)>  outputs, inputs, 1);

            return remains.Concat(outputs).ToList();
        }

        private static void ApplyDenomination(out List<(decimal outputAmount, decimal inputId)>  remains, out List<(decimal outputAmount, decimal inputId)>  outputs, List<(decimal inputAmount, decimal inputId)> inputs, decimal denomination)
        {
            // decimal: output, decimal: input
            outputs = new List<(decimal outputAmount, decimal inputId)>();
            // decimal: output, decimal: input
            remains = new List<(decimal outputAmount, decimal inputId)>();

            foreach (var input in inputs)
            {
                int times = (int)(input.inputAmount / denomination);
                for (int i = 0; i < times; i++)
                {
                    outputs.Add((denomination, input.inputId));
                }
                var rem = input.inputAmount % denomination;
                if (rem != 0)
                {
                    remains.Add((rem, input.inputId));
                }
            }
        }

        // decimal: output, decimal: input
        private static List<(decimal, decimal)> DoKnapsack(List<(decimal, decimal)> inputs)
        {
            List<(decimal, decimal)> bestOuts = null;
            foreach (var combo in Permute(inputs))
            {
                var outs = new List<(decimal, decimal)>();
                for (int i = 0; i < combo.Count(); i = i + 2)
                {
                    if(i + 1 == combo.Count())
                    {
                        outs.Add(combo.ElementAt(i));
                        break;
                    }

                    var mix = MixInputsKnapsack(new List<decimal> { combo.ElementAt(i).Item1 }, new List<decimal> { combo.ElementAt(i + 1).Item1 });
                    foreach(var m in mix)
                    {
                        outs.Add(m);
                    }
                }

                if(bestOuts == null)
                {
                    bestOuts = outs;
                }
                else
                {
                    var outCount = outs.Count;
                    var bestOutCount = bestOuts.Count;

                    var outOpi = CalculateOutputsPerInputs(outs);
                    var bestOutOpi = CalculateOutputsPerInputs(bestOuts);

                    var outAnonSetWeightedByValueSum = 0m;
                    var bestOutAnonSetWeightedByValueSum = 0m;

                    foreach (var opi in outOpi)
                    {
                        foreach (var o in opi.Item2)
                        {
                            int anonimitySet = outOpi.SelectMany(x => x.Item2).Count(x => x == o) / outOpi.Where(x => x.Item1 == opi.Item1).SelectMany(x => x.Item2).Count(x => x == o);
                            outAnonSetWeightedByValueSum += anonimitySet * o;
                        }
                    }

                    foreach (var opi in bestOutOpi)
                    {
                        foreach (var o in opi.Item2)
                        {
                            int anonimitySet = bestOutOpi.SelectMany(x => x.Item2).Count(x => x == o);
                            bestOutAnonSetWeightedByValueSum += anonimitySet * o;
                        }
                    }

                    var outWhatever = outAnonSetWeightedByValueSum / outCount;
                    var bestOutWhatever = bestOutAnonSetWeightedByValueSum / bestOutCount;

                    if(outWhatever > bestOutWhatever)
                    {
                        bestOuts = outs;
                    }
                }
            }

            return bestOuts;
        }

        // decimal: output, decimal: input
        private static List<(decimal, decimal)> MixInputsKnapsack(List<decimal> inputsA, List<decimal> inputsB)
        {
            if(inputsA.Sum() == inputsB.Sum())
            {
                // decimal: output, decimal: input
                var ret = new List<(decimal, decimal)>();
                foreach (var i in inputsA.Concat(inputsB))
                {
                    ret.Add((i, i));
                }

                return ret;
            }

            // decimal: output, decimal: input
            List<(decimal, decimal)> newOutputs = null;

            if (inputsA.Sum() > inputsB.Sum())
            {
                var diff = inputsA.Sum() - inputsB.Sum();
                newOutputs = RealizeSubsumKnapsack(inputsA, diff).ToList();

                foreach (var i in inputsB)
                {
                    newOutputs.Add((i, i));
                }
            }

            if(inputsB.Sum() > inputsA.Sum())
            {
                var diff = inputsB.Sum() - inputsA.Sum();
                newOutputs = RealizeSubsumKnapsack(inputsB, diff).ToList();

                foreach (var i in inputsA)
                {
                    newOutputs.Add((i, i));
                }
            }

            return newOutputs;
        }

        // decimal: output, decimal: input
        private static List<(decimal, decimal)> RealizeSubsumKnapsack(List<decimal> coins, decimal subsum)
        {
            // decimal: output, decimal: input
            var result = new List<(decimal, decimal)>();

            foreach (var coin in coins)
            {
                if(subsum == 0)
                {
                    result.Add((coin, coin));
                }
                else if(coin <= subsum)
                {
                    result.Add((coin, coin)); // 2 sub-trans.
                    subsum -= coin; // 3 sub-trans.
                }
                else if(coin > subsum)
                {
                    result.Add((subsum, coin));
                    result.Add((coin - subsum, coin));
                    subsum = 0;
                }
            }

            return result;
        }

        static decimal GCD(decimal a, decimal b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        static decimal GCD(decimal[] numbers)
        {
            return numbers.Aggregate(GCD);
        }

        // decimal: output, decimal: input
        private static List<(decimal, decimal)> DoAlwaysSmallest(List<(decimal, decimal)> inputs)
        {
            // decimal: output, decimal: input
            var outputs = new List<(decimal, decimal)>();
            var unfixedOutputs = new List<(decimal, decimal)>();

            var smallest = inputs.Select(x=>x.Item1).Min();

            foreach(var i in inputs)
            {
                var val = i.Item1 - smallest;
                if (val > 0)
                {
                    unfixedOutputs.Add((val, i.Item2));
                }
                outputs.Add((smallest, i.Item2));
            }

            if (unfixedOutputs.Any())
            {
                unfixedOutputs = DoAlwaysSmallest(unfixedOutputs);
            }

            foreach(var o in unfixedOutputs)
            {
                outputs.Add(o);
            }

            return outputs.ToList();
        }

        public static class Constants
        {
            public const int P2wpkhInputSizeInBytes = 41;
            public const int P2pkhInputSizeInBytes = 146;
            public const int OutputSizeInBytes = 33;
            public const int MaxStandardTxSizeInBytes = 100000;
        }
    }
}
