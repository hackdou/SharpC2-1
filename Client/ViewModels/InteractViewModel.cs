using Client.Models;
using Client.Services;
using Client.Utilities;
using MvvmHelpers;
using MvvmHelpers.Commands;

using System.Text;

namespace Client.ViewModels
{
    [QueryProperty(nameof(DroneId), "droneId")]
    public class InteractViewModel : ViewModel
    {
        private Drone _drone;
        public Drone Drone
        {
            get { return _drone; }
            set
            {
                _drone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InfoLabel));
            }
        }

        public ObservableRangeCollection<DroneTaskRecord> TaskResults { get; set; } = new();

        private string _droneId;
        public string DroneId
        {
            get { return _droneId; }
            set
            {
                _droneId = value;
                OnPropertyChanged();
            }
        }

        public string InfoLabel
        {
            get
            {
                return Drone is null
                    ? ""
                    : $"{Drone.User} @ {Drone.Hostname}";
            }
        }

        private string _commandInput;
        public string CommandInput
        {
            get { return _commandInput; }
            set
            {
                _commandInput = value;
                OnPropertyChanged();
            }
        }

        private readonly SharpC2Api _api;
        private readonly SharpC2Hub _hub;
        private readonly SharpC2Commands _commands;

        public AsyncCommand TaskCommand { get; }

        public InteractViewModel(SharpC2Api api, SharpC2Hub hub, SharpC2Commands commands)
        {
            _api = api;
            _hub = hub;
            _commands = commands;

            _hub.OnNotifyDroneCheckedIn += OnNotifyDroneCheckedIn;
            _hub.OnNotifyDroneTaskUpdated += OnNotifyDroneTaskUpdated;

            TaskCommand = new AsyncCommand(HandleCommand);
        }

        private async void OnNotifyDroneCheckedIn(string id)
        {
            if (Drone is null || !id.Equals(Drone.Id))
                return;

            var drone = await _api.GetDrone(id);
            if (drone is null) return;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Drone = drone;
            });
        }

        private async void OnNotifyDroneTaskUpdated(string droneId, string taskId)
        {
            var task = await _api.GetDroneTask(droneId, taskId);

            if (task is null)
                return;

            // check if existing
            var existing = TaskResults.FirstOrDefault(t => t.TaskId.Equals(task.TaskId));

            if (existing is null)
            {
                // if it doesn't exist, add it
                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    TaskResults.Insert(0, task);
                });
            }
            else
            {
                // else update it
                var index = TaskResults.IndexOf(existing);
                
                if (index == -1)
                    return;

                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    TaskResults[index] = task;
                });
            }
        }

        private async Task HandleCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandInput)) return;
            var split = CommandInput.Split(' ');
            if (split.Length == 0) return;

            var alias = split[0];
            var parameters = split[1..];

            try
            {
                // print help
                if (alias.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp(split);
                    return;
                }

                // get the command alias
                var command = _commands.GetCommand(alias);

                // print error if command doesn't exist
                if (command is null)
                {
                    PrintError(alias, parameters, "Unknown command");
                    return;
                }

                // execute the command
                await ExecuteCommand(command, parameters);

            }
            catch (Exception e)
            {
                PrintError(alias, parameters, e.Message);
            }
            finally
            {
                CommandInput = "";
            }
        }

        private void PrintHelp(string[] parameters)
        {
            string help;

            if (parameters.Length == 1) help = _commands.GetHelp();
            else help = _commands.GetHelp(parameters[1]);

            if (string.IsNullOrWhiteSpace(help))
            {
                PrintError(parameters[0], parameters[1..], "Unknown command");
                return;
            }

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                var now = DateTime.UtcNow;

                TaskResults.Insert(0, new DroneTaskRecord
                {
                    TaskId = "n/a",
                    CommandAlias = parameters[0],
                    Parameters = parameters[1..],
                    Status = DroneTaskRecord.TaskStatus.Complete,
                    StartTime = now,
                    EndTime = now,
                    Result = Encoding.UTF8.GetBytes(help)
                });
            });
        }

        private void PrintError(string command, string[] parameters, string error)
        {
            Shell.Current.Dispatcher.Dispatch(() =>
            {
                var now = DateTime.UtcNow;

                TaskResults.Insert(0, new DroneTaskRecord
                {
                    TaskId = "n/a",
                    DroneFunction = command,
                    Parameters = parameters,
                    Status = DroneTaskRecord.TaskStatus.Complete,
                    StartTime = now,
                    EndTime = now,
                    Result = Encoding.UTF8.GetBytes($"[!] {error}")
                });
            });
        }

        private async Task ExecuteCommand(DroneCommand command, string[] parameters)
        {
            string artefactPath = null;
            byte[] artefact = null;

            // check parameters
            for (var i = 0; i < command.Arguments.Length; i++)
            {
                // artefact?
                if (command.Arguments[i].Artefact)
                {
                    // default path?
                    try
                    {
                        artefactPath = string.IsNullOrWhiteSpace(command.Arguments[i].DefaultValue)
                        ? parameters[i] // if optional and not included, this will throw index out of bounds
                        : command.Arguments[i].DefaultValue;
                    }
                    catch
                    {
                        continue;
                    }

                    // if embedded
                    if (command.Arguments[i].Embedded)
                    {
                        artefact = await Helpers.GetEmbeddedResource(artefactPath);
                    }
                    else
                    {
                        if (!File.Exists(artefactPath))
                        {
                            PrintError(command.Alias, parameters, $"File \"{artefactPath}\" not found");
                            return;
                        }

                        artefact = File.ReadAllBytes(artefactPath);
                    }

                    // remove local path from parameters
                    parameters = parameters.Where(p => !p.Equals(artefactPath)).ToArray();

                    continue;
                }

                // mandatory?
                if (!command.Arguments[i].Optional)
                {
                    // is parameters too small
                    if (parameters.Length < i || parameters.Length == 0)
                    {
                        // check for a default value
                        if (!string.IsNullOrWhiteSpace(command.Arguments[i].DefaultValue))
                        {
                            Array.Resize(ref parameters, parameters.Length + 1);
                            parameters[i - 1] = command.Arguments[i].DefaultValue;
                        }
                        else
                        {
                            PrintError(command.Alias, parameters, $"Missing mandatory argument \"{command.Arguments[i].Name}\"");
                            return;
                        }
                    }
                }   
            }

            // go
            await _api.TaskDrone(Drone.Id, command.Function, command.Alias, parameters, artefactPath, artefact);
        }
    }
}