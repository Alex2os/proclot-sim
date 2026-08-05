using Avalonia.Controls;

using Avalonia.Media;


namespace MyAp;


public partial class MainWindow : Window // remember that this class is an instance during all the lifetime of the app while it's running, so any data is saved while the app is running. at least this is on the mainwindow, which is the program itself.
{

    private readonly ProcessesCollectionViewModel processes_collection; // we create a viewmodel of the processescollection. we have to do this so this is global, and the mainwindow class can pass this parameter between the pages, as we can see in the below code.

    public MainWindow()
    {
        InitializeComponent();
        processes_collection = new ProcessesCollectionViewModel(); // when we start the app, we create a new ProcessesCollectionViewModel object, so this will can be sent to the pages from here, the mainwindow, which is a class that will not be destructed until the app is closed.
        MainContent.Content = new StartingPage(processes_collection); // we get the starting page by default.
    }

    public void ChangePageToSimulation()
    {
        MainContent.Content = new SimulationPage(processes_collection); // we send the processes_collection to all our pages, since they are going to need this to do their operations.
    }

    public void ChangePageToMainPage()
    {
        MainContent.Content = new StartingPage(processes_collection);
    }

}

