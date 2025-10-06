{ fetchFromGitHub }:

{
  dotnet-crypto = fetchFromGitHub {
    owner = "ProtonDriveApps";
    repo = "dotnet-crypto";
    rev = "5aac829ce0ab4b21f7ad61b4c5b348b168e94d9b";
    hash = "sha256-+YrM4ByfOZJr8rl+VV/j6I6+uvUVRUDTCH8xKBxkFYU=";
  };

  sdk-tech-demo = fetchFromGitHub {
    owner = "mat8913";
    repo = "proton-sdk-tech-demo";
    rev = "bd34d0c6e7f4a8462f7e127038d618f952eeef44";
    hash = "sha256-avbcBuR4Bxwt6s8+EAawNiylkRntRidveQpqedfrflA=";
  };
}
