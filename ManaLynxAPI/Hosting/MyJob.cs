using ManaLynxAPI.Utils;

namespace ManaLynxAPI.Hosting
{
    public class MyJob
    {
        public Type Type { get; }
        public string Expression { get; }

        public MyJob(Type type, string expression)
        {
            Type = type;
            Expression = expression;
        }

    }
}
