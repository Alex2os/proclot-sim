using System.ComponentModel;
using System;

namespace MyAp
{
    public partial class Process : INotifyPropertyChanged // in this case we have to use INotifyPropertyChanged because we want to update a property of the process object itself, not only the collection or list like we've been doing until now.
    {
         public event PropertyChangedEventHandler PropertyChanged;

        public string _Name { get; set; }
        public string _ID { get; set; }
        public string _Operation { get; set; }
        public int _Time { get; set; }
        public string _Result { get; set; }
        public float RealResult =0; // we have to use another variable to save the real result, as saving it in the _Result variable will be susceptible to changes, as it can be changed for the "Error" word. the RealResult is just to save the real value
        public int _TimePassed { get; set; }
        public int _TimeLeft { get; set; }
        public int _TimePassedBlocked { get; set; } // this is used to determine how much time the process has passed in blocked state, in case the process is in this state.

        // all the following times are needed to show them in the table of the processes information. 
        public string _ArrivingTime { get; set; }
        public string _EndingTime { get; set; }
        public string _WaitingTime { get; set; }
        public string _ResponseTime { get; set; }
        public string _ReturnTime { get; set; }
        public string _ServiceTime { get; set; }
        public int RealServiceTime = 0;

        private bool _IsThereError = false;
        private bool BadName = false;
        private bool BadOperation = false;
        private bool BadID = false;
        private bool BadTime = false;
        public bool GetResponseTime = true; // to get the response time is set true by default, as when the process enters execution state then this will turn to false. this is to get the response time.

        public Process(string name, string operation, string id, int time) // if everything turned out to be correct, then we now assign the variables
        {

            _Name = name;
            _Operation = operation;
            _ID = id;
            _Time = time;

            // we assign the variables that we need to initialize.
            _Result = "";

            _TimeLeft = _Time;
            _TimePassed = 0;
            _TimePassedBlocked = 0;

            _ArrivingTime = "--";
            _EndingTime = "--";
            _WaitingTime = "--";
            _ResponseTime = "--";
            _ReturnTime = "--";
            _ServiceTime = "--";
            RealServiceTime = 0;
        }

        public string GetProcessID()
        {
            return _ID;
        }

        public void FirePropertyChangedEvent() // this function can be called externally, allowing us to fire the event that triggers the inotifypropertychanged, so the ui now gets updated correctly.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_TimePassedBlocked))); // here we pass the property that we want to update, in this case we want the _timepassedblocked. 
        }

        public bool StartProcessChecking()
        {

            if (CheckName()) { }
            else
            {
                Error("El nombre introducido es invalido.");
                BadName = true;
            }


            if (CheckTime()) { } // here we just check if the time is greater than zero.
            else
            {
                Error("El tiempo introducido no es valido. Debe ser mayor a cero.");
                BadTime = true;
            }

            if (CheckOperation()) { } // here we are checking if the operation is not possible (example: a number divided by zero), in case it's not possible, send a error message.
            else
            {
                Error("La operacion introducida no es valida");
                BadOperation = true;
            }

            if (CheckID()) { } // here we check if the ID is not repeated. in case it is, we should send an error message.
            else
            {
                Error("El ID introducido es invalido/repetido");
                BadID = true;
            }


            if (!_IsThereError) // if the parameters pass this second test, then return true and save the object. operations are done in the CheckOperation() function.
            {
                Console.WriteLine("process accepted.");
                return true;
            }

            Console.WriteLine("process not accepted. errors where found.");
            return false; // if there are errors, this returns false, saying that this object should and will be destroyed.
        }

        public bool ReturnBadOperations(string textbox_name)
        {
            switch (textbox_name) // there's no need to put breaks after the returns, as the function will return a value, so the breaks will not do anything anyways.
            {
                case "name":
                    return BadName;
                case "operation":
                    return BadOperation;
                case "id":
                    return BadID;
                case "time":
                    return BadTime;
                default:
                    return false; // this default is just put so the compilator doesn't give an error.
            }
        }

        public void AssignProcessTimes(string time_to_assign, ref int actual_time) // this is the function to assign all the times that have to be calculated through the simulation.
        {
            switch (time_to_assign)
            {
                case "arriving_time":
                    _ArrivingTime = actual_time.ToString();
                    break;
                case "ending_time":
                    _EndingTime = actual_time.ToString();
                    break;
                case "service_time":
                    RealServiceTime++;
                    _ServiceTime = RealServiceTime.ToString();
                    break;
                case "waiting_time":
                    bool IsServiceTimeNull = false;
                    if (_ServiceTime == "--")
                    {
                        IsServiceTimeNull = true;
                        _ServiceTime = "0";
                    }

                    _WaitingTime = (actual_time - int.Parse(_ArrivingTime) - int.Parse(_ServiceTime)).ToString(); // this is the way to calculate the waiting_time at the moment the user needs it.

                    if (IsServiceTimeNull) _ServiceTime = "--";
                    
                    break;
                case "response_time":
                    _ResponseTime = (actual_time- int.Parse(_ArrivingTime)).ToString();
                    break;
                case "return_time":
                    _ReturnTime = (int.Parse(_EndingTime) - int.Parse(_ArrivingTime)).ToString();
                    break;
            }
        }

        private bool GetNumbersAlgorithm() // in this function we extract the numbers from the string we previously had, or the string that the user introduced as operation
        {
            // variables for the getnumbersalgorithm (for)
            string _first_number_string = "";
            string _second_number_string = "";
            string _operator = "";

            // a bool that will help us to know when to start getting the 2nd number
            bool _get_second_number = false;

            // variables for when the numbers are finally converted
            int _first_number;
            int _second_number;

            Console.WriteLine(_Operation);
            for (int i = 0; i < _Operation.Length; i++)
            { // we start adding for the first number so we can get an string with only numbers, and then convert that to int and get the number in int value. // -3*4

                if ((Char.IsDigit(_Operation[i]) || (i == 0 && _Operation[i] == '-')) && !_get_second_number) _first_number_string += _Operation[i]; // we add the condition of (i == 0 && _Operation[i] == '-'). this is so if there is a negative symbol in the start, then add it to the first_number_string.

                else if (!_get_second_number) // we get the operator once the character is not a digit/number
                {
                    _operator += _Operation[i];
                    _get_second_number = true;
                }
                else _second_number_string += _Operation[i]; // once we get the operator, then we start getting the second number. we can add the negative symbol (if there is any) to the second_number_string without any problem, and then convert both strings to numbers.
            }

            Console.WriteLine("obtained: ");
            Console.WriteLine(_first_number_string);
            Console.WriteLine(_operator);
            Console.WriteLine(_second_number_string);

            // convert the string to numbers
            _first_number = int.Parse(_first_number_string);
            _second_number = int.Parse(_second_number_string);

            if (CheckPossibleOperation(_operator, _second_number)) // we check if the operation is possible. here we check if there's a division by zero, or a modulus with zero, etc. if true, means that the operation can be done.
            {
                Calculate(_first_number, _operator, _second_number); // here we calculate everything we've done before.
                return true;
            }
            else return false;
        }

        private void Calculate(int _first_number, string _operator, int _second_number)
        {
            switch (_operator) // in this case, we have to turn all the operations to string, as it is needed later in the program to change the result to a string. we simply do this to have the result as a string.
            {
                case "+": // plus
                    RealResult = (float)_first_number + _second_number;
                    break;
                case "-": // minus
                    RealResult = (float)_first_number - _second_number;
                    break;
                case "*": // multiplication
                    RealResult = (float)_first_number * _second_number;
                    break;
                case "/": // division
                    RealResult = (float)_first_number / _second_number;
                    break;
                case "%": // module operation
                    RealResult = (float)_first_number % _second_number;
                    break;
                case "^": // potencies
                    RealResult = (float)Math.Pow(_first_number, _second_number); // this is the function to obtain an exponent operation. first numbers is the base, and second one is the exponent.
                    break;
            }

            _Result = RealResult.ToString();

            Console.WriteLine(_Result);
        }

        private bool CheckID()
        {
            // we first check if the length of the id is <=0, because it could be that no id is introduced. in case this is true, return false.
            if (_ID.Length <= 0) return false;

            // in this part we check if any of the characters of the _ID is not a number. in case it's not a number, we return false.
            for (int i = 0; i < _ID.Length; i++) if (!char.IsDigit(_ID[i])) return false;

            // when we check everything is correct, we turn the id to numbers.
            int temp_id = int.Parse(_ID);

            // if the id is less than zero, return false, as by good practices, id should be greater or equal to zero.
            if (temp_id < 0) return false;

            foreach (int id in Globals._IDsRegistered)
            {
                if (id == temp_id) return false; // if there's found that the ID is repeated, return false
            }

            if (!_IsThereError) Globals._IDsRegistered.Add(temp_id); // if theres an error (bad operation or the time is less or equal to zero) then the id will not be assigned to the group.

            return true; // returs true if the id is not repeated. if there's an error, this still should return true, as there's no error with the id itself
        }

        private bool CheckName() // in this function we just check if the name is not of length zero. if it's of length zero, return false.
        {
            if (_Name.Length <= 0) return false;
            return true;
        }

        // in this function we check the operation and also calculate the result beforehand. 
        private bool CheckOperation()
        {
            if (!CheckOperationSymbols()) return false; // here we first check that all the symbols introduced. if it's not a symbol that is used in an operation, then return false.

            if (!CheckNumbersAndOperator()) return false;

            if (GetNumbersAlgorithm()) return true;


            return false;
        }

        private void Error(string message)
        {
            // here we should present the error to the user, followed by the message that is sent to this function.
            Console.WriteLine(message);
            _IsThereError = true;
        }

        private bool CheckTime()
        {
            if (_Time > 0) return true;
            else return false;
        }

        private bool CheckPossibleOperation(string _operator, int _second)
        {
            if (_second == 0 && _operator == "/") return false;
            else if (_second == 0 && _operator == "%") return false;

            return true;
        }

        // if this returns true, then it means that the operation is all good. otherwise, it will return false, when a bad symbol is found. this analyzes both numbers and operation symbols.
        private bool CheckOperationSymbols()
        {
            for (int i = 0; i < _Operation.Length; i++)
            {
                switch (_Operation[i])
                {
                    // for the cases, those are the allowed symbols to be used. otherwise, this function will return true if found any symbol that is not what we want in the operation itself.
                    case '/':
                        break;
                    case '%':
                        break;
                    case '*':
                        break;
                    case '+':
                        break;
                    case '-':
                        break;
                    case '^':
                        break;
                    default:

                        if (char.IsDigit(_Operation[i])) { } // we check if it's a number. if it's not, then this will return fase.

                        else return false;
                        break;
                }
            }
            return true;
        }

        private bool CheckNumbersAndOperator() // in this function whe check if the order of the symbols/operators and numbers is correct. we also check the negative numbers here.
        {
            // first and second number flags are to check if the first and second numbers have appeared.
            bool first_number_flag = false;
            bool second_number_flag = false;

            bool operation_flag = false; // this flag is used to see if there's already a symbol, so when this is true the second_number_flag can be now activated. 
            int operation_symbols = 0; // this is to count the operation symbols. could be the case where there are more than 1 symbols, or just that there are none, so this is to detect errors.

            for (int i = 0; i < _Operation.Length; i++) // 
            {
                if (char.IsDigit(_Operation[i]) && !first_number_flag) first_number_flag = true; // this is to check if there is the first number. if there is, activate this flag.

                if (i == 0 && _Operation[i] == '-') { } // if the first symbol is negative, then don't do anything. we can have negative in the start to specify that a number is negative
                else if (!char.IsDigit(_Operation[i])) // if the _operation[i] character is not a number, then it means this is the operation symbol. we don't have to check symbols, because the function "CheckOperationSymbols()" checked the symbols already, so what we have now are operation symbols.
                {
                    operation_symbols++;
                    operation_flag = true;

                    if (!second_number_flag) // we check if the second number has not been checked. could be the case when there's a minus symbol after the second number.
                    {
                        if (i + 1 == _Operation.Length) { } // if i++ is equal to the operation length, then we will go out of index, causing an error.
                        else if (_Operation[i + 1] == '-') i++; // if everything is correct, then we will check this operation, so we can skip the - sign and not add it to the operation_symbol variable
                    } // if after the operator flag goes a negative, then just skip the index, as there's no problem too if the second number is negative.
                }

                if (char.IsDigit(_Operation[i]) && first_number_flag && operation_flag && !second_number_flag) second_number_flag = true; // this is to activate the second_number_flag. we have to use those extra conditions so it gets activated correctly.


                if (!first_number_flag && operation_symbols > 0) return false; // if for some reason the operation flag is greater than zero first than the first_number_flag activates, then it means that an operation symbol is before any number. we return false in this case.
            }

            if (operation_symbols == 0) return false; // there shouln't be the case where the operation_symbol is zero or more than one.
            if (operation_symbols > 1) return false;

            if (!first_number_flag || !second_number_flag || !operation_flag) return false; // if any of the flags is set to false after the iterations, then return false.

            return true;
        }
}
}