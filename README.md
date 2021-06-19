# Shutdown Scheduler
WPF application used for scheduling shutdowns of Windows systems. The program uses a Windows Service (https://docs.microsoft.com/en-us/dotnet/framework/windows-services/) to
shutdown the system so that the program does not have to remain running on the users machine.

There are two shutdown modes: shutdown in, and shutdown at. 

#### Shutdown In
Allows scheduling a shutdown in 'x' amount of minutes/hours from the current time. For example, if the current time is 12:45 PM and you schedule a shutdown in 30 minutes,
the program will shutdown the system at 1:15 PM.

#### Shutdown At
Allows scheduling shutdowns at a specific date and time. For example, a shutdown could be scheduled for on June 12, 2021 at 3:37 PM.
