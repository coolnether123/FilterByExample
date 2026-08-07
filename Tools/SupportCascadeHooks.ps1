param([Parameter(Mandatory = $true)][string]$Phase, [Parameter(Mandatory = $true)][string]$Version)
$ErrorActionPreference = 'Stop'; Set-StrictMode -Version Latest
$repository = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot)); $leaf = [System.IO.Path]::GetFileName($repository)
$profiles = @{
  FilterByExample = @{ Name='Filter by Example'; Package='CoolNether123.FilterByExample'; Description='Apply filter changes by selecting exact example items.' }
  FilterSignals = @{ Name='Filter Signals'; Package='CoolNether123.FilterSignals'; Description='Show production and research status beside filter rows.' }
  PrisonerInteractionTimer = @{ Name='Prisoner Interaction Timer'; Package='CoolNether123.PrisonerInteractionTimer'; Description='Show prisoner interaction cooldowns and blockers.' }
  SOS2WeaponReadouts = @{ Name='SOS2 Weapon Readouts'; Package='CoolNether123.SOS2WeaponReadouts'; Description='Show Save Our Ship 2 weapon heat and power readouts.' }
  MechMuster = @{ Name='Mech Muster'; Package='CoolNether123.MechMuster'; Description='Assign newly available mechs to mechanitors with deficits.' }
}
$profile = $profiles[$leaf]; if ($null -eq $profile) { throw "No support metadata profile for $leaf." }
$dependencyXml = '<li><packageId>brrainz.harmony</packageId><displayName>Harmony</displayName></li><li><packageId>CoolNether123.Spine</packageId><displayName>SpineLib</displayName></li>'
$loadAfterXml = '<li>brrainz.harmony</li><li>CoolNether123.Spine</li>'
if ($leaf -eq 'MechMuster') { $dependencyXml += '<li><packageId>Ludeon.RimWorld.Biotech</packageId><displayName>Biotech</displayName></li>'; $loadAfterXml += '<li>Ludeon.RimWorld.Biotech</li>' }
if ($leaf -eq 'SOS2WeaponReadouts') { $dependencyXml += '<li><packageId>kentington.saveourship2</packageId><displayName>Save Our Ship 2</displayName></li>'; $loadAfterXml += '<li>kentington.saveourship2</li>' }
$aboutXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData><name>$($profile.Name)</name><author>CoolNether123</author><packageId>$($profile.Package)</packageId><modVersion>1.0.0</modVersion><supportedVersions><li>$Version</li></supportedVersions><modDependencies>$dependencyXml</modDependencies><loadAfter>$loadAfterXml</loadAfter><description>$($profile.Description) RimWorld $Version support build.</description></ModMetaData>
"@
$loadFoldersXml = "<?xml version=`"1.0`" encoding=`"utf-8`"?><loadFolders><v$Version><li>/</li></v$Version></loadFolders>"
if ($Phase -eq 'after-merge') {
  [System.IO.File]::WriteAllText((Join-Path $repository 'About\About.xml'), $aboutXml); [System.IO.File]::WriteAllText((Join-Path $repository 'LoadFolders.xml'), $loadFoldersXml)
  & git -C $repository add -- About/About.xml LoadFolders.xml; if ($LASTEXITCODE -ne 0) { throw 'Could not stage support metadata.' }
}
elseif ($Phase -eq 'before-stage') {
  $assembly = [string](Get-Content -Raw -LiteralPath (Join-Path $repository 'Tools\CascadeManifest.json') | ConvertFrom-Json).build.expectedAssembly; $source = Join-Path $repository "$Version\Assemblies\$assembly"; $root = Join-Path $repository 'Assemblies'; [System.IO.Directory]::CreateDirectory($root) | Out-Null; [System.IO.File]::Copy($source, (Join-Path $root $assembly), $true)
  & git -C $repository add -- Assemblies About/About.xml LoadFolders.xml; if ($LASTEXITCODE -ne 0) { throw 'Could not stage support payload.' }
}
