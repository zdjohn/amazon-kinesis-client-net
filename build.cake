#tool "nuget:?package=NUnit.Runners&version=2.6.4"

var target = Argument("target", "Default");

Task("Build")
  .Does(() =>
{
  MSBuild("./AWS_KCL_DotNet.sln", new MSBuildSettings {
    Verbosity = Verbosity.Minimal,
    Configuration = "Release",
    PlatformTarget = PlatformTarget.x86
    });
});

Task("Nunit")
	.IsDependentOn("Build")
	.Does(()=>
{
	var assemblies = GetFiles("./ClientLibrary.Test/bin/release/*.Test.dll");
	NUnit(assemblies);
});

Task("Bootstrap")
	.IsDependentOn("Nunit")
	.Does(()=>{
		CleanDirectories("./jars");
		var boostrap = MakeAbsolute(File("./Bootstrap/bin/Release/Bootstrap.exe"));
		Information(boostrap);
		StartProcess(boostrap, new ProcessSettings {
				Arguments = new ProcessArgumentBuilder()
					.Append("-p kcl.property")
				});
	});

Task("Nuget")
    .IsDependentOn("Bootstrap")
    .Does(() =>
{
	CleanDirectories("./.nuget");
	var nuGetPackSettings   = new NuGetPackSettings {
                                     BasePath                = "./ClientLibrary/",
                                     OutputDirectory         = "./.nuget"
                                 };
	NuGetPack("./ClientLibrary/ClientLibrary.nuspec", nuGetPackSettings);
});

Task("Default")
    .IsDependentOn("Nuget")
	.Does(() =>
{
	Information("All done!");
});

RunTarget(target);