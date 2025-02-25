echo Cleaning previous build files...
rmdir /s /q "bin" 
rmdir /s /q "obj" 

echo Checking for duplicate files...
if exist "MainForm copy.cs" del "MainForm copy.cs"
if exist "MainForm - Copy.cs" del "MainForm - Copy.cs" 
if exist "Copy of MainForm.cs" del "Copy of MainForm.cs"

echo Checking for hidden duplicates...
dir /b /s MainForm*.cs > temp_files.txt
findstr /v /i "MainForm.cs MainForm.Designer.cs" temp_files.txt > duplicate_files.txt
for /f "tokens=*" %%a in (duplicate_files.txt) do del "%%a"
del temp_files.txt
del duplicate_files.txt

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