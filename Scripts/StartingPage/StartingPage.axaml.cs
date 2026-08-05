// the name.Text, operation.Text, etc., can be used globally in the code. so it's better to have everything about these variables in other parts of the code, and not exactly on the mainwindow class
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity; // we need this library in order to use RoutedEventArgs.
using MsBox.Avalonia.Models;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia; // we use this external avalonia ui package to do the message boxes. 
using Avalonia.Media;
using MsBox.Avalonia.Enums; // this too is used for the message boxes.

using System;

namespace MyAp
{

    public partial class StartingPage : UserControl
    {
        private ProcessesCollectionViewModel collections_vm;

        public StartingPage(ProcessesCollectionViewModel processes_collection) // we send the processes_collection here too, as here can be modified and updated.
        {
            InitializeComponent();
            DataContext = processes_collection; // remember that here we have to vinculate to our viewmodel that contains our collection, so we can have it.
            collections_vm = processes_collection;

            AssignTextBoxDefaultValues();
        }

        public void OnAddProcessButtonClicked(object sender, RoutedEventArgs args)
        {
            ChangeTextBoxBorderBrushToDefault();

            CreateProcessObject();
        }

        public void UpdateProcessesAmountLabel() // this function updates the processesamountlabel.
        {
            ProcessesAmountLabel.Content = "Procesos que se crearán: " + collections_vm.Processes.Count.ToString();
        }

        public void CreateProcessObject()
        {
            Process proc = new Process(name.Text, operation.Text, id.Text, (int)time.Value); // create an object to check the process and pass all the parameters once checked

            if (proc.StartProcessChecking()) // if the process turned out to be correct, then returns true
            {
                collections_vm.Processes.Add(proc);
                UpdateProcessesAmountLabel();

            }


            else
            {
                if (proc.ReturnBadOperations("name")) ChangeTextBoxBorderBrushToRed("name");

                if (proc.ReturnBadOperations("operation")) ChangeTextBoxBorderBrushToRed("operation");

                if (proc.ReturnBadOperations("id")) ChangeTextBoxBorderBrushToRed("id");

                if (proc.ReturnBadOperations("time")) ChangeTextBoxBorderBrushToRed("time");
            }
        }

        public void OnStartButtonClicked(object sender, RoutedEventArgs args)
        {
            if (Globals._IDsRegistered.Count > 0 && CheckString(quantum_value.Text)) // we check if there are already ID's registered. if there's none, then we will show an error in the following line. we also check if the quantum value is right.
            {
                StartProgram();
                Globals.SimulationQuantum = int.Parse(quantum_value.Text);
            }
            else
            {
                // the var data type allows us to tell the compiler to choose whatever data type it's needed for a certain case. in this case, we don't need to specify the data type, as the compiler will choose for the MessageBoxManager data type.

                // we can use this to create "error" messageboxes thaat can appear in the program.
                if (!CheckString(quantum_value.Text))
                {
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Favor de especificar un valor del Quantum válido.",
                        ButtonEnum.Ok);

                    var result = box.ShowAsync(); // we need this too so the messagebox can appear. this will wait until the messagebox is closed.
                }

                else
                {
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "No se encontró ningún proceso para poder iniciar el programa.",
                        ButtonEnum.Ok);

                    var result = box.ShowAsync();
                }
            }
        }

        // this function happens when the erase process button is pressed in the app.
        public void OnEraseButtonClicked(object sender, RoutedEventArgs args)
        {
            if (sender is Button button && button.DataContext is Process process) // we have to do this so we can cast Button class to button, then use that to cast a Process class to process, then do the next if:
            {
                int id_to_remove = int.Parse(process.GetProcessID()); // we get the id from the object, so we can erase it from the ids list, so the id freed can be reused again in other processes.
                Globals._IDsRegistered.Remove(id_to_remove);

                collections_vm.Processes.Remove(process);
                UpdateProcessesAmountLabel();
            }

            Console.WriteLine("process erased");
        }

        private void StartProgram()
        {
            // what the line below does is that basically, with TopLevel we can get the top container, or in this case, the container that is in top of "this".
            // "this" refers to this instance, the StartingPage class instance, so now, specifying that we want to get the container on the top level of "this"-
            // we can now get the MainWindow, which is the one that is on top of all of the children.
            // now that we got that, we can now cast that container to MainWindow the class, and now we are able to use the methods of this MainWindow class, just like we do here.
            // in this case we make this so we can call the MainWindow to change the Page or instantiate another class to be the top page, like we want here once the start button is pressed on this StartingPageClass.
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this);
            mainWindow.ChangePageToSimulation(); // here we call the method to change the page.
        }

        public async void OnRestartButtonClicked(object sender, RoutedEventArgs args)
        {
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams // we can create a custom message box with this declaration.
            {
                ContentTitle = "Reiniciar procesos",
                ContentMessage = "Todos los procesos y la lista serán reiniciados. ¿Desea continuar?",

                // we define the buttons here.
                ButtonDefinitions = new[]
                {
                    new ButtonDefinition { Name = "No" }, // we put the button definitions here. in this case we want a Sí an No for the options
                    new ButtonDefinition { Name = "Sí" }
                },

                WindowStartupLocation = WindowStartupLocation.CenterOwner, // with this we can specify the window location when showed, in this case is in the center of the screen.

                CanResize = false // we specify if the window can be resized.
            });

            var result = await box.ShowAsync(); // in this case we have to use await for the if below to work. also, we have to put async in the declaration of the function so we can use the await.
            // await helps us to return the buttonresult that, in this case, the messageboxmanager returns. then we can use that in the if below.

            if (result == "Sí") // when we use custom messageboxes, they return a string instead of a button. the string they return is the text the buttons contain.
            {
                collections_vm.ClearMainProcessList();
                Globals._IDsRegistered.Clear();
                UpdateProcessesAmountLabel();
                Console.WriteLine("resetting list");
            }

        }

        private void ChangeTextBoxBorderBrushToRed(string textbox_name) // this function is to change the textboxborder brush to red when values are wrong in the textboxes/numericupdown
        {
            var border_brush = new LinearGradientBrush // we create a new linear gradientbrush for the textboxes when their values are wrong
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), // we create the startpoint
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative), // and the endpoint, just like in the styles.

                GradientStops = {
                    new GradientStop(Color.Parse("rgba(255, 0, 0, 1)"), 0), // and then we create the gradient for the border brush
                    new GradientStop(Color.Parse("rgba(255, 109, 109, 1)"), 1)
                }

            };

            switch (textbox_name)
            {
                case "name":
                    name.BorderBrush = border_brush;
                    break;
                case "operation":
                    operation.BorderBrush = border_brush;
                    break;
                case "id":
                    id.BorderBrush = border_brush;
                    break;
                case "time":
                    time.BorderBrush = border_brush;
                    break;
                case "random_processes":
                    random_processes.BorderBrush = border_brush;
                    break;
            }
        }

        private void ChangeTextBoxBorderBrushToDefault()
        {
            name.ClearValue(TextBox.BorderBrushProperty);
            operation.ClearValue(TextBox.BorderBrushProperty);
            id.ClearValue(TextBox.BorderBrushProperty);
            time.ClearValue(NumericUpDown.BorderBrushProperty);
            random_processes.ClearValue(TextBox.BorderBrushProperty);
        }

        private void AssignTextBoxDefaultValues() // in this function we assign the default values of the textboxes and the numericupdown. this is done so we don't have to check for errors in the first page.
        {
            name.Text = "";
            operation.Text = "";
            id.Text = "";
            time.Value = 5;
        }

        public void OnRandomButtonClicked(object sender, RoutedEventArgs args) // this function is activated when the random or "Aleatorio" button is pressed. here we check the string of the number of processes that the user wants, validating the number too.
        {
            ChangeTextBoxBorderBrushToDefault();

            string random_processes_text = random_processes.Text;

            if (CheckString(random_processes_text)) // the CheckRandomProcessText is identical to the checkid function from the process class, but now we check the text here, and here we don't check if the same value has been introduced before.
            {
                // if everything is correct with the text, then turn that to int and work the random processes and insert them.

                int number_of_processes = int.Parse(random_processes_text);

                // we clear both the ids list and the processes list, and we reset the lots amount too.
                collections_vm.ClearMainProcessList();
                Globals._IDsRegistered.Clear();

                // we obtain a list with the processes generated, and also add them to the processes collection. the list is obtained from the function that is inside globals. this was changed as we need to use this function globally with other scripts too.
                List<Process> processes_generated = Globals.GenerateRandomProcess(number_of_processes);

                for (int i = 0; i < processes_generated.Count; i++)
                {
                    collections_vm.Processes.Add(processes_generated[i]);
                }

                UpdateProcessesAmountLabel();
            }
            else ChangeTextBoxBorderBrushToRed("random_processes");
        }

        public bool CheckString(string string_to_check)
        {
            if (string_to_check.Length == 0) return false;

            for (int i = 0; i < string_to_check.Length; i++) if (!char.IsDigit(string_to_check[i])) return false;

            int number_of_processes = int.Parse(string_to_check);

            if (number_of_processes <= 0) return false;

            return true;
        }



    }
}