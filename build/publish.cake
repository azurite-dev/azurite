Task("Publish-Docker-Image")
.IsDependentOn("Build-Docker-Image")
.WithCriteria(() => !string.IsNullOrWhiteSpace(EnvironmentVariable("GITHUB_TOKEN")))
.Does(() => {
    var token = EnvironmentVariable("GITHUB_TOKEN");
    DockerLogin(new DockerRegistryLoginSettings{
        Password = token,
        Username = EnvironmentVariable("GITHUB_USER") ?? "agc93"
    }, "docker.pkg.github.com");
    DockerPush($"docker.pkg.github.com/azurite-dev/azurite/server:{packageVersion}");
});