using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using System;
using System.Linq;
using System.Reflection;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var types = Assembly.GetEntryAssembly().GetTypes();
            var finder = new SpecFinder(types, "");
            var tagsFilter = new Tags().Parse("");
            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var runner = new ContextRunner(tagsFilter, new ConsoleFormatter(), false);
            var results = runner.Run(builder.Contexts().Build());

            if(results.Failures().Count() > 0)
            {
                Environment.Exit(1);
            }
        }
    }
}
