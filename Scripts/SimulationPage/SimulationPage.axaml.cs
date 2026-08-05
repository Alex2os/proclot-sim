using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;

// only thing left to add (check if there are some bugs or glitches) is to add a process for when there are 4 processes in the blocked state.

namespace MyAp
{
    public partial class SimulationPage : UserControl
    {
        ProcessesCollectionViewModel collections_vm; // we use this variable so we don't cast every time we want to access the collections viewmodel

        private DispatcherTimer timer = new DispatcherTimer // here we declare a new timer, which this is what is helping us to have something like the _Process function in godot, as we need real time updates.
        {
            Interval = TimeSpan.FromMilliseconds(1000) // here we declare the timespan of the timer, in this case we want 1000 ms, which is equal to one second.
        };

        private int SimulationQuantumTime =0;

        private int TotalTime = 0;
        private int NewProcessesAmount = 0; // this helps us to get the newprocesses amount as a copy in this class, so we can use it here and not change the real value, which will be re-used in case the user wants to start the program again.
        private bool SimulationFinished = false;

        public SimulationPage()
        {

        }

        public SimulationPage(ProcessesCollectionViewModel processes_collection) // for all the pages, we have to send the processes_collection, so they have access to this collection and con modify it
        {
            InitializeComponent();

            // we have the following process so the events that we have in the KeyBindingViewModel and the pressing of the keys work.
            // we first have to obtain the original instance created by Avalonia of our keybinding viewmodel. To do this, we can search for the resource in the tree, with the following line and assigning it to a variable:
            var keybinding_vm = (KeyBindingViewModel)this.FindResource("KeyBindingVM");

            keybinding_vm.AssignSimPageVM(this); // once found the view model, we can invoke it to assign this class to a variable inside the view model. this is important so we can hook up our events in the keybinding vm.
            keybinding_vm.AssignSubscritionsFunctions(); // after that, we invoke this function, which is to hook up all the events to the right functions. 

            Globals._IDsRegistered.Sort(); // we sort the list that we use for the ids, as we are going to use this to create new processes with the N key of the keyboard.

            // here we assign the datacontext and our view model
            DataContext = processes_collection;
            collections_vm = processes_collection;

            timer.Tick += (_, __) => Process(); // when starting the class, we initialize the timer, specifying the function that will be activated whenever the timer ticks, in this case we want to activate the Process function.
            timer.Start(); // at the same time, we start the timer.

            TotalTime = 0; // we initialize the totaltime, as this will be restarted everytime we do this simulation

            collections_vm.ReAssignProcessesValues(); // we re-assign the values of each process in the main collection. this is in case the user has already used the processes before in another simulation. here can be put some new values if needed, and have them in a single function.
            collections_vm.AssignListsAndVariables(); // in this function we initialize everything needed to work here, including lists and variables.
            collections_vm.AssignNewProcesses();
            collections_vm.AssignReadyProcesses(TotalTime); // in this function we assign the lots in the pendingprocessescollection. we do a call here to assign the first processes, then this function is called later.

            AsignWorkingProcess(); // after assigning ready processes, we assign a working process.
            collections_vm.AssignReadyProcesses(TotalTime); // as we have to add and check again the processes, we add if there are any to do so. this is why we use this function two times.

            UpdateProcessesAmount(); // in this function we update the processesamount, including the variable and label.

            // we check for the response time after assigning the process. if this is not like this, the program will return an error.
            CheckResponseTimeProcessInExecution(); // we have to check for the response time of the first process. as this first process is instantly set, then the response time of the first process will always be zero, so this is just to keep the function here.
        }

        private void Process()
        {
            UpdateTotalTime();

            UpdateProcessesTimes(); // in this function we update all the times related to processes.

            CheckTimeOfExecutionProcess(); // we check the time for the process in execution, in case the remaining time has hit zero.

            CheckActualQuantum();

            CheckSimulationState();
        }

        private void UpdateProcessesTimes()
        {
            UpdateBlockedProcessesTime(); // in this function we check if any processes are on the blocked state, and if so, then we update their time.

            UpdateInExecutionProcessTime(); // update the time for the process in execution.

            UpdateProcessWaitingTime(); // in this function we ONLY update the waiting time for the processes in the ready collection. for the blocked processes, we update those in the UpdateBlockedTime function.
        }

        private void CheckActualQuantum()
        {
            if (SimulationQuantumTime == Globals.SimulationQuantum)
            {
                if (collections_vm.ProcessInExecution[0]._ID == "null") ;
                else if (collections_vm.ReadyProcesses.Count == 0) ;
                else
                {
                    MoveProcessFromExecutionToReady();
                    Console.WriteLine("moved process.");
                }


                SimulationQuantumTime = 0;
                QuantumCounter.Content = "Contador del Quantum: " + SimulationQuantumTime;
            }
            else Console.WriteLine("process not moved");
        }
        
        private void MoveProcessFromExecutionToReady()
        {
            collections_vm.ReadyProcesses.Add(collections_vm.ProcessInExecution[0]);
            collections_vm.ProcessInExecution.Remove(collections_vm.ProcessInExecution[0]);
            collections_vm.ProcessInExecution.Add(collections_vm.ReadyProcesses[0]);
            collections_vm.ReadyProcesses.Remove(collections_vm.ReadyProcesses[0]);

            UpdateLabelsForExecutionProcess();
        }

        // the following two functions are used as intermediaries with the pausesimulation() function and when a key or a button is pressed.
        public void OnPauseSimulationKeyPressed(object sender, EventArgs e) // the declaration of the subscription function has to be like this, using (object sender, EventArgs e). we use this function just to call the pause simulation, and we do the same below with the button clicked function.
        {
            if (timer.IsEnabled) PauseOrContinueSimulation(); // this function can anly pause the timer. so, if the user presses the letter P, only the program will be paused.
        }

        public void OnPauseSimulationButtonClicked(object sender, RoutedEventArgs args) { PauseOrContinueSimulation(); }

        public void OnContinueSimulationKeyPressed(object sender, EventArgs e) // this function can only unpause the program. 
        {
            if (!timer.IsEnabled)
            {
                ProcessesInfoTableControl.Content = null; // if the c key is pressed when the processes info table is present, then it will be "erased" and will continue the program as usually.
                PauseOrContinueSimulation();
            }
        } 

        public void OnEndProcessWithErrorKeyPressed(object sender, EventArgs e) // this is the function for when the user presses the w key, which ocassionates the process to go to the done processes collection, and putting the result and operation as error.
        {
            if (SimulationFinished) { } // this prevents the user from pressing again the key and occasioning an error if the simulation has been finished.
            else if (!timer.IsEnabled) { } // if the program is paused, then this key will not take effect
            else if (collections_vm.ProcessInExecution[0]._ID == "null") { } // if the process is the null process, then don't do anything.
            else
            {
                // in case the above if does not activate, then we addandremove the process, and also check the objects in the pendingprocessescollection to put a new process or determine if the simulation has finished.
                collections_vm.ProcessInExecution[0]._Result = "Error"; // as the process has finished by error, we assign the process' result to "Error".

                MoveProcessExecutionToEnded();
            }
        }

        public void OnInputAndOutputKeyPressed(object sender, EventArgs e)
        {
            if (SimulationFinished) { } // if simulation has finished, then don't do anything.
            else if (!timer.IsEnabled) { } // if the program is paused, then this key will not take effect
            else if (collections_vm.ProcessInExecution[0]._ID == "null") { } // if the process is the null process, then don't do anything.
            else
            {
                ChangeProcessFromExecutionToBlocked(); // we first change the process from execution to blocked
                UpdateAssignAndCheckProcesses();
            }
        }

        private void ChangeProcessFromExecutionToBlocked()
        {
            collections_vm.BlockedProcesses.Add(collections_vm.ProcessInExecution[0]);
            collections_vm.ProcessInExecution.Remove(collections_vm.ProcessInExecution[0]);

            SimulationQuantumTime = 0;
            QuantumCounter.Content = "Contador del Quantum: " + SimulationQuantumTime;
        }

        private void UpdateProcessesAmount()
        {
            NewProcessesAmount = collections_vm.NewProcesses.Count;
            NewProcessesLabel.Content = "No. de procesos nuevos:  " + NewProcessesAmount;
        }

        private void UpdateProcessWaitingTime()
        {
            // when updating the processes times, we have to take in mind both the ready and blocked processes. we can do what we do here, using two fors, as we are not increasing the total waiting time, but actually calculating it by formula, so this is safe to use as it is.
            for (int i = 0; i < collections_vm.ReadyProcesses.Count; i++) collections_vm.ReadyProcesses[i].AssignProcessTimes("waiting_time", ref TotalTime);
            for (int i = 0; i < collections_vm.BlockedProcesses.Count; i++) collections_vm.BlockedProcesses[i].AssignProcessTimes("waiting_time", ref TotalTime);
        }

        private void PauseOrContinueSimulation()
        {
            // we use the SimulationFinished bool variable to allow or not allow the user to pause or start the timer, since when the simulation is finished, everything should be stopped and not continued.
            if (timer.IsEnabled && !SimulationFinished) // timer.IsEnabled allows us to check if the timer is started or not. this returns a bool, and if it's enabled, we will stop the simulation.
            {
                timer.Stop();
                PauseAndPlayButton.Content = "Seguir";
            }

            else if (!timer.IsEnabled && !SimulationFinished)
            {
                timer.Start();
                PauseAndPlayButton.Content = "Pausar";
            }

            Console.WriteLine("timer enabled: " + timer.IsEnabled);
        }

        public void OnLeaveSimulationButton(object sender, RoutedEventArgs args)
        {
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this);

            timer.Stop(); // the timer has to be stopped when left this page, otherwise it will keep going.

            mainWindow.ChangePageToMainPage(); // here we call the method to change the page.

        }

        private void UpdateTotalTime()
        {
            TotalTime++;
            TimeCounter.Content = "Contador de tiempo: " + TotalTime;
            SimulationQuantumTime++;
            QuantumCounter.Content = "Contador del Quantum: " + SimulationQuantumTime;
        }

        private void UpdateInExecutionProcessTime()
        {
            collections_vm.ProcessInExecution[0]._TimePassed++; // we have to add every time the timer ticks or goes off, so the time passed keeps going.
            collections_vm.ProcessInExecution[0]._TimeLeft--; // also, we have to lower the time left, until it gets to zero.

            // here we update the other times of the process
            collections_vm.ProcessInExecution[0].AssignProcessTimes("service_time", ref TotalTime);

            // besides adding time, we have to update it every single time the timer goes off to see the changes in the visuals.
            process_time_passed.Content = collections_vm.ProcessInExecution[0]._TimePassed.ToString();
            process_time_left.Content = collections_vm.ProcessInExecution[0]._TimeLeft.ToString();

        }

        private void AsignWorkingProcess()
        {
            if (collections_vm.ReadyProcesses.Count == 0) return;

            collections_vm.ProcessInExecution.Add(collections_vm.ReadyProcesses[0]); // we put the process into the processinexecution list, so we can hide it from the pendingprocesses list and in the visual part of the program.
            collections_vm.ReadyProcesses.Remove(collections_vm.ReadyProcesses[0]); // also, we erase that process from our pendingprocesses list.

            collections_vm.ProcessInExecution[0].AssignProcessTimes("waiting_time", ref TotalTime); // we update the waiting_time here. we do this so the waiting_time variable doesn't stay as null, even if it's on execution.

            CheckResponseTimeProcessInExecution();
            CheckServiceTimeProcessInExecution();

            // then we now update the labels and stuff
            UpdateLabelsForExecutionProcess();
        }

        private void UpdateLabelsForExecutionProcess() // this is just a function to update the labels for the in execution process.
        {
            process_operation.Content = collections_vm.ProcessInExecution[0]._Operation;
            process_id.Content = collections_vm.ProcessInExecution[0]._ID;
            process_time.Content = collections_vm.ProcessInExecution[0]._Time;

            process_time_passed.Content = collections_vm.ProcessInExecution[0]._TimePassed;
            process_time_left.Content = collections_vm.ProcessInExecution[0]._TimeLeft;
        }

        private void CheckTimeOfExecutionProcess()
        {
            // we check if the time left is zero, so then we can now remove the process from the pendingprocess list and add it to the done process list.
            if (collections_vm.ProcessInExecution[0]._TimeLeft == 0) MoveProcessExecutionToEnded();
        }

        private void CheckServiceTimeProcessInExecution() // this function is just used to prevent problems when the w key is pressed too fast, not allowing the program to assign a valid servicetime.
        {
            if (collections_vm.ProcessInExecution[0]._ServiceTime == "--") collections_vm.ProcessInExecution[0]._ServiceTime = 0.ToString();
        }

        private void UpdateBlockedProcessesTime() // in this function we update the times for the processes that are in the blocked state.
        {
            for (int i = 0; i < collections_vm.BlockedProcesses.Count; i++)
            {
                collections_vm.BlockedProcesses[i]._TimePassedBlocked++; // we first add to the time of the process

                collections_vm.BlockedProcesses[i].FirePropertyChangedEvent(); // we use this function to update the blocked time in real time.
            }

            CheckBlockedTime();
        }

        private void CheckBlockedTime() // in this function we now check the blocked time. we update and check in two separated functions to not have trouble with the index of the blockedprocesses collection.
        {
            for (int collection_index = 0; collection_index < collections_vm.BlockedProcesses.Count; collection_index++)
            {
                if (collections_vm.BlockedProcesses[collection_index]._TimePassedBlocked == 8) //we check if the time hits 8. in case this is true, then we return the process to the readyprocesses collection again.
                {
                    collections_vm.BlockedProcesses[collection_index]._TimePassedBlocked = 0; // in case the process has already hit 8, then we reset the time. this is done if the process enters the blocked state again.

                    ReturnProcessFromBlockedToReady(collection_index); // we call the function to pass the process from blocked to ready in the collections.

                    collection_index = -1; // we reinitialize the collection_index used in this for, since there can be multiple processes that hit 8 at the same time, so if we do this, when a process is removed, we initialize again and iterate again to not have trouble with the index of the collection.
                    // remember that we put -1 because in the next iteration the for will add 1 to the variable, so it starts at zero again.
                }
            }
        }

        private void ReturnProcessFromBlockedToReady(int collection_index) // in this function we get the index so we can change the process from blocked to ready in the collections.
        {
            collections_vm.ReadyProcesses.Add(collections_vm.BlockedProcesses[collection_index]);
            collections_vm.BlockedProcesses.Remove(collections_vm.BlockedProcesses[collection_index]);
        }

        // in this function we add and remove the process from the done and pending processes respectively.
        private void MoveProcessExecutionToEnded()
        {
            // before passing the process to the ended processes, we save the times for when the process ends.
            collections_vm.ProcessInExecution[0].AssignProcessTimes("ending_time", ref TotalTime);
            collections_vm.ProcessInExecution[0].AssignProcessTimes("return_time", ref TotalTime);

            collections_vm.EndedProcesses.Add(collections_vm.ProcessInExecution[0]); // we add the process to the doneprocesses
            collections_vm.ProcessInExecution.Remove(collections_vm.ProcessInExecution[0]); // and then we erase that process form the processinexecution list, as the process has been done

            SimulationQuantumTime = 0;
            QuantumCounter.Content = "Contador del Quantum: " + SimulationQuantumTime;

            UpdateAssignAndCheckProcesses();
        }

        private void UpdateAssignAndCheckProcesses() // this is just a default function for when a function needs to assignreadyprocesses, assign a working process and check the simulation state. since there are times when we have to first assign new processes to ready and then assign a working progress, so this is achieved by having this function.
        {
            collections_vm.AssignReadyProcesses(TotalTime);
            AsignWorkingProcess();
            CheckSimulationState();
        }

        private void CheckSimulationState()
        {
            // here we check if the amount processes of inside of the three collections (readyprocesses, inexecution and blocked). this is checked inside the collections_vm function AssignReadyProcesses.
            collections_vm.AssignReadyProcesses(TotalTime);

            if (CheckFinishSimulation()) return; // here we check if the simulation can be ended or not. if the simulation has ended this will return true, preventing it to check other things below this, causing crashes.

            CheckToAssignNullProcess(); // here we check if the null process has to be assigned.

            UpdateProcessesAmount(); // we update the processes amount at the end of this function too, in case something changed.
        }

        private bool CheckFinishSimulation()
        {
            // if the amount of processes of the three main collections (ready, blocked and execution) is zero, then stop the program. this function is called after checking if more processes can be added to the simulation.
            if (collections_vm.ReadyProcesses.Count + collections_vm.ProcessInExecution.Count + collections_vm.BlockedProcesses.Count == 0)
            {
                timer.Stop();
                UpdateFinishedAttributesLabels();
                SimulationFinished = true;
                Console.WriteLine("simulation finished.");
                return true;
            }

            return false; // if we can't end the simulation, return false.
        }

        private void CheckToAssignNullProcess()
        {
            // here we first check if the blockedprocesses list has at least one process, and also if the sum of readyprocesses and processinexecution is equal to zero. if so, then we have to assign the null process.
            if (collections_vm.BlockedProcesses.Count >= 1 && collections_vm.ReadyProcesses.Count + collections_vm.ProcessInExecution.Count == 0)
            {
                // as process in execution will have a count of zero, we add a new process. this will hold the program and will prevent it from shutting down or ending the simulation.
                collections_vm.ProcessInExecution.Add(new Process("null", "null", "null", -1)); // should find a way to put an infinite-like time, as this special process can be for as long as there's not a process that can be in execution.
                UpdateLabelsForExecutionProcess(); // we update the labels for this special process.
                collections_vm.SetNullProcessFlag(true);
            }
            // for the following else if, this is for when there's a process that can be in execution, so we can change the null process for that one. we check if we have the null process (null process will always have the "null" as ID) and also check if the readyprocess count is at least one, so we can put the process in execution.
            else if (collections_vm.ProcessInExecution[0]._ID == "null" && collections_vm.ReadyProcesses.Count >= 1)
            {
                collections_vm.ProcessInExecution.Remove(collections_vm.ProcessInExecution[0]); // remember that this is important: remove the null process from the list
                collections_vm.SetNullProcessFlag(false);
                AsignWorkingProcess(); // we assign the new process.

                SimulationQuantumTime = 0;
                QuantumCounter.Content = "Contador del Quantum: " + SimulationQuantumTime;
            }
        }

        private void UpdateFinishedAttributesLabels() // in this function we just update the labels, as every process is finished, so they don't show any process in execution.7
        {
            process_operation.Content = "";
            process_id.Content = "";
            process_time.Content = "";
            process_time_passed.Content = "";
            process_time_left.Content = "";
        }

        public async void OnControlsButtonPressed(object sender, RoutedEventArgs args) // this function pops up the controls window, which shows what controls the user can use.
        {
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = "Controles",
                ContentMessage = "P -- Pausar el programa\nC -- Continuar el programa\nW -- Terminar proceso con error\nE -- Entrada y salida (pasar el proceso a Bloqueado)\nN -- Crear nuevo proceso\nB -- Ver tabla de procesos",

                // we define the buttons here.
                ButtonDefinitions = new[]
                {
                    new ButtonDefinition { Name = "OK" }
                },

                WindowStartupLocation = WindowStartupLocation.CenterOwner,

                CanResize = false
            });

            var result = await box.ShowAsync();
        }

        public void OnOpenProcessesInfoTableButtonClicked(object sender, RoutedEventArgs args) { ProcessesInfoTableControl.Content = new ProcessesInfoTable(collections_vm, this); }

        public void OnSeeBCPTableKeyPressed(object sender, EventArgs e) { ProcessesInfoTableControl.Content = new ProcessesInfoTable(collections_vm, this); }

        private void CheckResponseTimeProcessInExecution() // we use this function to check if we have to assign the response time of the process. this is checked by using the GetResponseTime bool variable of the process class itself. it's set as true by default, and updates to false when the response time is taken.
        {
            if (collections_vm.ProcessInExecution[0].GetResponseTime)
            {
                collections_vm.ProcessInExecution[0].AssignProcessTimes("response_time", ref TotalTime);// here we check if the response time has already been assigned, and if not, then assign it. the response time should be assigned when the process enters execution processes list for the first time.
                collections_vm.ProcessInExecution[0].GetResponseTime = false;
            }
        }

        public void OnCreateProcessKeyPressed(object sender, EventArgs e)
        {
            if (!timer.IsEnabled) return;

            Console.WriteLine("creating new process");

            List<Process> processes_generated = Globals.GenerateRandomProcess(1);

            collections_vm.NewProcesses.Add(processes_generated[0]); // we add to the newprocesses list the process created.
            collections_vm.Processes.Add(processes_generated[0]); // we also need to add the process created to the main processes list. with this, we can show the new processes created in the simulation too in the BCP table.
            

            CheckSimulationState();
        }

        public void SetTimerToFalse()
        {
            if(!SimulationFinished) timer.Stop();
            
        }

        public void SetTimerToTrue()
        {
            if(!SimulationFinished) timer.Start();
        }
    }
}