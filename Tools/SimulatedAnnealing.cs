class SimulatedAnnealing
{
  public static T solve<T>(
  T start,
  int maxK,
  Func<T, T> neighborFn,
  Func<T, double> scoreFn // lower is better
)
  {
    T solution = start;
    double score = scoreFn(solution);
    for (int k = 0; k < maxK; k++)
    {
      T newSolution = neighborFn(solution);
      double newScore = scoreFn(newSolution);
      double delta = newScore - score;

      if (delta < 0 || takeAnyway(k, delta))
      {
        solution = newSolution;
        score = newScore;
      }
    }

    return solution;
  }

  private static Random rand = new Random();
  private static bool takeAnyway(int k, double delta)
  {
    // see https://www.mathworks.com/help/gads/simulated-annealing-options.html
    // for other options to compute temp and acceptance probability
    double temp = 100 * Math.Pow(.95, k);
    double acceptanceProbability = Math.Exp(-delta / temp);

    // Console.WriteLine($"Iteration:   {k}");
    // Console.WriteLine($"Delta:       {delta}");
    // Console.WriteLine($"Temperature: {temp}");
    // Console.WriteLine($"Probability: {acceptanceProbability}");
    // Console.WriteLine();

    return acceptanceProbability >= rand.NextDouble();
  }
}