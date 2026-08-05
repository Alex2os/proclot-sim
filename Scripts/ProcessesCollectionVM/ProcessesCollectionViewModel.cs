using System.Collections.ObjectModel; // this is used for the observablecollection in the program. the purpose of the collections is to notify other code when something is modified inside this collection, so, we can say, sends an alarm when that happens.
using System.ComponentModel;
using System;

// this class is to use with our collection, as here is where all the objects/processes will be saved.

namespace MyAp
{

    public class ProcessesCollectionViewModel : INotifyPropertyChanged // remember that this MainWindowViewModel inherits from INotifyPropertyChanged, allowing us to notify other clients (in this case the axaml file) that our collection changed if so.
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Process> Processes { get; } = new(); // we create an observable collection and there we can save our processes. this collection is just to save the processes that the user introduces in the starting page
        public ObservableCollection<Process> EndedProcesses { get; } = new(); // a collection for the ended processes
        public ObservableCollection<Process> ReadyProcesses { get; } = new(); // a collection for the pending processes.
        public ObservableCollection<Process> ProcessInExecution { get; } = new(); // a list to put the process that is currently in execution.
        public ObservableCollection<Process> BlockedProcesses { get; } = new(); // collection for the blocked processes
        public ObservableCollection<Process> NewProcesses { get; } = new(); // this is the collection to save all the new processes.
        private bool NullProcessActive = false; // this variable is used for when we have to add more process, but there's a null process, making the sum of all 3 collections (blocked, ready and execution) equal four, when in reality there are just 3 real processes.
        // this is activated or set to true when the null process is present. then we use this in the AssignReadyProcesses function in this script.

        public ProcessesCollectionViewModel()
        {

        }

        // this function receives a parameter which is the time of the simulation. this is used to assign the arriving time of a process.
        public void AssignReadyProcesses(int actual_time) // with this function we assign a pending process. this function may be called when a process can enter the space in pendingprocessescollection.
        {
            // the following is a condition to check if we have to add more processes or not to the ready processes. we have two conditions: if the count is 4 (which is when all the slots are full) then don't add, and also when the count is 5, because this is when the null process is also in when the 4 actual processes are in the blocked state.
            // with all of this, this prevents that new processes pass to ready when it's not the time to do so.
            if (ReadyProcesses.Count + BlockedProcesses.Count + ProcessInExecution.Count == 4 && !NullProcessActive) return; // if the nullprocessactive isn't set to true, then return if there are already four processes. if it's true, don't return, as we would have 3 real process and a null process, making the sum 4, but as we said, one is the null process.

            if (ReadyProcesses.Count + BlockedProcesses.Count + ProcessInExecution.Count == 5) return;
            
            // have in mind that the three collections used here (readyprocesses, inexecution and blocked) are the collections that, combined, can only have 4 processes. this is why we use the sum of these three inside here to check if we have to add processes or not.
            for (int i = 0; i < NewProcesses.Count; i++)
            {
                if (ReadyProcesses.Count + BlockedProcesses.Count + ProcessInExecution.Count < 4) { } // we have here that if the sum of the 3 lists (readyprocesses, inexecution and blocked) is less than 4, then keep adding until hit 4
                else if (ReadyProcesses.Count + BlockedProcesses.Count + ProcessInExecution.Count == 4 && !NullProcessActive) break; // if the limit is already four, then break and don't keep adding. we add here the same condition with the nullprocessactive here, so we add one more even if there's the nullprocess. in the end there would be 5 processes, but the null process will disappear again, making now the 4 real processes again.
                else if (ReadyProcesses.Count + BlockedProcesses.Count + ProcessInExecution.Count == 5 && NullProcessActive) break; // this is for when there are already 5 processes (this is for when the nullprocessactive is set to true) and this breaks the cycle to leave.

                NewProcesses[0].AssignProcessTimes("arriving_time", ref actual_time); // in case a process can be added, we assign its arriving time when it enters the readyprocesses collection.
                Console.WriteLine(NewProcesses[0]._ArrivingTime);
                ReadyProcesses.Add(NewProcesses[0]); // we add the process in newprocesses to readyprocesses
                NewProcesses.Remove(NewProcesses[0]); // and then we erase that process from the new processes collection. with this, we are passing the process from one collection to another.

            }
        }

        public void SetNullProcessFlag(bool state) { NullProcessActive = state; }

        public void AssignListsAndVariables() // in this function we clear all the lists and variables (if there are any variables to be cleared)
        {
            ReadyProcesses.Clear(); // we clear the readyprocesses list if we start over.
            EndedProcesses.Clear(); // also the ended processes.
            ProcessInExecution.Clear();
            NewProcesses.Clear();
            BlockedProcesses.Clear();
        }

        public void ClearMainProcessList()
        {
            Processes.Clear();
        }

        public void ReAssignProcessesValues()
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                Processes[i]._Result = Processes[i].RealResult.ToString();

                // times for the processes
                Processes[i]._TimeLeft = Processes[i]._Time;
                Processes[i]._TimePassed = 0;
                Processes[i]._TimePassedBlocked = 0;

                Processes[i]._WaitingTime = "--";
                Processes[i]._ArrivingTime = "--";
                Processes[i]._ServiceTime = "--";
                Processes[i].RealServiceTime = 0;
                Processes[i]._ResponseTime = "--";
                Processes[i]._ReturnTime = "--";
                Processes[i]._EndingTime = "--";
                Processes[i].GetResponseTime = true;
            }
        }

        public void AssignNewProcesses() // this is to assign the new processes from the processes collection.
        {
            for (int i = 0; i < Processes.Count; i++) NewProcesses.Add(Processes[i]);
        }

    }
}
