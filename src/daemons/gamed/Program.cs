// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Daemon;

return await DaemonHost.RunAsync(args, ["game"], static services => services.AddGameServices());
