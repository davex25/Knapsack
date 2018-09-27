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
            do 
            {
                var users = Utility.RandomTransactions().Take(2).ToList();

                Console.WriteLine("Transactions to mix:");
                Console.WriteLine();
                foreach (var u in users) 
                {
                    Console.WriteLine(u.ToString());
                    Console.WriteLine();
                }
                Console.WriteLine();
                var outputTransaction = MixTransactions(users.First(), users.Last(), users.First().Inputs.Count());
                Console.WriteLine("Output mixed transaction:");
                Console.WriteLine();
                Console.WriteLine(outputTransaction.ToString());
            } while (Console.ReadLine() != "") ;
        }

        public static Transaction MixTransactions(Transaction a, Transaction b, int numInputs) 
        {
            var newInputs = new List<decimal>(a.Inputs.Concat(b.Inputs));
            if (a.Inputs.Sum() == b.Inputs.Sum()) 
            {
                return new Transaction 
                { 
                    Inputs=newInputs,
                    Outputs = a.Outputs.Concat(b.Outputs).ToList()
                };
            }
            decimal randSum;
            Console.WriteLine("Taking " +numInputs + " shuffled inputs");
            int sanity=0;
            do 
            {
                newInputs.Shuffle();
                randSum = newInputs.Take(numInputs).Sum();
                if (sanity++ > 1000)
                {
                    Console.WriteLine("Can't find a random sum using " + numInputs + " inputs that is less than both " + a.Outputs.Sum() + " and " + b.Outputs.Sum());
                    return new Transaction {Inputs = a.Inputs, Outputs = b.Outputs};
                }
            } while (randSum >= a.Outputs.Sum() && randSum >= b.Outputs.Sum()) ;
            IEnumerable<decimal> newOutputs = null;
            if (a.Outputs.Sum() > randSum) 
            {
                var subsumOuts = RealizeSubsum(a.Outputs, randSum).ToList();
                newOutputs = subsumOuts.Concat(b.Outputs);
            }
            else if (b.Outputs.Sum() > randSum) 
            {
                var subsumOuts = RealizeSubsum(b.Outputs, randSum).ToList();
                newOutputs = subsumOuts.Concat(a.Outputs);
            }
            return new Transaction
            {
                Inputs = newInputs.ToList(),
                Outputs = newOutputs.ToList()
            };
        }

        public static IEnumerable<decimal> RealizeSubsum(IEnumerable<decimal> coins, decimal subsum) 
        {
            foreach (var coin in coins) 
            {
                Console.WriteLine("Subsum="+subsum);
                Console.WriteLine("Coin="+coin);
                if (subsum == 0)
                {
                    yield return coin;
                } 
                else if (coin <= subsum)
                {
                    yield return coin;
                    subsum -= coin;
                }
                else if (coin > subsum) 
                {
                    yield return subsum;
                    yield return coin - subsum;
                    subsum = 0;
                }
            }
        }

    }

    public static class Utility 
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this List<T> items) 
        {
            int n = items.Count;
            while (n > 1) 
            {
                n--;
                int k = rng.Next(n + 1);
                var value = items[k];
                items[k] = items[n];
                items[n] = value;
            }
        }

        public static string AsLabelValueString(this IEnumerable<decimal> values, string label) 
        {
            return String.Join("\r\n", values.Select(v => label + ": " + v.ToString()));
        }

        public static IEnumerable<decimal> RandomDecimals(int min, int max) 
        {
            while (true)
                yield return decimal.Parse(rng.Next(min, max) + "." + rng.Next(0, 9999));
        }
        public static IEnumerable<Transaction> RandomTransactions() 
        {
            while (true) 
            {
                var inputCount = rng.Next(1, 5);
                var numList = RandomDecimals(0, 10).Take(inputCount).ToList();
                yield return new Transaction 
                {
                    Inputs = numList,
                    Outputs = numList.ToList() //Make a copy
                };
            }
        }
    }
    public class Transaction
    {
        public List<decimal> Inputs;
        public List<decimal> Outputs;

        public override string ToString()
        {
            return string.Join("\r\n", Inputs.AsLabelValueString("Input"), Outputs.AsLabelValueString("Output"));
        }
    }

}
