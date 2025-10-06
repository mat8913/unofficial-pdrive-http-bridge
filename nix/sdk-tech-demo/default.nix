{
  stdenvNoCC,
  buildDotnetModule,
  callPackage,
  dotnetCorePackages,
  dotnet-crypto-cs
}:

let csprojects = [
  "Proton.Sdk"
  "Proton.Sdk.Instrumentation"
  "Proton.Sdk.Drive"
];

in

buildDotnetModule rec {
  pname = "sdk-tech-demo";
  version = "0.2.10-mat8913";

  src = (callPackage ../sources.nix { }).sdk-tech-demo;

  projectFile = builtins.map (csproject: "src/${csproject}/${csproject}.csproj") csprojects;
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  buildInputs = [
    dotnet-crypto-cs
  ];

  dotnetFlags = "-p:Version=${version}";

  packNupkg = true;

  runtimeId = dotnetCorePackages.systemToDotnetRid stdenvNoCC.hostPlatform.system;

  # Workaround for https://github.com/NixOS/nixpkgs/issues/283430
  preInstall = builtins.map (csproject: ''
    cp "src/${csproject}/bin/Release/net9.0/${runtimeId}/"{*.dll,*.pdb} src/${csproject}/bin/Release/net9.0/
  '') csprojects;
}
