using Path = System.IO.Path;
/*
  Target list:
    - build
    - test
    - pack
    - publish
    - ci
    - local

*/

readonly string MainProjectRoot = Path.GetFullPath("src/ReflectionExtended");
readonly string TestProjectRoot = Path.GetFullPath("test/ReflectionExtended.Tests");
readonly string MainProjectPath = Path.Combine(MainProjectRoot, "ReflectionExtended.csproj");
readonly string TestProjectPath = Path.Combine(TestProjectRoot, "ReflectionExtended.Tests.csproj");

/*
Tasks:
  restore/main
  restore/test

  build/main:debug
  build/main:release
  build/test


*/
