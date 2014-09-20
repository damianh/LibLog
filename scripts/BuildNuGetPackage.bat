@echo off
pushd %~dp0

if exist "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" (

    call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
    goto build
    
) else (

    echo Build cannot continue because "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" does not exist.
    goto Exit
)

:Build
msbuild ..\src\LibLog.sln
if not "%errorlevel%" == "0" goto error

:Pack
..\src\.NuGet\NuGet Pack ..\src\LibLog\LibLog.nuspec
if not "%errorlevel%" == "0" goto error

echo.
echo.
echo Successfully built NuGet package.

goto Exit

:Error
echo.
echo.
echo ************************
echo *** AN ERROR OCCURED ***
echo ************************

:Exit
popd
echo.
echo.
pause