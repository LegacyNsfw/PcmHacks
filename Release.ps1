param
(
	[Parameter(Mandatory=$true)] [String] $ReleaseNumber,
	[Switch] $BranchAlreadyExists,
	[Switch] $Preview
)

if (($ReleaseNumber.Length -ne 3) -and -not $Preview)
{
	write-host You must provide a 3-digit release number.
	exit
}

Remove-Item -Recurse Apps\PcmHammer\bin\debug -ErrorAction SilentlyContinue
Remove-Item -Recurse Apps\PcmLogger\bin\debug -ErrorAction SilentlyContinue
Remove-Item -Recurse Release -ErrorAction SilentlyContinue;
Remove-Item -Recurse "PcmHammer$ReleaseNumber" -ErrorAction SilentlyContinue

if (-not $Preview)
{
	if (-not $BranchAlreadyExists)
	{
		git checkout develop
		git checkout -b "Release/$ReleaseNumber"
	}

	write-host ===============================================================================
	write-host = Updating version in help.html
	$file = "Apps\PcmHammer\help.html"
	$find = "    <h1>PCM Hammer Development Release</h1>"
	$replace = "    <h1>PCM Hammer Release $ReleaseNumber</h1>"
	(Get-Content $file).Replace($find, $replace) | Set-Content $file
	git add $file

	write-host ===============================================================================
	write-host = Updating version in PcmHammer MainForm.cs
	$file = "Apps\PcmHammer\MainForm.cs"
	$find = "        private const string AppVersion = null;"
	$replace = '        private const string AppVersion = "' + $ReleaseNumber + '";'
	(Get-Content $file).Replace($find, $replace) | Set-Content $file
	git add $file

	write-host ===============================================================================
	write-host = Running difftool - confirm changes to MainForm.cs and help.html now.
	git difftool --cached
}
else
{
	$ReleaseNumber = "$ReleaseNumber-Preview"
}

# Should call "dotnet build" here
write-host "Rebuild all in Visual Studio now."
read-host -Prompt "Press Enter to continue..."

write-host ===============================================================================
write-host = Rebuilding kernel
cd Kernels
.\build.bat
cd ..

write-host ===============================================================================
write-host = Copying files to release directory

$unused = mkdir Release

copy Kernels\*.bin Release

copy Apps\PcmHammer\bin\Debug\PcmHammer.* Release
copy Apps\PcmHammer\bin\Debug\*.dll Release
copy Apps\PcmHammer\bin\Debug\*.pdb Release

copy Apps\PcmLogger\bin\Debug\PcmLogger.* Release
copy Apps\PcmLogger\bin\Debug\*.dll Release
copy Apps\PcmLogger\bin\Debug\*.pdb Release
copy Apps\PcmLogger\*.LogProfile Release
copy Apps\PcmLogger\Parameters.*.xml Release

copy Apps\VpwExplorer\bin\Debug\VpwExplorer.* Release
copy Apps\VpwExplorer\bin\Debug\*.dll Release
copy Apps\VpwExplorer\bin\Debug\*.pdb Release


# The order of these two operations matters - it ensures that the zip file contains a directory named PcmHammerNNN.
Rename-Item Release "PcmHammer$ReleaseNumber"
7z.exe a -r "PcmHammer$ReleaseNumber.zip" "PcmHammer$ReleaseNumber\*.*"

if (-not $Preview)
{
	write-host ========================== WARNING ===============================
	write-host You still need to commit the changes to MainForm.cs and help.html.
	write-host ========================== WARNING ===============================
}

git status