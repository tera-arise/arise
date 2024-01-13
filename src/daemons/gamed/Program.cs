using Arise.Daemon;

return await DaemonHost.RunAsync(args, ["game"], static services => services.AddGameServices());
