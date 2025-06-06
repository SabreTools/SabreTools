#!/bin/bash

# This batch file assumes the following:
# - .NET 9.0 (or newer) SDK is installed and in PATH
# - zip is installed and in PATH
# - Git is installed and in PATH
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Optional parameters
USE_ALL=false
INCLUDE_DEBUG=false
NO_BUILD=false
NO_ARCHIVE=false
while getopts "udba" OPTION; do
    case $OPTION in
    u)
        USE_ALL=true
        ;;
    d)
        INCLUDE_DEBUG=true
        ;;
    b)
        NO_BUILD=true
        ;;
    a)
        NO_ARCHIVE=true
        ;;
    *)
        echo "Invalid option provided"
        exit 1
        ;;
    esac
done

# Set the current directory as a variable
BUILD_FOLDER=$PWD

# Set the current commit hash
COMMIT=$(git log --pretty=%H -1)

# Output the selected options
echo "Selected Options:"
echo "  Use all frameworks (-u)               $USE_ALL"
echo "  Include debug builds (-d)             $INCLUDE_DEBUG"
echo "  No build (-b)                         $NO_BUILD"
echo "  No archive (-a)                       $NO_ARCHIVE"
echo " "

# Create the build matrix arrays
FRAMEWORKS=("net9.0")
RUNTIMES=("win-x86" "win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")

# Use expanded lists, if requested
if [ $USE_ALL = true ]; then
    FRAMEWORKS=("net20" "net35" "net40" "net452" "net462" "net472" "net48" "netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0" "net9.0")
fi

# Create the filter arrays
SINGLE_FILE_CAPABLE=("net5.0" "net6.0" "net7.0" "net8.0" "net9.0")
VALID_APPLE_FRAMEWORKS=("net6.0" "net7.0" "net8.0" "net9.0")
VALID_CROSS_PLATFORM_FRAMEWORKS=("netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0" "net9.0")
VALID_CROSS_PLATFORM_RUNTIMES=("win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")

# Only build if requested
if [ $NO_BUILD = false ]; then
    # Restore Nuget packages for all builds
    echo "Restoring Nuget packages"
    dotnet restore

    # Create Nuget Packages
    dotnet pack SabreTools.Core/SabreTools.Core.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatFiles/SabreTools.DatFiles.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatItems/SabreTools.DatItems.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.DatTools/SabreTools.DatTools.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.FileTypes/SabreTools.FileTypes.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.Help/SabreTools.Help.csproj --output $BUILD_FOLDER
    dotnet pack SabreTools.Reports/SabreTools.Reports.csproj --output $BUILD_FOLDER

    # Build SabreTools
    for FRAMEWORK in "${FRAMEWORKS[@]}"; do
        for RUNTIME in "${RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Build SabreTools - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]; then
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish SabreTools/SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                fi
                dotnet publish SabreTools/SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish SabreTools/SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                fi
                dotnet publish SabreTools/SabreTools.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done
fi

# Only create archives if requested
if [ $NO_ARCHIVE = false ]; then
    # Create SabreTools archives
    for FRAMEWORK in "${FRAMEWORKS[@]}"; do
        for RUNTIME in "${RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Archive SabreTools - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only include Debug if set
            if [ $INCLUDE_DEBUG = true ]; then
                cd $BUILD_FOLDER/SabreTools/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                zip -r $BUILD_FOLDER/SabreTools_${FRAMEWORK}_${RUNTIME}_debug.zip .
            fi
            cd $BUILD_FOLDER/SabreTools/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            zip -r $BUILD_FOLDER/SabreTools_${FRAMEWORK}_${RUNTIME}_release.zip .
        done
    done

    # Reset the directory
    cd $BUILD_FOLDER
fi
