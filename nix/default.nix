{ callPackage }:

rec {
  dotnet-crypto-go = callPackage ./dotnet-crypto-go { };

  dotnet-crypto-cs = callPackage ./dotnet-crypto-cs { inherit dotnet-crypto-go; };

  sdk-tech-demo = callPackage ./sdk-tech-demo { inherit dotnet-crypto-cs; };

  unofficial-pdrive-http-bridge = callPackage ./unofficial-pdrive-http-bridge { inherit dotnet-crypto-cs sdk-tech-demo; };
}
