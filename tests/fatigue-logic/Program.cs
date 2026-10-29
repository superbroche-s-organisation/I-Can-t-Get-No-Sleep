using ICGNS.Tests;

int checks = 0;
FatigueChecks.Run((passed, message) =>
{
    if (!passed)
        throw new InvalidOperationException(message);
    checks++;
});
Console.WriteLine($"{checks} fatigue logic checks passed (no game libraries used).");
