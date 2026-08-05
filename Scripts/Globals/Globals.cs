using System.Collections.Generic; // this is used to use the Lists, which are like dynamic arrays. this is useful in this case for the ID's that we're using in the program.
using System;

namespace MyAp
{
    public static class Globals // we can use this class to make global variables, as we can just do Globals.VariableName since this is an static class, and has static attributes, so we don't have to create an object to access all this.
    {
        public static List<int> _IDsRegistered = new List<int>();
        
        public static int SimulationQuantum;

        public static List<Process> GenerateRandomProcess(int number_of_processes)
        {
            List<Process> RandomProcessesGenerated = new List<Process>();

            Random random = new Random(); // we create a new random to work with

            string[] operators = new string[] { // a string array for the operators. add the potency operator later, as there can be things messed up with randomness and potency, resulting in things like 234 to the 100 or something like that.
                        "+",
                        "-",
                        "*",
                        "/",
                        "%",
                        "^",
                    };

            // we need number1, number2 and operator for the operation
            string number1 = "";
            string number2 = "";
            string _operator = "";

            // also, for this processes their name will be default.
            string _name = "default";
            string _operation = "";
            string _id = "";
            int _time = 0;

            for (int i = 0; i < number_of_processes; i++)
            {
                // we first choose the two numbers and operator.
                number1 = random.Next(0, 1000).ToString();
                _operator = operators[random.Next(0, operators.Length)]; // the random.Next has the second number to be not inclusive, so we have to be careful about it. in this case we can do this, since the length is 6, but we will onle get numbers from 0 to 5.

                // in the second number we check if the operator is a potency, so the second number is not a big number that can cause trouble to the program. in the case it's not potency, choose any number.
                if (_operator == "^") number2 = random.Next(0, 4).ToString();
                // we also check for the division and module operator, as they can't be zero.
                else if (_operator == "/") number2 = random.Next(1, 1000).ToString();
                else if (_operator == "%") number2 = random.Next(1, 1000).ToString();
                else number2 = random.Next(0, 1000).ToString();

                _operation = number1 + _operator + number2; // we assign the _operation string
                _time = random.Next(6, 20); // we get a time between 6 and 20

                _id = AssignRandomProcessID();

                Process proc = new Process(_name, _operation, _id, _time); // we create the process.
                proc.StartProcessChecking(); // we then check the process, we do this for extra validation and also to get the result from the validation.

                RandomProcessesGenerated.Add(proc);
            }

            return RandomProcessesGenerated;
        }

        private static string AssignRandomProcessID()
        {
            int random_process_id = 0;

            _IDsRegistered.Sort();

            for (int i = 0; i < _IDsRegistered.Count; i++)
            {
                if (_IDsRegistered[i] != i)
                {
                    random_process_id = i;
                    break;
                }

                if (i + 1 == _IDsRegistered.Count)
                {
                    random_process_id = i + 1;
                    break;
                }
            }

            return random_process_id.ToString();
        }
    }
}