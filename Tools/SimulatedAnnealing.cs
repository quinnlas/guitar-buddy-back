class SimulatedAnnealing
{
  public static T solve<T>(
  T start,
  int maxIterations,
  Func<T, T> neighborFn,
  Func<T, double> scoreFn // lower is better
)
  {
    var solution = start;
    double score = scoreFn(solution);
    for (int iteration = 0; iteration < maxIterations; iteration++)
    {
      if (iteration % 200000 == 0) Console.WriteLine((double)iteration / maxIterations * 100);
      var newSolution = neighborFn(solution);
      double newScore = scoreFn(newSolution);
      double howMuchWorse = newScore - score;

      if (howMuchWorse < 0 || takeAnyway(iteration, howMuchWorse))
      {
        solution = newSolution;
        score = newScore;
      }
    }
    Console.WriteLine(100);

    return solution;
  }

  private static Random rand = new Random();

  // the probability of taking a bad solution decreases as the number of iterations increases
  // probability decreases as howMuchWorse increases
  private static bool takeAnyway(int iteration, double howMuchWorse)
  {
    // see https://www.mathworks.com/help/gads/simulated-annealing-options.html
    // for other options to compute temp and acceptance probability
    double temp = 100 * Math.Pow(.95, iteration);
    double acceptanceProbability = Math.Exp(-howMuchWorse / temp);

    // Console.WriteLine($"Iteration:   {iteration}");
    // Console.WriteLine($"Delta:       {howMuchWorse}");
    // Console.WriteLine($"Temperature: {temp}");
    // Console.WriteLine($"Probability: {acceptanceProbability}");
    // Console.WriteLine();

    return acceptanceProbability >= rand.NextDouble();
  }
}