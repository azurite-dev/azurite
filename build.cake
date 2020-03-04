#load "build/helpers.cake"
#addin nuget:?package=Cake.Docker
#addin nuget:?package=Cake.AzCopy&prerelease

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp3.1");

///////////////////////////////////////////////////////////////////////////////
// VERSIONING
///////////////////////////////////////////////////////////////////////////////

var packageVersion = string.Empty;
#load "build/version.cake"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/Azurite.sln");
var solution = ParseSolution(solutionPath);
var projects = GetProjects(solutionPath, configuration);
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
// var runtimes = new List<string> { "win10-x64", "osx.10.12-x64", "ubuntu.16.04-x64", "ubuntu.14.04-x64", "centos.7-x64", "debian.8-x64", "rhel.7-x64" };
var PackagedRuntimes = new List<string> { "centos", "ubuntu", "debian", "fedora", "rhel" };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
    packageVersion = BuildVersion(fallbackVersion);
	if (FileExists("./build/.dotnet/dotnet.exe")) {
		Information("Using local install of `dotnet` SDK!");
		Context.Tools.RegisterFile("./build/.dotnet/dotnet.exe");
	}
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projects.AllProjectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	foreach (var project in projects.AllProjectPaths) {
		DotNetCoreRestore(project.FullPath);
	}
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	var settings = new DotNetCoreBuildSettings {
		Configuration = configuration,
		NoIncremental = true,
	};
	DotNetCoreBuild(solutionPath, settings);
});

Task("Run-Unit-Tests")
	.IsDependentOn("Build")
	.Does(() =>
{
    CreateDirectory(testResultsPath);
	if (projects.TestProjects.Any()) {

		var settings = new DotNetCoreTestSettings {
			Configuration = configuration
		};

		foreach(var project in projects.TestProjects) {
			DotNetCoreTest(project.Path.FullPath, settings);
		}
	}
});

Task("Post-Build")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "build");
	CreateDirectory(artifacts + "build/Azurite");
	var frameworkDir = $"{artifacts}build/Azurite/";
	CreateDirectory(frameworkDir);
	var files = GetFiles($"./src/Azurite/bin/{configuration}/{framework}/*.*");
	CopyFiles(files, frameworkDir);
	CopyFiles(GetFiles("./Dockerfile*"), artifacts);
	//CopyFileToDirectory("./template/openshift.yaml", artifacts);
});

Task("Publish-Runtime")
	.IsDependentOn("Post-Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "publish/");
	var projectDir = $"{artifacts}publish";
	CreateDirectory(projectDir);
    DotNetCorePublish("./src/Azurite/Azurite.csproj", new DotNetCorePublishSettings {
        OutputDirectory = projectDir + "/dotnet-any"
    });
    var runtimes = new[] { "linux-x64", "win-x64"};
    foreach (var runtime in runtimes) {
	var runtimeDir = $"{projectDir}/server/{runtime}";
	var consoleDir = $"{projectDir}/console/{runtime}";
	CreateDirectory(runtimeDir);
	CreateDirectory(consoleDir);
	Information("Publishing for {0} runtime", runtime);
	/*
    var rSettings = new DotNetCoreRestoreSettings {
		ArgumentCustomization = args => args.Append("-r " + runtime)
	};
	DotNetCoreRestore(solutionPath, rSettings);
    */
	var settings = new DotNetCorePublishSettings {
		Runtime = runtime,
		Configuration = configuration,
		//NoBuild = true
	};
	DotNetCorePublish("./src/Azurite/Azurite.csproj", settings);
	var publishDir = $"./src/Azurite/bin/{configuration}/{framework}/{runtime}/publish/";
	
	CopyDirectory(publishDir, runtimeDir);
	//CopyDirectory(conPublishDir, consoleDir);
	CreateDirectory($"{artifacts}archive");
	Zip(runtimeDir, $"{artifacts}archive/azurite-{runtime}.zip");
    }
});

Task("Build-Linux-Packages")
	.IsDependentOn("Publish-Runtime")
	.WithCriteria(IsRunningOnUnix())
	.Does(() => 
{
	Information("Building packages in new container");
	CreateDirectory($"{artifacts}/packages/");
	foreach(var project in projects.SourceProjects.Where(p => p.Name == "Azurite")) {
        var runtime = "linux-x64";
        var sourceDir = MakeAbsolute(Directory($"{artifacts}publish/server/{runtime}"));
        var packageDir = MakeAbsolute(Directory($"{artifacts}packages/{runtime}"));
		foreach (var package in GetPackageFormats()) {
			var runSettings = new DockerContainerRunSettings {
				Name = $"docker-fpm-{(runtime.Replace(".", "-"))}",
				Volume = new[] { 
					$"{sourceDir}:/src:ro", 
					$"{packageDir}:/out:rw",
					$"{MakeAbsolute(Directory("./scripts/"))}:/scripts:ro",
				},
				Workdir = "/out",
				Rm = true,
				//User = "1000"
			};
			var opts = "-s dir -a x86_64 --force -m \"Alistair Chapman <alistair@agchapman.com>\" -n azurite-server --after-install /scripts/post-install.sh --before-remove /scripts/pre-remove.sh";
			DockerRun(runSettings, "tenzer/fpm", $"{opts} -v {packageVersion} --iteration {package.Key} {package.Value} /src/=/usr/lib/azurite/");
		}
		//DeleteDirectory(scriptsDir, true);
	}
});

Task("Build-Console")
	.IsDependentOn("Publish-Runtime")
	.Does(() => 
{
	Information("Building console packages");
	var runtimes = new[] { "linux-x64", "win-x64"};
    foreach (var runtime in runtimes) {
		var consoleDir = $"{artifacts}publish/console/{runtime}";
		CreateDirectory(consoleDir);
		Information("Publishing for {0} runtime", runtime);
		var settings = new DotNetCorePublishSettings {
			Runtime = runtime,
			Configuration = configuration,
			MSBuildSettings = new DotNetCoreMSBuildSettings()
				.WithProperty("PublishSingleFile", "true")
				.WithProperty("DebugType", "none")
				.WithProperty("PublishTrimmed", "true"),
			OutputDirectory = consoleDir
		};
		DotNetCorePublish("./src/Azurite.Console/Azurite.Console.csproj", settings);
		var publishDir = $"./src/Azurite/bin/{configuration}/{framework}/{runtime}/publish/";
		// CopyDirectory(publishDir, consoleDir);
	}
});

Task("Build-Console-Packages")
	.IsDependentOn("Build-Console")
	.WithCriteria(IsRunningOnUnix())
	.Does(() =>
{
	var runtime = "linux-x64";
	var sourceDir = MakeAbsolute(Directory($"{artifacts}publish/console/{runtime}"));
	var packageDir = MakeAbsolute(Directory($"{artifacts}packages/{runtime}"));
	foreach (var package in GetPackageFormats()) {
		var runSettings = new DockerContainerRunSettings {
			Name = $"docker-fpm-{(runtime.Replace(".", "-"))}",
			Volume = new[] { 
				$"{sourceDir}:/src:ro", 
				$"{packageDir}:/out:rw"
			},
			Workdir = "/out",
			Rm = true,
			//User = "1000"
		};
		var opts = "-s dir -a x86_64 --force -m \"Alistair Chapman <alistair@agchapman.com>\" -n azurite-cli";
		DockerRun(runSettings, "tenzer/fpm", $"{opts} -v {packageVersion} --iteration {package.Key} {package.Value} /src/=/usr/local/bin/");
		}
});

/*
Task("Build-Windows-Packages")
	.IsDependentOn("Publish-Runtimes")
	.WithCriteria(IsRunningOnUnix())
	.Does(() => 
{
	Information("Building Chocolatey package in new container");
	CreateDirectory($"{artifacts}packages");
	foreach(var project in projects.SourceProjects) {
		foreach(var runtime in runtimes.Where(r => r.StartsWith("win"))) {
			var publishDir = $"{artifacts}publish/{project.Name}/{runtime}";
			CopyFiles(GetFiles($"./build/{runtime}.nuspec"), publishDir);
			var sourceDir = MakeAbsolute(Directory(publishDir));
			var packageDir = MakeAbsolute(Directory($"{artifacts}packages/{runtime}"));
			var runSettings = new DockerRunSettings {
				Name = $"docker-choco-{(runtime.Replace(".", "-"))}",
				Volume = new[] { 
					$"{sourceDir}:/src/{runtime}:ro",
					$"{packageDir}:/out:rw",
					$"{sourceDir}/{runtime}.nuspec:/src/package.nuspec:ro"
				},
				Workdir = "/src",
				Rm = true
			};
			var opts = @"-y -v
				--outputdirectory /out/
				/src/package.nuspec";
			DockerRun(runSettings, "agc93/mono-choco", $"choco pack --version {versionInfo.NuGetVersionV2} {opts}");
		}
	}
});
*/


Task("Build-Docker-Image")
	//.WithCriteria(IsRunningOnUnix())
	.IsDependentOn("Build-Linux-Packages")
	.Does(() =>
{
	Information("Building Docker image...");
	CopyFileToDirectory("./build/Dockerfile.build", artifacts);
	var bSettings = new DockerImageBuildSettings {
        Tag = new[] { $"azurite-dev/azurite:{packageVersion}", $"quay.io/azurite/azurite:{packageVersion}"},
        File = artifacts + "Dockerfile.build"
    };
	DockerBuild(bSettings, artifacts);
	DeleteFile(artifacts + "Dockerfile.build");
});

#load "build/publish.cake"

Task("Default")
    .IsDependentOn("Post-Build");

Task("Publish")
	.IsDependentOn("Build-Linux-Packages")
	.IsDependentOn("Build-Console-Packages")
	.IsDependentOn("Build-Docker-Image");

RunTarget(target);