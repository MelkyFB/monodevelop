﻿//
// PackageReinstaller.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using MonoDevelop.PackageManagement.NodeBuilders;

namespace MonoDevelop.PackageManagement
{
	internal class PackageReinstaller
	{
		IBackgroundPackageActionRunner runner;

		public PackageReinstaller ()
			: this (
				PackageManagementServices.BackgroundPackageActionRunner)
		{
		}

		public PackageReinstaller (
			IBackgroundPackageActionRunner runner)
		{
			this.runner = runner;
		}

		public void Run (PackageReferenceNode packageReferenceNode)
		{
			ProgressMonitorStatusMessage progressMessage = ProgressMonitorStatusMessageFactory.CreateRetargetingSinglePackageMessage (packageReferenceNode.Id);
			Run (packageReferenceNode, progressMessage);
		}

		public void Run (PackageReferenceNode packageReferenceNode, ProgressMonitorStatusMessage progressMessage)
		{
			try {
				var solutionManager = PackageManagementServices.Workspace.GetSolutionManager (packageReferenceNode.Project.ParentSolution);

				var nugetProject = solutionManager.GetNuGetProject (packageReferenceNode.Project);

				var action = new ReinstallNuGetPackageAction (
					packageReferenceNode.Project,
					nugetProject,
					solutionManager);
				action.PackageId = packageReferenceNode.Id;
				action.Version = packageReferenceNode.Version;

				runner.Run (progressMessage, action);
			} catch (Exception ex) {
				runner.ShowError (progressMessage, ex);
			}
		}
	}
}

