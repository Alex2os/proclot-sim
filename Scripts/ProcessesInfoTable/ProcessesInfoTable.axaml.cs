using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace MyAp;

public partial class ProcessesInfoTable : UserControl
{
    private SimulationPage sim_page_vm;

    public ProcessesInfoTable(ProcessesCollectionViewModel collections_vm, SimulationPage simulation_page_vm)
    {
        InitializeComponent();
        DataContext = collections_vm;

        sim_page_vm = simulation_page_vm;

        sim_page_vm.SetTimerToFalse();

        ProcessesInfoTableBorder.IsVisible = true; // when an instance of this class is created, the mainborder will always be visible. this is as if the close button is selected, the border will turn invisible or not visible, so when entering again this window we havo to make it visible.
    }

    public void OnButtonCloseWindow(object sender, RoutedEventArgs args)
    {
        ProcessesInfoTableBorder.IsVisible = false; // if the button to close is pressed, then turn invisible the border and it will not be seen by the user. we dont have to put here another options, as turning this invisible will "close this", and when entering again this class, the consturctor will put this in a visible state again.
        sim_page_vm.SetTimerToTrue();
    }


}