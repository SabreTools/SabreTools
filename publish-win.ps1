# This batch file assumes the following:
# - .NET 10.0 (or newer) SDK is installed and in PATH
# - 7-zip commandline (7z.exe) is installed and in PATH
# - Git for Windows is installed and in PATH
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Optional parameters
param(
    [Parameter(Mandatory = $false)]
    [Alias("UseAll")]
    [switch]$USE_ALL,

    [Parameter(Mandatory = $false)]
    [Alias("IncludeDebug")]
    [switch]$INCLUDE_DEBUG,

    [Parameter(Mandatory = $false)]
    [Alias("NoBuild")]
    [switch]$NO_BUILD,

    [Parameter(Mandatory = $false)]
    [Alias("NoArchive")]
    [switch]$NO_ARCHIVE
)

# Set the current directory as a variable
$BUILD_FOLDER = $PSScriptRoot

# Set the current commit hash
$COMMIT = git log --pretty=format:"%H" -1

# Output the selected options
Write-Host "Selected Options:"
Write-Host "  Use all frameworks (-UseAll)          $USE_ALL"
Write-Host "  Include debug builds (-IncludeDebug)  $INCLUDE_DEBUG"
Write-Host "  No build (-NoBuild)                   $NO_BUILD"
Write-Host "  No archive (-NoArchive)               $NO_ARCHIVE"
Write-Host " "

# Create the build matrix arrays
$FRAMEWORKS = @('net10.0')
$RUNTIMES = @('win-x86', 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')

# Use expanded lists, if requested
if ($USE_ALL.IsPresent) {
    $FRAMEWORKS = @('net20', 'net35', 'net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0', 'net10.0')
}

# Create the filter arrays
$SINGLE_FILE_CAPABLE = @('net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0', 'net10.0')
$VALID_APPLE_FRAMEWORKS = @('net6.0', 'net7.0', 'net8.0', 'net9.0', 'net10.0')
$VALID_CROSS_PLATFORM_FRAMEWORKS = @('netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0', 'net10.0')
$VALID_CROSS_PLATFORM_RUNTIMES = @('win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')

# Only build if requested
if (!$NO_BUILD.IsPresent) {
    # Restore Nuget packages for all builds
    Write-Host "Restoring Nuget packages"
    dotnet restore

    # Create Nuget Packages
    dotnet pack SabreTools.Core\SabreTools.Core.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatFiles\SabreTools.DatFiles.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatItems\SabreTools.DatItems.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatTools\SabreTools.DatTools.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.FileTypes\SabreTools.FileTypes.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.Reports\SabreTools.Reports.csproj --output $BUILD_FOLDER

    # Build SabreTools
    foreach ($FRAMEWORK in $FRAMEWORKS) {
        foreach ($RUNTIME in $RUNTIMES) {
            # Output the current build
            Write-Host "===== Build SabreTools - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only .NET 5 and above can publish to a single file
            if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK) {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish SabreTools\SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                }
                dotnet publish SabreTools\SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            }
            else {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish SabreTools\SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                }
                dotnet publish SabreTools\SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            }
        }
    }
}

# Only create archives if requested
if (!$NO_ARCHIVE.IsPresent) {
    # Create SabreTools archives
    foreach ($FRAMEWORK in $FRAMEWORKS) {
        foreach ($RUNTIME in $RUNTIMES) {
            # Output the current build
            Write-Host "===== Archive SabreTools - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only include Debug if set
            if ($INCLUDE_DEBUG.IsPresent) {
                Set-Location -Path $BUILD_FOLDER\SabreTools\bin\Debug\${FRAMEWORK}\${RUNTIME}\publish\
                7z a -tzip $BUILD_FOLDER\SabreTools_${FRAMEWORK}_${RUNTIME}_debug.zip *
            }
        
            Set-Location -Path $BUILD_FOLDER\SabreTools\bin\Release\${FRAMEWORK}\${RUNTIME}\publish\
            7z a -tzip $BUILD_FOLDER\SabreTools_${FRAMEWORK}_${RUNTIME}_release.zip *
        }
    }

    # Reset the directory
    Set-Location -Path $PSScriptRoot
}
