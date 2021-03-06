﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Definitions;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Plots;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RHostClientApp : IRHostClientApp {
        private static readonly Lazy<IRHostClientApp> InstanceLazy = new Lazy<IRHostClientApp>(() => new RHostClientApp());

        public static IRHostClientApp Instance => InstanceLazy.Value;

        private RHostClientApp() { }

        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        public async Task ShowErrorMessage(string message) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsAppShell.Current.ShowErrorMessage(message);
        }

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public async System.Threading.Tasks.Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return VsAppShell.Current.ShowMessage(message, buttons);
        }

        /// <summary>
        /// Displays R help URL in a browser on in the host app-provided window
        /// </summary>
        public async Task ShowHelp(string url) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (RToolsSettings.Current.HelpBrowser == HelpBrowserType.External) {
                Process.Start(url);
            } else {
                HelpWindowPane pane = ToolWindowUtilities.ShowWindowPane<HelpWindowPane>(0, focus: false);
                pane.Component.Navigate(url);
            }
        }

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        public async Task Plot(string filePath, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IPlotHistoryProvider>();
            var history = historyProvider.GetPlotHistory(sessionProvider.GetInteractiveWindowRSession());
            history.PlotContentProvider.LoadFile(filePath);
        }

        /// <summary>
        /// Given CRAN mirror name returns URL
        /// </summary>
        public string CranUrlFromName(string mirrorName) {
            return CranMirrorList.UrlFromName(mirrorName);
        }
    }
}
