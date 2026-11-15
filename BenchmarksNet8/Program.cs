using BenchmarkDotNet.Running;
Environment.SetEnvironmentVariable("R_HOME", "C:\\Program Files\\R\\R-4.2.1");
BenchmarkRunner.Run(typeof(Program).Assembly);