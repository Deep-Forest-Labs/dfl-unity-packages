#nullable enable
using System;

namespace DeepForestLabs.BuildSystems
{
    public class CommandLineReader
    {
        private readonly string[] _args;
		
        public CommandLineReader()
        {
            _args = Environment.GetCommandLineArgs();
        }

        public bool IsValid(string arg) => !string.IsNullOrEmpty(GetValueFromCommandLine(arg));

        private string GetValueFromCommandLine(string arg)
        {
            if (_args.Length > 0)
            {
                int index = Array.IndexOf(_args, arg);
                if (( index >= 0 ) && ( index < _args.Length - 1 ))
                {
                    return _args[index + 1];
                }
            }
            return string.Empty;
        }

        public string StringArgument(string arg, string defaultValue)
        {
            string value = GetValueFromCommandLine(arg);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public bool BooleanArgument(string arg, bool defaultValue = false)
        {
            string value = GetValueFromCommandLine(arg).ToLower();
            switch (value)
            {
                case "true":
                case "t":
                case "1":
                    return true;

                case "false":
                case "f":
                case "0":
                    return false;

                default:
                    return defaultValue;
            }
        }

        public int IntArgument(string arg, int defaultValue = -1)
        {
            string value = GetValueFromCommandLine(arg);
            if (!int.TryParse(value, out int val))
            {
                return defaultValue;
            }
            return val;
        }
    }
}
#nullable disable