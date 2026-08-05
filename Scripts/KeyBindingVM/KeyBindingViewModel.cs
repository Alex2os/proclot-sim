using System;

namespace MyAp
{
    public class KeyBindingViewModel
    {
        // a variable for the simulation page instance. we need this to hook up the events.
        private SimulationPage sim_page_vm;

        // events for each key pressed.
        public event EventHandler OnWKeyPressed;
        public event EventHandler OnEKeyPressed;
        public event EventHandler OnCKeyPressed;
        public event EventHandler OnPKeyPressed;
        public event EventHandler OnNKeyPressed;
        public event EventHandler OnBKeyPressed;

        public KeyBindingViewModel() // constructor for the class
        {

        }

        public void AssignSimPageVM(SimulationPage vm) // with this class we can assign the simulation page view model to this class, so we can have access to the methods we will hook up to the events in this class.
        {
            sim_page_vm = vm;
        }

        public void AssignSubscritionsFunctions() // here is just to assign the subscriptions to the right functions, using the sim page view model that we obtained.
        {
            OnPKeyPressed += sim_page_vm.OnPauseSimulationKeyPressed;
            OnCKeyPressed += sim_page_vm.OnContinueSimulationKeyPressed;
            OnWKeyPressed += sim_page_vm.OnEndProcessWithErrorKeyPressed;
            OnEKeyPressed += sim_page_vm.OnInputAndOutputKeyPressed;
            OnNKeyPressed += sim_page_vm.OnCreateProcessKeyPressed;
            OnBKeyPressed += sim_page_vm.OnSeeBCPTableKeyPressed;
        }

        // all the following 4 functions are to call or invoke the events.
        public void OnWKeyPressedUI()
        {
            Console.WriteLine("W key pressed.");
            OnWKeyPressed?.Invoke(this, EventArgs.Empty); // we can invoke an event like this. in this case we are not sending anything in the Args, so we put EventArgs.Empty to specify this.
        }

        public void OnEKeyPressedUI()
        {
            Console.WriteLine("E key pressed.");
            OnEKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void OnCKeyPressedUI() // when letter C is pressed, the program will continue. this key can only continue the program.
        {
            Console.WriteLine("C Key pressed.");
            OnCKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void OnPKeyPressedUI() // when the P key is pressed, the program will be paused. this key can only pause the program.
        {
            Console.WriteLine("P key pressed.");
            OnPKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void OnNKeyPressedUI()
        {
            Console.WriteLine("N key pressed.");
            OnNKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void OnBKeyPressedUI()
        {
            Console.WriteLine("B key pressed.");
            OnBKeyPressed?.Invoke(this, EventArgs.Empty);
        }
        
    }
}
