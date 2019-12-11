namespace App.Samples
{
    public class Greeter
    {
        readonly string _setting;

        public Greeter(string setting)
        {
            _setting = setting;
        }

        public string Greet(string name)
        {
            if (_setting == "formal")
            {
                return $"Hello, Mr. {name}.";
            }
            else
            {
                return $"Hey {name}.";
            }
        }
    }
}