:; set -eo pipefail
:; SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
:; ${SCRIPT_DIR}/build.sh "$@"
:; exit $?

rem powershell -ExecutionPolicy ByPass -NoProfile -File "%~dp0build.ps1" %*

@ECHO OFF
dotnet "%~dp0build/bin/Debug/_build.dll" %*
