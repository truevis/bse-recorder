echo Cleaning previous build files...
rmdir /s /q "bin" 
rmdir /s /q "obj" 

echo.
echo Building application...
dotnet publish -c Release -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=none

echo.
echo Copying executable to output folder...
if not exist "output" mkdir "output" 2>nul
copy /y "bin\Release\net8.0-windows\win-x64\publish\AudioRecorder.exe" "output\AudioRecorder.exe"

echo.
echo Cleaning up build files...
rmdir /s /q "bin" 
rmdir /s /q "obj" 

echo.
if exist "bin" (
    echo WARNING: bin folder could not be fully removed
) else if exist "obj" (
    echo WARNING: obj folder could not be fully removed
) else (
    echo All build files successfully cleaned up
)

echo.
echo Build complete. Standalone exe is in the output folder.