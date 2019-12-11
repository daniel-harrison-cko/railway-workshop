using App.Samples;
using NSpec;
using Shouldly;

namespace Tests.Samples
{
    class describe_greeter : nspec
    {        
        void given_name_and_formal_setting()
        {
            var name = "Frodo";
            var setting = "formal";

            var greeter = new Greeter(setting);

            var result = greeter.Greet(name);

            it["greets person formally"] = () =>
                result.ShouldBe("Hello, Mr. Frodo.");
        }

        void given_name_and_informal_setting()
        {
            var name = "Frodo";
            var setting = "informal";

            var greeter = new Greeter(setting);

            var result = greeter.Greet(name);

            it["greets person informally"] = () =>
                result.ShouldBe("Hey Frodo.");
        }
    }
}