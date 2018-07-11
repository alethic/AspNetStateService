param(
	[string]$PackagePath,
	[string]$Version
)

$m = [xml](gc "$($PackagePath)\ApplicationManifest.xml")
$m.ApplicationManifest.ApplicationTypeVersion = $Version

foreach ($s in $m.ApplicationManifest.ServiceManifestImport) {
	$n = $s.ServiceManifestRef.ServiceManifestName

	Write-Debug "Updating service manifest ref version for $n to $Version."
	$s.ServiceManifestRef.ServiceManifestVersion = $Version
	
	Write-Debug "Updating service manifest version for $n to $Version."
	$r = [xml](gc "$($PackagePath)\$n\ServiceManifest.xml")
	$r.ServiceManifest.Version = $Version
	$r.ServiceManifest.CodePackage | %{ $_.Version = $Version }
	$r.ServiceManifest.ConfigPackage | %{ $_.Version = $Version }
	$r.Save("$($PackagePath)\$n\ServiceManifest.xml")
}

$m.Save("$($PackagePath)\ApplicationManifest.xml")
