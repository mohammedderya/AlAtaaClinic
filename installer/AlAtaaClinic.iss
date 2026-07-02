#define MyAppName "AlAtaa Clinic"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "AlAtaa"
#define MyAppExeName "AlAtaaClinic.Desktop.exe"
#define MyAppId "{{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=AlAtaaClinic-Setup-{#MyAppVersion}
SetupIconFile=..\src\AlAtaaClinic.Desktop\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Management System
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "installsqlexpress"; Description: "Install SQL Server Express (required if not already installed)"; GroupDescription: "Database:"; Flags: checkedonce

[Files]
Source: "..\publish\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\AlAtaaClinic.Desktop\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "..\docs\INSTALLATION.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "..\docs\DEPLOYMENT.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "redist\SQL2022-SSEI-Expr.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\SQL2022-SSEI-Expr.exe"; Parameters: "/ACTION=INSTALL /FEATURES=SQLENGINE /INSTANCENAME=MSSQLSERVER /SQLSVCAUTOSTART=Yes /SQLSVCSTARTUPTYPE=Automatic /ADDLOCAL=ALL /IAcceptSQLServerLicenseTerms /QUIET"; Description: "Install SQL Server Express"; Flags: waituntilterminated skipifnotsilent skipifsilent
Filename: "{tmp}\SQL2022-SSEI-Expr.exe"; Parameters: "/ACTION=INSTALL /FEATURES=SQLENGINE /INSTANCENAME=MSSQLSERVER /SQLSVCAUTOSTART=Yes /SQLSVCSTARTUPTYPE=Automatic /ADDLOCAL=ALL /IAcceptSQLServerLicenseTerms /QUIET"; Description: "Install SQL Server Express (this may take a few minutes)"; Flags: waituntilterminated skipifsilent
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsSqlInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL');
  if not Result then
    Result := RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\Microsoft SQL Server\Instance Names\SQL');
end;

function InitializeSetup: Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    ForceDirectories(ExpandConstant('{app}\logs'));
  end;
  
  if CurStep = ssInstall then
  begin
    if not IsSqlInstalled then
    begin
      if IsTaskSelected('installsqlexpress') then
      begin
        MsgBox(
          'SQL Server Express will now be installed.' + #13#10 + #13#10 +
          'This may take several minutes. Please wait...' + #13#10 + #13#10 +
          'The installation will continue automatically when SQL Server is ready.',
          mbInformation, MB_OK);
      end
      else
      begin
        MsgBox(
          'SQL Server was not detected on this computer.' + #13#10 + #13#10 +
          'AlAtaa Clinic requires a local SQL Server database.' + #13#10 + #13#10 +
          'You can either:' + #13#10 +
          '1. Re-run this installer and select "Install SQL Server Express"' + #13#10 +
          '2. Install SQL Server manually from Microsoft' + #13#10 + #13#10 +
          'Setup will continue, but the application cannot run until SQL Server is available.',
          mbInformation, MB_OK);
      end;
    end;
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nThe application requires SQL Server Express (will be installed automatically if not present).%n%nThe application works fully offline after installation.
