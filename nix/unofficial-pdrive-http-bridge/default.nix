{
  buildDotnetModule,
  dotnetCorePackages,
  dotnet-crypto-cs,
  sdk-tech-demo
}:

buildDotnetModule rec {
  pname = "unofficial-pdrive-http-bridge";
  version = "0.1";

  src = ../..;

  projectFile = "unofficial-pdrive-http-bridge/unofficial-pdrive-http-bridge.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  buildInputs = [
    dotnet-crypto-cs
    sdk-tech-demo
  ];
}
